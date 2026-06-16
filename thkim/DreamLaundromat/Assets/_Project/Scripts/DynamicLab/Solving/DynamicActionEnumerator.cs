using System.Collections.Generic;

namespace Thkim.DreamLaundromat.DynamicLab
{
    internal static class DynamicActionEnumerator
    {
        public static List<DynamicPlayerAction> Enumerate(DynamicRoundState state)
        {
            var actions = new List<DynamicPlayerAction>();
            actions.AddRange(DynamicModifierPipeline.EnumerateExtraActions(state));
            AddCoreActions(state, actions, filterBlockedByModifiers: true);
            return actions;
        }

        public static int CountBlockedCoreCandidates(DynamicRoundState state)
        {
            var coreActions = new List<DynamicPlayerAction>();
            AddCoreActions(state, coreActions, filterBlockedByModifiers: false);

            int blocked = 0;
            for (int i = 0; i < coreActions.Count; i++)
            {
                if (!DynamicModifierPipeline.CanApplyAction(state, coreActions[i]).Success)
                {
                    blocked++;
                }
            }

            return blocked;
        }

        private static void AddCoreActions(
            DynamicRoundState state,
            List<DynamicPlayerAction> actions,
            bool filterBlockedByModifiers)
        {
            for (int dreamIndex = 0; dreamIndex < state.ActiveDreams.Count; dreamIndex++)
            {
                DynamicDreamSlot dreamSlot = state.ActiveDreams[dreamIndex];
                if (dreamSlot.IsEmpty)
                {
                    continue;
                }

                for (int operationIndex = 0; operationIndex < state.ActionSet.Length; operationIndex++)
                {
                    DynamicOperation operation = state.ActionSet[operationIndex];
                    if (DynamicRulesEngine.CanApplyOperation(dreamSlot.Dream.Attributes, operation))
                    {
                        AddActionIfAllowed(
                            state,
                            actions,
                            DynamicPlayerAction.ApplyOperation(dreamSlot.SlotId, operation),
                            filterBlockedByModifiers);
                    }
                }

                for (int orderIndex = 0; orderIndex < state.ActiveOrders.Count; orderIndex++)
                {
                    DynamicOrderSlot orderSlot = state.ActiveOrders[orderIndex];
                    if (!orderSlot.IsEmpty && orderSlot.Order.CanAccept(dreamSlot.Dream.Attributes))
                    {
                        AddActionIfAllowed(
                            state,
                            actions,
                            DynamicPlayerAction.SubmitDream(dreamSlot.SlotId, orderSlot.SlotId),
                            filterBlockedByModifiers);
                    }
                }

                for (int storageIndex = 0; storageIndex < state.StorageSlots.Count; storageIndex++)
                {
                    DynamicStorageSlot storageSlot = state.StorageSlots[storageIndex];
                    if (storageSlot.IsEmpty)
                    {
                        AddActionIfAllowed(
                            state,
                            actions,
                            DynamicPlayerAction.StoreDream(dreamSlot.SlotId, storageSlot.SlotId),
                            filterBlockedByModifiers);
                    }
                }
            }

            for (int storageIndex = 0; storageIndex < state.StorageSlots.Count; storageIndex++)
            {
                DynamicStorageSlot storageSlot = state.StorageSlots[storageIndex];
                if (storageSlot.IsEmpty)
                {
                    continue;
                }

                for (int dreamIndex = 0; dreamIndex < state.ActiveDreams.Count; dreamIndex++)
                {
                    DynamicDreamSlot dreamSlot = state.ActiveDreams[dreamIndex];
                    if (dreamSlot.IsEmpty)
                    {
                        AddActionIfAllowed(
                            state,
                            actions,
                            DynamicPlayerAction.RecallDream(storageSlot.SlotId, dreamSlot.SlotId),
                            filterBlockedByModifiers);
                    }
                }
            }
        }

        private static void AddActionIfAllowed(
            DynamicRoundState state,
            List<DynamicPlayerAction> actions,
            DynamicPlayerAction action,
            bool filterBlockedByModifiers)
        {
            if (!filterBlockedByModifiers || DynamicModifierPipeline.CanApplyAction(state, action).Success)
            {
                actions.Add(action);
            }
        }
    }
}
