using NUnit.Framework;
using Thkim.DreamLaundromat.DynamicLab;

namespace Thkim.DreamLaundromat.Tests.EditMode.DynamicLab
{
    public sealed class DynamicLabModelTests
    {
        [Test]
        public void Attributes_CopyEqualityAndHashAreStable()
        {
            var attributes = new DynamicDreamAttributes(
                DreamTaint.Nightmare,
                DreamMood.Anxious,
                DreamClarity.Blurry,
                DreamStability.Unsettled);

            DynamicDreamAttributes copy = attributes.WithMood(DreamMood.Anxious);
            DynamicDreamAttributes changed = attributes.WithMood(DreamMood.Calm);

            Assert.That(copy, Is.EqualTo(attributes));
            Assert.That(copy.GetHashCode(), Is.EqualTo(attributes.GetHashCode()));
            Assert.That(changed, Is.Not.EqualTo(attributes));
        }

        [Test]
        public void Operations_TransformDreamDeterministically()
        {
            DynamicRoundState state = DynamicRoundInitializer.CreateInitialState(CreateSingleDreamRound(
                new DynamicDreamAttributes(
                    DreamTaint.Nightmare,
                    DreamMood.Anxious,
                    DreamClarity.Blurry,
                    DreamStability.Unsettled),
                DynamicOrderRequirement.Stable(
                    1,
                    true,
                    DreamTaint.Clean,
                    true,
                    DreamMood.Calm,
                    true,
                    DreamClarity.Vivid)));

            Assert.That(DynamicRulesEngine.Apply(state, DynamicPlayerAction.ApplyOperation(0, DynamicOperation.Wash)).Success, Is.True);
            Assert.That(DynamicRulesEngine.Apply(state, DynamicPlayerAction.ApplyOperation(0, DynamicOperation.Soothe)).Success, Is.True);
            Assert.That(DynamicRulesEngine.Apply(state, DynamicPlayerAction.ApplyOperation(0, DynamicOperation.Clarify)).Success, Is.True);
            Assert.That(DynamicRulesEngine.Apply(state, DynamicPlayerAction.ApplyOperation(0, DynamicOperation.Settle)).Success, Is.True);

            DynamicDreamAttributes attributes = state.FindActiveDreamSlot(0).Dream.Attributes;
            Assert.That(attributes, Is.EqualTo(new DynamicDreamAttributes(
                DreamTaint.Clean,
                DreamMood.Calm,
                DreamClarity.Vivid,
                DreamStability.Stable)));
        }

        [Test]
        public void InvalidOperation_DoesNotSpendMove()
        {
            DynamicRoundState state = DynamicRoundInitializer.CreateInitialState(CreateSingleDreamRound(
                new DynamicDreamAttributes(
                    DreamTaint.Clean,
                    DreamMood.Calm,
                    DreamClarity.Vivid,
                    DreamStability.Stable),
                DynamicOrderRequirement.Stable(
                    1,
                    true,
                    DreamTaint.Clean,
                    true,
                    DreamMood.Calm,
                    true,
                    DreamClarity.Vivid)));

            DynamicActionResult result = DynamicRulesEngine.Apply(
                state,
                DynamicPlayerAction.ApplyOperation(0, DynamicOperation.Wash));

            Assert.That(result.Success, Is.False);
            Assert.That(state.RemainingMoves, Is.EqualTo(8));
            Assert.That(state.Status, Is.EqualTo(DynamicRoundStatus.Playing));
        }

        [Test]
        public void SameSeed_ProducesSameActiveAndPreviewStreams()
        {
            DynamicRoundDefinition round = DynamicSampleRounds.CreateStreamTimingRound();

            DynamicRoundState first = DynamicRoundInitializer.CreateInitialState(round);
            DynamicRoundState second = DynamicRoundInitializer.CreateInitialState(round);

            Assert.That(second.ActiveDreams[0].Dream.Id, Is.EqualTo(first.ActiveDreams[0].Dream.Id));
            Assert.That(second.ActiveDreams[0].Dream.Attributes, Is.EqualTo(first.ActiveDreams[0].Dream.Attributes));
            Assert.That(second.DreamPreview[0].Id, Is.EqualTo(first.DreamPreview[0].Id));
            Assert.That(second.ActiveOrders[0].Order.Id, Is.EqualTo(first.ActiveOrders[0].Order.Id));
            Assert.That(second.OrderPreview[0].Id, Is.EqualTo(first.OrderPreview[0].Id));
        }

