namespace Thkim.DreamLaundromat.Rules
{
    public sealed class DreamRuntimeState
    {
        public string Id;
        public string DisplayName;
        public DreamAttributes Attributes;
        public int CapacityCost;
        public DreamLocation Location;

        public DreamRuntimeState Clone()
        {
            return new DreamRuntimeState
            {
                Id = Id,
                DisplayName = DisplayName,
                Attributes = Attributes,
                CapacityCost = CapacityCost,
                Location = Location
            };
        }
    }
}
