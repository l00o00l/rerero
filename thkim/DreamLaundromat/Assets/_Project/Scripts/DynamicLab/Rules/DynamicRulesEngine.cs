namespace Thkim.DreamLaundromat.DynamicLab
{
    public static class DynamicRulesEngine
    {
        public static DynamicActionResult Apply(DynamicRoundState state, DynamicPlayerAction action)
        {
            if (state == null)
            {
                return DynamicActionResult.Failed("Round state is missing.");
            }

            if (state.Status != DynamicRoundStatus.Playing)
            {
                return DynamicActionResult.Failed("Round is not playing.", state.FailureReason);
            }

            if (state.RemainingMoves <= 0)
            {
                MarkFailed(state, DynamicFailureReason.NoMovesRemaining);
                return DynamicActionResult.Failed("No moves remain.", DynamicFailureReason.NoMovesRemaining);
            }

            DynamicActionResult canApply = DynamicModifierPipeline.CanApplyAction(state, action);
            if (!canApply.Success)
            {
                return canApply;
            }

            bool consumesMove = DynamicModifierPipeline.ConsumesMove(state, action);
            DynamicActionResult result;
            if (action.Type == DynamicActionType.UseItem)
            {
                result = DynamicModifierPipeline.ResolveManualAction(state, action);
                if (result.Success && consumesMove)
                {
                    DynamicActionResult afterAction = DynamicModifierPipeline.AfterAction(state, action);
                    if (!afterAction.Success)
                    {
                        return afterAction;
                    }
                }
            }
            else
            {
                DynamicActionResult beforeAction = DynamicModifierPipeline.BeforeAction(state, action);
                if (!beforeAction.Success)
                {
                    return beforeAction;
                }

                result = action.Type switch
                {
                    DynamicActionType.ApplyOperation => ApplyOperation(state, action.ActiveDreamSlotId, action.Operation),
                    DynamicActionType.SubmitDream => SubmitDream(state, action.ActiveDreamSlotId, action.ActiveOrderSlotId),
                    DynamicActionType.StoreDream => StoreDream(state, action.ActiveDreamSlotId, action.StorageSlotId),
                    DynamicActionType.RecallDream => RecallDream(state, action.StorageSlotId, action.ActiveDreamSlotId),
                    _ => DynamicActionResult.Failed("Unknown action.")
                };

                if (!result.Success)
                {
                    return result;
                }

                DynamicActionResult afterAction = DynamicModifierPipeline.AfterAction(state, action);
                if (!afterAction.Success)
                {
                    return afterAction;
                }
            }

            if (!result.Success)
            {
                return result;
            }

            if (consumesMove)
            {
                state.RemainingMoves--;
            }

            RefreshStatus(state);
            return result;
        }

        public static bool CanApplyOperation(DynamicDreamAttributes attributes, DynamicOperation operation)
        {
            return operation switch
            {
                DynamicOperation.Wash => attributes.Taint == DreamTaint.Nightmare,
                DynamicOperation.Soothe => attributes.Mood == DreamMood.Anxious,
                DynamicOperation.Clarify => attributes.Clarity == DreamClarity.Blurry,
                DynamicOperation.Settle => attributes.Stability == DreamStability.Unsettled,
                _ => false
            };
        }

        private static DynamicActionResult ApplyOperation(
            DynamicRoundState state,
            int activeDreamSlotId,
            DynamicOperation operation)
        {
            if (!state.IsOperationAllowed(operation))
            {
                return DynamicActionResult.Failed("Operation is not enabled.");
            }

            DynamicDreamSlot slot = state.FindActiveDreamSlot(activeDreamSlotId);
            if (slot == null || slot.IsEmpty)
            {
                return DynamicActionResult.Failed("Active dream slot is empty.");
            }

            DynamicDreamAttributes attributes = slot.Dream.Attributes;
            if (!CanApplyOperation(attributes, operation))
            {
                return DynamicActionResult.Failed("Operation cannot be applied to this dream.");
            }

            slot.Dream.Attributes = operation switch
            {
                DynamicOperation.Wash => attributes
                    .WithTaint(DreamTaint.Clean)
                    .WithStability(DreamStability.Unsettled),
                DynamicOperation.Soothe => attributes.WithMood(DreamMood.Calm),
                DynamicOperation.Clarify => attributes.WithClarity(DreamClarity.Vivid),
                DynamicOperation.Settle => attributes.WithStability(DreamStability.Stable),
                _ => attributes
            };

            return DynamicActionResult.Succeeded("Operation applied.");
        }

        private static DynamicActionResult SubmitDream(
            DynamicRoundState state,
            int activeDreamSlotId,
            int activeOrderSlotId)
        {
            DynamicDreamSlot dreamSlot = state.FindActiveDreamSlot(activeDreamSlotId);
            if (dreamSlot == null || dreamSlot.IsEmpty)
            {
                return DynamicActionResult.Failed("Active dream slot is empty.");
            }

            DynamicOrderSlot orderSlot = state.FindActiveOrderSlot(activeOrderSlotId);
            if (orderSlot == null || orderSlot.IsEmpty)
            {
                return DynamicActionResult.Failed("Active order slot is empty.");
            }

            if (!orderSlot.Order.TrySubmit(dreamSlot.Dream.Attributes))
            {
                return DynamicActionResult.Failed("Dream does not satisfy the order.");
            }

            dreamSlot.Dream = null;
            if (orderSlot.Order.IsComplete)
            {
                state.CompletedOrders++;
                orderSlot.Order = null;
            }

            if (state.CompletedOrders < state.TargetCompletedOrders)
            {
                DynamicRoundStreams.FillDreamPreview(state);
                DynamicRoundStreams.FillActiveDreamSlots(state);
                DynamicRoundStreams.FillOrderPreview(state);
                DynamicRoundStreams.FillActiveOrderSlots(state);
            }

            return DynamicActionResult.Succeeded("Dream submitted.");
        }

        private static DynamicActionResult StoreDream(
            DynamicRoundState state,
            int activeDreamSlotId,
            int storageSlotId)
        {
            DynamicDreamSlot dreamSlot = state.FindActiveDreamSlot(activeDreamSlotId);
            if (dreamSlot == null || dreamSlot.IsEmpty)
            {
                return DynamicActionResult.Failed("Active dream slot is empty.");
            }

            DynamicStorageSlot storageSlot = state.FindStorageSlot(storageSlotId);
            if (storageSlot == null)
            {
                return DynamicActionResult.Failed("Storage slot does not exist.");
            }

            if (!storageSlot.IsEmpty)
            {
                return DynamicActionResult.Failed("Storage slot is occupied.");
            }

            storageSlot.Dream = dreamSlot.Dream;
            dreamSlot.Dream = null;
            DynamicRoundStreams.FillDreamPreview(state);
            DynamicRoundStreams.FillActiveDreamSlots(state);
            return DynamicActionResult.Succeeded("Dream stored.");
        }

        private static DynamicActionResult RecallDream(
            DynamicRoundState state,
            int storageSlotId,
            int activeDreamSlotId)
        {
            DynamicStorageSlot storageSlot = state.FindStorageSlot(storageSlotId);
            if (storageSlot == null || storageSlot.IsEmpty)
            {
                return DynamicActionResult.Failed("Storage slot is empty.");
            }

            DynamicDreamSlot dreamSlot = state.FindActiveDreamSlot(activeDreamSlotId);
            if (dreamSlot == null)
            {
                return DynamicActionResult.Failed("Active dream slot does not exist.");
            }

            if (!dreamSlot.IsEmpty)
            {
                return DynamicActionResult.Failed("Active dream slot is occupied.");
            }

            dreamSlot.Dream = storageSlot.Dream;
            storageSlot.Dream = null;
            return DynamicActionResult.Succeeded("Dream recalled.");
        }

        private static void RefreshStatus(DynamicRoundState state)
        {
            if (state.CompletedOrders >= state.TargetCompletedOrders)
            {
                state.Status = DynamicRoundStatus.Cleared;
                state.FailureReason = DynamicFailureReason.None;
                return;
            }

            if (state.RemainingMoves <= 0)
            {
                MarkFailed(state, DynamicFailureReason.NoMovesRemaining);
                return;
            }

            if (!HasAnyDreamAvailable(state))
            {
                MarkFailed(state, DynamicFailureReason.NoDreamsAvailable);
                return;
            }

            if (!HasAnyValidAction(state))
            {
                MarkFailed(state, DynamicFailureReason.NoValidActions);
            }
        }

        private static bool HasAnyDreamAvailable(DynamicRoundState state)
        {
            for (int i = 0; i < state.ActiveDreams.Count; i++)
            {
                if (!state.ActiveDreams[i].IsEmpty)
                {
                    return true;
                }
            }

            for (int i = 0; i < state.StorageSlots.Count; i++)
            {
                if (!state.StorageSlots[i].IsEmpty)
                {
                    return true;
                }
            }

            return state.DreamPreview.Count > 0 || state.NextDreamIndex < state.DreamDrawPile.Count;
        }

        private static bool HasAnyValidAction(DynamicRoundState state)
        {
            return DynamicActionEnumerator.Enumerate(state).Count > 0;
        }

        private static void MarkFailed(DynamicRoundState state, DynamicFailureReason reason)
        {
            state.Status = DynamicRoundStatus.Failed;
            state.FailureReason = reason;
        }
    }
}
