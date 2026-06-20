# Phase 25 交付说明：筑基丹 Run 内成长

> 日期：2026-06-20  
> 范围：在既有丹药槽基础上，接入筑基丹的 Run 内永久最大 HP 成长效果  
> 状态：已完成并通过 Unity MCP 验收

## 目标

本阶段继续扩展通用丹药池，把设计文档中“筑基丹：永久增加最大 HP 10（本 Run）”接入当前 Run 原型。

筑基丹与小还丹、增元丹、解毒丹、破境丹不同，它不是战斗内即时效果，而是 Run 级永久成长效果。因此本阶段新增战斗外丹药使用入口：

- 非战斗状态可使用 Run 级丹药。
- 战斗状态拒绝使用 Run 级丹药，避免和战斗内消耗品规则混淆。
- 使用后消耗丹药槽位。

## 实际改动

- 扩展 `CultivationRunState`：
  - `PlayerMaxHp` 改为受控可增长。
  - 新增 `IncreasePlayerMaxHp()`，增加最大 HP，并同步增加当前 HP。
- 扩展丹药定义：
  - `PillEffectType` 新增 `MaxHp`。
  - `PillDefinition` 新增 `MaxHpAmount`、`IsBattleEffect`、`IsRunEffect`。
  - 新增 `CultivationSeedData.FoundationPillItem`，即筑基丹。
  - 原型坊市新增 `market_foundation_pill`，价格 70 灵石。
- 扩展 RunEngine：
  - `UsePillInBattle()` 拒绝非战斗丹药。
  - 新增 `UsePillInRun()`，支持战斗外使用筑基丹。
  - 使用筑基丹后本 Run 最大 HP +10，当前 HP 同步 +10，并消耗丹药。
- 扩展原型 UI：
  - 非战斗状态下在手牌区显示可使用的 Run 级丹药按钮。
  - 使用筑基丹后显示最近一次 Run 丹药消息。
  - 战斗状态下只允许战斗丹药可点击。
- 补充测试：
  - 引擎层覆盖购买筑基丹并使用。
  - 引擎层覆盖战斗中拒绝使用 Run 级丹药且保留丹药。
  - Presenter 覆盖坊市文本展示筑基丹。
  - 原型 UI 覆盖在坊市使用筑基丹后最大 HP 与当前 HP 增长。

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
- Unity MCP `GameLogic.EditModeTests`：87/87 passed。
- Unity MCP `execute_code` 探针通过：
  - 返回 `PASS:Max=110,HP=60,Pills=0,Stones=5`。
  - 证明购买并使用筑基丹后，本 Run 最大 HP +10，当前 HP 同步 +10，丹药槽消耗，灵石余额正确。
- Unity MCP `read_console(types=["error","warning"])`：0 条。

## 假设与风险

- 筑基丹当前只能在非战斗状态使用；这是为了和“战斗内丹药”保持明确边界。
- 当前使用筑基丹时同时增加当前 HP 10，目的是让永久成长立即反映到生存曲线；后续如果需要“只增上限不回血”，可在平衡阶段调整。
- 筑基丹已进入原型坊市，但正式掉落权重、稀有度控制和事件来源仍未接入。

## 后续建议

- 丹药池剩余可继续接入大还丹，补齐通用丹药的高效恢复档位。
- 后续应把 Run 级丹药消息纳入更正式的 Run 日志，而不是只显示在原型 UI 的手牌区。
