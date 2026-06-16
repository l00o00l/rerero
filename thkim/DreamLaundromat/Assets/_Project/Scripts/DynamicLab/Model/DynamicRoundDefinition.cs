namespace Thkim.DreamLaundromat.DynamicLab
{
    public sealed class DynamicRoundDefinition
    {
        public string RoundId = string.Empty;
        public int Seed;
        public int MoveLimit;
        public int TargetCompletedOrders;
        public DynamicStreamConfig StreamConfig = new DynamicStreamConfig();
        public DynamicDreamBagEntry[] DreamBag = System.Array.Empty<DynamicDreamBagEntry>();
        public DynamicOrderDeckEntry[] OrderDeck = System.Array.Empty<DynamicOrderDeckEntry>();
        public DynamicOperation[] ActionSet =
        {
            DynamicOperation.Wash,
            DynamicOperation.Soothe,
            DynamicOperation.Clarify,
            DynamicOperation.Settle
        };
        public DynamicStorageConfig StorageConfig = new DynamicStorageConfig();
        public DynamicModifierDefinition[] Modifiers = System.Array.Empty<DynamicModifierDefinition>();
        public string[] TutorialTags = System.Array.Empty<string>();
        public int DifficultyTarget;
    }

    public sealed class DynamicStreamConfig
    {
        public int ActiveDreamSlots = 5;
        public int ActiveOrderSlots = 3;
        public int DreamPreviewCount = 2;
        public int OrderPreviewCount = 1;
        public int MaxDreamDraws = 12;
        public int MaxOrderDraws = 8;
    }

    public sealed class DynamicStorageConfig
    {
        public int StorageSlotCount = 2;
    }

    public readonly struct DynamicDreamBagEntry
    {
        public DynamicDreamBagEntry(DynamicDreamAttributes attributes, int count)
        {
            Attributes = attributes;
            Count = count;
        }

        public DynamicDreamAttributes Attributes { get; }
        public int Count { get; }
    }

    public readonly struct DynamicOrderDeckEntry
    {
        public DynamicOrderDeckEntry(DynamicOrderRequirement requirement, int count)
        {
            Requirement = requirement;
            Count = count;
        }

        public DynamicOrderRequirement Requirement { get; }
        public int Count { get; }
    }
}
