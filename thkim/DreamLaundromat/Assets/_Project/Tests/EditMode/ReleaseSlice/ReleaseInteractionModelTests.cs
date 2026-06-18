using NUnit.Framework;
using Thkim.DreamLaundromat.DynamicLab;
using Thkim.DreamLaundromat.Gameplay.ReleaseSlice;

namespace Thkim.DreamLaundromat.Tests.EditMode.ReleaseSlice
{
    public sealed class ReleaseInteractionModelTests
    {
        [Test]
        public void SelectionState_TogglesDreamAndClearsStorage()
        {
            var selection = new ReleaseSelectionState();

            selection.SelectStorage(0);
            selection.SelectDream(1);

            Assert.That(selection.SelectedDreamSlotId, Is.EqualTo(1));
            Assert.That(selection.SelectedStorageSlotId, Is.EqualTo(ReleaseSelectionState.NoSlot));

            selection.SelectDream(1);

            Assert.That(selection.HasDreamSelection, Is.False);
        }

        [Test]
        public void SelectionState_TogglesOrderWithoutClearingDream()
        {
            var selection = new ReleaseSelectionState();

            selection.SelectDream(0);
            selection.SelectOrder(1);
            selection.SelectOrder(1);

            Assert.That(selection.SelectedDreamSlotId, Is.EqualTo(0));
            Assert.That(selection.HasOrderSelection, Is.False);
        }

        [Test]
        public void ActionAvailability_DetectsOperationPreviewCandidate()
        {
            DynamicRoundState state = DynamicRoundInitializer.CreateInitialState(
                DynamicSampleRounds.CreateOperationOrderingRound());
            DynamicDreamSlot slot = FindFirstOperableDream(state, out DynamicOperation operation);
            var selection = new ReleaseSelectionState();

            selection.SelectDream(slot.SlotId);

            Assert.That(
                ReleaseActionAvailability.CanApplySelectedOperation(state, selection, operation),
                Is.True);
        }

        [Test]
        public void ActionAvailability_DetectsSubmitReadinessAndLabel()
        {
            DynamicRoundState state = DynamicRoundInitializer.CreateInitialState(
                DynamicSampleRounds.CreateStateAssignmentRound());
            FindFirstCompatibleSubmit(state, out DynamicDreamSlot dreamSlot, out DynamicOrderSlot orderSlot);
            var selection = new ReleaseSelectionState();

            selection.SelectDream(dreamSlot.SlotId);
            selection.SelectOrder(orderSlot.SlotId);

            bool canSubmit = ReleaseActionAvailability.CanSubmitSelection(state, selection);
            Assert.That(canSubmit, Is.True);
            Assert.That(ReleaseActionAvailability.BuildSubmitLabel(selection, canSubmit), Is.EqualTo("Ready"));
        }

        [Test]
        public void ActionAvailability_SeparatesStoreAndRecallCandidates()
        {
            DynamicRoundState state = DynamicRoundInitializer.CreateInitialState(
                DynamicSampleRounds.CreateStoragePressureRound());
            DynamicDreamSlot dreamSlot = FindFirstOccupiedDream(state);
            DynamicStorageSlot storageSlot = FindFirstEmptyStorage(state);
            var selection = new ReleaseSelectionState();

            selection.SelectDream(dreamSlot.SlotId);
            Assert.That(
                ReleaseActionAvailability.CanStoreSelection(state, selection, storageSlot),
                Is.True);

            DynamicActionResult result = DynamicRulesEngine.Apply(
                state,
                DynamicPlayerAction.StoreDream(dreamSlot.SlotId, storageSlot.SlotId));
            Assert.That(result.Success, Is.True, result.Message);

            DynamicRoundState recallState = DynamicRoundInitializer.CreateInitialState(
                DynamicSampleRounds.CreateStateAssignmentRound());
            recallState.StorageSlots[0].Dream = recallState.ActiveDreams[0].Dream;
            recallState.ActiveDreams[0].Dream = null;

            selection.ClearAll();
            selection.SelectStorage(recallState.StorageSlots[0].SlotId);
            Assert.That(ReleaseActionAvailability.ShouldRenderRecallRow(recallState, selection), Is.True);
            Assert.That(
                ReleaseActionAvailability.CanRecallSelection(recallState, selection, recallState.ActiveDreams[0]),
                Is.True);
        }

