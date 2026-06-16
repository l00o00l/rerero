namespace Thkim.DreamLaundromat.DynamicLab
{
    public static class DynamicSampleRecipes
    {
        public static DynamicStageRecipe[] CreateAll()
        {
            return new[]
            {
                CreateMoodBasicsRecipe(),
                CreateCleanClarityRecipe(),
                CreateCompactFlowRecipe(),
                CreateLockedSlotModifierRecipe()
            };
        }

        public static DynamicStageRecipe CreateMoodBasicsRecipe()
        {
            return new DynamicStageRecipe
            {
                RecipeId = "DLAB-R1-mood-basics",
                RoundIdPrefix = "DLAB-R1",
                MoveLimit = 8,
                TargetCompletedOrders = 3,
                CandidateDreamCount = 5,
                CandidateOrderCount = 3,
                DifficultyTarget = 1,
                StreamConfig = Stream(3, 2, 2, 1, 5, 3),
                StorageConfig = Storage(0),
                ActionSet = new[]
                {
                    DynamicOperation.Soothe,
                    DynamicOperation.Settle
                },
                DreamPool = new[]
                {
                    Dream(CleanAnxious(DreamClarity.Blurry, DreamStability.Stable), 3),
                    Dream(CleanCalm(DreamClarity.Blurry, DreamStability.Unsettled), 1),
                    Dream(CleanCalm(DreamClarity.Blurry, DreamStability.Stable), 2)
                },
                OrderPool = new[]
                {
                    Order(StableOrder(false, DreamTaint.Clean, true, DreamMood.Calm, false, DreamClarity.Blurry), 1)
                },
                MinAcceptedMoves = 3,
                MaxMoveSlack = 5,
                TutorialTags = new[] { "mood", "settle" },
                DesignIntent = "Anxious dreams can be made acceptable without introducing taint or clarity pressure.",
                PlayerQuestion = "Which dream needs soothing now, and which one is already calm enough to submit?",
                RiskNote = "If too many dreams start Calm and Stable, the round collapses into direct submit practice."
            };
        }

        public static DynamicStageRecipe CreateCleanClarityRecipe()
        {
            return new DynamicStageRecipe
            {
                RecipeId = "DLAB-R2-clean-clarity",
                RoundIdPrefix = "DLAB-R2",
                MoveLimit = 12,
                TargetCompletedOrders = 3,
                CandidateDreamCount = 5,
                CandidateOrderCount = 4,
                DifficultyTarget = 2,
                StreamConfig = Stream(3, 2, 2, 1, 5, 4),
                StorageConfig = Storage(1),
                DreamPool = new[]
                {
                    Dream(CleanAnxious(DreamClarity.Blurry, DreamStability.Stable), 2),
                    Dream(NightmareCalm(DreamClarity.Vivid, DreamStability.Stable), 2),
                    Dream(CleanCalm(DreamClarity.Blurry, DreamStability.Unsettled), 1)
                },
                OrderPool = new[]
                {
                    Order(StableOrder(true, DreamTaint.Clean, true, DreamMood.Calm, true, DreamClarity.Vivid), 1)
                },
                MinConversionCount = 1,
                MinOperationDiversity = 1,
                MaxMoveSlack = 7,
                TutorialTags = new[] { "wash", "clarify" },
                DesignIntent = "The same final order can be reached through cleaning, clarifying, soothing, or settling depending on the incoming dream.",
                PlayerQuestion = "Which state axis is cheapest to fix for the visible order stream?",
                RiskNote = "A broad order can accidentally accept unprocessed dreams, so the recipe uses a narrow Clean-Calm-Vivid target."
            };
        }

        public static DynamicStageRecipe CreateCompactFlowRecipe()
        {
            return new DynamicStageRecipe
            {
                RecipeId = "DLAB-R3-compact-flow",
                RoundIdPrefix = "DLAB-R3",
                MoveLimit = 16,
                TargetCompletedOrders = 4,
                CandidateDreamCount = 6,
                CandidateOrderCount = 5,
                DifficultyTarget = 3,
                StreamConfig = Stream(2, 2, 2, 1, 6, 5),
                StorageConfig = Storage(1),
                DreamPool = new[]
                {
                    Dream(NightmareAnxious(DreamClarity.Blurry, DreamStability.Stable), 3),
                    Dream(CleanAnxious(DreamClarity.Blurry, DreamStability.Stable), 2),
                    Dream(NightmareCalm(DreamClarity.Blurry, DreamStability.Unsettled), 1)
                },
                OrderPool = new[]
                {
                    Order(StableOrder(false, DreamTaint.Clean, true, DreamMood.Calm, true, DreamClarity.Vivid), 2),
                    Order(StableOrder(true, DreamTaint.Clean, false, DreamMood.Calm, true, DreamClarity.Vivid), 2),
                    Order(StableOrder(true, DreamTaint.Clean, true, DreamMood.Calm, false, DreamClarity.Blurry), 1)
                },
                MinConversionCount = 2,
                MinOperationDiversity = 1,
                MaxMoveSlack = 10,
                TutorialTags = new[] { "storage", "planning" },
                DesignIntent = "Small active space should make preview and temporary storage matter without becoming pure packing.",
                PlayerQuestion = "Which dream should be held back while another one is transformed for the current order?",
                RiskNote = "If storage actions dominate the shortest path, the puzzle stops being about state assignment."
            };
        }

        public static DynamicStageRecipe CreateLockedSlotModifierRecipe()
        {
            return new DynamicStageRecipe
            {
                RecipeId = "DLMOD-R1-locked-slot",
                RoundIdPrefix = "DLMOD-R1",
                MoveLimit = 3,
                TargetCompletedOrders = 1,
                CandidateDreamCount = 2,
                CandidateOrderCount = 1,
                DifficultyTarget = 1,
                StreamConfig = Stream(2, 1, 0, 0, 2, 1),
                StorageConfig = Storage(0),
                ActionSet = new[] { DynamicOperation.Settle },
                DreamPool = new[]
                {
                    Dream(CleanCalm(DreamClarity.Blurry, DreamStability.Stable), 1)
                },
                OrderPool = new[]
                {
                    Order(StableOrder(false, DreamTaint.Clean, true, DreamMood.Calm, false, DreamClarity.Blurry), 1)
                },
                AllowedModifiers = new[]
                {
                    DynamicBuiltInModifiers.LockedActiveDreamSlot(0)
                },
                MinAcceptedMoves = 1,
                MinActionTypeDiversity = 1,
                MaxMoveSlack = 2,
                TutorialTags = new[] { "modifier", "locked-slot" },
                DesignIntent = "A visible locked slot should create a routing constraint without hiding random failure.",
                PlayerQuestion = "Which unlocked dream slot can still satisfy the visible order?",
                RiskNote = "If locked slots appear too often, they can feel like UI denial rather than puzzle pressure."
            };
        }

        private static DynamicWeightedDreamEntry Dream(DynamicDreamAttributes attributes, int weight)
        {
            return new DynamicWeightedDreamEntry(attributes, weight);
        }

        private static DynamicWeightedOrderEntry Order(DynamicOrderRequirement requirement, int weight)
        {
            return new DynamicWeightedOrderEntry(requirement, weight);
        }

        private static DynamicStreamConfig Stream(
            int activeDreamSlots,
            int activeOrderSlots,
            int dreamPreviewCount,
            int orderPreviewCount,
            int maxDreamDraws,
            int maxOrderDraws)
        {
            return new DynamicStreamConfig
            {
                ActiveDreamSlots = activeDreamSlots,
                ActiveOrderSlots = activeOrderSlots,
                DreamPreviewCount = dreamPreviewCount,
                OrderPreviewCount = orderPreviewCount,
                MaxDreamDraws = maxDreamDraws,
                MaxOrderDraws = maxOrderDraws
            };
        }

        private static DynamicStorageConfig Storage(int storageSlotCount)
        {
            return new DynamicStorageConfig
            {
                StorageSlotCount = storageSlotCount
            };
        }

        private static DynamicOrderRequirement StableOrder(
            bool hasTaint,
            DreamTaint taint,
            bool hasMood,
            DreamMood mood,
            bool hasClarity,
            DreamClarity clarity)
        {
            return DynamicOrderRequirement.Stable(1, hasTaint, taint, hasMood, mood, hasClarity, clarity);
        }

        private static DynamicDreamAttributes CleanCalm(DreamClarity clarity, DreamStability stability)
        {
            return new DynamicDreamAttributes(DreamTaint.Clean, DreamMood.Calm, clarity, stability);
        }

        private static DynamicDreamAttributes CleanAnxious(DreamClarity clarity, DreamStability stability)
        {
            return new DynamicDreamAttributes(DreamTaint.Clean, DreamMood.Anxious, clarity, stability);
        }

        private static DynamicDreamAttributes NightmareAnxious(DreamClarity clarity, DreamStability stability)
        {
            return new DynamicDreamAttributes(DreamTaint.Nightmare, DreamMood.Anxious, clarity, stability);
        }

        private static DynamicDreamAttributes NightmareCalm(DreamClarity clarity, DreamStability stability)
        {
            return new DynamicDreamAttributes(DreamTaint.Nightmare, DreamMood.Calm, clarity, stability);
        }
    }
}
