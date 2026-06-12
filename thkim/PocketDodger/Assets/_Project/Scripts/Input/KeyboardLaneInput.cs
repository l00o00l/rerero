using UnityEngine.InputSystem;

namespace Thkim.PocketDodger.Input
{
    public sealed class KeyboardLaneInput : IPlayerInput
    {
        public bool TryReadMove(out MoveCommand command)
        {
            command = MoveCommand.Left;
            Keyboard keyboard = Keyboard.current;

            if (keyboard == null)
            {
                return false;
            }

            if (keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame)
            {
                command = MoveCommand.Left;
                return true;
            }

            if (keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame)
            {
                command = MoveCommand.Right;
                return true;
            }

            return false;
        }
    }
}
