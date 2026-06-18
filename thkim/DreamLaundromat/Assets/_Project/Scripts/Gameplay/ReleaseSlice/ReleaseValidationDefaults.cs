using Thkim.DreamLaundromat.DynamicLab;

namespace Thkim.DreamLaundromat.Gameplay.ReleaseSlice
{
    public static class ReleaseValidationDefaults
    {
        public static readonly DynamicSolveOptions SolveOptions = new DynamicSolveOptions
        {
            // Expert generated candidates can be solvable but slow enough to
            // make editor validation flaky on cold Windows Unity runs.
            MaxVisitedStates = 60000,
            TimeoutMilliseconds = 4000
        };
    }
}
