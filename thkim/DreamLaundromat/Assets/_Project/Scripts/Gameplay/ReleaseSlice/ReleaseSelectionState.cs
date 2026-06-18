namespace Thkim.DreamLaundromat.Gameplay.ReleaseSlice
{
    public sealed class ReleaseSelectionState
    {
        public const int NoSlot = -1;

        public int SelectedDreamSlotId { get; private set; } = NoSlot;
        public int SelectedOrderSlotId { get; private set; } = NoSlot;
        public int SelectedStorageSlotId { get; private set; } = NoSlot;

        public bool HasDreamSelection => SelectedDreamSlotId >= 0;
        public bool HasOrderSelection => SelectedOrderSlotId >= 0;
        public bool HasStorageSelection => SelectedStorageSlotId >= 0;

        public void SelectDream(int slotId)
        {
            SelectedDreamSlotId = SelectedDreamSlotId == slotId ? NoSlot : slotId;
            SelectedStorageSlotId = NoSlot;
        }

        public void SelectOrder(int slotId)
        {
            SelectedOrderSlotId = SelectedOrderSlotId == slotId ? NoSlot : slotId;
        }

        public void SelectStorage(int slotId)
        {
            SelectedStorageSlotId = SelectedStorageSlotId == slotId ? NoSlot : slotId;
            SelectedDreamSlotId = NoSlot;
        }

        public void ClearDream()
        {
            SelectedDreamSlotId = NoSlot;
        }

        public void ClearOrder()
        {
            SelectedOrderSlotId = NoSlot;
        }

        public void ClearStorage()
        {
            SelectedStorageSlotId = NoSlot;
        }

        public void ClearAll()
        {
            SelectedDreamSlotId = NoSlot;
            SelectedOrderSlotId = NoSlot;
            SelectedStorageSlotId = NoSlot;
        }
    }
}
