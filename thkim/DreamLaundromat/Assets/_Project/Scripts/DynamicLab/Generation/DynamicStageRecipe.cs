using System;

namespace Thkim.DreamLaundromat.DynamicLab
{
    public sealed class DynamicStageRecipe
    {
        public string RecipeId = string.Empty;
        public string RoundIdPrefix = string.Empty;
        public int MoveLimit = 10;
        public int TargetCompletedOrders = 3;
        public int DifficultyTarget;
        public int CandidateDreamCount = 6;
        public int CandidateOrderCount = 4;
        public DynamicStreamConfig StreamConfig = new DynamicStreamConfig();
        public DynamicStorageConfig StorageConfig = new DynamicStorageConfig();
        public DynamicOperation[] ActionSet =
        {
            DynamicOperation.Wash,
            DynamicOperation.Soothe,
            DynamicOperation.Clarify,
            DynamicOperation.Settle
        };

        public DynamicWeightedDreamEntry[] DreamPool = Array.Empty<DynamicWeightedDreamEntry>();
        public DynamicWeightedOrderEntry[] OrderPool = Array.Empty<DynamicWeightedOrderEntry>();
        public DynamicModifierDefinition[] AllowedModifiers = Array.Empty<DynamicModifierDefinition>();
        public bool RequiresItem;
        public bool CompareWithoutItems;
        public string[] TutorialTags = Array.Empty<string>();
        public string DesignIntent = string.Empty;
        public string PlayerQuestion = string.Empty;
        public string RiskNote = string.Empty;

        public int MinAcceptedMoves = 1;
        public int MaxAcceptedMoves;
        public int MinConversionCount;
        public int MinOperationDiversity;
        public int MinActionTypeDiversity = 2;
        public int MaxRepeatedActionTypeRun = 4;
        public int MaxMoveSlack = 8;
        public float MaxStorageMoveRatio = 1f;
        public float MaxSettleActionRatio = 1f;
        public bool RejectOnDesignWarnings;
    }

    public readonly struct DynamicWeightedDreamEntry
    {
        public DynamicWeightedDreamEntry(DynamicDreamAttributes attributes, int weight)
        {
            Attributes = attributes;
            Weight = weight;
        }

        public DynamicDreamAttributes Attributes { get; }
        public int Weight { get; }
    }

    public readonly struct DynamicWeightedOrderEntry
    {
        public DynamicWeightedOrderEntry(DynamicOrderRequirement requirement, int weight)
        {
            Requirement = requirement;
            Weight = weight;
        }

        public DynamicOrderRequirement Requirement { get; }
        public int Weight { get; }
    }
}
