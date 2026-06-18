using System;
using System.Collections.Generic;
using Thkim.DreamLaundromat.DynamicLab;

namespace Thkim.DreamLaundromat.Gameplay.ReleaseSlice
{
    public static class ReleaseModifierImpactAudit
    {
        public static ReleaseModifierImpactReport Audit(
            ReleaseLevelPack pack,
            DynamicSolveOptions solveOptions = null)
        {
            ReleaseLevelPackValidationResult validation = ReleaseLevelPackValidator.Validate(
                pack,
                solveOptions ?? ReleaseValidationDefaults.SolveOptions);
            return Audit(validation, solveOptions);
        }

        public static ReleaseModifierImpactReport Audit(
            ReleaseLevelPackValidationResult validation,
            DynamicSolveOptions solveOptions = null)
        {
            var report = new ReleaseModifierImpactReport();
            if (validation == null)
            {
                report.Warnings.Add("Release level validation result is missing.");
                return report;
            }

            solveOptions ??= ReleaseValidationDefaults.SolveOptions;
            for (int i = 0; i < validation.Entries.Count; i++)
            {
                ReleaseLevelValidationEntry validationEntry = validation.Entries[i];
                if (validationEntry?.Round == null
                    || validationEntry.Round.Modifiers == null
                    || validationEntry.Round.Modifiers.Length == 0)
                {
                    continue;
                }

                report.AddEntry(CreateEntry(validationEntry, solveOptions));
            }

            if (report.Entries.Count == 0)
            {
                report.Warnings.Add("No modifier levels were available for impact audit.");
            }

            return report;
        }

        private static ReleaseModifierImpactEntry CreateEntry(
            ReleaseLevelValidationEntry validationEntry,
            DynamicSolveOptions solveOptions)
        {
            DynamicRoundDefinition originalRound = validationEntry.Round;
            DynamicRoundMetrics originalMetrics = validationEntry.Metrics;
            if (originalMetrics == null
                && validationEntry.SolveResult != null
                && validationEntry.SolveResult.Solvable
                && !validationEntry.SolveResult.HitLimit)
            {
                originalMetrics = DynamicRoundMetricCalculator.Calculate(originalRound, validationEntry.SolveResult);
            }

            var entry = new ReleaseModifierImpactEntry
            {
                LevelId = validationEntry.Level?.LevelId ?? originalRound.RoundId,
                Effects = CollectEffects(originalRound.Modifiers),
                ModifierCount = originalRound.Modifiers.Length,
                OriginalMinMoves = validationEntry.SolveResult?.MinMoves ?? -1,
                OriginalMaxBranchingFactor = validationEntry.SolveResult?.MaxBranchingFactor ?? 0,
                OriginalAverageBranchingFactor = validationEntry.SolveResult?.AverageBranchingFactor ?? 0f,
                OriginalItemUseCount = originalMetrics?.ItemUseCount ?? 0,
                OriginalObstacleBlockedActionCount = originalMetrics?.ObstacleBlockedActionCount ?? 0
            };

            CountModifierTypes(originalRound.Modifiers, entry);
            SolveWithoutModifiers(originalRound, solveOptions, entry);
            AddWarnings(entry);
            return entry;
        }

        private static DynamicModifierEffect[] CollectEffects(DynamicModifierDefinition[] modifiers)
        {
            var effects = new List<DynamicModifierEffect>();
            for (int i = 0; i < modifiers.Length; i++)
            {
                if (modifiers[i] != null && !effects.Contains(modifiers[i].Effect))
                {
                    effects.Add(modifiers[i].Effect);
                }
            }

            effects.Sort((left, right) => string.CompareOrdinal(left.ToString(), right.ToString()));
            return effects.ToArray();
        }

        private static void CountModifierTypes(
            DynamicModifierDefinition[] modifiers,
            ReleaseModifierImpactEntry entry)
        {
            for (int i = 0; i < modifiers.Length; i++)
            {
                if (modifiers[i] == null)
                {
                    continue;
                }

                if (modifiers[i].Type == DynamicModifierType.Item)
                {
                    entry.ItemCount++;
                }
                else if (modifiers[i].Type == DynamicModifierType.Obstacle)
                {
                    entry.ObstacleCount++;
                }
            }
        }

