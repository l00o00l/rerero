using System;
using System.Collections.Generic;
using UnityEngine;

namespace Thkim.DreamLaundromat.Gameplay.ReleaseSlice
{
    public interface IReleaseProgressStore
    {
        ReleaseProgressState Load();
        void Save(ReleaseProgressState state);
    }

    public sealed class ReleaseMemoryProgressStore : IReleaseProgressStore
    {
        private ReleaseProgressState state = new ReleaseProgressState();

        public ReleaseProgressState Load()
        {
            return state.Clone();
        }

        public void Save(ReleaseProgressState value)
        {
            state = value?.Clone() ?? new ReleaseProgressState();
        }
    }

    public sealed class ReleasePlayerPrefsProgressStore : IReleaseProgressStore
    {
        private const string Key = "DreamLaundromat.ReleaseProgress.v1";

        public ReleaseProgressState Load()
        {
            string json = PlayerPrefs.GetString(Key, string.Empty);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new ReleaseProgressState();
            }

            try
            {
                ReleaseProgressSaveData data = JsonUtility.FromJson<ReleaseProgressSaveData>(json);
                return data?.ToState() ?? new ReleaseProgressState();
            }
            catch (ArgumentException)
            {
                return new ReleaseProgressState();
            }
        }

        public void Save(ReleaseProgressState state)
        {
            var data = ReleaseProgressSaveData.FromState(state ?? new ReleaseProgressState());
            PlayerPrefs.SetString(Key, JsonUtility.ToJson(data));
            PlayerPrefs.Save();
        }

        [Serializable]
        private sealed class ReleaseProgressSaveData
        {
            public int highestUnlockedLevelIndex;
            public List<string> completedLevelIds = new List<string>();
            public bool soundEnabled = true;
            public bool hapticsEnabled = true;
            public bool reducedMotion;
            public bool highContrast;
            public bool largeText;

            public static ReleaseProgressSaveData FromState(ReleaseProgressState state)
            {
                return new ReleaseProgressSaveData
                {
                    highestUnlockedLevelIndex = state.HighestUnlockedLevelIndex,
                    completedLevelIds = new List<string>(state.CompletedLevelIds),
                    soundEnabled = state.Settings == null || state.Settings.SoundEnabled,
                    hapticsEnabled = state.Settings == null || state.Settings.HapticsEnabled,
                    reducedMotion = state.Settings != null && state.Settings.ReducedMotion,
                    highContrast = state.Settings != null && state.Settings.HighContrast,
                    largeText = state.Settings != null && state.Settings.LargeText
                };
            }

            public ReleaseProgressState ToState()
            {
                var state = new ReleaseProgressState
                {
                    HighestUnlockedLevelIndex = Math.Max(0, highestUnlockedLevelIndex),
                    Settings = new ReleaseSettingsState
                    {
                        SoundEnabled = soundEnabled,
                        HapticsEnabled = hapticsEnabled,
                        ReducedMotion = reducedMotion,
                        HighContrast = highContrast,
                        LargeText = largeText
                    }
                };
                if (completedLevelIds != null)
                {
                    state.CompletedLevelIds.AddRange(completedLevelIds);
                }

                return state;
            }
        }
    }
}
