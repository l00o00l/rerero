namespace Thkim.DreamLaundromat.DynamicLab
{
    public static class DynamicSolveReplayVerifier
    {
        public static DynamicSolveReplayResult Verify(
            DynamicRoundDefinition definition,
            DynamicSolveResult solveResult)
        {
            var result = new DynamicSolveReplayResult();
            if (definition == null)
            {
                result.FailureMessage = "Round definition is missing.";
                return result;
            }

            if (solveResult == null || !solveResult.Solvable)
            {
                result.FailureMessage = "Solve result is missing or unsolved.";
                return result;
            }

            DynamicRoundState state = DynamicRoundInitializer.CreateInitialState(definition);
            for (int i = 0; i < solveResult.FirstSolutionActions.Count; i++)
            {
                DynamicActionResult actionResult = DynamicRulesEngine.Apply(state, solveResult.FirstSolutionActions[i]);
                if (!actionResult.Success)
                {
                    result.FinalState = state;
                    result.AppliedActionCount = i;
                    result.FailureMessage = $"Replay failed at action {i}: {actionResult.Message}";
                    return result;
                }
            }

            result.FinalState = state;
            result.AppliedActionCount = solveResult.FirstSolutionActions.Count;
            result.Success = state.Status == DynamicRoundStatus.Cleared
                && state.CompletedOrders >= definition.TargetCompletedOrders;

            if (!result.Success)
            {
                result.FailureMessage = $"Replay ended with status {state.Status}.";
            }

            return result;
        }
    }
}
