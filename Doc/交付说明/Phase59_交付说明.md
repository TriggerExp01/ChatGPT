# Phase59 交付说明 - 雷遁瞬击与闪避反击

## 结果摘要

本阶段继续补天雷阁灵品中期卡池，接入设计文档中的 `雷遁·瞬击`。本阶段同步补了通用闪避状态：玩家可获得闪避次数，敌人攻击命中前会优先消耗 1 次闪避并免疫该次攻击；若存在闪避反击伤害，则闪避成功时对攻击者造成反击伤害。

当前实现范围：

- `雷遁·瞬击`：1 灵，造成 6 伤害，获得 1 次闪避。
- `雷遁·连击`：伤害提升到 10，获得 1 次闪避。
- `雷遁·乱击`：造成 6 伤害 x2，获得 1 次闪避。
- `雷遁·护击`：造成 10 伤害，获得 2 次闪避。
- `雷遁·闪击`：造成 6 伤害，获得 1 次闪避；闪避成功时反击 5 伤害。
- `雷遁·反击`：闪避反击伤害提升到 8。
- `雷遁·灵击`：灵力消耗降为 0，造成 6 伤害，获得 1 次闪避。

## 实际改动清单

- 修改 `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/CultivationCardEnums.cs`
  - `CardEffectType` 新增 `Dodge`。
  - `CardEffectType` 新增 `DodgeCounter`。
- 修改 `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/BattleState.cs`
  - 新增 `DodgeCharges`。
  - 新增 `DodgeCounterDamage`。
  - 新增 `AddDodgeCharges(...)`、`AddDodgeCounterDamage(...)`、`TryConsumeDodge()`。
- 修改 `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/BattleEngine.cs`
  - 卡牌效果支持获得闪避与闪避反击。
  - 敌人攻击优先检查并消耗闪避。
  - 被闪避的攻击不会造成伤害，也不会附带灼烧、冰冻、眩晕等命中后状态。
  - 闪避成功时会按当前 `DodgeCounterDamage` 对攻击者反击。
  - `CultivationSeedData` 新增 `ThunderDodgeStrike` 及完整二层升级树。
  - `CreateThunderSectRewardPool()` 新增 `reward_thunder_dodge_strike`。
- 修改 `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/CultivationRunPrototypePresenter.cs`
  - 卡牌摘要新增闪避和闪避反击文本。
  - 战斗文本新增当前闪避次数和反击伤害。
- 修改 `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/CultivationBattlePrototypeUI.cs`
  - 原型战斗 UI 快照新增闪避次数和反击伤害。
  - 卡牌按钮摘要新增闪避和闪避反击文本。
- 修改 `UnityProject/Assets/Tests/EditMode/CultivationBattleEngineTests.cs`
  - 新增闪避免疫敌人攻击测试。
  - 新增闪避阻止攻击附带状态测试。
  - 新增闪避反击伤害测试。
- 修改 `UnityProject/Assets/Tests/EditMode/CultivationRunEngineTests.cs`
  - 天雷阁开局测试确认 `雷遁·瞬击` 进入奖励池但不进入初始牌组。
  - 新增 `ThunderDodgeStrikeFirstLayerUpgradesKeepSecondLayerChoices`，覆盖完整二层升级树。
- 修改 `UnityProject/Assets/Tests/EditMode/CultivationRunPrototypePresenterTests.cs`
  - 概率关键词展示测试新增闪避和闪避反击文本断言。
- 修改 `Doc/文档索引.md`
  - 最新阶段交付说明更新为 Phase59。

## 验证与结果

- `dotnet build UnityProject\UnityProject.sln --no-restore`
  - 结果：通过，0 Error。
- `git diff --check`
  - 结果：通过。
- Unity MCP
  - `refresh_unity(mode="force", scope="scripts", compile="request", wait_for_ready=true)`：通过，期间 MCP 短暂断连后自动恢复。
  - `run_tests(EditMode, assembly_names=["GameLogic.EditModeTests"])`：通过，180/180 Passed，0 Failed，0 Skipped。
  - `read_console(action="get", types=["error","warning"])`：0 条。
  - 结论：本阶段代码、测试和 Unity MCP 验收已补齐。

## 假设与风险

- `DodgeCounterDamage` 按战斗状态实现：获得后会在后续每次闪避成功时触发反击，不随单次闪避自动清零。后续若设计要求“只绑定到本次获得的闪避”，需要把反击状态改成按闪避次数配对的队列。
- 闪避只拦截敌人的攻击类意图，不拦截防御、治疗、召唤、蓄力等非攻击意图。
- 被闪避的攻击不会触发攻击附带状态，这是按“没有命中就不附带异常”的规则实现。
- 本阶段没有导入正式 UI 美术资产；用户已提醒后续需要补 UI 资产，建议单独开阶段按 UI 规范和资源许可处理。

## 可选下一步

- Phase60：按 `Doc/设计文档/UI_UX设计规范.md` 和 `Doc/设计文档/美术与音频资源清单.md` 建立当前游戏 UI 资产目录、占位图标/卡框资源和资源来源清单。
