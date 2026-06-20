# Phase 22 交付说明：增元丹临时灵力

> 日期：2026-06-20  
> 阶段：Phase 22  
> 范围：扩展丹药效果模型，并接入增元丹战斗内临时灵力  
> 状态：已通过当前阶段验收

## 1. 结果摘要

本阶段在 Phase 21 的小还丹基础上，把丹药效果从单一“恢复 HP”扩展为可承载不同战斗效果，并接入 GDD 中的增元丹方向。

当前已支持：

- 丹药定义具备效果类型与效果数值。
- 小还丹继续作为恢复类丹药，恢复 10 HP。
- 增元丹作为灵力类丹药，本回合灵力 +2。
- 坊市原型新增增元丹商品，价格 35 灵石。
- 使用增元丹后提升当前战斗灵力，并从丹药槽移除。
- 原型 UI 的丹药按钮支持非恢复类丹药。

## 2. 实际改动清单

- 更新 `CultivationRunReward`：
  - 新增 `PillEffectType`，当前支持 `Heal` 与 `Spirit`。
  - `PillDefinition` 改为使用 `EffectType` 与 `EffectValue`。
  - 保留 `HealAmount`，并新增 `SpiritAmount` 便捷属性。
- 更新 `CultivationSeedData`：
  - 小还丹迁移到通用效果字段。
  - 新增 `SpiritBoostPillItem`，即增元丹。
  - 原型坊市新增增元丹商品。
- 更新 `CultivationRunEngine`：
  - `UsePillInBattle()` 改为按丹药效果类型分发。
  - 恢复类丹药恢复 HP。
  - 灵力类丹药增加当前战斗 `Spirit`。
  - 两类丹药都会消耗槽位并写入战斗日志。
- 更新 `CultivationRunPrototypeUI`：
  - 丹药按钮可用性改为检查通用 `EffectValue`，不再只支持恢复类丹药。
- 更新 EditMode 测试：
  - 覆盖原型坊市中增元丹文本展示。
  - 覆盖购买增元丹并在下一场战斗使用。
  - 覆盖使用后当前灵力 +2、丹药槽消耗、灵石扣费和日志记录。

## 3. 当前覆盖能力

- 丹药系统已具备两种基础战斗效果：恢复 HP 与临时灵力。
- 原型坊市现在包含卡牌、小还丹和增元丹三类商品体验。
- 当前丹药效果分发仍保持在 RunEngine 内，便于后续继续接入解毒丹、破境丹等效果。

## 4. 验证与结果

已执行：

```powershell
dotnet build UnityProject\UnityProject.sln --no-restore
git diff --check
```

结果：

- `dotnet build UnityProject\UnityProject.sln --no-restore`：通过；仍保留既有 AdditionalFile、nullable 注释上下文和 Unity/MCP 程序集版本 warning，无新增编译错误。
- `git diff --check`：通过。

Unity MCP 验收：

- `refresh_unity(scope=all, mode=if_dirty, compile=request, wait_for_ready=true)`：通过；刷新期间出现一次 Unity disconnect/retry，随后恢复 ready。
- `GameLogic.EditModeTests`：79/79 通过。
- `execute_code` 探针：购买并使用增元丹后返回 `PASS:Spirit=5,Pills=0,Stones=5`。
- 最终 `read_console(types=["error","warning"])`：0 条。

## 5. 假设与风险

- 增元丹当前直接增加当前战斗的 `Spirit`，未额外记录“本回合临时灵力来源”；回合结束时现有回合开始逻辑会重置为 `SpiritMax`。
- 目前没有每回合使用丹药次数限制，也没有正式丹药 HUD。
- 解毒丹、破境丹、筑基丹等剩余丹药仍未接入。
