# Phase58 交付说明 - 雷神之锤暴击回报

## 结果摘要

本阶段继续补天雷阁灵品中期卡池，接入设计文档中的 `雷神之锤`。这张卡已具备“本回合已触发暴击”条件奖励，并进入天雷阁奖励池；完整升级树也已接入跑团升级和战斗摘要。

当前实现范围：

- `雷神之锤`：3 灵，造成 22 伤害；若本回合已触发暴击，额外造成 10 伤害。
- `雷神之锤·强`：暴击奖励提升到 18 伤害。
- `雷神之锤·极`：暴击奖励提升到 26 伤害。
- `雷神之锤·晕`：暴击奖励触发时附加 1 回合眩晕。
- `雷神之锤·稳`：基础伤害提升到 27，暴击奖励保持 10 伤害。
- `雷神之锤·速`：灵力消耗降为 2。
- `雷神之锤·连`：暴击奖励改为连锁全体敌人，各造成 10 伤害。

## 实际改动清单

- 修改 `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/CultivationCardEnums.cs`
  - `CardEffectType` 新增 `DamageAfterCriticalTriggered`。
  - `CardEffectType` 新增 `DamageAfterCriticalTriggeredWithStun`。
  - `CardEffectType` 新增 `DamageAfterCriticalTriggeredChainAll`。
- 修改 `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/BattleState.cs`
  - 新增 `HasTriggeredCriticalThisTurn`。
  - 新增 `MarkCriticalTriggered()` 和 `ResetTurnCriticalState()`。
- 修改 `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/BattleEngine.cs`
  - 玩家回合开始时重置本回合暴击状态。
  - `ChanceDamage`、`ChainOnChanceDamage`、`ChanceDamageWithStun`、`ChanceDamageWithChain` 实际暴击成功时标记状态。
  - 新增条件奖励伤害结算，支持暴击后追加伤害、追加眩晕、奖励连锁全体。
  - `CultivationSeedData` 新增 `ThunderHammer` 及完整二层升级树。
  - `CreateThunderSectRewardPool()` 新增 `reward_thunder_hammer`。
- 修改 `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/CultivationRunPrototypePresenter.cs`
  - 卡牌摘要新增暴击后奖励、眩晕和全体连锁文本。
- 修改 `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/CultivationBattlePrototypeUI.cs`
  - 旧战斗原型卡牌按钮摘要同步新增暴击后奖励文本。
- 修改 `UnityProject/Assets/Tests/EditMode/CultivationBattleEngineTests.cs`
  - 新增本回合暴击状态重置测试。
  - 新增 `雷神之锤` 基础伤害、暴击后奖励、眩晕升级、全体连锁升级行为测试。
- 修改 `UnityProject/Assets/Tests/EditMode/CultivationRunEngineTests.cs`
  - 天雷阁开局测试确认 `雷神之锤` 进入奖励池但不进入初始牌组。
  - 新增 `ThunderHammerFirstLayerUpgradesKeepSecondLayerChoices`，覆盖完整二层升级树。
- 修改 `UnityProject/Assets/Tests/EditMode/CultivationRunPrototypePresenterTests.cs`
  - 概率关键词展示测试新增暴击后奖励、眩晕、奖励连锁全体文本断言。
- 修改 `Doc/文档索引.md`
  - 最新阶段交付说明更新为 Phase58。

## 验证与结果

- `dotnet build UnityProject\UnityProject.sln --no-restore`
  - 结果：通过，0 Error。
- `git diff --check`
  - 结果：通过。
- Unity MCP
  - `refresh_unity(mode="force", scope="scripts", compile="request", wait_for_ready=true)`：通过，期间 MCP 短暂断连后自动恢复。
  - `run_tests(EditMode, assembly_names=["GameLogic.EditModeTests"])`：通过，176/176 Passed，0 Failed，0 Skipped。
  - `read_console(action="get", types=["error","warning"])`：0 条。
  - 结论：本阶段代码、测试和 Unity MCP 验收已补齐。

## 假设与风险

- `雷神之锤·连` 按“暴击奖励变为连锁全体敌人”实现：基础伤害仍打主目标；若本回合已暴击，则奖励伤害对所有存活敌人各结算一次。
- 现有伤害公式仍保持“先扣目标防御再扣 HP”；因此 `雷神之锤` 的 22 面板伤害打石魔时实际造成 20 HP 伤害，10 点奖励实际造成 8 HP 伤害。
- 本阶段只记录“玩家本回合已触发暴击”，不把眩晕、连锁、普通概率成功纳入暴击状态。

## 可选下一步

- Phase59：继续按 `Doc/设计文档/卡牌池设计_天雷阁.md` 接入下一张未实现的天雷阁卡牌，并复用本阶段的暴击回合状态。
