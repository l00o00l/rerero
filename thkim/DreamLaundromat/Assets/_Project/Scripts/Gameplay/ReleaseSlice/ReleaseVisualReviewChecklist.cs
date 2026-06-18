using System.Collections.Generic;
using Thkim.DreamLaundromat.DynamicLab;

namespace Thkim.DreamLaundromat.Gameplay.ReleaseSlice
{
    public static class ReleaseVisualReviewChecklist
    {
        public static string BuildSummary(ReleaseLevelValidationEntry entry)
        {
            if (entry?.Level == null)
            {
                return "missing level data";
            }

            List<string> checks = BuildChecks(entry);
            return checks.Count == 0 ? "core readability" : string.Join("; ", checks);
        }

        private static List<string> BuildChecks(ReleaseLevelValidationEntry entry)
        {
            var checks = new List<string>
            {
                "dream/order roles",
                "state badges",
                "action preview"
            };

            ReleaseLevelDefinition level = entry.Level;
            DynamicRoundDefinition round = entry.Round;
            if (level.GuidedActionRules.Length > 0 || level.HasTutorialTag("onboarding"))
            {
                checks.Add("guided prompt");
            }

            if (level.HasTutorialTag("preview"))
            {
                checks.Add("preview relevance");
            }

            if (level.HasTutorialTag("storage"))
            {
                checks.Add("storage pressure");
            }

            if (round != null)
            {
                bool hasItem = false;
                bool hasObstacle = false;
                for (int i = 0; i < round.Modifiers.Length; i++)
                {
                    hasItem |= round.Modifiers[i].Type == DynamicModifierType.Item;
                    hasObstacle |= round.Modifiers[i].Type == DynamicModifierType.Obstacle;
                }

                if (hasItem)
                {
                    checks.Add("item affordance");
                }

                if (hasObstacle)
                {
                    checks.Add("obstacle reason");
                }
            }

            if (!string.IsNullOrWhiteSpace(level.ManualGateNote))
            {
                checks.Add("manual gate note");
            }

            return checks;
        }
    }
}
