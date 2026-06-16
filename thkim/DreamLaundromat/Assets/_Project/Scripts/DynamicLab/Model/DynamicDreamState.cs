namespace Thkim.DreamLaundromat.DynamicLab
{
    public sealed class DynamicDreamState
    {
        public string Id = string.Empty;
        public string DisplayName = string.Empty;
        public DynamicDreamAttributes Attributes;

        public DynamicDreamState Clone()
        {
            return new DynamicDreamState
            {
                Id = Id,
                DisplayName = DisplayName,
                Attributes = Attributes
            };
        }
    }
}
