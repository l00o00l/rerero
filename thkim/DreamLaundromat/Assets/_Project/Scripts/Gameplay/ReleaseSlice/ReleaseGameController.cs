using System;
using System.Collections.Generic;
using System.IO;
using Thkim.DreamLaundromat.DynamicLab;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Thkim.DreamLaundromat.Gameplay.ReleaseSlice
{
    public sealed class ReleaseGameController : MonoBehaviour
    {
        [SerializeField] private ReleaseUiArtCatalog artCatalog;

        private ReleaseGameSession session;
        private ReleaseLevelPack levelPack;
        private Font font;
        private RectTransform safeAreaRoot;
        private Rect lastSafeArea = new Rect(-1f, -1f, -1f, -1f);
        private RectTransform homeRoot;
        private RectTransform levelSelectRoot;
        private RectTransform gameplayRoot;
        private RectTransform pauseRoot;
        private RectTransform resultRoot;
        private Transform dreamRoot;
        private Transform orderRoot;
        private Transform previewRoot;
        private Transform storageRoot;
        private Transform modifierRoot;
        private Transform actionRoot;
        private RectTransform gameplayHeader;
        private Text homeProgressText;
        private Text continueButtonText;
        private Text levelSelectProgressText;
        private Text headerText;
        private Text guidanceText;
        private Text messageText;
        private Text resultTitleText;
        private Text resultSubtitleText;
        private Text resultDetailText;
        private Text pauseSoundText;
        private Text pauseHapticsText;
        private Text pauseContrastText;
        private Image resultIconImage;
        private Button restartButton;
        private Button nextButton;
        private Button resultNextButton;
        private Button resultReplayButton;
        private ReleaseFeedbackPresenter feedbackPresenter;
        private readonly List<Button> levelSelectButtons = new List<Button>(30);
        private readonly List<Text> levelSelectButtonLabels = new List<Text>(30);
        private readonly ReleaseSelectionState selection = new ReleaseSelectionState();
        private readonly List<GameObject> activeDynamicUi = new List<GameObject>(96);
        private readonly Stack<GameObject> pooledPanels = new Stack<GameObject>(96);
        private readonly Stack<GameObject> pooledTexts = new Stack<GameObject>(96);
        private readonly Stack<GameObject> pooledButtons = new Stack<GameObject>(96);
        private readonly Stack<GameObject> pooledIcons = new Stack<GameObject>(96);
        private readonly List<RaycastResult> dragRaycastResults = new List<RaycastResult>(16);
        private bool useDynamicUiPool;
        private ReleaseScreenMode currentScreen;
        private static IReleaseProgressStore progressStoreOverrideForTests;
        private static IReleaseFeedbackSink feedbackSinkOverrideForTests;
        private const string ScreenshotStartLevelOverrideFileName = "release-screenshot-level.txt";

        public string CurrentLevelId => session?.CurrentLevel?.LevelId ?? string.Empty;
        public int CurrentLevelIndex => session?.CurrentLevelIndex ?? -1;
        public DynamicRoundStatus CurrentStatus => session?.CurrentState?.Status ?? DynamicRoundStatus.Ready;
        public int CompletedOrders => session?.CurrentState?.CompletedOrders ?? 0;
        public string CurrentScreenForTest => currentScreen.ToString();
        public bool HasReleaseArtCatalogForTest => artCatalog != null && artCatalog.IsComplete;
        public int DynamicUiObjectCountForTest => activeDynamicUi.Count
            + pooledPanels.Count
            + pooledTexts.Count
            + pooledButtons.Count
            + pooledIcons.Count;
        private int selectedDreamSlotId => selection.SelectedDreamSlotId;
        private int selectedOrderSlotId => selection.SelectedOrderSlotId;
        private int selectedStorageSlotId => selection.SelectedStorageSlotId;

        public void ConfigureArtCatalog(ReleaseUiArtCatalog catalog)
        {
            artCatalog = catalog;
        }

        public static void ConfigureTestServices(
            IReleaseProgressStore progressStore,
            IReleaseFeedbackSink feedbackSink = null)
        {
            progressStoreOverrideForTests = progressStore;
            feedbackSinkOverrideForTests = feedbackSink;
        }

        public static void ClearTestServices()
        {
            progressStoreOverrideForTests = null;
            feedbackSinkOverrideForTests = null;
        }

        private void Awake()
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            levelPack = ReleaseLevelPack.CreateDefault();
            session = new ReleaseGameSession(
                levelPack,
                progressStoreOverrideForTests ?? new ReleasePlayerPrefsProgressStore(),
                feedbackSinkOverrideForTests ?? new ReleaseUnityFeedbackSink());
            EnsureEventSystem();
            BuildUi();
        }

        private void Start()
        {
            if (!session.HasStarted)
            {
                int screenshotLevelOverride = TryConsumeScreenshotStartLevelOverride();
                if (screenshotLevelOverride >= 0)
                {
                    session.Progress.HighestUnlockedLevelIndex = Mathf.Max(
                        session.Progress.HighestUnlockedLevelIndex,
                        screenshotLevelOverride);
                    LoadLevel(screenshotLevelOverride);
                    ShowGameplayScreen();
                    return;
                }

                LoadLevel(session.GetDefaultStartLevelIndex());
            }

            ShowHomeScreen();
        }

        private void Update()
        {
            ApplySafeAreaIfNeeded();
        }

        public bool TryLoadLevelForTest(int index)
        {
            if (index < 0 || index >= levelPack.Levels.Count)
            {
                return false;
            }

            session.Progress.HighestUnlockedLevelIndex = Mathf.Max(
                session.Progress.HighestUnlockedLevelIndex,
                index);
            LoadLevel(index);
            ShowGameplayScreen();
            return true;
        }

        public bool TryApplyForTest(DynamicPlayerAction action)
        {
            if (!session.HasStarted)
            {
                return false;
            }

            DynamicActionResult result = ApplyAction(action);
            return result.Success;
        }

        public bool TryDragDreamToOrderForTest(int dreamSlotId, int orderSlotId)
        {
            return TryApplyDragAction(
                new ReleaseDragPayload(ReleaseDragSourceKind.ActiveDream, dreamSlotId),
                new ReleaseDropTargetDescriptor(ReleaseDropTargetKind.ActiveOrder, orderSlotId));
        }

        public bool TryDragDreamToStorageForTest(int dreamSlotId, int storageSlotId)
        {
            return TryApplyDragAction(
                new ReleaseDragPayload(ReleaseDragSourceKind.ActiveDream, dreamSlotId),
                new ReleaseDropTargetDescriptor(ReleaseDropTargetKind.Storage, storageSlotId));
        }

        public bool TryDragStorageToDreamForTest(int storageSlotId, int dreamSlotId)
        {
            return TryApplyDragAction(
                new ReleaseDragPayload(ReleaseDragSourceKind.Storage, storageSlotId),
                new ReleaseDropTargetDescriptor(ReleaseDropTargetKind.ActiveDream, dreamSlotId));
        }

        public bool TrySelectDreamForTest(int slotId)
        {
            if (!session.HasStarted || session.CurrentState.FindActiveDreamSlot(slotId) == null)
            {
                return false;
            }

            OnDreamSlotSelected(slotId);
            return true;
        }

        public bool TrySelectOrderForTest(int slotId)
        {
            if (!session.HasStarted || session.CurrentState.FindActiveOrderSlot(slotId) == null)
            {
                return false;
            }

            OnOrderSlotSelected(slotId);
            return true;
        }

        public bool TryOpenGameplayForTest()
        {
            if (!session.HasStarted)
            {
                return false;
            }

            ShowGameplayScreen();
            return true;
        }

        public bool TryOpenLevelSelectForTest()
        {
            if (levelPack == null || levelPack.Levels.Count == 0)
            {
                return false;
            }

            ShowLevelSelectScreen();
            return true;
        }

        public bool TrySelectLevelForTest(int index)
        {
            if (!CanOpenLevel(index))
            {
                return false;
            }

            OpenLevel(index);
            return true;
        }

        public bool TryOpenPauseForTest()
        {
            if (!session.HasStarted)
            {
                return false;
            }

            ShowPauseScreen();
            return true;
        }

        public bool TryResumeForTest()
        {
            if (currentScreen != ReleaseScreenMode.Pause)
            {
                return false;
            }

            ShowGameplayScreen();
            return true;
        }

        public bool TryResultNextForTest()
        {
            if (currentScreen != ReleaseScreenMode.ClearResult)
            {
                return false;
            }

            OnResultNext();
            return true;
        }

        public bool TryResultReplayForTest()
        {
            if (currentScreen != ReleaseScreenMode.ClearResult && currentScreen != ReleaseScreenMode.FailResult)
            {
                return false;
            }

            OnResultReplay();
            return true;
        }

        private void LoadLevel(int index)
        {
            session.StartLevel(index);
            ClearSelection();
            RefreshUi(session.CurrentLevel.Guidance);
            RefreshHomeUi();
            RefreshLevelSelectButtons();
        }

        private int TryConsumeScreenshotStartLevelOverride()
        {
            string path = Path.Combine(Application.persistentDataPath, ScreenshotStartLevelOverrideFileName);
            if (!File.Exists(path))
            {
                return -1;
            }

            string rawLevelIndex = string.Empty;
            try
            {
                rawLevelIndex = File.ReadAllText(path).Trim();
                File.Delete(path);
            }
            catch (IOException)
            {
                return -1;
            }

            if (!int.TryParse(rawLevelIndex, out int levelIndex)
                || levelPack == null
                || levelPack.Levels.Count == 0)
            {
                return -1;
            }

            return Mathf.Clamp(levelIndex, 0, levelPack.Levels.Count - 1);
        }

        private DynamicActionResult ApplyAction(DynamicPlayerAction action)
        {
            if (!session.HasStarted)
            {
                return DynamicActionResult.Failed("No level is running.");
            }

            DynamicActionResult result = session.Apply(action);
            if (result.Success
                && (action.Type == DynamicActionType.SubmitDream
                || action.Type == DynamicActionType.StoreDream
                || action.Type == DynamicActionType.RecallDream
                || action.Type == DynamicActionType.UseItem))
            {
                ClearSelection();
            }

            RefreshUi(result.Message);
            PresentGameplayFeedback(result);
            if (session.CurrentState.Status == DynamicRoundStatus.Cleared
                || session.CurrentState.Status == DynamicRoundStatus.Failed)
            {
                ShowResultScreen(result.Message);
            }

            return result;
        }

        private void BuildUi()
        {
            GameObject canvasObject = new GameObject("ReleaseGameplayCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.55f;

            safeAreaRoot = CreatePanel(canvasObject.transform, "SafeArea", ReleaseVisualStyle.Background);
            ApplyPanelSprite(safeAreaRoot, artCatalog?.GameplayBackground, Color.white);
            safeAreaRoot.anchorMin = Vector2.zero;
            safeAreaRoot.anchorMax = Vector2.one;
            safeAreaRoot.offsetMin = Vector2.zero;
            safeAreaRoot.offsetMax = Vector2.zero;
            AddVerticalLayout(safeAreaRoot, new RectOffset(28, 28, 24, 24), 12f);
            ApplySafeAreaIfNeeded();

            homeRoot = CreatePanel(safeAreaRoot, "HomeScreen", Color.white);
            ApplyPanelSprite(homeRoot, artCatalog?.TitleBackground, Color.white);
            SetFlexibleHeight(homeRoot.gameObject, 1f);
            AddVerticalLayout(homeRoot, new RectOffset(28, 28, 34, 34), 16f);
            BuildHomeScreen(homeRoot);

            levelSelectRoot = CreatePanel(safeAreaRoot, "LevelSelectScreen", Color.white);
            ApplyPanelSprite(levelSelectRoot, artCatalog?.LevelSelectBackground, Color.white);
            SetFlexibleHeight(levelSelectRoot.gameObject, 1f);
            AddVerticalLayout(levelSelectRoot, new RectOffset(24, 24, 24, 24), 12f);
            BuildLevelSelectScreen(levelSelectRoot);

            gameplayRoot = CreatePanel(safeAreaRoot, "GameplayScreen", new Color(0f, 0f, 0f, 0f));
            SetFlexibleHeight(gameplayRoot.gameObject, 1f);
            AddVerticalLayout(gameplayRoot, new RectOffset(0, 0, 0, 0), 12f);
            BuildGameplayScreen(gameplayRoot);

            pauseRoot = CreatePanel(safeAreaRoot, "PauseScreen", new Color(0.025f, 0.03f, 0.042f, 0.94f));
            SetFlexibleHeight(pauseRoot.gameObject, 1f);
            AddVerticalLayout(pauseRoot, new RectOffset(28, 28, 220, 28), 14f);
            BuildPauseScreen(pauseRoot);

            resultRoot = CreatePanel(safeAreaRoot, "ResultScreen", new Color(0.03f, 0.036f, 0.048f, 0.94f));
            SetFlexibleHeight(resultRoot.gameObject, 1f);
            AddVerticalLayout(resultRoot, new RectOffset(28, 28, 180, 28), 16f);
            BuildResultScreen(resultRoot);

            ShowScreen(ReleaseScreenMode.Home);
        }

        private void BuildHomeScreen(RectTransform root)
        {
            RectTransform hero = CreatePanel(root, "HomeHero", new Color(0f, 0f, 0f, 0f));
            SetFlexibleHeight(hero.gameObject, 1f);
            AddVerticalLayout(hero, new RectOffset(22, 22, 300, 24), 12f);
            CreateText(hero, "HomeTitle", "Dream Laundromat", 58, TextAnchor.MiddleLeft);
            CreateText(hero, "HomeSubtitle", "Night orders ready", 28, TextAnchor.MiddleLeft);
            homeProgressText = CreateText(hero, "HomeProgress", string.Empty, 24, TextAnchor.MiddleLeft);

            RectTransform actions = CreatePanel(root, "HomeActions", new Color(0.035f, 0.045f, 0.06f, 0.58f));
            SetPreferredHeight(actions.gameObject, 260f);
            AddVerticalLayout(actions, new RectOffset(18, 18, 18, 18), 10f);
            Button continueButton = CreateButton(
                actions,
                "Continue",
                "Continue",
                OnContinue,
                ReleaseVisualStyle.Positive,
                null,
                artCatalog?.SubmitButtonFrame);
            continueButtonText = continueButton.GetComponentInChildren<Text>(true);
            CreateButton(actions, "LevelSelect", "Level Select", ShowLevelSelectScreen, ReleaseVisualStyle.Action, null, artCatalog?.NavigationButtonFrame);
        }

        private void BuildLevelSelectScreen(RectTransform root)
        {
            RectTransform header = CreatePanel(root, "LevelSelectHeader", new Color(0.05f, 0.058f, 0.074f, 0.8f));
            SetPreferredHeight(header.gameObject, 168f);
            AddVerticalLayout(header, new RectOffset(18, 18, 14, 14), 8f);
            CreateText(header, "LevelSelectTitle", "Level Select", 40, TextAnchor.MiddleLeft);
            levelSelectProgressText = CreateText(header, "LevelSelectProgress", string.Empty, 23, TextAnchor.MiddleLeft);

            RectTransform scrollContent = CreateScrollContent(root, "LevelSelectScroll");
            for (int i = 0; i < levelPack.Levels.Count; i++)
            {
                int levelIndex = i;
                Button button = CreateButton(
                    scrollContent,
                    $"Level-{levelPack.Levels[i].LevelId}",
                    BuildLevelSelectLabel(i),
                    () => OpenLevel(levelIndex),
                    ReleaseVisualStyle.Action);
                levelSelectButtons.Add(button);
                levelSelectButtonLabels.Add(button.GetComponentInChildren<Text>(true));
            }

            RectTransform footer = CreateRow(root, "LevelSelectFooter", 82f);
            CreateButton(footer, "Home", "Home", ShowHomeScreen, ReleaseVisualStyle.Settings, null, artCatalog?.NavigationButtonFrame);
            CreateButton(footer, "Continue", "Continue", OnContinue, ReleaseVisualStyle.Positive, null, artCatalog?.SubmitButtonFrame);
        }

        private void BuildGameplayScreen(RectTransform root)
        {
            gameplayHeader = CreatePanel(root, "Header", ReleaseVisualStyle.Panel);
            ApplyPanelSprite(gameplayHeader, artCatalog?.TitleBackground, new Color(1f, 1f, 1f, 0.42f));
            SetPreferredHeight(gameplayHeader.gameObject, 106f);
            AddVerticalLayout(gameplayHeader, new RectOffset(18, 18, 10, 10), 4f);

            headerText = CreateText(gameplayHeader, "HeaderText", "Dream Laundromat", 30, TextAnchor.MiddleLeft);
            restartButton = CreateHeaderActionButton(gameplayHeader, "Restart", "Restart", OnRestart, new Color(0.25f, 0.25f, 0.31f, 1f), 168f, 78f);
            CreateHeaderActionButton(gameplayHeader, "Levels", "Levels", ShowLevelSelectScreen, ReleaseVisualStyle.Action, 92f, 70f);
            CreateHeaderActionButton(gameplayHeader, "Pause", "Pause", ShowPauseScreen, ReleaseVisualStyle.Settings, 18f, 68f);
            nextButton = CreateHeaderActionButton(gameplayHeader, "Next", "Next", OnNext, new Color(0.18f, 0.34f, 0.27f, 1f), 18f, 58f);
            nextButton.gameObject.SetActive(false);

            guidanceText = CreateText(gameplayHeader, "GuidanceText", string.Empty, 18, TextAnchor.MiddleLeft);
            messageText = CreateText(gameplayHeader, "MessageText", string.Empty, 18, TextAnchor.MiddleLeft);
            messageText.gameObject.SetActive(false);
            feedbackPresenter = GetOrAddComponent<ReleaseFeedbackPresenter>(gameObject);
            feedbackPresenter.Configure(messageText);

            RectTransform content = CreatePanel(root, "Content", ReleaseVisualStyle.Content);
            SetFlexibleHeight(content.gameObject, 1f);
            AddVerticalLayout(content, new RectOffset(12, 12, 10, 10), 8f);

            dreamRoot = CreateSection(content, "ActiveDreams", 268f);
            orderRoot = CreateSection(content, "ActiveOrders", 208f);
            previewRoot = CreateSection(content, "FocusPreview", 76f);
            storageRoot = CreateSection(content, "Storage", 86f);
            modifierRoot = CreateSection(content, "ToolsAndObstacles", 76f);

            actionRoot = CreatePanel(root, "ActionPanel", ReleaseVisualStyle.Panel);
            SetPreferredHeight(actionRoot.gameObject, 166f);
            AddVerticalLayout((RectTransform)actionRoot, new RectOffset(12, 12, 8, 8), 6f);
        }

        private void BuildPauseScreen(RectTransform root)
        {
            RectTransform panel = CreatePanel(root, "PausePanel", new Color(0.07f, 0.08f, 0.105f, 0.9f));
            SetFlexibleHeight(panel.gameObject, 1f);
            AddVerticalLayout(panel, new RectOffset(24, 24, 26, 26), 12f);
            CreateText(panel, "PauseTitle", "Pause", 48, TextAnchor.MiddleLeft);
            CreateText(panel, "PauseSubtitle", "Take a breath before the next order.", 23, TextAnchor.MiddleLeft);
            CreateButton(panel, "Resume", "Resume", ShowGameplayScreen, ReleaseVisualStyle.Positive);
            CreateButton(panel, "Restart", "Restart", OnPauseRestart, new Color(0.25f, 0.25f, 0.31f, 1f));
            CreateButton(panel, "LevelSelect", "Level Select", ShowLevelSelectScreen, ReleaseVisualStyle.Action);
            CreateButton(panel, "Home", "Home", ShowHomeScreen, ReleaseVisualStyle.Settings);

            RectTransform settings = CreatePanel(panel, "PauseSettings", new Color(0.04f, 0.05f, 0.07f, 0.72f));
            SetPreferredHeight(settings.gameObject, 210f);
            AddVerticalLayout(settings, new RectOffset(12, 12, 12, 12), 8f);
            CreateText(settings, "PauseSettingsTitle", "Settings", 22, TextAnchor.MiddleLeft);
            RectTransform row = CreateRow(settings, "PauseSettingsRow", 92f);
            pauseSoundText = CreateButton(row, "PauseSoundToggle", BuildSettingLabel("Sound", session.Progress.Settings.SoundEnabled), ToggleSound, ReleaseVisualStyle.Settings).GetComponentInChildren<Text>(true);
            pauseHapticsText = CreateButton(row, "PauseHapticsToggle", BuildSettingLabel("Haptic", session.Progress.Settings.HapticsEnabled), ToggleHaptics, ReleaseVisualStyle.Settings).GetComponentInChildren<Text>(true);
            pauseContrastText = CreateButton(row, "PauseContrastToggle", BuildSettingLabel("Contrast", session.Progress.Settings.HighContrast), ToggleContrast, ReleaseVisualStyle.Settings).GetComponentInChildren<Text>(true);
        }

        private void BuildResultScreen(RectTransform root)
        {
            RectTransform panel = CreatePanel(root, "ResultPanel", new Color(0.06f, 0.07f, 0.095f, 0.9f));
            SetFlexibleHeight(panel.gameObject, 1f);
            AddVerticalLayout(panel, new RectOffset(24, 24, 28, 28), 14f);

            resultIconImage = CreateLayoutIcon(panel, "ResultIcon", artCatalog?.EffectClearGlow, 112f, 112f);
            resultTitleText = CreateText(panel, "ResultTitle", "Result", 52, TextAnchor.MiddleLeft);
            resultSubtitleText = CreateText(panel, "ResultSubtitle", string.Empty, 25, TextAnchor.MiddleLeft);
            resultDetailText = CreateText(panel, "ResultDetail", string.Empty, 24, TextAnchor.MiddleLeft);

            RectTransform actions = CreatePanel(panel, "ResultActions", new Color(0.035f, 0.043f, 0.06f, 0.74f));
            SetPreferredHeight(actions.gameObject, 238f);
            AddVerticalLayout(actions, new RectOffset(12, 12, 12, 12), 10f);
            resultNextButton = CreateButton(actions, "ResultNext", "Next", OnResultNext, ReleaseVisualStyle.Positive, null, artCatalog?.SubmitButtonFrame);
            resultReplayButton = CreateButton(actions, "ResultReplay", "Replay", OnResultReplay, new Color(0.25f, 0.25f, 0.31f, 1f), null, artCatalog?.NavigationButtonFrame);
            CreateButton(actions, "ResultLevels", "Level Select", ShowLevelSelectScreen, ReleaseVisualStyle.Action, null, artCatalog?.NavigationButtonFrame);
        }

        private void RefreshUi(string message)
        {
            if (!session.HasStarted)
            {
                return;
            }

            BeginDynamicUiRefresh();

            DynamicRoundState state = session.CurrentState;
            ReleaseGameplayViewModel viewModel = ReleaseGameplayViewModel.Create(state, selection);
            headerText.text = $"{session.CurrentLevel.DisplayName}  {session.CurrentLevel.LevelId}";
            SetTextAndHeight(guidanceText, BuildCompactGameplayGuidance(state), 21);
            SetGameplayMessage(ReleaseGameplayCardRenderer.BuildStatusMessage(state, BuildVisibleGameplayMessage(message)));

            RenderDreams(viewModel);
            RenderOrders(viewModel);
            RenderPreview(viewModel);
            RenderStorage(viewModel);
            RenderModifiers(viewModel);
            RenderActions(viewModel);

            EndDynamicUiRefresh();
            restartButton.interactable = true;
            bool canAdvance = state.Status == DynamicRoundStatus.Cleared && session.HasNextLevel;
            nextButton.gameObject.SetActive(canAdvance);
            nextButton.interactable = canAdvance;
            RefreshHomeUi();
            RefreshLevelSelectButtons();
        }

        private void ShowHomeScreen()
        {
            RefreshHomeUi();
            ShowScreen(ReleaseScreenMode.Home);
        }

        private void ShowLevelSelectScreen()
        {
            RefreshLevelSelectButtons();
            ShowScreen(ReleaseScreenMode.LevelSelect);
        }

        private void ShowGameplayScreen()
        {
            if (!session.HasStarted)
            {
                LoadLevel(session.GetDefaultStartLevelIndex());
            }

            ShowScreen(ReleaseScreenMode.Gameplay);
        }

        private void ShowPauseScreen()
        {
            if (!session.HasStarted)
            {
                LoadLevel(session.GetDefaultStartLevelIndex());
            }

            RefreshPauseSettingsUi();
            ShowScreen(ReleaseScreenMode.Pause);
        }

        private void ShowResultScreen(string message)
        {
            ReleaseResultSummary summary = ReleaseResultSummary.Create(
                session.CurrentLevel,
                session.CurrentState,
                message,
                session.HasNextLevel);
            resultTitleText.text = summary.Title;
            resultSubtitleText.text = summary.Subtitle;
            resultDetailText.text = summary.Detail;
            ApplyTextHeight(resultTitleText, 52);
            ApplyTextHeight(resultSubtitleText, 25);
            ApplyTextHeight(resultDetailText, 24);
            if (resultIconImage != null)
            {
                resultIconImage.sprite = summary.Status == DynamicRoundStatus.Cleared
                    ? artCatalog?.EffectClearGlow
                    : artCatalog?.EffectFailWarning;
            }

            resultNextButton.gameObject.SetActive(summary.Status == DynamicRoundStatus.Cleared);
            resultNextButton.interactable = summary.CanAdvance;
            Text replayText = resultReplayButton.GetComponentInChildren<Text>(true);
            replayText.text = summary.Status == DynamicRoundStatus.Cleared ? "Replay" : "Retry";
            ApplyTextHeight(replayText, 18);
            ShowScreen(summary.Status == DynamicRoundStatus.Cleared
                ? ReleaseScreenMode.ClearResult
                : ReleaseScreenMode.FailResult);
        }

        private void ShowScreen(ReleaseScreenMode mode)
        {
            currentScreen = mode;
            if (homeRoot != null)
            {
                homeRoot.gameObject.SetActive(mode == ReleaseScreenMode.Home);
            }

            if (levelSelectRoot != null)
            {
                levelSelectRoot.gameObject.SetActive(mode == ReleaseScreenMode.LevelSelect);
            }

            if (gameplayRoot != null)
            {
                gameplayRoot.gameObject.SetActive(mode == ReleaseScreenMode.Gameplay);
            }

            if (pauseRoot != null)
            {
                pauseRoot.gameObject.SetActive(mode == ReleaseScreenMode.Pause);
            }

            if (resultRoot != null)
            {
                resultRoot.gameObject.SetActive(mode == ReleaseScreenMode.ClearResult
                    || mode == ReleaseScreenMode.FailResult);
            }
        }

        private void OnContinue()
        {
            if (!session.HasStarted)
            {
                LoadLevel(session.GetDefaultStartLevelIndex());
            }

            ShowGameplayScreen();
        }

        private void OpenLevel(int index)
        {
            if (!CanOpenLevel(index))
            {
                return;
            }

            LoadLevel(index);
            ShowGameplayScreen();
        }

        private bool CanOpenLevel(int index)
        {
            return levelPack != null
                && index >= 0
                && index < levelPack.Levels.Count
                && session != null
                && index <= session.Progress.HighestUnlockedLevelIndex;
        }

        private void RefreshHomeUi()
        {
            if (homeProgressText == null || session == null || levelPack == null)
            {
                return;
            }

            int highestUnlocked = Mathf.Clamp(session.Progress.HighestUnlockedLevelIndex, 0, levelPack.Levels.Count - 1);
            string currentLevel = session.HasStarted && session.CurrentLevel != null
                ? $"{session.CurrentLevel.LevelId}  {session.CurrentLevel.DisplayName}"
                : "No open order";
            homeProgressText.text = $"Open Orders {highestUnlocked + 1}/{levelPack.Levels.Count}\nCurrent: {currentLevel}";
            ApplyTextHeight(homeProgressText, 24);

            if (continueButtonText != null)
            {
                continueButtonText.text = $"Continue\n{currentLevel}";
                ApplyTextHeight(continueButtonText, 18);
            }
        }

        private void RefreshLevelSelectButtons()
        {
            if (levelSelectProgressText == null || session == null || levelPack == null)
            {
                return;
            }

            int highestUnlocked = Mathf.Clamp(session.Progress.HighestUnlockedLevelIndex, 0, levelPack.Levels.Count - 1);
            levelSelectProgressText.text = $"Open Orders {highestUnlocked + 1}/{levelPack.Levels.Count}";
            ApplyTextHeight(levelSelectProgressText, 23);

            for (int i = 0; i < levelSelectButtons.Count; i++)
            {
                bool unlocked = i <= highestUnlocked;
                levelSelectButtons[i].interactable = unlocked;
                if (i < levelSelectButtonLabels.Count && levelSelectButtonLabels[i] != null)
                {
                    levelSelectButtonLabels[i].text = BuildLevelSelectLabel(i);
                    ApplyTextHeight(levelSelectButtonLabels[i], 18);
                }
            }
        }

        private void RefreshPauseSettingsUi()
        {
            if (session == null)
            {
                return;
            }

            SetPauseSettingText(pauseSoundText, BuildSettingLabel("Sound", session.Progress.Settings.SoundEnabled));
            SetPauseSettingText(pauseHapticsText, BuildSettingLabel("Haptic", session.Progress.Settings.HapticsEnabled));
            SetPauseSettingText(pauseContrastText, BuildSettingLabel("Contrast", session.Progress.Settings.HighContrast));
        }

        private static void SetPauseSettingText(Text target, string value)
        {
            if (target == null)
            {
                return;
            }

            target.text = value;
            ApplyTextHeight(target, 18);
        }

        private string BuildLevelSelectLabel(int index)
        {
            ReleaseLevelDefinition level = levelPack.GetLevel(index);
            bool unlocked = session == null || index <= session.Progress.HighestUnlockedLevelIndex;
            bool current = session != null && session.HasStarted && index == session.CurrentLevelIndex;
            string state = current ? "Current" : unlocked ? "Open" : "Locked";
            return $"{index + 1:00}  {level.LevelId}\n{level.DisplayName}\n{level.DifficultyBand}  {state}";
        }

        private void RenderDreams(ReleaseGameplayViewModel viewModel)
        {
            RectTransform row = CreateRow(dreamRoot, "ActiveDreamRow", 238f);
            IReadOnlyList<ReleaseDreamSlotViewModel> dreams = viewModel.Dreams;
            for (int i = 0; i < dreams.Count; i++)
            {
                ReleaseDreamSlotViewModel card = dreams[i];
                Color color = card.IsSelected
                    ? ReleaseVisualStyle.Selected
                    : GetDreamColor(card.Slot);

                CreateDreamCard(
                    row,
                    card,
                    () => OnDreamSlotSelected(card.SlotId),
                    color);
            }
        }

        private void RenderOrders(ReleaseGameplayViewModel viewModel)
        {
            RectTransform row = CreateRow(orderRoot, "ActiveOrderRow", 178f);
            IReadOnlyList<ReleaseOrderSlotViewModel> orders = viewModel.Orders;
            for (int i = 0; i < orders.Count; i++)
            {
                ReleaseOrderSlotViewModel card = orders[i];
                Color color = card.IsSelected
                    ? ReleaseVisualStyle.Selected
                    : new Color(0.17f, 0.31f, 0.27f, 1f);

                CreateOrderCard(
                    row,
                    card,
                    () => OnOrderSlotSelected(card.SlotId),
                    color);
            }
        }

        private void RenderPreview(ReleaseGameplayViewModel viewModel)
        {
            bool hasWorkbenchState = !string.Equals(viewModel.FocusText, "Choose dream", StringComparison.Ordinal);
            previewRoot.gameObject.SetActive(hasWorkbenchState);
            if (!hasWorkbenchState)
            {
                return;
            }

            CreateText(previewRoot, "FocusText", viewModel.FocusText, 18, TextAnchor.MiddleLeft);
        }

        private void RenderStorage(ReleaseGameplayViewModel viewModel)
        {
            DynamicRoundState state = viewModel.State;
            bool shouldShowStorage = ShouldShowStorageStrip(viewModel);
            storageRoot.gameObject.SetActive(shouldShowStorage);
            if (!shouldShowStorage || state.StorageSlots.Count == 0)
            {
                return;
            }

            RectTransform row = CreateRow(storageRoot, "StorageRow", 58f);
            IReadOnlyList<ReleaseStorageSlotViewModel> storageSlots = viewModel.StorageSlots;
            for (int i = 0; i < storageSlots.Count; i++)
            {
                ReleaseStorageSlotViewModel card = storageSlots[i];
                Color color = card.IsSelected
                    ? ReleaseVisualStyle.Selected
                    : ReleaseVisualStyle.Storage;

                CreateStorageCard(
                    row,
                    card,
                    () => OnStorageSlotSelected(card.SlotId),
                    color);
            }
        }

        private void RenderModifiers(ReleaseGameplayViewModel viewModel)
        {
            IReadOnlyList<ReleaseModifierActionViewModel> modifiers = viewModel.Modifiers;
            modifierRoot.gameObject.SetActive(modifiers.Count > 0);
            if (modifiers.Count == 0)
            {
                return;
            }

            RectTransform row = CreateRow(modifierRoot, "ModifierRow", 54f);
            for (int i = 0; i < modifiers.Count; i++)
            {
                ReleaseModifierActionViewModel option = modifiers[i];
                DynamicModifierDefinition definition = option.Definition;
                Color color = definition.Type == DynamicModifierType.Item
                    ? ReleaseVisualStyle.Tool
                    : ReleaseVisualStyle.Obstacle;
                Button button = CreateButton(
                    row,
                    $"Modifier-{definition.Id}",
                    option.Label,
                    () => ApplyAction(DynamicPlayerAction.UseItem(definition.Id, option.TargetId)),
                    color,
                    artCatalog?.GetModifierIcon(definition),
                    artCatalog?.OperationButtonFrame);
                button.interactable = option.IsInteractable;
            }
        }

        private void RenderActions(ReleaseGameplayViewModel viewModel)
        {
            RectTransform operations = CreateRow(actionRoot, "OperationRow", 68f);
            IReadOnlyList<ReleaseOperationActionViewModel> operationOptions = viewModel.Operations;
            for (int i = 0; i < operationOptions.Count; i++)
            {
                ReleaseOperationActionViewModel option = operationOptions[i];
                Button button = CreateButton(
                    operations,
                    $"Operation-{option.Operation}",
                    option.Descriptor.Marker,
                    () => OnOperation(option.Operation),
                    option.Descriptor.Color,
                    artCatalog?.GetOperationIcon(option.Operation),
                    artCatalog?.OperationButtonFrame);
                button.interactable = option.IsInteractable;
                AddOperationPreviewChips(button.transform, option);
            }

            RectTransform submitRow = CreateRow(actionRoot, "SubmitStoreRow", 58f);
            Button submit = CreateButton(
                submitRow,
                "Submit",
                "Submit",
                OnSubmit,
                viewModel.CanSubmit ? ReleaseVisualStyle.Positive : ReleaseVisualStyle.Disabled,
                null,
                artCatalog?.SubmitButtonFrame);
            submit.interactable = viewModel.CanSubmit;

            IReadOnlyList<ReleaseStoreActionViewModel> storeActions = viewModel.StoreActions;
            for (int i = 0; i < storeActions.Count; i++)
            {
                if (!storeActions[i].IsInteractable)
                {
                    continue;
                }

                int storageSlotId = storeActions[i].StorageSlotId;
                Button store = CreateButton(
                    submitRow,
                    $"Store-{storageSlotId}",
                    $"Store S{storageSlotId + 1}",
                    () => OnStore(storageSlotId),
                    new Color(0.22f, 0.25f, 0.32f, 1f),
                    null,
                    artCatalog?.StorageActionFrame);
                store.interactable = storeActions[i].IsInteractable;
            }

            if (!viewModel.ShouldRenderRecallRow)
            {
                return;
            }

            RectTransform recallRow = CreateRow(actionRoot, "RecallRow", 52f);
            IReadOnlyList<ReleaseRecallActionViewModel> recallActions = viewModel.RecallActions;
            for (int i = 0; i < recallActions.Count; i++)
            {
                int activeDreamSlotId = recallActions[i].ActiveDreamSlotId;
                Button recall = CreateButton(
                    recallRow,
                    $"Recall-{activeDreamSlotId}",
                    $"Recall D{activeDreamSlotId + 1}",
                    () => OnRecall(activeDreamSlotId),
                    new Color(0.27f, 0.24f, 0.34f, 1f),
                    null,
                    artCatalog?.StorageActionFrame);
                recall.interactable = recallActions[i].IsInteractable;
            }
        }

        private void AddOperationPreviewChips(
            Transform parent,
            ReleaseOperationActionViewModel operation)
        {
            if (!operation.HasPreview || !operation.IsInteractable)
            {
                return;
            }

            AddDreamStateChips(parent, operation.PreviewAttributes, new Vector2(0.46f, 0.12f), new Vector2(0.94f, 0.48f));
            AddCardHalo(parent, ReleaseVisualStyle.Selected, 1.8f);
        }

        private static bool ShouldShowStorageStrip(ReleaseGameplayViewModel viewModel)
        {
            if (viewModel == null || viewModel.State.StorageSlots.Count == 0)
            {
                return false;
            }

            IReadOnlyList<ReleaseStorageSlotViewModel> storageSlots = viewModel.StorageSlots;
            for (int i = 0; i < storageSlots.Count; i++)
            {
                ReleaseStorageSlotViewModel storageSlot = storageSlots[i];
                if (!storageSlot.Slot.IsEmpty
                    || storageSlot.IsSelected
                    || storageSlot.CanStoreSelectedDream)
                {
                    return true;
                }
            }

            return false;
        }

        private Button CreateDreamCard(
            Transform parent,
            ReleaseDreamSlotViewModel card,
            UnityEngine.Events.UnityAction callback,
            Color color)
        {
            DynamicDreamSlot slot = card.Slot;
            Button button = CreateVisualCardButton(
                parent,
                $"ActiveDream-{slot.SlotId}",
                callback,
                color,
                artCatalog?.DreamCardFrame);
            ConfigureDropTarget(button.gameObject, ReleaseDropTargetKind.ActiveDream, slot.SlotId);
            if (!slot.IsEmpty)
            {
                ConfigureDragSource(button.gameObject, ReleaseDragSourceKind.ActiveDream, slot.SlotId);
            }

            Transform root = button.transform;
            AddCardSurfaceTreatment(root, color, true);
            CreateOverlayText(root, "SlotLabel", $"D{slot.SlotId + 1}", 15, TextAnchor.UpperLeft, new Vector2(0f, 0.72f), Vector2.one, new Vector2(12f, -4f), new Vector2(-8f, -6f));
            if (slot.IsEmpty)
            {
                CreateOverlayText(root, "EmptyLabel", "Open", 18, TextAnchor.MiddleCenter, new Vector2(0.12f, 0.18f), new Vector2(0.88f, 0.72f), Vector2.zero, Vector2.zero);
                if (card.CanRecallSelectedStorage)
                {
                    AddCardHalo(root, ReleaseVisualStyle.Positive, 2.2f);
                }

                return button;
            }

            bool locked = card.IsLocked;
            string status = locked ? "Lock" : string.Empty;
            if (!string.IsNullOrEmpty(status))
            {
                CreateOverlayText(root, "StatusLabel", status, 16, TextAnchor.UpperRight, new Vector2(0.35f, 0.72f), Vector2.one, new Vector2(4f, -6f), new Vector2(-12f, -8f));
            }

            AddDreamCoreGlow(root, ReleaseVisualDescriptors.PrimaryColor(slot.Dream.Attributes));
            CreateOverlayIcon(root, "HeroIcon", GetDreamIcon(slot), new Vector2(0.32f, 0.34f), new Vector2(0.68f, 0.78f), Vector2.zero, Vector2.zero);
            AddDreamStateChips(root, slot.Dream.Attributes, new Vector2(0.08f, 0.08f), new Vector2(0.92f, 0.34f));
            bool selected = card.IsSelected;
            bool readyForSelectedOrder = card.CanSubmitToSelectedOrder;
            if (selected || readyForSelectedOrder)
            {
                AddCardHalo(root, selected ? ReleaseVisualStyle.Selected : ReleaseVisualStyle.Positive, selected ? 3.2f : 2.2f);
            }

            return button;
        }

        private Button CreateOrderCard(
            Transform parent,
            ReleaseOrderSlotViewModel card,
            UnityEngine.Events.UnityAction callback,
            Color color)
        {
            DynamicOrderSlot slot = card.Slot;
            Button button = CreateVisualCardButton(
                parent,
                $"ActiveOrder-{slot.SlotId}",
                callback,
                color,
                artCatalog?.OrderSheetFrame);
            ConfigureDropTarget(button.gameObject, ReleaseDropTargetKind.ActiveOrder, slot.SlotId);

            Transform root = button.transform;
            AddCardSurfaceTreatment(root, color, false);
            CreateOverlayText(root, "SlotLabel", $"O{slot.SlotId + 1}", 15, TextAnchor.UpperLeft, new Vector2(0f, 0.64f), Vector2.one, new Vector2(12f, -4f), new Vector2(-8f, -6f));
            if (slot.IsEmpty)
            {
                CreateOverlayText(root, "DoneLabel", "Done", 20, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(8f, 8f), new Vector2(-8f, -8f));
                return button;
            }

            string countLabel = $"{slot.Order.FulfilledCount}/{slot.Order.Requirement.Count}";
            CreateOverlayText(root, "CountLabel", countLabel, 20, TextAnchor.UpperRight, new Vector2(0.45f, 0.62f), Vector2.one, new Vector2(4f, -4f), new Vector2(-12f, -6f));
            CreateOverlayIcon(root, "HeroIcon", GetOrderIcon(slot), new Vector2(0.08f, 0.2f), new Vector2(0.34f, 0.72f), Vector2.zero, Vector2.zero);
            AddRequirementChips(root, slot.Order.Requirement, new Vector2(0.36f, 0.18f), new Vector2(0.92f, 0.58f));
            AddOrderProgressMeter(root, slot.Order.FulfilledCount, slot.Order.Requirement.Count, color);
            bool selected = card.IsSelected;
            bool readyForSelectedDream = card.CanAcceptSelectedDream;
            if (selected || readyForSelectedDream)
            {
                AddCardHalo(root, selected ? ReleaseVisualStyle.Selected : ReleaseVisualStyle.Positive, selected ? 3.2f : 2.2f);
            }

            return button;
        }

        private Button CreateStorageCard(
            Transform parent,
            ReleaseStorageSlotViewModel card,
            UnityEngine.Events.UnityAction callback,
            Color color)
        {
            DynamicStorageSlot slot = card.Slot;
            Button button = CreateVisualCardButton(
                parent,
                $"Storage-{slot.SlotId}",
                callback,
                color,
                artCatalog?.StorageShelfFrame);
            ConfigureDropTarget(button.gameObject, ReleaseDropTargetKind.Storage, slot.SlotId);
            if (!slot.IsEmpty)
            {
                ConfigureDragSource(button.gameObject, ReleaseDragSourceKind.Storage, slot.SlotId);
            }

            Transform root = button.transform;
            AddCardSurfaceTreatment(root, color, false);
            AddStorageShelfCue(root, color);
            CreateOverlayText(root, "SlotLabel", $"S{slot.SlotId + 1}", 14, TextAnchor.UpperLeft, new Vector2(0f, 0.6f), Vector2.one, new Vector2(12f, -4f), new Vector2(-8f, -6f));
            if (slot.IsEmpty)
            {
                if (card.CanStoreSelectedDream)
                {
                    AddCardHalo(root, ReleaseVisualStyle.Positive, 2.2f);
                }

                return button;
            }

            CreateOverlayIcon(root, "HeroIcon", GetStorageIcon(slot), new Vector2(0.08f, 0.16f), new Vector2(0.34f, 0.74f), Vector2.zero, Vector2.zero);
            AddDreamStateChips(root, slot.Dream.Attributes, new Vector2(0.36f, 0.18f), new Vector2(0.92f, 0.58f));
            if (card.IsSelected)
            {
                AddCardHalo(root, ReleaseVisualStyle.Selected, 3.2f);
            }

            return button;
        }

        private Button CreateVisualCardButton(
            Transform parent,
            string objectName,
            UnityEngine.Events.UnityAction callback,
            Color color,
            Sprite surfaceSprite)
        {
            GameObject go = CreateUiObject(UiElementKind.Button, parent, objectName, typeof(RectTransform), typeof(Image), typeof(Button));
            ResetLayoutParticipation(go);

            Image image = go.GetComponent<Image>();
            // Surface sprites carry their own painted material; keep them untinted so
            // card art remains readable while overlays and chrome provide gameplay color.
            Color visualTint = surfaceSprite == null ? color : Color.white;
            image.color = visualTint;
            image.sprite = surfaceSprite;
            image.type = Image.Type.Simple;
            image.preserveAspect = false;
            ResetDragComponents(go);

            Button button = go.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(callback);
            ApplyButtonPalette(button, visualTint);
            ApplyButtonChrome(go, color, surfaceSprite != null);

            LayoutElement layout = GetOrAddComponent<LayoutElement>(go);
            layout.ignoreLayout = false;
            layout.minHeight = ReleaseVisualStyle.MinTouchTargetHeight;
            layout.preferredHeight = 104f;
            layout.flexibleWidth = 1f;
            return button;
        }

        private void ConfigureDragSource(
            GameObject target,
            ReleaseDragSourceKind sourceKind,
            int slotId)
        {
            ReleaseDragHandle dragHandle = GetOrAddComponent<ReleaseDragHandle>(target);
            dragHandle.enabled = true;
            dragHandle.Configure(sourceKind, slotId, OnCardDragStarted, OnCardDragEnded);
        }

        private static void ConfigureDropTarget(
            GameObject target,
            ReleaseDropTargetKind targetKind,
            int slotId)
        {
            ReleaseDragDropTarget dropTarget = GetOrAddComponent<ReleaseDragDropTarget>(target);
            dropTarget.enabled = true;
            dropTarget.Configure(targetKind, slotId);
        }

        private void OnCardDragStarted(ReleaseDragHandle dragHandle)
        {
            if (messageText == null)
            {
                return;
            }

            messageText.color = ReleaseVisualStyle.Text;
        }

        private void OnCardDragEnded(ReleaseDragHandle dragHandle, PointerEventData eventData)
        {
            ReleaseDragDropTarget dropTarget = FindDropTarget(eventData);
            if (dropTarget == null)
            {
                ShowInvalidDrag("Drop the card on a target.");
                return;
            }

            TryApplyDragAction(dragHandle.Payload, dropTarget.Descriptor);
        }

        private ReleaseDragDropTarget FindDropTarget(PointerEventData eventData)
        {
            if (EventSystem.current == null || eventData == null)
            {
                return null;
            }

            dragRaycastResults.Clear();
            EventSystem.current.RaycastAll(eventData, dragRaycastResults);
            for (int i = 0; i < dragRaycastResults.Count; i++)
            {
                GameObject hit = dragRaycastResults[i].gameObject;
                if (hit == null)
                {
                    continue;
                }

                ReleaseDragDropTarget dropTarget = hit.GetComponentInParent<ReleaseDragDropTarget>();
                if (dropTarget != null && dropTarget.isActiveAndEnabled)
                {
                    return dropTarget;
                }
            }

            return null;
        }

        private bool TryApplyDragAction(
            ReleaseDragPayload payload,
            ReleaseDropTargetDescriptor target)
        {
            if (!session.HasStarted)
            {
                ShowInvalidDrag("No level is running.");
                return false;
            }

            ReleaseDragActionResolution resolution = ReleaseDragActionResolver.Resolve(
                session.CurrentState,
                payload,
                target);
            if (!resolution.Success)
            {
                ShowInvalidDrag(resolution.Message);
                return false;
            }

            DynamicActionResult result = ApplyAction(resolution.Action);
            return result.Success;
        }

        private void ShowInvalidDrag(string message)
        {
            RefreshUi(message);
            PresentInvalidFeedback();
        }

        private void AddCardSurfaceTreatment(Transform parent, Color accent, bool includeDreamWell)
        {
            Color inset = new Color(0f, 0f, 0f, 0.2f);
            CreateOverlayPanel(parent, "CardInset", inset, new Vector2(0.03f, 0.06f), new Vector2(0.97f, 0.94f), Vector2.zero, Vector2.zero);
            CreateOverlayPanel(parent, "CardTopBand", WithAlpha(Color.Lerp(accent, Color.black, 0.18f), 0.34f), new Vector2(0f, 0.72f), Vector2.one, Vector2.zero, Vector2.zero);
            CreateOverlayPanel(parent, "CardAccentRail", WithAlpha(Color.Lerp(accent, Color.white, 0.16f), 0.42f), Vector2.zero, new Vector2(0.035f, 1f), Vector2.zero, Vector2.zero);

            if (includeDreamWell)
            {
                CreateOverlayPanel(parent, "DreamWell", new Color(0.04f, 0.06f, 0.085f, 0.42f), new Vector2(0.26f, 0.3f), new Vector2(0.74f, 0.82f), Vector2.zero, Vector2.zero);
            }
        }

        private void AddDreamCoreGlow(Transform parent, Color color)
        {
            CreateOverlayPanel(parent, "DreamCoreGlow", WithAlpha(Color.Lerp(color, Color.white, 0.18f), 0.18f), new Vector2(0.29f, 0.34f), new Vector2(0.71f, 0.78f), Vector2.zero, Vector2.zero);
        }

        private void AddOrderProgressMeter(Transform parent, int fulfilledCount, int requiredCount, Color color)
        {
            CreateOverlayPanel(parent, "OrderProgressTrack", new Color(0f, 0f, 0f, 0.28f), new Vector2(0.36f, 0.08f), new Vector2(0.92f, 0.14f), Vector2.zero, Vector2.zero);

            float ratio = requiredCount <= 0
                ? 1f
                : Mathf.Clamp01((float)fulfilledCount / requiredCount);
            if (ratio <= 0.001f)
            {
                return;
            }

            CreateOverlayPanel(
                parent,
                "OrderProgressFill",
                WithAlpha(Color.Lerp(color, Color.white, 0.2f), 0.76f),
                new Vector2(0.36f, 0.08f),
                new Vector2(0.36f + (0.56f * ratio), 0.14f),
                Vector2.zero,
                Vector2.zero);
        }

        private void AddStorageShelfCue(Transform parent, Color color)
        {
            CreateOverlayPanel(parent, "StorageShelfCue", WithAlpha(Color.Lerp(color, Color.white, 0.18f), 0.38f), new Vector2(0.08f, 0.12f), new Vector2(0.92f, 0.19f), Vector2.zero, Vector2.zero);
        }

        private void AddDreamStateChips(Transform parent, DynamicDreamAttributes attributes, Vector2 anchorMin, Vector2 anchorMax)
        {
            RectTransform strip = CreateChipStrip(parent, "StateChips", anchorMin, anchorMax);
            AddStateChip(strip, "Taint", artCatalog?.GetTaintIcon(attributes.Taint), ReleaseVisualDescriptors.ForTaint(attributes.Taint).Color);
            AddStateChip(strip, "Mood", artCatalog?.GetMoodIcon(attributes.Mood), ReleaseVisualDescriptors.ForMood(attributes.Mood).Color);
            AddStateChip(strip, "Clarity", artCatalog?.GetClarityIcon(attributes.Clarity), ReleaseVisualDescriptors.ForClarity(attributes.Clarity).Color);
            AddStateChip(strip, "Stability", artCatalog?.GetStabilityIcon(attributes.Stability), ReleaseVisualDescriptors.ForStability(attributes.Stability).Color);
        }

        private void AddRequirementChips(Transform parent, DynamicOrderRequirement requirement, Vector2 anchorMin, Vector2 anchorMax)
        {
            RectTransform strip = CreateChipStrip(parent, "RequirementChips", anchorMin, anchorMax);
            int count = 0;
            if (requirement.HasTaint)
            {
                ReleaseStateVisualDescriptor descriptor = ReleaseVisualDescriptors.ForTaint(requirement.RequiredTaint);
                AddStateChip(strip, "ReqTaint", artCatalog?.GetTaintIcon(requirement.RequiredTaint), descriptor.Color);
                count++;
            }

            if (requirement.HasMood)
            {
                ReleaseStateVisualDescriptor descriptor = ReleaseVisualDescriptors.ForMood(requirement.RequiredMood);
                AddStateChip(strip, "ReqMood", artCatalog?.GetMoodIcon(requirement.RequiredMood), descriptor.Color);
                count++;
            }

            if (requirement.HasClarity)
            {
                ReleaseStateVisualDescriptor descriptor = ReleaseVisualDescriptors.ForClarity(requirement.RequiredClarity);
                AddStateChip(strip, "ReqClarity", artCatalog?.GetClarityIcon(requirement.RequiredClarity), descriptor.Color);
                count++;
            }

            if (requirement.HasStability)
            {
                ReleaseStateVisualDescriptor descriptor = ReleaseVisualDescriptors.ForStability(requirement.RequiredStability);
                AddStateChip(strip, "ReqStability", artCatalog?.GetStabilityIcon(requirement.RequiredStability), descriptor.Color);
                count++;
            }

            if (count == 0)
            {
                AddStateChip(strip, "ReqStableAny", artCatalog?.GetStabilityIcon(DreamStability.Stable), ReleaseVisualStyle.StableDream);
            }
        }

        private RectTransform CreateChipStrip(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            RectTransform strip = CreatePanel(parent, name, new Color(0f, 0f, 0f, 0f));
            SetOverlayRect(strip, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
            LayoutElement layout = GetOrAddComponent<LayoutElement>(strip.gameObject);
            layout.ignoreLayout = true;
            AddHorizontalLayout(strip, new RectOffset(0, 0, 0, 0), 5f);
            return strip;
        }

        private void AddStateChip(Transform parent, string name, Sprite sprite, Color color)
        {
            RectTransform chip = CreatePanel(parent, name, Color.Lerp(color, Color.black, 0.12f));
            LayoutElement chipLayout = GetOrAddComponent<LayoutElement>(chip.gameObject);
            chipLayout.minWidth = 32f;
            chipLayout.preferredWidth = 42f;
            chipLayout.preferredHeight = 42f;
            chipLayout.flexibleWidth = 1f;
            CreateOverlayIcon(chip, "Icon", sprite, new Vector2(0.18f, 0.18f), new Vector2(0.82f, 0.82f), Vector2.zero, Vector2.zero);
        }

        private void AddCardHalo(Transform parent, Color color, float thickness)
        {
            RectTransform halo = CreatePanel(parent, "SelectionHalo", new Color(color.r, color.g, color.b, 0.035f));
            SetOverlayRect(halo, Vector2.zero, Vector2.one, new Vector2(4f, 4f), new Vector2(-4f, -4f));
            LayoutElement layout = GetOrAddComponent<LayoutElement>(halo.gameObject);
            layout.ignoreLayout = true;

            Outline outline = GetOrAddExactComponent<Outline>(halo.gameObject);
            outline.enabled = true;
            outline.effectColor = new Color(color.r, color.g, color.b, 0.82f);
            outline.effectDistance = new Vector2(thickness, -thickness);
            Image image = halo.GetComponent<Image>();
            if (image != null)
            {
                image.raycastTarget = false;
            }
        }

        private void OnDreamSlotSelected(int slotId)
        {
            DynamicDreamSlot slot = session.CurrentState.FindActiveDreamSlot(slotId);
            if (slot == null || slot.IsEmpty)
            {
                selection.ClearDream();
                RefreshUi("That dream slot is empty.");
                return;
            }

            selection.SelectDream(slotId);
            RefreshUi(selectedDreamSlotId < 0 ? "Dream cleared" : $"D{slotId + 1} selected");
        }

        private void OnOrderSlotSelected(int slotId)
        {
            DynamicOrderSlot slot = session.CurrentState.FindActiveOrderSlot(slotId);
            if (slot == null || slot.IsEmpty)
            {
                selection.ClearOrder();
                RefreshUi("That order slot is empty.");
                return;
            }

            selection.SelectOrder(slotId);
            RefreshUi(selectedOrderSlotId < 0 ? "Order cleared" : $"O{slotId + 1} selected");
        }

        private void OnStorageSlotSelected(int slotId)
        {
            DynamicStorageSlot slot = session.CurrentState.FindStorageSlot(slotId);
            if (slot == null || slot.IsEmpty)
            {
                selection.ClearStorage();
                RefreshUi("That storage slot is empty.");
                return;
            }

            selection.SelectStorage(slotId);
            RefreshUi(selectedStorageSlotId < 0 ? "Basket cleared" : $"S{slotId + 1} selected");
        }

        private void OnOperation(DynamicOperation operation)
        {
            if (selectedDreamSlotId < 0)
            {
                RefreshUi("Pick a dream first.");
                return;
            }

            ApplyAction(DynamicPlayerAction.ApplyOperation(selectedDreamSlotId, operation));
        }

        private void OnSubmit()
        {
            if (selectedDreamSlotId < 0 || selectedOrderSlotId < 0)
            {
                RefreshUi("Pick a dream and an order.");
                return;
            }

            ApplyAction(DynamicPlayerAction.SubmitDream(selectedDreamSlotId, selectedOrderSlotId));
        }

        private void OnStore(int storageSlotId)
        {
            if (selectedDreamSlotId < 0)
            {
                RefreshUi("Pick a dream first.");
                return;
            }

            ApplyAction(DynamicPlayerAction.StoreDream(selectedDreamSlotId, storageSlotId));
        }

        private void OnRecall(int activeDreamSlotId)
        {
            if (selectedStorageSlotId < 0)
            {
                RefreshUi("Pick a stored dream first.");
                return;
            }

            ApplyAction(DynamicPlayerAction.RecallDream(selectedStorageSlotId, activeDreamSlotId));
        }

        private void ToggleSound()
        {
            ReleaseSettingsState settings = session.Progress.Settings.Clone();
            settings.SoundEnabled = !settings.SoundEnabled;
            session.UpdateSettings(settings);
            RefreshUi("Sound setting updated.");
            RefreshPauseSettingsUi();
        }

        private void ToggleHaptics()
        {
            ReleaseSettingsState settings = session.Progress.Settings.Clone();
            settings.HapticsEnabled = !settings.HapticsEnabled;
            session.UpdateSettings(settings);
            RefreshUi("Haptic setting updated.");
            RefreshPauseSettingsUi();
        }

        private void ToggleContrast()
        {
            ReleaseSettingsState settings = session.Progress.Settings.Clone();
            settings.HighContrast = !settings.HighContrast;
            session.UpdateSettings(settings);
            RefreshUi("Contrast setting updated.");
            RefreshPauseSettingsUi();
        }

        private void OnRestart()
        {
            session.RestartLevel();
            ClearSelection();
            RefreshUi("Restart");
            ShowGameplayScreen();
        }

        private void OnNext()
        {
            if (!session.TryStartNextLevel())
            {
                RefreshUi(session.LastMessage);
                return;
            }

            ClearSelection();
            RefreshUi(session.CurrentLevel.Guidance);
            ShowGameplayScreen();
        }

        private void OnPauseRestart()
        {
            OnRestart();
        }

        private void OnResultNext()
        {
            OnNext();
        }

        private void OnResultReplay()
        {
            session.RestartLevel();
            ClearSelection();
            RefreshUi("Replay started.");
            ShowGameplayScreen();
        }

        private void ClearSelection()
        {
            selection.ClearAll();
        }

        private string BuildGuidedPrompt()
        {
            ReleaseGuidedActionRule rule = session.PendingGuidedAction;
            if (rule == null)
            {
                return string.Empty;
            }

            return $"\n{(string.IsNullOrWhiteSpace(rule.Prompt) ? rule.Describe() : rule.Prompt)}";
        }

        private string BuildCompactGameplayGuidance(DynamicRoundState state)
        {
            return $"{state.CompletedOrders}/{state.TargetCompletedOrders} orders   {state.RemainingMoves}M{BuildGuidedPrompt()}";
        }

        private string BuildVisibleGameplayMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message)
                || string.Equals(message, session.CurrentLevel.Guidance, StringComparison.Ordinal)
                || string.Equals(message, session.CurrentLevel.PlayerQuestion, StringComparison.Ordinal))
            {
                return string.Empty;
            }

            return message;
        }

        private void SetGameplayMessage(string message)
        {
            bool hasMessage = !string.IsNullOrWhiteSpace(message);
            messageText.gameObject.SetActive(hasMessage);
            if (hasMessage)
            {
                SetTextAndHeight(messageText, message, 18);
            }

            if (gameplayHeader != null)
            {
                SetPreferredHeight(gameplayHeader.gameObject, hasMessage ? 132f : 106f);
            }
        }

        private void PresentGameplayFeedback(DynamicActionResult result)
        {
            feedbackPresenter?.PresentActionResult(result, session?.Progress?.Settings);
        }

        private void PresentInvalidFeedback()
        {
            feedbackPresenter?.PresentInvalidTarget(session?.Progress?.Settings);
        }

        private static string BuildSettingLabel(string name, bool enabled)
        {
            return $"{name}\n{(enabled ? "On" : "Off")}";
        }

        private static Color GetDreamColor(DynamicDreamSlot slot)
        {
            if (slot.IsEmpty)
            {
                return new Color(0.16f, 0.17f, 0.2f, 1f);
            }

            return ReleaseVisualDescriptors.PrimaryColor(slot.Dream.Attributes);
        }

        private Sprite GetDreamIcon(DynamicDreamSlot slot)
        {
            if (slot.IsEmpty || artCatalog == null)
            {
                return null;
            }

            return artCatalog.GetPrimaryStateIcon(slot.Dream.Attributes);
        }

        private Sprite GetOrderIcon(DynamicOrderSlot slot)
        {
            if (slot.IsEmpty || artCatalog == null)
            {
                return null;
            }

            return artCatalog.GetRequirementIcon(slot.Order.Requirement);
        }

        private Sprite GetStorageIcon(DynamicStorageSlot slot)
        {
            if (slot.IsEmpty || artCatalog == null)
            {
                return null;
            }

            return artCatalog.GetPrimaryStateIcon(slot.Dream.Attributes);
        }

        private RectTransform CreateSection(Transform parent, string name, float preferredHeight)
        {
            RectTransform section = CreatePanel(parent, name, ReleaseVisualStyle.Panel);
            SetPreferredHeight(section.gameObject, preferredHeight);
            AddVerticalLayout(section, new RectOffset(10, 10, 10, 10), 8f);
            return section;
        }

        private RectTransform CreateRow(Transform parent, string name, float preferredHeight)
        {
            RectTransform row = CreatePanel(parent, name, ReleaseVisualStyle.MutedPanel);
            SetPreferredHeight(row.gameObject, preferredHeight);
            AddHorizontalLayout(row, new RectOffset(6, 6, 6, 6), 6f);
            return row;
        }

        private RectTransform CreateScrollContent(Transform parent, string name)
        {
            RectTransform scrollRoot = CreatePanel(parent, name, new Color(0.04f, 0.048f, 0.064f, 0.72f));
            SetFlexibleHeight(scrollRoot.gameObject, 1f);

            ScrollRect scrollRect = GetOrAddComponent<ScrollRect>(scrollRoot.gameObject);
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 42f;

            GameObject viewportObject = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewportObject.transform.SetParent(scrollRoot, false);
            RectTransform viewport = viewportObject.GetComponent<RectTransform>();
            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.offsetMin = new Vector2(10f, 10f);
            viewport.offsetMax = new Vector2(-10f, -10f);

            Image viewportImage = viewportObject.GetComponent<Image>();
            viewportImage.color = new Color(0f, 0f, 0f, 0.02f);
            viewportImage.raycastTarget = false;
            viewportObject.GetComponent<Mask>().showMaskGraphic = false;

            GameObject contentObject = new GameObject("Content", typeof(RectTransform));
            contentObject.transform.SetParent(viewportObject.transform, false);
            RectTransform content = contentObject.GetComponent<RectTransform>();
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.offsetMin = Vector2.zero;
            content.offsetMax = Vector2.zero;

            VerticalLayoutGroup layout = GetOrAddComponent<VerticalLayoutGroup>(contentObject);
            layout.padding = new RectOffset(6, 6, 6, 6);
            layout.spacing = 8f;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;

            ContentSizeFitter fitter = GetOrAddComponent<ContentSizeFitter>(contentObject);
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = viewport;
            scrollRect.content = content;
            return content;
        }

        private void CreateSectionTitle(Transform parent, string text)
        {
            CreateText(parent, "SectionTitle", text, 22, TextAnchor.MiddleLeft);
        }

        private RectTransform CreatePanel(Transform parent, string name, Color color)
        {
            GameObject go = CreateUiObject(UiElementKind.Panel, parent, name, typeof(RectTransform), typeof(Image));
            ResetLayoutParticipation(go);
            Image image = go.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            if (name != "SafeArea" && color.a > 0.05f)
            {
                ApplyPanelChrome(go);
            }
            else
            {
                DisablePanelChrome(go);
            }

            DisableLayoutGroups(go);
            return go.GetComponent<RectTransform>();
        }

        private Text CreateText(Transform parent, string name, string text, int fontSize, TextAnchor alignment)
        {
            GameObject go = CreateUiObject(UiElementKind.Text, parent, name, typeof(RectTransform), typeof(Text));
            ResetLayoutParticipation(go);

            Text label = go.GetComponent<Text>();
            label.font = font;
            label.text = text;
            label.fontSize = fontSize;
            label.alignment = alignment;
            label.color = ReleaseVisualStyle.Text;
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 13;
            label.resizeTextMaxSize = fontSize;
            label.raycastTarget = false;
            ApplyTextShadow(go);

            LayoutElement layout = GetOrAddComponent<LayoutElement>(go);
            layout.ignoreLayout = false;
            layout.minHeight = Mathf.Max(34f, fontSize + 10f);
            layout.preferredHeight = Mathf.Max(42f, fontSize + 18f);
            layout.flexibleWidth = 1f;
            ApplyTextHeight(label, fontSize);
            return label;
        }

        private Text CreateOverlayText(
            Transform parent,
            string name,
            string text,
            int fontSize,
            TextAnchor alignment,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin,
            Vector2 offsetMax)
        {
            Text label = CreateText(parent, name, text, fontSize, alignment);
            RectTransform rect = label.GetComponent<RectTransform>();
            SetOverlayRect(rect, anchorMin, anchorMax, offsetMin, offsetMax);
            LayoutElement layout = GetOrAddComponent<LayoutElement>(label.gameObject);
            layout.ignoreLayout = true;
            return label;
        }

        private RectTransform CreateOverlayPanel(
            Transform parent,
            string name,
            Color color,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin,
            Vector2 offsetMax)
        {
            RectTransform panel = CreatePanel(parent, name, color);
            SetOverlayRect(panel, anchorMin, anchorMax, offsetMin, offsetMax);

            LayoutElement layout = GetOrAddComponent<LayoutElement>(panel.gameObject);
            layout.ignoreLayout = true;
            DisablePanelChrome(panel.gameObject);

            Image image = panel.GetComponent<Image>();
            if (image != null)
            {
                image.raycastTarget = false;
            }

            return panel;
        }

        private Image CreateIcon(Transform parent, string name, Sprite sprite, float width, float height)
        {
            GameObject go = CreateUiObject(UiElementKind.Icon, parent, name, typeof(RectTransform), typeof(Image));
            Image image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.color = Color.white;
            image.preserveAspect = true;
            image.raycastTarget = false;

            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(36f, 0f);
            rect.sizeDelta = new Vector2(width, height);

            LayoutElement layout = GetOrAddComponent<LayoutElement>(go);
            layout.ignoreLayout = true;
            return image;
        }

        private Image CreateOverlayIcon(
            Transform parent,
            string name,
            Sprite sprite,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin,
            Vector2 offsetMax)
        {
            if (sprite == null)
            {
                return null;
            }

            GameObject go = CreateUiObject(UiElementKind.Icon, parent, name, typeof(RectTransform), typeof(Image));
            Image image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.color = Color.white;
            image.preserveAspect = true;
            image.raycastTarget = false;

            RectTransform rect = go.GetComponent<RectTransform>();
            SetOverlayRect(rect, anchorMin, anchorMax, offsetMin, offsetMax);
            LayoutElement layout = GetOrAddComponent<LayoutElement>(go);
            layout.ignoreLayout = true;
            return image;
        }

        private Image CreateLayoutIcon(Transform parent, string name, Sprite sprite, float width, float height)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.color = Color.white;
            image.preserveAspect = true;
            image.raycastTarget = false;

            LayoutElement layout = GetOrAddComponent<LayoutElement>(go);
            layout.minHeight = height;
            layout.preferredHeight = height;
            layout.preferredWidth = width;
            layout.flexibleWidth = 0f;
            return image;
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }

        private static void SetOverlayRect(
            RectTransform rect,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin,
            Vector2 offsetMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private static void ApplyPanelChrome(GameObject go)
        {
            Outline outline = GetOrAddExactComponent<Outline>(go);
            outline.enabled = true;
            outline.effectColor = ReleaseVisualStyle.PanelOutline;
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            Shadow shadow = GetOrAddExactComponent<Shadow>(go);
            shadow.enabled = true;
            shadow.effectColor = new Color(0f, 0f, 0f, 0.18f);
            shadow.effectDistance = new Vector2(0f, -3f);
        }

        private static void DisablePanelChrome(GameObject go)
        {
            DisableExactEffect<Outline>(go);
            DisableExactEffect<Shadow>(go);
        }

        private static void ApplyTextShadow(GameObject go)
        {
            Shadow shadow = GetOrAddExactComponent<Shadow>(go);
            shadow.enabled = true;
            shadow.effectColor = ReleaseVisualStyle.TextShadow;
            shadow.effectDistance = new Vector2(1.25f, -1.25f);
        }

        private static void ApplyButtonChrome(GameObject go, Color normalColor, bool hasSurfaceSprite)
        {
            Outline outline = GetOrAddExactComponent<Outline>(go);
            outline.enabled = true;
            outline.effectColor = hasSurfaceSprite
                ? Color.Lerp(normalColor, Color.white, 0.38f)
                : ReleaseVisualStyle.ButtonOutline;
            outline.effectDistance = hasSurfaceSprite ? new Vector2(2f, -2f) : new Vector2(1f, -1f);

            Shadow shadow = GetOrAddExactComponent<Shadow>(go);
            shadow.enabled = true;
            shadow.effectColor = new Color(0f, 0f, 0f, hasSurfaceSprite ? 0.24f : 0.16f);
            shadow.effectDistance = hasSurfaceSprite ? new Vector2(0f, -4f) : new Vector2(0f, -2f);
        }

        private static void ApplyPanelSprite(RectTransform target, Sprite sprite, Color tint)
        {
            if (target == null || sprite == null)
            {
                return;
            }

            Image image = target.GetComponent<Image>();
            if (image == null)
            {
                return;
            }

            image.sprite = sprite;
            image.color = tint;
            image.type = Image.Type.Simple;
            image.preserveAspect = false;
        }

        private static void SetTextAndHeight(Text label, string text, int fontSize)
        {
            label.text = text;
            ApplyTextHeight(label, fontSize);
        }

        private static void ApplyTextHeight(Text label, int fontSize)
        {
            int lineCount = 1;
            string value = label.text ?? string.Empty;
            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == '\n')
                {
                    lineCount++;
                }
            }

            LayoutElement layout = GetOrAddComponent<LayoutElement>(label.gameObject);
            layout.minHeight = Mathf.Max(34f, fontSize + 10f);
            layout.preferredHeight = Mathf.Max(42f, (fontSize + 14f) * lineCount);
            layout.flexibleWidth = 1f;
        }

        private Button CreateButton(Transform parent, string label, UnityEngine.Events.UnityAction callback, Color color)
        {
            return CreateButton(parent, label, label, callback, color, null, null);
        }

        private Button CreateCompactButton(
            Transform parent,
            string objectName,
            string label,
            UnityEngine.Events.UnityAction callback,
            Color color,
            float preferredWidth)
        {
            GameObject go = CreateUiObject(UiElementKind.Button, parent, objectName, typeof(RectTransform), typeof(Image), typeof(Button));
            ResetLayoutParticipation(go);

            Image image = go.GetComponent<Image>();
            image.color = color;
            image.type = Image.Type.Simple;
            image.preserveAspect = false;
            ResetDragComponents(go);

            Button button = go.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(callback);
            ApplyButtonPalette(button, color);
            ApplyButtonChrome(go, color, false);

            Text text = CreateText(go.transform, "Text", label, 14, TextAnchor.MiddleCenter);
            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(5f, 3f);
            textRect.offsetMax = new Vector2(-5f, -3f);

            LayoutElement layout = GetOrAddComponent<LayoutElement>(go);
            layout.ignoreLayout = false;
            layout.minHeight = 42f;
            layout.preferredHeight = 44f;
            layout.minWidth = Mathf.Max(48f, preferredWidth);
            layout.preferredWidth = Mathf.Max(48f, preferredWidth);
            layout.flexibleWidth = 0f;
            return button;
        }

        private Button CreateHeaderActionButton(
            RectTransform parent,
            string objectName,
            string label,
            UnityEngine.Events.UnityAction callback,
            Color color,
            float rightOffset,
            float width)
        {
            Button button = CreateCompactButton(parent, objectName, label, callback, color, width);
            RectTransform rect = button.GetComponent<RectTransform>();
            SetOverlayRect(
                rect,
                Vector2.one,
                Vector2.one,
                new Vector2(-(rightOffset + width), -50f),
                new Vector2(-rightOffset, -10f));

            LayoutElement layout = GetOrAddComponent<LayoutElement>(button.gameObject);
            layout.ignoreLayout = true;
            return button;
        }

        private Button CreateButton(Transform parent, string objectName, string label, UnityEngine.Events.UnityAction callback, Color color)
        {
            return CreateButton(parent, objectName, label, callback, color, null, null);
        }

        private Button CreateButton(
            Transform parent,
            string objectName,
            string label,
            UnityEngine.Events.UnityAction callback,
            Color color,
            Sprite icon,
            Sprite surfaceSprite = null)
        {
            GameObject go = CreateUiObject(UiElementKind.Button, parent, objectName, typeof(RectTransform), typeof(Image), typeof(Button));
            ResetLayoutParticipation(go);

            Image image = go.GetComponent<Image>();
            Color visualTint = surfaceSprite == null ? color : Color.white;
            image.color = visualTint;
            image.sprite = surfaceSprite;
            image.type = Image.Type.Simple;
            image.preserveAspect = false;
            ResetDragComponents(go);

            Button button = go.GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(callback);
            ApplyButtonPalette(button, visualTint);
            ApplyButtonChrome(go, color, surfaceSprite != null);

            if (icon != null)
            {
                CreateIcon(go.transform, "Icon", icon, 58f, 58f);
            }

            Text text = CreateText(go.transform, "Text", label, icon == null ? 18 : 17, icon == null ? TextAnchor.MiddleCenter : TextAnchor.MiddleLeft);
            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = icon == null ? new Vector2(7f, 6f) : new Vector2(82f, 6f);
            textRect.offsetMax = new Vector2(-7f, -6f);

            LayoutElement layout = GetOrAddComponent<LayoutElement>(go);
            layout.ignoreLayout = false;
            layout.minHeight = ReleaseVisualStyle.MinTouchTargetHeight;
            layout.preferredHeight = Mathf.Max(72f, 36f + (CountLines(label) * 24f));
            layout.flexibleWidth = 1f;
            return button;
        }

        private static int CountLines(string value)
        {
            int lineCount = 1;
            if (string.IsNullOrEmpty(value))
            {
                return lineCount;
            }

            for (int i = 0; i < value.Length; i++)
            {
                if (value[i] == '\n')
                {
                    lineCount++;
                }
            }

            return lineCount;
        }

        private static void AddVerticalLayout(RectTransform target, RectOffset padding, float spacing)
        {
            HorizontalLayoutGroup horizontal = target.GetComponent<HorizontalLayoutGroup>();
            if (horizontal != null)
            {
                horizontal.enabled = false;
            }

            VerticalLayoutGroup layout = GetOrAddComponent<VerticalLayoutGroup>(target.gameObject);
            layout.enabled = true;
            layout.padding = padding;
            layout.spacing = spacing;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
        }

        private static void AddHorizontalLayout(RectTransform target, RectOffset padding, float spacing)
        {
            VerticalLayoutGroup vertical = target.GetComponent<VerticalLayoutGroup>();
            if (vertical != null)
            {
                vertical.enabled = false;
            }

            HorizontalLayoutGroup layout = GetOrAddComponent<HorizontalLayoutGroup>(target.gameObject);
            layout.enabled = true;
            layout.padding = padding;
            layout.spacing = spacing;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = true;
            layout.childForceExpandWidth = true;
        }

        private static void ApplyButtonPalette(Button button, Color normalColor)
        {
            ColorBlock colors = button.colors;
            colors.normalColor = normalColor;
            colors.highlightedColor = Color.Lerp(normalColor, Color.white, 0.16f);
            colors.pressedColor = Color.Lerp(normalColor, Color.black, 0.18f);
            colors.selectedColor = Color.Lerp(normalColor, Color.white, 0.1f);
            colors.disabledColor = new Color(normalColor.r * 0.45f, normalColor.g * 0.45f, normalColor.b * 0.45f, 0.5f);
            button.colors = colors;
        }

        private void ApplySafeAreaIfNeeded()
        {
            if (safeAreaRoot == null || Screen.width <= 0 || Screen.height <= 0)
            {
                return;
            }

            Rect safeArea = Screen.safeArea;
            if (safeArea == lastSafeArea)
            {
                return;
            }

            lastSafeArea = safeArea;
            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;
            safeAreaRoot.anchorMin = anchorMin;
            safeAreaRoot.anchorMax = anchorMax;
            safeAreaRoot.offsetMin = Vector2.zero;
            safeAreaRoot.offsetMax = Vector2.zero;
        }

        private void BeginDynamicUiRefresh()
        {
            for (int i = activeDynamicUi.Count - 1; i >= 0; i--)
            {
                GameObject element = activeDynamicUi[i];
                if (element == null)
                {
                    continue;
                }

                ReleaseUiPoolItem poolItem = element.GetComponent<ReleaseUiPoolItem>();
                element.SetActive(false);
                GetPool(poolItem == null ? UiElementKind.Panel : poolItem.Kind).Push(element);
            }

            activeDynamicUi.Clear();
            useDynamicUiPool = true;
        }

        private void EndDynamicUiRefresh()
        {
            useDynamicUiPool = false;
        }

        private GameObject CreateUiObject(
            UiElementKind kind,
            Transform parent,
            string objectName,
            params Type[] requiredComponents)
        {
            GameObject go = null;
            if (useDynamicUiPool)
            {
                Stack<GameObject> pool = GetPool(kind);
                while (pool.Count > 0 && go == null)
                {
                    go = pool.Pop();
                }
            }

            if (go == null)
            {
                go = new GameObject(objectName, requiredComponents);
                go.AddComponent<ReleaseUiPoolItem>().Kind = kind;
            }
            else
            {
                DetachPooledChildren(go);
                for (int i = 0; i < requiredComponents.Length; i++)
                {
                    if (go.GetComponent(requiredComponents[i]) == null)
                    {
                        go.AddComponent(requiredComponents[i]);
                    }
                }
            }

            go.name = objectName;
            go.transform.SetParent(parent, false);
            go.SetActive(true);
            if (useDynamicUiPool)
            {
                activeDynamicUi.Add(go);
            }

            return go;
        }

        private static void DetachPooledChildren(GameObject go)
        {
            for (int i = go.transform.childCount - 1; i >= 0; i--)
            {
                Transform child = go.transform.GetChild(i);
                child.SetParent(null, false);
                child.gameObject.SetActive(false);
            }
        }

        private Stack<GameObject> GetPool(UiElementKind kind)
        {
            return kind switch
            {
                UiElementKind.Text => pooledTexts,
                UiElementKind.Button => pooledButtons,
                UiElementKind.Icon => pooledIcons,
                _ => pooledPanels
            };
        }

        private static void DisableLayoutGroups(GameObject go)
        {
            VerticalLayoutGroup vertical = go.GetComponent<VerticalLayoutGroup>();
            if (vertical != null)
            {
                vertical.enabled = false;
            }

            HorizontalLayoutGroup horizontal = go.GetComponent<HorizontalLayoutGroup>();
            if (horizontal != null)
            {
                horizontal.enabled = false;
            }
        }

        private static void SetPreferredHeight(GameObject go, float preferredHeight)
        {
            LayoutElement layout = GetOrAddComponent<LayoutElement>(go);
            layout.preferredHeight = preferredHeight;
        }

        private static void SetFlexibleHeight(GameObject go, float flexibleHeight)
        {
            LayoutElement layout = GetOrAddComponent<LayoutElement>(go);
            layout.flexibleHeight = flexibleHeight;
        }

        private static void ResetLayoutParticipation(GameObject go)
        {
            LayoutElement layout = go.GetComponent<LayoutElement>();
            if (layout != null)
            {
                layout.ignoreLayout = false;
            }
        }

        private static void ResetDragComponents(GameObject go)
        {
            ReleaseDragHandle dragHandle = go.GetComponent<ReleaseDragHandle>();
            if (dragHandle != null)
            {
                dragHandle.enabled = false;
            }

            ReleaseDragDropTarget dropTarget = go.GetComponent<ReleaseDragDropTarget>();
            if (dropTarget != null)
            {
                dropTarget.enabled = false;
            }

            CanvasGroup canvasGroup = go.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.blocksRaycasts = true;
            }

            go.transform.localScale = Vector3.one;
        }

        private static T GetOrAddComponent<T>(GameObject go) where T : Component
        {
            T component = go.GetComponent<T>();
            return component != null ? component : go.AddComponent<T>();
        }

        private static T GetOrAddExactComponent<T>(GameObject go) where T : Component
        {
            T[] components = go.GetComponents<T>();
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i].GetType() == typeof(T))
                {
                    return components[i];
                }
            }

            return go.AddComponent<T>();
        }

        private static void DisableExactEffect<T>(GameObject go) where T : Behaviour
        {
            T[] components = go.GetComponents<T>();
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i].GetType() == typeof(T))
                {
                    components[i].enabled = false;
                }
            }
        }

        private static void EnsureEventSystem()
        {
            if (UnityEngine.Object.FindAnyObjectByType<EventSystem>() != null)
            {
                return;
            }

            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            DontDestroyOnLoad(eventSystem);
        }

        private enum UiElementKind
        {
            Panel,
            Text,
            Button,
            Icon
        }

        private enum ReleaseScreenMode
        {
            Home,
            LevelSelect,
            Gameplay,
            Pause,
            ClearResult,
            FailResult
        }

        private sealed class ReleaseUiPoolItem : MonoBehaviour
        {
            public UiElementKind Kind;
        }
    }
}
