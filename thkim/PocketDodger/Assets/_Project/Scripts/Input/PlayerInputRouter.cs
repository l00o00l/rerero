using System;
using UnityEngine;

namespace Thkim.PocketDodger.Input
{
    public sealed class PlayerInputRouter : MonoBehaviour
    {
        [SerializeField] private bool enableKeyboard = true;
        [SerializeField] private bool enableTouch = true;
        [SerializeField] private float swipeThresholdPixels = 60.0f;

        private KeyboardLaneInput _keyboardInput;
        private TouchLaneInput _touchInput;
        private Action<MoveCommand> _moveRequested;

        private void Awake()
        {
            _keyboardInput = new KeyboardLaneInput();
            _touchInput = new TouchLaneInput(swipeThresholdPixels);
        }

        private void Update()
        {
            if (enableKeyboard && _keyboardInput.TryReadMove(out MoveCommand keyboardCommand))
            {
                _moveRequested?.Invoke(keyboardCommand);
                return;
            }

            if (enableTouch && _touchInput.TryReadMove(out MoveCommand touchCommand))
            {
                _moveRequested?.Invoke(touchCommand);
            }
        }

        public void Initialize(Action<MoveCommand> onMoveRequested)
        {
            _moveRequested = onMoveRequested;
        }
    }
}
