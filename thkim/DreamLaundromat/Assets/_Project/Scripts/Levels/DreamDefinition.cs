using System;
using Thkim.DreamLaundromat.Rules;

namespace Thkim.DreamLaundromat.Levels
{
    [Serializable]
    public sealed class DreamDefinition
    {
        public string Id;
        public string DisplayName;
        public DreamAttributes InitialAttributes;
        public int CapacityCost = 1;
        public string InitialBasketId;

        public DreamDefinition Clone()
        {
            return new DreamDefinition
            {
                Id = Id,
                DisplayName = DisplayName,
                InitialAttributes = InitialAttributes,
                CapacityCost = CapacityCost,
                InitialBasketId = InitialBasketId
            };
        }
    }
}
