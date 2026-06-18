using System;
using System.Collections.Generic;
using Thkim.DreamLaundromat.DynamicLab;

namespace Thkim.DreamLaundromat.Gameplay.ReleaseSlice
{
    public sealed class ReleaseGameplayViewModel
    {
        private readonly List<ReleaseDreamSlotViewModel> dreams;
        private readonly List<ReleaseOrderSlotViewModel> orders;
        private readonly List<ReleaseStorageSlotViewModel> storageSlots;
        private readonly List<ReleaseOperationActionViewModel> operations;
        private readonly List<ReleaseModifierActionViewModel> modifiers;
        private readonly List<ReleaseStoreActionViewModel> storeActions;
        private readonly List<ReleaseRecallActionViewModel> recallActions;

        private ReleaseGameplayViewModel(
            DynamicRoundState state,
            int selectedDreamSlotId,
            int selectedOrderSlotId,
            int selectedStorageSlotId,
            List<ReleaseDreamSlotViewModel> dreams,
            List<ReleaseOrderSlotViewModel> orders,
            List<ReleaseStorageSlotViewModel> storageSlots,
            List<ReleaseOperationActionViewModel> operations,
            List<ReleaseModifierActionViewModel> modifiers,
            List<ReleaseStoreActionViewModel> storeActions,
            List<ReleaseRecallActionViewModel> recallActions,
            bool canSubmit,
            string submitLabel,
            bool shouldRenderRecallRow,
            string focusText)
        {
            State = state;
            SelectedDreamSlotId = selectedDreamSlotId;
            SelectedOrderSlotId = selectedOrderSlotId;
            SelectedStorageSlotId = selectedStorageSlotId;
            this.dreams = dreams;
            this.orders = orders;
            this.storageSlots = storageSlots;
            this.operations = operations;
            this.modifiers = modifiers;
            this.storeActions = storeActions;
            this.recallActions = recallActions;
            CanSubmit = canSubmit;
            SubmitLabel = submitLabel;
            ShouldRenderRecallRow = shouldRenderRecallRow;
            FocusText = focusText;
        }

        public DynamicRoundState State { get; }
        public int SelectedDreamSlotId { get; }
        public int SelectedOrderSlotId { get; }
        public int SelectedStorageSlotId { get; }
        public IReadOnlyList<ReleaseDreamSlotViewModel> Dreams => dreams;
        public IReadOnlyList<ReleaseOrderSlotViewModel> Orders => orders;
        public IReadOnlyList<ReleaseStorageSlotViewModel> StorageSlots => storageSlots;
        public IReadOnlyList<ReleaseOperationActionViewModel> Operations => operations;
        public IReadOnlyList<ReleaseModifierActionViewModel> Modifiers => modifiers;
        public IReadOnlyList<ReleaseStoreActionViewModel> StoreActions => storeActions;
        public IReadOnlyList<ReleaseRecallActionViewModel> RecallActions => recallActions;
        public bool CanSubmit { get; }
        public string SubmitLabel { get; }
        public bool ShouldRenderRecallRow { get; }
        public string FocusText { get; }

        public static ReleaseGameplayViewModel Create(
            DynamicRoundState state,
            ReleaseSelectionState selection)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            int selectedDreamSlotId = selection?.SelectedDreamSlotId ?? ReleaseSelectionState.NoSlot;
            int selectedOrderSlotId = selection?.SelectedOrderSlotId ?? ReleaseSelectionState.NoSlot;
            int selectedStorageSlotId = selection?.SelectedStorageSlotId ?? ReleaseSelectionState.NoSlot;
            bool canSubmit = ReleaseActionAvailability.CanSubmitSelection(state, selection);
            bool shouldRenderRecallRow = ReleaseActionAvailability.ShouldRenderRecallRow(state, selection);

            var dreams = new List<ReleaseDreamSlotViewModel>(state.ActiveDreams.Count);
            for (int i = 0; i < state.ActiveDreams.Count; i++)
            {
                DynamicDreamSlot slot = state.ActiveDreams[i];
                bool selected = selectedDreamSlotId == slot.SlotId;
                dreams.Add(new ReleaseDreamSlotViewModel(
                    slot,
                    selected,
                    ReleaseGameplayCardRenderer.IsDreamSlotLocked(state, slot.SlotId),
                    !selected && ReleaseActionAvailability.CanDreamMatchSelectedOrder(state, selection, slot),
                    ReleaseActionAvailability.CanRecallSelection(state, selection, slot)));
            }

            var orders = new List<ReleaseOrderSlotViewModel>(state.ActiveOrders.Count);
            for (int i = 0; i < state.ActiveOrders.Count; i++)
            {
                DynamicOrderSlot slot = state.ActiveOrders[i];
                bool selected = selectedOrderSlotId == slot.SlotId;
                orders.Add(new ReleaseOrderSlotViewModel(
                    slot,
                    selected,
                    !selected && ReleaseActionAvailability.CanSelectedDreamMatchOrder(state, selection, slot)));
            }

