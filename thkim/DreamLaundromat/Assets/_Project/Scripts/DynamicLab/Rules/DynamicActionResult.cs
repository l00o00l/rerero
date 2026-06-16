namespace Thkim.DreamLaundromat.DynamicLab
{
    public readonly struct DynamicActionResult
    {
        public bool Success { get; }
        public string Message { get; }
        public DynamicFailureReason FailureReason { get; }

        private DynamicActionResult(bool success, string message, DynamicFailureReason failureReason)
        {
            Success = success;
            Message = message;
            FailureReason = failureReason;
        }

        public static DynamicActionResult Succeeded(string message)
        {
            return new DynamicActionResult(true, message, DynamicFailureReason.None);
        }

        public static DynamicActionResult Failed(string message, DynamicFailureReason failureReason = DynamicFailureReason.InvalidAction)
        {
            return new DynamicActionResult(false, message, failureReason);
        }
    }
}
