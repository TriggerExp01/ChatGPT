# Phase51 交付说明 - 天雷阁蓄雷术与蓄力基础

## 结果摘要

本阶段按天雷阁设计文档接入“蓄力”战斗状态，并新增天雷阁卡牌 `蓄雷术`。现在玩家可以通过 `蓄雷术` 暂停当前输出，获得护盾，并让下一次真实攻击伤害翻倍。

当前蓄力规则为最小可靠版：

- `蓄雷术` 本身不造成伤害。
- 打出后获得 4 护盾，并记录 1 次 `x2` 蓄力伤害。
- 下一次真实攻击伤害消耗蓄力倍率。
- 护盾、抽牌、治疗等非伤害效果不会消耗蓄力。
- 普通伤害、概率伤害、雷击连锁首目标伤害都会使用蓄力倍率。
- 雷击连锁的后续连锁伤害不消耗也不继承本次蓄力，本阶段明确只强化首个攻击伤害。

设计文档中更深层的蓄力联动、跨多段完整覆盖、连锁继承或专属特效表现暂不在本阶段实现。

## 实际改动清单

- 修改 `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/CultivationCardEnums.cs`
  - `CardEffectType` 新增 `ChargeDamage`。
- 修改 `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/BattleState.cs`
  - 新增 `ChargedDamageMultiplier` 和 `ChargedDamageUses`。
  - 新增 `AddChargedDamage(...)`。
  - 新增 `TryConsumeChargedDamageMultiplier()`。
- 修改 `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/BattleEngine.cs`
  - 新增 `ChargeDamage` 解析。
  - 普通伤害入口 `DealCardDamage(...)` 支持蓄力倍率。
  - `ChanceDamage` 支持蓄力倍率。
  - `ResolveChanceChainDamage(...)` 首目标伤害支持蓄力倍率，后续连锁不继承倍率。
  - 新增 `ThunderCharge` / `蓄雷术` 卡牌。
  - 新增升级分支：
    - `蓄雷大法`：1 灵，下次攻击伤害翻倍，获得 8 护盾。
    - `蓄雷速发`：0 灵，下次攻击伤害翻倍，获得 4 护盾。
  - 天雷阁奖励池新增 `reward_thunder_charge`。
  - 天雷阁初始牌组不加入 `蓄雷术`，保持 Phase48 开局结构稳定。
- 修改 `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/CultivationRunPrototypePresenter.cs`
  - `FormatEffect(...)` 新增蓄力文本。
  - 战斗文本新增当前蓄力倍率和剩余次数展示。
- 修改 `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/CultivationBattlePrototypeUI.cs`
  - 战斗原型卡牌摘要同步支持概率、连锁和蓄力文本，避免新效果只显示枚举名。
- 修改 `UnityProject/Assets/Tests/EditMode/CultivationBattleEngineTests.cs`
  - 新增 `ChargeDamageDoublesNextAttackAndThenExpires`。
  - 新增 `ChargeDamageDoesNotAffectNonDamageCards`。
  - 新增 `ChanceDamageUsesChargeMultiplierOnResolvedDamage`。
  - 新增 `ChanceChainDamageUsesChargeOnPrimaryDamageOnly`。
- 修改 `UnityProject/Assets/Tests/EditMode/CultivationRunEngineTests.cs`
  - 天雷阁启动测试确认 `蓄雷术` 不在初始牌组，但在奖励池中。
- 修改 `UnityProject/Assets/Tests/EditMode/CultivationRunPrototypePresenterTests.cs`
  - 概率关键词展示测试增加蓄力摘要断言。

## 验证与结果

- `dotnet build UnityProject\UnityProject.sln --no-restore`
  - 结果：通过，0 Error。
  - 备注：仍存在项目既有 warning，包括 `USG0001`、`CS8632`、`System.Net.Http` / `System.IO.Compression` 版本冲突。
- `git diff --check`
  - 结果：通过，退出码 0。
  - 备注：仅有 Git 提示 LF 将在下次触碰时转换为 CRLF。
- Unity MCP
  - 结果：未通过正式验收。
  - `mcpforunity://editor/state` 返回 `ready_for_tools=false`，阻塞原因为 `stale_status`。
  - `read_console(action="get", types=["error"])` 超时。
  - `manage_scene(action="get_active")` 超时。
  - 定向 `run_tests(EditMode)` 超时，目标测试包括：
    - `GameLogic.Tests.CultivationBattleEngineTests.ChargeDamageDoublesNextAttackAndThenExpires`
    - `GameLogic.Tests.CultivationBattleEngineTests.ChargeDamageDoesNotAffectNonDamageCards`
    - `GameLogic.Tests.CultivationBattleEngineTests.ChanceDamageUsesChargeMultiplierOnResolvedDamage`
    - `GameLogic.Tests.CultivationBattleEngineTests.ChanceChainDamageUsesChargeOnPrimaryDamageOnly`
    - `GameLogic.Tests.CultivationRunEngineTests.StartRunCanUseThunderSectStarterDeck`
    - `GameLogic.Tests.CultivationRunPrototypePresenterTests.FormatCardSummaryDescribesChanceKeywords`
  - 结论：本阶段新增代码和测试已通过 C# 编译，但 Unity Test Framework 实跑与控制台无 Error 验收仍待 MCP 恢复后补齐。

## 假设与风险

- 本阶段把“下次攻击伤害翻倍”定义为“下一次真实攻击伤害实例翻倍”，多段伤害只消耗第一个伤害实例。
- 雷击连锁只让首目标伤害享受蓄力倍率，后续连锁维持原始伤害，避免一次蓄力在多敌人场景中过度放大。
- `蓄雷术` 进入天雷阁奖励池，但不进入初始牌组，避免破坏当前已稳定的天雷阁开局体验。
- Unity MCP 若继续不可用，本阶段仍缺少 Unity Editor 内定向 EditMode 测试和控制台无 Error 验收。

## 可选下一步

- Phase52：补 `雷击符·连`、`天雷咒·连` 等天雷阁连锁升级分支。
- Phase53：恢复 Unity MCP 后补跑 Phase49 到 Phase51 的定向 EditMode 测试与完整 `GameLogic.EditModeTests`。
