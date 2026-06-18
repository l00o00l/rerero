using System;
using System.Collections.Generic;
using Thkim.DreamLaundromat.DynamicLab;

namespace Thkim.DreamLaundromat.Gameplay.ReleaseSlice
{
    public static class ReleaseLevelPackValidator
    {
        public const int MinimumReleaseLevelCount = 30;

        public static ReleaseLevelPackValidationResult Validate(
            ReleaseLevelPack pack,
            DynamicSolveOptions solveOptions = null)
        {
            var result = new ReleaseLevelPackValidationResult();
            if (pack == null)
            {
                result.AddError("Release level pack is missing.");
                return result;
            }

            if (pack.Levels.Count == 0)
            {
                result.AddError("Release level pack is empty.");
                return result;
            }
            else if (pack.Levels.Count < MinimumReleaseLevelCount)
            {
                result.AddError($"Release level pack must contain at least {MinimumReleaseLevelCount} levels.");
            }

            solveOptions ??= ReleaseValidationDefaults.SolveOptions;
            var seenLevelIds = new HashSet<string>();
            for (int i = 0; i < pack.Levels.Count; i++)
            {
                ReleaseLevelDefinition level = pack.Levels[i];
                if (level == null)
                {
                    result.AddError($"Release level at index {i} is missing.");
                    continue;
                }

                if (!seenLevelIds.Add(level.LevelId))
                {
                    result.AddError($"Release level id {level.LevelId} is duplicated.");
                    continue;
                }

                ValidateLevel(level, solveOptions, result);
            }

            ValidatePackCoverage(result);

            return result;
        }

        private static void ValidateLevel(
            ReleaseLevelDefinition level,
            DynamicSolveOptions solveOptions,
            ReleaseLevelPackValidationResult result)
        {
            var entry = new ReleaseLevelValidationEntry
            {
                Level = level
            };
            result.Entries.Add(entry);

            try
            {
                entry.ReleaseValidation = ValidateReleaseMetadata(level);
                AppendMessages(result, level.LevelId, "release", entry.ReleaseValidation.Errors, isError: true);
                AppendMessages(result, level.LevelId, "release", entry.ReleaseValidation.Warnings, isError: false);
                int releaseErrorCount = entry.ReleaseValidation.Errors.Count;
                int releaseWarningCount = entry.ReleaseValidation.Warnings.Count;

                entry.Round = level.CreateRoundDefinition();
                TrackReleaseCoverage(level, entry.Round, result);
                entry.HardValidation = DynamicRoundHardValidator.Validate(entry.Round);
                AppendMessages(result, level.LevelId, "hard", entry.HardValidation.Errors, isError: true);
                AppendMessages(result, level.LevelId, "hard", entry.HardValidation.Warnings, isError: false);

                if (!entry.HardValidation.IsValid)
                {
                    return;
                }

                entry.SolveResult = DynamicRoundSolver.Solve(entry.Round, solveOptions);
                if (entry.SolveResult.HitLimit)
                {
                    result.AddError($"{level.LevelId}: solver limit was reached.");
                    return;
                }

                if (!entry.SolveResult.Solvable)
                {
                    result.AddError($"{level.LevelId}: solver could not clear the round.");
                    return;
                }

                entry.ReplayResult = DynamicSolveReplayVerifier.Verify(entry.Round, entry.SolveResult);
                if (!entry.ReplayResult.Success)
                {
                    result.AddError($"{level.LevelId}: solver replay failed: {entry.ReplayResult.FailureMessage}");
                    return;
                }

                ValidateGuidedActionPrefix(level, entry.SolveResult, entry.ReleaseValidation);
                AppendMessagesFromIndex(result, level.LevelId, "tutorial", entry.ReleaseValidation.Errors, releaseErrorCount, isError: true);
                AppendMessagesFromIndex(result, level.LevelId, "tutorial", entry.ReleaseValidation.Warnings, releaseWarningCount, isError: false);

                entry.Metrics = DynamicRoundMetricCalculator.Calculate(entry.Round, entry.SolveResult);
                entry.DesignValidation = DynamicRoundDesignValidator.Validate(
                    entry.Round,
                    entry.SolveResult,
                    entry.Metrics);
                AppendMessages(result, level.LevelId, "design", entry.DesignValidation.Errors, isError: true);
                AppendDesignWarnings(result, level.LevelId, entry.DesignValidation.Warnings);
            }
            catch (Exception exception)
            {
                result.AddError($"{level.LevelId}: validation threw {exception.GetType().Name}: {exception.Message}");
            }
        }

        private static DynamicValidationResult ValidateReleaseMetadata(ReleaseLevelDefinition level)
        {
            var validation = new DynamicValidationResult();
            if (string.IsNullOrWhiteSpace(level.DisplayName))
            {
                validation.AddError("Release level display name is missing.");
            }

            if (string.IsNullOrWhiteSpace(level.Guidance))
            {
                validation.AddError("Release level guidance is missing.");
            }

            if (string.IsNullOrWhiteSpace(level.DesignIntent))
            {
                validation.AddError("Release level design intent is missing.");
            }

            if (string.IsNullOrWhiteSpace(level.PlayerQuestion))
            {
                validation.AddError("Release level player question is missing.");
            }

            if (string.IsNullOrWhiteSpace(level.RiskNote))
            {
                validation.AddError("Release level risk note is missing.");
            }

            if (level.TutorialTags.Length == 0)
            {
                validation.AddError("Release level must declare at least one tutorial or QA tag.");
            }

            if (level.Phase < 1)
            {
                validation.AddError("Release level phase must be positive.");
            }

            return validation;
        }

