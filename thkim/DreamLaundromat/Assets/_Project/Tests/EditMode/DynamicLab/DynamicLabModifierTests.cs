using NUnit.Framework;
using Thkim.DreamLaundromat.DynamicLab;

namespace Thkim.DreamLaundromat.Tests.EditMode.DynamicLab
{
    public sealed class DynamicLabModifierTests
    {
        [Test]
        public void HardValidator_RejectsUnsupportedModifierScope()
        {
            DynamicRoundDefinition round = DynamicSampleRounds.CreateStateAssignmentRound();
            round.Modifiers = new[]
            {
                new DynamicModifierDefinition
                {
                    Id = "invalid-preview-swap",
                    DisplayName = "Invalid Preview Swap",
                    Type = DynamicModifierType.Item,
                    Trigger = DynamicModifierTrigger.Manual,
                    Scope = DynamicModifierScope.Round,
                    Effect = DynamicModifierEffect.PreviewSwap,
                    Charges = 1
                }
            };

            DynamicValidationResult result = DynamicRoundHardValidator.Validate(round);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors.Exists(error => error.Contains("Preview Swap")), Is.True);
        }

        [Test]
        public void HardValidator_RejectsDuplicateModifierIds()
        {
            DynamicRoundDefinition round = DynamicSampleRounds.CreatePreviewSwapRequiredRound();
            round.Modifiers = new[]
            {
                DynamicBuiltInModifiers.PreviewSwap(),
                DynamicBuiltInModifiers.PreviewSwap()
            };

            DynamicValidationResult result = DynamicRoundHardValidator.Validate(round);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors.Exists(error => error.Contains("duplicated")), Is.True);
        }

        [Test]
        public void ModifierState_ClonesIndependentlyAndAffectsHash()
        {
            DynamicRoundState state = DynamicRoundInitializer.CreateInitialState(DynamicSampleRounds.CreatePreviewSwapRequiredRound());
            DynamicRoundState clone = state.Clone();
            string initialHash = DynamicRoundStateHasher.CreateHash(state);

            clone.Modifiers[0].RemainingCharges = 0;
            clone.Modifiers[0].IsResolved = true;

            Assert.That(clone.Modifiers[0].RemainingCharges, Is.Not.EqualTo(state.Modifiers[0].RemainingCharges));
            Assert.That(DynamicRoundStateHasher.CreateHash(clone), Is.Not.EqualTo(initialHash));
        }

        [Test]
        public void PreviewSwap_SwapsDreamPreviewAndConsumesMoveAndCharge()
        {
            DynamicRoundState state = DynamicRoundInitializer.CreateInitialState(DynamicSampleRounds.CreatePreviewSwapRequiredRound());
            string firstPreviewId = state.DreamPreview[0].Id;
            string secondPreviewId = state.DreamPreview[1].Id;

            DynamicActionResult result = DynamicRulesEngine.Apply(
                state,
                DynamicPlayerAction.UseItem(DynamicBuiltInModifiers.PreviewSwapId));

            Assert.That(result.Success, Is.True, result.Message);
            Assert.That(state.DreamPreview[0].Id, Is.EqualTo(secondPreviewId));
            Assert.That(state.DreamPreview[1].Id, Is.EqualTo(firstPreviewId));
            Assert.That(state.RemainingMoves, Is.EqualTo(2));
            Assert.That(state.Modifiers[0].RemainingCharges, Is.EqualTo(0));
            Assert.That(state.Modifiers[0].IsResolved, Is.True);
        }

        [Test]
        public void PreviewSwap_WithInsufficientPreviewDoesNotSpendMove()
        {
            DynamicRoundDefinition round = DynamicSampleRounds.CreatePreviewSwapRequiredRound();
            round.StreamConfig.DreamPreviewCount = 1;
            DynamicRoundState state = DynamicRoundInitializer.CreateInitialState(round);

            DynamicActionResult result = DynamicRulesEngine.Apply(
                state,
                DynamicPlayerAction.UseItem(DynamicBuiltInModifiers.PreviewSwapId));

            Assert.That(result.Success, Is.False);
            Assert.That(state.RemainingMoves, Is.EqualTo(3));
            Assert.That(state.Modifiers[0].RemainingCharges, Is.EqualTo(1));
        }

        [Test]
        public void PreviewSwap_WithInsufficientPreviewIsNotEnumerated()
        {
            DynamicRoundDefinition round = DynamicSampleRounds.CreatePreviewSwapRequiredRound();
            round.StreamConfig.DreamPreviewCount = 1;
            DynamicRoundState state = DynamicRoundInitializer.CreateInitialState(round);

            var actions = DynamicModifierPipeline.EnumerateExtraActions(state);

            Assert.That(actions.Exists(action => action.Type == DynamicActionType.UseItem), Is.False);
        }

        [Test]
        public void LockedSlot_BlocksCoreActionWithoutSpendingMove()
        {
            DynamicRoundState state = DynamicRoundInitializer.CreateInitialState(DynamicSampleRounds.CreateLockedSlotRound());

            DynamicActionResult result = DynamicRulesEngine.Apply(
                state,
                DynamicPlayerAction.SubmitDream(0, 0));

            Assert.That(result.Success, Is.False);
            Assert.That(result.Message, Does.Contain("locked"));
            Assert.That(state.RemainingMoves, Is.EqualTo(4));
        }

        [Test]
        public void Solver_UsesPreviewSwapWhenRequiredAndReplayClears()
        {
            DynamicRoundDefinition round = DynamicSampleRounds.CreatePreviewSwapRequiredRound();
            DynamicRoundState manualState = DynamicRoundInitializer.CreateInitialState(round);

            Assert.That(DynamicRulesEngine.Apply(manualState, DynamicPlayerAction.UseItem(DynamicBuiltInModifiers.PreviewSwapId)).Success, Is.True);
            Assert.That(DynamicRulesEngine.Apply(manualState, DynamicPlayerAction.SubmitDream(0, 0)).Success, Is.True);
            Assert.That(DynamicRulesEngine.Apply(manualState, DynamicPlayerAction.SubmitDream(0, 0)).Success, Is.True);
            Assert.That(manualState.Status, Is.EqualTo(DynamicRoundStatus.Cleared));

            DynamicSolveResult solve = DynamicRoundSolver.Solve(round);
            DynamicSolveReplayResult replay = DynamicSolveReplayVerifier.Verify(round, solve);

            Assert.That(solve.Solvable, Is.True, $"HitLimit={solve.HitLimit} Visited={solve.VisitedStates} DeadEnds={solve.DeadEndCount}");
            Assert.That(solve.MinMoves, Is.EqualTo(3));
            Assert.That(solve.FirstSolutionActions[0].Type, Is.EqualTo(DynamicActionType.UseItem));
            Assert.That(replay.Success, Is.True, replay.FailureMessage);
        }

        [Test]
        public void Solver_AvoidsLockedSlotAndTracksBlockedCandidates()
        {
            DynamicRoundDefinition round = DynamicSampleRounds.CreateLockedSlotRound();

            DynamicSolveResult solve = DynamicRoundSolver.Solve(round);
            DynamicRoundMetrics metrics = DynamicRoundMetricCalculator.Calculate(round, solve);

            Assert.That(solve.Solvable, Is.True);
            Assert.That(solve.FirstSolutionActions[0].ActiveDreamSlotId, Is.EqualTo(1));
            Assert.That(metrics.ObstacleBlockedActionCount, Is.GreaterThan(0));
        }
    }
}
