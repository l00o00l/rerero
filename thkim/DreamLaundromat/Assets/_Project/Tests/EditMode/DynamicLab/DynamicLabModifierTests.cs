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
            int initialMoves = state.RemainingMoves;
            string firstPreviewId = state.DreamPreview[0].Id;
            string secondPreviewId = state.DreamPreview[1].Id;

            DynamicActionResult result = DynamicRulesEngine.Apply(
                state,
                DynamicPlayerAction.UseItem(DynamicBuiltInModifiers.PreviewSwapId));

            Assert.That(result.Success, Is.True, result.Message);
            Assert.That(state.DreamPreview[0].Id, Is.EqualTo(secondPreviewId));
            Assert.That(state.DreamPreview[1].Id, Is.EqualTo(firstPreviewId));
            Assert.That(state.RemainingMoves, Is.EqualTo(initialMoves - 1));
            Assert.That(state.Modifiers[0].RemainingCharges, Is.EqualTo(0));
            Assert.That(state.Modifiers[0].IsResolved, Is.True);
        }

        [Test]
        public void PreviewSwap_WithInsufficientPreviewDoesNotSpendMove()
        {
            DynamicRoundDefinition round = DynamicSampleRounds.CreatePreviewSwapRequiredRound();
            round.StreamConfig.DreamPreviewCount = 1;
            DynamicRoundState state = DynamicRoundInitializer.CreateInitialState(round);
            int initialMoves = state.RemainingMoves;

            DynamicActionResult result = DynamicRulesEngine.Apply(
                state,
                DynamicPlayerAction.UseItem(DynamicBuiltInModifiers.PreviewSwapId));

            Assert.That(result.Success, Is.False);
            Assert.That(state.RemainingMoves, Is.EqualTo(initialMoves));
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

        [Test]
        public void HardValidator_AcceptsPhase613ModifierSet()
        {
            DynamicRoundDefinition round = CreateSettleSubmitRound();
            round.Modifiers = new[]
            {
                DynamicBuiltInModifiers.DreamRefresh(),
                DynamicBuiltInModifiers.OrderPin(0),
                DynamicBuiltInModifiers.OperationSoftBlock(DynamicOperation.Settle)
            };

            DynamicValidationResult result = DynamicRoundHardValidator.Validate(round);

            Assert.That(result.IsValid, Is.True, string.Join(" | ", result.Errors));
        }

        [Test]
        public void DreamRefresh_TargetsActiveDreamAndConsumesCharge()
        {
            DynamicRoundDefinition round = CreateDreamRefreshRound();
            DynamicRoundState state = DynamicRoundInitializer.CreateInitialState(round);
            string originalActiveId = state.ActiveDreams[0].Dream.Id;
            string originalPreviewId = state.DreamPreview[0].Id;

            DynamicActionResult result = DynamicRulesEngine.Apply(
                state,
                DynamicPlayerAction.UseItem(DynamicBuiltInModifiers.DreamRefreshId, 0));

            Assert.That(result.Success, Is.True, result.Message);
            Assert.That(state.ActiveDreams[0].Dream.Id, Is.EqualTo(originalPreviewId));
            Assert.That(state.DreamDrawPile[state.DreamDrawPile.Count - 1].Id, Is.EqualTo(originalActiveId));
            Assert.That(state.Modifiers[0].RemainingCharges, Is.EqualTo(0));
            Assert.That(state.Modifiers[0].IsResolved, Is.True);
        }

        [Test]
        public void ConsumingItem_AdvancesTimedObstacles()
        {
            DynamicRoundDefinition round = CreateDreamRefreshRound();
            round.Modifiers = new[]
            {
                DynamicBuiltInModifiers.DreamRefresh(),
                DynamicBuiltInModifiers.OrderPin(0)
            };
            DynamicRoundState state = DynamicRoundInitializer.CreateInitialState(round);
            DynamicModifierState orderPin = DynamicModifierPipeline.FindState(
                state,
                $"{DynamicBuiltInModifiers.OrderPinIdPrefix}-0");
            Assert.That(orderPin, Is.Not.Null);

            DynamicActionResult result = DynamicRulesEngine.Apply(
                state,
                DynamicPlayerAction.UseItem(DynamicBuiltInModifiers.DreamRefreshId, 0));

            Assert.That(result.Success, Is.True, result.Message);
            Assert.That(state.RemainingMoves, Is.EqualTo(5));
            Assert.That(orderPin.IsResolved, Is.True);
        }

        [Test]
        public void DreamRefresh_EnumeratesOneActionPerRefreshableActiveDream()
        {
            DynamicRoundState state = DynamicRoundInitializer.CreateInitialState(CreateDreamRefreshRound());

            var actions = DynamicModifierPipeline.EnumerateExtraActions(state);

            Assert.That(actions.Exists(action =>
                action.Type == DynamicActionType.UseItem
                && action.ModifierId == DynamicBuiltInModifiers.DreamRefreshId
                && action.ModifierTargetId == 0), Is.True);
        }

        [Test]
        public void StateHash_IncludesDreamRefreshDrawTail()
        {
            DynamicRoundState refreshed = DynamicRoundInitializer.CreateInitialState(CreateDreamRefreshRound());
            DynamicActionResult result = DynamicRulesEngine.Apply(
                refreshed,
                DynamicPlayerAction.UseItem(DynamicBuiltInModifiers.DreamRefreshId, 0));
            DynamicRoundState sameVisibleStateWithoutTail = refreshed.Clone();
            sameVisibleStateWithoutTail.DreamDrawPile.RemoveAt(sameVisibleStateWithoutTail.DreamDrawPile.Count - 1);

            Assert.That(result.Success, Is.True, result.Message);
            Assert.That(
                DynamicRoundStateHasher.CreateHash(refreshed),
                Is.Not.EqualTo(DynamicRoundStateHasher.CreateHash(sameVisibleStateWithoutTail)));
        }

        [Test]
        public void OrderPin_BlocksTargetOrderUntilAnotherActionAdvancesIt()
        {
            DynamicRoundDefinition round = CreateSettleSubmitRound();
            round.Modifiers = new[] { DynamicBuiltInModifiers.OrderPin(0) };
            DynamicRoundState state = DynamicRoundInitializer.CreateInitialState(round);

            DynamicActionResult blocked = DynamicRulesEngine.Apply(state, DynamicPlayerAction.SubmitDream(0, 0));
            DynamicActionResult settle = DynamicRulesEngine.Apply(state, DynamicPlayerAction.ApplyOperation(0, DynamicOperation.Settle));
            DynamicActionResult submit = DynamicRulesEngine.Apply(state, DynamicPlayerAction.SubmitDream(0, 0));

            Assert.That(blocked.Success, Is.False);
            Assert.That(blocked.Message, Does.Contain("pinned"));
            Assert.That(state.Modifiers[0].IsResolved, Is.True);
            Assert.That(settle.Success, Is.True, settle.Message);
            Assert.That(submit.Success, Is.True, submit.Message);
            Assert.That(state.Status, Is.EqualTo(DynamicRoundStatus.Cleared));
        }

        [Test]
        public void OperationSoftBlock_BlocksOperationUntilAnotherActionAdvancesIt()
        {
            DynamicRoundDefinition round = CreateSettleSubmitRound();
            round.Modifiers = new[] { DynamicBuiltInModifiers.OperationSoftBlock(DynamicOperation.Settle) };
            DynamicRoundState state = DynamicRoundInitializer.CreateInitialState(round);

            DynamicActionResult blocked = DynamicRulesEngine.Apply(state, DynamicPlayerAction.ApplyOperation(0, DynamicOperation.Settle));
            DynamicActionResult store = DynamicRulesEngine.Apply(state, DynamicPlayerAction.StoreDream(0, 0));
            DynamicActionResult recall = DynamicRulesEngine.Apply(state, DynamicPlayerAction.RecallDream(0, 0));
            DynamicActionResult settle = DynamicRulesEngine.Apply(state, DynamicPlayerAction.ApplyOperation(0, DynamicOperation.Settle));

            Assert.That(blocked.Success, Is.False);
            Assert.That(blocked.Message, Does.Contain("temporarily blocked"));
            Assert.That(store.Success, Is.True, store.Message);
            Assert.That(state.Modifiers[0].IsResolved, Is.True);
            Assert.That(recall.Success, Is.True, recall.Message);
            Assert.That(settle.Success, Is.True, settle.Message);
        }

        [Test]
        public void Solver_ReplaysOperationSoftBlockRound()
        {
            DynamicRoundDefinition round = CreateSettleSubmitRound();
            round.Modifiers = new[] { DynamicBuiltInModifiers.OperationSoftBlock(DynamicOperation.Settle) };

            DynamicSolveResult solve = DynamicRoundSolver.Solve(round);
            DynamicSolveReplayResult replay = DynamicSolveReplayVerifier.Verify(round, solve);

            Assert.That(solve.Solvable, Is.True, $"HitLimit={solve.HitLimit} Visited={solve.VisitedStates}");
            Assert.That(replay.Success, Is.True, replay.FailureMessage);
            Assert.That(solve.FirstSolutionActions.Exists(action => action.Type == DynamicActionType.StoreDream), Is.True);
        }

        private static DynamicRoundDefinition CreateDreamRefreshRound()
        {
            DynamicRoundDefinition round = CreateSettleSubmitRound();
            round.RoundId = "test-dream-refresh";
            round.MoveLimit = 6;
            round.StreamConfig.DreamPreviewCount = 1;
            round.StreamConfig.MaxDreamDraws = 3;
            round.DreamBag = new[]
            {
                Dream(CleanCalm(DreamClarity.Blurry, DreamStability.Unsettled), 1),
                Dream(CleanCalm(DreamClarity.Vivid, DreamStability.Stable), 1),
                Dream(CleanAnxious(DreamClarity.Blurry, DreamStability.Stable), 1)
            };
            round.Modifiers = new[] { DynamicBuiltInModifiers.DreamRefresh() };
            return round;
        }

        private static DynamicRoundDefinition CreateSettleSubmitRound()
        {
            return new DynamicRoundDefinition
            {
                RoundId = "test-settle-submit",
                Seed = 101,
                MoveLimit = 8,
                TargetCompletedOrders = 1,
                DreamBag = new[]
                {
                    Dream(CleanCalm(DreamClarity.Blurry, DreamStability.Unsettled), 1)
                },
                OrderDeck = new[]
                {
                    Order(DynamicOrderRequirement.Stable(
                        1,
                        false,
                        DreamTaint.Clean,
                        true,
                        DreamMood.Calm,
                        true,
                        DreamClarity.Blurry), 1)
                },
                StreamConfig = new DynamicStreamConfig
                {
                    ActiveDreamSlots = 1,
                    ActiveOrderSlots = 1,
                    DreamPreviewCount = 0,
                    OrderPreviewCount = 0,
                    MaxDreamDraws = 3,
                    MaxOrderDraws = 1
                },
                StorageConfig = new DynamicStorageConfig
                {
                    StorageSlotCount = 1
                }
            };
        }

        private static DynamicDreamBagEntry Dream(DynamicDreamAttributes attributes, int count)
        {
            return new DynamicDreamBagEntry(attributes, count);
        }

        private static DynamicOrderDeckEntry Order(DynamicOrderRequirement requirement, int count)
        {
            return new DynamicOrderDeckEntry(requirement, count);
        }

        private static DynamicDreamAttributes CleanCalm(DreamClarity clarity, DreamStability stability)
        {
            return new DynamicDreamAttributes(DreamTaint.Clean, DreamMood.Calm, clarity, stability);
        }

        private static DynamicDreamAttributes CleanAnxious(DreamClarity clarity, DreamStability stability)
        {
            return new DynamicDreamAttributes(DreamTaint.Clean, DreamMood.Anxious, clarity, stability);
        }
    }
}
