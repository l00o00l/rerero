using NUnit.Framework;
using Thkim.DreamLaundromat.DynamicLab;
using Thkim.DreamLaundromat.Gameplay.ReleaseSlice;

namespace Thkim.DreamLaundromat.Tests.EditMode.ReleaseSlice
{
    public sealed class ReleaseLevelPackTests
    {
        [Test]
        public void DefaultPack_HasThirtyUniqueLevels()
        {
            ReleaseLevelPack pack = ReleaseLevelPack.CreateDefault();

            Assert.That(pack.Levels, Has.Count.EqualTo(30));
            for (int i = 0; i < pack.Levels.Count; i++)
            {
                Assert.That(pack.IndexOf(pack.Levels[i].LevelId), Is.EqualTo(i));
            }
        }

        [Test]
        public void DefaultPack_ValidatesAllLevels()
        {
            ReleaseLevelPackValidationResult result = ReleaseLevelPackValidator.Validate(ReleaseLevelPack.CreateDefault());

            Assert.That(result.IsValid, Is.True, ReleaseLevelPackReportFormatter.Format(result));
            Assert.That(result.Warnings, Is.Empty, ReleaseLevelPackReportFormatter.Format(result));
            Assert.That(result.DesignNotes, Is.Not.Empty);
            Assert.That(result.Entries, Has.Count.EqualTo(30));
            Assert.That(result.GuidedLevelCount, Is.GreaterThan(0));
            Assert.That(result.DifficultyBands.Count, Is.GreaterThanOrEqualTo(4));
        }

        [Test]
        public void DefaultPack_IncludesExpandedItemAndObstacleCoverage()
        {
            ReleaseLevelPack pack = ReleaseLevelPack.CreateDefault();
            bool hasPreviewSwap = false;
            bool hasLockedSlot = false;
            bool hasOrderPin = false;
            bool hasDreamRefresh = false;
            bool hasOperationSoftBlock = false;

            for (int i = 0; i < pack.Levels.Count; i++)
            {
                DynamicRoundDefinition round = pack.Levels[i].CreateRoundDefinition();
                for (int modifierIndex = 0; modifierIndex < round.Modifiers.Length; modifierIndex++)
                {
                    DynamicModifierEffect effect = round.Modifiers[modifierIndex].Effect;
                    hasPreviewSwap |= effect == DynamicModifierEffect.PreviewSwap;
                    hasLockedSlot |= effect == DynamicModifierEffect.LockActiveDreamSlot;
                    hasOrderPin |= effect == DynamicModifierEffect.PinOrderSlot;
                    hasDreamRefresh |= effect == DynamicModifierEffect.RefreshActiveDream;
                    hasOperationSoftBlock |= effect == DynamicModifierEffect.SoftBlockOperation;
                }
            }

            Assert.That(hasPreviewSwap, Is.True);
            Assert.That(hasLockedSlot, Is.True);
            Assert.That(hasOrderPin, Is.True);
            Assert.That(hasDreamRefresh, Is.True);
            Assert.That(hasOperationSoftBlock, Is.True);
        }

        [Test]
        public void GuidedTutorialRules_MatchSolverPrefix()
        {
            ReleaseLevelPack pack = ReleaseLevelPack.CreateDefault();

            for (int i = 0; i < pack.Levels.Count; i++)
            {
                ReleaseLevelDefinition level = pack.Levels[i];
                if (level.GuidedActionRules.Length == 0)
                {
                    continue;
                }

                DynamicSolveResult solve = DynamicRoundSolver.Solve(level.CreateRoundDefinition());
                Assert.That(solve.Solvable, Is.True, level.LevelId);
                for (int ruleIndex = 0; ruleIndex < level.GuidedActionRules.Length; ruleIndex++)
                {
                    Assert.That(level.GuidedActionRules[ruleIndex].Matches(solve.FirstSolutionActions[ruleIndex]), Is.True, level.LevelId);
                }
            }
        }

        [Test]
        public void BalanceReport_IncludesCoverageSummary()
        {
            string report = ReleaseBalanceReportBuilder.Build(ReleaseLevelPack.CreateDefault());

            Assert.That(report, Does.Contain("Levels=30"));
            Assert.That(report, Does.Contain("Warnings=0"));
            Assert.That(report, Does.Contain("DesignNotes="));
            Assert.That(report, Does.Contain("ModifierEffects="));
            Assert.That(report, Does.Contain("ModifierImpactLevels="));
            Assert.That(report, Does.Contain("Modifier Impact"));
            Assert.That(report, Does.Contain("Visual UX Review Checklist"));
            Assert.That(report, Does.Contain("Feedback Timing"));
            Assert.That(report, Does.Contain("AverageDifficulty="));
        }

