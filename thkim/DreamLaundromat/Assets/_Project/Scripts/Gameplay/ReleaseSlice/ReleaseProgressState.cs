using System.Collections.Generic;

namespace Thkim.DreamLaundromat.Gameplay.ReleaseSlice
{
    public sealed class ReleaseProgressState
    {
        public int HighestUnlockedLevelIndex;
        public readonly List<string> CompletedLevelIds = new List<string>();
        public ReleaseSettingsState Settings = new ReleaseSettingsState();

        public bool IsLevelUnlocked(int levelIndex)
        {
            return levelIndex >= 0 && levelIndex <= HighestUnlockedLevelIndex;
        }

        public bool IsLevelCompleted(string levelId)
        {
            return !string.IsNullOrWhiteSpace(levelId) && CompletedLevelIds.Contains(levelId);
        }

        public bool MarkCompleted(string levelId, int levelIndex, int totalLevelCount)
        {
            bool changed = false;
            if (!string.IsNullOrWhiteSpace(levelId) && !CompletedLevelIds.Contains(levelId))
            {
                CompletedLevelIds.Add(levelId);
                changed = true;
            }

            int nextUnlocked = System.Math.Min(levelIndex + 1, System.Math.Max(0, totalLevelCount - 1));
            if (HighestUnlockedLevelIndex < nextUnlocked)
            {
                HighestUnlockedLevelIndex = nextUnlocked;
                changed = true;
            }

            return changed;
        }

        public ReleaseProgressState Clone()
        {
            var clone = new ReleaseProgressState
            {
                HighestUnlockedLevelIndex = HighestUnlockedLevelIndex,
                Settings = Settings?.Clone() ?? new ReleaseSettingsState()
            };
            clone.CompletedLevelIds.AddRange(CompletedLevelIds);
            return clone;
        }
    }
}
