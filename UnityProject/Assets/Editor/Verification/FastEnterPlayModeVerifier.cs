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
        private const double MinWaitSecondsPerPlayMode = 10.0d;
        private const int MinPlayFrames = 30;
        private const double TransitionTimeoutSeconds = 90.0d;

        private static readonly List<string> KeyFailureLogs = new List<string>();
        private static readonly List<string> PlayModeStateTimeline = new List<string>();

        private static VerificationPhase _phase = VerificationPhase.Idle;
        private static VerificationResult _result = VerificationResult.Skipped;
        private static double _phaseStartedAt;
        private static double _playModeEnteredAt;
        private static int _playModeEnteredFrame;
        private static WaitInfo _firstWaitInfo;
        private static WaitInfo _secondWaitInfo;
        private static ConsoleSnapshot _firstSnapshot;
        private static ConsoleSnapshot _secondSnapshot;
        private static string _resultLog;
        private static string _reportPath;
        private static bool _isRunning;
        private static bool _requestedExitPlayMode;
        private static bool _requestedEnterPlayMode;
        private static bool _unexpectedExitDetected;
        private static string _unexpectedExitReason;
        private static bool _hasFinished;

        private enum VerificationPhase
        {
            Idle,
            EnteringFirstPlayMode,
            WaitingFirstPlayMode,
            ExitingFirstPlayMode,
            EnteringSecondPlayMode,
            WaitingSecondPlayMode,
            ExitingSecondPlayMode,
            FinishingAfterUnexpectedExit,
            Finished
        }

        private enum VerificationResult
        {
            Pass,
            Fail,
            FailDiagnostic,
            Skipped
        }

        [MenuItem(MenuPath)]
        public static void RunFromMenu()
        {
            if (_isRunning)
            {
                Debug.LogWarning("Fast Enter Play Mode twice verification is already running.");
                return;
            }

            ResetRunState();
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                _result = VerificationResult.Skipped;
                _resultLog = "SKIPPED:\nFast Enter Play Mode twice verification skipped because editor is busy.";
                _reportPath = WriteReport();
                Finish();
                return;
            }

            _isRunning = true;
            ClearConsole();
            SetPhase(VerificationPhase.EnteringFirstPlayMode);
            RequestEnterPlayMode();
            Debug.Log("Fast Enter Play Mode twice verification started.");
        }

        private static void ResetRunState()
        {
            _phase = VerificationPhase.Idle;
            _result = VerificationResult.Skipped;
            _phaseStartedAt = EditorApplication.timeSinceStartup;
            _playModeEnteredAt = 0.0d;
            _playModeEnteredFrame = 0;
            _firstWaitInfo = WaitInfo.Empty;
            _secondWaitInfo = WaitInfo.Empty;
            _firstSnapshot = ConsoleSnapshot.Empty;
            _secondSnapshot = ConsoleSnapshot.Empty;
            _resultLog = string.Empty;
            _reportPath = string.Empty;
            _isRunning = false;
            _requestedExitPlayMode = false;
            _requestedEnterPlayMode = false;
            _unexpectedExitDetected = false;
            _unexpectedExitReason = string.Empty;
            _hasFinished = false;
            KeyFailureLogs.Clear();
            PlayModeStateTimeline.Clear();
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (!_isRunning && _phase != VerificationPhase.Idle)
            {
                return;
            }

            RecordPlayModeStateChange(state);

            switch (state)
            {
                case PlayModeStateChange.ExitingEditMode:
                    _requestedEnterPlayMode = true;
                    _requestedExitPlayMode = false;
                    break;

                case PlayModeStateChange.EnteredPlayMode:
                    _requestedEnterPlayMode = false;
                    _requestedExitPlayMode = false;
                    _playModeEnteredAt = EditorApplication.timeSinceStartup;
                    _playModeEnteredFrame = Time.frameCount;
                    if (_phase == VerificationPhase.EnteringFirstPlayMode)
                    {
                        SetPhase(VerificationPhase.WaitingFirstPlayMode);
                    }
                    else if (_phase == VerificationPhase.EnteringSecondPlayMode)
                    {
                        SetPhase(VerificationPhase.WaitingSecondPlayMode);
                    }
                    break;

                case PlayModeStateChange.ExitingPlayMode:
                    if (!_requestedExitPlayMode)
                    {
                        MarkUnexpectedExit("Editor left Play Mode without verifier exit request during " + _phase + ".");
                    }
                    break;

                case PlayModeStateChange.EnteredEditMode:
                    if (_phase == VerificationPhase.ExitingFirstPlayMode)
                    {
                        ClearConsole();
                        SetPhase(VerificationPhase.EnteringSecondPlayMode);
                        _requestedEnterPlayMode = false;
                        _requestedExitPlayMode = false;
                    }
                    else if (_phase == VerificationPhase.ExitingSecondPlayMode ||
                             _phase == VerificationPhase.FinishingAfterUnexpectedExit)
                    {
                        Finish();
                    }
                    break;
            }
        }

        private static void Tick()
        {
            try
            {
                switch (_phase)
                {
                    case VerificationPhase.EnteringFirstPlayMode:
                    case VerificationPhase.EnteringSecondPlayMode:
                        if (!EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
                        {
                            RequestEnterPlayMode();
                        }

                        if (ElapsedInPhase() >= TransitionTimeoutSeconds)
                        {
                            FailAndFinish("Timed out during " + _phase + ".");
                        }
                        break;

                    case VerificationPhase.ExitingFirstPlayMode:
                    case VerificationPhase.ExitingSecondPlayMode:
                    case VerificationPhase.FinishingAfterUnexpectedExit:
                        if (ElapsedInPhase() >= TransitionTimeoutSeconds)
                        {
                            FailAndFinish("Timed out during " + _phase + ".");
                        }
                        break;

                    case VerificationPhase.WaitingFirstPlayMode:
                        UpdateFirstWaitInfo();
                        if (!EditorApplication.isPlaying)
                        {
                            MarkUnexpectedExit("Editor left first Play Mode before verification wait completed.");
                            _firstSnapshot = ReadConsoleSnapshot();
                            FailDiagnosticAndExitIfNeeded();
                            return;
                        }

                        if (_firstWaitInfo.MeetsMinimumWait)
                        {
                            _firstSnapshot = ReadConsoleSnapshot();
                            RequestExitPlayMode(VerificationPhase.ExitingFirstPlayMode);
                        }
                        break;

                    case VerificationPhase.WaitingSecondPlayMode:
                        UpdateSecondWaitInfo();
                        if (!EditorApplication.isPlaying)
                        {
                            MarkUnexpectedExit("Editor left second Play Mode before verification wait completed.");
                            _secondSnapshot = ReadConsoleSnapshot();
                            FailDiagnosticAndExitIfNeeded();
                            return;
                        }

                        if (_secondWaitInfo.MeetsMinimumWait)
                        {
                            _secondSnapshot = ReadConsoleSnapshot();
                            CompleteNormalVerification();
                        }
                        break;
                }
            }
            catch (Exception exception)
            {
                FailAndFinish(exception.ToString());
            }
        }

        private static void CompleteNormalVerification()
        {
            var passed = _firstSnapshot.CanRead &&
                         _secondSnapshot.CanRead &&
                         _firstSnapshot.ErrorCount == 0 &&
                         _firstSnapshot.WarningCount == 0 &&
                         _secondSnapshot.ErrorCount == 0 &&
                         _secondSnapshot.WarningCount == 0 &&
                         !_unexpectedExitDetected;

            _result = passed ? VerificationResult.Pass : VerificationResult.Fail;
            _resultLog = passed
                ? "PASS:\nFast Enter Play Mode twice verification passed."
                : "FAIL:\nFast Enter Play Mode twice verification failed.";
            _reportPath = WriteReport();
            RequestExitPlayMode(VerificationPhase.ExitingSecondPlayMode);
        }

        private static void FailDiagnosticAndExitIfNeeded()
        {
            _result = VerificationResult.FailDiagnostic;
            _resultLog = "FAIL-DIAGNOSTIC:\nFast Enter Play Mode twice verification ended with diagnostic failure.";
            AddKeyFailureLog(_unexpectedExitReason);
            _reportPath = WriteReport();

            if (EditorApplication.isPlaying)
            {
                RequestExitPlayMode(VerificationPhase.FinishingAfterUnexpectedExit);
            }
            else
            {
                SetPhase(VerificationPhase.FinishingAfterUnexpectedExit);
                Finish();
            }
        }

        private static void FailAndFinish(string reason)
        {
            AddKeyFailureLog(reason);
            if (_unexpectedExitDetected || IsTransitionPhase(_phase))
            {
                _result = VerificationResult.FailDiagnostic;
                _resultLog = "FAIL-DIAGNOSTIC:\nFast Enter Play Mode twice verification ended with diagnostic failure.";
            }
            else
            {
                _result = VerificationResult.Fail;
                _resultLog = "FAIL:\nFast Enter Play Mode twice verification failed.";
            }

            _reportPath = WriteReport();
            if (EditorApplication.isPlaying)
            {
                RequestExitPlayMode(VerificationPhase.FinishingAfterUnexpectedExit);
            }
            else
            {
                Finish();
            }
        }

        private static bool IsTransitionPhase(VerificationPhase phase)
        {
            return phase == VerificationPhase.EnteringFirstPlayMode ||
                   phase == VerificationPhase.ExitingFirstPlayMode ||
                   phase == VerificationPhase.EnteringSecondPlayMode ||
                   phase == VerificationPhase.ExitingSecondPlayMode ||
                   phase == VerificationPhase.FinishingAfterUnexpectedExit;
        }

        private static void Finish()
        {
            if (_hasFinished)
            {
                return;
            }

            if (EditorApplication.isPlaying)
            {
                RequestExitPlayMode(VerificationPhase.FinishingAfterUnexpectedExit);
                return;
            }

            _hasFinished = true;
            EditorApplication.update -= Tick;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            SetPhase(VerificationPhase.Finished);
            _isRunning = false;

            if (string.IsNullOrEmpty(_resultLog))
            {
                _result = VerificationResult.Skipped;
                _resultLog = "SKIPPED:\nFast Enter Play Mode twice verification did not run.";
            }

            if (string.IsNullOrEmpty(_reportPath) || !File.Exists(_reportPath))
            {
                _reportPath = WriteReport();
            }

            if (_result == VerificationResult.Pass)
            {
                Debug.Log(_resultLog + "\nReport: " + _reportPath);
            }
            else
            {
                Debug.LogWarning(_resultLog + "\nReport: " + _reportPath);
            }
        }

        private static void RequestExitPlayMode(VerificationPhase nextPhase)
        {
            _requestedExitPlayMode = true;
            _requestedEnterPlayMode = false;
            SetPhase(nextPhase);
            EditorApplication.ExitPlaymode();
        }

        private static void RequestEnterPlayMode()
        {
            if (_requestedEnterPlayMode)
            {
                return;
            }

            _requestedEnterPlayMode = true;
            _requestedExitPlayMode = false;
            EditorApplication.EnterPlaymode();
        }

        private static void MarkUnexpectedExit(string reason)
        {
            if (_unexpectedExitDetected)
            {
                return;
            }

            _unexpectedExitDetected = true;
            _unexpectedExitReason = reason;
            AddKeyFailureLog(reason);
            if (_phase == VerificationPhase.WaitingFirstPlayMode)
            {
                UpdateFirstWaitInfo();
            }
            else if (_phase == VerificationPhase.WaitingSecondPlayMode)
            {
                UpdateSecondWaitInfo();
            }
        }

        private static void RecordPlayModeStateChange(PlayModeStateChange state)
        {
            var line = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") +
                       " | state=" + state +
                       " | phase=" + _phase +
                       " | isPlaying=" + EditorApplication.isPlaying +
                       " | isPlayingOrWillChangePlaymode=" + EditorApplication.isPlayingOrWillChangePlaymode +
                       " | frame=" + Time.frameCount +
                       " | requestedExit=" + _requestedExitPlayMode;
            PlayModeStateTimeline.Add(line);
        }

        private static void UpdateFirstWaitInfo()
        {
            _firstWaitInfo = CaptureWaitInfo();
        }

        private static void UpdateSecondWaitInfo()
        {
            _secondWaitInfo = CaptureWaitInfo();
        }

        private static WaitInfo CaptureWaitInfo()
        {
            var elapsedSeconds = _playModeEnteredAt <= 0.0d
                ? 0.0d
                : EditorApplication.timeSinceStartup - _playModeEnteredAt;
            var elapsedFrames = _playModeEnteredFrame <= 0
                ? 0
                : Time.frameCount - _playModeEnteredFrame;

            return new WaitInfo
            {
                ElapsedSeconds = elapsedSeconds,
                ElapsedFrames = elapsedFrames,
                MeetsMinimumWait = EditorApplication.isPlaying &&
                                   elapsedSeconds >= MinWaitSecondsPerPlayMode &&
                                   elapsedFrames >= MinPlayFrames
            };
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
                var messageField = logEntryType.GetField("message", instanceFlags);
                var fileField = logEntryType.GetField("file", instanceFlags);
                var lineField = logEntryType.GetField("line", instanceFlags);
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
                        var condition = BuildLogEntryMessage(entry, conditionField, messageField, fileField, lineField, i);

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

        private static string BuildLogEntryMessage(
            object entry,
            FieldInfo conditionField,
            FieldInfo messageField,
            FieldInfo fileField,
            FieldInfo lineField,
            int index)
        {
            var condition = conditionField != null ? conditionField.GetValue(entry) as string : null;
            var message = messageField != null ? messageField.GetValue(entry) as string : null;
            var file = fileField != null ? fileField.GetValue(entry) as string : null;
            var line = lineField != null ? lineField.GetValue(entry) : null;
            var text = !string.IsNullOrWhiteSpace(condition) ? condition : message;

            if (string.IsNullOrWhiteSpace(text))
            {
                text = "Console entry #" + index + " did not expose a readable message through reflection.";
            }

            if (!string.IsNullOrWhiteSpace(file))
            {
                text += " (" + file;
                if (line != null)
                {
                    text += ":" + line;
                }

                text += ")";
            }

            return text;
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
            var report = new StringBuilder();

            report.AppendLine("# Phase81 Fast Enter Play Mode 连续两次验证报告");
            report.AppendLine();
            report.Append("- 验证时间：").Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).AppendLine();
            report.Append("- Unity 版本：").Append(Application.unityVersion).AppendLine();
            report.Append("- 当前分支：").Append(RunGit("rev-parse --abbrev-ref HEAD")).AppendLine();
            report.Append("- 当前 commit：").Append(RunGit("rev-parse --short HEAD")).AppendLine();
            report.Append("- Enter Play Mode Options Enabled：").Append(enterPlayModeEnabled ? "1" : "0").AppendLine();
            report.Append("- Enter Play Mode Options：").Append((int)enterPlayModeOptions).Append(" (").Append(enterPlayModeOptions).AppendLine(")");
            report.Append("- 最终结果：").Append(GetResultText()).AppendLine();
            report.Append("- 最终阶段：").Append(_phase).AppendLine();
            report.Append("- 是否发生非预期退出：").Append(_unexpectedExitDetected ? "true" : "false").AppendLine();
            report.Append("- 非预期退出原因：").Append(string.IsNullOrEmpty(_unexpectedExitReason) ? "无" : _unexpectedExitReason).AppendLine();
            report.Append("- 是否工具主动请求退出：").Append(_requestedExitPlayMode ? "true" : "false").AppendLine();
            report.AppendLine();
            report.AppendLine("## 验证结论");
            report.AppendLine();
            report.AppendLine("```text");
            report.AppendLine(_resultLog);
            report.AppendLine("```");
            report.AppendLine();
            AppendWaitInfo(report);
            AppendConsoleSnapshot(report);
            AppendTimeline(report);
            AppendUnmodifiedScope(report);
            return report.ToString();
        }

        private static void AppendWaitInfo(StringBuilder report)
        {
            report.AppendLine("## 每轮 Play Mode 等待信息");
            report.AppendLine();
            AppendSingleWaitInfo(report, "第一次", _firstWaitInfo);
            AppendSingleWaitInfo(report, "第二次", _secondWaitInfo);
            report.AppendLine();
        }

        private static void AppendSingleWaitInfo(StringBuilder report, string label, WaitInfo waitInfo)
        {
            report.Append("- ").Append(label)
                .Append("实际等待秒数：").Append(waitInfo.ElapsedSeconds.ToString("0.00"))
                .Append("，实际经过帧数：").Append(waitInfo.ElapsedFrames)
                .Append("，是否满足最短等待条件：").Append(waitInfo.MeetsMinimumWait ? "true" : "false")
                .AppendLine();
        }

        private static void AppendConsoleSnapshot(StringBuilder report)
        {
            report.AppendLine("## Console Snapshot");
            report.AppendLine();
            report.Append("- 第一次 Play Mode Console Error：").Append(_firstSnapshot.ErrorCount).AppendLine();
            report.Append("- 第一次 Play Mode Console Warning：").Append(_firstSnapshot.WarningCount).AppendLine();
            report.Append("- 第二次 Play Mode Console Error：").Append(_secondSnapshot.ErrorCount).AppendLine();
            report.Append("- 第二次 Play Mode Console Warning：").Append(_secondSnapshot.WarningCount).AppendLine();
            report.Append("- Console 读取状态：").Append(GetConsoleReadStatus()).AppendLine();
            report.AppendLine();
            report.AppendLine("### 关键错误或警告日志");
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
        }

        private static void AppendTimeline(StringBuilder report)
        {
            report.AppendLine("## PlayModeStateChange 时间线");
            report.AppendLine();
            if (PlayModeStateTimeline.Count == 0)
            {
                report.AppendLine("- 未记录到 PlayModeStateChange。");
            }
            else
            {
                foreach (var line in PlayModeStateTimeline)
                {
                    report.Append("- ").Append(SanitizeMarkdownLine(line)).AppendLine();
                }
            }

            report.AppendLine();
        }

        private static void AppendUnmodifiedScope(StringBuilder report)
        {
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
        }

        private static string GetResultText()
        {
            switch (_result)
            {
                case VerificationResult.Pass:
                    return "PASS";
                case VerificationResult.Fail:
                    return "FAIL";
                case VerificationResult.FailDiagnostic:
                    return "FAIL-DIAGNOSTIC";
                default:
                    return "SKIPPED";
            }
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

        private struct WaitInfo
        {
            public static readonly WaitInfo Empty = new WaitInfo();

            public double ElapsedSeconds;
            public int ElapsedFrames;
            public bool MeetsMinimumWait;
        }
    }
}
#endif
