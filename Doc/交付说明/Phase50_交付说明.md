# Phase50 交付说明 - 天雷阁雷击连锁基础

## 结果摘要

本阶段在 Phase49 的概率/暴击基础上，接入天雷阁核心关键词“雷击连锁”的第一版战斗效果，并新增门派卡牌 `雷电链`。现在天雷阁奖励池可以产出一张通过连锁伤害处理多敌人的卡牌，设计文档中的“通过连锁而非直接 AOE”已经具备最小可运行表达。

当前连锁规则为基础版：

- 首目标固定受到基础伤害。
- 每次连锁按概率触发。
- 连锁只会跳到其他存活敌人。
- 同一张牌的一次结算不会重复命中同一个敌人。
- 没有可用目标时停止连锁。

设计稿中“理论无限连锁”“可连锁同一目标”“连锁附带眩晕”等升级分支暂不在本阶段实现。

## 实际改动清单

- 修改 `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/CultivationCardEnums.cs`
  - `CardEffectType` 新增 `ChanceChainDamage`。
- 修改 `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/CardEffect.cs`
  - 新增 `SecondaryValue` 字段，用于承载连锁伤害等第二数值。
  - 构造函数增加 `secondaryValue` 参数与非负校验。
  - 保持既有构造调用兼容。
- 修改 `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/BattleEngine.cs`
  - 新增 `ChanceChainDamage` 解析。
  - 新增 `ResolveChanceChainDamage(...)`。
  - 新增 `LightningChain` / `雷电链` 卡牌。
  - `雷电链` 基础效果：造成 `8` 伤害，`50%` 概率连锁至另一名敌人造成 `4` 伤害。
  - `雷电链·强`：连锁概率提升到 `75%`。
  - `雷电链·广`：首目标伤害提升到 `12`。
  - 天雷阁奖励池新增 `reward_lightning_chain`。
- 修改 `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/CultivationRunPrototypePresenter.cs`
  - `FormatEffect(...)` 新增连锁伤害文本。
- 修改 `UnityProject/Assets/Tests/EditMode/CultivationBattleEngineTests.cs`
  - 新增 `ChanceChainDamageCanHitAdditionalEnemies`。
  - 新增 `ChanceChainDamageCanStopAfterPrimaryTarget`。
- 修改 `UnityProject/Assets/Tests/EditMode/CultivationRunEngineTests.cs`
  - 天雷阁启动测试确认 `雷电链` 不在初始牌组，但在奖励池中。
- 修改 `UnityProject/Assets/Tests/EditMode/CultivationRunPrototypePresenterTests.cs`
  - 概率关键词展示测试增加连锁伤害摘要断言。

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
    - `GameLogic.Tests.CultivationBattleEngineTests.ChanceChainDamageCanHitAdditionalEnemies`
    - `GameLogic.Tests.CultivationBattleEngineTests.ChanceChainDamageCanStopAfterPrimaryTarget`
    - `GameLogic.Tests.CultivationRunPrototypePresenterTests.FormatCardSummaryDescribesChanceKeywords`
    - `GameLogic.Tests.CultivationRunEngineTests.StartRunCanUseThunderSectStarterDeck`
  - 结论：本阶段新增代码和测试已通过 C# 编译，但 Unity Test Framework 实跑与控制台无 Error 验收仍待 MCP 恢复后补齐。

## 假设与风险

- 本阶段的雷击连锁是安全基础版，不支持同一目标重复连锁，也不支持无限循环。
- `SecondaryValue` 当前只用于连锁伤害，后续也可承载其他第二数值，但需要继续保持语义清晰。
- `雷电链` 已进入奖励池，暂未加入天雷阁初始牌组，避免改变 Phase48 已稳定的开局牌组结构。
- Unity MCP 不可用导致本阶段缺少 Unity Editor 内定向 EditMode 测试和控制台验收。

## 可选下一步

- Phase51：补“蓄力”战斗状态，为 `蓄雷术` 接入天雷阁设计文档效果。
- Phase52：给 `雷击符·连`、`天雷咒·连` 等升级分支接入连锁增强。
- Phase53：恢复 Unity MCP 后补跑 Phase49/Phase50 的定向 EditMode 测试与完整 `GameLogic.EditModeTests`。
