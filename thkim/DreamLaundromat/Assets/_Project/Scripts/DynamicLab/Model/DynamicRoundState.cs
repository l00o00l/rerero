using System;
using System.Collections.Generic;

namespace Thkim.DreamLaundromat.DynamicLab
{
    public sealed class DynamicRoundState
    {
        public string RoundId = string.Empty;
        public int RemainingMoves;
        public int TargetCompletedOrders;
        public int CompletedOrders;
        public DynamicRoundStatus Status = DynamicRoundStatus.Ready;
        public DynamicFailureReason FailureReason = DynamicFailureReason.None;
        public DynamicOperation[] ActionSet = Array.Empty<DynamicOperation>();
        public DynamicStreamConfig StreamConfig = new DynamicStreamConfig();
        public DynamicModifierDefinition[] ModifierDefinitions = Array.Empty<DynamicModifierDefinition>();

        public List<DynamicDreamSlot> ActiveDreams = new List<DynamicDreamSlot>();
        public List<DynamicDreamState> DreamPreview = new List<DynamicDreamState>();
        public List<DynamicDreamState> DreamDrawPile = new List<DynamicDreamState>();
        public int NextDreamIndex;

        public List<DynamicOrderSlot> ActiveOrders = new List<DynamicOrderSlot>();
        public List<DynamicOrderState> OrderPreview = new List<DynamicOrderState>();
        public List<DynamicOrderState> OrderDrawPile = new List<DynamicOrderState>();
        public int NextOrderIndex;

        public List<DynamicStorageSlot> StorageSlots = new List<DynamicStorageSlot>();
        public List<DynamicModifierState> Modifiers = new List<DynamicModifierState>();

        public DynamicDreamSlot FindActiveDreamSlot(int slotId)
        {
            return ActiveDreams.Find(slot => slot.SlotId == slotId);
        }

        public DynamicOrderSlot FindActiveOrderSlot(int slotId)
        {
            return ActiveOrders.Find(slot => slot.SlotId == slotId);
        }

        public DynamicStorageSlot FindStorageSlot(int slotId)
        {
            return StorageSlots.Find(slot => slot.SlotId == slotId);
        }

        public bool IsOperationAllowed(DynamicOperation operation)
        {
            if (ActionSet == null || ActionSet.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < ActionSet.Length; i++)
            {
                if (ActionSet[i] == operation)
                {
                    return true;
                }
            }

            return false;
        }

        public DynamicRoundState Clone()
        {
            var clone = new DynamicRoundState
            {
                RoundId = RoundId,
                RemainingMoves = RemainingMoves,
                TargetCompletedOrders = TargetCompletedOrders,
                CompletedOrders = CompletedOrders,
                Status = Status,
                FailureReason = FailureReason,
                ActionSet = (DynamicOperation[])ActionSet.Clone(),
                ModifierDefinitions = CloneModifierDefinitions(ModifierDefinitions),
                StreamConfig = new DynamicStreamConfig
                {
                    ActiveDreamSlots = StreamConfig.ActiveDreamSlots,
                    ActiveOrderSlots = StreamConfig.ActiveOrderSlots,
                    DreamPreviewCount = StreamConfig.DreamPreviewCount,
                    OrderPreviewCount = StreamConfig.OrderPreviewCount,
                    MaxDreamDraws = StreamConfig.MaxDreamDraws,
                    MaxOrderDraws = StreamConfig.MaxOrderDraws
                },
                NextDreamIndex = NextDreamIndex,
                NextOrderIndex = NextOrderIndex
            };

            for (int i = 0; i < ActiveDreams.Count; i++)
            {
                clone.ActiveDreams.Add(ActiveDreams[i].Clone());
            }

            for (int i = 0; i < DreamPreview.Count; i++)
            {
                clone.DreamPreview.Add(DreamPreview[i].Clone());
            }

            for (int i = 0; i < DreamDrawPile.Count; i++)
            {
                clone.DreamDrawPile.Add(DreamDrawPile[i].Clone());
            }

            for (int i = 0; i < ActiveOrders.Count; i++)
            {
                clone.ActiveOrders.Add(ActiveOrders[i].Clone());
            }

            for (int i = 0; i < OrderPreview.Count; i++)
            {
                clone.OrderPreview.Add(OrderPreview[i].Clone());
            }

            for (int i = 0; i < OrderDrawPile.Count; i++)
            {
                clone.OrderDrawPile.Add(OrderDrawPile[i].Clone());
            }

            for (int i = 0; i < StorageSlots.Count; i++)
            {
                clone.StorageSlots.Add(StorageSlots[i].Clone());
            }

            for (int i = 0; i < Modifiers.Count; i++)
            {
                clone.Modifiers.Add(Modifiers[i].Clone());
            }

            return clone;
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
    }
}
