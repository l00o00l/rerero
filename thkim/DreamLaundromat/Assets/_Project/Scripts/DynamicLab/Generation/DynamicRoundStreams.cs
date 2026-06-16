namespace Thkim.DreamLaundromat.DynamicLab
{
    internal static class DynamicRoundStreams
    {
        public static void FillDreamPreview(DynamicRoundState state)
        {
            while (state.DreamPreview.Count < state.StreamConfig.DreamPreviewCount
                && state.NextDreamIndex < state.DreamDrawPile.Count)
            {
                state.DreamPreview.Add(state.DreamDrawPile[state.NextDreamIndex].Clone());
                state.NextDreamIndex++;
            }
        }

        public static void FillActiveDreamSlots(DynamicRoundState state)
        {
            for (int i = 0; i < state.ActiveDreams.Count; i++)
            {
                if (!state.ActiveDreams[i].IsEmpty)
                {
                    continue;
                }

                DynamicDreamState nextDream = DrawNextDream(state);
                if (nextDream == null)
                {
                    continue;
                }

                state.ActiveDreams[i].Dream = nextDream;
                FillDreamPreview(state);
            }
        }

        public static void FillOrderPreview(DynamicRoundState state)
        {
            while (state.OrderPreview.Count < state.StreamConfig.OrderPreviewCount
                && state.NextOrderIndex < state.OrderDrawPile.Count)
            {
                state.OrderPreview.Add(state.OrderDrawPile[state.NextOrderIndex].Clone());
                state.NextOrderIndex++;
            }
        }

        public static void FillActiveOrderSlots(DynamicRoundState state)
        {
            for (int i = 0; i < state.ActiveOrders.Count; i++)
            {
                if (!state.ActiveOrders[i].IsEmpty)
                {
                    continue;
                }

                DynamicOrderState nextOrder = DrawNextOrder(state);
                if (nextOrder == null)
                {
                    continue;
                }

                state.ActiveOrders[i].Order = nextOrder;
                FillOrderPreview(state);
            }
        }

        private static DynamicDreamState DrawNextDream(DynamicRoundState state)
        {
            if (state.DreamPreview.Count > 0)
            {
                DynamicDreamState previewDream = state.DreamPreview[0].Clone();
                state.DreamPreview.RemoveAt(0);
                return previewDream;
            }

            if (state.NextDreamIndex >= state.DreamDrawPile.Count)
            {
                return null;
            }

            DynamicDreamState draw = state.DreamDrawPile[state.NextDreamIndex].Clone();
            state.NextDreamIndex++;
            return draw;
        }

        private static DynamicOrderState DrawNextOrder(DynamicRoundState state)
        {
            if (state.OrderPreview.Count > 0)
            {
                DynamicOrderState previewOrder = state.OrderPreview[0].Clone();
                state.OrderPreview.RemoveAt(0);
                return previewOrder;
            }

            if (state.NextOrderIndex >= state.OrderDrawPile.Count)
            {
                return null;
            }

            DynamicOrderState draw = state.OrderDrawPile[state.NextOrderIndex].Clone();
            state.NextOrderIndex++;
            return draw;
        }
    }
}