        [Test]
        public void GameplayViewModel_BuildsSubmitAndFocusState()
        {
            DynamicRoundState state = DynamicRoundInitializer.CreateInitialState(
                DynamicSampleRounds.CreateStateAssignmentRound());
            FindFirstCompatibleSubmit(state, out DynamicDreamSlot dreamSlot, out DynamicOrderSlot orderSlot);
            var selection = new ReleaseSelectionState();

            selection.SelectDream(dreamSlot.SlotId);
            selection.SelectOrder(orderSlot.SlotId);
            ReleaseGameplayViewModel viewModel = ReleaseGameplayViewModel.Create(state, selection);

            Assert.That(viewModel.CanSubmit, Is.True);
            Assert.That(viewModel.SubmitLabel, Is.EqualTo("Ready"));
            Assert.That(viewModel.FocusText, Does.Contain($"Dream {dreamSlot.SlotId + 1}"));
            Assert.That(viewModel.FocusText, Does.Contain($"Order {orderSlot.SlotId + 1}"));
            Assert.That(viewModel.FocusText, Does.Contain("Ready"));
            Assert.That(FindDreamCard(viewModel, dreamSlot.SlotId).IsSelected, Is.True);
            Assert.That(FindOrderCard(viewModel, orderSlot.SlotId).IsSelected, Is.True);
        }

        [Test]
        public void GameplayViewModel_ExposesOperationPreview()
        {
            DynamicRoundState state = DynamicRoundInitializer.CreateInitialState(
                DynamicSampleRounds.CreateOperationOrderingRound());
            DynamicDreamSlot slot = FindFirstOperableDream(state, out DynamicOperation operation);
            var selection = new ReleaseSelectionState();

            selection.SelectDream(slot.SlotId);
            ReleaseGameplayViewModel viewModel = ReleaseGameplayViewModel.Create(state, selection);
            ReleaseOperationActionViewModel option = FindOperation(viewModel, operation);

            Assert.That(option.IsInteractable, Is.True);
            Assert.That(option.HasPreview, Is.True);
            Assert.That(
                option.PreviewAttributes,
                Is.EqualTo(ReleaseGameplayCardRenderer.PreviewOperation(slot.Dream.Attributes, operation)));
        }

        [Test]
        public void GameplayViewModel_HighlightsStoreAndRecallTargets()
        {
            DynamicRoundState state = DynamicRoundInitializer.CreateInitialState(
                DynamicSampleRounds.CreateStoragePressureRound());
            DynamicDreamSlot dreamSlot = FindFirstOccupiedDream(state);
            DynamicStorageSlot storageSlot = FindFirstEmptyStorage(state);
            var selection = new ReleaseSelectionState();

            selection.SelectDream(dreamSlot.SlotId);
            ReleaseGameplayViewModel storeViewModel = ReleaseGameplayViewModel.Create(state, selection);

            Assert.That(
                FindStorageCard(storeViewModel, storageSlot.SlotId).CanStoreSelectedDream,
                Is.True);
            Assert.That(
                FindStoreAction(storeViewModel, storageSlot.SlotId).IsInteractable,
                Is.True);

            DynamicRoundState recallState = DynamicRoundInitializer.CreateInitialState(
                DynamicSampleRounds.CreateStateAssignmentRound());
            recallState.StorageSlots[0].Dream = recallState.ActiveDreams[0].Dream;
            recallState.ActiveDreams[0].Dream = null;

            selection.ClearAll();
            selection.SelectStorage(recallState.StorageSlots[0].SlotId);
            ReleaseGameplayViewModel recallViewModel = ReleaseGameplayViewModel.Create(recallState, selection);

            Assert.That(recallViewModel.ShouldRenderRecallRow, Is.True);
            Assert.That(
                FindDreamCard(recallViewModel, recallState.ActiveDreams[0].SlotId).CanRecallSelectedStorage,
                Is.True);
            Assert.That(
                FindRecallAction(recallViewModel, recallState.ActiveDreams[0].SlotId).IsInteractable,
                Is.True);
        }

