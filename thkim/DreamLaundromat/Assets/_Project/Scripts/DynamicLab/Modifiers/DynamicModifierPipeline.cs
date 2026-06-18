using System.Collections.Generic;

namespace Thkim.DreamLaundromat.DynamicLab
{
    public static class DynamicModifierPipeline
    {
        public static void InitializeStates(DynamicRoundDefinition definition, DynamicRoundState state)
        {
            if (definition.Modifiers == null || definition.Modifiers.Length == 0)
            {
                return;
            }

            for (int i = 0; i < definition.Modifiers.Length; i++)
            {
                DynamicModifierDefinition modifier = definition.Modifiers[i];
                state.Modifiers.Add(new DynamicModifierState
                {
                    ModifierId = modifier.Id,
                    RemainingCharges = modifier.Charges,
                    BoundTargetKind = modifier.TargetKind,
                    BoundTargetId = modifier.TargetId
                });
            }
        }

        public static List<DynamicPlayerAction> EnumerateExtraActions(DynamicRoundState state)
        {
            var actions = new List<DynamicPlayerAction>();
            for (int i = 0; i < state.ModifierDefinitions.Length; i++)
            {
                DynamicModifierDefinition definition = state.ModifierDefinitions[i];
                if (definition.Type != DynamicModifierType.Item
                    || definition.Trigger != DynamicModifierTrigger.Manual)
                {
                    continue;
                }

                DynamicModifierState modifierState = FindState(state, definition.Id);
                if (definition.Effect == DynamicModifierEffect.RefreshActiveDream)
                {
                    AddDreamRefreshActions(state, actions, definition, modifierState);
                }
                else if (CanResolveManualItem(state, definition, modifierState, definition.TargetId).Success)
                {
                    actions.Add(DynamicPlayerAction.UseItem(definition.Id, definition.TargetId));
                }
            }

            return actions;
        }

        public static DynamicActionResult CanApplyAction(DynamicRoundState state, DynamicPlayerAction action)
        {
            for (int i = 0; i < state.ModifierDefinitions.Length; i++)
            {
                DynamicModifierDefinition definition = state.ModifierDefinitions[i];
                if (definition.Type != DynamicModifierType.Obstacle
                    || definition.Trigger != DynamicModifierTrigger.CanApplyAction)
                {
                    continue;
                }

                DynamicModifierState modifierState = FindState(state, definition.Id);
                if (modifierState == null || modifierState.IsResolved)
                {
                    continue;
                }

                if (definition.Effect == DynamicModifierEffect.LockActiveDreamSlot
                    && BlocksActiveDreamSlot(modifierState, action))
                {
                    return DynamicActionResult.Failed(
                        $"Active dream slot {modifierState.BoundTargetId} is locked.");
                }

                if (definition.Effect == DynamicModifierEffect.PinOrderSlot
                    && BlocksPinnedOrderSlot(modifierState, action))
                {
                    return DynamicActionResult.Failed(
                        $"Active order slot {modifierState.BoundTargetId} is pinned.");
                }

                if (definition.Effect == DynamicModifierEffect.SoftBlockOperation
                    && BlocksSoftBlockedOperation(modifierState, action))
                {
                    DynamicOperation operation = (DynamicOperation)modifierState.BoundTargetId;
                    return DynamicActionResult.Failed($"{operation} is temporarily blocked.");
                }
            }

            return DynamicActionResult.Succeeded("Action allowed.");
        }

        public static DynamicActionResult ResolveManualAction(DynamicRoundState state, DynamicPlayerAction action)
        {
            DynamicModifierDefinition definition = FindDefinition(state, action.ModifierId);
            if (definition == null)
            {
                return DynamicActionResult.Failed("Item modifier does not exist.");
            }

            if (definition.Type != DynamicModifierType.Item
                || definition.Trigger != DynamicModifierTrigger.Manual)
            {
                return DynamicActionResult.Failed("Modifier is not a usable item.");
            }

            DynamicModifierState modifierState = FindState(state, definition.Id);
            DynamicActionResult canResolve = CanResolveManualItem(
                state,
                definition,
                modifierState,
                action.ModifierTargetId);
            if (!canResolve.Success)
            {
                return canResolve;
            }

            if (definition.Effect == DynamicModifierEffect.PreviewSwap)
            {
                DynamicDreamState first = state.DreamPreview[0];
                state.DreamPreview[0] = state.DreamPreview[1];
                state.DreamPreview[1] = first;
                ConsumeCharge(modifierState);
                return DynamicActionResult.Succeeded("Preview order swapped.");
            }

            if (definition.Effect == DynamicModifierEffect.RefreshActiveDream)
            {
                DynamicDreamSlot slot = state.FindActiveDreamSlot(action.ModifierTargetId);
                DynamicDreamState refreshedDream = slot.Dream;
                slot.Dream = null;
                state.DreamDrawPile.Add(refreshedDream.Clone());
                DynamicRoundStreams.FillDreamPreview(state);
                DynamicRoundStreams.FillActiveDreamSlots(state);
                ConsumeCharge(modifierState);
                return DynamicActionResult.Succeeded("Dream refreshed into the later stream.");
            }

            return DynamicActionResult.Failed("Unsupported item effect.");
        }

        public static DynamicActionResult BeforeAction(DynamicRoundState state, DynamicPlayerAction action)
        {
            return DynamicActionResult.Succeeded("No modifier before-action effects.");
        }

        public static DynamicActionResult AfterAction(DynamicRoundState state, DynamicPlayerAction action)
        {
            AdvanceTimedObstacles(state);
            return DynamicActionResult.Succeeded("Timed modifiers advanced.");
        }

