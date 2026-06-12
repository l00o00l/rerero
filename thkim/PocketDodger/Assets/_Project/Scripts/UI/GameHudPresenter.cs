using UnityEngine;
using UnityEngine.UI;

namespace Thkim.PocketDodger.UI
{
    public sealed class GameHudPresenter : MonoBehaviour
    {
        [SerializeField] private Text scoreText;
        [SerializeField] private Text highScoreText;

        private int _displayedScore = int.MinValue;
        private int _displayedHighScore = int.MinValue;

        public void Configure(Text score, Text highScore)
        {
            scoreText = score;
            highScoreText = highScore;
            _displayedScore = int.MinValue;
            _displayedHighScore = int.MinValue;
        }

        public void SetScore(int score)
        {
            if (scoreText != null && score != _displayedScore)
            {
                scoreText.text = $"Score {score}";
                _displayedScore = score;
            }
        }

        public void SetHighScore(int highScore)
        {
            if (highScoreText != null && highScore != _displayedHighScore)
            {
                highScoreText.text = $"Best {highScore}";
                _displayedHighScore = highScore;
            }
        }
    }
}
