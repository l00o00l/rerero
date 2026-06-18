using UnityEngine;

namespace Thkim.DreamLaundromat.Gameplay.ReleaseSlice
{
    public static class ReleaseVisualStyle
    {
        public static readonly float MinTouchTargetHeight = 58f;
        public static readonly float MinimumBodyContrastRatio = 4.5f;

        public static readonly Color Background = new Color(0.045f, 0.052f, 0.07f, 1f);
        public static readonly Color Panel = new Color(0.105f, 0.12f, 0.145f, 0.78f);
        public static readonly Color Content = new Color(0.06f, 0.07f, 0.09f, 0.48f);
        public static readonly Color Text = new Color(0.92f, 0.94f, 0.98f, 1f);
        public static readonly Color TextShadow = new Color(0.015f, 0.018f, 0.025f, 0.72f);
        public static readonly Color PanelOutline = new Color(0.48f, 0.58f, 0.66f, 0.16f);
        public static readonly Color ButtonOutline = new Color(0.72f, 0.82f, 0.9f, 0.18f);
        public static readonly Color MutedPanel = new Color(0.075f, 0.085f, 0.105f, 0.34f);
        public static readonly Color Selected = new Color(0.72f, 0.58f, 0.26f, 1f);
        public static readonly Color Disabled = new Color(0.14f, 0.15f, 0.17f, 1f);
        public static readonly Color CleanDream = new Color(0.19f, 0.3f, 0.34f, 1f);
        public static readonly Color NightmareDream = new Color(0.31f, 0.24f, 0.38f, 1f);
        public static readonly Color AnxiousDream = new Color(0.32f, 0.25f, 0.22f, 1f);
        public static readonly Color CalmDream = new Color(0.18f, 0.32f, 0.25f, 1f);
        public static readonly Color VividDream = new Color(0.18f, 0.28f, 0.42f, 1f);
        public static readonly Color BlurryDream = new Color(0.27f, 0.28f, 0.31f, 1f);
        public static readonly Color StableDream = new Color(0.25f, 0.33f, 0.22f, 1f);
        public static readonly Color UnsettledDream = new Color(0.38f, 0.25f, 0.23f, 1f);
        public static readonly Color Action = new Color(0.22f, 0.29f, 0.42f, 1f);
        public static readonly Color WashAction = new Color(0.17f, 0.35f, 0.43f, 1f);
        public static readonly Color SootheAction = new Color(0.18f, 0.34f, 0.26f, 1f);
        public static readonly Color ClarifyAction = new Color(0.24f, 0.28f, 0.48f, 1f);
        public static readonly Color SettleAction = new Color(0.35f, 0.30f, 0.20f, 1f);
        public static readonly Color Positive = new Color(0.18f, 0.35f, 0.26f, 1f);
        public static readonly Color Storage = new Color(0.2f, 0.23f, 0.29f, 1f);
        public static readonly Color Settings = new Color(0.11f, 0.17f, 0.24f, 1f);
        public static readonly Color Tool = new Color(0.29f, 0.29f, 0.46f, 1f);
        public static readonly Color Obstacle = new Color(0.39f, 0.25f, 0.22f, 1f);

        public static float ContrastRatio(Color foreground, Color background)
        {
            float foregroundLuminance = RelativeLuminance(foreground);
            float backgroundLuminance = RelativeLuminance(background);
            float lighter = Mathf.Max(foregroundLuminance, backgroundLuminance);
            float darker = Mathf.Min(foregroundLuminance, backgroundLuminance);
            return (lighter + 0.05f) / (darker + 0.05f);
        }

        private static float RelativeLuminance(Color color)
        {
            return 0.2126f * LinearChannel(color.r)
                + 0.7152f * LinearChannel(color.g)
                + 0.0722f * LinearChannel(color.b);
        }

        private static float LinearChannel(float channel)
        {
            return channel <= 0.03928f
                ? channel / 12.92f
                : Mathf.Pow((channel + 0.055f) / 1.055f, 2.4f);
        }
    }
}
