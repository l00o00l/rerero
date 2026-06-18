using System.Collections.Generic;

namespace Thkim.DreamLaundromat.Gameplay.ReleaseSlice
{
    public sealed class ReleaseAccessibilityAuditResult
    {
        public readonly List<string> Errors = new List<string>();

        public bool IsValid => Errors.Count == 0;

        public void AddError(string message)
        {
            if (!string.IsNullOrWhiteSpace(message))
            {
                Errors.Add(message);
            }
        }
    }

    public static class ReleaseAccessibilityAudit
    {
        public static ReleaseAccessibilityAuditResult AuditDefaultStyle(ReleaseLevelPack pack)
        {
            var result = new ReleaseAccessibilityAuditResult();
            if (ReleaseVisualStyle.MinTouchTargetHeight < 56f)
            {
                result.AddError("Touch target height is below the mobile minimum.");
            }

            float bodyContrast = ReleaseVisualStyle.ContrastRatio(
                ReleaseVisualStyle.Text,
                ReleaseVisualStyle.Background);
            if (bodyContrast < ReleaseVisualStyle.MinimumBodyContrastRatio)
            {
                result.AddError("Body text contrast is below the configured minimum.");
            }

            if (pack == null || pack.Levels.Count == 0)
            {
                result.AddError("Level pack is missing.");
                return result;
            }

            for (int i = 0; i < pack.Levels.Count; i++)
            {
                ReleaseLevelDefinition level = pack.Levels[i];
                for (int ruleIndex = 0; ruleIndex < level.GuidedActionRules.Length; ruleIndex++)
                {
                    if (string.IsNullOrWhiteSpace(level.GuidedActionRules[ruleIndex].Prompt))
                    {
                        result.AddError($"{level.LevelId} guided action {ruleIndex} has no prompt.");
                    }
                }
            }

            return result;
        }
    }
}
