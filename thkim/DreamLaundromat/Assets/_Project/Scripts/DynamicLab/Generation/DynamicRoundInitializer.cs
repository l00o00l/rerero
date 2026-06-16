using System;
using System.Collections.Generic;

namespace Thkim.DreamLaundromat.DynamicLab
{
    public static class DynamicRoundInitializer
    {
        public static DynamicRoundState CreateInitialState(DynamicRoundDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            var state = new DynamicRoundState
            {
                RoundId = definition.RoundId,
                RemainingMoves = definition.MoveLimit,
                TargetCompletedOrders = definition.TargetCompletedOrders,
                Status = DynamicRoundStatus.Playing,
                FailureReason = DynamicFailureReason.None,
                ActionSet = (DynamicOperation[])definition.ActionSet.Clone(),
                ModifierDefinitions = CloneModifierDefinitions(definition.Modifiers),
                StreamConfig = CloneStreamConfig(definition.StreamConfig)
            };

            state.DreamDrawPile.AddRange(MaterializeDreams(definition.DreamBag));
            state.OrderDrawPile.AddRange(MaterializeOrders(definition.OrderDeck));
            SeededShuffler.Shuffle(state.DreamDrawPile, definition.Seed);
            SeededShuffler.Shuffle(state.OrderDrawPile, definition.Seed ^ unchecked((int)0x5F3759DF));

            for (int i = 0; i < definition.StreamConfig.ActiveDreamSlots; i++)
            {
                state.ActiveDreams.Add(new DynamicDreamSlot { SlotId = i });
            }

            for (int i = 0; i < definition.StreamConfig.ActiveOrderSlots; i++)
            {
                state.ActiveOrders.Add(new DynamicOrderSlot { SlotId = i });
            }

            for (int i = 0; i < definition.StorageConfig.StorageSlotCount; i++)
            {
                state.StorageSlots.Add(new DynamicStorageSlot { SlotId = i });
            }

            DynamicRoundStreams.FillDreamPreview(state);
            DynamicRoundStreams.FillActiveDreamSlots(state);
            DynamicRoundStreams.FillOrderPreview(state);
            DynamicRoundStreams.FillActiveOrderSlots(state);
            DynamicModifierPipeline.InitializeStates(definition, state);

            return state;
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

        private static DynamicModifierDefinition[] CloneModifierDefinitions(DynamicModifierDefinition[] source)
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

        private static List<DynamicDreamState> MaterializeDreams(DynamicDreamBagEntry[] entries)
        {
            var dreams = new List<DynamicDreamState>();
            int nextId = 1;

            for (int i = 0; i < entries.Length; i++)
            {
                int count = Math.Max(0, entries[i].Count);
                for (int copy = 0; copy < count; copy++)
                {
                    string id = $"dream-{nextId:000}";
                    dreams.Add(new DynamicDreamState
                    {
                        Id = id,
                        DisplayName = id,
                        Attributes = entries[i].Attributes
                    });
                    nextId++;
                }
            }

            return dreams;
        }

        private static List<DynamicOrderState> MaterializeOrders(DynamicOrderDeckEntry[] entries)
        {
            var orders = new List<DynamicOrderState>();
            int nextId = 1;

            for (int i = 0; i < entries.Length; i++)
            {
                int count = Math.Max(0, entries[i].Count);
                for (int copy = 0; copy < count; copy++)
                {
                    string id = $"order-{nextId:000}";
                    orders.Add(new DynamicOrderState
                    {
                        Id = id,
                        DisplayName = id,
                        Requirement = entries[i].Requirement
                    });
                    nextId++;
                }
            }

            return orders;
        }
    }
}
