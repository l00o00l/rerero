using Thkim.DreamLaundromat.DynamicLab;

namespace Thkim.DreamLaundromat.Gameplay.ReleaseSlice
{
    public sealed class ReleaseGameSession
    {
        private readonly ReleaseLevelPack levelPack;
        private readonly IReleaseProgressStore progressStore;
        private readonly IReleaseFeedbackSink feedbackSink;
        private int currentLevelIndex = -1;
        private int guidedActionIndex;

        public ReleaseGameSession(
            ReleaseLevelPack levelPack,
            IReleaseProgressStore progressStore = null,
            IReleaseFeedbackSink feedbackSink = null)
        {
            this.levelPack = levelPack ?? throw new System.ArgumentNullException(nameof(levelPack));
            this.progressStore = progressStore ?? new ReleaseMemoryProgressStore();
            this.feedbackSink = feedbackSink;
            Progress = this.progressStore.Load() ?? new ReleaseProgressState();
        }

        public int CurrentLevelIndex => currentLevelIndex;
        public ReleaseLevelDefinition CurrentLevel { get; private set; }
        public DynamicRoundState CurrentState { get; private set; }
        public ReleaseProgressState Progress { get; }
        public string LastMessage { get; private set; } = string.Empty;
        public bool LastActionSucceeded { get; private set; }
        public bool HasStarted => CurrentState != null;
        public bool HasNextLevel => currentLevelIndex >= 0 && currentLevelIndex + 1 < levelPack.Levels.Count;
        public bool HasPendingGuidedAction => CurrentLevel != null
            && guidedActionIndex < CurrentLevel.GuidedActionRules.Length;
        public ReleaseGuidedActionRule PendingGuidedAction => HasPendingGuidedAction
            ? CurrentLevel.GuidedActionRules[guidedActionIndex]
            : null;

        public int GetDefaultStartLevelIndex()
        {
            if (levelPack.Levels.Count == 0)
            {
                return 0;
            }

            return System.Math.Min(
                System.Math.Max(0, Progress.HighestUnlockedLevelIndex),
                levelPack.Levels.Count - 1);
        }

        public void StartDefaultLevel()
        {
            StartLevel(GetDefaultStartLevelIndex());
        }

        public void StartLevel(int levelIndex)
        {
            if (!Progress.IsLevelUnlocked(levelIndex))
            {
                throw new System.InvalidOperationException($"Level {levelIndex} is locked.");
            }

            CurrentLevel = levelPack.GetLevel(levelIndex);
            currentLevelIndex = levelIndex;
            guidedActionIndex = 0;
            CurrentState = DynamicRoundInitializer.CreateInitialState(CurrentLevel.CreateRoundDefinition());
            LastActionSucceeded = true;
            LastMessage = CurrentLevel.Guidance;
            Emit(ReleaseFeedbackEventType.LevelStarted, LastMessage);
        }

        public void RestartLevel()
        {
            if (currentLevelIndex < 0)
            {
                StartLevel(0);
                return;
            }

            StartLevel(currentLevelIndex);
        }

        public bool TryStartNextLevel()
        {
            if (!HasNextLevel)
            {
                LastActionSucceeded = false;
                LastMessage = "No next level is available.";
                return false;
            }

            if (CurrentState != null && CurrentState.Status != DynamicRoundStatus.Cleared)
            {
                LastActionSucceeded = false;
                LastMessage = "Clear the current level before moving to the next level.";
                return false;
            }

            StartLevel(currentLevelIndex + 1);
            return true;
        }

        public DynamicActionResult Apply(DynamicPlayerAction action)
        {
            if (CurrentState == null)
            {
                DynamicActionResult missingState = DynamicActionResult.Failed("No level is running.");
                RecordResult(missingState);
                return missingState;
            }

            DynamicActionResult guidedResult = ValidateGuidedAction(action);
            if (!guidedResult.Success)
            {
                RecordResult(guidedResult);
                Emit(ReleaseFeedbackEventType.ActionFailed, guidedResult.Message);
                return guidedResult;
            }

            DynamicActionResult result = DynamicRulesEngine.Apply(CurrentState, action);
            if (result.Success && HasPendingGuidedAction)
            {
                guidedActionIndex++;
            }

            RecordResult(result);
            if (result.Success)
            {
                Emit(ReleaseFeedbackEventType.ActionSucceeded, result.Message);
                if (CurrentState.Status == DynamicRoundStatus.Cleared)
                {
                    SaveCompletion();
                    Emit(ReleaseFeedbackEventType.LevelCleared, CurrentLevel.LevelId);
                }
            }
            else
            {
                Emit(ReleaseFeedbackEventType.ActionFailed, result.Message);
            }

            return result;
        }

        public void UpdateSettings(ReleaseSettingsState settings)
        {
            Progress.Settings = settings?.Clone() ?? new ReleaseSettingsState();
            progressStore.Save(Progress);
            Emit(ReleaseFeedbackEventType.ProgressSaved, "Settings saved.");
        }

        private DynamicActionResult ValidateGuidedAction(DynamicPlayerAction action)
        {
            ReleaseGuidedActionRule rule = PendingGuidedAction;
            if (rule == null || rule.Matches(action))
            {
                return DynamicActionResult.Succeeded("Tutorial step accepted.");
            }

            string prompt = string.IsNullOrWhiteSpace(rule.Prompt)
                ? rule.Describe()
                : rule.Prompt;
            return DynamicActionResult.Failed($"Tutorial asks for {prompt}.");
        }

        private void RecordResult(DynamicActionResult result)
        {
            LastActionSucceeded = result.Success;
            LastMessage = result.Message;
            if (!result.Success && CurrentState != null && CurrentState.Status == DynamicRoundStatus.Failed)
            {
                LastMessage = $"{result.Message} ({CurrentState.FailureReason})";
            }
        }

        private void SaveCompletion()
        {
            bool changed = Progress.MarkCompleted(
                CurrentLevel.LevelId,
                currentLevelIndex,
                levelPack.Levels.Count);
            if (!changed)
            {
                return;
            }

            progressStore.Save(Progress);
            Emit(ReleaseFeedbackEventType.ProgressSaved, CurrentLevel.LevelId);
        }

        private void Emit(ReleaseFeedbackEventType type, string message)
        {
            feedbackSink?.Emit(
                new ReleaseFeedbackEvent(type, CurrentLevel?.LevelId ?? string.Empty, message),
                Progress.Settings);
        }
    }
}
