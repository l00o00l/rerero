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

        private Action _restartRequested;

        private void Awake()
        {
            if (restartButton != null)
            {
                restartButton.onClick.AddListener(HandleRestartClicked);
            }
        }

        public void Configure(GameObject root, Text finalScore, Text highScore, Button restart)
        {
            panelRoot = root;
            finalScoreText = finalScore;
            highScoreText = highScore;
            restartButton = restart;
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
        }

        public void SetVisible(bool isVisible)
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(isVisible);
            }
        }

        private void HandleRestartClicked()
        {
            _restartRequested?.Invoke();
        }
    }
}
