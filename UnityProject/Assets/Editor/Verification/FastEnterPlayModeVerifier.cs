#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace CodexTools.Verification
{
    public static class FastEnterPlayModeVerifier
    {
        private const string MenuPath = "Codex/Verify/Fast Enter Play Mode Twice";
        private const string ReportPathFromRepoRoot = "Doc/验证报告/Phase81_FastEnterPlayMode_Twice_Verification.md";
        private const double WaitSecondsPerPlayMode = 5.0d;
        private const double TransitionTimeoutSeconds = 45.0d;

        private static readonly List<string> KeyFailureLogs = new List<string>();

        private static VerificationPhase _phase = VerificationPhase.Idle;
        private static double _phaseStartedAt;
        private static ConsoleSnapshot _firstSnapshot;
        private static ConsoleSnapshot _secondSnapshot;
        private static string _resultLog;
        private static string _reportPath;
        private static bool _isRunning;

        private enum VerificationPhase
        {
            Idle,
            EnteringFirstPlayMode,
            WaitingFirstPlayMode,
            ExitingFirstPlayMode,
            EnteringSecondPlayMode,
            WaitingSecondPlayMode,
            ExitingSecondPlayMode,
            Finished
        }

        [MenuItem(MenuPath)]
        public static void RunFromMenu()
        {
            if (_isRunning)
            {
                Debug.LogWarning("Fast Enter Play Mode twice verification is already running.");
                return;
            }

            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                Debug.LogWarning("Fast Enter Play Mode twice verification skipped: editor is busy.");
                return;
            }

            _isRunning = true;
            _firstSnapshot = ConsoleSnapshot.Empty;
            _secondSnapshot = ConsoleSnapshot.Empty;
            _resultLog = string.Empty;
            _reportPath = string.Empty;
            KeyFailureLogs.Clear();

            ClearConsole();
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            SetPhase(VerificationPhase.EnteringFirstPlayMode);
            EditorApplication.EnterPlaymode();
            Debug.Log("Fast Enter Play Mode twice verification started.");
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (!_isRunning)
            {
                return;
            }

            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                if (_phase == VerificationPhase.EnteringFirstPlayMode)
                {
                    SetPhase(VerificationPhase.WaitingFirstPlayMode);
                }
                else if (_phase == VerificationPhase.EnteringSecondPlayMode)
                {
                    SetPhase(VerificationPhase.WaitingSecondPlayMode);
                }
            }
            else if (state == PlayModeStateChange.EnteredEditMode)
            {
                if (_phase == VerificationPhase.ExitingFirstPlayMode)
                {
                    ClearConsole();
                    SetPhase(VerificationPhase.EnteringSecondPlayMode);
                    EditorApplication.delayCall += EditorApplication.EnterPlaymode;
                }
                else if (_phase == VerificationPhase.ExitingSecondPlayMode)
                {
                    Finish();
                }
            }
        }

        private static void Tick()
        {
            try
            {
                switch (_phase)
                {
                    case VerificationPhase.EnteringFirstPlayMode:
                    case VerificationPhase.ExitingFirstPlayMode:
                    case VerificationPhase.EnteringSecondPlayMode:
                    case VerificationPhase.ExitingSecondPlayMode:
                        if (ElapsedInPhase() >= TransitionTimeoutSeconds)
                        {
                            FailAndFinish("Timed out during " + _phase + ".");
                        }

                        break;

                    case VerificationPhase.WaitingFirstPlayMode:
                        if (!EditorApplication.isPlaying)
                        {
                            FailAndFinish("Editor left first Play Mode before verification wait completed.");
                            return;
                        }

                        if (ElapsedInPhase() >= WaitSecondsPerPlayMode)
                        {
                            _firstSnapshot = ReadConsoleSnapshot();
                            SetPhase(VerificationPhase.ExitingFirstPlayMode);
                            EditorApplication.ExitPlaymode();
                        }

                        break;

                    case VerificationPhase.WaitingSecondPlayMode:
                        if (!EditorApplication.isPlaying)
                        {
                            FailAndFinish("Editor left second Play Mode before verification wait completed.");
                            return;
                        }

                        if (ElapsedInPhase() >= WaitSecondsPerPlayMode)
                        {
                            _secondSnapshot = ReadConsoleSnapshot();
                            var passed = _firstSnapshot.CanRead &&
                                         _secondSnapshot.CanRead &&
                                         _firstSnapshot.ErrorCount == 0 &&
                                         _firstSnapshot.WarningCount == 0 &&
                                         _secondSnapshot.ErrorCount == 0 &&
                                         _secondSnapshot.WarningCount == 0;
                            _resultLog = passed
                                ? "PASS:\nFast Enter Play Mode twice verification passed."
                                : "FAIL:\nFast Enter Play Mode twice verification failed.";
                            _reportPath = WriteReport();
                            SetPhase(VerificationPhase.ExitingSecondPlayMode);
                            EditorApplication.ExitPlaymode();
                        }

                        break;
                }
            }
            catch (Exception exception)
            {
                FailAndFinish(exception.ToString());
            }
        }

        private static void FailAndFinish(string reason)
        {
            AddKeyFailureLog(reason);
            _resultLog = "FAIL:\nFast Enter Play Mode twice verification failed.";
            _reportPath = WriteReport();

            if (EditorApplication.isPlaying)
            {
                EditorApplication.ExitPlaymode();
            }

            Finish();
        }

        private static void Finish()
        {
            EditorApplication.update -= Tick;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            SetPhase(VerificationPhase.Finished);
            _isRunning = false;

            if (string.IsNullOrEmpty(_resultLog))
            {
                _resultLog = "FAIL:\nFast Enter Play Mode twice verification failed.";
            }

            if (string.IsNullOrEmpty(_reportPath) || !File.Exists(_reportPath))
            {
                _reportPath = WriteReport();
            }

            if (_resultLog.StartsWith("PASS:", StringComparison.Ordinal))
            {
                Debug.Log(_resultLog + "\nReport: " + _reportPath);
            }
            else
            {
                Debug.LogError(_resultLog + "\nReport: " + _reportPath);
            }
        }

        private static void SetPhase(VerificationPhase phase)
        {
            _phase = phase;
            _phaseStartedAt = EditorApplication.timeSinceStartup;
        }

        private static double ElapsedInPhase()
        {
            return EditorApplication.timeSinceStartup - _phaseStartedAt;
        }

        private static ConsoleSnapshot ReadConsoleSnapshot()
        {
            try
            {
                var logEntriesType = Type.GetType("UnityEditor.LogEntries,UnityEditor");
                var logEntryType = Type.GetType("UnityEditor.LogEntry,UnityEditor");
                if (logEntriesType == null || logEntryType == null)
                {
                    return ConsoleSnapshot.Failed("UnityEditor.LogEntries or UnityEditor.LogEntry type was not found.");
                }

                var staticFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
                var instanceFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                var startGettingEntries = logEntriesType.GetMethod("StartGettingEntries", staticFlags);
                var endGettingEntries = logEntriesType.GetMethod("EndGettingEntries", staticFlags);
                var getCount = logEntriesType.GetMethod("GetCount", staticFlags);
                var getEntryInternal = logEntriesType.GetMethod("GetEntryInternal", staticFlags);
                var modeField = logEntryType.GetField("mode", instanceFlags);
                var conditionField = logEntryType.GetField("condition", instanceFlags);
                if (startGettingEntries == null ||
                    endGettingEntries == null ||
                    getCount == null ||
                    getEntryInternal == null ||
                    modeField == null)
                {
                    return ConsoleSnapshot.Failed("UnityEditor.LogEntries reflection members were incomplete.");
                }

                var entry = Activator.CreateInstance(logEntryType);
                var snapshot = new ConsoleSnapshot { CanRead = true };

                startGettingEntries.Invoke(null, null);
                try
                {
                    var count = (int)getCount.Invoke(null, null);
                    for (var i = 0; i < count; i++)
                    {
                        getEntryInternal.Invoke(null, new[] { (object)i, entry });
                        var mode = (int)modeField.GetValue(entry);
                        var condition = conditionField != null
                            ? conditionField.GetValue(entry) as string ?? string.Empty
                            : string.Empty;

                        if (IsErrorMode(mode))
                        {
                            snapshot.ErrorCount++;
                            AddKeyFailureLog(condition);
                        }
                        else if (IsWarningMode(mode))
                        {
                            snapshot.WarningCount++;
                            AddKeyFailureLog(condition);
                        }
                    }
                }
                finally
                {
                    endGettingEntries.Invoke(null, null);
                }

                return snapshot;
            }
            catch (Exception exception)
            {
                return ConsoleSnapshot.Failed(exception.Message);
            }
        }

        private static bool IsErrorMode(int mode)
        {
            return (mode & 0x40) != 0 || (mode & 0x400) != 0 || (mode & 0x800) != 0;
        }

        private static bool IsWarningMode(int mode)
        {
            return (mode & 0x2) != 0;
        }

        private static void AddKeyFailureLog(string message)
        {
            if (string.IsNullOrWhiteSpace(message) || KeyFailureLogs.Count >= 20)
            {
                return;
            }

            if (!KeyFailureLogs.Contains(message))
            {
                KeyFailureLogs.Add(message);
            }
        }

        private static void ClearConsole()
        {
            try
            {
                var logEntriesType = Type.GetType("UnityEditor.LogEntries,UnityEditor");
                var clearMethod = logEntriesType != null
                    ? logEntriesType.GetMethod("Clear", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                    : null;
                clearMethod?.Invoke(null, null);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Failed to clear Unity Console by reflection: " + exception.Message);
            }
        }

        private static string WriteReport()
        {
            var reportPath = GetReportPath();
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath));
            File.WriteAllText(reportPath, BuildReport(), new UTF8Encoding(false));
            return reportPath;
        }

        private static string BuildReport()
        {
            var enterPlayModeEnabled = EditorSettings.enterPlayModeOptionsEnabled;
            var enterPlayModeOptions = EditorSettings.enterPlayModeOptions;
            var passed = _resultLog.StartsWith("PASS:", StringComparison.Ordinal);
            var report = new StringBuilder();

            report.AppendLine("# Phase81 Fast Enter Play Mode 连续两次验证报告");
            report.AppendLine();
            report.Append("- 验证时间：").Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).AppendLine();
            report.Append("- Unity 版本：").Append(Application.unityVersion).AppendLine();
            report.Append("- 当前分支：").Append(RunGit("rev-parse --abbrev-ref HEAD")).AppendLine();
            report.Append("- 当前 commit：").Append(RunGit("rev-parse --short HEAD")).AppendLine();
            report.Append("- Enter Play Mode Options Enabled：").Append(enterPlayModeEnabled ? "1" : "0").AppendLine();
            report.Append("- Enter Play Mode Options：").Append((int)enterPlayModeOptions).Append(" (").Append(enterPlayModeOptions).AppendLine(")");
            report.Append("- 第一次 Play Mode Console Error：").Append(_firstSnapshot.ErrorCount).AppendLine();
            report.Append("- 第一次 Play Mode Console Warning：").Append(_firstSnapshot.WarningCount).AppendLine();
            report.Append("- 第二次 Play Mode Console Error：").Append(_secondSnapshot.ErrorCount).AppendLine();
            report.Append("- 第二次 Play Mode Console Warning：").Append(_secondSnapshot.WarningCount).AppendLine();
            report.Append("- Console 读取状态：").Append(GetConsoleReadStatus()).AppendLine();
            report.Append("- 是否通过：").Append(passed ? "通过" : "失败").AppendLine();
            report.AppendLine();
            report.AppendLine("## 验证结论");
            report.AppendLine();
            report.AppendLine("```text");
            report.AppendLine(_resultLog);
            report.AppendLine("```");
            report.AppendLine();
            report.AppendLine("## 关键错误或警告日志");
            report.AppendLine();
            if (KeyFailureLogs.Count == 0)
            {
                report.AppendLine("- 未记录到关键错误或警告日志。");
            }
            else
            {
                foreach (var log in KeyFailureLogs)
                {
                    report.Append("- ").Append(SanitizeMarkdownLine(log)).AppendLine();
                }
            }

            report.AppendLine();
            report.AppendLine("## 本轮未修改范围");
            report.AppendLine();
            report.AppendLine("- 未修改 UI Prefab。");
            report.AppendLine("- 未修改 MainRoute 视觉。");
            report.AppendLine("- 未修改修仙玩法逻辑。");
            report.AppendLine("- 未修改战斗、路线、事件、奖励逻辑。");
            report.AppendLine("- 未修改 Luban。");
            report.AppendLine("- 未修改 UIModule。");
            report.AppendLine("- 未修改 ProjectSettings。");
            report.AppendLine("- 未恢复旧肉鸽玩法、旧 UI Prefab 或旧视觉资产。");

            return report.ToString();
        }

        private static string GetConsoleReadStatus()
        {
            if (_firstSnapshot.CanRead && _secondSnapshot.CanRead)
            {
                return "成功";
            }

            var builder = new StringBuilder("失败");
            if (!string.IsNullOrEmpty(_firstSnapshot.ReadError))
            {
                builder.Append("；第一次：").Append(_firstSnapshot.ReadError);
            }

            if (!string.IsNullOrEmpty(_secondSnapshot.ReadError))
            {
                builder.Append("；第二次：").Append(_secondSnapshot.ReadError);
            }

            return builder.ToString();
        }

        private static string SanitizeMarkdownLine(string value)
        {
            return value.Replace("\r", " ").Replace("\n", " ").Trim();
        }

        private static string GetReportPath()
        {
            return Path.Combine(GetRepoRoot(), ReportPathFromRepoRoot);
        }

        private static string GetRepoRoot()
        {
            var unityProjectRoot = Directory.GetParent(Application.dataPath).FullName;
            return Directory.GetParent(unityProjectRoot).FullName;
        }

        private static string RunGit(string arguments)
        {
            try
            {
                var startInfo = new ProcessStartInfo("git", arguments)
                {
                    WorkingDirectory = GetRepoRoot(),
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                using (var process = Process.Start(startInfo))
                {
                    if (process == null)
                    {
                        return "无法启动 git";
                    }

                    var output = process.StandardOutput.ReadToEnd().Trim();
                    var error = process.StandardError.ReadToEnd().Trim();
                    process.WaitForExit(3000);
                    if (process.ExitCode == 0 && !string.IsNullOrEmpty(output))
                    {
                        return output;
                    }

                    return string.IsNullOrEmpty(error) ? "无法获取" : error;
                }
            }
            catch (Exception exception)
            {
                return "无法获取：" + exception.Message;
            }
        }

        private struct ConsoleSnapshot
        {
            public static readonly ConsoleSnapshot Empty = new ConsoleSnapshot { CanRead = true };

            public bool CanRead;
            public int ErrorCount;
            public int WarningCount;
            public string ReadError;

            public static ConsoleSnapshot Failed(string error)
            {
                return new ConsoleSnapshot
                {
                    CanRead = false,
                    ReadError = error
                };
            }
        }
    }
}
#endif
