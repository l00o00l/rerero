using Thkim.DreamLaundromat.DynamicLab;
using UnityEngine;

namespace Thkim.DreamLaundromat.Gameplay.ReleaseSlice
{
    [CreateAssetMenu(menuName = "DreamLaundromat/Release UI Art Catalog")]
    public sealed class ReleaseUiArtCatalog : ScriptableObject
    {
        [SerializeField] private Sprite gameplayBackground;
        [SerializeField] private Sprite titleBackground;
        [SerializeField] private Sprite levelSelectBackground;
        [SerializeField] private Sprite dreamCardFrame;
        [SerializeField] private Sprite orderSheetFrame;
        [SerializeField] private Sprite storageShelfFrame;
        [SerializeField] private Sprite operationButtonFrame;
        [SerializeField] private Sprite submitButtonFrame;
        [SerializeField] private Sprite storageActionFrame;
        [SerializeField] private Sprite navigationButtonFrame;
        [SerializeField] private Sprite stateTaintClean;
        [SerializeField] private Sprite stateTaintNightmare;
        [SerializeField] private Sprite stateMoodCalm;
        [SerializeField] private Sprite stateMoodAnxious;
        [SerializeField] private Sprite stateClarityVivid;
        [SerializeField] private Sprite stateClarityBlurry;
        [SerializeField] private Sprite stateStabilityStable;
        [SerializeField] private Sprite stateStabilityUnsettled;
        [SerializeField] private Sprite operationWash;
        [SerializeField] private Sprite operationSoothe;
        [SerializeField] private Sprite operationClarify;
        [SerializeField] private Sprite operationSettle;
        [SerializeField] private Sprite toolPreviewSwap;
        [SerializeField] private Sprite toolDreamRefresh;
        [SerializeField] private Sprite obstacleLockedSlot;
        [SerializeField] private Sprite obstacleOrderPin;
        [SerializeField] private Sprite obstacleSoftBlock;
        [SerializeField] private Sprite effectClearGlow;
        [SerializeField] private Sprite effectFailWarning;

        public Sprite GameplayBackground => gameplayBackground;
        public Sprite TitleBackground => titleBackground;
        public Sprite LevelSelectBackground => levelSelectBackground;
        public Sprite DreamCardFrame => dreamCardFrame;
        public Sprite OrderSheetFrame => orderSheetFrame;
        public Sprite StorageShelfFrame => storageShelfFrame;
        public Sprite OperationButtonFrame => operationButtonFrame;
        public Sprite SubmitButtonFrame => submitButtonFrame;
        public Sprite StorageActionFrame => storageActionFrame;
        public Sprite NavigationButtonFrame => navigationButtonFrame;
        public Sprite EffectClearGlow => effectClearGlow;
        public Sprite EffectFailWarning => effectFailWarning;

        public bool IsComplete =>
            gameplayBackground != null &&
            titleBackground != null &&
            levelSelectBackground != null &&
            dreamCardFrame != null &&
            orderSheetFrame != null &&
            storageShelfFrame != null &&
            operationButtonFrame != null &&
            submitButtonFrame != null &&
            storageActionFrame != null &&
            navigationButtonFrame != null &&
            stateTaintClean != null &&
            stateTaintNightmare != null &&
            stateMoodCalm != null &&
            stateMoodAnxious != null &&
            stateClarityVivid != null &&
            stateClarityBlurry != null &&
            stateStabilityStable != null &&
            stateStabilityUnsettled != null &&
            operationWash != null &&
            operationSoothe != null &&
            operationClarify != null &&
            operationSettle != null &&
            toolPreviewSwap != null &&
            toolDreamRefresh != null &&
            obstacleLockedSlot != null &&
            obstacleOrderPin != null &&
            obstacleSoftBlock != null &&
            effectClearGlow != null &&
            effectFailWarning != null;

        public Sprite GetPrimaryStateIcon(DynamicDreamAttributes attributes)
        {
            if (attributes.Taint == DreamTaint.Nightmare)
            {
                return stateTaintNightmare;
            }

            if (attributes.Mood == DreamMood.Anxious)
            {
                return stateMoodAnxious;
            }

            if (attributes.Clarity == DreamClarity.Blurry)
            {
                return stateClarityBlurry;
            }

            if (attributes.Stability == DreamStability.Unsettled)
            {
                return stateStabilityUnsettled;
            }

            return stateTaintClean;
        }

        public Sprite GetTaintIcon(DreamTaint value)
        {
            return value == DreamTaint.Clean ? stateTaintClean : stateTaintNightmare;
        }

        public Sprite GetMoodIcon(DreamMood value)
        {
            return value == DreamMood.Calm ? stateMoodCalm : stateMoodAnxious;
        }

        public Sprite GetClarityIcon(DreamClarity value)
        {
            return value == DreamClarity.Vivid ? stateClarityVivid : stateClarityBlurry;
        }

