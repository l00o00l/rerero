namespace Thkim.DreamLaundromat.DynamicLab
{
    public enum DynamicModifierType
    {
        Item,
        Obstacle
    }

    public enum DynamicModifierTrigger
    {
        Manual,
        CanApplyAction,
        BeforeAction,
        AfterAction
    }

    public enum DynamicModifierScope
    {
        Round,
        Dream,
        Order,
        Slot,
        Storage,
        Preview
    }

    public enum DynamicModifierEffect
    {
        PreviewSwap,
        LockActiveDreamSlot,
        PinOrderSlot,
        RefreshActiveDream,
        SoftBlockOperation
    }

    public enum DynamicModifierTargetKind
    {
        None,
        ActiveDreamSlot,
        StorageSlot,
        OrderSlot,
        Operation
    }
}