        [Test]
        public void VisualDescriptors_DescribeEveryDreamStateAxis()
        {
            var attributes = new DynamicDreamAttributes(
                DreamTaint.Nightmare,
                DreamMood.Anxious,
                DreamClarity.Blurry,
                DreamStability.Unsettled);

            string description = ReleaseVisualDescriptors.Describe(attributes);

            Assert.That(description, Does.Contain("Taint:"));
            Assert.That(description, Does.Contain("Mood:"));
            Assert.That(description, Does.Contain("Clarity:"));
            Assert.That(description, Does.Contain("Stability:"));
            Assert.That(description, Does.Contain("NMR"));
            Assert.That(ReleaseVisualDescriptors.DescribeForCard(attributes), Does.Contain("\n"));
            Assert.That(ReleaseVisualDescriptors.ForOperation(DynamicOperation.Wash).ShortHint, Does.Contain("Taint"));
        }

        [Test]
        public void GameplayCardRenderer_BuildsSceneIndependentLabels()
        {
            DynamicRoundState state = DynamicRoundInitializer.CreateInitialState(
                ReleaseLevelPack.CreateDefault().GetLevel(0).CreateRoundDefinition());
            DynamicDreamSlot dreamSlot = FindFirstDreamSlot(state);

            string dreamLabel = ReleaseGameplayCardRenderer.BuildDreamCardLabel(state, dreamSlot);
            string statusLabel = ReleaseGameplayCardRenderer.BuildStatusMessage(state, "Ready.");
            DynamicDreamAttributes preview = ReleaseGameplayCardRenderer.PreviewOperation(
                new DynamicDreamAttributes(DreamTaint.Nightmare, DreamMood.Anxious, DreamClarity.Blurry, DreamStability.Stable),
                DynamicOperation.Wash);

            Assert.That(dreamLabel, Does.StartWith($"D{dreamSlot.SlotId}"));
            Assert.That(dreamLabel, Does.Not.Contain("Laundry tag"));
            Assert.That(statusLabel, Is.EqualTo("Ready."));
            Assert.That(preview.Taint, Is.EqualTo(DreamTaint.Clean));
        }

        [Test]
        public void ResultSummary_SeparatesClearAndFailCopy()
        {
            ReleaseLevelDefinition level = ReleaseLevelPack.CreateDefault().GetLevel(0);
            DynamicRoundState state = DynamicRoundInitializer.CreateInitialState(level.CreateRoundDefinition());
            state.Status = DynamicRoundStatus.Cleared;
            state.CompletedOrders = state.TargetCompletedOrders;

            ReleaseResultSummary clear = ReleaseResultSummary.Create(level, state, "Done.", hasNextLevel: true);
            state.Status = DynamicRoundStatus.Failed;
            state.FailureReason = DynamicFailureReason.NoMovesRemaining;
            ReleaseResultSummary fail = ReleaseResultSummary.Create(level, state, "Stopped.", hasNextLevel: true);

            Assert.That(clear.Title, Is.EqualTo("Clear Result"));
            Assert.That(clear.CanAdvance, Is.True);
            Assert.That(clear.Detail, Does.Contain("Orders complete"));
            Assert.That(fail.Title, Is.EqualTo("Fail Result"));
            Assert.That(fail.CanAdvance, Is.False);
            Assert.That(fail.Detail, Does.Contain("Reason: NoMovesRemaining"));
        }

        [Test]
        public void FeedbackTiming_DistinguishesClearMomentFromRepeatedActions()
        {
            ReleaseFeedbackTimingProfile action = ReleaseFeedbackTiming.ForEvent(ReleaseFeedbackEventType.ActionSucceeded);
            ReleaseFeedbackTimingProfile clear = ReleaseFeedbackTiming.ForEvent(ReleaseFeedbackEventType.LevelCleared);

            Assert.That(action.UsesHaptic, Is.False);
            Assert.That(clear.UsesHaptic, Is.True);
            Assert.That(clear.VisualPulseSeconds, Is.GreaterThan(action.VisualPulseSeconds));
            Assert.That(clear.AudioSeconds, Is.GreaterThan(action.AudioSeconds));
        }

