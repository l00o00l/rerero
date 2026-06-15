using System;
using System.Collections.Generic;
using Thkim.DreamLaundromat.Levels;

namespace Thkim.DreamLaundromat.Rules
{
    public sealed class LevelState
    {
        public string LevelId;
        public int RemainingMoves;
        public LevelStatus Status = LevelStatus.Playing;
        public string FailureReason = string.Empty;
        public List<DreamRuntimeState> Dreams = new List<DreamRuntimeState>();
        public List<BasketDefinition> Baskets = new List<BasketDefinition>();
        public List<MachineDefinition> Machines = new List<MachineDefinition>();
        public List<OrderRuntimeState> Orders = new List<OrderRuntimeState>();

        public DreamRuntimeState FindDream(string dreamId)
        {
            return Dreams.Find(d => string.Equals(d.Id, dreamId, StringComparison.Ordinal));
        }

        public BasketDefinition FindBasket(string basketId)
        {
            return Baskets.Find(b => string.Equals(b.Id, basketId, StringComparison.Ordinal));
        }

        public MachineDefinition FindMachine(string machineId)
        {
            return Machines.Find(m => string.Equals(m.Id, machineId, StringComparison.Ordinal));
        }

        public OrderRuntimeState FindOrder(string orderId)
        {
            return Orders.Find(o => string.Equals(o.Id, orderId, StringComparison.Ordinal));
        }

        public int GetUsedCapacity(DreamLocation location)
        {
            int used = 0;
            for (int i = 0; i < Dreams.Count; i++)
            {
                if (Dreams[i].Location.Equals(location))
                {
                    used += Dreams[i].CapacityCost;
                }
            }

            return used;
        }

        public bool AllOrdersComplete()
        {
            for (int i = 0; i < Orders.Count; i++)
            {
                if (!Orders[i].IsComplete)
                {
                    return false;
                }
            }

            return Orders.Count > 0;
        }

        public LevelState Clone()
        {
            var clone = new LevelState
            {
                LevelId = LevelId,
                RemainingMoves = RemainingMoves,
                Status = Status,
                FailureReason = FailureReason,
                Baskets = new List<BasketDefinition>(Baskets),
                Machines = new List<MachineDefinition>(Machines)
            };

            for (int i = 0; i < Dreams.Count; i++)
            {
                clone.Dreams.Add(Dreams[i].Clone());
            }

            for (int i = 0; i < Orders.Count; i++)
            {
                clone.Orders.Add(Orders[i].Clone());
            }

            return clone;
        }
    }
}
