namespace Thkim.DreamLaundromat.Gameplay.ReleaseSlice
{
    public sealed class ReleaseBalanceReportResult
    {
        public ReleaseBalanceReportResult(
            ReleaseLevelPackValidationResult validation,
            ReleaseAccessibilityAuditResult accessibility,
            ReleaseModifierImpactReport modifierImpact,
            string report)
        {
            Validation = validation;
            Accessibility = accessibility;
            ModifierImpact = modifierImpact;
            Report = report ?? string.Empty;
        }

        public ReleaseLevelPackValidationResult Validation { get; }
        public ReleaseAccessibilityAuditResult Accessibility { get; }
        public ReleaseModifierImpactReport ModifierImpact { get; }
        public string Report { get; }
        public bool IsValid => Validation != null
            && Validation.IsValid
            && Accessibility != null
            && Accessibility.IsValid;
    }
}
