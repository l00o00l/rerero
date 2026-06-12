namespace Thkim.PocketDodger.Gameplay
{
    public sealed class ObstacleSpawnPattern
    {
        private static readonly LaneIndex[][] s_patterns =
        {
            new[] { LaneIndex.Left, LaneIndex.Center, LaneIndex.Right, LaneIndex.Center },
            new[] { LaneIndex.Right, LaneIndex.Center, LaneIndex.Left, LaneIndex.Center },
            new[] { LaneIndex.Left, LaneIndex.Right, LaneIndex.Left, LaneIndex.Center },
            new[] { LaneIndex.Right, LaneIndex.Left, LaneIndex.Right, LaneIndex.Center },
            new[] { LaneIndex.Center, LaneIndex.Left, LaneIndex.Center, LaneIndex.Right },
        };

        private int _patternIndex;
        private int _stepIndex;

        public void Reset()
        {
            _patternIndex = 0;
            _stepIndex = 0;
        }

        public LaneIndex NextLane()
        {
            LaneIndex[] pattern = s_patterns[_patternIndex];
            LaneIndex lane = pattern[_stepIndex];

            _stepIndex++;
            if (_stepIndex >= pattern.Length)
            {
                _stepIndex = 0;
                _patternIndex = (_patternIndex + 1) % s_patterns.Length;
            }

            return lane;
        }
    }
}
