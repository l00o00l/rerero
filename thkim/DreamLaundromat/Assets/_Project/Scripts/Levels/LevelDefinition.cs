using System;
using UnityEngine;

namespace Thkim.DreamLaundromat.Levels
{
    [CreateAssetMenu(menuName = "DreamLaundromat/Level Definition")]
    public sealed class LevelDefinition : ScriptableObject
    {
        [SerializeField] private string _levelId;
        [SerializeField] private int _moveLimit = 10;
        [SerializeField] private DreamDefinition[] _dreams = Array.Empty<DreamDefinition>();
        [SerializeField] private MachineDefinition[] _machines = Array.Empty<MachineDefinition>();
        [SerializeField] private BasketDefinition[] _baskets = Array.Empty<BasketDefinition>();
        [SerializeField] private OrderDefinition[] _orders = Array.Empty<OrderDefinition>();
        [SerializeField] private string _tutorialHint;

        public string LevelId => _levelId;
        public int MoveLimit => _moveLimit;
        public DreamDefinition[] Dreams => _dreams;
        public MachineDefinition[] Machines => _machines;
        public BasketDefinition[] Baskets => _baskets;
        public OrderDefinition[] Orders => _orders;
        public string TutorialHint => _tutorialHint;

        public void Configure(
            string levelId,
            int moveLimit,
            DreamDefinition[] dreams,
            MachineDefinition[] machines,
            BasketDefinition[] baskets,
            OrderDefinition[] orders,
            string tutorialHint)
        {
            _levelId = levelId;
            _moveLimit = moveLimit;
            _dreams = dreams ?? Array.Empty<DreamDefinition>();
            _machines = machines ?? Array.Empty<MachineDefinition>();
            _baskets = baskets ?? Array.Empty<BasketDefinition>();
            _orders = orders ?? Array.Empty<OrderDefinition>();
            _tutorialHint = tutorialHint ?? string.Empty;
        }
    }
}
