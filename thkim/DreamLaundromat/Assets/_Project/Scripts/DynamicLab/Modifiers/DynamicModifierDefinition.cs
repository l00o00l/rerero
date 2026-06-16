namespace Thkim.DreamLaundromat.DynamicLab
{
    public sealed class DynamicModifierDefinition
    {
        public string Id = string.Empty;
        public string DisplayName = string.Empty;
        public DynamicModifierType Type;
        public DynamicModifierTrigger Trigger;
        public DynamicModifierScope Scope;
        public DynamicModifierEffect Effect;
        public int Charges = 1;
        public bool ConsumesMove = true;
        public bool RequiresItem;
        public DynamicModifierTargetKind TargetKind = DynamicModifierTargetKind.None;
        public int TargetId = -1;
        public string[] Tags = System.Array.Empty<string>();

        public DynamicModifierDefinition Clone()
        {
            return new DynamicModifierDefinition
            {
                Id = Id,
                DisplayName = DisplayName,
                Type = Type,
                Trigger = Trigger,
                Scope = Scope,
                Effect = Effect,
                Charges = Charges,
                ConsumesMove = ConsumesMove,
                RequiresItem = RequiresItem,
                TargetKind = TargetKind,
                TargetId = TargetId,
                Tags = Tags == null ? System.Array.Empty<string>() : (string[])Tags.Clone()
            };
        }
    }
}
