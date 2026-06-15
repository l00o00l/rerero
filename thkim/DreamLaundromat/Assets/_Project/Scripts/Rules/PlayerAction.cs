namespace Thkim.DreamLaundromat.Rules
{
    public readonly struct PlayerAction
    {
        public PlayerActionType Type { get; }
        public string DreamId { get; }
        public string TargetId { get; }
        public DreamLocation TakeDestination { get; }

        private PlayerAction(PlayerActionType type, string dreamId, string targetId, DreamLocation takeDestination)
        {
            Type = type;
            DreamId = dreamId;
            TargetId = targetId;
            TakeDestination = takeDestination;
        }

        public static PlayerAction MoveToBasket(string dreamId, string basketId)
        {
            return new PlayerAction(PlayerActionType.MoveToBasket, dreamId, basketId, default);
        }

        public static PlayerAction MoveToMachine(string dreamId, string machineId)
        {
            return new PlayerAction(PlayerActionType.MoveToMachine, dreamId, machineId, default);
        }

        public static PlayerAction TakeFromMachine(string dreamId, DreamLocation destination)
        {
            return new PlayerAction(PlayerActionType.TakeFromMachine, dreamId, string.Empty, destination);
        }

        public static PlayerAction Submit(string dreamId, string orderId)
        {
            return new PlayerAction(PlayerActionType.Submit, dreamId, orderId, default);
        }
    }
}
