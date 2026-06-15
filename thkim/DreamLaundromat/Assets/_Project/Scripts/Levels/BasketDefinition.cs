using System;

namespace Thkim.DreamLaundromat.Levels
{
    [Serializable]
    public sealed class BasketDefinition
    {
        public string Id;
        public string DisplayName;
        public int Capacity = 2;
    }
}
