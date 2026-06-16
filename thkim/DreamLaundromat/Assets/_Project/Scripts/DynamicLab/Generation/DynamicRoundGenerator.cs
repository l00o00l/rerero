using System;
using System.Collections.Generic;

namespace Thkim.DreamLaundromat.DynamicLab
{
    public static class DynamicRoundGenerator
    {
        public static DynamicRoundCandidateReport GenerateCandidate(
            DynamicStageRecipe recipe,
            int seed,
            DynamicSolveOptions solveOptions = null)
        {
            var report = new DynamicRoundCandidateReport
            {
                RecipeId = recipe?.RecipeId ?? string.Empty,
                DesignIntent = recipe?.DesignIntent ?? string.Empty,
                PlayerQuestion = recipe?.PlayerQuestion ?? string.Empty,
                RiskNote = recipe?.RiskNote ?? string.Empty,
                Seed = seed
            };

            report.RecipeValidation = DynamicStageRecipeValidator.Validate(recipe);
            if (!report.RecipeValidation.IsValid)
            {
                AddRejectReasons(report, "Recipe", report.RecipeValidation.Errors);
                return report;
            }

            report.Round = CreateRound(recipe, seed);
            report.RoundId = report.Round.RoundId;
            report.HardValidation = DynamicRoundHardValidator.Validate(report.Round);
            AddWarnings(report, report.HardValidation.Warnings);
            if (!report.HardValidation.IsValid)
            {
                AddRejectReasons(report, "Hard validation", report.HardValidation.Errors);
                return report;
            }

            report.SolveResult = DynamicRoundSolver.Solve(report.Round, solveOptions);
            if (report.SolveResult.HitLimit)
            {
                report.RejectReasons.Add("Solver limit was reached.");
                return report;
            }

            if (!report.SolveResult.Solvable)
            {
                report.RejectReasons.Add("Solver could not clear the candidate.");
                return report;
            }

            report.Metrics = DynamicRoundMetricCalculator.Calculate(report.Round, report.SolveResult);
            TryCalculateWithoutItemComparison(recipe, report, solveOptions);
            ApplyRecipeAcceptance(recipe, report);

            report.DesignValidation = DynamicRoundDesignValidator.Validate(
                report.Round,
                report.SolveResult,
                report.Metrics);
            AddWarnings(report, report.DesignValidation.Warnings);
            if (!report.DesignValidation.IsValid)
            {
                AddRejectReasons(report, "Design validation", report.DesignValidation.Errors);
            }

            if (recipe.RejectOnDesignWarnings && report.DesignValidation.Warnings.Count > 0)
            {
                report.RejectReasons.Add("Design warnings are configured as rejection reasons.");
            }

            report.Accepted = report.RejectReasons.Count == 0;
            return report;
        }

        private static DynamicRoundDefinition CreateRound(DynamicStageRecipe recipe, int seed)
        {
            string prefix = string.IsNullOrWhiteSpace(recipe.RoundIdPrefix)
                ? recipe.RecipeId
                : recipe.RoundIdPrefix;

            return new DynamicRoundDefinition
            {
                RoundId = $"{prefix}-{seed}",
                Seed = seed,
                MoveLimit = recipe.MoveLimit,
                TargetCompletedOrders = recipe.TargetCompletedOrders,
                StreamConfig = CloneStreamConfig(recipe.StreamConfig),
                StorageConfig = CloneStorageConfig(recipe.StorageConfig),
                ActionSet = (DynamicOperation[])recipe.ActionSet.Clone(),
                Modifiers = CloneModifiers(recipe.AllowedModifiers),
                DreamBag = CreateDreamBag(recipe, seed),
                OrderDeck = CreateOrderDeck(recipe, seed),
                TutorialTags = (string[])recipe.TutorialTags.Clone(),
                DifficultyTarget = recipe.DifficultyTarget
            };
        }

        private static DynamicDreamBagEntry[] CreateDreamBag(DynamicStageRecipe recipe, int seed)
        {
            var random = new SeededRandom(seed ^ unchecked((int)0xA341316C));
            var counts = new List<DreamCount>();
            for (int i = 0; i < recipe.CandidateDreamCount; i++)
            {
                DynamicDreamAttributes attributes = recipe.DreamPool[NextWeightedIndex(recipe.DreamPool, ref random)].Attributes;
                int index = counts.FindIndex(item => item.Attributes == attributes);
                if (index >= 0)
                {
                    counts[index].Count++;
                }
                else
                {
                    counts.Add(new DreamCount(attributes, 1));
                }
            }

            var entries = new DynamicDreamBagEntry[counts.Count];
            for (int i = 0; i < counts.Count; i++)
            {
                entries[i] = new DynamicDreamBagEntry(counts[i].Attributes, counts[i].Count);
            }

            return entries;
        }

