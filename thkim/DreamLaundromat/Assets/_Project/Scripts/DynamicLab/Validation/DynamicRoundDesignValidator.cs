namespace Thkim.DreamLaundromat.DynamicLab
{
    public static class DynamicRoundDesignValidator
    {
        public static DynamicValidationResult Validate(
            DynamicRoundDefinition definition,
            DynamicSolveResult solveResult,
            DynamicRoundMetrics metrics)
        {
            var result = new DynamicValidationResult();
            if (!solveResult.Solvable)
            {
                result.AddError("Round is not solvable.");
                return result;
            }

            if (metrics.MoveSlack < 0)
            {
                result.AddError("Move slack is invalid.");
            }
            else if (metrics.MoveSlack == 0)
            {
                result.AddWarning("Move limit has no slack.");
            }
            else if (metrics.MoveSlack > 8)
            {
                result.AddWarning("Move limit has too much slack.");
            }

            if (metrics.SubmitCount >= solveResult.FirstSolutionActions.Count && metrics.SubmitCount >= 3)
            {
                result.AddWarning("First solution is mostly direct submit actions.");
            }

            if (metrics.ConversionCount >= 2 && metrics.OperationDiversity <= 1)
            {
                result.AddWarning("First solution repeats one operation type.");
            }

            if (metrics.StorageMoveRatio > 0.5f)
            {
                result.AddWarning("Storage moves dominate the first solution.");
            }

            if (metrics.SettleCount >= 2 && metrics.SettleActionRatio >= 0.25f)
            {
                result.AddWarning("Settle acts like a repeated submit tax.");
            }

            if (metrics.OperationSubmitCadenceCount >= 2 && metrics.OperationSubmitCadenceCount >= metrics.SubmitCount - 1)
            {
                result.AddWarning("First solution has a mechanical operation-submit cadence.");
            }

            if (metrics.ActionTypeDiversity <= 1 && solveResult.FirstSolutionActions.Count > 2)
            {
                result.AddWarning("First solution uses only one action category.");
            }

            if (solveResult.MaxBranchingFactor <= 1 && solveResult.FirstSolutionActions.Count > 2)
            {
                result.AddWarning("Round has low branching and may feel scripted.");
            }

            if (!HasOrderCompetition(definition))
            {
                result.AddWarning("No obvious order competition in the deck.");
            }

            if (definition.StreamConfig != null && definition.StreamConfig.ActiveOrderSlots <= 1)
            {
                result.AddWarning("Only one active order is visible, so order competition is muted.");
            }

            if (!HasPreviewRelevance(definition))
            {
                result.AddWarning("Preview is unlikely to affect decisions in this round.");
            }

            return result;
        }

        private static bool HasOrderCompetition(DynamicRoundDefinition definition)
        {
            if (definition.OrderDeck == null || definition.OrderDeck.Length < 2)
            {
                return false;
            }

            for (int i = 0; i < definition.OrderDeck.Length; i++)
            {
                for (int j = i + 1; j < definition.OrderDeck.Length; j++)
                {
                    if (RequirementsOverlap(definition.OrderDeck[i].Requirement, definition.OrderDeck[j].Requirement))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool RequirementsOverlap(DynamicOrderRequirement first, DynamicOrderRequirement second)
        {
            if (first.HasTaint && second.HasTaint && first.RequiredTaint != second.RequiredTaint)
            {
                return false;
            }

            if (first.HasMood && second.HasMood && first.RequiredMood != second.RequiredMood)
            {
                return false;
            }

            if (first.HasClarity && second.HasClarity && first.RequiredClarity != second.RequiredClarity)
            {
                return false;
            }

            if (first.HasStability && second.HasStability && first.RequiredStability != second.RequiredStability)
            {
                return false;
            }

            return true;
        }

        private static bool HasPreviewRelevance(DynamicRoundDefinition definition)
        {
            if (definition.StreamConfig == null)
            {
                return false;
            }

            int dreamCount = CountDreams(definition);
            int orderCount = CountOrders(definition);
            bool dreamStreamExtendsPastVisibleSlots = dreamCount
                > definition.StreamConfig.ActiveDreamSlots + definition.StreamConfig.DreamPreviewCount;
            bool orderStreamExtendsPastVisibleSlots = orderCount
                > definition.StreamConfig.ActiveOrderSlots + definition.StreamConfig.OrderPreviewCount;

            return (definition.StreamConfig.DreamPreviewCount > 0 && dreamStreamExtendsPastVisibleSlots)
                || (definition.StreamConfig.OrderPreviewCount > 0 && orderStreamExtendsPastVisibleSlots);
        }

        private static int CountDreams(DynamicRoundDefinition definition)
        {
            if (definition.DreamBag == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < definition.DreamBag.Length; i++)
            {
                count += definition.DreamBag[i].Count;
            }

            return count;
        }

        private static int CountOrders(DynamicRoundDefinition definition)
        {
            if (definition.OrderDeck == null)
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < definition.OrderDeck.Length; i++)
            {
                count += definition.OrderDeck[i].Count;
            }

            return count;
        }
    }
}
