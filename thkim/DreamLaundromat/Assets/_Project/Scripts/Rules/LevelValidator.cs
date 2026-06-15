using System;
using System.Collections.Generic;
using Thkim.DreamLaundromat.Levels;

namespace Thkim.DreamLaundromat.Rules
{
    public static class LevelValidator
    {
        public static ValidationResult Validate(LevelDefinition level)
        {
            var result = new ValidationResult();

            if (level == null)
            {
                result.AddError("Level is null.");
                return result;
            }

            if (string.IsNullOrWhiteSpace(level.LevelId))
            {
                result.AddError("Level id is empty.");
            }

            if (level.MoveLimit <= 0)
            {
                result.AddError($"{level.LevelId}: move limit must be greater than zero.");
            }

            CheckUniqueIds(level.LevelId, "dream", level.Dreams, d => d.Id, result);
            CheckUniqueIds(level.LevelId, "machine", level.Machines, m => m.Id, result);
            CheckUniqueIds(level.LevelId, "basket", level.Baskets, b => b.Id, result);
            CheckUniqueIds(level.LevelId, "order", level.Orders, o => o.Id, result);
            CheckInitialBasketReferences(level, result);
            CheckInitialCapacity(level, result);
            CheckOrders(level, result);

            return result;
        }

        private static void CheckUniqueIds<T>(
            string levelId,
            string label,
            T[] items,
            Func<T, string> getId,
            ValidationResult result)
        {
            var ids = new HashSet<string>(StringComparer.Ordinal);

            for (int i = 0; i < items.Length; i++)
            {
                string id = getId(items[i]);
                if (string.IsNullOrWhiteSpace(id))
                {
                    result.AddError($"{levelId}: {label} id is empty at index {i}.");
                    continue;
                }

                if (!ids.Add(id))
                {
                    result.AddError($"{levelId}: duplicate {label} id '{id}'.");
                }
            }
        }

        private static void CheckInitialBasketReferences(LevelDefinition level, ValidationResult result)
        {
            for (int i = 0; i < level.Dreams.Length; i++)
            {
                DreamDefinition dream = level.Dreams[i];
                if (string.IsNullOrWhiteSpace(dream.InitialBasketId))
                {
                    continue;
                }

                if (Array.Find(level.Baskets, b => b.Id == dream.InitialBasketId) == null)
                {
                    result.AddError($"{level.LevelId}: dream '{dream.Id}' references missing basket '{dream.InitialBasketId}'.");
                }
            }
        }

        private static void CheckInitialCapacity(LevelDefinition level, ValidationResult result)
        {
            for (int i = 0; i < level.Baskets.Length; i++)
            {
                BasketDefinition basket = level.Baskets[i];
                int used = 0;

                for (int j = 0; j < level.Dreams.Length; j++)
                {
                    DreamDefinition dream = level.Dreams[j];
                    if (dream.InitialBasketId == basket.Id)
                    {
                        used += Math.Max(1, dream.CapacityCost);
                    }
                }

                if (used > basket.Capacity)
                {
                    result.AddError($"{level.LevelId}: basket '{basket.Id}' starts over capacity.");
                }
            }
        }

        private static void CheckOrders(LevelDefinition level, ValidationResult result)
        {
            if (level.Orders.Length == 0)
            {
                result.AddError($"{level.LevelId}: level has no orders.");
                return;
            }

            for (int i = 0; i < level.Orders.Length; i++)
            {
                OrderDefinition order = level.Orders[i];

                if (order.Requirements.Length == 0)
                {
                    result.AddError($"{level.LevelId}: order '{order.Id}' has no requirements.");
                    continue;
                }

                for (int j = 0; j < order.Requirements.Length; j++)
                {
                    OrderRequirement requirement = order.Requirements[j];
                    if (requirement.Count <= 0)
                    {
                        result.AddError($"{level.LevelId}: order '{order.Id}' has non-positive count.");
                    }

                    if (!CanAnyDreamSatisfy(level, requirement))
                    {
                        result.AddWarning($"{level.LevelId}: no obvious dream can satisfy order '{order.Id}' requirement {requirement.Describe()}.");
                    }
                }
            }
        }

        private static bool CanAnyDreamSatisfy(LevelDefinition level, OrderRequirement requirement)
        {
            bool hasWasher = Array.Exists(level.Machines, m => m.Type == MachineType.Washer);
            bool hasDryer = Array.Exists(level.Machines, m => m.Type == MachineType.Dryer);

            for (int i = 0; i < level.Dreams.Length; i++)
            {
                DreamAttributes attributes = level.Dreams[i].InitialAttributes;

                if (requirement.Matches(attributes))
                {
                    return true;
                }

                if (hasWasher && attributes.Stain == DreamStain.Nightmare)
                {
                    DreamAttributes washed = new DreamAttributes(DreamStain.None, DreamMoisture.Wet);
                    if (requirement.Matches(washed))
                    {
                        return true;
                    }

                    if (hasDryer)
                    {
                        DreamAttributes dried = new DreamAttributes(DreamStain.None, DreamMoisture.Dry);
                        if (requirement.Matches(dried))
                        {
                            return true;
                        }
                    }
                }

                if (hasDryer && attributes.Moisture == DreamMoisture.Wet)
                {
                    DreamAttributes dried = new DreamAttributes(attributes.Stain, DreamMoisture.Dry);
                    if (requirement.Matches(dried))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
