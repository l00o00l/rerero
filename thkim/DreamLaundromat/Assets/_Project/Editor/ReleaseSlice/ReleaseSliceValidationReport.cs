using System;
using System.Globalization;
using System.IO;
using System.Text;
using Thkim.DreamLaundromat.DynamicLab;
using Thkim.DreamLaundromat.Gameplay.ReleaseSlice;
using UnityEditor;
using UnityEngine;

namespace Thkim.DreamLaundromat.Editor.ReleaseSlice
{
    public static class ReleaseSliceValidationReport
    {
        [MenuItem("DreamLaundromat/Release Slice/Validate Level Pack")]
        public static void WriteDefaultReport()
        {
            WriteReport(CreateOptionsFromCommandLine());
        }

        public static void RunFromCommandLine()
        {
            WriteReport(CreateOptionsFromCommandLine());
        }

        private static void WriteReport(ReleaseSliceValidationOptions options)
        {
            var solveOptions = new DynamicSolveOptions
            {
                MaxVisitedStates = options.MaxVisitedStates,
                TimeoutMilliseconds = options.SolverTimeoutMilliseconds
            };
            ReleaseLevelPackValidationResult result = ReleaseLevelPackValidator.Validate(
                ReleaseLevelPack.CreateDefault(),
                solveOptions);

            string report = ReleaseLevelPackReportFormatter.Format(result);
            Directory.CreateDirectory(Path.GetDirectoryName(options.ReportPath));
            File.WriteAllText(options.ReportPath, report, new UTF8Encoding(true));

            Debug.Log($"Release Slice validation report written: {options.ReportPath}");
            Debug.Log($"Release Slice validation summary: valid={result.IsValid} levels={result.Entries.Count} errors={result.Errors.Count} warnings={result.Warnings.Count} designNotes={result.DesignNotes.Count}");

            if (options.FailOnInvalid && !result.IsValid)
            {
                throw new InvalidOperationException("Release Slice validation failed.");
            }
        }

        private static ReleaseSliceValidationOptions CreateOptionsFromCommandLine()
        {
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            string defaultReportPath = Path.Combine(projectRoot, "Logs", "release-slice-validation-report.txt");
            string[] args = Environment.GetCommandLineArgs();

            return new ReleaseSliceValidationOptions
            {
                ReportPath = GetStringArg(args, "-releaseSliceReportPath", defaultReportPath),
                MaxVisitedStates = GetIntArg(
                    args,
                    "-releaseSliceMaxVisitedStates",
                    ReleaseValidationDefaults.SolveOptions.MaxVisitedStates),
                SolverTimeoutMilliseconds = GetIntArg(
                    args,
                    "-releaseSliceSolverTimeoutMilliseconds",
                    ReleaseValidationDefaults.SolveOptions.TimeoutMilliseconds),
                FailOnInvalid = GetBoolArg(args, "-releaseSliceFailOnInvalid", true)
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

        private sealed class ReleaseSliceValidationOptions
        {
            public string ReportPath = string.Empty;
            public int MaxVisitedStates;
            public int SolverTimeoutMilliseconds;
            public bool FailOnInvalid;
        }
    }
}
