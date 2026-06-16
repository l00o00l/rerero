using System.Collections.Generic;

namespace Thkim.DreamLaundromat.DynamicLab
{
    public sealed class DynamicRoundCandidateReport
    {
        public string RecipeId = string.Empty;
        public string RoundId = string.Empty;
        public string DesignIntent = string.Empty;
        public string PlayerQuestion = string.Empty;
        public string RiskNote = string.Empty;
        public int Seed;
        public bool Accepted;
        public DynamicRoundDefinition Round;
        public DynamicValidationResult RecipeValidation;
        public DynamicValidationResult HardValidation;
        public DynamicValidationResult DesignValidation;
        public DynamicSolveResult SolveResult;
        public DynamicRoundMetrics Metrics;
        public readonly List<string> RejectReasons = new List<string>();
        public readonly List<string> Warnings = new List<string>();
    }
}