            var storageSlots = new List<ReleaseStorageSlotViewModel>(state.StorageSlots.Count);
            var storeActions = new List<ReleaseStoreActionViewModel>(state.StorageSlots.Count);
            for (int i = 0; i < state.StorageSlots.Count; i++)
            {
                DynamicStorageSlot slot = state.StorageSlots[i];
                bool canStore = ReleaseActionAvailability.CanStoreSelection(state, selection, slot);
                storageSlots.Add(new ReleaseStorageSlotViewModel(
                    slot,
                    selectedStorageSlotId == slot.SlotId,
                    canStore));
                storeActions.Add(new ReleaseStoreActionViewModel(slot.SlotId, canStore));
            }

            var operations = new List<ReleaseOperationActionViewModel>(state.ActionSet.Length);
            for (int i = 0; i < state.ActionSet.Length; i++)
            {
                DynamicOperation operation = state.ActionSet[i];
                bool canApply = ReleaseActionAvailability.CanApplySelectedOperation(state, selection, operation);
                DynamicDreamAttributes preview = default;
                bool hasPreview = false;
                DynamicDreamSlot selectedDreamSlot = selectedDreamSlotId < 0
                    ? null
                    : state.FindActiveDreamSlot(selectedDreamSlotId);
                if (canApply && selectedDreamSlot != null && !selectedDreamSlot.IsEmpty)
                {
                    preview = ReleaseGameplayCardRenderer.PreviewOperation(
                        selectedDreamSlot.Dream.Attributes,
                        operation);
                    hasPreview = true;
                }

                operations.Add(new ReleaseOperationActionViewModel(
                    operation,
                    ReleaseVisualDescriptors.ForOperation(operation),
                    canApply,
                    hasPreview,
                    preview));
            }

            var modifiers = new List<ReleaseModifierActionViewModel>(state.ModifierDefinitions.Length);
            for (int i = 0; i < state.ModifierDefinitions.Length; i++)
            {
                DynamicModifierDefinition definition = state.ModifierDefinitions[i];
                DynamicModifierState modifierState = DynamicModifierPipeline.FindState(state, definition.Id);
                int charges = modifierState?.RemainingCharges ?? 0;
                int targetId = definition.Effect == DynamicModifierEffect.RefreshActiveDream
                    ? selectedDreamSlotId
                    : definition.TargetId;
                bool isInteractable = definition.Type == DynamicModifierType.Item
                    && charges > 0
                    && state.Status == DynamicRoundStatus.Playing
                    && ReleaseActionAvailability.HasUsableItemAction(state, definition.Id, targetId);
                modifiers.Add(new ReleaseModifierActionViewModel(
                    definition,
                    modifierState,
                    targetId,
                    charges,
                    ReleaseGameplayCardRenderer.BuildModifierLabel(definition, modifierState, targetId),
                    isInteractable));
            }

            var recallActions = new List<ReleaseRecallActionViewModel>(state.ActiveDreams.Count);
            for (int i = 0; i < state.ActiveDreams.Count; i++)
            {
                DynamicDreamSlot slot = state.ActiveDreams[i];
                recallActions.Add(new ReleaseRecallActionViewModel(
                    slot.SlotId,
                    ReleaseActionAvailability.CanRecallSelection(state, selection, slot)));
            }

            return new ReleaseGameplayViewModel(
                state,
                selectedDreamSlotId,
                selectedOrderSlotId,
                selectedStorageSlotId,
                dreams,
                orders,
                storageSlots,
                operations,
                modifiers,
                storeActions,
                recallActions,
                canSubmit,
                ReleaseActionAvailability.BuildSubmitLabel(selection, canSubmit),
                shouldRenderRecallRow,
                BuildFocusText(state, selection, canSubmit));
        }

