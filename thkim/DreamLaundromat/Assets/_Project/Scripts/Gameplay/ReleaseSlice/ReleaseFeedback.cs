using System.Collections.Generic;
using UnityEngine;

namespace Thkim.DreamLaundromat.Gameplay.ReleaseSlice
{
    public enum ReleaseFeedbackEventType
    {
        LevelStarted,
        ActionSucceeded,
        ActionFailed,
        LevelCleared,
        ProgressSaved
    }

    public readonly struct ReleaseFeedbackEvent
    {
        public ReleaseFeedbackEvent(ReleaseFeedbackEventType type, string levelId, string message)
        {
            Type = type;
            LevelId = levelId ?? string.Empty;
            Message = message ?? string.Empty;
        }

        public ReleaseFeedbackEventType Type { get; }
        public string LevelId { get; }
        public string Message { get; }
    }

    public interface IReleaseFeedbackSink
    {
        void Emit(ReleaseFeedbackEvent feedbackEvent, ReleaseSettingsState settings);
    }

    public readonly struct ReleaseFeedbackTimingProfile
    {
        public ReleaseFeedbackTimingProfile(
            string eventName,
            float visualPulseSeconds,
            float audioSeconds,
            bool usesHaptic)
        {
            EventName = eventName ?? string.Empty;
            VisualPulseSeconds = visualPulseSeconds;
            AudioSeconds = audioSeconds;
            UsesHaptic = usesHaptic;
        }

        public string EventName { get; }
        public float VisualPulseSeconds { get; }
        public float AudioSeconds { get; }
        public bool UsesHaptic { get; }
    }

    public static class ReleaseFeedbackTiming
    {
        public static ReleaseFeedbackTimingProfile ForEvent(ReleaseFeedbackEventType type)
        {
            return type switch
            {
                ReleaseFeedbackEventType.ActionSucceeded => new ReleaseFeedbackTimingProfile("ActionSucceeded", 0.08f, 0.05f, false),
                ReleaseFeedbackEventType.ActionFailed => new ReleaseFeedbackTimingProfile("ActionFailed", 0.1f, 0.08f, false),
                ReleaseFeedbackEventType.LevelCleared => new ReleaseFeedbackTimingProfile("LevelCleared", 0.18f, 0.12f, true),
                ReleaseFeedbackEventType.ProgressSaved => new ReleaseFeedbackTimingProfile("ProgressSaved", 0.04f, 0f, false),
                ReleaseFeedbackEventType.LevelStarted => new ReleaseFeedbackTimingProfile("LevelStarted", 0.06f, 0f, false),
                _ => new ReleaseFeedbackTimingProfile(type.ToString(), 0.05f, 0f, false)
            };
        }
    }

    public sealed class ReleaseFeedbackRecorder : IReleaseFeedbackSink
    {
        public readonly List<ReleaseFeedbackEvent> Events = new List<ReleaseFeedbackEvent>();

        public void Emit(ReleaseFeedbackEvent feedbackEvent, ReleaseSettingsState settings)
        {
            Events.Add(feedbackEvent);
        }
    }

    public sealed class ReleaseUnityFeedbackSink : IReleaseFeedbackSink
    {
        private static AudioClip actionClip;
        private static AudioClip failClip;
        private static AudioClip clearClip;
        private AudioSource audioSource;

        public void Emit(ReleaseFeedbackEvent feedbackEvent, ReleaseSettingsState settings)
        {
            if (settings == null)
            {
                return;
            }

            if (settings.SoundEnabled)
            {
                PlayGeneratedSound(feedbackEvent.Type);
            }

            ReleaseFeedbackTimingProfile timing = ReleaseFeedbackTiming.ForEvent(feedbackEvent.Type);
            if (!settings.HapticsEnabled
                || settings.ReducedMotion
                || !timing.UsesHaptic)
            {
                return;
            }

            // Unity exposes only a coarse platform haptic here; richer Android
            // haptics should stay behind this sink so tests remain deterministic.
            Handheld.Vibrate();
        }

        private void PlayGeneratedSound(ReleaseFeedbackEventType type)
        {
            AudioClip clip = type switch
            {
                ReleaseFeedbackEventType.ActionFailed => failClip ??= CreateTone("DreamLaundromatFail", 180f, ReleaseFeedbackTiming.ForEvent(type).AudioSeconds),
                ReleaseFeedbackEventType.LevelCleared => clearClip ??= CreateTone("DreamLaundromatClear", 660f, ReleaseFeedbackTiming.ForEvent(type).AudioSeconds),
                ReleaseFeedbackEventType.ActionSucceeded => actionClip ??= CreateTone("DreamLaundromatAction", 440f, ReleaseFeedbackTiming.ForEvent(type).AudioSeconds),
                _ => null
            };

            if (clip != null)
            {
                AudioSource source = GetAudioSource();
                source.PlayOneShot(clip, 0.28f);
            }
        }

        private AudioSource GetAudioSource()
        {
            if (audioSource != null)
            {
                return audioSource;
            }

            var audioObject = new GameObject("ReleaseFeedbackAudio");
            UnityEngine.Object.DontDestroyOnLoad(audioObject);
            audioSource = audioObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f;
            return audioSource;
        }

        private static AudioClip CreateTone(string name, float frequency, float durationSeconds)
        {
            const int sampleRate = 22050;
            int sampleCount = Mathf.Max(1, Mathf.RoundToInt(sampleRate * durationSeconds));
            var samples = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                float t = i / (float)sampleRate;
                float fade = 1f - (i / (float)sampleCount);
                samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * fade * 0.18f;
            }

            AudioClip clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
