using Thkim.DreamLaundromat.DynamicLab;

namespace Thkim.DreamLaundromat.Gameplay.ReleaseSlice
{
    public sealed class ReleaseGuidedActionRule
    {
        private ReleaseGuidedActionRule(
            DynamicActionType actionType,
            DynamicOperation operation,
            bool matchOperation,
            string prompt)
        {
            ActionType = actionType;
            Operation = operation;
            MatchOperation = matchOperation;
            Prompt = prompt ?? string.Empty;
        }

        public DynamicActionType ActionType { get; }
        public DynamicOperation Operation { get; }
        public bool MatchOperation { get; }
        public string Prompt { get; }

        public static ReleaseGuidedActionRule Any(DynamicActionType actionType, string prompt)
        {
            return new ReleaseGuidedActionRule(actionType, default, false, prompt);
        }

        public static ReleaseGuidedActionRule OperationRule(DynamicOperation operation, string prompt)
        {
            return new ReleaseGuidedActionRule(DynamicActionType.ApplyOperation, operation, true, prompt);
        }

        public bool Matches(DynamicPlayerAction action)
        {
            if (action.Type != ActionType)
            {
                return false;
            }

            return !MatchOperation || action.Operation == Operation;
        }

        public string Describe()
        {
            return MatchOperation ? $"{ActionType}:{Operation}" : ActionType.ToString();
        }
    }
}
