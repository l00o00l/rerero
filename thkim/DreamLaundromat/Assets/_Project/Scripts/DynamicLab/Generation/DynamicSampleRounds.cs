namespace Thkim.DreamLaundromat.DynamicLab
{
    public static class DynamicSampleRounds
    {
        public static DynamicRoundDefinition[] CreateAll()
        {
            return new[]
            {
                CreateStateAssignmentRound(),
                CreateOperationOrderingRound(),
                CreateStreamTimingRound(),
                CreateStoragePressureRound(),
                CreateReversalOrderRound(),
                CreatePreviewSwapRequiredRound(),
                CreateLockedSlotRound()
            };
        }

        public static DynamicRoundDefinition CreateStateAssignmentRound()
        {
            return CreateRound(
                "DLAB-A-state-assignment",
                1101,
                10,
                2,
                new[]
                {
                    Dream(CleanCalm(DreamClarity.Blurry, DreamStability.Stable), 2),
                    Dream(CleanAnxious(DreamClarity.Vivid, DreamStability.Stable), 1)
                },
                new[]
                {
                    Order(StableOrder(true, DreamMood.Calm, false, DreamClarity.Blurry), 1),
                    Order(StableOrder(true, DreamMood.Anxious, true, DreamClarity.Vivid), 1)
                });
        }

        public static DynamicRoundDefinition CreateOperationOrderingRound()
        {
            return CreateRound(
                "DLAB-B-operation-ordering",
                2202,
                12,
                1,
                new[]
                {
                    Dream(NightmareAnxious(DreamClarity.Blurry, DreamStability.Stable), 1),
                    Dream(CleanAnxious(DreamClarity.Blurry, DreamStability.Unsettled), 1)
                },
                new[]
                {
                    Order(StableOrder(true, DreamMood.Calm, true, DreamClarity.Vivid), 1)
                });
        }

        public static DynamicRoundDefinition CreateStreamTimingRound()
        {
            return CreateRound(
                "DLAB-C-stream-timing",
                3303,
                12,
                3,
                new[]
                {
                    Dream(CleanCalm(DreamClarity.Blurry, DreamStability.Stable), 2),
                    Dream(CleanAnxious(DreamClarity.Blurry, DreamStability.Stable), 2),
                    Dream(NightmareCalm(DreamClarity.Vivid, DreamStability.Stable), 1)
                },
                new[]
                {
                    Order(StableOrder(true, DreamMood.Calm, false, DreamClarity.Blurry), 2),
                    Order(StableOrder(false, DreamMood.Calm, true, DreamClarity.Vivid), 1)
                });
        }

        public static DynamicRoundDefinition CreateStoragePressureRound()
        {
            DynamicRoundDefinition round = CreateRound(
                "DLAB-D-storage-pressure",
                4404,
                14,
                2,
                new[]
                {
                    Dream(NightmareAnxious(DreamClarity.Blurry, DreamStability.Stable), 2),
                    Dream(CleanCalm(DreamClarity.Vivid, DreamStability.Stable), 1)
                },
                new[]
                {
                    Order(StableOrder(true, DreamMood.Calm, true, DreamClarity.Vivid), 1),
                    Order(StableOrder(true, DreamMood.Calm, false, DreamClarity.Blurry), 1)
                });
            round.StreamConfig.ActiveDreamSlots = 2;
            round.StorageConfig.StorageSlotCount = 1;
            return round;
        }

        public static DynamicRoundDefinition CreateReversalOrderRound()
        {
            return CreateRound(
                "DLAB-E-reversal-order",
                5505,
                10,
                2,
                new[]
                {
                    Dream(NightmareCalm(DreamClarity.Blurry, DreamStability.Stable), 1),
                    Dream(CleanCalm(DreamClarity.Blurry, DreamStability.Stable), 2)
                },
                new[]
                {
                    Order(StableOrder(true, DreamTaint.Nightmare, false, DreamMood.Calm, false, DreamClarity.Blurry), 1),
                    Order(StableOrder(true, DreamTaint.Clean, true, DreamMood.Calm, false, DreamClarity.Blurry), 1)
                });
        }

        public static DynamicRoundDefinition CreatePreviewSwapRequiredRound()
        {
            var round = CreateRound(
                "DLMOD-A-preview-swap-required",
                2,
                3,
                2,
                new[]
                {
                    Dream(CleanAnxious(DreamClarity.Blurry, DreamStability.Stable), 1),
                    Dream(CleanCalm(DreamClarity.Blurry, DreamStability.Stable), 1),
                    Dream(CleanCalm(DreamClarity.Vivid, DreamStability.Stable), 1)
                },
                new[]
                {
                    Order(StableOrder(true, DreamMood.Calm, true, DreamClarity.Vivid), 1),
                    Order(StableOrder(true, DreamMood.Calm, true, DreamClarity.Blurry), 1)
                });
            round.StreamConfig.ActiveDreamSlots = 1;
            round.StreamConfig.ActiveOrderSlots = 1;
            round.StreamConfig.DreamPreviewCount = 2;
            round.StreamConfig.OrderPreviewCount = 1;
            round.StorageConfig.StorageSlotCount = 0;
            round.ActionSet = new[] { DynamicOperation.Settle };
            round.Modifiers = new[] { DynamicBuiltInModifiers.PreviewSwap(requiresItem: true) };
            return round;
        }

        public static DynamicRoundDefinition CreateLockedSlotRound()
        {
            var round = CreateRound(
                "DLMOD-B-locked-slot",
                7707,
                4,
                1,
                new[]
                {
                    Dream(CleanCalm(DreamClarity.Blurry, DreamStability.Stable), 2)
                },
                new[]
                {
                    Order(StableOrder(true, DreamMood.Calm, false, DreamClarity.Blurry), 1)
                });
            round.StreamConfig.ActiveDreamSlots = 2;
            round.StreamConfig.ActiveOrderSlots = 1;
            round.StreamConfig.DreamPreviewCount = 0;
            round.StreamConfig.OrderPreviewCount = 0;
            round.StorageConfig.StorageSlotCount = 0;
            round.Modifiers = new[] { DynamicBuiltInModifiers.LockedActiveDreamSlot(0) };
            return round;
        }

        private static DynamicRoundDefinition CreateRound(
            string id,
            int seed,
            int moveLimit,
            int targetCompletedOrders,
            DynamicDreamBagEntry[] dreamBag,
            DynamicOrderDeckEntry[] orderDeck)
        {
            return new DynamicRoundDefinition
            {
                RoundId = id,
                Seed = seed,
                MoveLimit = moveLimit,
                TargetCompletedOrders = targetCompletedOrders,
                DreamBag = dreamBag,
                OrderDeck = orderDeck,
                StreamConfig = new DynamicStreamConfig
                {
                    ActiveDreamSlots = 3,
                    ActiveOrderSlots = 2,
                    DreamPreviewCount = 2,
                    OrderPreviewCount = 1,
                    MaxDreamDraws = 8,
                    MaxOrderDraws = 6
                },
                StorageConfig = new DynamicStorageConfig
                {
                    StorageSlotCount = 2
                }
            };
        }

        private static DynamicDreamBagEntry Dream(DynamicDreamAttributes attributes, int count)
        {
            return new DynamicDreamBagEntry(attributes, count);
        }

        private static DynamicOrderDeckEntry Order(DynamicOrderRequirement requirement, int count)
        {
            return new DynamicOrderDeckEntry(requirement, count);
        }

        private static DynamicOrderRequirement StableOrder(
            bool hasMood,
            DreamMood mood,
            bool hasClarity,
            DreamClarity clarity)
        {
            return StableOrder(false, DreamTaint.Clean, hasMood, mood, hasClarity, clarity);
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
