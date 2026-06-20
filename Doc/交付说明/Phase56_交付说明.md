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
  - 修正 `ChanceDamageWithStun` / `ChanceDamageWithChain` 的参数语义：多段基础伤害固定使用 `Value`，`FallbackValue` 只作为暴击后眩晕概率或连锁伤害，不再被误当作未暴击伤害。
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
- `git diff --check`
  - 结果：通过。
- Unity MCP
  - `read_console(action="get", types=["error","warning"])`：0 条。
  - `run_tests(EditMode, assembly_names=["GameLogic.EditModeTests"])`：通过，170/170 Passed，0 Failed，0 Skipped。
  - 结论：本阶段代码、测试和 Unity MCP 验收已补齐。

## 假设与风险

- `ChanceDamage` 的 `RepeatCount` 用于多段独立暴击；单段旧卡仍保持原有文本和结算语义。
- `雷霆万钧·晕` 的 `FallbackValue` 承载暴击后眩晕概率，`Duration` 承载眩晕回合数。
- `雷霆万钧·连` 的 `FallbackValue` 承载连锁伤害，`SecondaryValue` 承载每击连锁概率。
- 现有伤害公式仍保持“先扣目标防御再扣 HP”；因此 `雷霆万钧·连` 的 2 点连锁伤害会被石魔 2 点防御完全抵消，但连锁触发日志和目标选择仍会发生。

## 可选下一步

- Phase57：接入天雷阁灵品卡 `雷神之锤`，补“本回合已触发过暴击”的战斗状态追踪和回报牌。
