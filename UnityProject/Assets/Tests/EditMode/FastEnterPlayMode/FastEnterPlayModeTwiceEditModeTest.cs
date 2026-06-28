#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using Debug = UnityEngine.Debug;

namespace GameLogic.Tests.FastEnterPlayMode
{
    public sealed class FastEnterPlayModeTwiceEditModeTest
    {
        private const double MinWaitSecondsPerPlayMode = 10.0d;
        private const int MinPlayFrames = 30;
        private const string ReportPathFromRepoRoot = "Doc/验证报告/Phase81_FastEnterPlayMode_UnityTest_Verification.md";

        private readonly List<CapturedLog> _firstLogs = new List<CapturedLog>();
        private readonly List<CapturedLog> _secondLogs = new List<CapturedLog>();
        private readonly List<string> _keyLogs = new List<string>();

        private int _activeRound;
        private RoundInfo _firstRound;
        private RoundInfo _secondRound;
        private string _result = "FAIL-DIAGNOSTIC";
        private string _failureReason = string.Empty;

        [UnityTest]
        public IEnumerator FastEnterPlayModeTwice_HasNoConsoleErrorsOrWarnings()
        {
            LogAssert.ignoreFailingMessages = true;
            Application.logMessageReceived += OnLogMessageReceived;

            yield return RunRound(1);
            yield return new ExitPlayMode();
            yield return WaitForEditModeStable();

            yield return RunRound(2);
            yield return new ExitPlayMode();
            yield return WaitForEditModeStable();

            Application.logMessageReceived -= OnLogMessageReceived;
            EvaluateResult();
            WriteReport();

            Assert.Pass("Fast Enter Play Mode command line verification finished with result: " + _result);
        }

        private IEnumerator RunRound(int round)
        {
            _activeRound = round;
            yield return new EnterPlayMode();

            var startedAt = Time.realtimeSinceStartupAsDouble;
            var startedFrame = Time.frameCount;
            while (Time.realtimeSinceStartupAsDouble - startedAt < MinWaitSecondsPerPlayMode ||
                   Time.frameCount - startedFrame < MinPlayFrames)
            {
                yield return null;
            }

            var info = new RoundInfo
            {
                ElapsedSeconds = Time.realtimeSinceStartupAsDouble - startedAt,
                ElapsedFrames = Time.frameCount - startedFrame
            };

            if (round == 1)
            {
                _firstRound = info;
            }
            else
            {
                _secondRound = info;
            }
        }

        private static IEnumerator WaitForEditModeStable()
        {
            for (var i = 0; i < 5; i++)
            {
                yield return null;
            }
        }

        private void OnLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            if (_activeRound != 1 && _activeRound != 2)
            {
                return;
            }

            var log = new CapturedLog
            {
                Condition = condition,
                StackTrace = stackTrace,
                Type = type
            };

            if (_activeRound == 1)
            {
                _firstLogs.Add(log);
            }
            else
            {
                _secondLogs.Add(log);
            }

            if (type == LogType.Error || type == LogType.Exception || type == LogType.Assert || type == LogType.Warning)
            {
                AddKeyLog("Round " + _activeRound + " " + type + ": " + condition);
            }
        }

        private void EvaluateResult()
        {
            _firstRound.CountLogs(_firstLogs);
            _secondRound.CountLogs(_secondLogs);

            var hasBlockingFailure =
                _firstRound.ErrorCount > 0 ||
                _firstRound.ExceptionCount > 0 ||
                _firstRound.AssertCount > 0 ||
                _secondRound.ErrorCount > 0 ||
                _secondRound.ExceptionCount > 0 ||
                _secondRound.AssertCount > 0;

            var hasWarningsOnly =
                !hasBlockingFailure &&
                (_firstRound.WarningCount > 0 || _secondRound.WarningCount > 0);

            if (hasBlockingFailure)
            {
                _result = "FAIL";
                _failureReason = "At least one Error, Exception, or Assert log was captured.";
            }
            else if (hasWarningsOnly)
            {
                _result = "FAIL-DIAGNOSTIC";
                _failureReason = "Warning logs were captured without Error, Exception, or Assert logs.";
            }
            else
            {
                _result = "PASS";
                _failureReason = "无";
            }
        }

