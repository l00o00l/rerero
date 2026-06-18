using System.Globalization;
using System.Linq;
using System.Text;

namespace Thkim.DreamLaundromat.Gameplay.ReleaseSlice
{
    public static class ReleaseLevelPackReportFormatter
    {
        public static string Format(ReleaseLevelPackValidationResult result)
        {
            var builder = new StringBuilder();
            builder.AppendLine("DreamLaundromat Release Slice Validation Report");
            builder.AppendLine($"Valid={result.IsValid}");
            builder.AppendLine($"Levels={result.Entries.Count}");
            builder.AppendLine($"Errors={result.Errors.Count}");
            builder.AppendLine($"Warnings={result.Warnings.Count}");
            builder.AppendLine($"DesignNotes={result.DesignNotes.Count}");
            builder.AppendLine($"GuidedLevels={result.GuidedLevelCount}");
            builder.AppendLine($"ItemLevels={result.ItemLevelCount}");
            builder.AppendLine($"ObstacleLevels={result.ObstacleLevelCount}");
            builder.AppendLine($"DifficultyBands={string.Join(", ", result.DifficultyBands.OrderBy(band => (int)band))}");
            builder.AppendLine($"ModifierEffects={string.Join(", ", result.ModifierEffects.OrderBy(effect => effect.ToString()))}");
            builder.AppendLine($"TutorialTags={string.Join(", ", result.TutorialTags.OrderBy(tag => tag))}");
            builder.AppendLine();

            for (int i = 0; i < result.Entries.Count; i++)
            {
                ReleaseLevelValidationEntry entry = result.Entries[i];
                string levelId = entry.Level?.LevelId ?? "<missing>";
                string minMoves = entry.SolveResult == null ? "n/a" : entry.SolveResult.MinMoves.ToString(CultureInfo.InvariantCulture);
                string difficulty = entry.Metrics == null
                    ? "n/a"
                    : entry.Metrics.DifficultyScore.ToString("0.00", CultureInfo.InvariantCulture);
                builder.AppendLine($"[{i + 1:00}] {levelId} valid={entry.IsValid} minMoves={minMoves} difficulty={difficulty}");

                if (entry.Level != null)
                {
                    builder.AppendLine($"     phase: {entry.Level.Phase} band: {entry.Level.DifficultyBand} tags: {string.Join(", ", entry.Level.TutorialTags)}");
                    builder.AppendLine($"     intent: {entry.Level.DesignIntent}");
                    builder.AppendLine($"     question: {entry.Level.PlayerQuestion}");
                    builder.AppendLine($"     risk: {entry.Level.RiskNote}");
                    if (!string.IsNullOrWhiteSpace(entry.Level.ManualGateNote))
                    {
                        builder.AppendLine($"     manual: {entry.Level.ManualGateNote}");
                    }
                }
            }

            AppendMessages(builder, "Errors", result.Errors);
            AppendMessages(builder, "Warnings", result.Warnings);
            AppendMessages(builder, "Design Notes", result.DesignNotes);
            return builder.ToString();
        }

        private static void AppendMessages(StringBuilder builder, string title, System.Collections.Generic.List<string> messages)
        {
            if (messages.Count == 0)
            {
                return;
            }

            builder.AppendLine();
            builder.AppendLine(title);
            for (int i = 0; i < messages.Count; i++)
            {
                builder.AppendLine($"- {messages[i]}");
            }
        }
    }
}
