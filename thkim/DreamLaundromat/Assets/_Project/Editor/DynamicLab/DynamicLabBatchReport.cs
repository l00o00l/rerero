using System;
using System.Globalization;
using System.IO;
using Thkim.DreamLaundromat.DynamicLab;
using UnityEditor;
using UnityEngine;

namespace Thkim.DreamLaundromat.Editor.DynamicLab
{
    public static class DynamicLabBatchReport
    {
        [MenuItem("DreamLaundromat/Dynamic Lab/Write Batch Report")]
        public static void WriteDefaultReport()
        {
            WriteReport(CreateOptionsFromCommandLine());
        }

        public static void RunFromCommandLine()
        {
            WriteReport(CreateOptionsFromCommandLine());
        }

        private static void WriteReport(DynamicLabBatchReportOptions options)
        {
            DynamicBatchSimulationResult result = DynamicRoundBatchSimulator.Run(
                DynamicSampleRecipes.CreateAll(),
                new DynamicBatchSimulationOptions
                {
                    SeedStart = options.SeedStart,
                    CandidateCountPerRecipe = options.CandidateCountPerRecipe,
                    SolveOptions = new DynamicSolveOptions
                    {
                        MaxVisitedStates = options.MaxVisitedStates,
                        TimeoutMilliseconds = options.SolverTimeoutMilliseconds
                    }
                });

            string report = DynamicBatchReportFormatter.Format(result);
            Directory.CreateDirectory(Path.GetDirectoryName(options.ReportPath));
            File.WriteAllText(options.ReportPath, report);

            Debug.Log($"Dynamic Lab batch report written: {options.ReportPath}");
            Debug.Log($"Dynamic Lab batch summary: total={result.TotalCount} accepted={result.AcceptedCount} rejected={result.RejectedCount}");

            if (options.FailOnNoAccepted && result.AcceptedCount == 0)
            {
                throw new InvalidOperationException("Dynamic Lab batch report produced no accepted candidates.");
            }
        }

        private static DynamicLabBatchReportOptions CreateOptionsFromCommandLine()
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string defaultReportPath = Path.Combine(projectRoot, "Logs", "dynamic-lab-batch-report.txt");
            string[] args = Environment.GetCommandLineArgs();

            return new DynamicLabBatchReportOptions
            {
                ReportPath = GetStringArg(args, "-dynamicLabReportPath", defaultReportPath),
                SeedStart = GetIntArg(args, "-dynamicLabSeedStart", 1),
                CandidateCountPerRecipe = GetIntArg(args, "-dynamicLabCandidateCount", 8),
                MaxVisitedStates = GetIntArg(args, "-dynamicLabMaxVisitedStates", 10000),
                SolverTimeoutMilliseconds = GetIntArg(args, "-dynamicLabSolverTimeoutMilliseconds", 1000),
                FailOnNoAccepted = GetBoolArg(args, "-dynamicLabFailOnNoAccepted", true)
            };
        }

        private static string GetStringArg(string[] args, string name, string defaultValue)
        {
            int index = Array.IndexOf(args, name);
            if (index < 0 || index + 1 >= args.Length)
            {
                return defaultValue;
            }

            return args[index + 1];
        }

        private static int GetIntArg(string[] args, string name, int defaultValue)
        {
            string value = GetStringArg(args, name, string.Empty);
            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed)
                ? parsed
                : defaultValue;
        }

        private static bool GetBoolArg(string[] args, string name, bool defaultValue)
        {
            string value = GetStringArg(args, name, string.Empty);
            return bool.TryParse(value, out bool parsed) ? parsed : defaultValue;
        }

        private sealed class DynamicLabBatchReportOptions
        {
            public string ReportPath = string.Empty;
            public int SeedStart;
            public int CandidateCountPerRecipe;
            public int MaxVisitedStates;
            public int SolverTimeoutMilliseconds;
            public bool FailOnNoAccepted;
        }
    }
}
