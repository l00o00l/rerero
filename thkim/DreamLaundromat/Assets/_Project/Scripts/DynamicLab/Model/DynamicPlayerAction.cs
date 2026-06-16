namespace Thkim.DreamLaundromat.DynamicLab
{
    public readonly struct DynamicPlayerAction
    {
        public DynamicActionType Type { get; }
        public int ActiveDreamSlotId { get; }
        public int ActiveOrderSlotId { get; }
        public int StorageSlotId { get; }
        public DynamicOperation Operation { get; }
        public string ModifierId { get; }
        public int ModifierTargetId { get; }

        private DynamicPlayerAction(
            DynamicActionType type,
            int activeDreamSlotId,
            int activeOrderSlotId,
            int storageSlotId,
            DynamicOperation operation,
            string modifierId,
            int modifierTargetId)
        {
            Type = type;
            ActiveDreamSlotId = activeDreamSlotId;
            ActiveOrderSlotId = activeOrderSlotId;
            StorageSlotId = storageSlotId;
            Operation = operation;
            ModifierId = modifierId;
            ModifierTargetId = modifierTargetId;
        }

        public static DynamicPlayerAction ApplyOperation(int activeDreamSlotId, DynamicOperation operation)
        {
            return new DynamicPlayerAction(
                DynamicActionType.ApplyOperation,
                activeDreamSlotId,
                -1,
                -1,
                operation,
                string.Empty,
                -1);
        }

        public static DynamicPlayerAction SubmitDream(int activeDreamSlotId, int activeOrderSlotId)
        {
            return new DynamicPlayerAction(
                DynamicActionType.SubmitDream,
                activeDreamSlotId,
                activeOrderSlotId,
                -1,
                default,
                string.Empty,
                -1);
        }

        public static DynamicPlayerAction StoreDream(int activeDreamSlotId, int storageSlotId)
        {
            return new DynamicPlayerAction(
                DynamicActionType.StoreDream,
                activeDreamSlotId,
                -1,
                storageSlotId,
                default,
                string.Empty,
                -1);
        }

        public static DynamicPlayerAction RecallDream(int storageSlotId, int activeDreamSlotId)
        {
            return new DynamicPlayerAction(
                DynamicActionType.RecallDream,
                activeDreamSlotId,
                -1,
                storageSlotId,
                default,
                string.Empty,
                -1);
        }

        public static DynamicPlayerAction UseItem(string modifierId, int targetId = -1)
        {
            return new DynamicPlayerAction(
                DynamicActionType.UseItem,
                -1,
                -1,
                -1,
                default,
                modifierId ?? string.Empty,
                targetId);
        }
    }
}