        private static void SolveWithoutModifiers(
            DynamicRoundDefinition originalRound,
            DynamicSolveOptions solveOptions,
            ReleaseModifierImpactEntry entry)
        {
            DynamicRoundDefinition comparisonRound = CloneRoundWithoutModifiers(originalRound);
            DynamicSolveResult comparisonSolve = DynamicRoundSolver.Solve(comparisonRound, solveOptions);
            entry.WithoutModifiersHitLimit = comparisonSolve.HitLimit;
            entry.WithoutModifiersSolvable = comparisonSolve.Solvable && !comparisonSolve.HitLimit;
            if (!entry.WithoutModifiersSolvable)
            {
                return;
            }

            entry.WithoutModifiersMinMoves = comparisonSolve.MinMoves;
            entry.WithoutModifiersMaxBranchingFactor = comparisonSolve.MaxBranchingFactor;
            entry.WithoutModifiersAverageBranchingFactor = comparisonSolve.AverageBranchingFactor;
        }

        private static void AddWarnings(ReleaseModifierImpactEntry entry)
        {
            if (entry.ItemCount > 0 && !entry.FirstSolutionUsesItem)
            {
                entry.Warnings.Add("Item is present but the first solver solution does not use it.");
            }

            if (entry.ObstacleCount > 0 && entry.OriginalObstacleBlockedActionCount == 0)
            {
                entry.Warnings.Add("Obstacle is present but the first solver solution never encounters a blocked core action.");
            }

            if (entry.WithoutModifiersHitLimit)
            {
                entry.Warnings.Add("Counterfactual solve without modifiers hit the solver limit.");
                return;
            }

            if (!entry.WithoutModifiersSolvable)
            {
                return;
            }

            bool moveUnchanged = entry.MinMoveDelta == 0;
            bool branchingUnchanged = entry.MaxBranchingDelta == 0;
            bool noDirectInteraction = entry.OriginalItemUseCount == 0
                && entry.OriginalObstacleBlockedActionCount == 0;
            if (moveUnchanged && branchingUnchanged && noDirectInteraction)
            {
                entry.Warnings.Add("Removing all modifiers does not change min moves, max branching, item use, or blocked action counts.");
            }

            if (entry.ItemCount > 0
                && entry.FirstSolutionUsesItem
                && entry.WithoutModifiersMinMoves <= entry.OriginalMinMoves)
            {
                entry.Warnings.Add("The first solver solution uses an item, but the no-modifier comparison is not more expensive.");
            }
        }

        private static DynamicRoundDefinition CloneRoundWithoutModifiers(DynamicRoundDefinition source)
        {
            return new DynamicRoundDefinition
            {
                RoundId = $"{source.RoundId}-without-modifiers",
                Seed = source.Seed,
                MoveLimit = source.MoveLimit,
                TargetCompletedOrders = source.TargetCompletedOrders,
                StreamConfig = CloneStreamConfig(source.StreamConfig),
                DreamBag = source.DreamBag == null
                    ? Array.Empty<DynamicDreamBagEntry>()
                    : (DynamicDreamBagEntry[])source.DreamBag.Clone(),
                OrderDeck = source.OrderDeck == null
                    ? Array.Empty<DynamicOrderDeckEntry>()
                    : (DynamicOrderDeckEntry[])source.OrderDeck.Clone(),
                ActionSet = source.ActionSet == null
                    ? Array.Empty<DynamicOperation>()
                    : (DynamicOperation[])source.ActionSet.Clone(),
                StorageConfig = CloneStorageConfig(source.StorageConfig),
                Modifiers = Array.Empty<DynamicModifierDefinition>(),
                TutorialTags = source.TutorialTags == null
                    ? Array.Empty<string>()
                    : (string[])source.TutorialTags.Clone(),
                DifficultyTarget = source.DifficultyTarget
            };
        }

        private static DynamicStreamConfig CloneStreamConfig(DynamicStreamConfig source)
        {
            if (source == null)
            {
                return new DynamicStreamConfig();
            }

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
            if (source == null)
            {
                return new DynamicStorageConfig();
            }

            return new DynamicStorageConfig
            {
                StorageSlotCount = source.StorageSlotCount
            };
        }
    }
}
