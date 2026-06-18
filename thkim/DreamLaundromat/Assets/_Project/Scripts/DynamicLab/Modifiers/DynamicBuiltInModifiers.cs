namespace Thkim.DreamLaundromat.DynamicLab
{
    public static class DynamicBuiltInModifiers
    {
        public const string PreviewSwapId = "item.preview-swap";
        public const string DreamRefreshId = "item.dream-refresh";
        public const string LockedSlotIdPrefix = "obstacle.locked-slot";
        public const string OrderPinIdPrefix = "obstacle.order-pin";
        public const string OperationSoftBlockIdPrefix = "obstacle.operation-soft-block";

        public static DynamicModifierDefinition PreviewSwap(int charges = 1, bool requiresItem = false)
        {
            return new DynamicModifierDefinition
            {
                Id = PreviewSwapId,
                DisplayName = "Preview Swap",
                Type = DynamicModifierType.Item,
                Trigger = DynamicModifierTrigger.Manual,
                Scope = DynamicModifierScope.Preview,
                Effect = DynamicModifierEffect.PreviewSwap,
                Charges = charges,
                RequiresItem = requiresItem,
                TargetKind = DynamicModifierTargetKind.None,
                TargetId = -1
            };
        }

        public static DynamicModifierDefinition LockedActiveDreamSlot(int activeDreamSlotId)
        {
            return new DynamicModifierDefinition
            {
                Id = $"{LockedSlotIdPrefix}-{activeDreamSlotId}",
                DisplayName = $"Locked Slot {activeDreamSlotId}",
                Type = DynamicModifierType.Obstacle,
                Trigger = DynamicModifierTrigger.CanApplyAction,
                Scope = DynamicModifierScope.Slot,
                Effect = DynamicModifierEffect.LockActiveDreamSlot,
                Charges = 1,
                ConsumesMove = false,
                TargetKind = DynamicModifierTargetKind.ActiveDreamSlot,
                TargetId = activeDreamSlotId
            };
        }

        public static DynamicModifierDefinition OrderPin(int activeOrderSlotId, int turns = 1)
        {
            return new DynamicModifierDefinition
            {
                Id = $"{OrderPinIdPrefix}-{activeOrderSlotId}",
                DisplayName = $"Pinned Order {activeOrderSlotId}",
                Type = DynamicModifierType.Obstacle,
                Trigger = DynamicModifierTrigger.CanApplyAction,
                Scope = DynamicModifierScope.Order,
                Effect = DynamicModifierEffect.PinOrderSlot,
                Charges = System.Math.Max(1, turns),
                ConsumesMove = false,
                TargetKind = DynamicModifierTargetKind.OrderSlot,
                TargetId = activeOrderSlotId
            };
        }

        public static DynamicModifierDefinition DreamRefresh(int charges = 1)
        {
            return new DynamicModifierDefinition
            {
                Id = DreamRefreshId,
                DisplayName = "Dream Refresh",
                Type = DynamicModifierType.Item,
                Trigger = DynamicModifierTrigger.Manual,
                Scope = DynamicModifierScope.Dream,
                Effect = DynamicModifierEffect.RefreshActiveDream,
                Charges = charges,
                RequiresItem = true,
                TargetKind = DynamicModifierTargetKind.ActiveDreamSlot,
                TargetId = -1
            };
        }

        public static DynamicModifierDefinition OperationSoftBlock(DynamicOperation operation, int turns = 1)
        {
            return new DynamicModifierDefinition
            {
                Id = $"{OperationSoftBlockIdPrefix}-{operation}",
                DisplayName = $"{operation} Soft Block",
                Type = DynamicModifierType.Obstacle,
                Trigger = DynamicModifierTrigger.CanApplyAction,
                Scope = DynamicModifierScope.Round,
                Effect = DynamicModifierEffect.SoftBlockOperation,
                Charges = System.Math.Max(1, turns),
                ConsumesMove = false,
                TargetKind = DynamicModifierTargetKind.Operation,
                TargetId = (int)operation,
                Tags = new[] { $"operation:{operation}" }
            };
        }
    }
}