        private static DynamicOrderDeckEntry[] CreateOrderDeck(DynamicStageRecipe recipe, int seed)
        {
            var random = new SeededRandom(seed ^ unchecked((int)0xC8013EA4));
            var counts = new List<OrderCount>();
            for (int i = 0; i < recipe.CandidateOrderCount; i++)
            {
                DynamicOrderRequirement requirement = recipe.OrderPool[NextWeightedIndex(recipe.OrderPool, ref random)].Requirement;
                int index = counts.FindIndex(item => item.Requirement.Equals(requirement));
                if (index >= 0)
                {
                    counts[index].Count++;
                }
                else
                {
                    counts.Add(new OrderCount(requirement, 1));
                }
            }

            var entries = new DynamicOrderDeckEntry[counts.Count];
            for (int i = 0; i < counts.Count; i++)
            {
                entries[i] = new DynamicOrderDeckEntry(counts[i].Requirement, counts[i].Count);
            }

            return entries;
        }

        private static int NextWeightedIndex(DynamicWeightedDreamEntry[] entries, ref SeededRandom random)
        {
            int totalWeight = 0;
            for (int i = 0; i < entries.Length; i++)
            {
                totalWeight += Math.Max(0, entries[i].Weight);
            }

            int roll = random.NextInt(totalWeight);
            for (int i = 0; i < entries.Length; i++)
            {
                roll -= Math.Max(0, entries[i].Weight);
                if (roll < 0)
                {
                    return i;
                }
            }

            return entries.Length - 1;
        }

        private static int NextWeightedIndex(DynamicWeightedOrderEntry[] entries, ref SeededRandom random)
        {
            int totalWeight = 0;
            for (int i = 0; i < entries.Length; i++)
            {
                totalWeight += Math.Max(0, entries[i].Weight);
            }

            int roll = random.NextInt(totalWeight);
            for (int i = 0; i < entries.Length; i++)
            {
                roll -= Math.Max(0, entries[i].Weight);
                if (roll < 0)
                {
                    return i;
                }
            }

            return entries.Length - 1;
        }

        private static DynamicStreamConfig CloneStreamConfig(DynamicStreamConfig source)
        {
            return new DynamicStreamConfig
            {
                ActiveDreamSlots = source.ActiveDreamSlots,
                ActiveOrderSlots = source.ActiveOrderSlots,
                DreamPreviewCount = source.DreamPreviewCount,
                OrderPreviewCount = source.OrderPreviewCount,
                MaxDreamDraws = source.MaxDreamDraws,
                MaxOrderDraws = source.MaxOrderDraws
            };
        }

        private static DynamicStorageConfig CloneStorageConfig(DynamicStorageConfig source)
        {
            return new DynamicStorageConfig
            {
                StorageSlotCount = source.StorageSlotCount
            };
        }

        private static DynamicModifierDefinition[] CloneModifiers(DynamicModifierDefinition[] source)
        {
            if (source == null || source.Length == 0)
            {
                return Array.Empty<DynamicModifierDefinition>();
            }

            var clone = new DynamicModifierDefinition[source.Length];
            for (int i = 0; i < source.Length; i++)
            {
                clone[i] = source[i]?.Clone();
            }

            return clone;
        }

        private static void TryCalculateWithoutItemComparison(
            DynamicStageRecipe recipe,
            DynamicRoundCandidateReport report,
            DynamicSolveOptions solveOptions)
        {
            if (!recipe.CompareWithoutItems || report.Round.Modifiers.Length == 0)
            {
                return;
            }

            DynamicRoundDefinition comparisonRound = CloneRoundWithoutItems(report.Round);
            DynamicSolveResult comparisonSolve = DynamicRoundSolver.Solve(comparisonRound, solveOptions);
            if (comparisonSolve.HitLimit)
            {
                report.Metrics.MinMovesWithoutItemsAvailable = false;
                return;
            }

            report.Metrics.MinMovesWithoutItemsAvailable = comparisonSolve.Solvable;
            report.Metrics.MinMovesWithoutItems = comparisonSolve.Solvable ? comparisonSolve.MinMoves : -1;
        }

