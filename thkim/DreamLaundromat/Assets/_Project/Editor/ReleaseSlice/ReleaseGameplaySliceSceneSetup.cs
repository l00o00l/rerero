using System.IO;
using Thkim.DreamLaundromat.Gameplay.ReleaseSlice;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Thkim.DreamLaundromat.Editor.ReleaseSlice
{
    public static class ReleaseGameplaySliceSceneSetup
    {
        public const string ScenePath = "Assets/_Project/Scenes/ReleaseGameplaySlice.unity";

        [MenuItem("DreamLaundromat/Release Slice/Create Gameplay Scene")]
        public static void CreateGameplayScene()
        {
            string directory = Path.GetDirectoryName(ScenePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = "ReleaseGameplaySlice";
            ReleaseUiArtCatalog artCatalog = ReleaseUiArtGenerator.GenerateReleaseUiArtAssets();

            GameObject cameraObject = new GameObject("Main Camera", typeof(Camera));
            cameraObject.tag = "MainCamera";
            SceneManager.MoveGameObjectToScene(cameraObject, scene);
            Camera camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.075f, 0.08f, 0.095f, 1f);
            camera.orthographic = true;
            camera.orthographicSize = 5f;

            GameObject gameObject = new GameObject("ReleaseGameController", typeof(ReleaseGameController));
            ReleaseGameController controller = gameObject.GetComponent<ReleaseGameController>();
            controller.ConfigureArtCatalog(artCatalog);
            SceneManager.MoveGameObjectToScene(gameObject, scene);

            EditorSceneManager.SaveScene(scene, ScenePath);
            EnsureSceneInBuildSettings(ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Release gameplay slice scene created: {ScenePath}");
        }

        public static void RunFromCommandLine()
        {
            CreateGameplayScene();
        }

        private static void EnsureSceneInBuildSettings(string scenePath)
        {
            var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>();
            scenes.Add(new EditorBuildSettingsScene(scenePath, true));

            for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
            {
                EditorBuildSettingsScene scene = EditorBuildSettings.scenes[i];
                if (scene.path == scenePath)
                {
                    continue;
                }

                scenes.Add(scene);
            }

            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
