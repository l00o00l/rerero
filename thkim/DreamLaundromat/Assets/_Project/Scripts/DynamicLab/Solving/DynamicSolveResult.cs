using System.Collections.Generic;

namespace Thkim.DreamLaundromat.DynamicLab
{
    public sealed class DynamicSolveResult
    {
        public bool Solvable;
        public int MinMoves = -1;
        public int SolutionCountEstimate;
        public readonly List<DynamicPlayerAction> FirstSolutionActions = new List<DynamicPlayerAction>();
        public int VisitedStates;
        public int DeadEndCount;
        public int MaxBranchingFactor;
        public float AverageBranchingFactor;
        public DynamicFailureReason FailureReason = DynamicFailureReason.None;
        public bool HitLimit;
    }
}
