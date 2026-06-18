using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Thkim.DreamLaundromat.DynamicLab;
using Thkim.DreamLaundromat.Gameplay.ReleaseSlice;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Thkim.DreamLaundromat.Tests.PlayMode
{
    public sealed class ReleaseGameplaySlicePlayModeTests
    {
        [TearDown]
        public void TearDown()
        {
            ReleaseGameController.ClearTestServices();
        }

        [UnityTest]
        public IEnumerator ReleaseScene_LoadsFirstReleaseLevel()
        {
            yield return LoadReleaseSceneWithTestStore();

            ReleaseGameController game = Object.FindAnyObjectByType<ReleaseGameController>();

            Assert.That(game, Is.Not.Null);
            Assert.That(game.CurrentLevelId, Is.EqualTo("DL-RS-001"));
            Assert.That(game.CurrentStatus, Is.EqualTo(DynamicRoundStatus.Playing));
            Assert.That(game.CurrentScreenForTest, Is.EqualTo("Home"));
        }

        [UnityTest]
        public IEnumerator ReleaseScene_StartsAtSavedHighestUnlockedLevel()
        {
            var store = new ReleaseMemoryProgressStore();
            store.Save(new ReleaseProgressState { HighestUnlockedLevelIndex = 2 });

            yield return LoadReleaseSceneWithTestStore(store);

            ReleaseGameController game = Object.FindAnyObjectByType<ReleaseGameController>();

            Assert.That(game, Is.Not.Null);
            Assert.That(game.CurrentLevelIndex, Is.EqualTo(2));
            Assert.That(game.CurrentLevelId, Is.EqualTo("DL-RS-003"));
            Assert.That(game.CurrentStatus, Is.EqualTo(DynamicRoundStatus.Playing));
            Assert.That(game.CurrentScreenForTest, Is.EqualTo("Home"));
        }

        [UnityTest]
        public IEnumerator ReleaseScene_StartsOnHomeScreen()
        {
            yield return LoadReleaseSceneWithTestStore();

            Text[] labels = Object.FindObjectsByType<Text>(FindObjectsInactive.Exclude);

            AssertTextExists(labels, "Dream Laundromat");
            AssertTextExists(labels, "Continue");
            AssertTextExists(labels, "Level Select");
            AssertTextExists(labels, "Open Orders");
        }

        [UnityTest]
        public IEnumerator ReleaseScene_ShowsCoreInteractionRegions()
        {
            yield return LoadReleaseSceneWithTestStore();

            ReleaseGameController game = Object.FindAnyObjectByType<ReleaseGameController>();
            Assert.That(game, Is.Not.Null);
            Assert.That(game.TryOpenGameplayForTest(), Is.True);
            yield return null;

            Text[] labels = Object.FindObjectsByType<Text>(FindObjectsInactive.Exclude);

            AssertObjectExists("ActiveDreams");
            AssertObjectExists("ActiveOrders");
            AssertObjectExists("ActionPanel");
            AssertButtonTextEquals("Operation-Wash", "W");
            AssertButtonTextEquals("Operation-Soothe", "So");
            AssertButtonTextEquals("Operation-Clarify", "Cl");
            AssertButtonTextEquals("Operation-Settle", "Se");
            AssertTextExists(labels, "Restart");
            AssertTextExists(labels, "Pause");
            AssertTextExists(labels, "D1");
            AssertTextExists(labels, "O1");
            AssertObjectDoesNotExist("FocusPreview");
            AssertTextDoesNotExist(labels, "Choose dream");
            AssertTextDoesNotExist(labels, "S1");
            AssertTextDoesNotExist(labels, "Store 1");
            AssertTextDoesNotExist(labels, "Submit Order");
            AssertTextDoesNotExist(labels, "Clarify");
            AssertTextDoesNotExist(labels, "D0");
            AssertTextDoesNotExist(labels, "O0");
            AssertTextDoesNotExist(labels, "S0");
            AssertTextDoesNotExist(labels, "Guide:");
            AssertTextDoesNotExist(labels, "Playing");
            AssertTextDoesNotExist(labels, "Tools / Blocks");
            AssertTextDoesNotExist(labels, "Recall ->");
            AssertTextDoesNotExist(labels, "Taint:");
            AssertTextDoesNotExist(labels, "Mood:");
            AssertTextDoesNotExist(labels, "Clarity:");
            AssertTextDoesNotExist(labels, "Stability:");
        }

        [UnityTest]
        public IEnumerator ReleaseScene_ShowsReleaseUiArtSprites()
        {
            yield return LoadReleaseSceneWithTestStore();

            ReleaseGameController game = Object.FindAnyObjectByType<ReleaseGameController>();
            Assert.That(game, Is.Not.Null);
            Assert.That(game.TryOpenGameplayForTest(), Is.True);
            yield return null;

            Image[] images = Object.FindObjectsByType<Image>(FindObjectsInactive.Exclude);

            Assert.That(game.HasReleaseArtCatalogForTest, Is.True);
            AssertSpriteExists(images, "release-gameplay-background");
            AssertSpriteExists(images, "release-state-");
            AssertSpriteExists(images, "release-operation-wash");
            AssertSpriteExists(images, "release-dream-card-frame");
        }

        [UnityTest]
        public IEnumerator ReleaseScene_SelectionShowsSubmitReadiness()
        {
            yield return LoadReleaseSceneWithTestStore();

            ReleaseGameController game = Object.FindAnyObjectByType<ReleaseGameController>();
            Assert.That(game, Is.Not.Null);
            Assert.That(game.TryOpenGameplayForTest(), Is.True);
            Assert.That(game.TrySelectDreamForTest(0), Is.True);
            Assert.That(game.TrySelectOrderForTest(0), Is.True);
            yield return null;

            Text[] labels = Object.FindObjectsByType<Text>(FindObjectsInactive.Exclude);
            AssertObjectExists("FocusPreview");
            AssertTextExists(labels, "D1");
            AssertTextExists(labels, "O1");
            AssertTextExists(labels, "S1");
            AssertTextExists(labels, "Match");
            AssertTextExists(labels, "O1 selected");
            AssertTextDoesNotExist(labels, "Ready");
            AssertTextDoesNotExist(labels, "Selected D0");
            AssertTextDoesNotExist(labels, "Target O0");
        }

        [UnityTest]
        public IEnumerator ReleaseScene_DragDreamToOrderDispatchesSubmit()
        {
            yield return LoadReleaseSceneWithTestStore();

            ReleaseGameController game = Object.FindAnyObjectByType<ReleaseGameController>();
            Assert.That(game, Is.Not.Null);
            Assert.That(game.TryOpenGameplayForTest(), Is.True);
            int completedBefore = game.CompletedOrders;

            Assert.That(game.TryDragDreamToOrderForTest(0, 0), Is.True);
            yield return null;

            Assert.That(game.CompletedOrders, Is.GreaterThanOrEqualTo(completedBefore));
            AssertTextExists(Object.FindObjectsByType<Text>(FindObjectsInactive.Exclude), "Dream submitted");
        }

        [UnityTest]
        public IEnumerator ReleaseScene_LevelSelectShowsReleaseLevelsAndOpensUnlockedLevel()
        {
            var store = new ReleaseMemoryProgressStore();
            store.Save(new ReleaseProgressState { HighestUnlockedLevelIndex = 2 });

            yield return LoadReleaseSceneWithTestStore(store);

            ReleaseGameController game = Object.FindAnyObjectByType<ReleaseGameController>();
            Assert.That(game, Is.Not.Null);
            Assert.That(game.TryOpenLevelSelectForTest(), Is.True);
            yield return null;

            Text[] labels = Object.FindObjectsByType<Text>(FindObjectsInactive.Exclude);
            AssertTextExists(labels, "Level Select");
            AssertTextExists(labels, "DL-RS-001");
            AssertTextExists(labels, "DL-RS-003");
            AssertTextExists(labels, "Locked");

            Assert.That(game.TrySelectLevelForTest(1), Is.True);
            yield return null;
            Assert.That(game.CurrentLevelId, Is.EqualTo("DL-RS-002"));
            Assert.That(game.CurrentScreenForTest, Is.EqualTo("Gameplay"));
        }

        [UnityTest]
        public IEnumerator ReleaseScene_PauseScreenResumesGameplay()
        {
            yield return LoadReleaseSceneWithTestStore();

            ReleaseGameController game = Object.FindAnyObjectByType<ReleaseGameController>();
            Assert.That(game, Is.Not.Null);
            Assert.That(game.TryOpenGameplayForTest(), Is.True);
            Assert.That(game.TryOpenPauseForTest(), Is.True);
            yield return null;

            Assert.That(game.CurrentScreenForTest, Is.EqualTo("Pause"));
            Text[] pauseLabels = Object.FindObjectsByType<Text>(FindObjectsInactive.Exclude);
            AssertTextExists(pauseLabels, "Pause");
            AssertTextExists(pauseLabels, "Resume");
            AssertTextExists(pauseLabels, "Level Select");
            AssertTextExists(pauseLabels, "Settings");

            Assert.That(game.TryResumeForTest(), Is.True);
            yield return null;
            Assert.That(game.CurrentScreenForTest, Is.EqualTo("Gameplay"));
            AssertTextExists(Object.FindObjectsByType<Text>(FindObjectsInactive.Exclude), "D1");
        }

        [UnityTest]
        public IEnumerator ReleaseScene_ReplaysSolverSolutionThroughController()
        {
            yield return LoadReleaseSceneWithTestStore();

            ReleaseGameController game = Object.FindAnyObjectByType<ReleaseGameController>();
            ReleaseLevelDefinition level = ReleaseLevelPack.CreateDefault().GetLevel(0);
            DynamicSolveResult solve = DynamicRoundSolver.Solve(level.CreateRoundDefinition());

            Assert.That(game, Is.Not.Null);
            Assert.That(solve.Solvable, Is.True);
            Assert.That(game.TryOpenGameplayForTest(), Is.True);
            yield return null;

            for (int i = 0; i < solve.FirstSolutionActions.Count; i++)
            {
                Assert.That(game.TryApplyForTest(solve.FirstSolutionActions[i]), Is.True);
                yield return null;
            }

            Assert.That(game.CurrentStatus, Is.EqualTo(DynamicRoundStatus.Cleared));
            Assert.That(game.CompletedOrders, Is.EqualTo(level.CreateRoundDefinition().TargetCompletedOrders));
            Assert.That(game.CurrentScreenForTest, Is.EqualTo("ClearResult"));
            AssertTextExists(Object.FindObjectsByType<Text>(FindObjectsInactive.Exclude), "Clear Result");
            AssertTextExists(Object.FindObjectsByType<Text>(FindObjectsInactive.Exclude), "Orders complete");

            Assert.That(game.TryResultNextForTest(), Is.True);
            yield return null;
            Assert.That(game.CurrentLevelId, Is.EqualTo("DL-RS-002"));
            Assert.That(game.CurrentScreenForTest, Is.EqualTo("Gameplay"));
            Assert.That(game.CurrentStatus, Is.EqualTo(DynamicRoundStatus.Playing));
        }

        [UnityTest]
        public IEnumerator ReleaseScene_FailResultAllowsRetry()
        {
            FailingActionSequence sequence = FindFailingActionSequence();
            yield return LoadReleaseSceneWithTestStore();

            ReleaseGameController game = Object.FindAnyObjectByType<ReleaseGameController>();
            Assert.That(game, Is.Not.Null);
            Assert.That(game.TryLoadLevelForTest(sequence.LevelIndex), Is.True);
            yield return null;

            for (int i = 0; i < sequence.Actions.Count; i++)
            {
                Assert.That(game.TryApplyForTest(sequence.Actions[i]), Is.True);
                yield return null;
            }

            Assert.That(game.CurrentStatus, Is.EqualTo(DynamicRoundStatus.Failed));
            Assert.That(game.CurrentScreenForTest, Is.EqualTo("FailResult"));
            AssertTextExists(Object.FindObjectsByType<Text>(FindObjectsInactive.Exclude), "Fail Result");
            AssertTextExists(Object.FindObjectsByType<Text>(FindObjectsInactive.Exclude), "Retry");

            Assert.That(game.TryResultReplayForTest(), Is.True);
            yield return null;
            Assert.That(game.CurrentScreenForTest, Is.EqualTo("Gameplay"));
            Assert.That(game.CurrentStatus, Is.EqualTo(DynamicRoundStatus.Playing));
        }

        [UnityTest]
        public IEnumerator ReleaseScene_ShowsItemAndObstacleInformation()
        {
            yield return LoadReleaseSceneWithTestStore();

            ReleaseGameController game = Object.FindAnyObjectByType<ReleaseGameController>();
            Assert.That(game, Is.Not.Null);

            Assert.That(game.TryLoadLevelForTest(8), Is.True);
            yield return null;
            AssertTextExists(Object.FindObjectsByType<Text>(FindObjectsInactive.Exclude), "Swap");
            AssertTextExists(Object.FindObjectsByType<Text>(FindObjectsInactive.Exclude), "Tool");
            AssertTextDoesNotExist(Object.FindObjectsByType<Text>(FindObjectsInactive.Exclude), "Preview Swap");

            Assert.That(game.TryLoadLevelForTest(9), Is.True);
            yield return null;
            AssertTextExists(Object.FindObjectsByType<Text>(FindObjectsInactive.Exclude), "Lock D1");
            AssertTextExists(Object.FindObjectsByType<Text>(FindObjectsInactive.Exclude), "Fault");
            AssertTextDoesNotExist(Object.FindObjectsByType<Text>(FindObjectsInactive.Exclude), "Locked Slot 0");
        }

        [UnityTest]
        public IEnumerator ReleaseScene_ReusesDynamicUiAcrossRefreshes()
        {
            yield return LoadReleaseSceneWithTestStore();

            ReleaseGameController game = Object.FindAnyObjectByType<ReleaseGameController>();
            Assert.That(game, Is.Not.Null);
            int initialCount = game.DynamicUiObjectCountForTest;

            for (int i = 0; i < 3; i++)
            {
                Assert.That(game.TryLoadLevelForTest(0), Is.True);
                yield return null;
            }

            Assert.That(game.DynamicUiObjectCountForTest, Is.EqualTo(initialCount));
        }

        private static IEnumerator LoadReleaseSceneWithTestStore(ReleaseMemoryProgressStore store = null)
        {
            ReleaseGameController.ConfigureTestServices(
                store ?? new ReleaseMemoryProgressStore(),
                new ReleaseFeedbackRecorder());
            yield return SceneManager.LoadSceneAsync("ReleaseGameplaySlice", LoadSceneMode.Single);
            yield return null;
        }

        private static void AssertTextExists(Text[] labels, string expectedText)
        {
            for (int i = 0; i < labels.Length; i++)
            {
                if (labels[i].text.Contains(expectedText))
                {
                    return;
                }
            }

            Assert.Fail($"Expected UI text was not found: {expectedText}");
        }

        private static void AssertTextDoesNotExist(Text[] labels, string unexpectedText)
        {
            for (int i = 0; i < labels.Length; i++)
            {
                if (labels[i].text.Contains(unexpectedText))
                {
                    Assert.Fail($"Unexpected UI text was found: {unexpectedText}");
                }
            }
        }

        private static void AssertObjectExists(string objectName)
        {
            Assert.That(GameObject.Find(objectName), Is.Not.Null, $"Expected active UI object was not found: {objectName}");
        }

        private static void AssertObjectDoesNotExist(string objectName)
        {
            Assert.That(GameObject.Find(objectName), Is.Null, $"Unexpected active UI object was found: {objectName}");
        }

        private static void AssertButtonTextEquals(string objectName, string expectedText)
        {
            GameObject buttonObject = GameObject.Find(objectName);
            Assert.That(buttonObject, Is.Not.Null, $"Expected active button was not found: {objectName}");

            Text text = buttonObject.GetComponentInChildren<Text>(true);
            Assert.That(text, Is.Not.Null, $"Expected button text was not found: {objectName}");
            Assert.That(text.text, Is.EqualTo(expectedText));
        }

        private static void AssertSpriteExists(Image[] images, string expectedSpriteName)
        {
            for (int i = 0; i < images.Length; i++)
            {
                if (images[i].sprite != null && images[i].sprite.name.Contains(expectedSpriteName))
                {
                    return;
                }
            }

            Assert.Fail($"Expected UI sprite was not found: {expectedSpriteName}");
        }

        private static FailingActionSequence FindFailingActionSequence()
        {
            ReleaseLevelPack pack = ReleaseLevelPack.CreateDefault();
            for (int levelIndex = 0; levelIndex < pack.Levels.Count; levelIndex++)
            {
                ReleaseLevelDefinition level = pack.Levels[levelIndex];
                if (level.GuidedActionRules.Length > 0)
                {
                    continue;
                }

                DynamicRoundState state = DynamicRoundInitializer.CreateInitialState(level.CreateRoundDefinition());
                var actions = new List<DynamicPlayerAction>(state.RemainingMoves + 4);
                int guard = Mathf.Max(4, state.RemainingMoves + 8);
                for (int step = 0; step < guard && state.Status == DynamicRoundStatus.Playing; step++)
                {
                    List<DynamicPlayerAction> candidates = EnumerateProbeActions(state);
                    if (candidates.Count == 0)
                    {
                        break;
                    }

                    DynamicPlayerAction action = ChooseFailingProbeAction(candidates);
                    DynamicActionResult result = DynamicRulesEngine.Apply(state, action);
                    if (!result.Success)
                    {
                        break;
                    }

                    actions.Add(action);
                }

                if (state.Status == DynamicRoundStatus.Failed && actions.Count > 0)
                {
                    return new FailingActionSequence(levelIndex, actions);
                }
            }

            Assert.Fail("Could not find a default release level sequence that reaches FailResult.");
            return default;
        }

        private static List<DynamicPlayerAction> EnumerateProbeActions(DynamicRoundState state)
        {
            var actions = new List<DynamicPlayerAction>();
            actions.AddRange(DynamicModifierPipeline.EnumerateExtraActions(state));
            for (int dreamIndex = 0; dreamIndex < state.ActiveDreams.Count; dreamIndex++)
            {
                DynamicDreamSlot dreamSlot = state.ActiveDreams[dreamIndex];
                if (dreamSlot.IsEmpty)
                {
                    continue;
                }

                for (int operationIndex = 0; operationIndex < state.ActionSet.Length; operationIndex++)
                {
                    DynamicOperation operation = state.ActionSet[operationIndex];
                    if (state.IsOperationAllowed(operation)
                        && DynamicRulesEngine.CanApplyOperation(dreamSlot.Dream.Attributes, operation))
                    {
                        AddIfAllowed(state, actions, DynamicPlayerAction.ApplyOperation(dreamSlot.SlotId, operation));
                    }
                }

                for (int storageIndex = 0; storageIndex < state.StorageSlots.Count; storageIndex++)
                {
                    DynamicStorageSlot storageSlot = state.StorageSlots[storageIndex];
                    if (storageSlot.IsEmpty)
                    {
                        AddIfAllowed(state, actions, DynamicPlayerAction.StoreDream(dreamSlot.SlotId, storageSlot.SlotId));
                    }
                }

                for (int orderIndex = 0; orderIndex < state.ActiveOrders.Count; orderIndex++)
                {
                    DynamicOrderSlot orderSlot = state.ActiveOrders[orderIndex];
                    if (!orderSlot.IsEmpty && orderSlot.Order.CanAccept(dreamSlot.Dream.Attributes))
                    {
                        AddIfAllowed(state, actions, DynamicPlayerAction.SubmitDream(dreamSlot.SlotId, orderSlot.SlotId));
                    }
                }
            }

            for (int storageIndex = 0; storageIndex < state.StorageSlots.Count; storageIndex++)
            {
                DynamicStorageSlot storageSlot = state.StorageSlots[storageIndex];
                if (storageSlot.IsEmpty)
                {
                    continue;
                }

                for (int dreamIndex = 0; dreamIndex < state.ActiveDreams.Count; dreamIndex++)
                {
                    DynamicDreamSlot dreamSlot = state.ActiveDreams[dreamIndex];
                    if (dreamSlot.IsEmpty)
                    {
                        AddIfAllowed(state, actions, DynamicPlayerAction.RecallDream(storageSlot.SlotId, dreamSlot.SlotId));
                    }
                }
            }

            return actions;
        }

        private static void AddIfAllowed(
            DynamicRoundState state,
            List<DynamicPlayerAction> actions,
            DynamicPlayerAction action)
        {
            if (DynamicModifierPipeline.CanApplyAction(state, action).Success)
            {
                actions.Add(action);
            }
        }

        private static DynamicPlayerAction ChooseFailingProbeAction(List<DynamicPlayerAction> actions)
        {
            DynamicActionType[] preference =
            {
                DynamicActionType.StoreDream,
                DynamicActionType.RecallDream,
                DynamicActionType.ApplyOperation,
                DynamicActionType.UseItem,
                DynamicActionType.SubmitDream
            };

            for (int preferenceIndex = 0; preferenceIndex < preference.Length; preferenceIndex++)
            {
                for (int actionIndex = 0; actionIndex < actions.Count; actionIndex++)
                {
                    if (actions[actionIndex].Type == preference[preferenceIndex])
                    {
                        return actions[actionIndex];
                    }
                }
            }

            return actions[0];
        }

        private readonly struct FailingActionSequence
        {
            public FailingActionSequence(int levelIndex, List<DynamicPlayerAction> actions)
            {
                LevelIndex = levelIndex;
                Actions = actions;
            }

            public int LevelIndex { get; }
            public List<DynamicPlayerAction> Actions { get; }
        }
    }
}
