using System;

namespace Thkim.DreamLaundromat.Rules
{
    public sealed class OrderRuntimeState
    {
        public string Id;
        public string DisplayName;
        public OrderRequirement[] Requirements = Array.Empty<OrderRequirement>();
        public int[] FulfilledCounts = Array.Empty<int>();

        public bool IsComplete
        {
            get
            {
                for (int i = 0; i < Requirements.Length; i++)
                {
                    if (FulfilledCounts[i] < Requirements[i].Count)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        public bool TrySubmit(DreamAttributes attributes)
        {
            for (int i = 0; i < Requirements.Length; i++)
            {
                if (FulfilledCounts[i] >= Requirements[i].Count)
                {
                    continue;
                }

                if (!Requirements[i].Matches(attributes))
                {
                    continue;
                }

                FulfilledCounts[i]++;
                return true;
            }

            return false;
        }

        public OrderRuntimeState Clone()
        {
            return new OrderRuntimeState
            {
                Id = Id,
                DisplayName = DisplayName,
                Requirements = (OrderRequirement[])Requirements.Clone(),
                FulfilledCounts = (int[])FulfilledCounts.Clone()
            };
        }
    }
}