        [Test]
        public void Submit_RequiresStableDreamAndClearsRound()
        {
            DynamicRoundState state = DynamicRoundInitializer.CreateInitialState(CreateSingleDreamRound(
                new DynamicDreamAttributes(
                    DreamTaint.Clean,
                    DreamMood.Calm,
                    DreamClarity.Blurry,
                    DreamStability.Unsettled),
                DynamicOrderRequirement.Stable(
                    1,
                    true,
                    DreamTaint.Clean,
                    true,
                    DreamMood.Calm,
                    false,
                    DreamClarity.Blurry)));

            DynamicActionResult failedSubmit = DynamicRulesEngine.Apply(
                state,
                DynamicPlayerAction.SubmitDream(0, 0));
            Assert.That(failedSubmit.Success, Is.False);
            Assert.That(state.RemainingMoves, Is.EqualTo(8));

            Assert.That(DynamicRulesEngine.Apply(state, DynamicPlayerAction.ApplyOperation(0, DynamicOperation.Settle)).Success, Is.True);
            DynamicActionResult submitted = DynamicRulesEngine.Apply(state, DynamicPlayerAction.SubmitDream(0, 0));

            Assert.That(submitted.Success, Is.True);
            Assert.That(state.CompletedOrders, Is.EqualTo(1));
            Assert.That(state.Status, Is.EqualTo(DynamicRoundStatus.Cleared));
        }

        [Test]
        public void StoreAndRecall_MoveDreamBetweenActiveAndStorageSlots()
        {
            DynamicRoundDefinition round = CreateSingleDreamRound(
                new DynamicDreamAttributes(
                    DreamTaint.Clean,
                    DreamMood.Calm,
                    DreamClarity.Blurry,
                    DreamStability.Stable),
                DynamicOrderRequirement.Stable(
                    1,
                    true,
                    DreamTaint.Clean,
                    true,
                    DreamMood.Calm,
                    false,
                    DreamClarity.Blurry));
            round.StreamConfig.ActiveDreamSlots = 2;
            round.StreamConfig.DreamPreviewCount = 0;
            round.StorageConfig.StorageSlotCount = 1;

            DynamicRoundState state = DynamicRoundInitializer.CreateInitialState(round);
            string dreamId = state.ActiveDreams[0].Dream.Id;

            Assert.That(DynamicRulesEngine.Apply(state, DynamicPlayerAction.StoreDream(0, 0)).Success, Is.True);
            Assert.That(state.ActiveDreams[0].IsEmpty, Is.True);
            Assert.That(state.StorageSlots[0].Dream.Id, Is.EqualTo(dreamId));

            Assert.That(DynamicRulesEngine.Apply(state, DynamicPlayerAction.RecallDream(0, 1)).Success, Is.True);
            Assert.That(state.StorageSlots[0].IsEmpty, Is.True);
            Assert.That(state.ActiveDreams[1].Dream.Id, Is.EqualTo(dreamId));
        }

        [Test]
        public void StateClone_IsolatesNestedDreamMutation()
        {
            DynamicRoundState state = DynamicRoundInitializer.CreateInitialState(DynamicSampleRounds.CreateStateAssignmentRound());
            DynamicRoundState clone = state.Clone();
            DynamicDreamAttributes original = state.ActiveDreams[0].Dream.Attributes;
            DreamMood changedMood = original.Mood == DreamMood.Calm ? DreamMood.Anxious : DreamMood.Calm;

            clone.ActiveDreams[0].Dream.Attributes = clone.ActiveDreams[0].Dream.Attributes.WithMood(changedMood);

            Assert.That(clone.ActiveDreams[0].Dream.Attributes, Is.Not.EqualTo(state.ActiveDreams[0].Dream.Attributes));
            Assert.That(state.ActiveDreams[0].Dream.Attributes, Is.EqualTo(original));
        }

        [Test]
        public void SampleRounds_InitializeWithActiveDreamsAndOrders()
        {
            DynamicRoundDefinition[] rounds = DynamicSampleRounds.CreateAll();

            Assert.That(rounds, Has.Length.EqualTo(10));
            for (int i = 0; i < rounds.Length; i++)
            {
                DynamicRoundState state = DynamicRoundInitializer.CreateInitialState(rounds[i]);

                Assert.That(state.Status, Is.EqualTo(DynamicRoundStatus.Playing));
                Assert.That(state.ActiveDreams.Exists(slot => !slot.IsEmpty), Is.True, rounds[i].RoundId);
                Assert.That(state.ActiveOrders.Exists(slot => !slot.IsEmpty), Is.True, rounds[i].RoundId);
                for (int orderIndex = 0; orderIndex < rounds[i].OrderDeck.Length; orderIndex++)
                {
                    Assert.That(rounds[i].OrderDeck[orderIndex].Requirement.RequiresUnsettled(), Is.False, rounds[i].RoundId);
                }
            }
        }

        private static DynamicRoundDefinition CreateSingleDreamRound(
            DynamicDreamAttributes dream,
            DynamicOrderRequirement order)
        {
            return new DynamicRoundDefinition
            {
                RoundId = "test-round",
                Seed = 1234,
                MoveLimit = 8,
                TargetCompletedOrders = 1,
                DreamBag = new[]
                {
                    new DynamicDreamBagEntry(dream, 1)
                },
                OrderDeck = new[]
                {
                    new DynamicOrderDeckEntry(order, 1)
                },
                StreamConfig = new DynamicStreamConfig
                {
                    ActiveDreamSlots = 1,
                    ActiveOrderSlots = 1,
                    DreamPreviewCount = 0,
                    OrderPreviewCount = 0,
                    MaxDreamDraws = 1,
                    MaxOrderDraws = 1
                },
                StorageConfig = new DynamicStorageConfig
                {
                    StorageSlotCount = 1
                }
            };
        }
    }
}
