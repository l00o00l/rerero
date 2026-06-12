using System.Collections.Generic;
using System.IO;
using Thkim.PocketDodger.Gameplay;
using Thkim.PocketDodger.Input;
using Thkim.PocketDodger.UI;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Thkim.PocketDodger.Editor.Setup
{
    public static class PocketDodgerProjectSetup
    {
        private const string MainScenePath = "Assets/_Project/Scenes/MainGame.unity";
        private const string DifficultySettingsPath = "Assets/_Project/ScriptableObjects/Difficulty/DefaultDifficulty.asset";
        private const string LaneMaterialPath = "Assets/_Project/Materials/LaneGuide.mat";
        private const string PlayerMaterialPath = "Assets/_Project/Materials/Player.mat";
        private const string PlayerTrailMaterialPath = "Assets/_Project/Materials/PlayerTrail.mat";
        private const string ObstacleMaterialPath = "Assets/_Project/Materials/Obstacle.mat";
        private const string PlayerPrefabPath = "Assets/_Project/Prefabs/Gameplay/Player.prefab";
        private const string ObstaclePrefabPath = "Assets/_Project/Prefabs/Gameplay/Obstacle.prefab";
        private const string SharedPlayerShipSpritePath = "Packages/com.rerero.shared-assets/Runtime/Art/Kenney/SpaceShooterExtension/Sprites/Ships/spaceShips_001.png";
        private const string SharedMeteorSpritePath = "Packages/com.rerero.shared-assets/Runtime/Art/Kenney/SpaceShooterExtension/Sprites/Meteors/spaceMeteors_001.png";

        [MenuItem("PocketDodger/Setup/Apply Project Baseline")]
        public static void ApplyProjectBaseline()
        {
            CreateProjectFolders();
            CreateMainGameScene();
            ConfigureProjectSettings();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void VerifyProjectBaseline()
        {
            List<string> failures = new List<string>();

            RequireFolder(failures, "Assets/_Project");
            RequireFolder(failures, "Assets/_Project/Art/Sprites");
            RequireFolder(failures, "Assets/_Project/Audio");
            RequireFolder(failures, "Assets/_Project/Editor/Setup");
            RequireFolder(failures, "Assets/_Project/Materials");
            RequireFolder(failures, "Assets/_Project/Prefabs/Gameplay");
            RequireFolder(failures, "Assets/_Project/Prefabs/UI");
            RequireFolder(failures, "Assets/_Project/Scenes");
            RequireFolder(failures, "Assets/_Project/ScriptableObjects/Difficulty");
            RequireFolder(failures, "Assets/_Project/Scripts/Gameplay");
            RequireFolder(failures, "Assets/_Project/Scripts/Input");
            RequireFolder(failures, "Assets/_Project/Scripts/Infrastructure");
            RequireFolder(failures, "Assets/_Project/Scripts/UI");
            RequireFolder(failures, "Assets/_Project/Settings");
            RequireFolder(failures, "Assets/_Project/Tests/EditMode");
            RequireFolder(failures, "Assets/_Project/Tests/PlayMode");
            RequireFolder(failures, "Assets/_Project/UI");

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(MainScenePath) == null)
            {
                failures.Add($"{MainScenePath} is missing.");
            }

            if (EditorSettings.serializationMode != SerializationMode.ForceText)
            {
                failures.Add("Asset serialization mode is not Force Text.");
            }

            if (PlayerSettings.companyName != "Rerero")
            {
                failures.Add("Company name is not Rerero.");
            }

            if (PlayerSettings.productName != "PocketDodger")
            {
                failures.Add("Product name is not PocketDodger.");
            }

            if (PlayerSettings.defaultInterfaceOrientation != UIOrientation.Portrait)
            {
                failures.Add($"Default orientation is {PlayerSettings.defaultInterfaceOrientation}, not Portrait.");
            }

            if (PlayerSettings.GetApplicationIdentifier(NamedBuildTarget.Android) != "com.rerero.pocketdodger")
            {
                failures.Add("Android application identifier is not com.rerero.pocketdodger.");
            }

            if (PlayerSettings.GetUseDefaultGraphicsAPIs(BuildTarget.Android))
            {
                failures.Add("Android graphics APIs are using Unity defaults, not explicit OpenGLES3.");
            }
            else
            {
                GraphicsDeviceType[] graphicsApis = PlayerSettings.GetGraphicsAPIs(BuildTarget.Android);
                if (graphicsApis.Length != 1 || graphicsApis[0] != GraphicsDeviceType.OpenGLES3)
                {
                    failures.Add("Android graphics APIs are not OpenGLES3-only.");
                }
            }

            bool mainSceneRegistered = false;
            foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            {
                if (scene.enabled && scene.path == MainScenePath)
                {
                    mainSceneRegistered = true;
                    break;
                }
            }

            if (!mainSceneRegistered)
            {
                failures.Add($"{MainScenePath} is not registered as an enabled build scene.");
            }

            if (failures.Count > 0)
            {
                foreach (string failure in failures)
                {
                    Debug.LogError(failure);
                }

                EditorApplication.Exit(1);
                return;
            }

            Debug.Log("PocketDodger project baseline verification passed.");
            EditorApplication.Exit(0);
        }

        public static void VerifyPlayableScene()
        {
            List<string> failures = new List<string>();

            if (AssetDatabase.LoadAssetAtPath<DifficultySettings>(DifficultySettingsPath) == null)
            {
                failures.Add($"{DifficultySettingsPath} is missing.");
            }

            if (AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath) == null)
            {
                failures.Add($"{PlayerPrefabPath} is missing.");
            }

            if (AssetDatabase.LoadAssetAtPath<GameObject>(ObstaclePrefabPath) == null)
            {
                failures.Add($"{ObstaclePrefabPath} is missing.");
            }

            if (AssetDatabase.LoadAssetAtPath<Sprite>(SharedPlayerShipSpritePath) == null)
            {
                failures.Add($"{SharedPlayerShipSpritePath} is missing or is not imported as a Sprite.");
            }

            if (AssetDatabase.LoadAssetAtPath<Sprite>(SharedMeteorSpritePath) == null)
            {
                failures.Add($"{SharedMeteorSpritePath} is missing or is not imported as a Sprite.");
            }

            Scene scene = EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);

            Transform mainCamera = RequireScenePath(failures, scene, "Main Camera");
            RequireComponent<Camera>(failures, mainCamera, "Main Camera");
            RequireComponent<AudioListener>(failures, mainCamera, "Main Camera");

            Transform gameController = RequireScenePath(failures, scene, "GameRoot/GameController");
            RequireComponent<GameController>(failures, gameController, "GameRoot/GameController");
            RequireComponent<PlayerInputRouter>(failures, gameController, "GameRoot/GameController");
            RequireComponent<AudioSource>(failures, gameController, "GameRoot/GameController");
            RequireComponent<SimpleSfxPlayer>(failures, gameController, "GameRoot/GameController");

            Transform laneRoot = RequireScenePath(failures, scene, "GameRoot/LaneRoot");
            RequireComponent<LaneLayout>(failures, laneRoot, "GameRoot/LaneRoot");
            RequireScenePath(failures, scene, "GameRoot/LaneRoot/Lane_Left/Guide");
            RequireScenePath(failures, scene, "GameRoot/LaneRoot/Lane_Center/Guide");
            RequireScenePath(failures, scene, "GameRoot/LaneRoot/Lane_Right/Guide");

            Transform player = RequireScenePath(failures, scene, "GameRoot/Player");
            RequireComponent<PlayerLaneMover>(failures, player, "GameRoot/Player");
            RequireComponent<Rigidbody2D>(failures, player, "GameRoot/Player");
            RequireComponent<BoxCollider2D>(failures, player, "GameRoot/Player");
            RequireComponent<TrailRenderer>(failures, player, "GameRoot/Player");
            Transform playerVisual = RequireScenePath(failures, scene, "GameRoot/Player/Visual");
            RequireComponent<SpriteRenderer>(failures, playerVisual, "GameRoot/Player/Visual");

            Transform obstaclePool = RequireScenePath(failures, scene, "GameRoot/ObstaclePool");
            RequireComponent<ObstaclePool>(failures, obstaclePool, "GameRoot/ObstaclePool");
            Transform obstacleTemplate = RequireScenePath(failures, scene, "GameRoot/ObstaclePool/ObstacleTemplate");
            RequireComponent<Obstacle>(failures, obstacleTemplate, "GameRoot/ObstaclePool/ObstacleTemplate");
            Transform obstacleVisual = RequireScenePath(failures, scene, "GameRoot/ObstaclePool/ObstacleTemplate/Visual");
            RequireComponent<SpriteRenderer>(failures, obstacleVisual, "GameRoot/ObstaclePool/ObstacleTemplate/Visual");

            Transform spawnRoot = RequireScenePath(failures, scene, "GameRoot/SpawnRoot");
            RequireComponent<ObstacleSpawner>(failures, spawnRoot, "GameRoot/SpawnRoot");

            RequireScenePath(failures, scene, "Canvas/SafeArea/TopHud/ScoreText");
            RequireScenePath(failures, scene, "Canvas/SafeArea/TopHud/HighScoreText");
            Transform topHud = RequireScenePath(failures, scene, "Canvas/SafeArea/TopHud");
            RequireComponent<GameHudPresenter>(failures, topHud, "Canvas/SafeArea/TopHud");

            Transform safeArea = RequireScenePath(failures, scene, "Canvas/SafeArea");
            RequireComponent<SafeAreaFitter>(failures, safeArea, "Canvas/SafeArea");
            Transform startPanel = RequireScenePath(failures, scene, "Canvas/SafeArea/CenterOverlay/StartPanel");
            RequireComponent<StartPanelPresenter>(failures, startPanel, "Canvas/SafeArea/CenterOverlay/StartPanel");
            Transform gameOverPanel = RequireScenePath(failures, scene, "Canvas/SafeArea/CenterOverlay/GameOverPanel");
            RequireComponent<GameOverPresenter>(failures, gameOverPanel, "Canvas/SafeArea/CenterOverlay/GameOverPanel");

            Transform eventSystem = RequireScenePath(failures, scene, "EventSystem");
            RequireComponent<EventSystem>(failures, eventSystem, "EventSystem");
            RequireComponent<InputSystemUIInputModule>(failures, eventSystem, "EventSystem");

            int missingScriptCount = 0;
            foreach (GameObject rootObject in scene.GetRootGameObjects())
            {
                missingScriptCount += CountMissingScripts(rootObject);
            }

            if (missingScriptCount > 0)
            {
                failures.Add($"Scene has {missingScriptCount} missing script reference(s).");
            }

            if (failures.Count > 0)
            {
                foreach (string failure in failures)
                {
                    Debug.LogError(failure);
                }

                EditorApplication.Exit(1);
                return;
            }

            Debug.Log("PocketDodger playable scene verification passed.");
            EditorApplication.Exit(0);
        }

        private static void CreateProjectFolders()
        {
            EnsureFolder("Assets", "_Project");
            EnsureFolder("Assets/_Project", "Art");
            EnsureFolder("Assets/_Project/Art", "Sprites");
            EnsureFolder("Assets/_Project", "Audio");
            EnsureFolder("Assets/_Project", "Editor");
            EnsureFolder("Assets/_Project/Editor", "Setup");
            EnsureFolder("Assets/_Project", "Materials");
            EnsureFolder("Assets/_Project", "Prefabs");
            EnsureFolder("Assets/_Project/Prefabs", "Gameplay");
            EnsureFolder("Assets/_Project/Prefabs", "UI");
            EnsureFolder("Assets/_Project", "Scenes");
            EnsureFolder("Assets/_Project", "ScriptableObjects");
            EnsureFolder("Assets/_Project/ScriptableObjects", "Difficulty");
            EnsureFolder("Assets/_Project", "Scripts");
            EnsureFolder("Assets/_Project/Scripts", "Gameplay");
            EnsureFolder("Assets/_Project/Scripts", "Input");
            EnsureFolder("Assets/_Project/Scripts", "Infrastructure");
            EnsureFolder("Assets/_Project/Scripts", "UI");
            EnsureFolder("Assets/_Project", "Settings");
            EnsureFolder("Assets/_Project", "Tests");
            EnsureFolder("Assets/_Project/Tests", "EditMode");
            EnsureFolder("Assets/_Project/Tests", "PlayMode");
            EnsureFolder("Assets/_Project", "UI");
        }

        private static void ConfigureProjectSettings()
        {
            EditorSettings.serializationMode = SerializationMode.ForceText;
#pragma warning disable CS0618
            EditorSettings.externalVersionControl = "Visible Meta Files";
#pragma warning restore CS0618

            PlayerSettings.companyName = "Rerero";
            PlayerSettings.productName = "PocketDodger";
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.rerero.pocketdodger");
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.OpenGLES3 });

            AssetDatabase.ImportAsset(MainScenePath, ImportAssetOptions.ForceUpdate);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(MainScenePath, true)
            };
        }

        private static void CreateMainGameScene()
        {
            DifficultySettings difficultySettings = EnsureDifficultySettings();
            Material laneMaterial = EnsureMaterial(LaneMaterialPath, new Color(0.15f, 0.19f, 0.25f));
            Material playerMaterial = EnsureMaterial(PlayerMaterialPath, new Color(0.10f, 0.95f, 0.78f));
            Material playerTrailMaterial = EnsureTrailMaterial(PlayerTrailMaterialPath);
            Material obstacleMaterial = EnsureMaterial(ObstacleMaterialPath, new Color(1.0f, 0.18f, 0.30f));
            Sprite playerSprite = EnsureSpriteAsset(SharedPlayerShipSpritePath, 190.0f);
            Sprite obstacleSprite = EnsureSpriteAsset(SharedMeteorSpritePath, 390.0f);
            EnsurePlayerPrefab(playerMaterial, playerTrailMaterial, playerSprite);
            EnsureObstaclePrefab(obstacleMaterial, obstacleSprite);

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "MainGame";

            GameObject cameraObject = new GameObject("Main Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
            camera.orthographic = true;
            camera.orthographicSize = 6.0f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.035f, 0.045f, 0.065f);
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0.0f, 0.0f, -10.0f);

            GameObject gameRoot = new GameObject("GameRoot");
            GameObject gameControllerObject = CreateChild(gameRoot.transform, "GameController");
            GameController gameController = gameControllerObject.AddComponent<GameController>();
            PlayerInputRouter inputRouter = gameControllerObject.AddComponent<PlayerInputRouter>();
            AudioSource audioSource = gameControllerObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            SimpleSfxPlayer sfxPlayer = gameControllerObject.AddComponent<SimpleSfxPlayer>();
            sfxPlayer.Configure(audioSource, 0.25f);

            GameObject laneRoot = CreateChild(gameRoot.transform, "LaneRoot");
            LaneLayout laneLayout = laneRoot.AddComponent<LaneLayout>();
            laneLayout.Configure(2.4f, -3.6f, 5.5f, -6.2f);
            CreateLane(laneRoot.transform, "Lane_Left", -3.6f, laneMaterial);
            CreateLane(laneRoot.transform, "Lane_Center", -1.2f, laneMaterial);
            CreateLane(laneRoot.transform, "Lane_Right", 1.2f, laneMaterial);
            CreateLane(laneRoot.transform, "Lane_OuterRight", 3.6f, laneMaterial);

            GameObject playerObject = CreatePlayerObject(gameRoot.transform, "Player", playerMaterial, playerTrailMaterial, playerSprite);
            PlayerLaneMover player = playerObject.GetComponent<PlayerLaneMover>();
            player.Configure(laneLayout, LaneIndex.Center, 0.16f);
            player.ResetToStartLane();

            GameObject obstaclePoolObject = CreateChild(gameRoot.transform, "ObstaclePool");
            ObstaclePool obstaclePool = obstaclePoolObject.AddComponent<ObstaclePool>();
            GameObject obstacleTemplateObject = CreateObstacleObject(obstaclePoolObject.transform, "ObstacleTemplate", obstacleMaterial, obstacleSprite);
            obstacleTemplateObject.SetActive(false);
            Obstacle obstacleTemplate = obstacleTemplateObject.GetComponent<Obstacle>();
            obstaclePool.Configure(obstacleTemplate, obstaclePoolObject.transform, 8);

            GameObject spawnRoot = CreateChild(gameRoot.transform, "SpawnRoot");
            spawnRoot.transform.position = new Vector3(0.0f, 5.5f, 0.0f);
            ObstacleSpawner obstacleSpawner = spawnRoot.AddComponent<ObstacleSpawner>();
            obstacleSpawner.Configure(laneLayout, obstaclePool, difficultySettings);

            GameObject canvasObject = new GameObject("Canvas", typeof(RectTransform));
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler canvasScaler = canvasObject.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1080.0f, 1920.0f);
            canvasScaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            GameObject safeArea = CreateUiChild(canvasObject.transform, "SafeArea");
            safeArea.AddComponent<SafeAreaFitter>();
            RectTransform safeAreaRect = safeArea.GetComponent<RectTransform>();
            safeAreaRect.anchorMin = Vector2.zero;
            safeAreaRect.anchorMax = Vector2.one;
            safeAreaRect.offsetMin = Vector2.zero;
            safeAreaRect.offsetMax = Vector2.zero;

            GameObject topHud = CreateUiChild(safeArea.transform, "TopHud");
            RectTransform topHudRect = topHud.GetComponent<RectTransform>();
            topHudRect.anchorMin = new Vector2(0.0f, 1.0f);
            topHudRect.anchorMax = new Vector2(1.0f, 1.0f);
            topHudRect.pivot = new Vector2(0.5f, 1.0f);
            topHudRect.sizeDelta = new Vector2(0.0f, 120.0f);
            topHudRect.anchoredPosition = Vector2.zero;

            Text scoreText = CreateText(topHud.transform, "ScoreText", "Score 0", 42, TextAnchor.MiddleLeft);
            SetRect(scoreText.rectTransform, new Vector2(0.0f, 0.0f), new Vector2(0.5f, 1.0f), new Vector2(32.0f, 0.0f), new Vector2(-16.0f, 0.0f));

            Text highScoreText = CreateText(topHud.transform, "HighScoreText", "Best 0", 42, TextAnchor.MiddleRight);
            SetRect(highScoreText.rectTransform, new Vector2(0.5f, 0.0f), Vector2.one, new Vector2(16.0f, 0.0f), new Vector2(-32.0f, 0.0f));

            GameObject pauseButton = CreateUiChild(topHud.transform, "PauseButton");
            pauseButton.SetActive(false);
            GameHudPresenter hudPresenter = topHud.AddComponent<GameHudPresenter>();
            hudPresenter.Configure(scoreText, highScoreText);

            GameObject centerOverlay = CreateUiChild(safeArea.transform, "CenterOverlay");
            RectTransform centerOverlayRect = centerOverlay.GetComponent<RectTransform>();
            centerOverlayRect.anchorMin = Vector2.zero;
            centerOverlayRect.anchorMax = Vector2.one;
            centerOverlayRect.offsetMin = Vector2.zero;
            centerOverlayRect.offsetMax = Vector2.zero;

            GameObject startPanel = CreatePanel(centerOverlay.transform, "StartPanel", new Color(0.05f, 0.06f, 0.08f, 0.82f));
            Text titleText = CreateText(startPanel.transform, "TitleText", "PocketDodger", 72, TextAnchor.MiddleCenter);
            SetRect(titleText.rectTransform, new Vector2(0.0f, 0.55f), new Vector2(1.0f, 0.75f), new Vector2(48.0f, 0.0f), new Vector2(-48.0f, 0.0f));
            Text hintText = CreateText(startPanel.transform, "HintText", "Tap left or right to dodge", 34, TextAnchor.MiddleCenter);
            SetRect(hintText.rectTransform, new Vector2(0.0f, 0.44f), new Vector2(1.0f, 0.54f), new Vector2(48.0f, 0.0f), new Vector2(-48.0f, 0.0f));
            Button startButton = CreateButton(startPanel.transform, "StartButton", "Start");
            SetRect(startButton.GetComponent<RectTransform>(), new Vector2(0.28f, 0.30f), new Vector2(0.72f, 0.39f), Vector2.zero, Vector2.zero);
            StartPanelPresenter startPanelPresenter = startPanel.AddComponent<StartPanelPresenter>();
            startPanelPresenter.Configure(startPanel, startButton);

            GameObject gameOverPanel = CreatePanel(centerOverlay.transform, "GameOverPanel", new Color(0.05f, 0.06f, 0.08f, 0.88f));
            Text gameOverTitleText = CreateText(gameOverPanel.transform, "TitleText", "Game Over", 72, TextAnchor.MiddleCenter);
            SetRect(gameOverTitleText.rectTransform, new Vector2(0.0f, 0.58f), new Vector2(1.0f, 0.75f), new Vector2(48.0f, 0.0f), new Vector2(-48.0f, 0.0f));
            Text finalScoreText = CreateText(gameOverPanel.transform, "FinalScoreText", "Score 0", 44, TextAnchor.MiddleCenter);
            SetRect(finalScoreText.rectTransform, new Vector2(0.0f, 0.47f), new Vector2(1.0f, 0.56f), new Vector2(48.0f, 0.0f), new Vector2(-48.0f, 0.0f));
            Text gameOverHighScoreText = CreateText(gameOverPanel.transform, "HighScoreText", "Best 0", 38, TextAnchor.MiddleCenter);
            SetRect(gameOverHighScoreText.rectTransform, new Vector2(0.0f, 0.39f), new Vector2(1.0f, 0.47f), new Vector2(48.0f, 0.0f), new Vector2(-48.0f, 0.0f));
            Button restartButton = CreateButton(gameOverPanel.transform, "RestartButton", "Restart");
            SetRect(restartButton.GetComponent<RectTransform>(), new Vector2(0.28f, 0.26f), new Vector2(0.72f, 0.35f), Vector2.zero, Vector2.zero);
            GameOverPresenter gameOverPresenter = gameOverPanel.AddComponent<GameOverPresenter>();
            gameOverPresenter.Configure(gameOverPanel, finalScoreText, gameOverHighScoreText, restartButton);
            gameOverPanel.SetActive(false);

            GameObject eventSystemObject = new GameObject("EventSystem");
            eventSystemObject.AddComponent<EventSystem>();
            eventSystemObject.AddComponent<InputSystemUIInputModule>();

            gameController.Configure(
                player,
                obstaclePool,
                obstacleSpawner,
                inputRouter,
                difficultySettings,
                hudPresenter,
                startPanelPresenter,
                gameOverPresenter,
                sfxPlayer,
                cameraObject.transform);

            Selection.activeGameObject = gameControllerObject;
            EditorSceneManager.MarkSceneDirty(scene);
            Directory.CreateDirectory(Path.GetDirectoryName(MainScenePath));
            EditorSceneManager.SaveScene(scene, MainScenePath);
        }

        private static void CreateLane(Transform parent, string name, float x, Material laneMaterial)
        {
            GameObject lane = CreateChild(parent, name);
            lane.transform.position = new Vector3(x, 0.0f, 0.0f);
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "Guide";
            visual.transform.SetParent(lane.transform, false);
            visual.transform.localPosition = new Vector3(0.0f, 0.0f, 1.0f);
            visual.transform.localScale = new Vector3(0.08f, 11.2f, 0.05f);
            Object.DestroyImmediate(visual.GetComponent<Collider>());
            MeshRenderer renderer = visual.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = laneMaterial;
        }

        private static GameObject CreateChild(Transform parent, string name)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(parent, false);
            return child;
        }

        private static GameObject CreateUiChild(Transform parent, string name)
        {
            GameObject child = new GameObject(name, typeof(RectTransform));
            child.transform.SetParent(parent, false);
            return child;
        }

        private static DifficultySettings EnsureDifficultySettings()
        {
            DifficultySettings settings = AssetDatabase.LoadAssetAtPath<DifficultySettings>(DifficultySettingsPath);

            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<DifficultySettings>();
                AssetDatabase.CreateAsset(settings, DifficultySettingsPath);
            }

            settings.ConfigureForSetup(4.0f, 10.0f, 1.1f, 0.45f, 60.0f, 10, 0);
            EditorUtility.SetDirty(settings);
            return settings;
        }

        private static Material EnsureMaterial(string path, Color color)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (material == null)
            {
                material = new Material(FindUnlitShader());
                AssetDatabase.CreateAsset(material, path);
            }

            material.shader = FindUnlitShader();
            SetMaterialColor(material, color);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Material EnsureTrailMaterial(string path)
        {
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            Shader shader = Shader.Find("Sprites/Default");

            if (shader == null)
            {
                shader = FindUnlitShader();
            }

            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }

            material.shader = shader;
            SetMaterialColor(material, new Color(0.10f, 0.95f, 0.78f, 0.65f));
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Shader FindUnlitShader()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");

            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }

            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            return shader;
        }

        private static void SetMaterialColor(Material material, Color color)
        {
            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
                return;
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }
        }

        private static Sprite EnsureSpriteAsset(string path, float pixelsPerUnit)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;

            if (importer == null)
            {
                Debug.LogWarning($"{path} is not available. Falling back to generated primitive visuals.");
                return null;
            }

            bool changed = false;

            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                changed = true;
            }

            if (importer.spriteImportMode != SpriteImportMode.Single)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                changed = true;
            }

            if (!Mathf.Approximately(importer.spritePixelsPerUnit, pixelsPerUnit))
            {
                importer.spritePixelsPerUnit = pixelsPerUnit;
                changed = true;
            }

            if (!importer.alphaIsTransparency)
            {
                importer.alphaIsTransparency = true;
                changed = true;
            }

            if (importer.mipmapEnabled)
            {
                importer.mipmapEnabled = false;
                changed = true;
            }

            if (importer.filterMode != FilterMode.Bilinear)
            {
                importer.filterMode = FilterMode.Bilinear;
                changed = true;
            }

            if (changed)
            {
                importer.SaveAndReimport();
            }

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void EnsurePlayerPrefab(Material material, Material trailMaterial, Sprite sprite)
        {
            GameObject root = CreatePlayerObject(null, "Player", material, trailMaterial, sprite);
            PrefabUtility.SaveAsPrefabAsset(root, PlayerPrefabPath);
            Object.DestroyImmediate(root);
            AssetDatabase.ImportAsset(PlayerPrefabPath, ImportAssetOptions.ForceUpdate);
        }

        private static void EnsureObstaclePrefab(Material material, Sprite sprite)
        {
            GameObject root = CreateObstacleObject(null, "Obstacle", material, sprite);
            PrefabUtility.SaveAsPrefabAsset(root, ObstaclePrefabPath);
            Object.DestroyImmediate(root);
            AssetDatabase.ImportAsset(ObstaclePrefabPath, ImportAssetOptions.ForceUpdate);
        }

        private static GameObject CreatePlayerObject(Transform parent, string name, Material material, Material trailMaterial, Sprite sprite)
        {
            GameObject root = new GameObject(name);
            root.tag = "Player";

            if (parent != null)
            {
                root.transform.SetParent(parent, false);
            }

            PlayerLaneMover mover = root.AddComponent<PlayerLaneMover>();
            Rigidbody2D body = root.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0.0f;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;

            BoxCollider2D collider = root.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(0.9f, 0.9f);

            TrailRenderer trail = root.AddComponent<TrailRenderer>();
            trail.time = 0.18f;
            trail.startWidth = 0.42f;
            trail.endWidth = 0.02f;
            trail.minVertexDistance = 0.02f;
            trail.numCornerVertices = 4;
            trail.numCapVertices = 4;
            trail.startColor = new Color(0.10f, 0.95f, 0.78f, 0.60f);
            trail.endColor = new Color(0.10f, 0.95f, 0.78f, 0.0f);
            trail.sharedMaterial = trailMaterial;
            trail.sortingOrder = -1;

            GameObject visual = sprite == null
                ? CreateCubeVisual(root.transform, "Visual", new Vector3(0.9f, 0.9f, 0.2f), material)
                : CreateSpriteVisual(root.transform, "Visual", sprite, new Vector3(0.94f, 0.94f, 1.0f), 2);
            visual.transform.localPosition = Vector3.zero;
            mover.Configure(null, LaneIndex.Center, 0.16f);
            mover.ConfigureVisual(visual.transform);
            return root;
        }

        private static GameObject CreateObstacleObject(Transform parent, string name, Material material, Sprite sprite)
        {
            GameObject root = new GameObject(name);

            if (parent != null)
            {
                root.transform.SetParent(parent, false);
            }

            root.AddComponent<Obstacle>();
            Rigidbody2D body = root.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0.0f;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;

            BoxCollider2D collider = root.AddComponent<BoxCollider2D>();
            collider.isTrigger = true;
            collider.size = new Vector2(1.0f, 1.0f);

            GameObject visual = sprite == null
                ? CreateCubeVisual(root.transform, "Visual", new Vector3(1.0f, 1.0f, 0.2f), material)
                : CreateSpriteVisual(root.transform, "Visual", sprite, new Vector3(0.96f, 0.96f, 1.0f), 1);
            visual.transform.localPosition = Vector3.zero;
            root.GetComponent<Obstacle>().ConfigureVisual(visual.transform);
            return root;
        }

        private static GameObject CreateCubeVisual(Transform parent, string name, Vector3 scale, Material material)
        {
            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = name;
            visual.transform.SetParent(parent, false);
            visual.transform.localScale = scale;
            Object.DestroyImmediate(visual.GetComponent<Collider>());
            MeshRenderer renderer = visual.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            return visual;
        }

        private static GameObject CreateSpriteVisual(Transform parent, string name, Sprite sprite, Vector3 scale, int sortingOrder)
        {
            GameObject visual = new GameObject(name);
            visual.transform.SetParent(parent, false);
            visual.transform.localScale = scale;

            SpriteRenderer renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
            renderer.color = Color.white;
            return visual;
        }

        private static GameObject CreatePanel(Transform parent, string name, Color color)
        {
            GameObject panel = CreateUiChild(parent, name);
            Image image = panel.AddComponent<Image>();
            image.color = color;
            SetRect(panel.GetComponent<RectTransform>(), Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            return panel;
        }

        private static Text CreateText(Transform parent, string name, string value, int fontSize, TextAnchor alignment)
        {
            GameObject textObject = CreateUiChild(parent, name);
            Text text = textObject.AddComponent<Text>();
            text.text = value;
            text.font = GetDefaultFont();
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }

        private static Button CreateButton(Transform parent, string name, string label)
        {
            GameObject buttonObject = CreateUiChild(parent, name);
            Image image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.20f, 0.86f, 0.72f, 1.0f);

            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;

            Text labelText = CreateText(buttonObject.transform, "Label", label, 40, TextAnchor.MiddleCenter);
            labelText.color = new Color(0.04f, 0.05f, 0.06f, 1.0f);
            SetRect(labelText.rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            return button;
        }

        private static Font GetDefaultFont()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            if (font == null)
            {
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }

            return font;
        }

        private static void SetRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private static void EnsureFolder(string parent, string child)
        {
            string path = $"{parent}/{child}";
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }

        private static void RequireFolder(List<string> failures, string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                failures.Add($"{path} is missing.");
            }
        }

        private static Transform RequireScenePath(List<string> failures, Scene scene, string path)
        {
            Transform transform = FindScenePath(scene, path);

            if (transform == null)
            {
                failures.Add($"{path} is missing from {MainScenePath}.");
            }

            return transform;
        }

        private static Transform FindScenePath(Scene scene, string path)
        {
            string[] parts = path.Split('/');
            GameObject[] roots = scene.GetRootGameObjects();
            Transform current = null;

            for (int i = 0; i < roots.Length; i++)
            {
                if (roots[i].name == parts[0])
                {
                    current = roots[i].transform;
                    break;
                }
            }

            if (current == null)
            {
                return null;
            }

            for (int i = 1; i < parts.Length; i++)
            {
                current = current.Find(parts[i]);

                if (current == null)
                {
                    return null;
                }
            }

            return current;
        }

        private static void RequireComponent<T>(List<string> failures, Transform transform, string path)
            where T : Component
        {
            if (transform == null)
            {
                return;
            }

            if (transform.GetComponent<T>() == null)
            {
                failures.Add($"{path} is missing {typeof(T).Name}.");
            }
        }

        private static int CountMissingScripts(GameObject gameObject)
        {
            int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(gameObject);

            foreach (Transform child in gameObject.transform)
            {
                count += CountMissingScripts(child.gameObject);
            }

            return count;
        }
    }
}
