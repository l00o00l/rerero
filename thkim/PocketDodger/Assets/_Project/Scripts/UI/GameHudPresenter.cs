using UnityEngine;
using UnityEngine.UI;

namespace Thkim.PocketDodger.UI
{
    public sealed class GameHudPresenter : MonoBehaviour
    {
        [SerializeField] private Text scoreText;
        [SerializeField] private Text highScoreText;
        [SerializeField] private float pulseScale = 1.08f;
        [SerializeField] private float pulseDuration = 0.12f;

        private int _displayedScore = int.MinValue;
        private int _displayedHighScore = int.MinValue;
        private RectTransform _scoreRectTransform;
        private RectTransform _highScoreRectTransform;
        private Vector3 _scoreBaseScale = Vector3.one;
        private Vector3 _highScoreBaseScale = Vector3.one;
        private float _scorePulseTimer;
        private float _highScorePulseTimer;

        private void Awake()
        {
            CacheTextReferences();
            ApplyTextStyle(scoreText);
            ApplyTextStyle(highScoreText);
        }

        private void Update()
        {
            UpdatePulse(_scoreRectTransform, _scoreBaseScale, ref _scorePulseTimer);
            UpdatePulse(_highScoreRectTransform, _highScoreBaseScale, ref _highScorePulseTimer);
        }

        public void Configure(Text score, Text highScore)
        {
            scoreText = score;
            highScoreText = highScore;
            _displayedScore = int.MinValue;
            _displayedHighScore = int.MinValue;
            CacheTextReferences();
            ApplyTextStyle(scoreText);
            ApplyTextStyle(highScoreText);
        }

        public void SetScore(int score)
        {
            if (scoreText != null && score != _displayedScore)
            {
                scoreText.text = $"Score {score}";
                _displayedScore = score;
                if (score == 0 || score % 10 == 0)
                {
                    StartPulse(ref _scorePulseTimer);
                }
            }
        }

        public void SetHighScore(int highScore)
        {
            if (highScoreText != null && highScore != _displayedHighScore)
            {
                highScoreText.text = $"Best {highScore}";
                _displayedHighScore = highScore;
                StartPulse(ref _highScorePulseTimer);
            }
        }

        private void CacheTextReferences()
        {
            _scoreRectTransform = scoreText == null ? null : scoreText.rectTransform;
            _highScoreRectTransform = highScoreText == null ? null : highScoreText.rectTransform;
            _scoreBaseScale = _scoreRectTransform == null ? Vector3.one : _scoreRectTransform.localScale;
            _highScoreBaseScale = _highScoreRectTransform == null ? Vector3.one : _highScoreRectTransform.localScale;
        }

        private void StartPulse(ref float timer)
        {
            timer = pulseDuration;
        }

        private void UpdatePulse(RectTransform rectTransform, Vector3 baseScale, ref float timer)
        {
            if (rectTransform == null || timer <= 0.0f)
            {
                return;
            }

            timer -= Time.unscaledDeltaTime;
            float t = pulseDuration <= 0.0f ? 1.0f : Mathf.Clamp01(1.0f - timer / pulseDuration);
            float scale = Mathf.Lerp(pulseScale, 1.0f, EaseOutCubic(t));
            rectTransform.localScale = baseScale * scale;

            if (timer <= 0.0f)
            {
                rectTransform.localScale = baseScale;
            }
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
    }
}
