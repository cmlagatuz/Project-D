// Project D  |  Codename: Zantetsuken
// DuelBuildErrorReport.cs  (Editor-only)
//
//   Tools > Project D > Save Last Gradle Error Report
//
// The Unity Console only shows "Gradle build failed". The real reason ("What went wrong") is in Unity's Editor.log.
// This pulls it out into Builds/LastGradleError.txt (and opens it) so it can be pasted or attached. It also runs
// automatically when the duel demo APK build fails.
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace ProjectD.EditorTools
{
    public static class DuelBuildErrorReport
    {
        private const string OutputPath = "Builds/LastGradleError.txt";

        [MenuItem("Tools/Project D/Save Last Gradle Error Report", priority = 49)]
        public static void Menu()
        {
            string path = Save();
            if (path == null)
            {
                const string msg = "Could not read Unity's Editor.log, so no report was saved.";
                Debug.LogWarning("[Project D] " + msg);
                if (!Application.isBatchMode) EditorUtility.DisplayDialog("Project D", msg, "OK");
                return;
            }
            Debug.Log("[Project D] Gradle error report saved to " + path);
            if (!Application.isBatchMode) EditorUtility.RevealInFinder(path);
        }

        /// <summary>Writes the last Gradle failure found in Editor.log to Builds/LastGradleError.txt. Returns the path or null.</summary>
        public static string Save()
        {
            string log;
            try
            {
                string logPath = Application.consoleLogPath;   // Editor.log while in the editor
                using (var fs = new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var sr = new StreamReader(fs)) log = sr.ReadToEnd();
            }
            catch (Exception e)
            {
                Debug.LogWarning("[Project D] Could not read Editor.log: " + e.Message);
                return null;
            }

            var sb = new StringBuilder();
            sb.AppendLine("Project D - last Gradle failure (from Editor.log, " + DateTime.Now + ")");
            sb.AppendLine();

            int fail = log.LastIndexOf("FAILURE: Build failed", StringComparison.Ordinal);
            if (fail < 0) fail = log.LastIndexOf("What went wrong", StringComparison.Ordinal);
            if (fail >= 0)
            {
                int end = log.IndexOf("BUILD FAILED", fail, StringComparison.Ordinal);
                if (end < 0 || end - fail > 6000) end = Math.Min(log.Length, fail + 6000);
                sb.AppendLine(log.Substring(fail, end - fail));
            }
            else sb.AppendLine("No 'FAILURE: Build failed' section found in Editor.log.");

            // Duplicate-class lines (the Kotlin problem), de-duplicated.
            var dup = new List<string>();
            var seen = new HashSet<string>();
            foreach (var raw in log.Split('\n'))
            {
                string line = raw.Trim();
                if (line.StartsWith("Duplicate class ") && seen.Add(line) && dup.Count < 20) dup.Add(line);
            }
            if (dup.Count > 0)
            {
                sb.AppendLine();
                sb.AppendLine("Duplicate class lines (first " + dup.Count + "):");
                foreach (var d in dup) sb.AppendLine(d);
            }

            Directory.CreateDirectory("Builds");
            File.WriteAllText(OutputPath, sb.ToString());
            return OutputPath;
        }
    }
}
#endif
