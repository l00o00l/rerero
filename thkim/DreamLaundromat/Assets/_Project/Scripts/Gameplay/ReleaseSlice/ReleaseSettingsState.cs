namespace Thkim.DreamLaundromat.Gameplay.ReleaseSlice
{
    public sealed class ReleaseSettingsState
    {
        public bool SoundEnabled = true;
        public bool HapticsEnabled = true;
        public bool ReducedMotion;
        public bool HighContrast;
        public bool LargeText;

        public ReleaseSettingsState Clone()
        {
            return new ReleaseSettingsState
            {
                SoundEnabled = SoundEnabled,
                HapticsEnabled = HapticsEnabled,
                ReducedMotion = ReducedMotion,
                HighContrast = HighContrast,
                LargeText = LargeText
            };
        }
    }
}
