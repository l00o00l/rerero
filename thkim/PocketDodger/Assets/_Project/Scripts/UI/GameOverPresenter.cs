using System;
using UnityEngine;
using UnityEngine.UI;

namespace Thkim.PocketDodger.UI
{
    public sealed class GameOverPresenter : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Text finalScoreText;
        [SerializeField] private Text highScoreText;
        [SerializeField] private Button restartButton;
        [SerializeField] private float showScale = 0.96f;
        [SerializeField] private float showAnimationDuration = 0.18f;

        private Action _restartRequested;
        private RectTransform _panelRectTransform;
        private Vector3 _panelBaseScale = Vector3.one;
        private float _showAnimationTimer;

        private void Awake()
        {
            CachePanelTransform();
            ApplyTextStyle(finalScoreText);
            ApplyTextStyle(highScoreText);

            if (restartButton != null)
            {
                restartButton.onClick.AddListener(HandleRestartClicked);
            }
        }

        private void Update()
        {
            if (_panelRectTransform == null || _showAnimationTimer <= 0.0f)
            {
                return;
            }

            _showAnimationTimer -= Time.unscaledDeltaTime;
            float t = showAnimationDuration <= 0.0f ? 1.0f : Mathf.Clamp01(1.0f - _showAnimationTimer / showAnimationDuration);
            _panelRectTransform.localScale = _panelBaseScale * Mathf.Lerp(showScale, 1.0f, EaseOutCubic(t));

            if (_showAnimationTimer <= 0.0f)
            {
                _panelRectTransform.localScale = _panelBaseScale;
            }
        }

        public void Configure(GameObject root, Text finalScore, Text highScore, Button restart)
        {
            panelRoot = root;
            finalScoreText = finalScore;
            highScoreText = highScore;
            restartButton = restart;
            CachePanelTransform();
            ApplyTextStyle(finalScoreText);
            ApplyTextStyle(highScoreText);
        }

        public void Initialize(Action onRestartRequested)
        {
            _restartRequested = onRestartRequested;
        }

        public void Show(int finalScore, int highScore)
        {
            if (finalScoreText != null)
            {
                finalScoreText.text = $"Score {finalScore}";
            }

            if (highScoreText != null)
            {
                highScoreText.text = $"Best {highScore}";
            }

            SetVisible(true);
            StartShowAnimation();
        }

        public void SetVisible(bool isVisible)
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(isVisible);
            }
        }

        private void CachePanelTransform()
        {
            _panelRectTransform = panelRoot == null ? null : panelRoot.GetComponent<RectTransform>();
            _panelBaseScale = _panelRectTransform == null ? Vector3.one : _panelRectTransform.localScale;
        }

        private void StartShowAnimation()
        {
            if (_panelRectTransform == null)
            {
                return;
            }

            _showAnimationTimer = showAnimationDuration;
            _panelRectTransform.localScale = _panelBaseScale * showScale;
        }

        private static void ApplyTextStyle(Text text)
        {
            if (text == null)
            {
                return;
            }

            text.fontStyle = FontStyle.Bold;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = Mathf.Min(text.resizeTextMinSize, text.fontSize);
            text.resizeTextMaxSize = Mathf.Max(text.resizeTextMaxSize, text.fontSize);
            text.alignByGeometry = true;
        }

        private static float EaseOutCubic(float value)
        {
            float inverse = 1.0f - value;
            return 1.0f - inverse * inverse * inverse;
        }

        private void HandleRestartClicked()
        {
            _restartRequested?.Invoke();
        }
    }
}
