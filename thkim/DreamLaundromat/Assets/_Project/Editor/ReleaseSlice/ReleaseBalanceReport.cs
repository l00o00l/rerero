using System;
using System.IO;
using System.Text;
using Thkim.DreamLaundromat.Gameplay.ReleaseSlice;
using UnityEditor;
using UnityEngine;

namespace Thkim.DreamLaundromat.Editor.ReleaseSlice
{
    public static class ReleaseBalanceReport
    {
        [MenuItem("DreamLaundromat/Release Slice/Write Balance Report")]
        public static void WriteDefaultReport()
        {
            WriteReport(CreateReportPathFromCommandLine());
        }

        public static void RunFromCommandLine()
        {
            WriteReport(CreateReportPathFromCommandLine());
        }

        private static void WriteReport(string reportPath)
        {
            ReleaseBalanceReportResult result = ReleaseBalanceReportBuilder.BuildResult(ReleaseLevelPack.CreateDefault());
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath));
            File.WriteAllText(reportPath, result.Report, new UTF8Encoding(true));
            Debug.Log($"Release balance report written: {reportPath}");

            if (!result.IsValid)
            {
                throw new InvalidOperationException("Release balance report failed validation.");
            }
        }

        private static string CreateReportPathFromCommandLine()
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string defaultReportPath = Path.Combine(projectRoot, "Logs", "release-balance-report.txt");
            string[] args = Environment.GetCommandLineArgs();
            int index = Array.IndexOf(args, "-releaseBalanceReportPath");
            return index >= 0 && index + 1 < args.Length ? args[index + 1] : defaultReportPath;
        }
    }
}
