using System;
using Thkim.DreamLaundromat.Rules;

namespace Thkim.DreamLaundromat.Levels
{
    [Serializable]
    public sealed class OrderDefinition
    {
        public string Id;
        public string DisplayName;
        public OrderRequirement[] Requirements = Array.Empty<OrderRequirement>();
    }
}
