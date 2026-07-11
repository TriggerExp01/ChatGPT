# Phase84：真实 Run 统一接入

## 阶段目标

本阶段解决“画面显示的是一套路线，实际业务又是另一套规则”的结构问题。

Editor 快速入口与正式 `Procedure -> GameApp` 入口现在统一进入：

```text
CultivationRunUIService.OpenGameRun
  -> CultivationRunWindow / 运行时 fallback
  -> CultivationRunPrototypeUI
  -> CultivationRunSession
  -> CultivationRunEngine + BattleEngine
  -> CultivationRunState
```

## 已完成

- 删除固定每回合敌人减 14、玩家减 6 的 `CultivationGameFlowController` 假流程。
- 删除独立的 `CultivationRouteRuntimeData`、假节点、假状态和对应测试，避免两份可变业务数据并存。
- 新增纯 C# `CultivationRunSession`：
  - 独占一局 Run 的 `BattleEngine`、`CultivationRunEngine` 和 `CultivationRunState`。
  - 显式创建 13 节点完整分支路线，而不是四节点线性样例。
  - 统一承接出牌、结束回合、丹药、战斗结算、奖励、路线、闭关、坊市、宝箱、秘境与金丹被动命令。
- `CultivationRunPrototypeUI` 只通过 Session 改变状态；Presenter 与 Snapshot 继续负责展示转换。
- 修复重复打开真实 Run UI 会无条件重开一局的问题；同一 UI、Session、RunState 与进度现在会保留。
- 修复禁用 Domain Reload 时，TEngine UI 栈保留已销毁窗口并在第二次 Play Mode 刷新失效对象的问题。
- 六个现有门派均增加固定种子 PlayMode 通关覆盖：剑宗、火云宗、天雷阁、厚土宗、药王谷、魔道。

## 验收结果

- Unity EditMode：`268 / 268 Passed`。
- Fast Enter Play Mode 双轮生命周期：`1 / 1 Passed`，两轮均无 Error、Warning、Exception、Assert。
- Unity PlayMode：`7 / 7 Passed`。
  - 包含正式 UI Service 同一 Session、不回档、真实战斗胜利、奖励与路线选择验证。
  - 包含六门派固定种子当前路线通关。
- Unity Console：`0 Error / 0 Warning`。
- `dotnet build UnityProject\UnityProject.sln --no-restore`：`0 Error`；仅保留既有工具链警告。
- `git diff --check`：通过。

## 当前边界

本阶段证明的是“当前单层 Run 的正式入口、状态与交互已统一”，不是 v1.0 已完成。

- 当前完整分支路线到金丹入门战，共 13 个节点。
- 当前节点枚举没有独立 `Boss` 类型，石魔首领使用 `Elite`，金丹魔修使用普通 `Battle`；正式 Boss 元数据、阶段结算和终局仍需扩展。
- 当前 UI 功能完整但仍是程序化原型布局。下一阶段将以真实 Session/Presenter 为数据源制作主路线与战斗黄金样板，不再绑定旧 Restore 假数据。
- 存档、五境界完整路线、20 难度、局外成长、正式设置/本地化/音频与 Windows Player 发布验收仍属于后续阶段。

## 下一阶段

Phase85 优先制作两个可复用黄金样板：

1. 主路线：只显示当前层路线决策、角色状态、资源和关键背包信息。
2. 战斗：突出敌人意图、玩家资源、手牌和回合操作，移除诊断文本墙。

两个样板必须直接读取 `CultivationRunSession` / Presenter，不新增平行业务状态。
