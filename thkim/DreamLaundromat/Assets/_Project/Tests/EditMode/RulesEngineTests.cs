using NUnit.Framework;
using Thkim.DreamLaundromat.Levels;
using Thkim.DreamLaundromat.Rules;
using UnityEngine;

namespace Thkim.DreamLaundromat.Tests.EditMode
{
    public sealed class RulesEngineTests
    {
        [Test]
        public void OrderRequirement_MatchesOnlySpecifiedAttributes()
        {
            var requirement = new OrderRequirement(1, true, DreamStain.None, false, DreamMoisture.Dry);

            Assert.That(requirement.Matches(new DreamAttributes(DreamStain.None, DreamMoisture.Wet)), Is.True);
            Assert.That(requirement.Matches(new DreamAttributes(DreamStain.Nightmare, DreamMoisture.Wet)), Is.False);
        }

        [Test]
        public void Washer_RemovesNightmareAndMakesWet()
        {
            LevelSession session = new LevelSession(CreateSingleDreamLevel(
                new DreamAttributes(DreamStain.Nightmare, DreamMoisture.Dry),
                new[] { Machine("washer", MachineType.Washer) },
                Order("order-a", new OrderRequirement(1, true, DreamStain.None, false, DreamMoisture.Dry))));

            ActionResult result = session.Apply(PlayerAction.MoveToMachine("dream-a", "washer"));

            Assert.That(result.Success, Is.True);
            DreamRuntimeState dream = session.State.FindDream("dream-a");
            Assert.That(dream.Attributes, Is.EqualTo(new DreamAttributes(DreamStain.None, DreamMoisture.Wet)));
            Assert.That(dream.Location, Is.EqualTo(DreamLocation.Machine("washer")));
        }

        [Test]
        public void Dryer_RequiresWetDream()
        {
            LevelSession session = new LevelSession(CreateSingleDreamLevel(
                new DreamAttributes(DreamStain.None, DreamMoisture.Dry),
                new[] { Machine("dryer", MachineType.Dryer) },
                Order("order-a", new OrderRequirement(1, true, DreamStain.None, true, DreamMoisture.Dry))));

            ActionResult result = session.Apply(PlayerAction.MoveToMachine("dream-a", "dryer"));

            Assert.That(result.Success, Is.False);
            Assert.That(session.State.RemainingMoves, Is.EqualTo(8));
        }

        [Test]
        public void BasketCapacity_BlocksOverflow()
        {
            LevelDefinition level = CreateLevel(
                "capacity-test",
                8,
                new[]
                {
                    Dream("dream-a", new DreamAttributes(DreamStain.None, DreamMoisture.Dry)),
                    Dream("dream-b", new DreamAttributes(DreamStain.None, DreamMoisture.Dry))
                },
                System.Array.Empty<MachineDefinition>(),
                new[] { new BasketDefinition { Id = "basket-a", DisplayName = "Basket A", Capacity = 1 } },
                new[] { Order("order-a", new OrderRequirement(2, true, DreamStain.None, true, DreamMoisture.Dry)) });
            LevelSession session = new LevelSession(level);

            Assert.That(session.Apply(PlayerAction.MoveToBasket("dream-a", "basket-a")).Success, Is.True);
            Assert.That(session.Apply(PlayerAction.MoveToBasket("dream-b", "basket-a")).Success, Is.False);
        }

        [Test]
        public void Undo_RestoresPreviousState()
        {
            LevelSession session = new LevelSession(CreateSingleDreamLevel(
                new DreamAttributes(DreamStain.Nightmare, DreamMoisture.Dry),
                new[] { Machine("washer", MachineType.Washer) },
                Order("order-a", new OrderRequirement(1, true, DreamStain.None, false, DreamMoisture.Dry))));

            session.Apply(PlayerAction.MoveToMachine("dream-a", "washer"));
            Assert.That(session.Undo(), Is.True);

            DreamRuntimeState dream = session.State.FindDream("dream-a");
            Assert.That(dream.Attributes, Is.EqualTo(new DreamAttributes(DreamStain.Nightmare, DreamMoisture.Dry)));
            Assert.That(dream.Location, Is.EqualTo(DreamLocation.Queue()));
            Assert.That(session.State.RemainingMoves, Is.EqualTo(8));
        }

        [Test]
        public void Submit_CompletesLevelWhenOrderIsSatisfied()
        {
            LevelSession session = new LevelSession(CreateSingleDreamLevel(
                new DreamAttributes(DreamStain.None, DreamMoisture.Dry),
                System.Array.Empty<MachineDefinition>(),
                Order("order-a", new OrderRequirement(1, true, DreamStain.None, true, DreamMoisture.Dry))));

            ActionResult result = session.Apply(PlayerAction.Submit("dream-a", "order-a"));

            Assert.That(result.Success, Is.True);
            Assert.That(session.State.Status, Is.EqualTo(LevelStatus.Cleared));
        }

        private static LevelDefinition CreateSingleDreamLevel(
            DreamAttributes attributes,
            MachineDefinition[] machines,
            OrderDefinition order)
        {
            return CreateLevel(
                "test-level",
                8,
                new[] { Dream("dream-a", attributes) },
                machines,
                new[] { new BasketDefinition { Id = "basket-a", DisplayName = "Basket A", Capacity = 2 } },
                new[] { order });
        }

        private static LevelDefinition CreateLevel(
            string id,
            int moveLimit,
            DreamDefinition[] dreams,
            MachineDefinition[] machines,
            BasketDefinition[] baskets,
            OrderDefinition[] orders)
        {
            LevelDefinition level = ScriptableObject.CreateInstance<LevelDefinition>();
            level.Configure(id, moveLimit, dreams, machines, baskets, orders, string.Empty);
            return level;
        }

        private static DreamDefinition Dream(string id, DreamAttributes attributes)
        {
            return new DreamDefinition
            {
                Id = id,
                DisplayName = id,
                InitialAttributes = attributes,
                CapacityCost = 1
            };
        }

        private static MachineDefinition Machine(string id, MachineType type)
        {
            return new MachineDefinition
            {
                Id = id,
                DisplayName = id,
                Type = type,
                Capacity = 1
            };
        }

        private static OrderDefinition Order(string id, params OrderRequirement[] requirements)
        {
            return new OrderDefinition
            {
                Id = id,
                DisplayName = id,
                Requirements = requirements
            };
        }
    }
}
