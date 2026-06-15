using System;
using UnityEngine;

namespace Thkim.DreamLaundromat.Levels
{
    [CreateAssetMenu(menuName = "DreamLaundromat/Level Catalog")]
    public sealed class LevelCatalog : ScriptableObject
    {
        [SerializeField] private LevelDefinition[] _levels = Array.Empty<LevelDefinition>();

        public LevelDefinition[] Levels => _levels;

        public void Configure(LevelDefinition[] levels)
        {
            _levels = levels ?? Array.Empty<LevelDefinition>();
        }
    }
}
