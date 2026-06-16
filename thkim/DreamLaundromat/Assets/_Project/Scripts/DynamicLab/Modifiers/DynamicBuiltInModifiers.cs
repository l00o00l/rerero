namespace Thkim.DreamLaundromat.DynamicLab
{
    public static class DynamicBuiltInModifiers
    {
        public const string PreviewSwapId = "item.preview-swap";
        public const string LockedSlotIdPrefix = "obstacle.locked-slot";

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
    }
}
