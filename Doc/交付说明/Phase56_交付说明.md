# Phase56 交付说明 - 雷霆万钧多段暴击

## 结果摘要

本阶段继续补天雷阁灵品中期卡池，接入设计文档中的 `雷霆万钧`。这张卡已具备多段攻击、每击独立暴击、暴击附带眩晕分支、每击概率连锁分支，并进入天雷阁奖励池。

当前实现范围：

- `雷霆万钧`：2 灵，造成 4 伤害 x3，每击独立 10% 概率暴击。
- `雷霆万钧·多`：攻击次数提升到 5 次。
- `雷霆万钧·极`：在多分支上攻击次数提升到 7 次。
- `雷霆万钧·强`：在多分支上每击伤害提升到 6，次数保持 3 次。
- `雷霆万钧·暴`：每击独立暴击率提升到 25%。
- `雷霆万钧·晕`：暴击时额外 20% 概率眩晕。
- `雷霆万钧·连`：每击有 30% 概率连锁 2 伤害。

## 实际改动清单

- 修改 `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/CultivationCardEnums.cs`
  - `CardEffectType` 新增 `ChanceDamageWithStun`。
  - `CardEffectType` 新增 `ChanceDamageWithChain`。
- 修改 `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/BattleEngine.cs`
  - `ChanceDamage` 现在支持 `RepeatCount`，可用于多段独立概率伤害。
  - 新增 `ResolveChanceDamageHits(...)`，统一处理多段暴击、暴击眩晕和每击连锁。
  - `CultivationSeedData` 新增 `ThunderousBarrage`。
  - `CreateThunderSectRewardPool()` 新增 `reward_thunderous_barrage`。
- 修改 `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/CultivationRunPrototypePresenter.cs`
  - 卡牌摘要新增多段暴击、暴击眩晕、每击连锁文本。
- 修改 `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/CultivationBattlePrototypeUI.cs`
  - 旧战斗原型卡牌按钮摘要同步新增多段暴击文本。
- 修改 `UnityProject/Assets/Tests/EditMode/CultivationBattleEngineTests.cs`
  - 新增 `ThunderousBarrageHitsMultipleTimes`。
  - 新增 `ThunderousBarrageCriticalCanStun`。
  - 新增 `ThunderousBarrageChainCanTriggerPerHit`。
- 修改 `UnityProject/Assets/Tests/EditMode/CultivationRunEngineTests.cs`
  - 天雷阁开局测试确认 `雷霆万钧` 进入奖励池但不进入初始牌组。
  - 新增 `ThunderousBarrageFirstLayerUpgradesKeepSecondLayerChoices`，覆盖完整二层升级树。
- 修改 `UnityProject/Assets/Tests/EditMode/CultivationRunPrototypePresenterTests.cs`
  - 概率关键词展示测试新增多段暴击、暴击眩晕、每击连锁文本断言。

## 验证与结果

- `dotnet build UnityProject\UnityProject.sln --no-restore`
  - 结果：通过，0 Error。
  - 备注：仍存在项目既有 warning，包括 `USG0001`、`CS8632`、`System.Net.Http` / `System.IO.Compression` 版本冲突。
- Unity MCP
  - 结果：未通过正式验收。
  - `mcpforunity://editor/state` 可读取，但返回 `ready_for_tools=false`，阻塞原因为 `stale_status`。
  - `read_console(action="get", types=["error"])` 超时。
  - 定向 `run_tests(EditMode)` 超时，目标测试包括：
    - `GameLogic.Tests.CultivationBattleEngineTests.ThunderousBarrageHitsMultipleTimes`
    - `GameLogic.Tests.CultivationBattleEngineTests.ThunderousBarrageCriticalCanStun`
    - `GameLogic.Tests.CultivationRunEngineTests.ThunderousBarrageFirstLayerUpgradesKeepSecondLayerChoices`
  - 结论：本阶段新增代码和测试已通过 C# 编译，但 Unity Test Framework 实跑与控制台无 Error 验收仍待 MCP 恢复后补齐。

## 假设与风险

- `ChanceDamage` 的 `RepeatCount` 用于多段独立暴击；单段旧卡仍保持原有文本和结算语义。
- `雷霆万钧·晕` 的 `FallbackValue` 承载暴击后眩晕概率，`Duration` 承载眩晕回合数。
- `雷霆万钧·连` 的 `FallbackValue` 承载连锁伤害，`SecondaryValue` 承载每击连锁概率。
- 当前仍缺少 Unity Editor 内测试实跑和控制台无 Error 验收，原因是 Unity MCP 连接状态不稳定。

## 可选下一步

- Phase57：接入天雷阁灵品卡 `雷神之锤`，补“本回合已触发过暴击”的战斗状态追踪和回报牌。
- MCP 恢复后补跑 Phase49 到 Phase56 的天雷阁定向 EditMode 测试与完整 `GameLogic.EditModeTests`。
