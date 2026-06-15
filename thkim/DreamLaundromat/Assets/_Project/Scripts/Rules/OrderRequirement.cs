using System;

namespace Thkim.DreamLaundromat.Rules
{
    [Serializable]
    public struct OrderRequirement
    {
        public bool RequiresStain;
        public DreamStain Stain;
        public bool RequiresMoisture;
        public DreamMoisture Moisture;
        public int Count;

        public OrderRequirement(
            int count,
            bool requiresStain,
            DreamStain stain,
            bool requiresMoisture,
            DreamMoisture moisture)
        {
            Count = count;
            RequiresStain = requiresStain;
            Stain = stain;
            RequiresMoisture = requiresMoisture;
            Moisture = moisture;
        }

        public bool Matches(DreamAttributes attributes)
        {
            if (RequiresStain && attributes.Stain != Stain)
            {
                return false;
            }

            if (RequiresMoisture && attributes.Moisture != Moisture)
            {
                return false;
            }

            return true;
        }

        public string Describe()
        {
            string stain = RequiresStain ? $"stain={Stain}" : "stain=*";
            string moisture = RequiresMoisture ? $"moisture={Moisture}" : "moisture=*";
            return $"{stain}, {moisture} x{Count}";
        }
    }
}
