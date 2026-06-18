using Thkim.DreamLaundromat.DynamicLab;

namespace Thkim.DreamLaundromat.Gameplay.ReleaseSlice
{
    public sealed class ReleaseLevelValidationEntry
    {
        public ReleaseLevelDefinition Level;
        public DynamicRoundDefinition Round;
        public DynamicValidationResult HardValidation;
        public DynamicSolveResult SolveResult;
        public DynamicSolveReplayResult ReplayResult;
        public DynamicRoundMetrics Metrics;
        public DynamicValidationResult DesignValidation;
        public DynamicValidationResult ReleaseValidation;

        public bool IsValid => ReleaseValidation != null
            && ReleaseValidation.IsValid
            && HardValidation != null
            && HardValidation.IsValid
            && SolveResult != null
            && SolveResult.Solvable
            && !SolveResult.HitLimit
            && ReplayResult != null
            && ReplayResult.Success
            && DesignValidation != null
            && DesignValidation.IsValid;
    }
}
