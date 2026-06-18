using System;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CodexTools
{
    public static class CodexUnityMcpWorkflow
    {
        private const string MainScenePath = "Assets/Scenes/main.unity";
        private const string PlayModeAssemblyName = "GameLogic.PlayModeTests";
        private const string ReportFolderFromRepoRoot = "Doc/MCPReports";

        [MenuItem("Codex/TEngine MCP/生成健康报告")]
        public static void GenerateHealthReportMenu()
        {
            var path = GenerateHealthReportFile();
            Debug.Log("Codex Unity MCP health report generated: " + path);
        }

        [MenuItem("Codex/TEngine MCP/打开报告目录")]
        public static void OpenReportFolder()
        {
            var reportDirectory = GetReportDirectory();
            Directory.CreateDirectory(reportDirectory);
            EditorUtility.RevealInFinder(reportDirectory);
        }

        public static string GenerateHealthReportFile()
        {
            return WriteReport(BuildHealthReport());
        }

        public static string GenerateHealthReportText()
        {
            return BuildHealthReport();
        }

        public static string GetHealthSummary()
        {
            var scene = SceneManager.GetActiveScene();
            var console = TryGetConsoleCounts();
            var summary = new StringBuilder();
            summary.Append("ready_for_tools_assumption=").Append(IsEditorReadyForMcp()).AppendLine();
            summary.Append("is_playing=").Append(EditorApplication.isPlaying).AppendLine();
            summary.Append("is_compiling=").Append(EditorApplication.isCompiling).AppendLine();
            summary.Append("is_updating=").Append(EditorApplication.isUpdating).AppendLine();
            summary.Append("active_scene=").Append(scene.path).AppendLine();
            summary.Append("main_scene_loaded=").Append(string.Equals(scene.path, MainScenePath, StringComparison.OrdinalIgnoreCase)).AppendLine();
            summary.Append("console_errors=").Append(console.errorCount).AppendLine();
            summary.Append("console_warnings=").Append(console.warningCount).AppendLine();
            summary.Append("recommended_playmode_filter=").Append(GetRecommendedPlayModeTestFilter()).AppendLine();
            return summary.ToString();
        }

        public static string GetRecommendedPlayModeTestFilter()
        {
            return PlayModeAssemblyName;
        }

        public static string EnsureMainSceneLoaded()
        {
            if (EditorApplication.isPlaying)
            {
                return "Cannot load main scene while editor is in Play Mode.";
            }

            var currentScene = SceneManager.GetActiveScene();
            if (string.Equals(currentScene.path, MainScenePath, StringComparison.OrdinalIgnoreCase))
            {
                return "Main scene is already loaded: " + MainScenePath;
            }

            if (currentScene.isDirty && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return "Main scene load cancelled because current dirty scene was not saved.";
            }

            EditorSceneManager.OpenScene(MainScenePath, OpenSceneMode.Single);
            return "Loaded main scene: " + MainScenePath;
        }

        private static string BuildHealthReport()
        {
            var scene = SceneManager.GetActiveScene();
            var console = TryGetConsoleCounts();
            var testCount = CountPlayModeTestScripts();
            var mainSceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(MainScenePath);

            var report = new StringBuilder();
            report.AppendLine("# Codex Unity MCP 健康报告");
            report.AppendLine();
            report.Append("- 生成时间：").Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).AppendLine();
            report.Append("- Unity 版本：").Append(Application.unityVersion).AppendLine();
            report.Append("- 项目路径：").Append(Directory.GetParent(Application.dataPath).FullName).AppendLine();
            report.Append("- 当前场景：").Append(string.IsNullOrEmpty(scene.path) ? scene.name : scene.path).AppendLine();
            report.Append("- 主场景存在：").Append(mainSceneAsset != null ? "是" : "否").AppendLine();
            report.Append("- 主场景已加载：").Append(string.Equals(scene.path, MainScenePath, StringComparison.OrdinalIgnoreCase) ? "是" : "否").AppendLine();
            report.Append("- Play Mode：").Append(EditorApplication.isPlaying ? "运行中" : "未运行").AppendLine();
            report.Append("- 正在编译：").Append(EditorApplication.isCompiling ? "是" : "否").AppendLine();
            report.Append("- AssetDatabase 更新中：").Append(EditorApplication.isUpdating ? "是" : "否").AppendLine();
            report.Append("- 控制台错误数：").Append(console.errorCount).AppendLine();
            report.Append("- 控制台警告数：").Append(console.warningCount).AppendLine();
            report.Append("- PlayMode 测试脚本数：").Append(testCount).AppendLine();
            report.Append("- 推荐 PlayMode 测试过滤：").Append(GetRecommendedPlayModeTestFilter()).AppendLine();
            report.Append("- MCP 操作建议：").Append(IsEditorReadyForMcp() ? "可以执行只读检查和阶段验收工具" : "等待编译、资源刷新或退出 Play Mode 后再执行").AppendLine();
            report.AppendLine();
            report.AppendLine("## Codex 调用入口");
            report.AppendLine();
            report.AppendLine("在 Unity MCP `execute_code` 中可直接调用：");
            report.AppendLine();
            report.AppendLine("```csharp");
            report.AppendLine("return CodexTools.CodexUnityMcpWorkflow.GetHealthSummary();");
            report.AppendLine("```");
            report.AppendLine();
            report.AppendLine("需要落盘报告时调用：");
            report.AppendLine();
            report.AppendLine("```csharp");
            report.AppendLine("return CodexTools.CodexUnityMcpWorkflow.GenerateHealthReportFile();");
            report.AppendLine("```");
            report.AppendLine();
            report.AppendLine("需要确保主场景打开时调用：");
            report.AppendLine();
            report.AppendLine("```csharp");
            report.AppendLine("return CodexTools.CodexUnityMcpWorkflow.EnsureMainSceneLoaded();");
            report.AppendLine("```");
            return report.ToString();
        }

        private static bool IsEditorReadyForMcp()
        {
            return !EditorApplication.isCompiling && !EditorApplication.isUpdating && !EditorApplication.isPlayingOrWillChangePlaymode;
        }

        private static string WriteReport(string report)
        {
            var directory = GetReportDirectory();
            Directory.CreateDirectory(directory);
            var fileName = "unity-mcp-health-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".md";
            var path = Path.Combine(directory, fileName);
            File.WriteAllText(path, report, new UTF8Encoding(false));
            AssetDatabase.Refresh();
            return path;
        }

        private static string GetReportDirectory()
        {
            var unityProjectRoot = Directory.GetParent(Application.dataPath).FullName;
            var repoRoot = Directory.GetParent(unityProjectRoot).FullName;
            return Path.Combine(repoRoot, ReportFolderFromRepoRoot);
        }

        private static int CountPlayModeTestScripts()
        {
            var guids = AssetDatabase.FindAssets("t:MonoScript", new[] { "Assets/GameScripts/Tests/PlayMode" });
            return guids == null ? 0 : guids.Length;
        }

        private static (int errorCount, int warningCount) TryGetConsoleCounts()
        {
            var logEntriesType = Type.GetType("UnityEditor.LogEntries,UnityEditor");
            var logEntryType = Type.GetType("UnityEditor.LogEntry,UnityEditor");
            if (logEntriesType == null || logEntryType == null)
            {
                return (0, 0);
            }

            var startGettingEntries = logEntriesType.GetMethod("StartGettingEntries", BindingFlags.Public | BindingFlags.Static);
            var endGettingEntries = logEntriesType.GetMethod("EndGettingEntries", BindingFlags.Public | BindingFlags.Static);
            var getCount = logEntriesType.GetMethod("GetCount", BindingFlags.Public | BindingFlags.Static);
            var getEntryInternal = logEntriesType.GetMethod("GetEntryInternal", BindingFlags.Public | BindingFlags.Static);
            var modeField = logEntryType.GetField("mode", BindingFlags.Instance | BindingFlags.Public);
            if (startGettingEntries == null || endGettingEntries == null || getCount == null || getEntryInternal == null || modeField == null)
            {
                return (0, 0);
            }

            var entry = Activator.CreateInstance(logEntryType);
            var errorCount = 0;
            var warningCount = 0;

            try
            {
                startGettingEntries.Invoke(null, null);
                var count = (int)getCount.Invoke(null, null);
                for (var i = 0; i < count; i++)
                {
                    getEntryInternal.Invoke(null, new[] { (object)i, entry });
                    var mode = (int)modeField.GetValue(entry);
                    if ((mode & 0x40) != 0 || (mode & 0x400) != 0 || (mode & 0x800) != 0)
                    {
                        errorCount++;
                    }
                    else if ((mode & 0x2) != 0)
                    {
                        warningCount++;
                    }
                }
            }
            catch (Exception)
            {
                return (0, 0);
            }
            finally
            {
                endGettingEntries.Invoke(null, null);
            }

            return (errorCount, warningCount);
        }
    }
}
