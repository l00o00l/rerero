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
                if (CanResolveManualItem(state, definition, modifierState).Success)
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
            DynamicActionResult canResolve = CanResolveManualItem(state, definition, modifierState);
            if (!canResolve.Success)
            {
                return canResolve;
            }

            DynamicDreamState first = state.DreamPreview[0];
            state.DreamPreview[0] = state.DreamPreview[1];
            state.DreamPreview[1] = first;
            ConsumeCharge(modifierState);
            return DynamicActionResult.Succeeded("Preview order swapped.");
        }

        public static DynamicActionResult BeforeAction(DynamicRoundState state, DynamicPlayerAction action)
        {
            return DynamicActionResult.Succeeded("No modifier before-action effects.");
        }

        public static DynamicActionResult AfterAction(DynamicRoundState state, DynamicPlayerAction action)
        {
            return DynamicActionResult.Succeeded("No modifier after-action effects.");
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

        private static DynamicActionResult CanResolveManualItem(
            DynamicRoundState state,
            DynamicModifierDefinition definition,
            DynamicModifierState modifierState)
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

            if (definition.Effect != DynamicModifierEffect.PreviewSwap)
            {
                return DynamicActionResult.Failed("Unsupported item effect.");
            }

            if (state.DreamPreview.Count < 2)
            {
                return DynamicActionResult.Failed("Preview Swap needs at least two dream previews.");
            }

            return DynamicActionResult.Succeeded("Item can be resolved.");
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
