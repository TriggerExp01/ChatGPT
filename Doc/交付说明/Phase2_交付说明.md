# Phase 2 交付说明：战斗原型界面接入

> 日期：2026-06-20  
> 阶段：Phase 2  
> 范围：把 Phase 1 `BattleEngine` 接入最小可运行 uGUI 测试界面  
> 状态：已通过当前阶段验收

## 1. 结果摘要

本阶段完成了一个运行时生成的战斗原型界面，用于在 Unity Play Mode 中直接验证“剑宗初始牌组 vs 石魔”的单场战斗闭环。

该界面不是正式美术 UI，也不替代后续 Prefab/UI 黄金样板流程；它的目标是让 Phase 1 纯逻辑从测试代码走到可视化、可点击、可反复重置的开发验证入口。

## 2. 实际改动清单

- 新增 `CultivationBattlePrototypeUI`：
  - 运行时创建 uGUI 根节点、信息面板、手牌按钮、结束回合按钮、重开按钮和战斗日志。
  - 自动绑定 `UIModule.UIRoot` / 场景 `UIRoot` 下的 `Canvas`，缺失时创建临时 Overlay Canvas。
  - 点击手牌按钮会调用 `BattleEngine.PlayCard`，结束回合按钮会调用 `BattleEngine.EndPlayerTurn`。
  - 暴露 `BattlePrototypeSnapshot`，方便 EditMode 和 MCP 验证读取战斗状态。
- 修改 `GameApp.StartGameLogic()`：
  - 正常热更入口走到 `GameApp` 时自动打开 Phase 2 原型 UI。
- 修改 `GameEntry`：
  - 增加仅 Editor 生效的原型启动开关 `useCultivationPrototypeInEditor`，默认开启。
  - 在当前空骨架资源链尚未补齐时，Play Mode 可直接打开战斗原型 UI，避免 YooAsset 空包信息阻塞本阶段验证。
- 修改 `GameLogic.EditModeTests.asmdef`：
  - 使用 GUID 引用 `GameLogic` 与 `TEngine.Runtime`，修复测试程序集里源生成代码找不到 `TEngine` 的问题。
- 扩展 `CultivationBattleEngineTests`：
  - 新增 `PrototypeUICanCreateBattleAndPlayCard`，验证 UI 能创建战斗、读取快照、打出一张手牌并刷新状态。

## 3. 已覆盖能力

- Play Mode 自动创建 `CultivationBattlePrototypeUI`。
- 初始手牌、灵力、敌人名称和敌人 HP 可通过快照读取。
- 打出手牌后，手牌数减少，灵力减少。
- UI 可重置战斗。
- UI 可结束玩家回合并驱动敌人意图结算。
- 无场景/域重载 Play Mode 下，重复打开 UI 会复用并重置已有节点，避免残留空状态。

## 4. 验证与结果

已执行并通过：

```powershell
dotnet build UnityProject\UnityProject.sln --no-restore
git diff --check
```

Unity MCP 验收结果：

- `refresh_unity`：脚本刷新并等待 Editor ready；过程中出现过 MCP 断连恢复，最终 ready。
- `GameLogic.EditModeTests`：10 个 EditMode 测试全部通过。
- Play Mode：找到 1 个 `CultivationBattlePrototypeUI` 节点。
- Play Mode 运行态快照：
  - `playing=True`
  - `parent=UICanvas`
  - `beforeHand=5`
  - `afterHand=4`
  - `beforeSpirit=3`
  - `afterSpirit=2`
  - `enemy=石魔`
- `read_console`：最终检查 0 Error / 0 Warning。

当前 `dotnet build` 仍存在项目既有 warning，包括 AdditionalFile、nullable 注释上下文，以及 Unity/MCP 程序集版本冲突；这些 warning 不阻塞 Phase 2，本阶段未扩大处理。

## 5. 假设与风险

- 该界面是开发验证用的占位界面，不代表最终视觉风格。
- `GameEntry` 中的 Editor 原型入口是当前空骨架阶段的临时通道；后续资源链、热更入口和正式 UI Prefab 就绪后，应关闭或迁移该入口。
- 当前 UI 仍是代码生成 uGUI，没有走 TEngine UI Prefab、资源加载、音效、本地化或动画流程。
- 视觉截图不作为本阶段硬验收标准；本阶段以 MCP 层级、快照、点击后状态变化和控制台结果作为验收依据。

## 6. 下一阶段边界

Phase 3 建议进入“正式 UI 样板前置阶段”：

- 先确认参考图、屏幕布局、色彩和信息层级。
- 产出一张战斗 UI 黄金样板，不直接批量做 Prefab。
- 在样板确认后，再把当前运行时占位 UI 迁移为 TEngine UI 窗口或正式 Prefab。
- 继续保持 `BattleEngine` 为规则源，UI 不复制战斗规则。
