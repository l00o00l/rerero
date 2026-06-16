using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Thkim.DreamLaundromat.DynamicLab
{
    public sealed class DynamicLabDebugGame : MonoBehaviour
    {
        [SerializeField] private int _recipeIndex;
        [SerializeField] private int _seed = 1;

        private readonly List<DynamicRoundState> _history = new List<DynamicRoundState>();
        private DynamicRoundDefinition _roundDefinition;
        private DynamicRoundState _state;
        private Font _font;
        private RectTransform _root;
        private Transform _dreamRoot;
        private Transform _orderRoot;
        private Transform _previewRoot;
        private Transform _storageRoot;
        private Transform _modifierRoot;
        private Transform _actionRoot;
        private Text _headerText;
        private Text _messageText;
        private int _selectedDreamSlotId = -1;
        private int _selectedOrderSlotId = -1;
        private int _selectedStorageSlotId = -1;

        public DynamicRoundStatus CurrentStatus => _state?.Status ?? DynamicRoundStatus.Ready;
        public int CompletedOrders => _state?.CompletedOrders ?? 0;
        public string CurrentRoundId => _roundDefinition?.RoundId ?? string.Empty;

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
            if (_state == null)
            {
                LoadGeneratedRound(_recipeIndex, _seed);
            }
        }

        public void LoadRoundForTest(DynamicRoundDefinition definition)
        {
            LoadRound(definition, "Loaded test round.");
        }

        public bool TryApplyForTest(DynamicPlayerAction action)
        {
            if (_state == null)
            {
                return false;
            }

            DynamicActionResult result = ApplyAction(action);
            return result.Success;
        }

        private void LoadGeneratedRound(int recipeIndex, int seed)
        {
            DynamicStageRecipe[] recipes = DynamicSampleRecipes.CreateAll();
            int index = Mathf.Clamp(recipeIndex, 0, recipes.Length - 1);
            DynamicRoundCandidateReport accepted = FindAcceptedCandidate(recipes[index], seed);
            if (accepted != null)
            {
                _recipeIndex = index;
                _seed = accepted.Seed;
                LoadRound(accepted.Round, $"Loaded accepted candidate from {recipes[index].RecipeId}.");
                return;
            }

            LoadRound(DynamicSampleRounds.CreateStateAssignmentRound(), "Loaded fallback handwritten round.");
        }

        private static DynamicRoundCandidateReport FindAcceptedCandidate(DynamicStageRecipe recipe, int seedStart)
        {
            for (int offset = 0; offset < 40; offset++)
            {
                DynamicRoundCandidateReport report = DynamicRoundGenerator.GenerateCandidate(recipe, seedStart + offset);
                if (report.Accepted)
                {
                    return report;
                }
            }

            return null;
        }

        private void LoadRound(DynamicRoundDefinition definition, string message)
        {
            _roundDefinition = definition;
            _state = DynamicRoundInitializer.CreateInitialState(definition);
            _history.Clear();
            _selectedDreamSlotId = -1;
            _selectedOrderSlotId = -1;
            _selectedStorageSlotId = -1;
            RefreshUi(message);
        }

        private DynamicActionResult ApplyAction(DynamicPlayerAction action)
        {
            _history.Add(_state.Clone());
            DynamicActionResult result = DynamicRulesEngine.Apply(_state, action);
            if (!result.Success)
            {
                _history.RemoveAt(_history.Count - 1);
            }
            else if (action.Type == DynamicActionType.SubmitDream || action.Type == DynamicActionType.StoreDream)
            {
                _selectedDreamSlotId = -1;
                _selectedOrderSlotId = -1;
            }

            RefreshUi(result.Message);
            return result;
        }

        private void BuildUi()
        {
            GameObject canvasObject = new GameObject("DynamicLabCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;

            _root = CreatePanel(canvasObject.transform, "Root", new Color(0.08f, 0.09f, 0.12f, 1f));
            _root.anchorMin = Vector2.zero;
            _root.anchorMax = Vector2.one;
            _root.offsetMin = Vector2.zero;
            _root.offsetMax = Vector2.zero;
            AddVerticalLayout(_root, new RectOffset(24, 24, 24, 24), 12f);

            RectTransform top = CreatePanel(_root, "Top", new Color(0.14f, 0.15f, 0.19f, 1f));
            top.gameObject.AddComponent<LayoutElement>().preferredHeight = 180f;
            AddVerticalLayout(top, new RectOffset(14, 14, 14, 14), 8f);
            _headerText = CreateText(top, "Header", "Dynamic Lab", 30, TextAnchor.MiddleLeft);
            _messageText = CreateText(top, "Message", string.Empty, 22, TextAnchor.MiddleLeft);

            RectTransform content = CreatePanel(_root, "Content", new Color(0.1f, 0.11f, 0.15f, 1f));
            content.gameObject.AddComponent<LayoutElement>().preferredHeight = 1320f;
            AddVerticalLayout(content, new RectOffset(14, 14, 14, 14), 12f);

            _dreamRoot = CreateSection(content, "Dreams", 310f);
            _orderRoot = CreateSection(content, "Orders", 250f);
            _previewRoot = CreateSection(content, "Preview", 210f);
            _storageRoot = CreateSection(content, "Storage", 230f);
            _modifierRoot = CreateSection(content, "Modifiers", 180f);
            _actionRoot = CreateSection(content, "Actions", 260f);

            RectTransform bottom = CreatePanel(_root, "Bottom", new Color(0.14f, 0.15f, 0.19f, 1f));
            bottom.gameObject.AddComponent<LayoutElement>().preferredHeight = 170f;
            AddHorizontalLayout(bottom, new RectOffset(12, 12, 12, 12), 10f);
            CreateButton(bottom, "Undo", OnUndo, new Color(0.22f, 0.24f, 0.31f, 1f));
            CreateButton(bottom, "Restart", OnRestart, new Color(0.22f, 0.24f, 0.31f, 1f));
            CreateButton(bottom, "Next Seed", OnNextSeed, new Color(0.23f, 0.31f, 0.27f, 1f));
        }

        private void RefreshUi(string message)
        {
            if (_state == null)
            {
                return;
            }

            ClearChildren(_dreamRoot);
            ClearChildren(_orderRoot);
            ClearChildren(_previewRoot);
            ClearChildren(_storageRoot);
            ClearChildren(_modifierRoot);
            ClearChildren(_actionRoot);

            _headerText.text = $"{_state.RoundId} | Moves {_state.RemainingMoves} | Orders {_state.CompletedOrders}/{_state.TargetCompletedOrders} | {_state.Status}";
            _messageText.text = message;

            RenderDreams();
            RenderOrders();
            RenderPreview();
            RenderStorage();
            RenderModifiers();
            RenderActions();
        }

        private void RenderDreams()
        {
            CreateTitle(_dreamRoot, "Active Dreams");
            RectTransform row = CreateRow(_dreamRoot, "ActiveDreamRow", 240f);
            for (int i = 0; i < _state.ActiveDreams.Count; i++)
            {
                DynamicDreamSlot slot = _state.ActiveDreams[i];
                string label = slot.IsEmpty
                    ? $"Slot {slot.SlotId}\nEmpty"
                    : $"Slot {slot.SlotId}\n{Describe(slot.Dream.Attributes)}{DescribeSlotLock(slot.SlotId)}";
                Color color = GetDreamSlotColor(slot.SlotId);

                CreateButton(row, label, () => OnDreamSlotSelected(slot.SlotId), color);
            }
        }

        private void RenderOrders()
        {
            CreateTitle(_orderRoot, "Active Orders");
            RectTransform row = CreateRow(_orderRoot, "ActiveOrderRow", 180f);
            for (int i = 0; i < _state.ActiveOrders.Count; i++)
            {
                DynamicOrderSlot slot = _state.ActiveOrders[i];
                string label = slot.IsEmpty
                    ? $"Order {slot.SlotId}\nEmpty"
                    : $"Order {slot.SlotId}\n{Describe(slot.Order.Requirement)}\n{slot.Order.FulfilledCount}/{slot.Order.Requirement.Count}";
                Color color = _selectedOrderSlotId == slot.SlotId
                    ? new Color(0.5f, 0.56f, 0.3f, 1f)
                    : new Color(0.18f, 0.31f, 0.28f, 1f);

                CreateButton(row, label, () => OnOrderSlotSelected(slot.SlotId), color);
            }
        }

        private void RenderPreview()
        {
            CreateTitle(_previewRoot, "Stream Preview");
            CreateText(_previewRoot, "DreamPreview", $"Dreams: {DescribeDreamPreview()}", 20, TextAnchor.MiddleLeft);
            CreateText(_previewRoot, "OrderPreview", $"Orders: {DescribeOrderPreview()}", 20, TextAnchor.MiddleLeft);
        }

        private void RenderStorage()
        {
            CreateTitle(_storageRoot, "Storage");
            RectTransform row = CreateRow(_storageRoot, "StorageRow", 160f);
            for (int i = 0; i < _state.StorageSlots.Count; i++)
            {
                DynamicStorageSlot slot = _state.StorageSlots[i];
                string label = slot.IsEmpty
                    ? $"Storage {slot.SlotId}\nEmpty"
                    : $"Storage {slot.SlotId}\n{Describe(slot.Dream.Attributes)}";
                Color color = _selectedStorageSlotId == slot.SlotId
                    ? new Color(0.52f, 0.44f, 0.3f, 1f)
                    : new Color(0.19f, 0.22f, 0.29f, 1f);

                CreateButton(row, label, () => OnStorageSlotSelected(slot.SlotId), color);
            }
        }

        private void RenderModifiers()
        {
            CreateTitle(_modifierRoot, "Modifiers");
            if (_state.ModifierDefinitions.Length == 0)
            {
                CreateText(_modifierRoot, "NoModifiers", "None", 20, TextAnchor.MiddleLeft);
                return;
            }

            RectTransform row = CreateRow(_modifierRoot, "ModifierRow", 110f);
            for (int i = 0; i < _state.ModifierDefinitions.Length; i++)
            {
                DynamicModifierDefinition definition = _state.ModifierDefinitions[i];
                DynamicModifierState modifierState = DynamicModifierPipeline.FindState(_state, definition.Id);
                int charges = modifierState?.RemainingCharges ?? 0;
                string label = $"{definition.DisplayName}\n{definition.Effect} x{charges}";
                if (definition.Type == DynamicModifierType.Item)
                {
                    CreateButton(row, label, () => ApplyAction(DynamicPlayerAction.UseItem(definition.Id)), new Color(0.29f, 0.31f, 0.48f, 1f));
                }
                else
                {
                    CreateText(row, definition.Id, label, 18, TextAnchor.MiddleCenter);
                }
            }
        }

        private void RenderActions()
        {
            CreateTitle(_actionRoot, "Actions");
            RectTransform operationRow = CreateRow(_actionRoot, "OperationRow", 90f);
            for (int i = 0; i < _state.ActionSet.Length; i++)
            {
                DynamicOperation operation = _state.ActionSet[i];
                CreateButton(operationRow, operation.ToString(), () => OnOperation(operation), new Color(0.22f, 0.29f, 0.42f, 1f));
            }

            RectTransform flowRow = CreateRow(_actionRoot, "FlowRow", 90f);
            CreateButton(flowRow, "Submit", OnSubmit, new Color(0.22f, 0.35f, 0.28f, 1f));

            for (int i = 0; i < _state.StorageSlots.Count; i++)
            {
                int storageSlotId = _state.StorageSlots[i].SlotId;
                CreateButton(flowRow, $"Store {storageSlotId}", () => OnStore(storageSlotId), new Color(0.22f, 0.25f, 0.32f, 1f));
            }

            RectTransform recallRow = CreateRow(_actionRoot, "RecallRow", 70f);
            for (int i = 0; i < _state.ActiveDreams.Count; i++)
            {
                int dreamSlotId = _state.ActiveDreams[i].SlotId;
                CreateButton(recallRow, $"Recall -> {dreamSlotId}", () => OnRecall(dreamSlotId), new Color(0.27f, 0.24f, 0.34f, 1f));
            }
        }

        private void OnDreamSlotSelected(int slotId)
        {
            DynamicDreamSlot slot = _state.FindActiveDreamSlot(slotId);
            if (slot == null || slot.IsEmpty)
            {
                _selectedDreamSlotId = -1;
                RefreshUi("Dream slot is empty.");
                return;
            }

            _selectedDreamSlotId = _selectedDreamSlotId == slotId ? -1 : slotId;
            RefreshUi(_selectedDreamSlotId < 0 ? "Dream selection cleared." : $"Selected dream slot {slotId}.");
        }

        private void OnOrderSlotSelected(int slotId)
        {
            DynamicOrderSlot slot = _state.FindActiveOrderSlot(slotId);
            if (slot == null || slot.IsEmpty)
            {
                _selectedOrderSlotId = -1;
                RefreshUi("Order slot is empty.");
                return;
            }

            _selectedOrderSlotId = _selectedOrderSlotId == slotId ? -1 : slotId;
            RefreshUi(_selectedOrderSlotId < 0 ? "Order selection cleared." : $"Selected order slot {slotId}.");
        }

        private void OnStorageSlotSelected(int slotId)
        {
            DynamicStorageSlot slot = _state.FindStorageSlot(slotId);
            if (slot == null || slot.IsEmpty)
            {
                _selectedStorageSlotId = -1;
                RefreshUi("Storage slot is empty.");
                return;
            }

            _selectedStorageSlotId = _selectedStorageSlotId == slotId ? -1 : slotId;
            RefreshUi(_selectedStorageSlotId < 0 ? "Storage selection cleared." : $"Selected storage slot {slotId}.");
        }

        private void OnOperation(DynamicOperation operation)
        {
            if (_selectedDreamSlotId < 0)
            {
                RefreshUi("Select an active dream first.");
                return;
            }

            ApplyAction(DynamicPlayerAction.ApplyOperation(_selectedDreamSlotId, operation));
        }

        private void OnSubmit()
        {
            if (_selectedDreamSlotId < 0 || _selectedOrderSlotId < 0)
            {
                RefreshUi("Select an active dream and an active order first.");
                return;
            }

            ApplyAction(DynamicPlayerAction.SubmitDream(_selectedDreamSlotId, _selectedOrderSlotId));
        }

        private void OnStore(int storageSlotId)
        {
            if (_selectedDreamSlotId < 0)
            {
                RefreshUi("Select an active dream first.");
                return;
            }

            ApplyAction(DynamicPlayerAction.StoreDream(_selectedDreamSlotId, storageSlotId));
        }

        private void OnRecall(int activeDreamSlotId)
        {
            if (_selectedStorageSlotId < 0)
            {
                RefreshUi("Select a storage slot first.");
                return;
            }

            ApplyAction(DynamicPlayerAction.RecallDream(_selectedStorageSlotId, activeDreamSlotId));
        }

        private bool IsActiveDreamSlotLocked(int slotId)
        {
            for (int i = 0; i < _state.Modifiers.Count; i++)
            {
                DynamicModifierState modifierState = _state.Modifiers[i];
                DynamicModifierDefinition definition = DynamicModifierPipeline.FindDefinition(_state, modifierState.ModifierId);
                if (definition != null
                    && definition.Effect == DynamicModifierEffect.LockActiveDreamSlot
                    && !modifierState.IsResolved
                    && modifierState.BoundTargetKind == DynamicModifierTargetKind.ActiveDreamSlot
                    && modifierState.BoundTargetId == slotId)
                {
                    return true;
                }
            }

            return false;
        }

        private Color GetDreamSlotColor(int slotId)
        {
            if (_selectedDreamSlotId == slotId)
            {
                return new Color(0.68f, 0.58f, 0.28f, 1f);
            }

            if (IsActiveDreamSlotLocked(slotId))
            {
                return new Color(0.42f, 0.19f, 0.22f, 1f);
            }

            return new Color(0.2f, 0.27f, 0.35f, 1f);
        }

        private string DescribeSlotLock(int slotId)
        {
            return IsActiveDreamSlotLocked(slotId) ? "\nLocked" : string.Empty;
        }

        private void OnUndo()
        {
            if (_history.Count == 0)
            {
                RefreshUi("Nothing to undo.");
                return;
            }

            _state = _history[_history.Count - 1];
            _history.RemoveAt(_history.Count - 1);
            _selectedDreamSlotId = -1;
            _selectedOrderSlotId = -1;
            _selectedStorageSlotId = -1;
            RefreshUi("Undone.");
        }

        private void OnRestart()
        {
            LoadRound(_roundDefinition, "Restarted.");
        }

        private void OnNextSeed()
        {
            LoadGeneratedRound(_recipeIndex, _seed + 1);
        }

        private string DescribeDreamPreview()
        {
            if (_state.DreamPreview.Count == 0)
            {
                return "none";
            }

            var values = new List<string>();
            for (int i = 0; i < _state.DreamPreview.Count; i++)
            {
                values.Add(Describe(_state.DreamPreview[i].Attributes));
            }

            return string.Join(" | ", values);
        }

        private string DescribeOrderPreview()
        {
            if (_state.OrderPreview.Count == 0)
            {
                return "none";
            }

            var values = new List<string>();
            for (int i = 0; i < _state.OrderPreview.Count; i++)
            {
                values.Add(Describe(_state.OrderPreview[i].Requirement));
            }

            return string.Join(" | ", values);
        }

        private static string Describe(DynamicDreamAttributes attributes)
        {
            return $"{attributes.Taint}/{attributes.Mood}/{attributes.Clarity}/{attributes.Stability}";
        }

        private static string Describe(DynamicOrderRequirement requirement)
        {
            var parts = new List<string>();
            if (requirement.HasTaint)
            {
                parts.Add(requirement.RequiredTaint.ToString());
            }

            if (requirement.HasMood)
            {
                parts.Add(requirement.RequiredMood.ToString());
            }

            if (requirement.HasClarity)
            {
                parts.Add(requirement.RequiredClarity.ToString());
            }

            if (requirement.HasStability)
            {
                parts.Add(requirement.RequiredStability.ToString());
            }

            return parts.Count == 0 ? "Any stable" : string.Join("/", parts);
        }

        private RectTransform CreateSection(Transform parent, string name, float preferredHeight)
        {
            RectTransform section = CreatePanel(parent, name, new Color(0.13f, 0.14f, 0.18f, 1f));
            section.gameObject.AddComponent<LayoutElement>().preferredHeight = preferredHeight;
            AddVerticalLayout(section, new RectOffset(10, 10, 10, 10), 8f);
            return section;
        }

        private RectTransform CreateRow(Transform parent, string name, float preferredHeight)
        {
            RectTransform row = CreatePanel(parent, name, new Color(0.11f, 0.12f, 0.16f, 0.35f));
            row.gameObject.AddComponent<LayoutElement>().preferredHeight = preferredHeight;
            AddHorizontalLayout(row, new RectOffset(6, 6, 6, 6), 6f);
            return row;
        }

        private void CreateTitle(Transform parent, string text)
        {
            CreateText(parent, "Title", text, 22, TextAnchor.MiddleLeft);
        }

        private RectTransform CreatePanel(Transform parent, string name, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.color = color;
            return go.GetComponent<RectTransform>();
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
            label.resizeTextMinSize = 12;
            label.resizeTextMaxSize = fontSize;
            go.AddComponent<LayoutElement>().preferredHeight = Mathf.Max(36f, fontSize + 14f);
            return label;
        }

        private Button CreateButton(Transform parent, string label, UnityEngine.Events.UnityAction callback, Color color)
        {
            GameObject go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            Image image = go.GetComponent<Image>();
            image.color = color;

            Button button = go.GetComponent<Button>();
            button.onClick.AddListener(callback);
            ApplyButtonPalette(button, color);

            Text text = CreateText(go.transform, "Text", label, 18, TextAnchor.MiddleCenter);
            RectTransform rect = text.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(6f, 6f);
            rect.offsetMax = new Vector2(-6f, -6f);

            LayoutElement layout = go.AddComponent<LayoutElement>();
            layout.minHeight = 58f;
            layout.preferredHeight = 82f;
            layout.flexibleWidth = 1f;
            return button;
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

        private static void AddHorizontalLayout(RectTransform target, RectOffset padding, float spacing)
        {
            HorizontalLayoutGroup layout = target.gameObject.AddComponent<HorizontalLayoutGroup>();
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
            if (Object.FindAnyObjectByType<EventSystem>() != null)
            {
                return;
            }

            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            DontDestroyOnLoad(eventSystem);
        }
    }
}
