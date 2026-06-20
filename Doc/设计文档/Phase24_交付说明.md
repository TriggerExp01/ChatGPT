# Phase 24 交付说明：破境丹整场降费

> 日期：2026-06-20  
> 范围：在既有丹药槽与战斗内使用链路上，接入破境丹的整场战斗灵力消耗降低效果  
> 状态：已完成并通过 Unity MCP 验收

## 目标

本阶段继续扩展通用丹药池，把设计文档中“破境丹：本场战斗所有功法灵力消耗 -1（最低 0）”接入当前 Run 原型。

破境丹是战斗内稀有爆发型丹药，不修改卡牌定义本身，而是在当前 `BattleState` 上记录本场战斗的费用修正。这样可以保证：

- 牌库、手牌、弃牌堆、奖励卡定义不会被永久污染。
- `CanPlay()`、`PlayCard()` 和原型 UI 按钮可用性都使用同一套有效费用。
- 费用最低为 0，不会出现负数灵力消耗。

## 实际改动

- 扩展战斗状态：
  - `BattleState.SpiritCostReduction` 记录本场战斗灵力消耗降低值。
  - `BattleState.AddSpiritCostReduction()` 增加费用修正。
  - `BattleState.GetEffectiveSpiritCost()` 统一计算有效灵力消耗，最低 0。
- 扩展战斗引擎：
  - `BattleEngine.CanPlay()` 使用有效费用判断是否可打出。
  - `BattleEngine.PlayCard()` 使用有效费用扣除灵力。
- 扩展丹药定义：
  - `PillEffectType` 新增 `CostReduction`。
  - `PillDefinition` 新增 `CostReductionAmount` 派生值。
  - 新增 `CultivationSeedData.BreakthroughPillItem`，即破境丹。
  - 原型坊市新增 `market_breakthrough_pill`，价格 90 灵石。
- 扩展丹药效果分发：
  - `CultivationRunEngine.UsePillInBattle()` 支持破境丹。
  - 使用后当前战斗功法灵力消耗 -1，并消耗丹药槽位。
  - 战斗日志写入“使用 破境丹，本场战斗功法灵力消耗 -1。”。
- 优化原型 UI：
  - 战斗文本显示当前“灵力消耗 -X”。
  - 手牌按钮在费用被降低时显示有效费用与原始费用，例如 `灵力 1（原 2）`。

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
- Unity MCP `GameLogic.EditModeTests`：84/84 passed。
- Unity MCP `execute_code` 探针通过：
  - 返回 `PASS:Before=False,After=True,Reduce=1,Spirit=0,Pills=0,Stones=5`。
  - 证明 2 费牌在 1 灵力时原本不可打出，使用破境丹后可打出，并按有效费用扣到 0。

## 假设与风险

- 破境丹当前只降低玩家功法卡费用，不影响敌人意图或非卡牌行为。
- 多次使用同类降费效果会叠加；当前丹药槽和稀有度控制尚未正式接入，后续需要在完整掉落/经济系统里控制获取频率。
- 独立 `CultivationBattlePrototypeUI` 没有丹药入口，本阶段只更新 Run 原型战斗 UI 的有效费用显示。

## 后续建议

- 丹药池剩余条目可继续接入大还丹或筑基丹。
- 若后续引入“本场战斗结束后结算”的更多临时效果，可考虑把 `BattleState` 上的战斗级修正统一收束为专门的战斗修正结构。
