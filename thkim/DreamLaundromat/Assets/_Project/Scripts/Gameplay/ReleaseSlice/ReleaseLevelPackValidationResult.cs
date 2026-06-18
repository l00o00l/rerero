using System.Collections.Generic;
using Thkim.DreamLaundromat.DynamicLab;

namespace Thkim.DreamLaundromat.Gameplay.ReleaseSlice
{
    public sealed class ReleaseLevelPackValidationResult
    {
        public readonly List<ReleaseLevelValidationEntry> Entries = new List<ReleaseLevelValidationEntry>();
        public readonly List<string> Errors = new List<string>();
        public readonly List<string> Warnings = new List<string>();
        public readonly List<string> DesignNotes = new List<string>();
        public readonly HashSet<ReleaseDifficultyBand> DifficultyBands = new HashSet<ReleaseDifficultyBand>();
        public readonly HashSet<string> TutorialTags = new HashSet<string>();
        public readonly HashSet<DynamicModifierEffect> ModifierEffects = new HashSet<DynamicModifierEffect>();
        public int GuidedLevelCount;
        public int ItemLevelCount;
        public int ObstacleLevelCount;

        public bool IsValid => Errors.Count == 0 && Entries.TrueForAll(entry => entry.IsValid);

        public void AddError(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                Errors.Add(message);
            }
        }

        public void AddWarning(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                Warnings.Add(message);
            }
        }

        public void AddDesignNote(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                DesignNotes.Add(message);
            }
        }
    }
}
