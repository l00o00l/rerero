using System.Collections;
using NUnit.Framework;
using Thkim.DreamLaundromat.Rules;
using Thkim.DreamLaundromat.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Thkim.DreamLaundromat.Tests.PlayMode
{
    public sealed class GameFlowPlayModeTests
    {
        [UnityTest]
        public IEnumerator MainScene_LoadsFirstLevel()
        {
            yield return SceneManager.LoadSceneAsync("MainGame", LoadSceneMode.Single);
            yield return null;

            DreamLaundromatGame game = Object.FindAnyObjectByType<DreamLaundromatGame>();

            Assert.That(game, Is.Not.Null);
            Assert.That(game.CurrentLevelId, Is.EqualTo("DL-001"));
            Assert.That(game.CurrentStatus, Is.EqualTo(LevelStatus.Playing));
        }

        [UnityTest]
        public IEnumerator MainScene_ShowsPrototypeEvaluationAffordances()
        {
            yield return SceneManager.LoadSceneAsync("MainGame", LoadSceneMode.Single);
            yield return null;

            Text[] labels = Object.FindObjectsByType<Text>(FindObjectsInactive.Exclude);

            AssertTextExists(labels, "Orders - tap a Submit card after selecting a dream");
            AssertTextExists(labels, "Submit: Clean Dry");
            AssertTextExists(labels, "Dream Queue - select a dream first");
            AssertTextExists(labels, "Storage - temporary baskets for machine output");
        }

        [UnityTest]
        public IEnumerator MainScene_ShowsCustomUiIcons()
        {
            yield return SceneManager.LoadSceneAsync("MainGame", LoadSceneMode.Single);
            yield return null;

            Image[] images = Object.FindObjectsByType<Image>(FindObjectsInactive.Exclude);

            AssertSpriteExists(images, "icon-dream-clean");
            AssertSpriteExists(images, "icon-state-dry");
            AssertSpriteExists(images, "icon-submit-order");
            AssertSpriteExists(images, "icon-storage-basket");
        }

        [UnityTest]
        public IEnumerator MainScene_UsesCompactRowsForLateLevelContent()
        {
            yield return SceneManager.LoadSceneAsync("MainGame", LoadSceneMode.Single);
            yield return null;

            DreamLaundromatGame game = Object.FindAnyObjectByType<DreamLaundromatGame>();

            Assert.That(game, Is.Not.Null);
            Assert.That(game.TryLoadLevelForTest(9), Is.True);
            yield return null;
            Canvas.ForceUpdateCanvases();

            Text[] labels = Object.FindObjectsByType<Text>(FindObjectsInactive.Exclude);
            AssertTextExists(labels, "Submit: Dry Clean 1");
            AssertTextExists(labels, "Submit: Dry Clean 2");
            AssertTextExists(labels, "Submit: Wet Clean");
            AssertTextExists(labels, "Washer");
            AssertTextExists(labels, "Dryer");

            Transform orderCards = FindTransformByName("OrderCards");
            Transform machineCards = FindTransformByName("MachineCards");

            Assert.That(orderCards, Is.Not.Null);
            Assert.That(machineCards, Is.Not.Null);
            Assert.That(orderCards.childCount, Is.EqualTo(3));
            Assert.That(machineCards.childCount, Is.EqualTo(2));
        }

        [UnityTest]
        public IEnumerator MainScene_AppliesScreenSafeAreaAnchors()
        {
            yield return SceneManager.LoadSceneAsync("MainGame", LoadSceneMode.Single);
            yield return null;
            Canvas.ForceUpdateCanvases();

            RectTransform safeArea = FindTransformByName("SafeArea") as RectTransform;

            Assert.That(safeArea, Is.Not.Null);
            if (Screen.width <= 0 || Screen.height <= 0)
            {
                yield break;
            }

            Rect screenSafeArea = Screen.safeArea;
            Vector2 expectedMin = screenSafeArea.position;
            Vector2 expectedMax = screenSafeArea.position + screenSafeArea.size;
            expectedMin.x /= Screen.width;
            expectedMin.y /= Screen.height;
            expectedMax.x /= Screen.width;
            expectedMax.y /= Screen.height;

            Assert.That(safeArea.anchorMin.x, Is.EqualTo(expectedMin.x).Within(0.001f));
            Assert.That(safeArea.anchorMin.y, Is.EqualTo(expectedMin.y).Within(0.001f));
            Assert.That(safeArea.anchorMax.x, Is.EqualTo(expectedMax.x).Within(0.001f));
            Assert.That(safeArea.anchorMax.y, Is.EqualTo(expectedMax.y).Within(0.001f));
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

        private static void AssertSpriteExists(Image[] images, string expectedSpriteName)
        {
            for (int i = 0; i < images.Length; i++)
            {
                if (images[i].sprite != null && images[i].sprite.name == expectedSpriteName)
                {
                    return;
                }
            }

            Assert.Fail($"Expected UI sprite was not found: {expectedSpriteName}");
        }

        private static Transform FindTransformByName(string expectedName)
        {
            RectTransform[] transforms = Object.FindObjectsByType<RectTransform>(FindObjectsInactive.Exclude);
            for (int i = 0; i < transforms.Length; i++)
            {
                if (transforms[i].name == expectedName)
                {
                    return transforms[i];
                }
            }

            return null;
        }
    }
}