        [Test]
        public void DragActionResolver_ResolvesDreamToOrderSubmit()
        {
            DynamicRoundState state = DynamicRoundInitializer.CreateInitialState(
                DynamicSampleRounds.CreateStateAssignmentRound());
            FindFirstCompatibleSubmit(state, out DynamicDreamSlot dreamSlot, out DynamicOrderSlot orderSlot);

            ReleaseDragActionResolution resolution = ReleaseDragActionResolver.Resolve(
                state,
                new ReleaseDragPayload(ReleaseDragSourceKind.ActiveDream, dreamSlot.SlotId),
                new ReleaseDropTargetDescriptor(ReleaseDropTargetKind.ActiveOrder, orderSlot.SlotId));

            Assert.That(resolution.Success, Is.True, resolution.Message);
            Assert.That(resolution.Action.Type, Is.EqualTo(DynamicActionType.SubmitDream));
            Assert.That(resolution.Action.ActiveDreamSlotId, Is.EqualTo(dreamSlot.SlotId));
            Assert.That(resolution.Action.ActiveOrderSlotId, Is.EqualTo(orderSlot.SlotId));
        }

        [Test]
        public void DragActionResolver_ResolvesDreamToStorage()
        {
            DynamicRoundState state = DynamicRoundInitializer.CreateInitialState(
                DynamicSampleRounds.CreateStoragePressureRound());
            DynamicDreamSlot dreamSlot = FindFirstOccupiedDream(state);
            DynamicStorageSlot storageSlot = FindFirstEmptyStorage(state);

            ReleaseDragActionResolution resolution = ReleaseDragActionResolver.Resolve(
                state,
                new ReleaseDragPayload(ReleaseDragSourceKind.ActiveDream, dreamSlot.SlotId),
                new ReleaseDropTargetDescriptor(ReleaseDropTargetKind.Storage, storageSlot.SlotId));

            Assert.That(resolution.Success, Is.True, resolution.Message);
            Assert.That(resolution.Action.Type, Is.EqualTo(DynamicActionType.StoreDream));
            Assert.That(resolution.Action.ActiveDreamSlotId, Is.EqualTo(dreamSlot.SlotId));
            Assert.That(resolution.Action.StorageSlotId, Is.EqualTo(storageSlot.SlotId));
        }

        [Test]
        public void DragActionResolver_ResolvesStorageToEmptyDreamSlot()
        {
            DynamicRoundState state = DynamicRoundInitializer.CreateInitialState(
                DynamicSampleRounds.CreateStateAssignmentRound());
            state.StorageSlots[0].Dream = state.ActiveDreams[0].Dream;
            state.ActiveDreams[0].Dream = null;

            ReleaseDragActionResolution resolution = ReleaseDragActionResolver.Resolve(
                state,
                new ReleaseDragPayload(ReleaseDragSourceKind.Storage, state.StorageSlots[0].SlotId),
                new ReleaseDropTargetDescriptor(ReleaseDropTargetKind.ActiveDream, state.ActiveDreams[0].SlotId));

            Assert.That(resolution.Success, Is.True, resolution.Message);
            Assert.That(resolution.Action.Type, Is.EqualTo(DynamicActionType.RecallDream));
            Assert.That(resolution.Action.StorageSlotId, Is.EqualTo(state.StorageSlots[0].SlotId));
            Assert.That(resolution.Action.ActiveDreamSlotId, Is.EqualTo(state.ActiveDreams[0].SlotId));
        }

        [Test]
        public void DragActionResolver_RejectsUnsupportedDropPair()
        {
            DynamicRoundState state = DynamicRoundInitializer.CreateInitialState(
                DynamicSampleRounds.CreateStateAssignmentRound());

            ReleaseDragActionResolution resolution = ReleaseDragActionResolver.Resolve(
                state,
                new ReleaseDragPayload(ReleaseDragSourceKind.ActiveDream, 0),
                new ReleaseDropTargetDescriptor(ReleaseDropTargetKind.ActiveDream, 1));

            Assert.That(resolution.Success, Is.False);
            Assert.That(resolution.Message, Is.Not.Empty);
        }

        private static DynamicDreamSlot FindFirstOperableDream(
            DynamicRoundState state,
            out DynamicOperation operation)
        {
            for (int dreamIndex = 0; dreamIndex < state.ActiveDreams.Count; dreamIndex++)
            {
                DynamicDreamSlot slot = state.ActiveDreams[dreamIndex];
                if (slot.IsEmpty)
                {
                    continue;
                }

                for (int operationIndex = 0; operationIndex < state.ActionSet.Length; operationIndex++)
                {
                    operation = state.ActionSet[operationIndex];
                    if (DynamicRulesEngine.CanApplyOperation(slot.Dream.Attributes, operation))
                    {
                        return slot;
                    }
                }
            }

            Assert.Fail("Expected at least one operable active dream.");
            operation = default;
            return null;
        }

