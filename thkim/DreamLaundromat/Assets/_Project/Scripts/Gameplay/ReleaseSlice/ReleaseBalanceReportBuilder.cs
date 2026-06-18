using System.Globalization;
using System.Linq;
using System.Text;

namespace Thkim.DreamLaundromat.Gameplay.ReleaseSlice
{
    public static class ReleaseBalanceReportBuilder
    {
        public static string Build(ReleaseLevelPack pack)
        {
            return BuildResult(pack).Report;
        }

        public static ReleaseBalanceReportResult BuildResult(ReleaseLevelPack pack)
        {
            ReleaseLevelPackValidationResult validation = ReleaseLevelPackValidator.Validate(pack);
            ReleaseAccessibilityAuditResult accessibility = ReleaseAccessibilityAudit.AuditDefaultStyle(pack);
            ReleaseModifierImpactReport modifierImpact = ReleaseModifierImpactAudit.Audit(
                validation,
                ReleaseValidationDefaults.SolveOptions);
            string report = BuildReport(validation, accessibility, modifierImpact);
            return new ReleaseBalanceReportResult(validation, accessibility, modifierImpact, report);
        }

        private static string BuildReport(
            ReleaseLevelPackValidationResult validation,
            ReleaseAccessibilityAuditResult accessibility,
            ReleaseModifierImpactReport modifierImpact)
        {
            var builder = new StringBuilder();
            builder.AppendLine("DreamLaundromat QA Balance Report");
            builder.AppendLine($"Valid={validation.IsValid}");
            builder.AppendLine($"AccessibilityValid={accessibility.IsValid}");
            builder.AppendLine($"Levels={validation.Entries.Count}");
            builder.AppendLine($"Warnings={validation.Warnings.Count}");
            builder.AppendLine($"DesignNotes={validation.DesignNotes.Count}");
            builder.AppendLine($"GuidedLevels={validation.GuidedLevelCount}");
            builder.AppendLine($"ItemLevels={validation.ItemLevelCount}");
            builder.AppendLine($"ObstacleLevels={validation.ObstacleLevelCount}");
            builder.AppendLine($"DifficultyBands={string.Join(", ", validation.DifficultyBands.OrderBy(band => (int)band))}");
            builder.AppendLine($"ModifierEffects={string.Join(", ", validation.ModifierEffects.OrderBy(effect => effect.ToString()))}");
            builder.AppendLine($"ModifierImpactLevels={modifierImpact.Entries.Count}");
            builder.AppendLine($"ModifierImpactWarnings={modifierImpact.Warnings.Count}");
            builder.AppendLine($"ModifierImpactEffects={string.Join(", ", modifierImpact.Effects.OrderBy(effect => effect.ToString()))}");
            builder.AppendLine($"ManualGateLevels={CountManualGateLevels(validation)}");
            builder.AppendLine($"AverageDifficulty={CalculateAverageDifficulty(validation).ToString("0.00", CultureInfo.InvariantCulture)}");
            builder.AppendLine();
            builder.AppendLine("Modifier Impact");

            for (int i = 0; i < modifierImpact.Entries.Count; i++)
            {
                ReleaseModifierImpactEntry entry = modifierImpact.Entries[i];
                builder.AppendLine(
                    $"{entry.LevelId} effects={string.Join(",", entry.Effects)} items={entry.ItemCount} obstacles={entry.ObstacleCount} itemUsed={entry.FirstSolutionUsesItem} blocked={entry.OriginalObstacleBlockedActionCount} minMoves={entry.OriginalMinMoves} withoutModifiers={FormatWithoutModifierMinMoves(entry)} moveDelta={entry.MinMoveDelta} branchingDelta={entry.MaxBranchingDelta} warnings={FormatWarningCount(entry)}");
            }

            if (modifierImpact.Warnings.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine("Modifier Impact Warnings");
                for (int i = 0; i < modifierImpact.Warnings.Count; i++)
                {
                    builder.AppendLine($"- {modifierImpact.Warnings[i]}");
                }
            }

            builder.AppendLine();
            builder.AppendLine("Level Review Queue");

            for (int i = 0; i < validation.Entries.Count; i++)
            {
                ReleaseLevelValidationEntry entry = validation.Entries[i];
                if (entry.Level == null)
                {
                    continue;
                }

                string difficulty = entry.Metrics == null
                    ? "n/a"
                    : entry.Metrics.DifficultyScore.ToString("0.00", CultureInfo.InvariantCulture);
                builder.AppendLine(
                    $"{entry.Level.LevelId} band={entry.Level.DifficultyBand} minMoves={FormatMinMoves(entry)} score={difficulty} tags={string.Join(",", entry.Level.TutorialTags)}");
            }

            builder.AppendLine();
            builder.AppendLine("Visual UX Review Checklist");
            for (int i = 0; i < validation.Entries.Count; i++)
            {
                ReleaseLevelValidationEntry entry = validation.Entries[i];
                if (entry.Level == null)
                {
                    continue;
                }

                builder.AppendLine($"{entry.Level.LevelId}: {ReleaseVisualReviewChecklist.BuildSummary(entry)}");
            }

            builder.AppendLine();
            builder.AppendLine("Feedback Timing");
            AppendFeedbackTiming(builder, ReleaseFeedbackEventType.ActionSucceeded);
            AppendFeedbackTiming(builder, ReleaseFeedbackEventType.ActionFailed);
            AppendFeedbackTiming(builder, ReleaseFeedbackEventType.LevelCleared);

            if (validation.Errors.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine("Errors");
                for (int i = 0; i < validation.Errors.Count; i++)
                {
                    builder.AppendLine($"- {validation.Errors[i]}");
                }
            }

            if (accessibility.Errors.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine("Accessibility Errors");
                for (int i = 0; i < accessibility.Errors.Count; i++)
                {
                    builder.AppendLine($"- {accessibility.Errors[i]}");
                }
            }

            if (validation.Warnings.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine("Warnings");
                for (int i = 0; i < validation.Warnings.Count; i++)
                {
                    builder.AppendLine($"- {validation.Warnings[i]}");
                }
            }

            if (validation.DesignNotes.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine("Design Notes");
                for (int i = 0; i < validation.DesignNotes.Count; i++)
                {
                    builder.AppendLine($"- {validation.DesignNotes[i]}");
                }
            }

            return builder.ToString();
        }

