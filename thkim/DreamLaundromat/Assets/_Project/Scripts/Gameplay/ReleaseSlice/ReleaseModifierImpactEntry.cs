using System;
using System.Collections.Generic;
using Thkim.DreamLaundromat.DynamicLab;

namespace Thkim.DreamLaundromat.Gameplay.ReleaseSlice
{
    public sealed class ReleaseModifierImpactEntry
    {
        public string LevelId = string.Empty;
        public DynamicModifierEffect[] Effects = Array.Empty<DynamicModifierEffect>();
        public int ModifierCount;
        public int ItemCount;
        public int ObstacleCount;
        public int OriginalMinMoves = -1;
        public int OriginalMaxBranchingFactor;
        public float OriginalAverageBranchingFactor;
        public int OriginalItemUseCount;
        public int OriginalObstacleBlockedActionCount;
        public bool WithoutModifiersSolvable;
        public bool WithoutModifiersHitLimit;
        public int WithoutModifiersMinMoves = -1;
        public int WithoutModifiersMaxBranchingFactor;
        public float WithoutModifiersAverageBranchingFactor;
        public readonly List<string> Warnings = new List<string>();

        public bool FirstSolutionUsesItem => OriginalItemUseCount > 0;
        public bool HasWarning => Warnings.Count > 0;

        public int MinMoveDelta => WithoutModifiersSolvable
            ? WithoutModifiersMinMoves - OriginalMinMoves
            : 0;

        public int MaxBranchingDelta => WithoutModifiersSolvable
            ? WithoutModifiersMaxBranchingFactor - OriginalMaxBranchingFactor
            : 0;
    }
}