        private static void ValidateGuidedActionPrefix(
            ReleaseLevelDefinition level,
            DynamicSolveResult solveResult,
            DynamicValidationResult validation)
        {
            if (level.GuidedActionRules.Length == 0 || solveResult == null || !solveResult.Solvable)
            {
                return;
            }

            if (solveResult.FirstSolutionActions.Count < level.GuidedActionRules.Length)
            {
                validation.AddError("Guided action rule count exceeds solver solution length.");
                return;
            }

            for (int i = 0; i < level.GuidedActionRules.Length; i++)
            {
                if (!level.GuidedActionRules[i].Matches(solveResult.FirstSolutionActions[i]))
                {
                    validation.AddError(
                        $"Guided action {i} expects {level.GuidedActionRules[i].Describe()} but solver uses {solveResult.FirstSolutionActions[i].Type}.");
                }
            }
        }

        private static void TrackReleaseCoverage(
            ReleaseLevelDefinition level,
            DynamicRoundDefinition round,
            ReleaseLevelPackValidationResult result)
        {
            result.DifficultyBands.Add(level.DifficultyBand);
            if (level.GuidedActionRules.Length > 0)
            {
                result.GuidedLevelCount++;
            }

            for (int i = 0; i < level.TutorialTags.Length; i++)
            {
                result.TutorialTags.Add(level.TutorialTags[i]);
            }

            for (int i = 0; i < round.Modifiers.Length; i++)
            {
                DynamicModifierDefinition modifier = round.Modifiers[i];
                result.ModifierEffects.Add(modifier.Effect);
                if (modifier.Type == DynamicModifierType.Item)
                {
                    result.ItemLevelCount++;
                }
                else if (modifier.Type == DynamicModifierType.Obstacle)
                {
                    result.ObstacleLevelCount++;
                }
            }
        }

        private static void ValidatePackCoverage(ReleaseLevelPackValidationResult result)
        {
            RequireModifier(result, DynamicModifierEffect.PreviewSwap);
            RequireModifier(result, DynamicModifierEffect.LockActiveDreamSlot);
            RequireModifier(result, DynamicModifierEffect.PinOrderSlot);
            RequireModifier(result, DynamicModifierEffect.RefreshActiveDream);
            RequireModifier(result, DynamicModifierEffect.SoftBlockOperation);

            if (result.GuidedLevelCount == 0)
            {
                result.AddError("Release level pack must include at least one guided tutorial level.");
            }

            if (result.DifficultyBands.Count < 4)
            {
                result.AddError("Release level pack must cover at least four difficulty bands.");
            }

            if (result.TutorialTags.Count < 10)
            {
                result.AddError("Release level pack must expose at least ten tutorial/QA tags.");
            }
        }

        private static void RequireModifier(
            ReleaseLevelPackValidationResult result,
            DynamicModifierEffect effect)
        {
            if (!result.ModifierEffects.Contains(effect))
            {
                result.AddError($"Release level pack does not cover modifier effect {effect}.");
            }
        }

        private static void AppendMessages(
            ReleaseLevelPackValidationResult result,
            string levelId,
            string source,
            List<string> messages,
            bool isError)
        {
            if (messages == null)
            {
                return;
            }

            for (int i = 0; i < messages.Count; i++)
            {
                string message = $"{levelId}: {source}: {messages[i]}";
                if (isError)
                {
                    result.AddError(message);
                }
                else
                {
                    result.AddWarning(message);
                }
            }
        }

        private static void AppendMessagesFromIndex(
            ReleaseLevelPackValidationResult result,
            string levelId,
            string source,
            List<string> messages,
            int startIndex,
            bool isError)
        {
            if (messages == null)
            {
                return;
            }

            for (int i = System.Math.Max(0, startIndex); i < messages.Count; i++)
            {
                string message = $"{levelId}: {source}: {messages[i]}";
                if (isError)
                {
                    result.AddError(message);
                }
                else
                {
                    result.AddWarning(message);
                }
            }
        }

        private static void AppendDesignWarnings(
            ReleaseLevelPackValidationResult result,
            string levelId,
            List<string> messages)
        {
            if (messages == null)
            {
                return;
            }

            for (int i = 0; i < messages.Count; i++)
            {
                string message = $"{levelId}: design: {messages[i]}";
                if (IsAcceptedReleaseDesignNote(messages[i]))
                {
                    result.AddDesignNote(message);
                }
                else
                {
                    result.AddWarning(message);
                }
            }
        }

        private static bool IsAcceptedReleaseDesignNote(string warning)
        {
            // These are useful QA/balance observations, but they are not release
            // gate warnings for this slice because several levels intentionally
            // isolate one mechanic before combining it with full order pressure.
            return warning == "No obvious order competition in the deck."
                || warning == "Only one active order is visible, so order competition is muted."
                || warning == "Preview is unlikely to affect decisions in this round."
                || warning == "First solution has a mechanical operation-submit cadence."
                || warning == "Storage moves dominate the first solution."
                || warning == "First solution is mostly direct submit actions."
                || warning == "First solution uses only one action category.";
        }
    }
}
