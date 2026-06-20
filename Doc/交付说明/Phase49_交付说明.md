# Phase49 交付说明 - 天雷阁概率与暴击基础

## 结果摘要

本阶段在 Phase48 的天雷阁最小门派基础上，补入第一版可复用的概率判定能力，让天雷阁从“确定性伤害/眩晕占位”向设计文档中的“高方差暴击 + 概率控制”推进。

当前已支持两类概率效果：

- `ChanceDamage`：按概率造成高伤害，失败时造成兜底伤害，用于表达雷击符的暴击。
- `ChanceStun`：按概率施加眩晕，用于表达天雷咒的概率控制。

本阶段只做概率与暴击基础；雷击连锁、蓄力、概率强化仍保留到后续阶段。

## 实际改动清单

- 修改 `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/CultivationCardEnums.cs`
  - `CardEffectType` 新增 `ChanceDamage`。
  - `CardEffectType` 新增 `ChanceStun`。
- 修改 `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/CardEffect.cs`
  - 新增 `ChancePercent` 字段。
  - 新增 `FallbackValue` 字段。
  - 构造函数增加参数校验，概率限制在 `0..100`，兜底值不能为负数。
  - 保持原有构造调用兼容，默认概率为 `100`，兜底值为 `0`。
- 修改 `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/BattleEngine.cs`
  - 新增概率判定逻辑 `RollChance(...)`。
  - `ChanceDamage` 命中时使用 `Value`，未命中时使用 `FallbackValue`。
  - `ChanceStun` 命中时施加眩晕，未命中时只记录未触发日志。
  - `ThunderTalisman` 改为 `30%` 概率造成 `10` 伤害，失败造成 `5` 伤害。
  - `HeavenlyThunderSpell` 改为造成 `10` 伤害，并 `40%` 概率眩晕 `1` 回合。
  - 对应升级分支也改用概率表达。
- 修改 `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/CultivationRunPrototypePresenter.cs`
  - `FormatEffect(...)` 新增概率伤害与概率眩晕文本，原型 UI 可以直接展示概率。
- 修改 `UnityProject/Assets/Tests/EditMode/CultivationBattleEngineTests.cs`
  - 更新天雷阁初始牌组测试，验证 `ChanceDamage` 和 `ChanceStun`。
  - 新增 `ChanceDamageUsesFallbackWhenProbabilityMisses`。
  - 新增 `ChanceDamageUsesCriticalValueWhenProbabilityHits`。
  - 新增 `ChanceStunOnlyAppliesWhenProbabilityHits`。
- 修改 `UnityProject/Assets/Tests/EditMode/CultivationRunPrototypePresenterTests.cs`
  - 新增 `FormatCardSummaryDescribesChanceKeywords`，验证原型 UI 文本能展示概率信息。

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
  - `read_console` 超时。
  - `manage_scene(action="get_active")` 超时。
  - 定向 `run_tests(EditMode)` 超时，目标测试包括：
    - `GameLogic.Tests.CultivationBattleEngineTests.ChanceDamageUsesFallbackWhenProbabilityMisses`
    - `GameLogic.Tests.CultivationBattleEngineTests.ChanceDamageUsesCriticalValueWhenProbabilityHits`
    - `GameLogic.Tests.CultivationBattleEngineTests.ChanceStunOnlyAppliesWhenProbabilityHits`
    - `GameLogic.Tests.CultivationRunPrototypePresenterTests.FormatCardSummaryDescribesChanceKeywords`
  - 结论：本阶段新增代码和测试已通过 C# 编译，但 Unity Test Framework 实跑与控制台无 Error 验收仍待 MCP 恢复后补齐。

## 假设与风险

- `ChanceDamage` 当前只表达“命中高伤害 / 未命中兜底伤害”，还没有全局暴击率、暴击倍率、概率强化等战斗级属性。
- `ChanceStun` 当前只表达单次概率眩晕，不含“失败改破防”“概率被法宝/被动提升”等扩展。
- 现有随机源复用 `BattleEngine` 的种子随机，便于后续写可重复测试；但 Unity Editor 实跑仍未验证。
- 雷击连锁和蓄力尚未实现，天雷阁仍不是完整设计稿状态。

## 可选下一步

- Phase50：补天雷阁“雷击连锁”基础效果，支持对额外敌人造成连锁伤害。
- Phase51：补“蓄力”战斗状态，为蓄雷术接入设计文档效果。
- Phase52：恢复 Unity MCP 后补跑 Phase48/Phase49 的定向 EditMode 测试与完整 `GameLogic.EditModeTests`。