        public Sprite GetStabilityIcon(DreamStability value)
        {
            return value == DreamStability.Stable ? stateStabilityStable : stateStabilityUnsettled;
        }

        public Sprite GetRequirementIcon(DynamicOrderRequirement requirement)
        {
            if (requirement.HasTaint)
            {
                return requirement.RequiredTaint == DreamTaint.Clean ? stateTaintClean : stateTaintNightmare;
            }

            if (requirement.HasMood)
            {
                return requirement.RequiredMood == DreamMood.Calm ? stateMoodCalm : stateMoodAnxious;
            }

            if (requirement.HasClarity)
            {
                return requirement.RequiredClarity == DreamClarity.Vivid ? stateClarityVivid : stateClarityBlurry;
            }

            if (requirement.HasStability)
            {
                return requirement.RequiredStability == DreamStability.Stable ? stateStabilityStable : stateStabilityUnsettled;
            }

            return stateStabilityStable;
        }

        public Sprite GetOperationIcon(DynamicOperation operation)
        {
            return operation switch
            {
                DynamicOperation.Wash => operationWash,
                DynamicOperation.Soothe => operationSoothe,
                DynamicOperation.Clarify => operationClarify,
                DynamicOperation.Settle => operationSettle,
                _ => null
            };
        }

        public Sprite GetModifierIcon(DynamicModifierDefinition definition)
        {
            if (definition == null)
            {
                return null;
            }

            if (definition.Type == DynamicModifierType.Item)
            {
                return definition.Effect == DynamicModifierEffect.RefreshActiveDream
                    ? toolDreamRefresh
                    : toolPreviewSwap;
            }

            return definition.Effect switch
            {
                DynamicModifierEffect.LockActiveDreamSlot => obstacleLockedSlot,
                DynamicModifierEffect.PinOrderSlot => obstacleOrderPin,
                DynamicModifierEffect.SoftBlockOperation => obstacleSoftBlock,
                _ => obstacleSoftBlock
            };
        }

        public void Configure(
            Sprite gameplayBackgroundSprite,
            Sprite titleBackgroundSprite,
            Sprite levelSelectBackgroundSprite,
            Sprite dreamCardFrameSprite,
            Sprite orderSheetFrameSprite,
            Sprite storageShelfFrameSprite,
            Sprite operationButtonFrameSprite,
            Sprite submitButtonFrameSprite,
            Sprite storageActionFrameSprite,
            Sprite navigationButtonFrameSprite,
            Sprite stateTaintCleanSprite,
            Sprite stateTaintNightmareSprite,
            Sprite stateMoodCalmSprite,
            Sprite stateMoodAnxiousSprite,
            Sprite stateClarityVividSprite,
            Sprite stateClarityBlurrySprite,
            Sprite stateStabilityStableSprite,
            Sprite stateStabilityUnsettledSprite,
            Sprite operationWashSprite,
            Sprite operationSootheSprite,
            Sprite operationClarifySprite,
            Sprite operationSettleSprite,
            Sprite toolPreviewSwapSprite,
            Sprite toolDreamRefreshSprite,
            Sprite obstacleLockedSlotSprite,
            Sprite obstacleOrderPinSprite,
            Sprite obstacleSoftBlockSprite,
            Sprite effectClearGlowSprite,
            Sprite effectFailWarningSprite)
        {
            gameplayBackground = gameplayBackgroundSprite;
            titleBackground = titleBackgroundSprite;
            levelSelectBackground = levelSelectBackgroundSprite;
            dreamCardFrame = dreamCardFrameSprite;
            orderSheetFrame = orderSheetFrameSprite;
            storageShelfFrame = storageShelfFrameSprite;
            operationButtonFrame = operationButtonFrameSprite;
            submitButtonFrame = submitButtonFrameSprite;
            storageActionFrame = storageActionFrameSprite;
            navigationButtonFrame = navigationButtonFrameSprite;
            stateTaintClean = stateTaintCleanSprite;
            stateTaintNightmare = stateTaintNightmareSprite;
            stateMoodCalm = stateMoodCalmSprite;
            stateMoodAnxious = stateMoodAnxiousSprite;
            stateClarityVivid = stateClarityVividSprite;
            stateClarityBlurry = stateClarityBlurrySprite;
            stateStabilityStable = stateStabilityStableSprite;
            stateStabilityUnsettled = stateStabilityUnsettledSprite;
            operationWash = operationWashSprite;
            operationSoothe = operationSootheSprite;
            operationClarify = operationClarifySprite;
            operationSettle = operationSettleSprite;
            toolPreviewSwap = toolPreviewSwapSprite;
            toolDreamRefresh = toolDreamRefreshSprite;
            obstacleLockedSlot = obstacleLockedSlotSprite;
            obstacleOrderPin = obstacleOrderPinSprite;
            obstacleSoftBlock = obstacleSoftBlockSprite;
            effectClearGlow = effectClearGlowSprite;
            effectFailWarning = effectFailWarningSprite;
        }
    }
}
