namespace Thkim.DreamLaundromat.DynamicLab
{
    public sealed class DynamicDreamSlot
    {
        public int SlotId;
        public DynamicDreamState Dream;

        public bool IsEmpty => Dream == null;

        public DynamicDreamSlot Clone()
        {
            return new DynamicDreamSlot
            {
                SlotId = SlotId,
                Dream = Dream?.Clone()
            };
        }
    }

    public sealed class DynamicOrderSlot
    {
        public int SlotId;
        public DynamicOrderState Order;

        public bool IsEmpty => Order == null;

        public DynamicOrderSlot Clone()
        {
            return new DynamicOrderSlot
            {
                SlotId = SlotId,
                Order = Order?.Clone()
            };
        }
    }

    public sealed class DynamicStorageSlot
    {
        public int SlotId;
        public DynamicDreamState Dream;

        public bool IsEmpty => Dream == null;

        public DynamicStorageSlot Clone()
        {
            return new DynamicStorageSlot
            {
                SlotId = SlotId,
                Dream = Dream?.Clone()
            };
        }
    }
}
