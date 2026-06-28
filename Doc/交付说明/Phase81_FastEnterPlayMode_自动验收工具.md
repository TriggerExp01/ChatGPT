# Phase81 Fast Enter Play Mode 自动验收工具交付说明

## 背景

当前 Unity 工程启用了 Enter Play Mode Options：

- `m_EnterPlayModeOptionsEnabled: 1`
- `m_EnterPlayModeOptions: 3`

这表示 Editor Play Mode 禁用了 Domain Reload 和 Scene Reload。Phase80 已经修复 TEngine 在 Fast Enter Play Mode 下的静态状态残留问题，本阶段补充一个可重复执行的 Editor 自动验收入口，用于验证连续两次进入 Play Mode 时不再出现 TEngine 重复初始化相关 Error / Warning。

## 为什么需要这个工具

Fast Enter Play Mode 会让 static 字段跨 Play Mode 生命周期保留，人工点击两次 Play Mode 容易遗漏 Console 清理、等待时长和结果记录。本工具将“清理 Console -> 进入 Play Mode -> 等待启动 -> 退出 -> 再次进入 -> 读取 Console -> 生成报告”的流程固化为一个菜单，后续回归时可以用同一流程复测。

## 验收入口

Unity Editor 菜单仍保留为调试入口：

```text
Codex/Verify/Fast Enter Play Mode Twice
```

但 MCP 菜单触发会干扰 Play Mode transition，已观察到 `playmode_transition` 卡住、`is_playing=true / is_changing=true`、MCP 自身 `Client handler error: Cannot access a disposed object.` 等现象。因此菜单不再作为最终验收依据，也不要求用户手动打开 Unity 或手动点击菜单。

最终验收改为 Unity 命令行 / Test Runner：

```powershell
powershell -ExecutionPolicy Bypass -File Tools\Verify\RunFastEnterPlayModeTwice.ps1
```

后续 Codex 必须执行该脚本完成验收；如果命令行失败，必须生成诊断报告或输出诊断原因，不能要求用户手动点击。

## 命令行验证流程

1. PowerShell 脚本定位 Unity.exe，优先读取 `UNITY_EXE`，否则使用 Unity 2022.3.17f1 默认安装路径。
2. 使用 Unity Test Runner Command Line 执行 EditMode 测试。
3. 测试通过 `UnityEngine.TestTools.EnterPlayMode` / `ExitPlayMode` 连续执行两轮 Play Mode。
4. 每轮至少等待 10 秒且至少 30 帧。
5. 通过 `Application.logMessageReceived` 区分第一轮和第二轮日志。
6. 统计 Error / Warning / Exception / Assert。
7. 生成 Markdown 验证报告。
8. 若 Unity 命令行失败、XML 未生成或 Markdown 报告未生成，脚本生成命令行诊断报告。

## 输出报告路径

```text
Doc/验证报告/Phase81_FastEnterPlayMode_UnityTest_Verification.md
Doc/验证报告/Phase81_FastEnterPlayMode_UnityTestResults.xml
Doc/验证报告/Phase81_FastEnterPlayMode_UnityTest.log
Doc/验证报告/Phase81_FastEnterPlayMode_CommandLine_Diagnostic.md
```

## 已验证结果

首版工具已能生成报告，但曾在第一次 Play Mode 等待阶段发现 Editor 提前退出：

```text
Editor left first Play Mode before verification wait completed.
```

该结果不能证明 Phase80 的 TEngine 生命周期修复失败，因为当次报告中的 Console Error / Warning 均为 0，且 Console 读取状态为成功。本轮目标改为增强 Phase81 验证工具的状态机与诊断能力，让它能区分 TEngine 运行错误和验证环境/外部退出问题。

本轮增强内容：

- 增加 `PlayModeStateChange` 时间线，记录 `EnteredEditMode`、`ExitingEditMode`、`EnteredPlayMode`、`ExitingPlayMode`。
- 增加工具主动退出标记，用于区分工具自身请求退出和非预期退出。
- 增加非预期退出诊断，不再把提前离开 Play Mode 简单归类为普通失败。
- 等待条件改为最短秒数 + 最短帧数：至少 10 秒且至少 30 帧。
- 无论 PASS / FAIL / FAIL-DIAGNOSTIC / SKIPPED，均生成 Markdown 报告。

工具已实现，并已通过以下静态验证：

- `dotnet build UnityProject\UnityProject.sln --no-restore`：通过，存在既有依赖版本冲突警告，无编译错误。
- `git diff --check`：通过。

本轮进一步新增 Unity Test Runner 命令行验收流程。最终结果以 `Tools/Verify/RunFastEnterPlayModeTwice.ps1` 生成的报告为准，不再以 MCP 菜单触发结果作为最终验收依据。

## 明确未修改范围

- 未修改 UI Prefab。
- 未修改 MainRoute 视觉。
- 未修改修仙玩法逻辑。
- 未修改战斗逻辑。
- 未修改路线逻辑。
- 未修改事件逻辑。
- 未修改奖励逻辑。
- 未修改 Luban。
- 未修改 UIModule。
- 未修改 ProjectSettings。
- 未恢复旧肉鸽玩法、旧 UI Prefab 或旧视觉资产。
