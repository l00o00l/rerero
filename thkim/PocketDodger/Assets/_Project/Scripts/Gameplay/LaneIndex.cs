namespace Thkim.PocketDodger.Gameplay
{
    public enum LaneIndex
    {
        Left = 0,
        Center = 1,
        Right = 2
    }

    public static class LaneIndexExtensions
    {
        public const int LaneCount = 3;

        public static LaneIndex Clamp(int lane)
        {
            if (lane < 0)
            {
                return LaneIndex.Left;
            }

            if (lane >= LaneCount)
            {
                return LaneIndex.Right;
            }

            return (LaneIndex)lane;
        }

        public static LaneIndex Move(this LaneIndex lane, int direction)
        {
            return Clamp((int)lane + direction);
        }
    }
}