        [Test]
        public void FeedbackPresenterProfile_DistinguishesInvalidDropFromSuccess()
        {
            ReleaseUiFeedbackProfile success = ReleaseFeedbackPresenter.ProfileFor(ReleaseUiFeedbackKind.ActionSuccess);
            ReleaseUiFeedbackProfile invalid = ReleaseFeedbackPresenter.ProfileFor(ReleaseUiFeedbackKind.InvalidTarget);

            Assert.That(success.ShakePixels, Is.EqualTo(0f));
            Assert.That(invalid.ShakePixels, Is.GreaterThan(0f));
            Assert.That(invalid.DurationSeconds, Is.GreaterThan(0f));
        }

        [Test]
        public void ModifierImpactAudit_DefaultPackReportsItemAndObstacleLevels()
        {
            ReleaseBalanceReportResult result = ReleaseBalanceReportBuilder.BuildResult(ReleaseLevelPack.CreateDefault());

            Assert.That(result.ModifierImpact, Is.Not.Null);
            Assert.That(result.ModifierImpact.Entries, Has.Count.GreaterThanOrEqualTo(5));
            Assert.That(result.ModifierImpact.ItemLevelCount, Is.GreaterThan(0));
            Assert.That(result.ModifierImpact.ObstacleLevelCount, Is.GreaterThan(0));
            Assert.That(result.ModifierImpact.Effects, Does.Contain(DynamicModifierEffect.PreviewSwap));
            Assert.That(result.ModifierImpact.Effects, Does.Contain(DynamicModifierEffect.RefreshActiveDream));
            Assert.That(result.ModifierImpact.Effects, Does.Contain(DynamicModifierEffect.PinOrderSlot));
            Assert.That(result.ModifierImpact.Effects, Does.Contain(DynamicModifierEffect.SoftBlockOperation));
        }

        [Test]
        public void ModifierImpactAudit_WarnsWhenItemIsOnlyDecorative()
        {
            var pack = new ReleaseLevelPack(new[]
            {
                CreateDecorativeItemLevel()
            });

            ReleaseModifierImpactReport report = ReleaseModifierImpactAudit.Audit(pack);

            Assert.That(report.Entries, Has.Count.EqualTo(1));
            Assert.That(report.Warnings, Has.Some.Contains("first solver solution does not use it"));
        }

        [Test]
        public void BalanceReport_InvalidPackIsFailingGate()
        {
            ReleaseBalanceReportResult result = ReleaseBalanceReportBuilder.BuildResult(
                new ReleaseLevelPack(new ReleaseLevelDefinition[0]));

            Assert.That(result.IsValid, Is.False);
            Assert.That(result.Report, Does.Contain("Release level pack is empty."));
        }

        [Test]
        public void AccessibilityAudit_DefaultStylePasses()
        {
            ReleaseAccessibilityAuditResult result = ReleaseAccessibilityAudit.AuditDefaultStyle(ReleaseLevelPack.CreateDefault());

            Assert.That(result.IsValid, Is.True, string.Join(" | ", result.Errors));
        }

        private static ReleaseLevelDefinition CreateDecorativeItemLevel()
        {
            return new ReleaseLevelDefinition(
                "DL-TEST-ITEM",
                "Decorative Item",
                "이미 맞는 꿈을 바로 제출할 수 있는 레벨에 장식 item을 붙인다.",
                "modifier impact audit이 의미 없는 item을 찾아내는지 검증한다.",
                "item 없이도 바로 풀 수 있는가?",
                "테스트 전용 레벨이다.",
                "test.decorative-item",
                1,
                () =>
                {
                    DynamicRoundDefinition round = DynamicSampleRounds.CreateStateAssignmentRound();
                    round.RoundId = "DL-TEST-ITEM";
                    round.Modifiers = new[]
                    {
                        DynamicBuiltInModifiers.PreviewSwap()
                    };
                    return round;
                },
                6,
                ReleaseDifficultyBand.Tutorial,
                new[] { "test", "item" });
        }

        private static DynamicDreamSlot FindFirstDreamSlot(DynamicRoundState state)
        {
            for (int i = 0; i < state.ActiveDreams.Count; i++)
            {
                if (!state.ActiveDreams[i].IsEmpty)
                {
                    return state.ActiveDreams[i];
                }
            }

            Assert.Fail("Expected at least one active dream slot.");
            return null;
        }
    }
}
