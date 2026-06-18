using System.Collections;
using Thkim.DreamLaundromat.DynamicLab;
using UnityEngine;
using UnityEngine.UI;

namespace Thkim.DreamLaundromat.Gameplay.ReleaseSlice
{
    public enum ReleaseUiFeedbackKind
    {
        ActionSuccess,
        ActionFailure,
        InvalidTarget,
        LevelClear
    }

    public readonly struct ReleaseUiFeedbackProfile
    {
        public ReleaseUiFeedbackProfile(
            string name,
            Color pulseColor,
            float durationSeconds,
            float shakePixels)
        {
            Name = name ?? string.Empty;
            PulseColor = pulseColor;
            DurationSeconds = durationSeconds;
            ShakePixels = shakePixels;
        }

        public string Name { get; }
        public Color PulseColor { get; }
        public float DurationSeconds { get; }
        public float ShakePixels { get; }
    }

    public sealed class ReleaseFeedbackPresenter : MonoBehaviour
    {
        private Text messageText;
        private Coroutine feedbackRoutine;
        private Vector2 baseMessagePosition;

        public static ReleaseUiFeedbackProfile ProfileFor(ReleaseUiFeedbackKind kind)
        {
            return kind switch
            {
                ReleaseUiFeedbackKind.ActionSuccess => new ReleaseUiFeedbackProfile(
                    "ActionSuccess",
                    ReleaseVisualStyle.Positive,
                    0.24f,
                    0f),
                ReleaseUiFeedbackKind.ActionFailure => new ReleaseUiFeedbackProfile(
                    "ActionFailure",
                    ReleaseVisualStyle.Obstacle,
                    0.28f,
                    7f),
                ReleaseUiFeedbackKind.InvalidTarget => new ReleaseUiFeedbackProfile(
                    "InvalidTarget",
                    ReleaseVisualStyle.Obstacle,
                    0.22f,
                    9f),
                ReleaseUiFeedbackKind.LevelClear => new ReleaseUiFeedbackProfile(
                    "LevelClear",
                    ReleaseVisualStyle.Selected,
                    0.34f,
                    0f),
                _ => new ReleaseUiFeedbackProfile(
                    kind.ToString(),
                    ReleaseVisualStyle.Text,
                    0.2f,
                    0f)
            };
        }

        public void Configure(Text targetMessageText)
        {
            messageText = targetMessageText;
            if (messageText != null)
            {
                baseMessagePosition = messageText.rectTransform.anchoredPosition;
            }
        }

        public void PresentActionResult(DynamicActionResult result, ReleaseSettingsState settings)
        {
            Present(
                result.Success ? ReleaseUiFeedbackKind.ActionSuccess : ReleaseUiFeedbackKind.ActionFailure,
                settings);
        }

        public void PresentInvalidTarget(ReleaseSettingsState settings)
        {
            Present(ReleaseUiFeedbackKind.InvalidTarget, settings);
        }

        public void Present(ReleaseUiFeedbackKind kind, ReleaseSettingsState settings)
        {
            if (messageText == null)
            {
                return;
            }

            if (feedbackRoutine != null)
            {
                StopCoroutine(feedbackRoutine);
            }

            messageText.color = ReleaseVisualStyle.Text;
            messageText.rectTransform.anchoredPosition = baseMessagePosition;
            if (settings != null && settings.ReducedMotion)
            {
                feedbackRoutine = null;
                return;
            }

            feedbackRoutine = StartCoroutine(PresentRoutine(ProfileFor(kind)));
        }

        private IEnumerator PresentRoutine(ReleaseUiFeedbackProfile profile)
        {
            float elapsed = 0f;
            RectTransform rect = messageText.rectTransform;
            while (elapsed < profile.DurationSeconds && messageText != null)
            {
                elapsed += Time.unscaledDeltaTime;
                float normalized = Mathf.Clamp01(elapsed / profile.DurationSeconds);
                float intensity = Mathf.Sin(normalized * Mathf.PI);
                messageText.color = Color.Lerp(ReleaseVisualStyle.Text, profile.PulseColor, intensity * 0.85f);

                if (profile.ShakePixels > 0f)
                {
                    float shake = Mathf.Sin(normalized * Mathf.PI * 6f)
                        * profile.ShakePixels
                        * (1f - normalized);
                    rect.anchoredPosition = baseMessagePosition + new Vector2(shake, 0f);
                }

                yield return null;
            }

            if (messageText != null)
            {
                messageText.color = ReleaseVisualStyle.Text;
                messageText.rectTransform.anchoredPosition = baseMessagePosition;
            }

            feedbackRoutine = null;
        }
    }
}
