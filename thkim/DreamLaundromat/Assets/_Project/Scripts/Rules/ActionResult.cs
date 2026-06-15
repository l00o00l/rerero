namespace Thkim.DreamLaundromat.Rules
{
    public readonly struct ActionResult
    {
        public bool Success { get; }
        public bool ConsumedTurn { get; }
        public string Message { get; }

        private ActionResult(bool success, bool consumedTurn, string message)
        {
            Success = success;
            ConsumedTurn = consumedTurn;
            Message = message;
        }

        public static ActionResult Succeeded(string message)
        {
            return new ActionResult(true, true, message);
        }

        public static ActionResult Failed(string message)
        {
            return new ActionResult(false, false, message);
        }
    }
}