        private void WriteReport()
        {
            var reportPath = GetReportPath();
            Directory.CreateDirectory(Path.GetDirectoryName(reportPath));
            File.WriteAllText(reportPath, BuildReport(), new UTF8Encoding(false));
            Debug.Log("Fast Enter Play Mode Unity Test verification report generated: " + reportPath);
        }

        private string BuildReport()
        {
            var report = new StringBuilder();
            report.AppendLine("# Phase81 Fast Enter Play Mode Unity Test Runner 验证报告");
            report.AppendLine();
            report.Append("- 验证时间：").Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).AppendLine();
            report.Append("- Unity 版本：").Append(Application.unityVersion).AppendLine();
            report.Append("- 当前分支：").Append(RunGit("rev-parse --abbrev-ref HEAD")).AppendLine();
            report.Append("- 当前 commit：").Append(RunGit("rev-parse --short HEAD")).AppendLine();
            report.Append("- Enter Play Mode Options Enabled：").Append(EditorSettings.enterPlayModeOptionsEnabled ? "1" : "0").AppendLine();
            report.Append("- Enter Play Mode Options：").Append((int)EditorSettings.enterPlayModeOptions).Append(" (").Append(EditorSettings.enterPlayModeOptions).AppendLine(")");
            report.AppendLine("- 执行方式：Unity Test Runner Command Line");
            report.Append("- 第一轮等待秒数：").Append(_firstRound.ElapsedSeconds.ToString("0.00")).AppendLine();
            report.Append("- 第一轮等待帧数：").Append(_firstRound.ElapsedFrames).AppendLine();
            report.Append("- 第一轮 Error / Warning / Exception / Assert 数量：")
                .Append(_firstRound.ErrorCount).Append(" / ")
                .Append(_firstRound.WarningCount).Append(" / ")
                .Append(_firstRound.ExceptionCount).Append(" / ")
                .Append(_firstRound.AssertCount).AppendLine();
            report.Append("- 第二轮等待秒数：").Append(_secondRound.ElapsedSeconds.ToString("0.00")).AppendLine();
            report.Append("- 第二轮等待帧数：").Append(_secondRound.ElapsedFrames).AppendLine();
            report.Append("- 第二轮 Error / Warning / Exception / Assert 数量：")
                .Append(_secondRound.ErrorCount).Append(" / ")
                .Append(_secondRound.WarningCount).Append(" / ")
                .Append(_secondRound.ExceptionCount).Append(" / ")
                .Append(_secondRound.AssertCount).AppendLine();
            report.Append("- 最终结果：").Append(_result).AppendLine();
            report.Append("- 失败原因：").Append(_failureReason).AppendLine();
            report.AppendLine();
            report.AppendLine("## 关键日志");
            report.AppendLine();
            if (_keyLogs.Count == 0)
            {
                report.AppendLine("- 未捕获 Error / Warning / Exception / Assert 日志。");
            }
            else
            {
                foreach (var keyLog in _keyLogs)
                {
                    report.Append("- ").Append(SanitizeMarkdownLine(keyLog)).AppendLine();
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
            report.AppendLine("- 未修改 Phase80 的 TEngine Runtime 修复逻辑。");
            report.AppendLine("- 未恢复旧肉鸽玩法、旧 UI Prefab 或旧视觉资产。");
            return report.ToString();
        }

        private void AddKeyLog(string message)
        {
            if (!string.IsNullOrWhiteSpace(message) && _keyLogs.Count < 40)
            {
                _keyLogs.Add(message);
            }
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

        private struct CapturedLog
        {
            public string Condition;
            public string StackTrace;
            public LogType Type;
        }

        private struct RoundInfo
        {
            public double ElapsedSeconds;
            public int ElapsedFrames;
            public int ErrorCount;
            public int WarningCount;
            public int ExceptionCount;
            public int AssertCount;

            public void CountLogs(List<CapturedLog> logs)
            {
                ErrorCount = 0;
                WarningCount = 0;
                ExceptionCount = 0;
                AssertCount = 0;

                foreach (var log in logs)
                {
                    switch (log.Type)
                    {
                        case LogType.Error:
                            ErrorCount++;
                            break;
                        case LogType.Warning:
                            WarningCount++;
                            break;
                        case LogType.Exception:
                            ExceptionCount++;
                            break;
                        case LogType.Assert:
                            AssertCount++;
                            break;
                    }
                }
            }
        }
    }
}
#endif
