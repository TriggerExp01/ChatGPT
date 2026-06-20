# Phase 26 交付说明：大还丹高效恢复
> 日期：2026-06-20  
> 范围：在既有丹药槽与战斗内使用链路上，接入大还丹的高效恢复效果  
> 状态：已完成并通过 Unity MCP 验收

## 目标

本阶段继续补齐通用丹药池，把设计文档中“大还丹：恢复 15 HP，坊市价格 35 灵石”的效果接入当前 Run 原型。

大还丹沿用 Phase 21-25 已确定的丹药槽原型规则：丹药不进入牌组，不占手牌，不消耗灵力；购买后进入丹药槽，战斗中点击使用并立即消耗槽位。

## 实际改动

- 新增 `CultivationSeedData.BigRestorePillItem`：
  - ID：`big_restore_pill`。
  - 名称：大还丹。
  - 效果：战斗内恢复 15 HP。
- 原型坊市新增 `market_big_restore_pill`：
  - 价格 35 灵石。
  - 插入在小还丹之后，作为灵品恢复丹药。
- 复用既有 `PillEffectType.Heal` 分发：
  - 不新增平行恢复逻辑。
  - 继续通过 `CultivationRunEngine.UsePillInBattle()` 结算恢复、写入战斗日志并移除丹药槽位。
- 更新测试覆盖：
  - 引擎层覆盖购买并使用大还丹。
  - Presenter 覆盖坊市文本展示大还丹。
  - 原型 UI 覆盖从坊市购买大还丹、进入下一场战斗、受伤后恢复 15 HP 并消耗丹药。
  - 因坊市商品数量增加，相关测试索引和商品数量断言同步更新。

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
- Unity MCP `GameLogic.EditModeTests`：89/89 passed。
- Unity MCP `execute_code` 探针通过：
  - 返回 `PASS:HP=45,Pills=0,Stones=5`。
  - 证明购买大还丹后进入下一场战斗，玩家受伤后使用可恢复 15 HP，丹药槽消耗，灵石扣费正确。
- Unity MCP `read_console(types=["error","warning"])`：0 条。

## 假设与风险

- 大还丹当前不消耗灵力，这是沿用 Phase 21 已确认的原型丹药规则；设计文档中的“灵力 2”暂未接入到丹药槽使用成本。
- 大还丹当前只作为坊市商品进入原型，不接入奖励权重、秘境事件和随机库存。
- 坊市商品索引因新增大还丹后移，现有测试已覆盖主要购买路径；后续正式 UI 应避免依赖硬编码索引。

## 后续建议

- 通用丹药池的核心 6 张已基本接入当前原型；后续可转向秘境事件、法宝或丹药正式成本规则。
- 若要让丹药消耗灵力，应单独开阶段，统一处理小还丹、大还丹、增元丹等所有战斗内丹药的费用与按钮可用状态。
