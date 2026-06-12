using UnityEngine;
using UnityEngine.UI;

namespace Thkim.PocketDodger.UI
{
    public sealed class GameHudPresenter : MonoBehaviour
    {
        [SerializeField] private Text scoreText;
        [SerializeField] private Text highScoreText;

        public void Configure(Text score, Text highScore)
        {
            scoreText = score;
            highScoreText = highScore;
        }

        public void SetScore(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = $"Score {score}";
            }
        }

        public void SetHighScore(int highScore)
        {
            if (highScoreText != null)
            {
                highScoreText.text = $"Best {highScore}";
            }
        }
    }
}
