using Thkim.DreamLaundromat.DynamicLab;
using UnityEngine;

namespace Thkim.DreamLaundromat.Gameplay.ReleaseSlice
{
    public readonly struct ReleaseActionVisualDescriptor
    {
        public ReleaseActionVisualDescriptor(
            DynamicOperation operation,
            string displayName,
            string marker,
            string shortHint,
            Color color)
        {
            Operation = operation;
            DisplayName = displayName ?? string.Empty;
            Marker = marker ?? string.Empty;
            ShortHint = shortHint ?? string.Empty;
            Color = color;
        }

        public DynamicOperation Operation { get; }
        public string DisplayName { get; }
        public string Marker { get; }
        public string ShortHint { get; }
        public Color Color { get; }
        public string ButtonTitle => DisplayName;
    }
}
