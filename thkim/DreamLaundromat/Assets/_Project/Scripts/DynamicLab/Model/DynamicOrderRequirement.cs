namespace Thkim.DreamLaundromat.DynamicLab
{
    public readonly struct DynamicOrderRequirement
    {
        public DynamicOrderRequirement(
            int count,
            bool hasTaint,
            DreamTaint requiredTaint,
            bool hasMood,
            DreamMood requiredMood,
            bool hasClarity,
            DreamClarity requiredClarity,
            bool hasStability,
            DreamStability requiredStability)
        {
            Count = count;
            HasTaint = hasTaint;
            RequiredTaint = requiredTaint;
            HasMood = hasMood;
            RequiredMood = requiredMood;
            HasClarity = hasClarity;
            RequiredClarity = requiredClarity;
            HasStability = hasStability;
            RequiredStability = requiredStability;
        }

        public int Count { get; }
        public bool HasTaint { get; }
        public DreamTaint RequiredTaint { get; }
        public bool HasMood { get; }
        public DreamMood RequiredMood { get; }
        public bool HasClarity { get; }
        public DreamClarity RequiredClarity { get; }
        public bool HasStability { get; }
        public DreamStability RequiredStability { get; }

        public bool Matches(DynamicDreamAttributes attributes)
        {
            if (HasTaint && attributes.Taint != RequiredTaint)
            {
                return false;
            }

            if (HasMood && attributes.Mood != RequiredMood)
            {
                return false;
            }

            if (HasClarity && attributes.Clarity != RequiredClarity)
            {
                return false;
            }

            return !HasStability || attributes.Stability == RequiredStability;
        }

        public bool RequiresUnsettled()
        {
            return HasStability && RequiredStability == DreamStability.Unsettled;
        }

        public static DynamicOrderRequirement Stable(
            int count,
            bool hasTaint,
            DreamTaint requiredTaint,
            bool hasMood,
            DreamMood requiredMood,
            bool hasClarity,
            DreamClarity requiredClarity)
        {
            return new DynamicOrderRequirement(
                count,
                hasTaint,
                requiredTaint,
                hasMood,
                requiredMood,
                hasClarity,
                requiredClarity,
                true,
                DreamStability.Stable);
        }
    }
}
