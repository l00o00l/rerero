namespace Thkim.DreamLaundromat.DynamicLab
{
    public sealed class DynamicOrderState
    {
        public string Id = string.Empty;
        public string DisplayName = string.Empty;
        public DynamicOrderRequirement Requirement;
        public int FulfilledCount;

        public bool IsComplete => FulfilledCount >= Requirement.Count;

        public bool CanAccept(DynamicDreamAttributes attributes)
        {
            return attributes.Stability == DreamStability.Stable
                && !IsComplete
                && Requirement.Matches(attributes);
        }

        public bool TrySubmit(DynamicDreamAttributes attributes)
        {
            if (!CanAccept(attributes))
            {
                return false;
            }

            FulfilledCount++;
            return true;
        }

        public DynamicOrderState Clone()
        {
            return new DynamicOrderState
            {
                Id = Id,
                DisplayName = DisplayName,
                Requirement = Requirement,
                FulfilledCount = FulfilledCount
            };
        }
    }
}