        private static void AppendFeedbackTiming(StringBuilder builder, ReleaseFeedbackEventType type)
        {
            ReleaseFeedbackTimingProfile timing = ReleaseFeedbackTiming.ForEvent(type);
            builder.AppendLine(
                $"{timing.EventName} visual={timing.VisualPulseSeconds.ToString("0.00", CultureInfo.InvariantCulture)}s audio={timing.AudioSeconds.ToString("0.00", CultureInfo.InvariantCulture)}s haptic={timing.UsesHaptic}");
        }

        private static string FormatMinMoves(ReleaseLevelValidationEntry entry)
        {
            return entry.SolveResult == null
                ? "n/a"
                : entry.SolveResult.MinMoves.ToString(CultureInfo.InvariantCulture);
        }

        private static string FormatWithoutModifierMinMoves(ReleaseModifierImpactEntry entry)
        {
            if (entry.WithoutModifiersHitLimit)
            {
                return "limit";
            }

            return entry.WithoutModifiersSolvable
                ? entry.WithoutModifiersMinMoves.ToString(CultureInfo.InvariantCulture)
                : "unsolved";
        }

        private static string FormatWarningCount(ReleaseModifierImpactEntry entry)
        {
            return entry.Warnings.Count == 0
                ? "none"
                : entry.Warnings.Count.ToString(CultureInfo.InvariantCulture);
        }

        private static int CountManualGateLevels(ReleaseLevelPackValidationResult validation)
        {
            int count = 0;
            for (int i = 0; i < validation.Entries.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(validation.Entries[i].Level?.ManualGateNote))
                {
                    count++;
                }
            }

            return count;
        }

        private static float CalculateAverageDifficulty(ReleaseLevelPackValidationResult validation)
        {
            float total = 0f;
            int count = 0;
            for (int i = 0; i < validation.Entries.Count; i++)
            {
                if (validation.Entries[i].Metrics == null)
                {
                    continue;
                }

                total += validation.Entries[i].Metrics.DifficultyScore;
                count++;
            }

            return count == 0 ? 0f : total / count;
        }
    }
}
