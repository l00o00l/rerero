using System;
using System.Collections.Generic;
using Thkim.DreamLaundromat.DynamicLab;

namespace Thkim.DreamLaundromat.Gameplay.ReleaseSlice
{
    public sealed class ReleaseLevelDefinition
    {
        private readonly Func<DynamicRoundDefinition> roundFactory;

        public ReleaseLevelDefinition(
            string levelId,
            string displayName,
            string guidance,
            string designIntent,
            string playerQuestion,
            string riskNote,
            string sourceId,
            int seed,
            Func<DynamicRoundDefinition> roundFactory,
            int phase = 1,
            ReleaseDifficultyBand difficultyBand = ReleaseDifficultyBand.Easy,
            IEnumerable<string> tutorialTags = null,
            ReleaseGuidedActionRule[] guidedActionRules = null,
            string manualGateNote = "")
        {
            LevelId = RequireText(levelId, nameof(levelId));
            DisplayName = RequireText(displayName, nameof(displayName));
            Guidance = guidance ?? string.Empty;
            DesignIntent = designIntent ?? string.Empty;
            PlayerQuestion = playerQuestion ?? string.Empty;
            RiskNote = riskNote ?? string.Empty;
            SourceId = sourceId ?? string.Empty;
            Seed = seed;
            Phase = Math.Max(1, phase);
            DifficultyBand = difficultyBand;
            TutorialTags = CreateTagArray(tutorialTags);
            GuidedActionRules = guidedActionRules ?? Array.Empty<ReleaseGuidedActionRule>();
            ManualGateNote = manualGateNote ?? string.Empty;
            this.roundFactory = roundFactory ?? throw new ArgumentNullException(nameof(roundFactory));
        }

        public string LevelId { get; }
        public string DisplayName { get; }
        public string Guidance { get; }
        public string DesignIntent { get; }
        public string PlayerQuestion { get; }
        public string RiskNote { get; }
        public string SourceId { get; }
        public int Seed { get; }
        public int Phase { get; }
        public ReleaseDifficultyBand DifficultyBand { get; }
        public string[] TutorialTags { get; }
        public ReleaseGuidedActionRule[] GuidedActionRules { get; }
        public string ManualGateNote { get; }

        public DynamicRoundDefinition CreateRoundDefinition()
        {
            DynamicRoundDefinition round = roundFactory();
            if (round == null)
            {
                throw new InvalidOperationException($"Release level {LevelId} produced no round definition.");
            }

            // Release level ids are the stable player-facing ids even when the
            // underlying round came from a generated candidate.
            round.RoundId = LevelId;
            round.TutorialTags = TutorialTags;
            round.DifficultyTarget = (int)DifficultyBand;
            return round;
        }

        public bool HasTutorialTag(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                return false;
            }

            for (int i = 0; i < TutorialTags.Length; i++)
            {
                if (string.Equals(TutorialTags[i], tag, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static string RequireText(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Value must not be empty.", parameterName);
            }

            return value;
        }

        private static string[] CreateTagArray(IEnumerable<string> tags)
        {
            if (tags == null)
            {
                return Array.Empty<string>();
            }

            var values = new List<string>();
            foreach (string tag in tags)
            {
                if (!string.IsNullOrWhiteSpace(tag))
                {
                    values.Add(tag.Trim());
                }
            }

            return values.ToArray();
        }
    }
}
