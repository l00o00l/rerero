using System.IO;
using Thkim.DreamLaundromat.DynamicLab;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Thkim.DreamLaundromat.Editor.DynamicLab
{
    public static class DynamicLabDebugSceneSetup
    {
        private const string ScenePath = "Assets/_Project/Scenes/DynamicLabDebug.unity";

        [MenuItem("DreamLaundromat/Dynamic Lab/Create Debug Scene")]
        public static void CreateDebugScene()
        {
            string directory = Path.GetDirectoryName(ScenePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var gameObject = new GameObject("DynamicLabDebugGame", typeof(DynamicLabDebugGame));
            SceneManager.MoveGameObjectToScene(gameObject, scene);

            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            Debug.Log($"Dynamic Lab debug scene created: {ScenePath}");
        }
    }
}
