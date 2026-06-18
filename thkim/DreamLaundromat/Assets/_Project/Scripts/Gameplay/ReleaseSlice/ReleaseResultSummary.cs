using Thkim.DreamLaundromat.DynamicLab;

namespace Thkim.DreamLaundromat.Gameplay.ReleaseSlice
{
    public readonly struct ReleaseResultSummary
    {
        public ReleaseResultSummary(
            DynamicRoundStatus status,
            string title,
            string subtitle,
            string detail,
            bool canAdvance)
        {
            Status = status;
            Title = title;
            Subtitle = subtitle;
            Detail = detail;
            CanAdvance = canAdvance;
        }

        public DynamicRoundStatus Status { get; }
        public string Title { get; }
        public string Subtitle { get; }
        public string Detail { get; }
        public bool CanAdvance { get; }

        public static ReleaseResultSummary Create(
            ReleaseLevelDefinition level,
            DynamicRoundState state,
            string message,
            bool hasNextLevel)
        {
            if (state.Status == DynamicRoundStatus.Cleared)
            {
                return new ReleaseResultSummary(
                    state.Status,
                    "Clear Result",
                    $"{level.LevelId}  {level.DisplayName}",
                    $"Orders complete {state.CompletedOrders}/{state.TargetCompletedOrders}\nRemaining moves {state.RemainingMoves}\n{message}",
                    hasNextLevel);
            }

            string reason = state.FailureReason == DynamicFailureReason.None
                ? "The order flow stopped."
                : state.FailureReason.ToString();
            return new ReleaseResultSummary(
                state.Status,
                "Fail Result",
                $"{level.LevelId}  {level.DisplayName}",
                $"Reason: {reason}\nRetry the order with a cleaner sequence.\n{message}",
                false);
        }
    }
}
