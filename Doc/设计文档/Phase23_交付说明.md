# Phase 23 交付说明：解毒丹净化效果

> 日期：2026-06-20  
> 范围：在既有丹药槽与战斗内使用链路上，接入解毒丹的净化与少量恢复效果  
> 状态：已完成并通过 Unity MCP 验收

## 目标

本阶段继续扩展丹药系统，把设计文档中“解毒丹：清除所有负面状态，恢复 3 HP”的战斗内消耗品效果接入当前 Run 原型。

由于当前战斗核心尚未实现完整“中毒”体系，本阶段将“所有负面状态”限定为现有已经建模、且可在战斗中真实产生或验证的负面状态：

- 玩家身上的灼烧层数与持续回合。
- 玩家身上的破防层数。

## 实际改动

- 扩展 `PillEffectType`：
  - 新增 `Cleanse` 效果类型。
  - `PillDefinition` 新增 `CleanseHealAmount` 只读派生值。
- 扩展战斗单位状态：
  - `CombatantState.ClearNegativeStatuses()` 清除破防、灼烧层数和灼烧回合。
- 接入解毒丹种子数据：
  - 新增 `CultivationSeedData.CleansePillItem`。
  - 原型坊市新增 `market_cleanse_pill`，价格 15 灵石。
- 扩展丹药战斗内效果分发：
  - `CultivationRunEngine.UsePillInBattle()` 支持 `Cleanse`。
  - 使用解毒丹后清除玩家负面状态，恢复 3 HP，并消耗丹药槽位。
  - 战斗日志写入“使用 解毒丹，清除负面状态并恢复 3 HP。”。
- 优化原型文本可见性：
  - 战斗文本中显示玩家侧破防与灼烧状态，便于验证净化效果。
- 补充测试：
  - 引擎层覆盖购买并使用解毒丹。
  - Presenter 覆盖坊市文本展示解毒丹。
  - 原型 UI 覆盖从坊市购买解毒丹、进入下一场战斗、清除负面状态并恢复 HP。

## 验收结果

已执行：

```bash
dotnet build UnityProject\UnityProject.sln --no-restore
git diff --check
```

结果：

- `dotnet build` 成功，0 Error；保留项目既有 warning。
- `git diff --check` 通过；仅出现 Git 换行提示。
- Unity MCP `refresh_unity(scope=all, mode=if_dirty, compile=request, wait_for_ready=true)` 成功，期间发生一次可恢复断连，最终 Editor ready。
- Unity MCP `GameLogic.EditModeTests`：81/81 passed。
- Unity MCP `execute_code` 探针通过：
  - 返回 `PASS:HP=47,Burn=0/0,Break=0,Pills=0,Stones=5`。
- Unity MCP `read_console(types=["error","warning"])`：0 条。

## 假设与风险

- 当前没有完整中毒系统，所以解毒丹暂不处理“中毒”专属字段；后续接入中毒后，应把中毒层数/回合纳入 `ClearNegativeStatuses()`。
- 解毒丹目前不消耗灵力、不限制每回合使用次数，沿用 Phase 21/22 的丹药使用规则。
- 正式丹药 HUD、丹药稀有度权重和奖励池投放仍未制作，本阶段只完成原型坊市与战斗效果闭环。

## 后续建议

- 下一阶段可继续接入丹药池剩余条目，优先做规则明确且不依赖新 UI 的破境丹或回春丹。
- 若要扩展“毒”玩法，应先单独建立中毒状态、回合结算和敌我双方显示规则，再让解毒丹覆盖该状态。
