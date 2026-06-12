using System.Collections;
using System.Reflection;
using NUnit.Framework;
using Thkim.PocketDodger.Gameplay;
using Thkim.PocketDodger.Infrastructure;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.TestTools;

namespace Thkim.PocketDodger.Tests.PlayMode
{
    public sealed class GameFlowPlayModeTests
    {
        private const string HighScoreKey = "PocketDodger.HighScore";

        [UnitySetUp]
        public IEnumerator LoadScene()
        {
            PlayerPrefs.DeleteKey(HighScoreKey);
            SceneManager.LoadScene("MainGame");
            yield return null;
            yield return null;
        }

        [UnityTearDown]
        public IEnumerator Cleanup()
        {
            PlayerPrefs.DeleteKey(HighScoreKey);
            yield return null;
        }

        [UnityTest]
        public IEnumerator StartButton_BeginsGameplayAndScoreTicks()
        {
            GameController controller = Object.FindAnyObjectByType<GameController>();
            Assert.IsNotNull(controller);
            Assert.AreEqual(GameState.Ready, controller.State);

            ClickButton("StartButton");
            yield return null;

            Assert.AreEqual(GameState.Playing, controller.State);
            Assert.IsFalse(FindObject("StartPanel").activeSelf);

            yield return new WaitForSeconds(0.35f);

            Assert.Greater(controller.CurrentScore, 0);
        }

        [UnityTest]
        public IEnumerator CollisionGameOver_ThenRestartButtonReturnsToPlaying()
        {
            GameController controller = Object.FindAnyObjectByType<GameController>();
            PlayerLaneMover player = Object.FindAnyObjectByType<PlayerLaneMover>();
            ObstaclePool pool = Object.FindAnyObjectByType<ObstaclePool>();
            ObstacleSpawner spawner = Object.FindAnyObjectByType<ObstacleSpawner>();
            LaneLayout laneLayout = Object.FindAnyObjectByType<LaneLayout>();

            Assert.IsNotNull(controller);
            Assert.IsNotNull(player);
            Assert.IsNotNull(pool);
            Assert.IsNotNull(spawner);
            Assert.IsNotNull(laneLayout);

            ClickButton("StartButton");
            yield return null;
            Assert.AreEqual(GameState.Playing, controller.State);

            TriggerPlayerCollision(player, pool, spawner, laneLayout);
            yield return null;

            Assert.AreEqual(GameState.GameOver, controller.State);
            Assert.IsTrue(FindObject("GameOverPanel").activeSelf);

            ClickButton("RestartButton");
            yield return null;

            Assert.AreEqual(GameState.Playing, controller.State);
            Assert.IsFalse(FindObject("GameOverPanel").activeSelf);
            Assert.AreEqual(0, controller.CurrentScore);
        }

        [UnityTest]
        public IEnumerator GameOver_SavesHighScoreAndReloadShowsIt()
        {
            GameController controller = Object.FindAnyObjectByType<GameController>();
            PlayerLaneMover player = Object.FindAnyObjectByType<PlayerLaneMover>();
            ObstaclePool pool = Object.FindAnyObjectByType<ObstaclePool>();
            ObstacleSpawner spawner = Object.FindAnyObjectByType<ObstacleSpawner>();
            LaneLayout laneLayout = Object.FindAnyObjectByType<LaneLayout>();

            Assert.IsNotNull(controller);
            Assert.IsNotNull(player);
            Assert.IsNotNull(pool);
            Assert.IsNotNull(spawner);
            Assert.IsNotNull(laneLayout);

            ClickButton("StartButton");
            yield return new WaitForSeconds(0.35f);

            int finalScore = controller.CurrentScore;
            Assert.Greater(finalScore, 0);

            TriggerPlayerCollision(player, pool, spawner, laneLayout);
            yield return null;

            Assert.AreEqual(GameState.GameOver, controller.State);
            Assert.AreEqual(finalScore, HighScoreStore.Load());

            SceneManager.LoadScene("MainGame");
            yield return null;
            yield return null;

            Assert.AreEqual(finalScore, HighScoreStore.Load());

            Text highScoreText = FindObjectAtPath("Canvas/SafeArea/TopHud/HighScoreText").GetComponent<Text>();
            Assert.IsNotNull(highScoreText);
            Assert.AreEqual($"Best {finalScore}", highScoreText.text);
        }

        private static void ClickButton(string name)
        {
            Button button = FindObject(name).GetComponent<Button>();
            Assert.IsNotNull(button);
            button.onClick.Invoke();
        }

        private static void TriggerPlayerCollision(
            PlayerLaneMover player,
            ObstaclePool pool,
            ObstacleSpawner spawner,
            LaneLayout laneLayout)
        {
            Obstacle obstacle = pool.Rent();
            obstacle.PlayerHit = _ => spawner.PlayerHit?.Invoke();
            obstacle.Activate(
                player.CurrentLane,
                laneLayout.GetLanePosition(player.CurrentLane, laneLayout.PlayerY),
                0.0f,
                laneLayout.DespawnY);

            InvokeTrigger(obstacle, player.GetComponent<Collider2D>());
        }

        private static GameObject FindObject(string name)
        {
            GameObject gameObject = FindSceneObject(name);
            Assert.IsNotNull(gameObject, $"{name} not found.");
            return gameObject;
        }

        private static GameObject FindObjectAtPath(string path)
        {
            Scene targetScene = FindTargetScene();
            GameObject[] rootObjects = targetScene.GetRootGameObjects();
            for (int i = 0; i < rootObjects.Length; i++)
            {
                GameObject rootObject = rootObjects[i];
                if (rootObject.name == path)
                {
                    return rootObject;
                }

                string prefix = rootObject.name + "/";
                if (path.StartsWith(prefix))
                {
                    Transform child = rootObject.transform.Find(path.Substring(prefix.Length));
                    if (child != null)
                    {
                        return child.gameObject;
                    }
                }
            }

            Assert.Fail($"{path} not found.");
            return null;
        }

        private static GameObject FindSceneObject(string name)
        {
            Scene targetScene = FindTargetScene();
            GameObject[] gameObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            for (int i = 0; i < gameObjects.Length; i++)
            {
                GameObject gameObject = gameObjects[i];
                if (gameObject.name == name && gameObject.scene == targetScene)
                {
                    return gameObject;
                }
            }

            return null;
        }

        private static Scene FindTargetScene()
        {
            Scene targetScene = SceneManager.GetSceneByName("MainGame");
            return targetScene.IsValid() && targetScene.isLoaded ? targetScene : SceneManager.GetActiveScene();
        }

        private static void InvokeTrigger(Obstacle obstacle, Collider2D other)
        {
            MethodInfo method = typeof(Obstacle).GetMethod(
                "OnTriggerEnter2D",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.IsNotNull(method);
            method.Invoke(obstacle, new object[] { other });
        }
    }
}
