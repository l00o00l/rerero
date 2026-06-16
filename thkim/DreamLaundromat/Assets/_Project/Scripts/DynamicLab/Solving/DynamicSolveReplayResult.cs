namespace Thkim.DreamLaundromat.DynamicLab
{
    public sealed class DynamicSolveReplayResult
    {
        public bool Success;
        public string FailureMessage = string.Empty;
        public DynamicRoundState FinalState;
        public int AppliedActionCount;
    }
}