        public static bool ConsumesMove(DynamicRoundState state, DynamicPlayerAction action)
        {
            if (action.Type != DynamicActionType.UseItem)
            {
                return true;
            }

            DynamicModifierDefinition definition = FindDefinition(state, action.ModifierId);
            return definition == null || definition.ConsumesMove;
        }

        public static DynamicModifierDefinition FindDefinition(DynamicRoundState state, string modifierId)
        {
            for (int i = 0; i < state.ModifierDefinitions.Length; i++)
            {
                if (state.ModifierDefinitions[i].Id == modifierId)
                {
                    return state.ModifierDefinitions[i];
                }
            }

            return null;
        }

        public static DynamicModifierState FindState(DynamicRoundState state, string modifierId)
        {
            for (int i = 0; i < state.Modifiers.Count; i++)
            {
                if (state.Modifiers[i].ModifierId == modifierId)
                {
                    return state.Modifiers[i];
                }
            }

            return null;
        }

        private static void AddDreamRefreshActions(
            DynamicRoundState state,
            List<DynamicPlayerAction> actions,
            DynamicModifierDefinition definition,
            DynamicModifierState modifierState)
        {
            for (int i = 0; i < state.ActiveDreams.Count; i++)
            {
                DynamicDreamSlot slot = state.ActiveDreams[i];
                if (!slot.IsEmpty
                    && CanResolveManualItem(state, definition, modifierState, slot.SlotId).Success)
                {
                    actions.Add(DynamicPlayerAction.UseItem(definition.Id, slot.SlotId));
                }
            }
        }

        private static DynamicActionResult CanResolveManualItem(
            DynamicRoundState state,
            DynamicModifierDefinition definition,
            DynamicModifierState modifierState,
            int targetId)
        {
            if (definition == null)
            {
                return DynamicActionResult.Failed("Item modifier does not exist.");
            }

            if (definition.Type != DynamicModifierType.Item
                || definition.Trigger != DynamicModifierTrigger.Manual)
            {
                return DynamicActionResult.Failed("Modifier is not a usable item.");
            }

            if (modifierState == null || !modifierState.HasCharges || modifierState.IsResolved)
            {
                return DynamicActionResult.Failed("Item has no remaining charges.");
            }

            if (definition.Effect == DynamicModifierEffect.PreviewSwap)
            {
                if (state.DreamPreview.Count < 2)
                {
                    return DynamicActionResult.Failed("Preview Swap needs at least two dream previews.");
                }

                return DynamicActionResult.Succeeded("Item can be resolved.");
            }

            if (definition.Effect == DynamicModifierEffect.RefreshActiveDream)
            {
                DynamicDreamSlot slot = state.FindActiveDreamSlot(targetId);
                if (slot == null || slot.IsEmpty)
                {
                    return DynamicActionResult.Failed("Dream Refresh needs an active dream target.");
                }

                if (state.DreamPreview.Count == 0 && state.NextDreamIndex >= state.DreamDrawPile.Count)
                {
                    return DynamicActionResult.Failed("Dream Refresh needs another dream in the stream.");
                }

                return DynamicActionResult.Succeeded("Item can be resolved.");
            }

            return DynamicActionResult.Failed("Unsupported item effect.");
        }

        private static void AdvanceTimedObstacles(DynamicRoundState state)
        {
            for (int i = 0; i < state.ModifierDefinitions.Length; i++)
            {
                DynamicModifierDefinition definition = state.ModifierDefinitions[i];
                if (definition.Type != DynamicModifierType.Obstacle
                    || definition.Trigger != DynamicModifierTrigger.CanApplyAction)
                {
                    continue;
                }

                if (definition.Effect != DynamicModifierEffect.PinOrderSlot
                    && definition.Effect != DynamicModifierEffect.SoftBlockOperation)
                {
                    continue;
                }

                DynamicModifierState modifierState = FindState(state, definition.Id);
                if (modifierState == null || modifierState.IsResolved)
                {
                    continue;
                }

                ConsumeCharge(modifierState);
            }
        }

        private static bool BlocksPinnedOrderSlot(DynamicModifierState modifierState, DynamicPlayerAction action)
        {
            if (modifierState.BoundTargetKind != DynamicModifierTargetKind.OrderSlot)
            {
                return false;
            }

            return action.Type == DynamicActionType.SubmitDream
                && action.ActiveOrderSlotId == modifierState.BoundTargetId;
        }

        private static bool BlocksSoftBlockedOperation(DynamicModifierState modifierState, DynamicPlayerAction action)
        {
            if (modifierState.BoundTargetKind != DynamicModifierTargetKind.Operation)
            {
                return false;
            }

            return action.Type == DynamicActionType.ApplyOperation
                && (int)action.Operation == modifierState.BoundTargetId;
        }

        private static bool BlocksActiveDreamSlot(DynamicModifierState modifierState, DynamicPlayerAction action)
        {
            if (modifierState.BoundTargetKind != DynamicModifierTargetKind.ActiveDreamSlot)
            {
                return false;
            }

            return action.Type switch
            {
                DynamicActionType.ApplyOperation => action.ActiveDreamSlotId == modifierState.BoundTargetId,
                DynamicActionType.SubmitDream => action.ActiveDreamSlotId == modifierState.BoundTargetId,
                DynamicActionType.StoreDream => action.ActiveDreamSlotId == modifierState.BoundTargetId,
                DynamicActionType.RecallDream => action.ActiveDreamSlotId == modifierState.BoundTargetId,
                _ => false
            };
        }

        private static void ConsumeCharge(DynamicModifierState modifierState)
        {
            modifierState.RemainingCharges--;
            if (modifierState.RemainingCharges <= 0)
            {
                modifierState.RemainingCharges = 0;
                modifierState.IsResolved = true;
            }
        }
    }
}
