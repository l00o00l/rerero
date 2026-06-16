using System.Collections.Generic;

namespace Thkim.DreamLaundromat.DynamicLab
{
    public sealed class DynamicRoundMetrics
    {
        public int MinMoves;
        public int MoveLimit;
        public int MoveSlack;
        public int ConversionCount;
        public int OperationDiversity;
        public int ActionTypeDiversity;
        public int SubmitCount;
        public int StorageMoveCount;
        public int SettleCount;
        public int ItemUseCount;
        public int ModifierTriggeredCount;
        public int ObstacleBlockedActionCount;
        public int MinMovesWithoutItems = -1;
        public bool MinMovesWithoutItemsAvailable;
        public int MaxRepeatedActionTypeRun;
        public int OperationSubmitCadenceCount;
        public float StorageMoveRatio;
        public float SettleActionRatio;
        public int MaxBranchingFactor;
        public float AverageBranchingFactor;
        public int DeadEndCount;
        public float DifficultyScore;
    }

    public static class DynamicRoundMetricCalculator
    {
        public static DynamicRoundMetrics Calculate(DynamicRoundDefinition definition, DynamicSolveResult solveResult)
        {
            var metrics = new DynamicRoundMetrics
            {
                MinMoves = solveResult.MinMoves,
                MoveLimit = definition.MoveLimit,
                MoveSlack = solveResult.Solvable ? definition.MoveLimit - solveResult.MinMoves : -1,
                MaxBranchingFactor = solveResult.MaxBranchingFactor,
                AverageBranchingFactor = solveResult.AverageBranchingFactor,
                DeadEndCount = solveResult.DeadEndCount
            };

            var operations = new HashSet<DynamicOperation>();
            var actionTypes = new HashSet<DynamicActionType>();
            DynamicRoundState replayState = DynamicRoundInitializer.CreateInitialState(definition);
            DynamicActionType previousType = default;
            bool hasPreviousType = false;
            int repeatedRun = 0;
            for (int i = 0; i < solveResult.FirstSolutionActions.Count; i++)
            {
                DynamicPlayerAction action = solveResult.FirstSolutionActions[i];
                metrics.ObstacleBlockedActionCount += DynamicActionEnumerator.CountBlockedCoreCandidates(replayState);
                actionTypes.Add(action.Type);
                if (!hasPreviousType || action.Type != previousType)
                {
                    repeatedRun = 1;
                    previousType = action.Type;
                    hasPreviousType = true;
                }
                else
                {
                    repeatedRun++;
                }

                metrics.MaxRepeatedActionTypeRun = System.Math.Max(metrics.MaxRepeatedActionTypeRun, repeatedRun);

                if (action.Type == DynamicActionType.ApplyOperation)
                {
                    metrics.ConversionCount++;
                    operations.Add(action.Operation);
                    if (action.Operation == DynamicOperation.Settle)
                    {
                        metrics.SettleCount++;
                    }
                }
                else if (action.Type == DynamicActionType.SubmitDream)
                {
                    metrics.SubmitCount++;
                }
                else if (action.Type == DynamicActionType.StoreDream || action.Type == DynamicActionType.RecallDream)
                {
                    metrics.StorageMoveCount++;
                }
                else if (action.Type == DynamicActionType.UseItem)
                {
                    metrics.ItemUseCount++;
                    metrics.ModifierTriggeredCount++;
                }

                if (i > 0
                    && solveResult.FirstSolutionActions[i - 1].Type == DynamicActionType.ApplyOperation
                    && action.Type == DynamicActionType.SubmitDream)
                {
                    metrics.OperationSubmitCadenceCount++;
                }

                DynamicActionResult replayResult = DynamicRulesEngine.Apply(replayState, action);
                if (!replayResult.Success)
                {
                    break;
                }
            }

            metrics.OperationDiversity = operations.Count;
            metrics.ActionTypeDiversity = actionTypes.Count;
            metrics.StorageMoveRatio = solveResult.FirstSolutionActions.Count == 0
                ? 0f
                : metrics.StorageMoveCount / (float)solveResult.FirstSolutionActions.Count;
            metrics.SettleActionRatio = solveResult.FirstSolutionActions.Count == 0
                ? 0f
                : metrics.SettleCount / (float)solveResult.FirstSolutionActions.Count;
            metrics.DifficultyScore = CalculateDifficultyScore(metrics);
            return metrics;
        }

        private static float CalculateDifficultyScore(DynamicRoundMetrics metrics)
        {
            if (metrics.MinMoves < 0)
            {
                return 0f;
            }

            return metrics.MinMoves
                + (metrics.ConversionCount * 0.75f)
                + (metrics.OperationDiversity * 1.25f)
                + (metrics.MaxBranchingFactor * 0.25f)
                + (metrics.StorageMoveCount * 0.5f)
                - (metrics.MoveSlack * 0.35f);
        }
    }
}
