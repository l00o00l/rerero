using System.Collections.Generic;
using Thkim.DreamLaundromat.DynamicLab;

namespace Thkim.DreamLaundromat.Gameplay.ReleaseSlice
{
    public static class ReleaseActionAvailability
    {
        public static bool CanApplySelectedOperation(
            DynamicRoundState state,
            ReleaseSelectionState selection,
            DynamicOperation operation)
        {
            if (state == null
                || selection == null
                || state.Status != DynamicRoundStatus.Playing
                || !selection.HasDreamSelection)
            {
                return false;
            }

            DynamicDreamSlot slot = state.FindActiveDreamSlot(selection.SelectedDreamSlotId);
            if (slot == null || slot.IsEmpty || !state.IsOperationAllowed(operation))
            {
                return false;
            }

            if (!DynamicRulesEngine.CanApplyOperation(slot.Dream.Attributes, operation))
            {
                return false;
            }

            return DynamicModifierPipeline.CanApplyAction(
                state,
                DynamicPlayerAction.ApplyOperation(selection.SelectedDreamSlotId, operation)).Success;
        }

        public static bool CanSubmitSelection(
            DynamicRoundState state,
            ReleaseSelectionState selection)
        {
            if (selection == null)
            {
                return false;
            }

            DynamicDreamSlot dreamSlot = selection.HasDreamSelection
                ? state?.FindActiveDreamSlot(selection.SelectedDreamSlotId)
                : null;
            DynamicOrderSlot orderSlot = selection.HasOrderSelection
                ? state?.FindActiveOrderSlot(selection.SelectedOrderSlotId)
                : null;
            return CanSubmitDreamToOrder(state, dreamSlot, orderSlot);
        }

        public static bool CanDreamMatchSelectedOrder(
            DynamicRoundState state,
            ReleaseSelectionState selection,
            DynamicDreamSlot dreamSlot)
        {
            DynamicOrderSlot orderSlot = selection != null && selection.HasOrderSelection
                ? state?.FindActiveOrderSlot(selection.SelectedOrderSlotId)
                : null;
            return CanSubmitDreamToOrder(state, dreamSlot, orderSlot);
        }

        public static bool CanSelectedDreamMatchOrder(
            DynamicRoundState state,
            ReleaseSelectionState selection,
            DynamicOrderSlot orderSlot)
        {
            DynamicDreamSlot dreamSlot = selection != null && selection.HasDreamSelection
                ? state?.FindActiveDreamSlot(selection.SelectedDreamSlotId)
                : null;
            return CanSubmitDreamToOrder(state, dreamSlot, orderSlot);
        }

        public static bool CanStoreSelection(
            DynamicRoundState state,
            ReleaseSelectionState selection,
            DynamicStorageSlot storageSlot)
        {
            if (state == null
                || selection == null
                || state.Status != DynamicRoundStatus.Playing
                || !selection.HasDreamSelection
                || storageSlot == null
                || !storageSlot.IsEmpty)
            {
                return false;
            }

            DynamicDreamSlot dreamSlot = state.FindActiveDreamSlot(selection.SelectedDreamSlotId);
            if (dreamSlot == null || dreamSlot.IsEmpty)
            {
                return false;
            }

            return DynamicModifierPipeline.CanApplyAction(
                state,
                DynamicPlayerAction.StoreDream(dreamSlot.SlotId, storageSlot.SlotId)).Success;
        }

        public static bool ShouldRenderRecallRow(
            DynamicRoundState state,
            ReleaseSelectionState selection)
        {
            if (state == null
                || selection == null
                || state.Status != DynamicRoundStatus.Playing
                || !selection.HasStorageSelection)
            {
                return false;
            }

            DynamicStorageSlot selectedStorage = state.FindStorageSlot(selection.SelectedStorageSlotId);
            if (selectedStorage == null || selectedStorage.IsEmpty)
            {
                return false;
            }

            for (int i = 0; i < state.ActiveDreams.Count; i++)
            {
                if (CanRecallSelection(state, selection, state.ActiveDreams[i]))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool CanRecallSelection(
            DynamicRoundState state,
            ReleaseSelectionState selection,
            DynamicDreamSlot activeDreamSlot)
        {
            if (state == null
                || selection == null
                || state.Status != DynamicRoundStatus.Playing
                || !selection.HasStorageSelection
                || activeDreamSlot == null
                || !activeDreamSlot.IsEmpty)
            {
                return false;
            }

            DynamicStorageSlot storageSlot = state.FindStorageSlot(selection.SelectedStorageSlotId);
            if (storageSlot == null || storageSlot.IsEmpty)
            {
                return false;
            }

            return DynamicModifierPipeline.CanApplyAction(
                state,
                DynamicPlayerAction.RecallDream(storageSlot.SlotId, activeDreamSlot.SlotId)).Success;
        }

        public static bool HasUsableItemAction(
            DynamicRoundState state,
            string modifierId,
            int targetId)
        {
            if (state == null)
            {
                return false;
            }

            List<DynamicPlayerAction> actions = DynamicModifierPipeline.EnumerateExtraActions(state);
            for (int i = 0; i < actions.Count; i++)
            {
                if (actions[i].Type == DynamicActionType.UseItem
                    && actions[i].ModifierId == modifierId
                    && actions[i].ModifierTargetId == targetId)
                {
                    return true;
                }
            }

            return false;
        }

        public static string BuildSubmitLabel(ReleaseSelectionState selection, bool canSubmit)
        {
            if (canSubmit)
            {
                return "Ready";
            }

            return selection == null || !selection.HasDreamSelection || !selection.HasOrderSelection
                ? "Pick"
                : "No";
        }

        public static bool CanSubmitDreamToOrder(
            DynamicRoundState state,
            DynamicDreamSlot dreamSlot,
            DynamicOrderSlot orderSlot)
        {
            if (state == null
                || state.Status != DynamicRoundStatus.Playing
                || dreamSlot == null
                || dreamSlot.IsEmpty
                || orderSlot == null
                || orderSlot.IsEmpty
                || !orderSlot.Order.CanAccept(dreamSlot.Dream.Attributes))
            {
                return false;
            }

            return DynamicModifierPipeline.CanApplyAction(
                state,
                DynamicPlayerAction.SubmitDream(dreamSlot.SlotId, orderSlot.SlotId)).Success;
        }
    }
}
