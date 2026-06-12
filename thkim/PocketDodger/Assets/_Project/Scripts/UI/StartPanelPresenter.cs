using System;
using UnityEngine;
using UnityEngine.UI;

namespace Thkim.PocketDodger.UI
{
    public sealed class StartPanelPresenter : MonoBehaviour
    {
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private Button startButton;

        private Action _startRequested;

        private void Awake()
        {
            if (startButton != null)
            {
                startButton.onClick.AddListener(HandleStartClicked);
            }
        }

        public void Configure(GameObject root, Button button)
        {
            panelRoot = root;
            startButton = button;
        }

        public void Initialize(Action onStartRequested)
        {
            _startRequested = onStartRequested;
        }

        public void SetVisible(bool isVisible)
        {
            if (panelRoot != null)
            {
                panelRoot.SetActive(isVisible);
            }
        }

        private void HandleStartClicked()
        {
            _startRequested?.Invoke();
        }
    }
}
