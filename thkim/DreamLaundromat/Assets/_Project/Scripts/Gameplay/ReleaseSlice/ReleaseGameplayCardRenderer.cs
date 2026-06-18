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
                return $"D{slot.SlotId + 1}\nOpen";
            }

            string lockLabel = IsDreamSlotLocked(state, slot.SlotId) ? "\nLock" : string.Empty;
            return $"D{slot.SlotId + 1}\n{ReleaseVisualDescriptors.DescribeCompact(slot.Dream.Attributes)}{lockLabel}";
        }

        public static string BuildOrderCardLabel(DynamicOrderSlot slot)
        {
            if (slot.IsEmpty)
            {
                return $"O{slot.SlotId + 1}\nDone";
            }

            return $"O{slot.SlotId + 1}\n{slot.Order.FulfilledCount}/{slot.Order.Requirement.Count}\n{DescribeRequirementCompact(slot.Order.Requirement)}";
        }

        public static string BuildStorageCardLabel(DynamicStorageSlot slot)
        {
            if (slot.IsEmpty)
            {
                return $"S{slot.SlotId + 1}";
            }

            return $"S{slot.SlotId + 1}\nStored\n{ReleaseVisualDescriptors.DescribeCompact(slot.Dream.Attributes)}";
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
            string countLabel = BuildModifierCountLabel(definition, modifierState);
            int displayTargetId = definition.Effect == DynamicModifierEffect.RefreshActiveDream
                ? targetId
                : modifierState?.BoundTargetId ?? targetId;
            if (definition.Type == DynamicModifierType.Item)
            {
                if (definition.Effect == DynamicModifierEffect.RefreshActiveDream && displayTargetId < 0)
                {
                    return JoinLabelLines("Tool", BuildModifierActionLabel(definition, displayTargetId), "Pick D");
                }

                return JoinLabelLines("Tool", BuildModifierActionLabel(definition, displayTargetId), countLabel);
            }

            return JoinLabelLines("Fault", BuildModifierActionLabel(definition, displayTargetId), countLabel);
        }

        public static string BuildStatusMessage(DynamicRoundState state, string message)
        {
            if (state.Status == DynamicRoundStatus.Cleared)
            {
                return string.IsNullOrWhiteSpace(message)
                    ? "Cleared\nOrders complete."
                    : $"Cleared\nOrders complete. {message}";
            }

            if (state.Status == DynamicRoundStatus.Failed)
            {
                return string.IsNullOrWhiteSpace(message)
                    ? $"Failed\n{state.FailureReason}."
                    : $"Failed\n{state.FailureReason}. {message}";
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

        private static string BuildModifierActionLabel(DynamicModifierDefinition definition, int targetId)
        {
            return definition.Effect switch
            {
                DynamicModifierEffect.PreviewSwap => "Swap",
                DynamicModifierEffect.RefreshActiveDream => targetId >= 0
                    ? $"Refresh D{targetId + 1}"
                    : "Refresh",
                DynamicModifierEffect.LockActiveDreamSlot => targetId >= 0
                    ? $"Lock D{targetId + 1}"
                    : "Lock",
                DynamicModifierEffect.PinOrderSlot => targetId >= 0
                    ? $"Pin O{targetId + 1}"
                    : "Pin",
                DynamicModifierEffect.SoftBlockOperation => BuildSoftBlockLabel(targetId),
                _ => definition.DisplayName
            };
        }

        private static string BuildModifierCountLabel(
            DynamicModifierDefinition definition,
            DynamicModifierState modifierState)
        {
            if (modifierState == null)
            {
                return definition.Charges > 0 ? $"x{definition.Charges}" : string.Empty;
            }

            if (modifierState.IsResolved)
            {
                return "done";
            }

            return modifierState.RemainingCharges > 0
                ? $"x{modifierState.RemainingCharges}"
                : string.Empty;
        }

        private static string BuildSoftBlockLabel(int targetId)
        {
            if (targetId < 0)
            {
                return "Jam";
            }

            var operation = (DynamicOperation)targetId;
            return $"Jam {ReleaseVisualDescriptors.ForOperation(operation).ButtonTitle}";
        }

        private static string JoinLabelLines(params string[] lines)
        {
            var values = new List<string>();
            for (int i = 0; i < lines.Length; i++)
            {
                if (!string.IsNullOrWhiteSpace(lines[i]))
                {
                    values.Add(lines[i]);
                }
            }

            return string.Join("\n", values);
        }
    }
}
