# Phase52 交付说明 - 天雷阁条件连锁升级分支

## 结果摘要

本阶段在 Phase50 的雷击连锁和 Phase51 的蓄力基础上，继续补齐天雷阁早期卡牌的二层升级树。现在 `雷击符·强` 可以继续升级为暴击连锁方向，`天雷咒·速` 可以继续升级为眩晕连锁方向，天雷阁的“概率成功后产生额外连锁收益”开始具备可运行表达。

当前条件连锁规则为最小可靠版：

- `雷击符·连`：暴击命中时额外连锁 1 名其他存活敌人。
- `天雷咒·连`：眩晕判定成功时额外连锁 1 名其他存活敌人。
- 概率失败时不触发额外连锁。
- 本阶段条件连锁只连锁 1 次，不做无限连锁。
- 连锁目标不会选择当前主目标。

设计文档中更深的全体连锁、连锁眩晕、可重复命中同一目标等分支暂不在本阶段展开。

## 实际改动清单

- 修改 `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/CultivationCardEnums.cs`
  - `CardEffectType` 新增 `ChainOnChanceDamage`。
  - `CardEffectType` 新增 `ChainOnChanceStun`。
- 修改 `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/BattleEngine.cs`
  - 新增 `ChainOnChanceDamage` 结算：暴击成功后触发一次额外连锁。
  - 新增 `ChainOnChanceStun` 结算：眩晕成功后触发一次额外连锁。
  - 新增 `ChainToOneAdditionalEnemy(...)`，复用单次条件连锁逻辑。
  - `雷击符·强` 增加二层升级：
    - `雷击符·极`：暴击概率提升到 50%。
    - `雷击符·连`：暴击时额外连锁 1 名敌人，造成 7 伤害。
  - `天雷咒·速` 增加二层升级：
    - `天雷咒·连`：眩晕成功时额外连锁 1 名敌人，造成 5 伤害。
    - `天雷咒·晕`：造成 10 伤害，40% 概率眩晕，并施加 1 层破防。
- 修改 `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/CultivationRunPrototypePresenter.cs`
  - 卡牌摘要新增暴击连锁和眩晕连锁文本。
- 修改 `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/CultivationBattlePrototypeUI.cs`
  - 战斗原型卡牌按钮摘要同步新增暴击连锁和眩晕连锁文本。
- 修改 `UnityProject/Assets/Tests/EditMode/CultivationBattleEngineTests.cs`
  - 新增 `ChainOnChanceDamageOnlyChainsWhenCriticalHits`。
  - 新增 `ChainOnChanceStunOnlyChainsWhenStunHits`。
- 修改 `UnityProject/Assets/Tests/EditMode/CultivationRunEngineTests.cs`
  - 新增 `ThunderFirstLayerUpgradesKeepSecondLayerChainChoices`，确认二层升级树可见。
- 修改 `UnityProject/Assets/Tests/EditMode/CultivationRunPrototypePresenterTests.cs`
  - 概率关键词展示测试增加条件连锁文本断言。

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
  - 定向 `run_tests(EditMode)` 返回 `No Unity Editor instances found. Please ensure Unity is running with MCP for Unity bridge.`，目标测试包括：
    - `GameLogic.Tests.CultivationBattleEngineTests.ChainOnChanceDamageOnlyChainsWhenCriticalHits`
    - `GameLogic.Tests.CultivationBattleEngineTests.ChainOnChanceStunOnlyChainsWhenStunHits`
    - `GameLogic.Tests.CultivationRunEngineTests.ThunderFirstLayerUpgradesKeepSecondLayerChainChoices`
    - `GameLogic.Tests.CultivationRunPrototypePresenterTests.FormatCardSummaryDescribesChanceKeywords`
  - 结论：本阶段新增代码和测试已通过 C# 编译，但 Unity Test Framework 实跑与控制台无 Error 验收仍待 MCP 恢复后补齐。

## 假设与风险

- `雷击符·连` 的连锁伤害按暴击伤害 15 的约 50% 取 7 点，符合设计文档“50% 伤害”的基础意图。
- `天雷咒·连` 的连锁伤害按主伤害 10 的 50% 取 5 点。
- 条件连锁本阶段只触发一次，不沿用 `雷电链` 的概率继续连锁循环，避免早期升级过度膨胀。
- Unity MCP 若继续不可用，本阶段仍缺少 Unity Editor 内定向 EditMode 测试和控制台无 Error 验收。

## 可选下一步

- Phase53：补 `雷电链` 二层升级，例如 `雷电链·极`、`雷电链·速` 或 `雷电链·晕`。
- Phase54：恢复 Unity MCP 后补跑 Phase49 到 Phase52 的定向 EditMode 测试与完整 `GameLogic.EditModeTests`。
