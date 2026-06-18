namespace Thkim.DreamLaundromat.DynamicLab
{
    public static class DynamicRoundHardValidator
    {
        public static DynamicValidationResult Validate(DynamicRoundDefinition definition)
        {
            var result = new DynamicValidationResult();
            if (definition == null)
            {
                result.AddError("Round definition is missing.");
                return result;
            }

            if (string.IsNullOrWhiteSpace(definition.RoundId))
            {
                result.AddError("Round id is missing.");
            }

            if (definition.MoveLimit <= 0)
            {
                result.AddError("Move limit must be greater than zero.");
            }

            if (definition.TargetCompletedOrders <= 0)
            {
                result.AddError("Target completed orders must be greater than zero.");
            }

            ValidateStreamConfig(definition, result);
            ValidateDreamBag(definition, result);
            ValidateOrderDeck(definition, result);
            ValidateActionSet(definition, result);
            ValidateStorage(definition, result);
            ValidateModifiers(definition, result);
            return result;
        }

        private static void ValidateStreamConfig(DynamicRoundDefinition definition, DynamicValidationResult result)
        {
            if (definition.StreamConfig == null)
            {
                result.AddError("Stream config is missing.");
                return;
            }

            if (definition.StreamConfig.ActiveDreamSlots <= 0)
            {
                result.AddError("Active dream slots must be greater than zero.");
            }

            if (definition.StreamConfig.ActiveOrderSlots <= 0)
            {
                result.AddError("Active order slots must be greater than zero.");
            }

            if (definition.StreamConfig.DreamPreviewCount < 0)
            {
                result.AddError("Dream preview count must not be negative.");
            }
            else if (definition.StreamConfig.DreamPreviewCount > definition.StreamConfig.ActiveDreamSlots
                && !HasModifierEffect(definition, DynamicModifierEffect.PreviewSwap))
            {
                result.AddWarning("Dream preview count exceeds active dream slots.");
            }

            if (definition.StreamConfig.OrderPreviewCount < 0)
            {
                result.AddError("Order preview count must not be negative.");
            }
            else if (definition.StreamConfig.OrderPreviewCount > definition.StreamConfig.ActiveOrderSlots)
            {
                result.AddWarning("Order preview count exceeds active order slots.");
            }
        }

        private static void ValidateDreamBag(DynamicRoundDefinition definition, DynamicValidationResult result)
        {
            if (definition.DreamBag == null || definition.DreamBag.Length == 0)
            {
                result.AddError("Dream bag is empty.");
                return;
            }

            int total = 0;
            for (int i = 0; i < definition.DreamBag.Length; i++)
            {
                if (definition.DreamBag[i].Count <= 0)
                {
                    result.AddError("Dream bag entries must use positive count values.");
                }

                total += definition.DreamBag[i].Count;
            }

            if (definition.StreamConfig != null && definition.StreamConfig.MaxDreamDraws > 0 && total > definition.StreamConfig.MaxDreamDraws)
            {
                result.AddWarning("Dream bag count exceeds configured max dream draws.");
            }
        }

        private static void ValidateOrderDeck(DynamicRoundDefinition definition, DynamicValidationResult result)
        {
            if (definition.OrderDeck == null || definition.OrderDeck.Length == 0)
            {
                result.AddError("Order deck is empty.");
                return;
            }

            int total = 0;
            for (int i = 0; i < definition.OrderDeck.Length; i++)
            {
                if (definition.OrderDeck[i].Count <= 0)
                {
                    result.AddError("Order deck entries must use positive count values.");
                }

                if (definition.OrderDeck[i].Requirement.Count <= 0)
                {
                    result.AddError("Order requirement count must be greater than zero.");
                }

                if (definition.OrderDeck[i].Requirement.RequiresUnsettled())
                {
                    result.AddError("Unsettled submit orders are not valid in the first lab.");
                }

                total += definition.OrderDeck[i].Count;
            }

            if (total < definition.TargetCompletedOrders)
            {
                result.AddError("Order deck cannot satisfy target completed orders.");
            }
        }

        private static void ValidateActionSet(DynamicRoundDefinition definition, DynamicValidationResult result)
        {
            if (definition.ActionSet == null || definition.ActionSet.Length == 0)
            {
                result.AddError("Action set is empty.");
            }
        }

        private static void ValidateStorage(DynamicRoundDefinition definition, DynamicValidationResult result)
        {
            if (definition.StorageConfig == null)
            {
                result.AddError("Storage config is missing.");
                return;
            }

            if (definition.StorageConfig.StorageSlotCount < 0)
            {
                result.AddError("Storage slot count must not be negative.");
            }
        }

        private static void ValidateModifiers(DynamicRoundDefinition definition, DynamicValidationResult result)
        {
            if (definition.Modifiers == null)
            {
                result.AddError("Modifier list must not be null.");
                return;
            }

            for (int i = 0; i < definition.Modifiers.Length; i++)
            {
                DynamicModifierDefinition modifier = definition.Modifiers[i];
                if (modifier == null)
                {
                    result.AddError("Modifier definition must not be null.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(modifier.Id))
                {
                    result.AddError("Modifier id is missing.");
                }
                else if (HasPreviousModifierWithId(definition, i, modifier.Id))
                {
                    result.AddError($"Modifier id {modifier.Id} is duplicated.");
                }

                if (modifier.Charges < 0)
                {
                    result.AddError($"Modifier {modifier.Id} charges must not be negative.");
                }

                if (modifier.Effect == DynamicModifierEffect.PreviewSwap)
                {
                    ValidatePreviewSwap(modifier, result);
                }
                else if (modifier.Effect == DynamicModifierEffect.LockActiveDreamSlot)
                {
                    ValidateLockedSlot(definition, modifier, result);
                }
                else if (modifier.Effect == DynamicModifierEffect.PinOrderSlot)
                {
                    ValidateOrderPin(definition, modifier, result);
                }
                else if (modifier.Effect == DynamicModifierEffect.RefreshActiveDream)
                {
                    ValidateDreamRefresh(modifier, result);
                }
                else if (modifier.Effect == DynamicModifierEffect.SoftBlockOperation)
                {
                    ValidateOperationSoftBlock(modifier, result);
                }
                else
                {
                    result.AddError($"Modifier {modifier.Id} effect is not supported.");
                }
            }
        }

        private static bool HasModifierEffect(
            DynamicRoundDefinition definition,
            DynamicModifierEffect effect)
        {
            if (definition.Modifiers == null)
            {
                return false;
            }

            for (int i = 0; i < definition.Modifiers.Length; i++)
            {
                if (definition.Modifiers[i] != null && definition.Modifiers[i].Effect == effect)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool HasPreviousModifierWithId(
            DynamicRoundDefinition definition,
            int currentIndex,
            string modifierId)
        {
            for (int i = 0; i < currentIndex; i++)
            {
                DynamicModifierDefinition previous = definition.Modifiers[i];
                if (previous != null && previous.Id == modifierId)
                {
                    return true;
                }
            }

            return false;
        }

        private static void ValidatePreviewSwap(
            DynamicModifierDefinition modifier,
            DynamicValidationResult result)
        {
            if (modifier.Type != DynamicModifierType.Item
                || modifier.Trigger != DynamicModifierTrigger.Manual
                || modifier.Scope != DynamicModifierScope.Preview)
            {
                result.AddError("Preview Swap must be a manual item scoped to preview.");
            }
        }

        private static void ValidateLockedSlot(
            DynamicRoundDefinition definition,
            DynamicModifierDefinition modifier,
            DynamicValidationResult result)
        {
            if (modifier.Type != DynamicModifierType.Obstacle
                || modifier.Trigger != DynamicModifierTrigger.CanApplyAction
                || modifier.Scope != DynamicModifierScope.Slot
                || modifier.TargetKind != DynamicModifierTargetKind.ActiveDreamSlot)
            {
                result.AddError("Locked Slot must be a can-apply obstacle scoped to an active dream slot.");
            }

            if (definition.StreamConfig != null
                && (modifier.TargetId < 0 || modifier.TargetId >= definition.StreamConfig.ActiveDreamSlots))
            {
                result.AddError("Locked Slot target must point to an existing active dream slot.");
            }
        }

        private static void ValidateOrderPin(
            DynamicRoundDefinition definition,
            DynamicModifierDefinition modifier,
            DynamicValidationResult result)
        {
            if (modifier.Type != DynamicModifierType.Obstacle
                || modifier.Trigger != DynamicModifierTrigger.CanApplyAction
                || modifier.Scope != DynamicModifierScope.Order
                || modifier.TargetKind != DynamicModifierTargetKind.OrderSlot)
            {
                result.AddError("Order Pin must be a can-apply obstacle scoped to an active order slot.");
            }

            if (definition.StreamConfig != null
                && (modifier.TargetId < 0 || modifier.TargetId >= definition.StreamConfig.ActiveOrderSlots))
            {
                result.AddError("Order Pin target must point to an existing active order slot.");
            }
        }

        private static void ValidateDreamRefresh(
            DynamicModifierDefinition modifier,
            DynamicValidationResult result)
        {
            if (modifier.Type != DynamicModifierType.Item
                || modifier.Trigger != DynamicModifierTrigger.Manual
                || modifier.Scope != DynamicModifierScope.Dream
                || modifier.TargetKind != DynamicModifierTargetKind.ActiveDreamSlot)
            {
                result.AddError("Dream Refresh must be a manual item scoped to an active dream target.");
            }
        }

        private static void ValidateOperationSoftBlock(
            DynamicModifierDefinition modifier,
            DynamicValidationResult result)
        {
            if (modifier.Type != DynamicModifierType.Obstacle
                || modifier.Trigger != DynamicModifierTrigger.CanApplyAction
                || modifier.Scope != DynamicModifierScope.Round
                || modifier.TargetKind != DynamicModifierTargetKind.Operation)
            {
                result.AddError("Operation Soft Block must be a can-apply obstacle scoped to an operation.");
            }

            if (!System.Enum.IsDefined(typeof(DynamicOperation), modifier.TargetId))
            {
                result.AddError("Operation Soft Block target must point to an existing operation.");
            }
        }
    }
}