        private static DynamicRoundDefinition CloneRoundWithoutItems(DynamicRoundDefinition source)
        {
            var modifiers = new List<DynamicModifierDefinition>();
            for (int i = 0; i < source.Modifiers.Length; i++)
            {
                if (source.Modifiers[i].Type != DynamicModifierType.Item)
                {
                    modifiers.Add(source.Modifiers[i].Clone());
                }
            }

            return new DynamicRoundDefinition
            {
                RoundId = $"{source.RoundId}-without-items",
                Seed = source.Seed,
                MoveLimit = source.MoveLimit,
                TargetCompletedOrders = source.TargetCompletedOrders,
                StreamConfig = CloneStreamConfig(source.StreamConfig),
                StorageConfig = CloneStorageConfig(source.StorageConfig),
                ActionSet = (DynamicOperation[])source.ActionSet.Clone(),
                Modifiers = modifiers.ToArray(),
                DreamBag = (DynamicDreamBagEntry[])source.DreamBag.Clone(),
                OrderDeck = (DynamicOrderDeckEntry[])source.OrderDeck.Clone(),
                TutorialTags = source.TutorialTags == null ? Array.Empty<string>() : (string[])source.TutorialTags.Clone(),
                DifficultyTarget = source.DifficultyTarget
            };
        }

        private static void ApplyRecipeAcceptance(
            DynamicStageRecipe recipe,
            DynamicRoundCandidateReport report)
        {
            // The first optimal solution is the stable comparison point for early recipe tuning.
            if (report.Metrics.MinMoves < recipe.MinAcceptedMoves)
            {
                report.RejectReasons.Add("Minimum move target was not met.");
            }

            if (recipe.MaxAcceptedMoves > 0 && report.Metrics.MinMoves > recipe.MaxAcceptedMoves)
            {
                report.RejectReasons.Add("Maximum move target was exceeded.");
            }

            if (report.Metrics.ConversionCount < recipe.MinConversionCount)
            {
                report.RejectReasons.Add("Minimum conversion target was not met.");
            }

            if (report.Metrics.OperationDiversity < recipe.MinOperationDiversity)
            {
                report.RejectReasons.Add("Minimum operation diversity target was not met.");
            }

            if (report.Metrics.ActionTypeDiversity < recipe.MinActionTypeDiversity)
            {
                report.RejectReasons.Add("Minimum action type diversity target was not met.");
            }

            if (recipe.MaxRepeatedActionTypeRun > 0
                && report.Metrics.MaxRepeatedActionTypeRun > recipe.MaxRepeatedActionTypeRun)
            {
                report.RejectReasons.Add("Repeated action type run target was exceeded.");
            }

            if (recipe.MaxMoveSlack >= 0 && report.Metrics.MoveSlack > recipe.MaxMoveSlack)
            {
                report.RejectReasons.Add("Move slack target was exceeded.");
            }

            if (report.Metrics.StorageMoveRatio > recipe.MaxStorageMoveRatio)
            {
                report.RejectReasons.Add("Storage move ratio target was exceeded.");
            }

            if (report.Metrics.SettleActionRatio > recipe.MaxSettleActionRatio)
            {
                report.RejectReasons.Add("Settle action ratio target was exceeded.");
            }

            if (recipe.RequiresItem && report.Metrics.ItemUseCount <= 0)
            {
                report.RejectReasons.Add("Required item was not used by the first solution.");
            }
        }

        private static void AddRejectReasons(
            DynamicRoundCandidateReport report,
            string prefix,
            List<string> errors)
        {
            for (int i = 0; i < errors.Count; i++)
            {
                report.RejectReasons.Add($"{prefix}: {errors[i]}");
            }
        }

        private static void AddWarnings(DynamicRoundCandidateReport report, List<string> warnings)
        {
            for (int i = 0; i < warnings.Count; i++)
            {
                report.Warnings.Add(warnings[i]);
            }
        }

        private sealed class DreamCount
        {
            public DreamCount(DynamicDreamAttributes attributes, int count)
            {
                Attributes = attributes;
                Count = count;
            }

            public DynamicDreamAttributes Attributes { get; }
            public int Count;
        }

        private sealed class OrderCount
        {
            public OrderCount(DynamicOrderRequirement requirement, int count)
            {
                Requirement = requirement;
                Count = count;
            }

            public DynamicOrderRequirement Requirement { get; }
            public int Count;
        }
    }
}
