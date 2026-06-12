using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Thkim.PocketDodger.Input
{
    public sealed class TouchLaneInput : IPlayerInput
    {
        private readonly float _swipeThresholdPixels;
        private Vector2 _startPosition;
        private bool _isTracking;

        public TouchLaneInput(float swipeThresholdPixels)
        {
            _swipeThresholdPixels = swipeThresholdPixels;
        }

        public bool TryReadMove(out MoveCommand command)
        {
            command = MoveCommand.Left;

            if (TryReadTouch(out command))
            {
                return true;
            }

            return TryReadMouse(out command);
        }

        private bool TryReadTouch(out MoveCommand command)
        {
            command = MoveCommand.Left;
            Touchscreen touchscreen = Touchscreen.current;

            if (touchscreen == null)
            {
                return false;
            }

            TouchControl touch = touchscreen.primaryTouch;

            if (touch.press.wasPressedThisFrame)
            {
                _startPosition = touch.position.ReadValue();
                _isTracking = !IsPointerOverUi(touch.touchId.ReadValue());
                return false;
            }

            if (!_isTracking || !touch.press.wasReleasedThisFrame)
            {
                return false;
            }

            _isTracking = false;
            Vector2 endPosition = touch.position.ReadValue();
            command = ResolveCommand(_startPosition, endPosition);
            return true;
        }

        private bool TryReadMouse(out MoveCommand command)
        {
            command = MoveCommand.Left;
            Mouse mouse = Mouse.current;

            if (mouse == null)
            {
                return false;
            }

            if (mouse.leftButton.wasPressedThisFrame)
            {
                _startPosition = mouse.position.ReadValue();
                _isTracking = !IsPointerOverUi();
                return false;
            }

            if (!_isTracking || !mouse.leftButton.wasReleasedThisFrame)
            {
                return false;
            }

            _isTracking = false;
            Vector2 endPosition = mouse.position.ReadValue();
            command = ResolveCommand(_startPosition, endPosition);
            return true;
        }

        private MoveCommand ResolveCommand(Vector2 startPosition, Vector2 endPosition)
        {
            float deltaX = endPosition.x - startPosition.x;

            if (Mathf.Abs(deltaX) >= _swipeThresholdPixels)
            {
                return deltaX < 0.0f ? MoveCommand.Left : MoveCommand.Right;
            }

            return endPosition.x < Screen.width * 0.5f ? MoveCommand.Left : MoveCommand.Right;
        }

        private bool IsPointerOverUi(int pointerId)
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(pointerId);
        }

        private bool IsPointerOverUi()
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
        }
    }
}
