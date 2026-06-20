# Phase 20 交付说明：丹药槽最小闭环

> 日期：2026-06-20  
> 阶段：Phase 20  
> 范围：接入 Run 级丹药槽，并支持在坊市购买丹药  
> 状态：已通过当前阶段验收

## 1. 结果摘要

本阶段把设计文档中的“丹药不占牌库位置，单独丹药槽，每 Run 最多携带 3 颗”接入到当前 Run 原型。

当前已支持：

- Run 状态持有独立丹药槽，默认上限为 3。
- 坊市商品可区分卡牌商品和丹药商品。
- 购买卡牌仍加入牌组。
- 购买丹药加入丹药槽，不改变牌组数量。
- 丹药槽满时拒绝继续购买丹药，且不扣灵石、不移除商品。
- 原型 Presenter 与 uGUI 原型界面显示丹药数量和丹药商品。

## 2. 实际改动清单

- 更新 `CultivationRunReward`：
  - 新增 `PillDefinition`。
  - 扩展 `CultivationMarketItem`，支持卡牌或丹药二选一载荷。
  - 新增 `IsCard`、`IsPill` 判定。
- 更新 `CultivationRunState`：
  - 新增 `DefaultPillSlotLimit = 3`。
  - 新增 `PillSlotLimit`、`Pills`、`PurchasedMarketPills`。
- 更新 `CultivationRunEngine`：
  - `BuyMarketItem()` 支持购买丹药。
  - 丹药槽满时提前拒绝购买，避免产生扣费或商品移除副作用。
  - `GetMarketSellValue()` 只按卡牌商品估值，避免丹药价格影响卡牌回收价。
- 更新 `BattleEngine / CultivationSeedData`：
  - 新增坊市丹药 `small_restore_pill`。
  - 原型坊市商品新增小还丹，价格 15 灵石。
- 更新 `CultivationRunPrototypePresenter`：
  - Run 文本展示 `丹药：当前/上限`。
  - 坊市文本将丹药商品标注为“丹药”。
  - 快照新增丹药计数字段。
- 更新 `CultivationRunPrototypeUI`：
  - 坊市购买按钮区分卡牌摘要与丹药描述。
  - 丹药槽满时禁用丹药购买按钮。
- 更新 EditMode 测试：
  - 覆盖购买丹药不改变牌组。
  - 覆盖丹药槽满时拒绝购买且不产生副作用。
  - 覆盖 Presenter 文本与快照。
  - 覆盖原型 UI 可购买丹药。

## 3. 当前覆盖能力

- 丹药已经成为 Run 级独立资源，不再需要伪装成卡牌进入牌组。
- 坊市商品模型可以承载不同商品类型，为后续法宝、丹药、事件道具等非卡牌商品留出基础结构。
- 当前只完成“携带与购买”闭环，尚未接入战斗内使用丹药。

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
- `GameLogic.EditModeTests`：72/72 通过。
- `execute_code` 探针：购买 `small_restore_pill` 后返回 `PASS:Status=Market,Stones=5,Deck=12,Pills=1/3,PurchasedPills=1`。
- 最终 `read_console(types=["error","warning"])`：0 条。

## 5. 假设与风险

- 当前阶段不实现战斗内使用丹药；小还丹描述中的“恢复 10 HP”先作为后续战斗消耗品效果约定。
- 奖励池里原有的 `SmallRestorePill` 卡牌暂不迁移，避免扩大到奖励系统重构；本阶段只把坊市原型商品中的小还丹接入为丹药。
- 原型 UI 仍是功能验证界面，正式坊市界面和丹药槽视觉需要后续单独设计与验收。
