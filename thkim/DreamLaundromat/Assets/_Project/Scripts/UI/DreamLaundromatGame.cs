using System.Collections.Generic;
using Thkim.DreamLaundromat.Levels;
using Thkim.DreamLaundromat.Rules;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Thkim.DreamLaundromat.UI
{
    public sealed class DreamLaundromatGame : MonoBehaviour
    {
        [SerializeField] private LevelCatalog _levelCatalog;
        [SerializeField] private UiIconCatalog _iconCatalog;

        private readonly Dictionary<string, Button> _dreamButtons = new Dictionary<string, Button>();
        private LevelSession _session;
        private int _levelIndex;
        private string _selectedDreamId;
        private Font _font;
        private RectTransform _safeAreaRoot;
        private Rect _lastSafeArea = new Rect(-1f, -1f, -1f, -1f);
        private Transform _orderRoot;
        private Transform _queueRoot;
        private Transform _machineRoot;
        private Transform _basketRoot;
        private Text _levelText;
        private Text _moveText;
        private Text _messageText;
        private Button _undoButton;
        private Button _restartButton;
        private Button _nextButton;

        public string CurrentLevelId => _session?.State.LevelId ?? string.Empty;
        public LevelStatus CurrentStatus => _session?.State.Status ?? LevelStatus.Failed;

        private void Awake()
        {
            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_font == null)
            {
                _font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            EnsureEventSystem();
            BuildUi();
        }

        private void Start()
        {
            LoadLevel(0);
        }

        private void Update()
        {
            ApplySafeAreaIfNeeded();
        }

        public void Configure(LevelCatalog catalog)
        {
            _levelCatalog = catalog;
        }

        public void Configure(LevelCatalog catalog, UiIconCatalog iconCatalog)
        {
            _levelCatalog = catalog;
            _iconCatalog = iconCatalog;
        }

        public bool TryApplyForTest(PlayerAction action)
        {
            if (_session == null)
            {
                return false;
            }

            ActionResult result = _session.Apply(action);
            RefreshUi(result.Message);
            return result.Success;
        }

        public bool TryLoadLevelForTest(int index)
        {
            if (_levelCatalog == null || index < 0 || index >= _levelCatalog.Levels.Length)
            {
                return false;
            }

            LoadLevel(index);
            return true;
        }

        private void LoadLevel(int index)
        {
            if (_levelCatalog == null || _levelCatalog.Levels.Length == 0)
            {
                SetMessage("Level catalog is missing.");
                return;
            }

            _levelIndex = Mathf.Clamp(index, 0, _levelCatalog.Levels.Length - 1);
            _session = new LevelSession(_levelCatalog.Levels[_levelIndex]);
            _selectedDreamId = string.Empty;
            RefreshUi("Level started.");
        }

        private void BuildUi()
        {
            GameObject canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform root = CreatePanel(canvasObject.transform, "SafeArea", new Color(0.08f, 0.09f, 0.12f, 1f));
            _safeAreaRoot = root;
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;
            ApplySafeAreaIfNeeded();
            AddVerticalLayout(root, new RectOffset(32, 32, 32, 32), 18f);

            RectTransform top = CreatePanel(root, "Top", new Color(0.13f, 0.14f, 0.18f, 1f));
            top.gameObject.AddComponent<LayoutElement>().preferredHeight = 360f;
            AddVerticalLayout(top, new RectOffset(18, 18, 18, 18), 10f);
            _levelText = CreateText(top, "LevelText", "Level", 38, TextAnchor.MiddleLeft);
            _moveText = CreateText(top, "MoveText", "Moves", 34, TextAnchor.MiddleLeft);
            _orderRoot = CreateGroup(top, "Orders", 190f);

            RectTransform middle = CreatePanel(root, "Middle", new Color(0.1f, 0.11f, 0.15f, 1f));
            middle.gameObject.AddComponent<LayoutElement>().preferredHeight = 1160f;
            AddVerticalLayout(middle, new RectOffset(18, 18, 18, 18), 16f);
            _queueRoot = CreateGroup(middle, "Dream Queue", 225f);
            _machineRoot = CreateGroup(middle, "Machines", 360f);
            _basketRoot = CreateGroup(middle, "Storage", 500f);

            RectTransform bottom = CreatePanel(root, "Bottom", new Color(0.13f, 0.14f, 0.18f, 1f));
            bottom.gameObject.AddComponent<LayoutElement>().preferredHeight = 240f;
            AddVerticalLayout(bottom, new RectOffset(18, 18, 18, 18), 14f);
            _messageText = CreateText(bottom, "MessageText", string.Empty, 30, TextAnchor.MiddleLeft);
            RectTransform buttonRow = CreateHorizontalGroup(bottom, "Actions", 96f);
            _undoButton = CreateButton(buttonRow, "Undo", OnUndo);
            _restartButton = CreateButton(buttonRow, "Restart", OnRestart);
            _nextButton = CreateButton(buttonRow, "Next", OnNext);
        }

        private void RefreshUi(string message)
        {
            if (_session == null)
            {
                return;
            }

            _dreamButtons.Clear();
            ClearChildren(_orderRoot);
            ClearChildren(_queueRoot);
            ClearChildren(_machineRoot);
            ClearChildren(_basketRoot);

            LevelState state = _session.State;
            _levelText.text = $"{state.LevelId} ({_levelIndex + 1}/{_levelCatalog.Levels.Length})";
            _moveText.text = $"Moves: {state.RemainingMoves}";
            SetMessage(message);

            CreateSectionTitle(_orderRoot, "Orders - tap a Submit card after selecting a dream");
            RectTransform orderRow = CreateHorizontalGroup(_orderRoot, "OrderCards", 104f);
            for (int i = 0; i < state.Orders.Count; i++)
            {
                OrderRuntimeState order = state.Orders[i];
                string label = order.IsComplete ? $"Submitted: {order.DisplayName}" : BuildOrderLabel(order);
                CreateButton(orderRow, label, () => OnDestinationOrder(order.Id), new Color(0.2f, 0.34f, 0.28f, 1f), _iconCatalog?.SubmitOrder);
            }

            CreateSectionTitle(_queueRoot, "Dream Queue - select a dream first");
            CreateDreamSection(_queueRoot, "Queue", DreamLocation.Queue());

            CreateSectionTitle(_machineRoot, "Machines - transform selected dreams");
            if (state.Machines.Count == 0)
            {
                CreateText(_machineRoot, "NoMachines", "No machines in this level.", 20, TextAnchor.MiddleCenter);
            }

            RectTransform machineRow = state.Machines.Count > 0 ? CreateHorizontalGroup(_machineRoot, "MachineCards", 280f) : null;
            for (int i = 0; i < state.Machines.Count; i++)
            {
                MachineDefinition machine = state.Machines[i];
                RectTransform machinePanel = CreatePanel(machineRow, machine.DisplayName, new Color(0.16f, 0.17f, 0.22f, 1f));
                machinePanel.gameObject.AddComponent<LayoutElement>().preferredHeight = 260f;
                AddVerticalLayout(machinePanel, new RectOffset(8, 8, 8, 8), 8f);
                CreateButton(machinePanel, BuildMachineLabel(machine), () => OnDestinationMachine(machine.Id), new Color(0.22f, 0.3f, 0.43f, 1f), GetMachineIcon(machine.Type));
                Text outputText = CreateText(machinePanel, "OutputTitle", "Output: move processed dreams to Storage before submitting.", 18, TextAnchor.MiddleLeft);
                SetPreferredHeight(outputText.gameObject, 36f);
                CreateDreamSection(machinePanel, $"In {machine.Id}", DreamLocation.Machine(machine.Id), 104f);
            }

            CreateSectionTitle(_basketRoot, "Storage - temporary baskets for machine output");
            for (int i = 0; i < state.Baskets.Count; i++)
            {
                BasketDefinition basket = state.Baskets[i];
                RectTransform basketPanel = CreatePanel(_basketRoot, basket.DisplayName, new Color(0.16f, 0.17f, 0.22f, 1f));
                basketPanel.gameObject.AddComponent<LayoutElement>().preferredHeight = 190f;
                AddVerticalLayout(basketPanel, new RectOffset(8, 8, 8, 8), 8f);
                int used = state.GetUsedCapacity(DreamLocation.Basket(basket.Id));
                CreateButton(basketPanel, $"Store in {basket.DisplayName} {used}/{basket.Capacity}", () => OnDestinationBasket(basket.Id), new Color(0.22f, 0.25f, 0.32f, 1f), _iconCatalog?.StorageBasket);
                CreateDreamSection(basketPanel, $"In {basket.Id}", DreamLocation.Basket(basket.Id));
            }

            _undoButton.interactable = state.Status == LevelStatus.Playing;
            _restartButton.interactable = true;
            _nextButton.interactable = state.Status == LevelStatus.Cleared;
        }

        private void CreateDreamSection(Transform parent, string label, DreamLocation location)
        {
            CreateDreamSection(parent, label, location, 86f);
        }

        private void CreateDreamSection(Transform parent, string label, DreamLocation location, float preferredHeight)
        {
            LevelState state = _session.State;
            RectTransform row = CreateHorizontalGroup(parent, label, preferredHeight);
            bool hasDreams = false;

            for (int i = 0; i < state.Dreams.Count; i++)
            {
                DreamRuntimeState dream = state.Dreams[i];
                if (!dream.Location.Equals(location))
                {
                    continue;
                }

                hasDreams = true;
                Button button = CreateDreamButton(row, dream, () => OnDreamSelected(dream.Id));
                _dreamButtons[dream.Id] = button;

                Color color = dream.Id == _selectedDreamId ? new Color(0.85f, 0.75f, 0.25f, 1f) : GetDreamButtonColor(dream);
                ApplyButtonPalette(button, color);
            }

            if (!hasDreams)
            {
                CreateText(row, "Empty", "Empty", 20, TextAnchor.MiddleCenter);
            }
        }

        private void OnDreamSelected(string dreamId)
        {
            _selectedDreamId = _selectedDreamId == dreamId ? string.Empty : dreamId;
            if (string.IsNullOrEmpty(_selectedDreamId))
            {
                RefreshUi("Selection cleared.");
                return;
            }

            DreamRuntimeState dream = _session.State.FindDream(_selectedDreamId);
            string name = dream?.DisplayName ?? "Dream";
            RefreshUi($"{name} selected. Tap Submit, Machine, or Storage.");
        }

        private void OnDestinationBasket(string basketId)
        {
            if (!TryGetSelectedDream(out DreamRuntimeState dream))
            {
                return;
            }

            PlayerAction action = dream.Location.Kind == LocationKind.Machine
                ? PlayerAction.TakeFromMachine(dream.Id, DreamLocation.Basket(basketId))
                : PlayerAction.MoveToBasket(dream.Id, basketId);
            ApplySelected(action);
        }

        private void OnDestinationMachine(string machineId)
        {
            if (!TryGetSelectedDream(out DreamRuntimeState dream))
            {
                return;
            }

            ApplySelected(PlayerAction.MoveToMachine(dream.Id, machineId));
        }

        private void OnDestinationOrder(string orderId)
        {
            if (!TryGetSelectedDream(out DreamRuntimeState dream))
            {
                return;
            }

            ApplySelected(PlayerAction.Submit(dream.Id, orderId));
        }

        private void OnUndo()
        {
            string message = _session.Undo() ? "Undone." : "Nothing to undo.";
            _selectedDreamId = string.Empty;
            RefreshUi(message);
        }

        private void OnRestart()
        {
            _session.Restart();
            _selectedDreamId = string.Empty;
            RefreshUi("Restarted.");
        }

        private void OnNext()
        {
            if (_levelCatalog == null || _levelCatalog.Levels.Length == 0)
            {
                return;
            }

            int next = Mathf.Min(_levelIndex + 1, _levelCatalog.Levels.Length - 1);
            LoadLevel(next);
        }

        private bool TryGetSelectedDream(out DreamRuntimeState dream)
        {
            dream = null;
            if (string.IsNullOrEmpty(_selectedDreamId))
            {
                SetMessage("Select a dream first.");
                return false;
            }

            dream = _session.State.FindDream(_selectedDreamId);
            if (dream == null)
            {
                _selectedDreamId = string.Empty;
                SetMessage("Selected dream is no longer available.");
                return false;
            }

            return true;
        }

        private void ApplySelected(PlayerAction action)
        {
            ActionResult result = _session.Apply(action);
            if (result.Success)
            {
                _selectedDreamId = string.Empty;
            }

            RefreshUi(result.Message);
        }

        private void SetMessage(string message)
        {
            if (_messageText != null)
            {
                string status = _session?.State.Status == LevelStatus.Cleared ? "Cleared! " : string.Empty;
                if (_session?.State.Status == LevelStatus.Failed)
                {
                    status = $"Failed: {_session.State.FailureReason} ";
                }

                _messageText.text = status + message;
            }
        }

        private string BuildDreamLabel(DreamRuntimeState dream)
        {
            string stain = dream.Attributes.Stain == DreamStain.Nightmare ? "Nightmare" : "Clean";
            string moisture = dream.Attributes.Moisture == DreamMoisture.Wet ? "Wet" : "Dry";
            return $"{dream.DisplayName}\n{stain} / {moisture}";
        }

        private static string BuildOrderLabel(OrderRuntimeState order)
        {
            string label = $"Submit: {order.DisplayName}";
            for (int i = 0; i < order.Requirements.Length; i++)
            {
                label += $"\n{order.Requirements[i].Describe()} ({order.FulfilledCounts[i]}/{order.Requirements[i].Count})";
            }

            return label;
        }

        private static string BuildMachineLabel(MachineDefinition machine)
        {
            if (machine.Type == MachineType.Washer)
            {
                return $"{machine.DisplayName}\nNightmare/Dry -> Clean/Wet";
            }

            if (machine.Type == MachineType.Dryer)
            {
                return $"{machine.DisplayName}\nWet -> Dry";
            }

            return $"{machine.DisplayName}\nUnknown transform";
        }

        private Sprite GetMachineIcon(MachineType type)
        {
            if (_iconCatalog == null)
            {
                return null;
            }

            return type == MachineType.Washer ? _iconCatalog.WasherMachine : _iconCatalog.DryerMachine;
        }

        private Sprite GetDreamStainIcon(DreamRuntimeState dream)
        {
            if (_iconCatalog == null)
            {
                return null;
            }

            return dream.Attributes.Stain == DreamStain.Nightmare ? _iconCatalog.NightmareDream : _iconCatalog.CleanDream;
        }

        private Sprite GetDreamMoistureIcon(DreamRuntimeState dream)
        {
            if (_iconCatalog == null)
            {
                return null;
            }

            return dream.Attributes.Moisture == DreamMoisture.Wet ? _iconCatalog.WetState : _iconCatalog.DryState;
        }

        private static Color GetDreamButtonColor(DreamRuntimeState dream)
        {
            if (dream.Attributes.Stain == DreamStain.Nightmare)
            {
                return new Color(0.29f, 0.23f, 0.37f, 1f);
            }

            if (dream.Attributes.Moisture == DreamMoisture.Wet)
            {
                return new Color(0.18f, 0.27f, 0.42f, 1f);
            }

            return new Color(0.2f, 0.3f, 0.34f, 1f);
        }

        private RectTransform CreatePanel(Transform parent, string name, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.color = color;
            return go.GetComponent<RectTransform>();
        }

        private void ApplySafeAreaIfNeeded()
        {
            if (_safeAreaRoot == null || Screen.width <= 0 || Screen.height <= 0)
            {
                return;
            }

            Rect safeArea = Screen.safeArea;
            if (safeArea == _lastSafeArea)
            {
                return;
            }

            _lastSafeArea = safeArea;
            Vector2 anchorMin = safeArea.position;
            Vector2 anchorMax = safeArea.position + safeArea.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;

            _safeAreaRoot.anchorMin = anchorMin;
            _safeAreaRoot.anchorMax = anchorMax;
            _safeAreaRoot.offsetMin = Vector2.zero;
            _safeAreaRoot.offsetMax = Vector2.zero;
        }

        private RectTransform CreateGroup(Transform parent, string name, float preferredHeight)
        {
            RectTransform group = CreatePanel(parent, name, new Color(0.12f, 0.13f, 0.17f, 1f));
            group.gameObject.AddComponent<LayoutElement>().preferredHeight = preferredHeight;
            AddVerticalLayout(group, new RectOffset(12, 12, 12, 12), 8f);
            return group;
        }

        private RectTransform CreateHorizontalGroup(Transform parent, string name, float preferredHeight)
        {
            RectTransform group = CreatePanel(parent, name, new Color(0.12f, 0.13f, 0.17f, 0.25f));
            group.gameObject.AddComponent<LayoutElement>().preferredHeight = preferredHeight;
            HorizontalLayoutGroup layout = group.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(8, 8, 8, 8);
            layout.spacing = 8f;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            return group;
        }

        private void CreateSectionTitle(Transform parent, string text)
        {
            CreateText(parent, "SectionTitle", text, 22, TextAnchor.MiddleLeft);
        }

        private static void AddVerticalLayout(RectTransform target, RectOffset padding, float spacing)
        {
            VerticalLayoutGroup layout = target.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = padding;
            layout.spacing = spacing;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = true;
        }

        private Text CreateText(Transform parent, string name, string text, int fontSize, TextAnchor alignment)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            Text label = go.GetComponent<Text>();
            label.font = _font;
            label.text = text;
            label.fontSize = fontSize;
            label.alignment = alignment;
            label.color = new Color(0.92f, 0.94f, 0.98f, 1f);
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 16;
            label.resizeTextMaxSize = fontSize;
            go.AddComponent<LayoutElement>().preferredHeight = Mathf.Max(48f, fontSize + 18f);
            return label;
        }

        private static void SetPreferredHeight(GameObject target, float preferredHeight)
        {
            LayoutElement layout = target.GetComponent<LayoutElement>();
            if (layout == null)
            {
                layout = target.AddComponent<LayoutElement>();
            }

            layout.minHeight = preferredHeight;
            layout.preferredHeight = preferredHeight;
        }

        private Button CreateButton(Transform parent, string label, UnityEngine.Events.UnityAction callback)
        {
            return CreateButton(parent, label, callback, new Color(0.22f, 0.25f, 0.32f, 1f));
        }

        private Button CreateButton(Transform parent, string label, UnityEngine.Events.UnityAction callback, Color color)
        {
            return CreateButton(parent, label, callback, color, null);
        }

        private Button CreateButton(Transform parent, string label, UnityEngine.Events.UnityAction callback, Color color, Sprite icon)
        {
            GameObject go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);

            Image image = go.GetComponent<Image>();
            image.color = color;

            Button button = go.GetComponent<Button>();
            button.onClick.AddListener(callback);
            ApplyButtonPalette(button, color);

            if (icon != null)
            {
                HorizontalLayoutGroup layout = go.AddComponent<HorizontalLayoutGroup>();
                layout.padding = new RectOffset(12, 14, 8, 8);
                layout.spacing = 10f;
                layout.childAlignment = TextAnchor.MiddleCenter;
                layout.childControlHeight = true;
                layout.childControlWidth = true;
                layout.childForceExpandHeight = true;
                layout.childForceExpandWidth = false;

                CreateIcon(go.transform, "Icon", icon, 54f, 54f);
                CreateButtonText(go.transform, "Text", label, 23, TextAnchor.MiddleLeft);
            }
            else
            {
                Text text = CreateButtonText(go.transform, "Text", label, 24, TextAnchor.MiddleCenter);
                RectTransform textRect = text.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = new Vector2(8f, 8f);
                textRect.offsetMax = new Vector2(-8f, -8f);
            }

            LayoutElement buttonLayoutElement = go.AddComponent<LayoutElement>();
            buttonLayoutElement.minHeight = 72f;
            buttonLayoutElement.preferredHeight = label.Contains("\n") ? 104f : 84f;
            buttonLayoutElement.flexibleWidth = 1f;

            return button;
        }

        private Button CreateDreamButton(Transform parent, DreamRuntimeState dream, UnityEngine.Events.UnityAction callback)
        {
            GameObject go = new GameObject(BuildDreamLabel(dream), typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);

            Color color = GetDreamButtonColor(dream);
            Image image = go.GetComponent<Image>();
            image.color = color;

            Button button = go.GetComponent<Button>();
            button.onClick.AddListener(callback);
            ApplyButtonPalette(button, color);

            HorizontalLayoutGroup layout = go.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(12, 12, 8, 8);
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = true;
            layout.childForceExpandWidth = false;

            // Dream cards show both rule axes because stain and moisture can change independently.
            RectTransform iconGroup = CreateTransparentHorizontalGroup(go.transform, "DreamIcons", 92f);
            CreateIcon(iconGroup, "StainIcon", GetDreamStainIcon(dream), 44f, 44f);
            CreateIcon(iconGroup, "MoistureIcon", GetDreamMoistureIcon(dream), 34f, 34f);
            CreateButtonText(go.transform, "Text", BuildDreamLabel(dream), 22, TextAnchor.MiddleLeft);

            LayoutElement layoutElement = go.AddComponent<LayoutElement>();
            layoutElement.minHeight = 88f;
            layoutElement.preferredHeight = 104f;
            layoutElement.flexibleWidth = 1f;

            return button;
        }

        private RectTransform CreateTransparentHorizontalGroup(Transform parent, string name, float preferredWidth)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            HorizontalLayoutGroup layout = go.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 4f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlHeight = true;
            layout.childControlWidth = true;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = false;

            LayoutElement layoutElement = go.AddComponent<LayoutElement>();
            layoutElement.preferredWidth = preferredWidth;
            layoutElement.flexibleWidth = 0f;
            return go.GetComponent<RectTransform>();
        }

        private Image CreateIcon(Transform parent, string name, Sprite sprite, float width, float height)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);

            Image image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.color = sprite == null ? new Color(1f, 1f, 1f, 0f) : Color.white;

            LayoutElement layout = go.AddComponent<LayoutElement>();
            layout.minWidth = width;
            layout.preferredWidth = width;
            layout.minHeight = height;
            layout.preferredHeight = height;
            layout.flexibleWidth = 0f;
            layout.flexibleHeight = 0f;
            return image;
        }

        private Text CreateButtonText(Transform parent, string name, string text, int fontSize, TextAnchor alignment)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);

            Text label = go.GetComponent<Text>();
            label.font = _font;
            label.text = text;
            label.fontSize = fontSize;
            label.alignment = alignment;
            label.color = new Color(0.94f, 0.96f, 0.99f, 1f);
            label.resizeTextForBestFit = true;
            label.resizeTextMinSize = 14;
            label.resizeTextMaxSize = fontSize;

            LayoutElement layout = go.AddComponent<LayoutElement>();
            layout.minHeight = 48f;
            layout.flexibleHeight = 1f;
            layout.flexibleWidth = 1f;
            return label;
        }

        private static void ApplyButtonPalette(Button button, Color normalColor)
        {
            ColorBlock colors = button.colors;
            colors.normalColor = normalColor;
            colors.highlightedColor = Color.Lerp(normalColor, Color.white, 0.16f);
            colors.pressedColor = Color.Lerp(normalColor, Color.black, 0.18f);
            colors.selectedColor = Color.Lerp(normalColor, Color.white, 0.1f);
            colors.disabledColor = new Color(normalColor.r * 0.55f, normalColor.g * 0.55f, normalColor.b * 0.55f, 0.55f);
            button.colors = colors;
        }

        private static void ClearChildren(Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
            {
                Destroy(parent.GetChild(i).gameObject);
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
    }
}