        private static string BuildFocusText(
            DynamicRoundState state,
            ReleaseSelectionState selection,
            bool canSubmit)
        {
            var lines = new List<string>(4);
            int selectedDreamSlotId = selection?.SelectedDreamSlotId ?? ReleaseSelectionState.NoSlot;
            int selectedOrderSlotId = selection?.SelectedOrderSlotId ?? ReleaseSelectionState.NoSlot;
            int selectedStorageSlotId = selection?.SelectedStorageSlotId ?? ReleaseSelectionState.NoSlot;

            DynamicDreamSlot dreamSlot = selectedDreamSlotId < 0 ? null : state.FindActiveDreamSlot(selectedDreamSlotId);
            DynamicOrderSlot orderSlot = selectedOrderSlotId < 0 ? null : state.FindActiveOrderSlot(selectedOrderSlotId);
            DynamicStorageSlot storageSlot = selectedStorageSlotId < 0 ? null : state.FindStorageSlot(selectedStorageSlotId);

            if (dreamSlot != null && !dreamSlot.IsEmpty)
            {
                lines.Add($"D{dreamSlot.SlotId + 1}  {ReleaseVisualDescriptors.DescribeCompact(dreamSlot.Dream.Attributes)}");
            }

            if (orderSlot != null && !orderSlot.IsEmpty)
            {
                lines.Add($"O{orderSlot.SlotId + 1}  {orderSlot.Order.FulfilledCount}/{orderSlot.Order.Requirement.Count} {ReleaseGameplayCardRenderer.DescribeRequirementCompact(orderSlot.Order.Requirement)}");
            }

            if (canSubmit)
            {
                lines.Add("Match");
            }

            if (storageSlot != null && !storageSlot.IsEmpty)
            {
                lines.Add($"S{storageSlot.SlotId + 1}  {ReleaseVisualDescriptors.DescribeCompact(storageSlot.Dream.Attributes)}");
            }

            if (lines.Count == 0)
            {
                lines.Add("Choose dream");
            }

            return string.Join("\n", lines);
        }
    }

    public readonly struct ReleaseDreamSlotViewModel
    {
        public ReleaseDreamSlotViewModel(
            DynamicDreamSlot slot,
            bool isSelected,
            bool isLocked,
            bool canSubmitToSelectedOrder,
            bool canRecallSelectedStorage)
        {
            Slot = slot;
            SlotId = slot?.SlotId ?? ReleaseSelectionState.NoSlot;
            IsSelected = isSelected;
            IsLocked = isLocked;
            CanSubmitToSelectedOrder = canSubmitToSelectedOrder;
            CanRecallSelectedStorage = canRecallSelectedStorage;
        }

        public DynamicDreamSlot Slot { get; }
        public int SlotId { get; }
        public bool IsSelected { get; }
        public bool IsLocked { get; }
        public bool CanSubmitToSelectedOrder { get; }
        public bool CanRecallSelectedStorage { get; }
    }

    public readonly struct ReleaseOrderSlotViewModel
    {
        public ReleaseOrderSlotViewModel(
            DynamicOrderSlot slot,
            bool isSelected,
            bool canAcceptSelectedDream)
        {
            Slot = slot;
            SlotId = slot?.SlotId ?? ReleaseSelectionState.NoSlot;
            IsSelected = isSelected;
            CanAcceptSelectedDream = canAcceptSelectedDream;
        }

        public DynamicOrderSlot Slot { get; }
        public int SlotId { get; }
        public bool IsSelected { get; }
        public bool CanAcceptSelectedDream { get; }
    }

    public readonly struct ReleaseStorageSlotViewModel
    {
        public ReleaseStorageSlotViewModel(
            DynamicStorageSlot slot,
            bool isSelected,
            bool canStoreSelectedDream)
        {
            Slot = slot;
            SlotId = slot?.SlotId ?? ReleaseSelectionState.NoSlot;
            IsSelected = isSelected;
            CanStoreSelectedDream = canStoreSelectedDream;
        }

        public DynamicStorageSlot Slot { get; }
        public int SlotId { get; }
        public bool IsSelected { get; }
        public bool CanStoreSelectedDream { get; }
    }

    public readonly struct ReleaseOperationActionViewModel
    {
        public ReleaseOperationActionViewModel(
            DynamicOperation operation,
            ReleaseActionVisualDescriptor descriptor,
            bool isInteractable,
            bool hasPreview,
            DynamicDreamAttributes previewAttributes)
        {
            Operation = operation;
            Descriptor = descriptor;
            IsInteractable = isInteractable;
            HasPreview = hasPreview;
            PreviewAttributes = previewAttributes;
        }

        public DynamicOperation Operation { get; }
        public ReleaseActionVisualDescriptor Descriptor { get; }
        public bool IsInteractable { get; }
        public bool HasPreview { get; }
        public DynamicDreamAttributes PreviewAttributes { get; }
    }

    public readonly struct ReleaseModifierActionViewModel
    {
        public ReleaseModifierActionViewModel(
            DynamicModifierDefinition definition,
            DynamicModifierState modifierState,
            int targetId,
            int charges,
            string label,
            bool isInteractable)
        {
            Definition = definition;
            ModifierState = modifierState;
            TargetId = targetId;
            Charges = charges;
            Label = label ?? string.Empty;
            IsInteractable = isInteractable;
        }

        public DynamicModifierDefinition Definition { get; }
        public DynamicModifierState ModifierState { get; }
        public int TargetId { get; }
        public int Charges { get; }
        public string Label { get; }
        public bool IsInteractable { get; }
    }

    public readonly struct ReleaseStoreActionViewModel
    {
        public ReleaseStoreActionViewModel(int storageSlotId, bool isInteractable)
        {
            StorageSlotId = storageSlotId;
            IsInteractable = isInteractable;
        }

        public int StorageSlotId { get; }
        public bool IsInteractable { get; }
    }

    public readonly struct ReleaseRecallActionViewModel
    {
        public ReleaseRecallActionViewModel(int activeDreamSlotId, bool isInteractable)
        {
            ActiveDreamSlotId = activeDreamSlotId;
            IsInteractable = isInteractable;
        }

        public int ActiveDreamSlotId { get; }
        public bool IsInteractable { get; }
    }
}
