using System.Text;

namespace Thkim.DreamLaundromat.DynamicLab
{
    public static class DynamicRoundStateHasher
    {
        public static string CreateHash(DynamicRoundState state)
        {
            var builder = new StringBuilder(256);
            builder.Append(state.RemainingMoves).Append('|');
            builder.Append(state.CompletedOrders).Append('|');
            builder.Append(state.NextDreamIndex).Append('|');
            builder.Append(state.NextOrderIndex).Append('|');

            for (int i = 0; i < state.ActiveDreams.Count; i++)
            {
                AppendDream(builder, state.ActiveDreams[i].Dream);
            }

            builder.Append('/');
            for (int i = 0; i < state.StorageSlots.Count; i++)
            {
                AppendDream(builder, state.StorageSlots[i].Dream);
            }

            builder.Append('/');
            for (int i = 0; i < state.ActiveOrders.Count; i++)
            {
                AppendOrder(builder, state.ActiveOrders[i].Order);
            }

            builder.Append('/');
            for (int i = 0; i < state.DreamPreview.Count; i++)
            {
                AppendDream(builder, state.DreamPreview[i]);
            }

            builder.Append('/');
            for (int i = 0; i < state.OrderPreview.Count; i++)
            {
                AppendOrder(builder, state.OrderPreview[i]);
            }

            builder.Append("/dream-draw:");
            for (int i = state.NextDreamIndex; i < state.DreamDrawPile.Count; i++)
            {
                AppendDream(builder, state.DreamDrawPile[i]);
            }

            builder.Append("/order-draw:");
            for (int i = state.NextOrderIndex; i < state.OrderDrawPile.Count; i++)
            {
                AppendOrder(builder, state.OrderDrawPile[i]);
            }

            builder.Append('/');
            for (int i = 0; i < state.Modifiers.Count; i++)
            {
                AppendModifier(builder, state.Modifiers[i]);
            }

            return builder.ToString();
        }

        private static void AppendDream(StringBuilder builder, DynamicDreamState dream)
        {
            if (dream == null)
            {
                builder.Append("empty;");
                return;
            }

            builder
                .Append(dream.Id).Append(':')
                .Append((int)dream.Attributes.Taint).Append(',')
                .Append((int)dream.Attributes.Mood).Append(',')
                .Append((int)dream.Attributes.Clarity).Append(',')
                .Append((int)dream.Attributes.Stability).Append(';');
        }

        private static void AppendOrder(StringBuilder builder, DynamicOrderState order)
        {
            if (order == null)
            {
                builder.Append("empty;");
                return;
            }

            DynamicOrderRequirement requirement = order.Requirement;
            builder
                .Append(order.Id).Append(':')
                .Append(order.FulfilledCount).Append('/')
                .Append(requirement.Count).Append(':')
                .Append(requirement.HasTaint ? (int)requirement.RequiredTaint : -1).Append(',')
                .Append(requirement.HasMood ? (int)requirement.RequiredMood : -1).Append(',')
                .Append(requirement.HasClarity ? (int)requirement.RequiredClarity : -1).Append(',')
                .Append(requirement.HasStability ? (int)requirement.RequiredStability : -1).Append(';');
        }

        private static void AppendModifier(StringBuilder builder, DynamicModifierState modifier)
        {
            builder
                .Append(modifier.ModifierId).Append(':')
                .Append(modifier.RemainingCharges).Append(':')
                .Append((int)modifier.BoundTargetKind).Append(',')
                .Append(modifier.BoundTargetId).Append(',')
                .Append(modifier.IsResolved ? 1 : 0).Append(';');
        }
    }
}
