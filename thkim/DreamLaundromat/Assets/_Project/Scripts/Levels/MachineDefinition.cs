using System;
using Thkim.DreamLaundromat.Rules;

namespace Thkim.DreamLaundromat.Levels
{
    [Serializable]
    public sealed class MachineDefinition
    {
        public string Id;
        public string DisplayName;
        public MachineType Type;
        public int Capacity = 1;
    }
}
