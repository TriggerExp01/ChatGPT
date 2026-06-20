# Phase 7 交付说明：Run 原型操作界面

> 日期：2026-06-20  
> 阶段：Phase 7  
> 范围：把战斗、奖励、闭关升级和路线选择串成可操作的 Run 原型界面  
> 状态：已通过当前阶段验收

## 1. 结果摘要

本阶段把前六个阶段的纯逻辑能力接入到一个运行时原型 UI：

- 游戏启动入口从单场战斗原型切换为 Run 原型界面。
- Run UI 可展示当前 Run 状态、节点、HP、牌组、敌人和战斗日志。
- 战斗胜利后可进入奖励选择，并支持选择奖励或跳过。
- 奖励后可进入路线选择，并选择普通战斗或闭关分支。
- 闭关节点支持只恢复 HP，也支持选择一张可升级牌的升级分支。
- 闭关升级后可进入石魔首领战，形成可手动操作的纵向切片。

本阶段仍是临时原型 UI，不作为正式视觉样板或最终 Prefab。

## 2. 实际改动清单

- 新增 `CultivationRunPrototypeUI`：
  - 运行时创建 uGUI 原型界面。
  - 展示 Run 状态、节点信息、战斗信息、当前牌组和战斗日志。
  - 提供战斗卡牌按钮、结束回合、战斗结算、奖励选择、跳过奖励、闭关恢复、闭关升级和路线选择入口。
  - 内置一条最小分支原型路线：山门石魔后可选择火蝠洞或闭关调息，最终进入石魔首领。
- 新增 `RunPrototypeSnapshot`：
  - 为测试和 MCP 探针提供稳定状态快照。
- 更新 `GameApp.StartGameLogic()`：
  - 启动 `CultivationRunPrototypeUI`。
- 修正原型 UI 在 EditMode 下的生命周期问题：
  - `CultivationRunPrototypeUI` 和旧 `CultivationBattlePrototypeUI` 只在 Play Mode 调用 `DontDestroyOnLoad`。
- 新增 `CultivationRunPrototypeUITests`：
  - 验证 UI 可打开并创建 Run。
  - 验证 UI 可从战斗结算进入奖励，再进入路线选择和闭关。
  - 验证 UI 可从闭关升级推进到首领战。

## 3. 当前覆盖能力

- 可在 Unity 运行时手动操作一条包含战斗、奖励、路线分支、闭关升级和首领战的最小 Run。
- UI 直接使用当前逻辑层，不引入第二套战斗或 Run 框架。
- 当前界面用于调试和纵向切片验收，后续正式 UI 仍需按视觉规范单独制作。

## 4. 验证与结果

已执行：

```powershell
dotnet build UnityProject\UnityProject.sln --no-restore
git diff --check
```

结果：

- `dotnet build UnityProject\UnityProject.sln --no-restore`：通过；保留既有 Unity/MCP 引用版本警告，无新增编译错误。
- `git diff --check`：通过。

Unity MCP 验收：

- `refresh_unity(mode=force, scope=scripts, compile=request, wait_for_ready=true)`：通过；过程中发生一次 MCP 断连重试，最终恢复并 ready。
- `GameLogic.EditModeTests`：27/27 通过。
- `execute_code` 探针：打开 `CultivationRunPrototypeUI`，强制首战胜利后依次执行结算、跳过奖励、选择闭关路线、闭关升级，最终进入 `石魔首领` 战。
- 最终 `read_console(types=["error","warning"])`：0 条。

## 5. 假设与风险

- 当前 UI 是临时原型，不做正式美术、动画、布局适配或 Prefab 资产化。
- 当前原型路线写在 `CultivationRunPrototypeUI.CreatePrototypeRoute()` 中，后续应迁移到配置或专门的 Run 原型数据。
- 当前 UI 为了快速验证闭环，展示信息偏调试密度，不代表最终用户体验。
- 正式路线图、节点图标、路线连线、奖励卡牌展示和闭关升级界面仍需后续阶段单独制作。

## 6. 下一阶段建议

Phase 8 可继续推进以下之一：

- 把 Run 原型路线数据从 UI 脚本中移出，沉到种子数据或配置层。
- 做正式 Run 调试面板的结构化拆分，降低单脚本体量。
- 补第二层升级树。
- 开始把卡牌、敌人、路线等种子数据迁移到 Luban 配置表。
