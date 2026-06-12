using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Thkim.PocketDodger.Editor.BuildPipeline
{
    public static class BuildAndroidDebug
    {
        private const string MainScenePath = "Assets/_Project/Scenes/MainGame.unity";

        [MenuItem("PocketDodger/Build/Android Debug APK")]
        public static void BuildApk()
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string outputDirectory = Path.Combine(projectRoot, "Builds", "Android");
            string outputPath = Path.Combine(outputDirectory, "PocketDodger-debug.apk");

            Directory.CreateDirectory(outputDirectory);
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = new[] { MainScenePath },
                locationPathName = outputPath,
                target = BuildTarget.Android,
                options = BuildOptions.Development
            };

            BuildReport report = UnityEditor.BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result != BuildResult.Succeeded)
            {
                throw new InvalidOperationException($"Android debug build failed: {summary.result}");
            }

            Debug.Log($"Android debug build succeeded: {outputPath} ({summary.totalSize} bytes)");
        }
    }
}
