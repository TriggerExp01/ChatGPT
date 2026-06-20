# Phase54 交付说明 - 雷电链·多重复目标连锁

## 结果摘要

本阶段补齐 Phase53 暂缓的 `雷电链·多` 分支。现在 `雷电链·强` 的第二层升级不再使用临时的 `雷电链·稳`，而是按设计文档接入“可重复命中同一目标”的连锁变体。

当前实现范围：

- `雷电链·多`：造成 8 伤害，75% 概率继续连锁造成 4 伤害。
- 连锁目标允许重复命中同一个敌人，单敌人场景也能继续触发连锁。
- 为避免 100% 概率或随机连锁造成死循环，本阶段给重复目标连锁加入最大连锁次数，上限为 4 次。
- 旧 `ChanceChainDamage` 的“不重复命中已连锁目标”行为保持不变，`雷电链·极`、`雷电链·速` 和 `雷电链·晕` 不受影响。

## 实际改动清单

- 修改 `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/CultivationCardEnums.cs`
  - `CardEffectType` 新增 `ChanceChainDamageRepeatTarget`。
- 修改 `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/BattleEngine.cs`
  - `ResolveChanceChainDamage(...)` 增加重复目标连锁参数。
  - 重复目标连锁使用 `CardEffect.RepeatCount` 承载最大连锁次数。
  - 当重复目标连锁达到上限时写入战斗日志并停止继续判定。
  - `雷电链·强` 的第二个二层升级替换为：
    - `lightning_chain_chance_2_repeat`
    - `雷电链·多`
    - `ChanceChainDamageRepeatTarget`
    - 最大连锁次数 4。
- 修改 `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/CultivationRunPrototypePresenter.cs`
  - 卡牌摘要新增“可重复目标，最多 N 次”的展示文本。
- 修改 `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/CultivationBattlePrototypeUI.cs`
  - 旧战斗原型卡牌按钮摘要同步新增重复目标连锁文本。
- 修改 `UnityProject/Assets/Tests/EditMode/CultivationBattleEngineTests.cs`
  - 新增 `ChanceChainDamageRepeatTargetCanHitSameEnemyUntilLimit`，覆盖单敌人重复命中和上限停止。
- 修改 `UnityProject/Assets/Tests/EditMode/CultivationRunEngineTests.cs`
  - 更新 `LightningChainFirstLayerUpgradesKeepSecondLayerChoices`，断言 `雷电链·多` 替代 Phase53 临时 `雷电链·稳`。
- 修改 `UnityProject/Assets/Tests/EditMode/CultivationRunPrototypePresenterTests.cs`
  - 概率关键词展示测试新增重复目标连锁文本断言。

## 验证与结果

- `dotnet build UnityProject\UnityProject.sln --no-restore`
  - 结果：通过，0 Error。
  - 备注：仍存在项目既有 warning，包括 `USG0001`、`CS8632`、`System.Net.Http` / `System.IO.Compression` 版本冲突。
- Unity MCP
  - 结果：未通过正式验收。
  - `mcpforunity://editor/state` 可读取，但返回 `ready_for_tools=false`，阻塞原因为 `stale_status`。
  - `read_console(action="get", types=["error"])` 超时。
  - `manage_scene(action="get_active")` 超时。
  - 定向 `run_tests(EditMode)` 超时，目标测试包括：
    - `GameLogic.Tests.CultivationBattleEngineTests.ChanceChainDamageRepeatTargetCanHitSameEnemyUntilLimit`
    - `GameLogic.Tests.CultivationRunEngineTests.LightningChainFirstLayerUpgradesKeepSecondLayerChoices`
    - `GameLogic.Tests.CultivationRunPrototypePresenterTests.FormatCardSummaryDescribesChanceKeywords`
  - 结论：本阶段新增代码和测试已通过 C# 编译，但 Unity Test Framework 实跑与控制台无 Error 验收仍待 MCP 恢复后补齐。

## 假设与风险

- 设计文档写的是 `雷电链·多` 可连锁同一目标“不限制”。工程实现必须防止无限循环，因此本阶段采用“最多 4 次连锁”的可验收上限。
- `RepeatCount` 在该效果类型中表示“最大连锁次数”，不会影响普通 `Damage` 的多段伤害含义。
- 重复目标连锁只影响 `ChanceChainDamageRepeatTarget`，不改变既有连锁牌的目标排重规则。
- Unity MCP 若继续不可用，本阶段仍缺少 Unity Editor 内定向 EditMode 测试和控制台无 Error 验收。

## 可选下一步

- Phase55：开始接入天雷阁中期卡 `五雷正法`，组合伤害、眩晕与连锁。
- MCP 恢复后补跑 Phase49 到 Phase54 的天雷阁定向 EditMode 测试与完整 `GameLogic.EditModeTests`。
