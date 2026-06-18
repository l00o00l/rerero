using UnityEngine;

namespace Thkim.DreamLaundromat.Gameplay.ReleaseSlice
{
    public readonly struct ReleaseStateVisualDescriptor
    {
        public ReleaseStateVisualDescriptor(string axis, string value, string marker, Color color)
        {
            Axis = axis ?? string.Empty;
            Value = value ?? string.Empty;
            Marker = marker ?? string.Empty;
            Color = color;
        }

        public string Axis { get; }
        public string Value { get; }
        public string Marker { get; }
        public Color Color { get; }
        public string BadgeLabel => $"{Marker} {Axis}: {Value}";
        public string CompactLabel => $"{Marker} {Value}";
    }
}
