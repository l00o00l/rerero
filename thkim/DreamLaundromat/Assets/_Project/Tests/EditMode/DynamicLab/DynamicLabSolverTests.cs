using NUnit.Framework;
using Thkim.DreamLaundromat.DynamicLab;

namespace Thkim.DreamLaundromat.Tests.EditMode.DynamicLab
{
    public sealed class DynamicLabSolverTests
    {
        [Test]
        public void HardValidator_RejectsEmptyDreamBag()
        {
            DynamicRoundDefinition round = CreateRound(
                System.Array.Empty<DynamicDreamBagEntry>(),
                new[] { Order(CalmStableOrder(), 1) },
                targetCompletedOrders: 1);

            DynamicValidationResult result = DynamicRoundHardValidator.Validate(round);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors.Exists(error => error.Contains("Dream bag")), Is.True);
        }

        [Test]
        public void HardValidator_RejectsUnsettledSubmitOrder()
        {
            var unstableOrder = new DynamicOrderRequirement(
                1,
                false,
                DreamTaint.Clean,
                false,
                DreamMood.Calm,
                false,
                DreamClarity.Blurry,
                true,
                DreamStability.Unsettled);
            DynamicRoundDefinition round = CreateRound(
                new[] { Dream(CleanCalm(DreamClarity.Blurry, DreamStability.Unsettled), 1) },
                new[] { Order(unstableOrder, 1) },
                targetCompletedOrders: 1);

            DynamicValidationResult result = DynamicRoundHardValidator.Validate(round);

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Errors.Exists(error => error.Contains("Unsettled")), Is.True);
        }

        [Test]
        public void Solver_FindsMinimumSettleThenSubmitSolution()
        {
            DynamicRoundDefinition round = CreateRound(
                new[] { Dream(CleanCalm(DreamClarity.Blurry, DreamStability.Unsettled), 1) },
                new[] { Order(CalmStableOrder(), 1) },
                targetCompletedOrders: 1);

            DynamicSolveResult result = DynamicRoundSolver.Solve(round);

            Assert.That(result.Solvable, Is.True);
            Assert.That(result.MinMoves, Is.EqualTo(2));
            Assert.That(result.FirstSolutionActions[0].Type, Is.EqualTo(DynamicActionType.ApplyOperation));
            Assert.That(result.FirstSolutionActions[0].Operation, Is.EqualTo(DynamicOperation.Settle));
            Assert.That(result.FirstSolutionActions[1].Type, Is.EqualTo(DynamicActionType.SubmitDream));
        }

        [Test]
        public void ReplayVerifier_ReplaysSolverSolutionToClear()
        {
            DynamicRoundDefinition round = CreateRound(
                new[] { Dream(CleanCalm(DreamClarity.Blurry, DreamStability.Unsettled), 1) },
                new[] { Order(CalmStableOrder(), 1) },
                targetCompletedOrders: 1);
            DynamicSolveResult solve = DynamicRoundSolver.Solve(round);

            DynamicSolveReplayResult replay = DynamicSolveReplayVerifier.Verify(round, solve);

            Assert.That(replay.Success, Is.True, replay.FailureMessage);
            Assert.That(replay.AppliedActionCount, Is.EqualTo(solve.FirstSolutionActions.Count));
            Assert.That(replay.FinalState.Status, Is.EqualTo(DynamicRoundStatus.Cleared));
        }

        [Test]
        public void Solver_ReturnsUnsolvableWhenRequiredOperationIsDisabled()
        {
            DynamicRoundDefinition round = CreateRound(
                new[] { Dream(CleanCalm(DreamClarity.Blurry, DreamStability.Stable), 1) },
                new[] { Order(VividStableOrder(), 1) },
                targetCompletedOrders: 1);
            round.ActionSet = new[] { DynamicOperation.Settle };

            DynamicSolveResult result = DynamicRoundSolver.Solve(round);

            Assert.That(result.Solvable, Is.False);
            Assert.That(result.HitLimit, Is.False);
        }

        [Test]
        public void Solver_ReportsLimitWhenVisitedBudgetIsExhausted()
        {
            DynamicRoundDefinition round = CreateRound(
                new[] { Dream(CleanCalm(DreamClarity.Blurry, DreamStability.Unsettled), 1) },
                new[] { Order(CalmStableOrder(), 1) },
                targetCompletedOrders: 1);

            DynamicSolveResult result = DynamicRoundSolver.Solve(round, new DynamicSolveOptions
            {
                MaxVisitedStates = 0,
                TimeoutMilliseconds = 1000
            });

            Assert.That(result.HitLimit, Is.True);
            Assert.That(result.Solvable, Is.False);
        }

        [Test]
        public void Metrics_SummarizeFirstSolution()
        {
            DynamicRoundDefinition round = CreateRound(
                new[] { Dream(CleanCalm(DreamClarity.Blurry, DreamStability.Unsettled), 1) },
                new[] { Order(CalmStableOrder(), 1) },
                targetCompletedOrders: 1);
            DynamicSolveResult solve = DynamicRoundSolver.Solve(round);

            DynamicRoundMetrics metrics = DynamicRoundMetricCalculator.Calculate(round, solve);

            Assert.That(metrics.MinMoves, Is.EqualTo(2));
            Assert.That(metrics.MoveSlack, Is.EqualTo(round.MoveLimit - 2));
            Assert.That(metrics.ConversionCount, Is.EqualTo(1));
            Assert.That(metrics.OperationDiversity, Is.EqualTo(1));
            Assert.That(metrics.SubmitCount, Is.EqualTo(1));
            Assert.That(metrics.ActionTypeDiversity, Is.EqualTo(2));
            Assert.That(metrics.SettleCount, Is.EqualTo(1));
        }

        [Test]
        public void DesignValidator_WarnsWhenRoundIsMostlyDirectSubmits()
        {
            DynamicRoundDefinition round = CreateRound(
                new[]
                {
                    Dream(CleanCalm(DreamClarity.Blurry, DreamStability.Stable), 3)
                },
                new[]
                {
                    Order(CalmStableOrder(), 3)
                },
                targetCompletedOrders: 3);
            round.MoveLimit = 3;
            round.StreamConfig.ActiveDreamSlots = 3;
            round.StreamConfig.ActiveOrderSlots = 3;

            DynamicSolveResult solve = DynamicRoundSolver.Solve(round);
            DynamicRoundMetrics metrics = DynamicRoundMetricCalculator.Calculate(round, solve);
            DynamicValidationResult design = DynamicRoundDesignValidator.Validate(round, solve, metrics);

            Assert.That(solve.Solvable, Is.True);
            Assert.That(design.Warnings.Exists(warning => warning.Contains("direct submit")), Is.True);
        }

        [Test]
        public void DesignValidator_WarnsWhenSettleBecomesRepeatedTax()
        {
            DynamicRoundDefinition round = CreateRound(
                new[]
                {
                    Dream(CleanCalm(DreamClarity.Blurry, DreamStability.Unsettled), 3)
                },
                new[]
                {
                    Order(CalmStableOrder(), 3)
                },
                targetCompletedOrders: 3);
            round.MoveLimit = 6;
            round.StreamConfig.ActiveDreamSlots = 3;
            round.StreamConfig.ActiveOrderSlots = 3;

            DynamicSolveResult solve = DynamicRoundSolver.Solve(round);
            DynamicRoundMetrics metrics = DynamicRoundMetricCalculator.Calculate(round, solve);
            DynamicValidationResult design = DynamicRoundDesignValidator.Validate(round, solve, metrics);

            Assert.That(solve.Solvable, Is.True);
            Assert.That(design.Warnings.Exists(warning => warning.Contains("Settle acts")), Is.True);
        }

        private static DynamicRoundDefinition CreateRound(
            DynamicDreamBagEntry[] dreams,
            DynamicOrderDeckEntry[] orders,
            int targetCompletedOrders)
        {
            return new DynamicRoundDefinition
            {
                RoundId = "solver-test-round",
                Seed = 4321,
                MoveLimit = 8,
                TargetCompletedOrders = targetCompletedOrders,
                DreamBag = dreams,
                OrderDeck = orders,
                StreamConfig = new DynamicStreamConfig
                {
                    ActiveDreamSlots = 1,
                    ActiveOrderSlots = 1,
                    DreamPreviewCount = 0,
                    OrderPreviewCount = 0,
                    MaxDreamDraws = 8,
                    MaxOrderDraws = 8
                },
                StorageConfig = new DynamicStorageConfig
                {
                    StorageSlotCount = 0
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

        private static DynamicOrderRequirement CalmStableOrder()
        {
            return DynamicOrderRequirement.Stable(
                1,
                false,
                DreamTaint.Clean,
                true,
                DreamMood.Calm,
                false,
                DreamClarity.Blurry);
        }

        private static DynamicOrderRequirement VividStableOrder()
        {
            return DynamicOrderRequirement.Stable(
                1,
                false,
                DreamTaint.Clean,
                false,
                DreamMood.Calm,
                true,
                DreamClarity.Vivid);
        }

        private static DynamicDreamAttributes CleanCalm(DreamClarity clarity, DreamStability stability)
        {
            return new DynamicDreamAttributes(DreamTaint.Clean, DreamMood.Calm, clarity, stability);
        }
    }
}
