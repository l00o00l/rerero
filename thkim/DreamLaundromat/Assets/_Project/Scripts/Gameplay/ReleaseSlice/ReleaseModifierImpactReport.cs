using System.Collections.Generic;
using Thkim.DreamLaundromat.DynamicLab;

namespace Thkim.DreamLaundromat.Gameplay.ReleaseSlice
{
    public sealed class ReleaseModifierImpactReport
    {
        public readonly List<ReleaseModifierImpactEntry> Entries = new List<ReleaseModifierImpactEntry>();
        public readonly List<string> Warnings = new List<string>();
        public readonly HashSet<DynamicModifierEffect> Effects = new HashSet<DynamicModifierEffect>();
        public int ItemLevelCount;
        public int ObstacleLevelCount;

        public int WarningLevelCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < Entries.Count; i++)
                {
                    if (Entries[i].HasWarning)
                    {
                        count++;
                    }
                }

                return count;
            }
        }

        public void AddEntry(ReleaseModifierImpactEntry entry)
        {
            if (entry == null)
            {
                return;
            }

            Entries.Add(entry);
            if (entry.ItemCount > 0)
            {
                ItemLevelCount++;
            }

            if (entry.ObstacleCount > 0)
            {
                ObstacleLevelCount++;
            }

            for (int i = 0; i < entry.Effects.Length; i++)
            {
                Effects.Add(entry.Effects[i]);
            }

            for (int i = 0; i < entry.Warnings.Count; i++)
            {
                Warnings.Add($"{entry.LevelId}: {entry.Warnings[i]}");
            }
        }
    }
}
