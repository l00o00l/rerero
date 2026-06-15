using System;
using Thkim.DreamLaundromat.Levels;

namespace Thkim.DreamLaundromat.Rules
{
    public static class RulesEngine
    {
        public static LevelState CreateInitialState(LevelDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            var state = new LevelState
            {
                LevelId = definition.LevelId,
                RemainingMoves = definition.MoveLimit,
                Status = LevelStatus.Playing
            };

            state.Baskets.AddRange(definition.Baskets);
            state.Machines.AddRange(definition.Machines);

            for (int i = 0; i < definition.Dreams.Length; i++)
            {
                DreamDefinition dream = definition.Dreams[i];
                DreamLocation initialLocation = string.IsNullOrWhiteSpace(dream.InitialBasketId)
                    ? DreamLocation.Queue()
                    : DreamLocation.Basket(dream.InitialBasketId);

                state.Dreams.Add(new DreamRuntimeState
                {
                    Id = dream.Id,
                    DisplayName = dream.DisplayName,
                    Attributes = dream.InitialAttributes,
                    CapacityCost = Math.Max(1, dream.CapacityCost),
                    Location = initialLocation
                });
            }

            for (int i = 0; i < definition.Orders.Length; i++)
            {
                OrderDefinition order = definition.Orders[i];
                state.Orders.Add(new OrderRuntimeState
                {
                    Id = order.Id,
                    DisplayName = order.DisplayName,
                    Requirements = (OrderRequirement[])order.Requirements.Clone(),
                    FulfilledCounts = new int[order.Requirements.Length]
                });
            }

            return state;
        }

        public static ActionResult Apply(LevelState state, PlayerAction action)
        {
            if (state == null)
            {
                return ActionResult.Failed("Level state is missing.");
            }

            if (state.Status != LevelStatus.Playing)
            {
                return ActionResult.Failed("Level is not playing.");
            }

            if (state.RemainingMoves <= 0)
            {
                state.Status = LevelStatus.Failed;
                state.FailureReason = "턴이 부족합니다.";
                return ActionResult.Failed(state.FailureReason);
            }

            ActionResult result = action.Type switch
            {
                PlayerActionType.MoveToBasket => MoveToBasket(state, action.DreamId, action.TargetId),
                PlayerActionType.MoveToMachine => MoveToMachine(state, action.DreamId, action.TargetId),
                PlayerActionType.TakeFromMachine => TakeFromMachine(state, action.DreamId, action.TakeDestination),
                PlayerActionType.Submit => Submit(state, action.DreamId, action.TargetId),
                _ => ActionResult.Failed("Unknown action.")
            };

            if (!result.Success)
            {
                return result;
            }

            state.RemainingMoves--;
            RefreshLevelStatus(state);
            return result;
        }

        public static bool CanFit(LevelState state, DreamRuntimeState dream, DreamLocation destination, out string reason)
        {
            reason = string.Empty;

            if (destination.Kind == LocationKind.Queue)
            {
                return true;
            }

            if (destination.Kind == LocationKind.Basket)
            {
                BasketDefinition basket = state.FindBasket(destination.Id);
                if (basket == null)
                {
                    reason = "없는 바구니입니다.";
                    return false;
                }

                int used = state.GetUsedCapacity(destination);
                if (used + dream.CapacityCost > basket.Capacity)
                {
                    reason = "바구니 capacity가 부족합니다.";
                    return false;
                }

                return true;
            }

            if (destination.Kind == LocationKind.Machine)
            {
                MachineDefinition machine = state.FindMachine(destination.Id);
                if (machine == null)
                {
                    reason = "없는 기계입니다.";
                    return false;
                }

                int used = state.GetUsedCapacity(destination);
                if (used + dream.CapacityCost > machine.Capacity)
                {
                    reason = "기계 capacity가 부족합니다.";
                    return false;
                }

                return true;
            }

            reason = "이 위치로 이동할 수 없습니다.";
            return false;
        }

        private static ActionResult MoveToBasket(LevelState state, string dreamId, string basketId)
        {
            DreamRuntimeState dream = state.FindDream(dreamId);
            if (dream == null)
            {
                return ActionResult.Failed("없는 꿈 조각입니다.");
            }

            if (dream.Location.Kind == LocationKind.Submitted)
            {
                return ActionResult.Failed("이미 제출한 꿈입니다.");
            }

            if (dream.Location.Kind == LocationKind.Machine)
            {
                return ActionResult.Failed("기계 안의 꿈은 먼저 꺼내야 합니다.");
            }

            DreamLocation destination = DreamLocation.Basket(basketId);
            if (!CanFit(state, dream, destination, out string reason))
            {
                return ActionResult.Failed(reason);
            }

            dream.Location = destination;
            return ActionResult.Succeeded("꿈을 바구니로 옮겼습니다.");
        }

