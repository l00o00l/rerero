using System;
using System.Collections.Generic;
using System.Text;

namespace Thkim.DreamLaundromat.DynamicLab
{
    public sealed class DynamicBatchSimulationOptions
    {
        public int SeedStart = 1;
        public int CandidateCountPerRecipe = 8;
        public DynamicSolveOptions SolveOptions = new DynamicSolveOptions();
    }

    public sealed class DynamicBatchSimulationResult
    {
        public readonly List<DynamicRoundCandidateReport> AllCandidates = new List<DynamicRoundCandidateReport>();
        public readonly List<DynamicRoundCandidateReport> AcceptedCandidates = new List<DynamicRoundCandidateReport>();
        public readonly List<DynamicRoundCandidateReport> RejectedCandidates = new List<DynamicRoundCandidateReport>();

        public int TotalCount => AllCandidates.Count;
        public int AcceptedCount => AcceptedCandidates.Count;
        public int RejectedCount => RejectedCandidates.Count;
    }

    public static class DynamicRoundBatchSimulator
    {
        public static DynamicBatchSimulationResult Run(
            DynamicStageRecipe[] recipes,
            DynamicBatchSimulationOptions options = null)
        {
            options ??= new DynamicBatchSimulationOptions();
            var result = new DynamicBatchSimulationResult();
            if (recipes == null || recipes.Length == 0 || options.CandidateCountPerRecipe <= 0)
            {
                return result;
            }

            for (int recipeIndex = 0; recipeIndex < recipes.Length; recipeIndex++)
            {
                for (int candidateIndex = 0; candidateIndex < options.CandidateCountPerRecipe; candidateIndex++)
                {
                    int seed = options.SeedStart + candidateIndex;
                    DynamicRoundCandidateReport report = DynamicRoundGenerator.GenerateCandidate(
                        recipes[recipeIndex],
                        seed,
                        options.SolveOptions);

                    result.AllCandidates.Add(report);
                    if (report.Accepted)
                    {
                        result.AcceptedCandidates.Add(report);
                    }
                    else
                    {
                        result.RejectedCandidates.Add(report);
                    }
                }
            }

            return result;
        }
    }

    public static class DynamicBatchReportFormatter
    {
        public static string Format(DynamicBatchSimulationResult result)
        {
            if (result == null)
            {
                return "No dynamic batch result.";
            }

            var builder = new StringBuilder();
            builder.AppendLine($"Total={result.TotalCount} Accepted={result.AcceptedCount} Rejected={result.RejectedCount}");
            for (int i = 0; i < result.AllCandidates.Count; i++)
            {
                DynamicRoundCandidateReport candidate = result.AllCandidates[i];
                builder.Append(candidate.Accepted ? "ACCEPT " : "REJECT ");
                builder.Append(candidate.RoundId);
                builder.Append(" seed=");
                builder.Append(candidate.Seed.ToString(System.Globalization.CultureInfo.InvariantCulture));

                if (candidate.Metrics != null)
                {
                    builder.Append(" minMoves=");
                    builder.Append(candidate.Metrics.MinMoves.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    builder.Append(" score=");
                    builder.Append(candidate.Metrics.DifficultyScore.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture));
                    builder.Append(" actionTypes=");
                    builder.Append(candidate.Metrics.ActionTypeDiversity.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    builder.Append(" items=");
                    builder.Append(candidate.Metrics.ItemUseCount.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    builder.Append(" blocked=");
                    builder.Append(candidate.Metrics.ObstacleBlockedActionCount.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    if (candidate.Metrics.MinMovesWithoutItemsAvailable)
                    {
                        builder.Append(" minWithoutItems=");
                        builder.Append(candidate.Metrics.MinMovesWithoutItems.ToString(System.Globalization.CultureInfo.InvariantCulture));
                    }
                }

                if (candidate.Round != null && candidate.Round.Modifiers.Length > 0)
                {
                    builder.Append(" modifiers=");
                    builder.Append(DescribeModifiers(candidate.Round.Modifiers));
                }

                if (candidate.RejectReasons.Count > 0)
                {
                    builder.Append(" reasons=");
                    builder.Append(string.Join(" | ", candidate.RejectReasons));
                }

                if (candidate.Warnings.Count > 0)
                {
                    builder.Append(" warnings=");
                    builder.Append(string.Join(" | ", candidate.Warnings));
                }

                builder.AppendLine();

                if (!string.IsNullOrEmpty(candidate.DesignIntent))
                {
                    builder.AppendLine($"  intent: {candidate.DesignIntent}");
                }

                if (!string.IsNullOrEmpty(candidate.PlayerQuestion))
                {
                    builder.AppendLine($"  question: {candidate.PlayerQuestion}");
                }

                if (!string.IsNullOrEmpty(candidate.RiskNote))
                {
                    builder.AppendLine($"  risk: {candidate.RiskNote}");
                }
            }

            return builder.ToString();
        }

        private static string DescribeModifiers(DynamicModifierDefinition[] modifiers)
        {
            var values = new List<string>();
            for (int i = 0; i < modifiers.Length; i++)
            {
                values.Add($"{modifiers[i].Id}:{modifiers[i].Effect}");
            }

            return string.Join(",", values);
        }
    }
}
