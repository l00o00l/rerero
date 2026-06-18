using System.Collections.Generic;
using Thkim.DreamLaundromat.DynamicLab;

namespace Thkim.DreamLaundromat.Gameplay.ReleaseSlice
{
    public static class ReleaseGameplayCardRenderer
    {
        public static string BuildDreamCardLabel(DynamicRoundState state, DynamicDreamSlot slot)
        {
            if (slot.IsEmpty)
            {
                return $"D{slot.SlotId}\nEmpty";
            }

            string lockLabel = IsDreamSlotLocked(state, slot.SlotId) ? "\nLocked" : string.Empty;
            return $"D{slot.SlotId}\n{ReleaseVisualDescriptors.DescribeCompact(slot.Dream.Attributes)}{lockLabel}";
        }

        public static string BuildOrderCardLabel(DynamicOrderSlot slot)
        {
            if (slot.IsEmpty)
            {
                return $"O{slot.SlotId}\nDone";
            }

            return $"O{slot.SlotId}\n{slot.Order.FulfilledCount}/{slot.Order.Requirement.Count}\n{DescribeRequirementCompact(slot.Order.Requirement)}";
        }

        public static string BuildStorageCardLabel(DynamicStorageSlot slot)
        {
            if (slot.IsEmpty)
            {
                return $"S{slot.SlotId}\nEmpty";
            }

            return $"S{slot.SlotId}\nStored\n{ReleaseVisualDescriptors.DescribeCompact(slot.Dream.Attributes)}";
        }

        public static string BuildOperationLabel(
            DynamicRoundState state,
            int selectedDreamSlotId,
            DynamicOperation operation)
        {
            DynamicDreamSlot slot = selectedDreamSlotId < 0 ? null : state.FindActiveDreamSlot(selectedDreamSlotId);
            if (slot == null || slot.IsEmpty)
            {
                ReleaseActionVisualDescriptor descriptor = ReleaseVisualDescriptors.ForOperation(operation);
                return descriptor.ButtonTitle;
            }

            if (!DynamicRulesEngine.CanApplyOperation(slot.Dream.Attributes, operation))
            {
                ReleaseActionVisualDescriptor descriptor = ReleaseVisualDescriptors.ForOperation(operation);
                return $"{descriptor.ButtonTitle}\nNo change";
            }

            DynamicActionResult modifierCheck = DynamicModifierPipeline.CanApplyAction(
                state,
                DynamicPlayerAction.ApplyOperation(selectedDreamSlotId, operation));
            if (!modifierCheck.Success)
            {
                ReleaseActionVisualDescriptor descriptor = ReleaseVisualDescriptors.ForOperation(operation);
                return $"{descriptor.ButtonTitle}\nBlocked\nConstraint active";
            }

            ReleaseActionVisualDescriptor activeDescriptor = ReleaseVisualDescriptors.ForOperation(operation);
            return $"{activeDescriptor.ButtonTitle}\n{ReleaseVisualDescriptors.DescribeCompact(PreviewOperation(slot.Dream.Attributes, operation))}";
        }

        public static string BuildModifierLabel(
            DynamicModifierDefinition definition,
            DynamicModifierState modifierState,
            int targetId)
        {
            int charges = modifierState?.RemainingCharges ?? 0;
            if (definition.Type == DynamicModifierType.Item)
            {
                if (definition.Effect == DynamicModifierEffect.RefreshActiveDream && targetId < 0)
                {
                    return $"Item\n{definition.DisplayName}\nPick dream";
                }

                return $"Item\n{definition.DisplayName}\n{charges}";
            }

            string state = modifierState == null || modifierState.IsResolved
                ? "resolved"
                : $"{modifierState.RemainingCharges}";
            return $"Block\n{definition.DisplayName}\n{state}";
        }

        public static string BuildStatusMessage(DynamicRoundState state, string message)
        {
            if (state.Status == DynamicRoundStatus.Cleared)
            {
                return $"Cleared\nOrders complete. {message}";
            }

            if (state.Status == DynamicRoundStatus.Failed)
            {
                return $"Failed\n{state.FailureReason}. {message}";
            }

            return message;
        }

        public static string DescribeRequirementCompact(DynamicOrderRequirement requirement)
        {
            var parts = new List<string>();
            if (requirement.HasTaint)
            {
                parts.Add(ReleaseVisualDescriptors.ForTaint(requirement.RequiredTaint).Marker);
            }

            if (requirement.HasMood)
            {
                parts.Add(ReleaseVisualDescriptors.ForMood(requirement.RequiredMood).Marker);
            }

            if (requirement.HasClarity)
            {
                parts.Add(ReleaseVisualDescriptors.ForClarity(requirement.RequiredClarity).Marker);
            }

            if (requirement.HasStability)
            {
                parts.Add(ReleaseVisualDescriptors.ForStability(requirement.RequiredStability).Marker);
            }

            return parts.Count == 0 ? "STB" : string.Join(" ", parts);
        }

        public static string DescribeDreamPreview(DynamicRoundState state)
        {
            if (state.DreamPreview.Count == 0)
            {
                return "none";
            }

            var values = new List<string>();
            for (int i = 0; i < state.DreamPreview.Count; i++)
            {
                values.Add(ReleaseVisualDescriptors.DescribeCompact(state.DreamPreview[i].Attributes));
            }

            return string.Join(" | ", values);
        }

        public static string DescribeOrderPreview(DynamicRoundState state)
        {
            if (state.OrderPreview.Count == 0)
            {
                return "none";
            }

            var values = new List<string>();
            for (int i = 0; i < state.OrderPreview.Count; i++)
            {
                values.Add(ReleaseVisualDescriptors.Describe(state.OrderPreview[i].Requirement));
            }

            return string.Join(" | ", values);
        }

        public static DynamicDreamAttributes PreviewOperation(DynamicDreamAttributes attributes, DynamicOperation operation)
        {
            return operation switch
            {
                DynamicOperation.Wash => attributes.WithTaint(DreamTaint.Clean).WithStability(DreamStability.Unsettled),
                DynamicOperation.Soothe => attributes.WithMood(DreamMood.Calm),
                DynamicOperation.Clarify => attributes.WithClarity(DreamClarity.Vivid),
                DynamicOperation.Settle => attributes.WithStability(DreamStability.Stable),
                _ => attributes
            };
        }

        public static bool IsDreamSlotLocked(DynamicRoundState state, int slotId)
        {
            for (int i = 0; i < state.Modifiers.Count; i++)
            {
                DynamicModifierState modifierState = state.Modifiers[i];
                DynamicModifierDefinition definition = DynamicModifierPipeline.FindDefinition(state, modifierState.ModifierId);
                if (definition != null
                    && definition.Effect == DynamicModifierEffect.LockActiveDreamSlot
                    && !modifierState.IsResolved
                    && modifierState.BoundTargetKind == DynamicModifierTargetKind.ActiveDreamSlot
                    && modifierState.BoundTargetId == slotId)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