        private static ActionResult MoveToMachine(LevelState state, string dreamId, string machineId)
        {
            DreamRuntimeState dream = state.FindDream(dreamId);
            if (dream == null)
            {
                return ActionResult.Failed("없는 꿈 조각입니다.");
            }

            if (dream.Location.Kind == LocationKind.Machine)
            {
                return ActionResult.Failed("이미 기계 안에 있습니다.");
            }

            if (dream.Location.Kind == LocationKind.Submitted)
            {
                return ActionResult.Failed("이미 제출한 꿈입니다.");
            }

            MachineDefinition machine = state.FindMachine(machineId);
            if (machine == null)
            {
                return ActionResult.Failed("없는 기계입니다.");
            }

            DreamLocation destination = DreamLocation.Machine(machineId);
            if (!CanFit(state, dream, destination, out string reason))
            {
                return ActionResult.Failed(reason);
            }

            ActionResult transformResult = ApplyMachine(machine, dream);
            if (!transformResult.Success)
            {
                return transformResult;
            }

            dream.Location = destination;
            return transformResult;
        }

        private static ActionResult TakeFromMachine(LevelState state, string dreamId, DreamLocation destination)
        {
            DreamRuntimeState dream = state.FindDream(dreamId);
            if (dream == null)
            {
                return ActionResult.Failed("없는 꿈 조각입니다.");
            }

            if (dream.Location.Kind != LocationKind.Machine)
            {
                return ActionResult.Failed("기계 안에 있는 꿈만 꺼낼 수 있습니다.");
            }

            if (destination.Kind != LocationKind.Queue && destination.Kind != LocationKind.Basket)
            {
                return ActionResult.Failed("기계에서 꺼낸 꿈은 대기열이나 바구니로만 이동할 수 있습니다.");
            }

            if (!CanFit(state, dream, destination, out string reason))
            {
                return ActionResult.Failed(reason);
            }

            dream.Location = destination;
            return ActionResult.Succeeded("기계에서 꿈을 꺼냈습니다.");
        }

        private static ActionResult Submit(LevelState state, string dreamId, string orderId)
        {
            DreamRuntimeState dream = state.FindDream(dreamId);
            if (dream == null)
            {
                return ActionResult.Failed("없는 꿈 조각입니다.");
            }

            if (dream.Location.Kind == LocationKind.Machine)
            {
                return ActionResult.Failed("기계 안의 꿈은 바로 제출할 수 없습니다.");
            }

            if (dream.Location.Kind == LocationKind.Submitted)
            {
                return ActionResult.Failed("이미 제출한 꿈입니다.");
            }

            OrderRuntimeState order = state.FindOrder(orderId);
            if (order == null)
            {
                return ActionResult.Failed("없는 주문입니다.");
            }

            if (!order.TrySubmit(dream.Attributes))
            {
                return ActionResult.Failed("주문 조건과 맞지 않습니다.");
            }

            dream.Location = DreamLocation.Submitted(orderId);
            return ActionResult.Succeeded("주문에 제출했습니다.");
        }

        private static ActionResult ApplyMachine(MachineDefinition machine, DreamRuntimeState dream)
        {
            if (machine.Type == MachineType.Washer)
            {
                if (dream.Attributes.Stain != DreamStain.Nightmare)
                {
                    return ActionResult.Failed("세탁기는 악몽 얼룩이 있는 꿈만 처리합니다.");
                }

                dream.Attributes = new DreamAttributes(DreamStain.None, DreamMoisture.Wet);
                return ActionResult.Succeeded("세탁했습니다.");
            }

            if (machine.Type == MachineType.Dryer)
            {
                if (dream.Attributes.Moisture != DreamMoisture.Wet)
                {
                    return ActionResult.Failed("건조기는 젖은 꿈만 처리합니다.");
                }

                dream.Attributes = new DreamAttributes(dream.Attributes.Stain, DreamMoisture.Dry);
                return ActionResult.Succeeded("건조했습니다.");
            }

            return ActionResult.Failed("지원하지 않는 기계입니다.");
        }

        private static void RefreshLevelStatus(LevelState state)
        {
            if (state.AllOrdersComplete())
            {
                state.Status = LevelStatus.Cleared;
                state.FailureReason = string.Empty;
                return;
            }

            if (state.RemainingMoves <= 0)
            {
                state.Status = LevelStatus.Failed;
                state.FailureReason = "턴이 부족합니다.";
            }
        }
    }
}
