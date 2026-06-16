namespace Thkim.DreamLaundromat.DynamicLab
{
    public sealed class DynamicModifierState
    {
        public string ModifierId = string.Empty;
        public int RemainingCharges;
        public DynamicModifierTargetKind BoundTargetKind = DynamicModifierTargetKind.None;
        public int BoundTargetId = -1;
        public bool IsResolved;

        public bool HasCharges => RemainingCharges > 0;

        public DynamicModifierState Clone()
        {
            return new DynamicModifierState
            {
                ModifierId = ModifierId,
                RemainingCharges = RemainingCharges,
                BoundTargetKind = BoundTargetKind,
                BoundTargetId = BoundTargetId,
                IsResolved = IsResolved
            };
        }
    }
}
