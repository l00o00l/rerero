namespace Thkim.DreamLaundromat.DynamicLab
{
    public static class DynamicStageRecipeValidator
    {
        public static DynamicValidationResult Validate(DynamicStageRecipe recipe)
        {
            var result = new DynamicValidationResult();
            if (recipe == null)
            {
                result.AddError("Stage recipe is missing.");
                return result;
            }

            if (string.IsNullOrWhiteSpace(recipe.RecipeId))
            {
                result.AddError("Recipe id is missing.");
            }

            if (recipe.MoveLimit <= 0)
            {
                result.AddError("Move limit must be greater than zero.");
            }

            if (recipe.TargetCompletedOrders <= 0)
            {
                result.AddError("Target completed orders must be greater than zero.");
            }

            if (recipe.CandidateDreamCount <= 0)
            {
                result.AddError("Candidate dream count must be greater than zero.");
            }

            if (recipe.CandidateOrderCount < recipe.TargetCompletedOrders)
            {
                result.AddError("Candidate order count must cover the target completed orders.");
            }

            ValidateStreamConfig(recipe, result);
            ValidateStorageConfig(recipe, result);
            ValidateActionSet(recipe, result);
            ValidateDreamPool(recipe, result);
            ValidateOrderPool(recipe, result);
            ValidateModifiers(recipe, result);
            ValidateAcceptanceBounds(recipe, result);
            return result;
        }

        private static void ValidateStreamConfig(DynamicStageRecipe recipe, DynamicValidationResult result)
        {
            if (recipe.StreamConfig == null)
            {
                result.AddError("Stream config is missing.");
                return;
            }

            if (recipe.StreamConfig.ActiveDreamSlots <= 0)
            {
                result.AddError("Active dream slots must be greater than zero.");
            }

            if (recipe.StreamConfig.ActiveOrderSlots <= 0)
            {
                result.AddError("Active order slots must be greater than zero.");
            }

            if (recipe.StreamConfig.DreamPreviewCount < 0)
            {
                result.AddError("Dream preview count must not be negative.");
            }
            else if (recipe.StreamConfig.DreamPreviewCount > recipe.StreamConfig.ActiveDreamSlots)
            {
                result.AddWarning("Dream preview count exceeds active dream slots.");
            }

            if (recipe.StreamConfig.OrderPreviewCount < 0)
            {
                result.AddError("Order preview count must not be negative.");
            }
            else if (recipe.StreamConfig.OrderPreviewCount > recipe.StreamConfig.ActiveOrderSlots)
            {
                result.AddWarning("Order preview count exceeds active order slots.");
            }
        }

        private static void ValidateStorageConfig(DynamicStageRecipe recipe, DynamicValidationResult result)
        {
            if (recipe.StorageConfig == null)
            {
                result.AddError("Storage config is missing.");
                return;
            }

            if (recipe.StorageConfig.StorageSlotCount < 0)
            {
                result.AddError("Storage slot count must not be negative.");
            }
        }

        private static void ValidateActionSet(DynamicStageRecipe recipe, DynamicValidationResult result)
        {
            if (recipe.ActionSet == null || recipe.ActionSet.Length == 0)
            {
                result.AddError("Action set is empty.");
            }
        }

        private static void ValidateDreamPool(DynamicStageRecipe recipe, DynamicValidationResult result)
        {
            if (recipe.DreamPool == null || recipe.DreamPool.Length == 0)
            {
                result.AddError("Dream pool is empty.");
                return;
            }

            for (int i = 0; i < recipe.DreamPool.Length; i++)
            {
                if (recipe.DreamPool[i].Weight <= 0)
                {
                    result.AddError("Dream pool weights must be positive.");
                }
            }
        }

        private static void ValidateOrderPool(DynamicStageRecipe recipe, DynamicValidationResult result)
        {
            if (recipe.OrderPool == null || recipe.OrderPool.Length == 0)
            {
                result.AddError("Order pool is empty.");
                return;
            }

            for (int i = 0; i < recipe.OrderPool.Length; i++)
            {
                if (recipe.OrderPool[i].Weight <= 0)
                {
                    result.AddError("Order pool weights must be positive.");
                }

                if (recipe.OrderPool[i].Requirement.Count <= 0)
                {
                    result.AddError("Order requirement count must be greater than zero.");
                }

                if (recipe.OrderPool[i].Requirement.RequiresUnsettled())
                {
                    result.AddError("Unsettled submit orders are not valid in the first lab.");
                }
            }
        }

        private static void ValidateAcceptanceBounds(DynamicStageRecipe recipe, DynamicValidationResult result)
        {
            if (recipe.MaxAcceptedMoves > 0 && recipe.MinAcceptedMoves > recipe.MaxAcceptedMoves)
            {
                result.AddError("Minimum accepted moves cannot exceed maximum accepted moves.");
            }

            if (recipe.MaxStorageMoveRatio < 0f || recipe.MaxStorageMoveRatio > 1f)
            {
                result.AddError("Maximum storage move ratio must be between 0 and 1.");
            }

            if (recipe.MaxSettleActionRatio < 0f || recipe.MaxSettleActionRatio > 1f)
            {
                result.AddError("Maximum settle action ratio must be between 0 and 1.");
            }

            if (recipe.MaxRepeatedActionTypeRun < 0)
            {
                result.AddError("Maximum repeated action type run must not be negative.");
            }
        }

        private static void ValidateModifiers(DynamicStageRecipe recipe, DynamicValidationResult result)
        {
            if (recipe.AllowedModifiers == null)
            {
                result.AddError("Allowed modifiers must not be null.");
                return;
            }

            var round = new DynamicRoundDefinition
            {
                RoundId = string.IsNullOrWhiteSpace(recipe.RoundIdPrefix)
                    ? recipe.RecipeId
                    : recipe.RoundIdPrefix,
                Seed = 1,
                MoveLimit = recipe.MoveLimit,
                TargetCompletedOrders = recipe.TargetCompletedOrders,
                StreamConfig = recipe.StreamConfig,
                StorageConfig = recipe.StorageConfig,
                ActionSet = recipe.ActionSet,
                DreamBag = new[]
                {
                    new DynamicDreamBagEntry(
                        recipe.DreamPool != null && recipe.DreamPool.Length > 0
                            ? recipe.DreamPool[0].Attributes
                            : new DynamicDreamAttributes(DreamTaint.Clean, DreamMood.Calm, DreamClarity.Blurry, DreamStability.Stable),
                        1)
                },
                OrderDeck = new[]
                {
                    new DynamicOrderDeckEntry(
                        recipe.OrderPool != null && recipe.OrderPool.Length > 0
                            ? recipe.OrderPool[0].Requirement
                            : DynamicOrderRequirement.Stable(1, false, DreamTaint.Clean, false, DreamMood.Calm, false, DreamClarity.Blurry),
                        1)
                },
                Modifiers = recipe.AllowedModifiers
            };

            DynamicValidationResult modifierValidation = DynamicRoundHardValidator.Validate(round);
            for (int i = 0; i < modifierValidation.Errors.Count; i++)
            {
                if (modifierValidation.Errors[i].Contains("Modifier")
                    || modifierValidation.Errors[i].Contains("Preview Swap")
                    || modifierValidation.Errors[i].Contains("Locked Slot"))
                {
                    result.AddError(modifierValidation.Errors[i]);
                }
            }
        }
    }
}
