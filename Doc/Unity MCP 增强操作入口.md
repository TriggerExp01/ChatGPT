# Unity MCP 增强操作入口

## 目标

本项目不直接修改 `com.coplaydev.unity-mcp` 上游包或 `mcpforunityserver` 的 uv 缓存包。项目内 MCP 增强优先通过 Unity Editor 静态入口实现，避免后续包升级时丢失改动。

当前新增入口：

```text
UnityProject/Assets/Editor/CodexMcpWorkflow/CodexUnityMcpWorkflow.cs
```

## Codex 常用调用

在 Unity MCP `execute_code` 中读取项目健康摘要：

```csharp
return CodexTools.CodexUnityMcpWorkflow.GetHealthSummary();
```

生成并落盘健康报告：

```csharp
return CodexTools.CodexUnityMcpWorkflow.GenerateHealthReportFile();
```

确保打开主场景：

```csharp
return CodexTools.CodexUnityMcpWorkflow.EnsureMainSceneLoaded();
```

推荐 PlayMode 测试过滤：

```csharp
return CodexTools.CodexUnityMcpWorkflow.GetRecommendedPlayModeTestFilter();
```

## Unity 菜单入口

Unity Editor 顶部菜单新增：

```text
Codex/TEngine MCP/生成健康报告
Codex/TEngine MCP/打开报告目录
```

报告默认写入：

```text
Doc/MCPReports
```

## 阶段验收建议流程

1. 通过 MCP 读取 `editor/state`，确认 `ready_for_tools=true`。
2. 调用 `CodexTools.CodexUnityMcpWorkflow.GetHealthSummary()`，快速确认编译、刷新、主场景和控制台状态。
3. 如主场景未加载，调用 `EnsureMainSceneLoaded()`。
4. 运行阶段对应 PlayMode 测试或 `GameLogic.PlayModeTests`。
5. 验收完成后调用 `GenerateHealthReportFile()`，把本次 MCP 环境证据写入 `Doc/MCPReports`。

## 后续增强方向

- 增加“阶段验收报告”入口，自动记录阶段号、测试过滤器、控制台摘要和截图路径。
- 增加“演示截图准备”入口，统一打开主场景、重置 Play Mode 前置条件和推荐截图视角。
- 若确认上游 MCP 工具能力不足，再考虑 fork 或本地 overlay，但必须先评估维护成本。
