using Thkim.DreamLaundromat.DynamicLab;

namespace Thkim.DreamLaundromat.Gameplay.ReleaseSlice
{
    public enum ReleaseDragSourceKind
    {
        ActiveDream,
        Storage
    }

    public enum ReleaseDropTargetKind
    {
        ActiveDream,
        ActiveOrder,
        Storage
    }

    public readonly struct ReleaseDragPayload
    {
        public ReleaseDragPayload(ReleaseDragSourceKind sourceKind, int slotId)
        {
            SourceKind = sourceKind;
            SlotId = slotId;
        }

        public ReleaseDragSourceKind SourceKind { get; }
        public int SlotId { get; }
    }

    public readonly struct ReleaseDropTargetDescriptor
    {
        public ReleaseDropTargetDescriptor(ReleaseDropTargetKind targetKind, int slotId)
        {
            TargetKind = targetKind;
            SlotId = slotId;
        }

        public ReleaseDropTargetKind TargetKind { get; }
        public int SlotId { get; }
    }

    public readonly struct ReleaseDragActionResolution
    {
        private ReleaseDragActionResolution(bool success, DynamicPlayerAction action, string message)
        {
            Success = success;
            Action = action;
            Message = message ?? string.Empty;
        }

        public bool Success { get; }
        public DynamicPlayerAction Action { get; }
        public string Message { get; }

        public static ReleaseDragActionResolution Succeeded(DynamicPlayerAction action, string message)
        {
            return new ReleaseDragActionResolution(true, action, message);
        }

        public static ReleaseDragActionResolution Failed(string message)
        {
            return new ReleaseDragActionResolution(false, default, message);
        }
    }

    public static class ReleaseDragActionResolver
    {
        public static ReleaseDragActionResolution Resolve(
            DynamicRoundState state,
            ReleaseDragPayload payload,
            ReleaseDropTargetDescriptor target)
        {
            if (state == null || state.Status != DynamicRoundStatus.Playing)
            {
                return ReleaseDragActionResolution.Failed("Round is not playing.");
            }

            if (payload.SourceKind == ReleaseDragSourceKind.ActiveDream
                && target.TargetKind == ReleaseDropTargetKind.ActiveOrder)
            {
                return ResolveSubmit(state, payload.SlotId, target.SlotId);
            }

            if (payload.SourceKind == ReleaseDragSourceKind.ActiveDream
                && target.TargetKind == ReleaseDropTargetKind.Storage)
            {
                return ResolveStore(state, payload.SlotId, target.SlotId);
            }

            if (payload.SourceKind == ReleaseDragSourceKind.Storage
                && target.TargetKind == ReleaseDropTargetKind.ActiveDream)
            {
                return ResolveRecall(state, payload.SlotId, target.SlotId);
            }

            return ReleaseDragActionResolution.Failed("Drop that card on a matching target.");
        }

        private static ReleaseDragActionResolution ResolveSubmit(
            DynamicRoundState state,
            int activeDreamSlotId,
            int activeOrderSlotId)
        {
            DynamicDreamSlot dreamSlot = state.FindActiveDreamSlot(activeDreamSlotId);
            DynamicOrderSlot orderSlot = state.FindActiveOrderSlot(activeOrderSlotId);
            if (!ReleaseActionAvailability.CanSubmitDreamToOrder(state, dreamSlot, orderSlot))
            {
                return ReleaseDragActionResolution.Failed("That dream does not fit the order.");
            }

            return ReleaseDragActionResolution.Succeeded(
                DynamicPlayerAction.SubmitDream(activeDreamSlotId, activeOrderSlotId),
                "Dream submitted.");
        }

        private static ReleaseDragActionResolution ResolveStore(
            DynamicRoundState state,
            int activeDreamSlotId,
            int storageSlotId)
        {
            var selection = new ReleaseSelectionState();
            selection.SelectDream(activeDreamSlotId);
            DynamicStorageSlot storageSlot = state.FindStorageSlot(storageSlotId);
            if (!ReleaseActionAvailability.CanStoreSelection(state, selection, storageSlot))
            {
                return ReleaseDragActionResolution.Failed("That dream cannot be stored there.");
            }

            return ReleaseDragActionResolution.Succeeded(
                DynamicPlayerAction.StoreDream(activeDreamSlotId, storageSlotId),
                "Dream stored.");
        }

        private static ReleaseDragActionResolution ResolveRecall(
            DynamicRoundState state,
            int storageSlotId,
            int activeDreamSlotId)
        {
            var selection = new ReleaseSelectionState();
            selection.SelectStorage(storageSlotId);
            DynamicDreamSlot dreamSlot = state.FindActiveDreamSlot(activeDreamSlotId);
            if (!ReleaseActionAvailability.CanRecallSelection(state, selection, dreamSlot))
            {
                return ReleaseDragActionResolution.Failed("That storage card cannot return there.");
            }

            return ReleaseDragActionResolution.Succeeded(
                DynamicPlayerAction.RecallDream(storageSlotId, activeDreamSlotId),
                "Dream recalled.");
        }
    }
}
