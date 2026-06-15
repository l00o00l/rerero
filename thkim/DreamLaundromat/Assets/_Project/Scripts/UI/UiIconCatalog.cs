using UnityEngine;

namespace Thkim.DreamLaundromat.UI
{
    [CreateAssetMenu(menuName = "DreamLaundromat/UI Icon Catalog")]
    public sealed class UiIconCatalog : ScriptableObject
    {
        [SerializeField] private Sprite _cleanDream;
        [SerializeField] private Sprite _nightmareDream;
        [SerializeField] private Sprite _wetState;
        [SerializeField] private Sprite _dryState;
        [SerializeField] private Sprite _washerMachine;
        [SerializeField] private Sprite _dryerMachine;
        [SerializeField] private Sprite _submitOrder;
        [SerializeField] private Sprite _storageBasket;

        public Sprite CleanDream => _cleanDream;
        public Sprite NightmareDream => _nightmareDream;
        public Sprite WetState => _wetState;
        public Sprite DryState => _dryState;
        public Sprite WasherMachine => _washerMachine;
        public Sprite DryerMachine => _dryerMachine;
        public Sprite SubmitOrder => _submitOrder;
        public Sprite StorageBasket => _storageBasket;

        public bool IsComplete =>
            _cleanDream != null &&
            _nightmareDream != null &&
            _wetState != null &&
            _dryState != null &&
            _washerMachine != null &&
            _dryerMachine != null &&
            _submitOrder != null &&
            _storageBasket != null;

        public void Configure(
            Sprite cleanDream,
            Sprite nightmareDream,
            Sprite wetState,
            Sprite dryState,
            Sprite washerMachine,
            Sprite dryerMachine,
            Sprite submitOrder,
            Sprite storageBasket)
        {
            _cleanDream = cleanDream;
            _nightmareDream = nightmareDream;
            _wetState = wetState;
            _dryState = dryState;
            _washerMachine = washerMachine;
            _dryerMachine = dryerMachine;
            _submitOrder = submitOrder;
            _storageBasket = storageBasket;
        }
    }
}
