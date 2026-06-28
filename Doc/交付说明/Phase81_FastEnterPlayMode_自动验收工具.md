# Phase81 Fast Enter Play Mode 自动验收工具交付说明

## 背景

当前 Unity 工程启用了 Enter Play Mode Options：

- `m_EnterPlayModeOptionsEnabled: 1`
- `m_EnterPlayModeOptions: 3`

这表示 Editor Play Mode 禁用了 Domain Reload 和 Scene Reload。Phase80 已经修复 TEngine 在 Fast Enter Play Mode 下的静态状态残留问题，本阶段补充一个可重复执行的 Editor 自动验收入口，用于验证连续两次进入 Play Mode 时不再出现 TEngine 重复初始化相关 Error / Warning。

## 为什么需要这个工具

Fast Enter Play Mode 会让 static 字段跨 Play Mode 生命周期保留，人工点击两次 Play Mode 容易遗漏 Console 清理、等待时长和结果记录。本工具将“清理 Console -> 进入 Play Mode -> 等待启动 -> 退出 -> 再次进入 -> 读取 Console -> 生成报告”的流程固化为一个菜单，后续回归时可以用同一流程复测。

## 菜单入口

Unity 菜单：

```text
Codex/Verify/Fast Enter Play Mode Twice
```

## 验证流程

1. 清理 Unity Console。
2. 第一次进入 Play Mode。
3. 等待固定时间，让启动流程有时间进入主流程。
4. 读取第一次 Play Mode 的 Console Error / Warning 数量。
5. 退出 Play Mode。
6. 再次清理 Unity Console。
7. 第二次进入 Play Mode。
8. 再次等待固定时间。
9. 读取第二次 Play Mode 的 Console Error / Warning 数量。
10. 输出明确的 PASS / FAIL 日志。
11. 退出 Play Mode。
12. 生成 Markdown 验证报告。

## 输出报告路径

```text
Doc/验证报告/Phase81_FastEnterPlayMode_Twice_Verification.md
```

## 已验证结果

工具已实现，并已通过以下静态验证：

- `dotnet build UnityProject\UnityProject.sln --no-restore`：通过，存在既有依赖版本冲突警告，无编译错误。
- `git diff --check`：通过。

本轮曾通过 Unity MCP 刷新 Unity，并尝试执行菜单 `Codex/Verify/Fast Enter Play Mode Twice`。菜单可以被 MCP 触发，但当前 Editor/MCP 会话在 Play Mode 切换阶段未稳定完成两轮状态机，因此本轮不伪造 Unity Play Mode 验证结果，不提交验证报告。

需要在本地 Unity Editor 中重新执行菜单，完成后工具会生成：

```text
Doc/验证报告/Phase81_FastEnterPlayMode_Twice_Verification.md
```

通过时 Console 会输出：

```text
PASS:
Fast Enter Play Mode twice verification passed.
```

失败时 Console 会输出：

```text
FAIL:
Fast Enter Play Mode twice verification failed.
```

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
