namespace Thkim.DreamLaundromat.DynamicLab
{
    public enum DreamTaint
    {
        Clean,
        Nightmare
    }

    public enum DreamMood
    {
        Anxious,
        Calm
    }

    public enum DreamClarity
    {
        Blurry,
        Vivid
    }

    public enum DreamStability
    {
        Unsettled,
        Stable
    }

    public enum DynamicOperation
    {
        Wash,
        Soothe,
        Clarify,
        Settle
    }

    public enum DynamicActionType
    {
        ApplyOperation,
        SubmitDream,
        StoreDream,
        RecallDream,
        UseItem
    }

    public enum DynamicRoundStatus
    {
        Ready,
        Playing,
        Cleared,
        Failed
    }

    public enum DynamicFailureReason
    {
        None,
        NoMovesRemaining,
        NoValidActions,
        NoDreamsAvailable,
        ImpossibleOrderState,
        InvalidAction
    }
}
