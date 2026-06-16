using System.Collections.Generic;
using System.Diagnostics;

namespace Thkim.DreamLaundromat.DynamicLab
{
    public static class DynamicRoundSolver
    {
        public static DynamicSolveResult Solve(DynamicRoundDefinition definition, DynamicSolveOptions options = null)
        {
            options ??= new DynamicSolveOptions();
            var hardValidation = DynamicRoundHardValidator.Validate(definition);
            if (!hardValidation.IsValid)
            {
                return new DynamicSolveResult
                {
                    FailureReason = DynamicFailureReason.ImpossibleOrderState
                };
            }

            DynamicRoundState initialState = DynamicRoundInitializer.CreateInitialState(definition);
            var result = new DynamicSolveResult();
            var queue = new Queue<SolveNode>();
            var visited = new HashSet<string>();
            var stopwatch = Stopwatch.StartNew();
            int totalBranching = 0;
            int expandedNodes = 0;

            queue.Enqueue(new SolveNode(initialState, new List<DynamicPlayerAction>()));
            visited.Add(DynamicRoundStateHasher.CreateHash(initialState));

            while (queue.Count > 0)
            {
                if (result.VisitedStates >= options.MaxVisitedStates
                    || stopwatch.ElapsedMilliseconds > options.TimeoutMilliseconds)
                {
                    result.HitLimit = true;
                    return result;
                }

                SolveNode node = queue.Dequeue();
                result.VisitedStates++;

                if (node.State.Status == DynamicRoundStatus.Cleared)
                {
                    MarkSolved(result, node.Actions);
                    result.AverageBranchingFactor = expandedNodes == 0 ? 0f : totalBranching / (float)expandedNodes;
                    return result;
                }

                if (node.State.Status == DynamicRoundStatus.Failed)
                {
                    result.DeadEndCount++;
                    continue;
                }

                List<DynamicPlayerAction> actions = DynamicActionEnumerator.Enumerate(node.State);
                result.MaxBranchingFactor = System.Math.Max(result.MaxBranchingFactor, actions.Count);
                totalBranching += actions.Count;
                expandedNodes++;

                if (actions.Count == 0)
                {
                    result.DeadEndCount++;
                    continue;
                }

                for (int i = 0; i < actions.Count; i++)
                {
                    DynamicRoundState nextState = node.State.Clone();
                    DynamicActionResult actionResult = DynamicRulesEngine.Apply(nextState, actions[i]);
                    if (!actionResult.Success)
                    {
                        continue;
                    }

                    var nextActions = new List<DynamicPlayerAction>(node.Actions.Count + 1);
                    nextActions.AddRange(node.Actions);
                    nextActions.Add(actions[i]);

                    if (nextState.Status == DynamicRoundStatus.Cleared)
                    {
                        MarkSolved(result, nextActions);
                        result.VisitedStates++;
                        result.AverageBranchingFactor = expandedNodes == 0 ? 0f : totalBranching / (float)expandedNodes;
                        return result;
                    }

                    if (nextState.Status == DynamicRoundStatus.Failed)
                    {
                        result.DeadEndCount++;
                        continue;
                    }

                    string hash = DynamicRoundStateHasher.CreateHash(nextState);
                    if (visited.Add(hash))
                    {
                        queue.Enqueue(new SolveNode(nextState, nextActions));
                    }
                }
            }

            result.AverageBranchingFactor = expandedNodes == 0 ? 0f : totalBranching / (float)expandedNodes;
            return result;
        }

        private static void MarkSolved(DynamicSolveResult result, List<DynamicPlayerAction> actions)
        {
            result.Solvable = true;
            result.MinMoves = actions.Count;
            result.SolutionCountEstimate = 1;
            result.FirstSolutionActions.AddRange(actions);
            result.FailureReason = DynamicFailureReason.None;
        }

        private readonly struct SolveNode
        {
            public SolveNode(DynamicRoundState state, List<DynamicPlayerAction> actions)
            {
                State = state;
                Actions = actions;
            }

            public DynamicRoundState State { get; }
            public List<DynamicPlayerAction> Actions { get; }
        }
    }
}