        private static void FindFirstCompatibleSubmit(
            DynamicRoundState state,
            out DynamicDreamSlot dreamSlot,
            out DynamicOrderSlot orderSlot)
        {
            for (int dreamIndex = 0; dreamIndex < state.ActiveDreams.Count; dreamIndex++)
            {
                DynamicDreamSlot candidateDream = state.ActiveDreams[dreamIndex];
                if (candidateDream.IsEmpty)
                {
                    continue;
                }

                for (int orderIndex = 0; orderIndex < state.ActiveOrders.Count; orderIndex++)
                {
                    DynamicOrderSlot candidateOrder = state.ActiveOrders[orderIndex];
                    if (!candidateOrder.IsEmpty
                        && ReleaseActionAvailability.CanSubmitDreamToOrder(state, candidateDream, candidateOrder))
                    {
                        dreamSlot = candidateDream;
                        orderSlot = candidateOrder;
                        return;
                    }
                }
            }

            Assert.Fail("Expected a compatible active dream and order.");
            dreamSlot = null;
            orderSlot = null;
        }

        private static DynamicDreamSlot FindFirstOccupiedDream(DynamicRoundState state)
        {
            for (int i = 0; i < state.ActiveDreams.Count; i++)
            {
                if (!state.ActiveDreams[i].IsEmpty)
                {
                    return state.ActiveDreams[i];
                }
            }

            Assert.Fail("Expected an occupied active dream slot.");
            return null;
        }

        private static DynamicStorageSlot FindFirstEmptyStorage(DynamicRoundState state)
        {
            for (int i = 0; i < state.StorageSlots.Count; i++)
            {
                if (state.StorageSlots[i].IsEmpty)
                {
                    return state.StorageSlots[i];
                }
            }

            Assert.Fail("Expected an empty storage slot.");
            return null;
        }

        private static ReleaseDreamSlotViewModel FindDreamCard(
            ReleaseGameplayViewModel viewModel,
            int slotId)
        {
            for (int i = 0; i < viewModel.Dreams.Count; i++)
            {
                if (viewModel.Dreams[i].SlotId == slotId)
                {
                    return viewModel.Dreams[i];
                }
            }

            Assert.Fail($"Expected dream card D{slotId}.");
            return default;
        }

        private static ReleaseOrderSlotViewModel FindOrderCard(
            ReleaseGameplayViewModel viewModel,
            int slotId)
        {
            for (int i = 0; i < viewModel.Orders.Count; i++)
            {
                if (viewModel.Orders[i].SlotId == slotId)
                {
                    return viewModel.Orders[i];
                }
            }

            Assert.Fail($"Expected order card O{slotId}.");
            return default;
        }

        private static ReleaseStorageSlotViewModel FindStorageCard(
            ReleaseGameplayViewModel viewModel,
            int slotId)
        {
            for (int i = 0; i < viewModel.StorageSlots.Count; i++)
            {
                if (viewModel.StorageSlots[i].SlotId == slotId)
                {
                    return viewModel.StorageSlots[i];
                }
            }

            Assert.Fail($"Expected storage card S{slotId}.");
            return default;
        }

        private static ReleaseOperationActionViewModel FindOperation(
            ReleaseGameplayViewModel viewModel,
            DynamicOperation operation)
        {
            for (int i = 0; i < viewModel.Operations.Count; i++)
            {
                if (viewModel.Operations[i].Operation == operation)
                {
                    return viewModel.Operations[i];
                }
            }

            Assert.Fail($"Expected operation option {operation}.");
            return default;
        }

        private static ReleaseStoreActionViewModel FindStoreAction(
            ReleaseGameplayViewModel viewModel,
            int storageSlotId)
        {
            for (int i = 0; i < viewModel.StoreActions.Count; i++)
            {
                if (viewModel.StoreActions[i].StorageSlotId == storageSlotId)
                {
                    return viewModel.StoreActions[i];
                }
            }

            Assert.Fail($"Expected store action S{storageSlotId}.");
            return default;
        }

        private static ReleaseRecallActionViewModel FindRecallAction(
            ReleaseGameplayViewModel viewModel,
            int activeDreamSlotId)
        {
            for (int i = 0; i < viewModel.RecallActions.Count; i++)
            {
                if (viewModel.RecallActions[i].ActiveDreamSlotId == activeDreamSlotId)
                {
                    return viewModel.RecallActions[i];
                }
            }

            Assert.Fail($"Expected recall action D{activeDreamSlotId}.");
            return default;
        }
    }
}
