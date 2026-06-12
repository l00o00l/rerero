namespace Thkim.PocketDodger.Input
{
    public interface IPlayerInput
    {
        bool TryReadMove(out MoveCommand command);
    }
}
