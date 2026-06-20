# Phase 16 交付说明：最小坊市购买闭环

> 日期：2026-06-20  
> 阶段：Phase 16  
> 范围：接入最小坊市节点，让灵石可以购买卡牌  
> 状态：已通过当前阶段验收

## 1. 结果摘要

本阶段在 Phase 15 的灵石经济基础上，补齐第一个灵石消费场景：坊市购买。

当前 Run 已支持：

- 新增 `Market` 节点类型和 `Market` Run 状态。
- 坊市节点携带固定商品列表。
- 商品包含 id、卡牌和价格。
- 玩家可在坊市中购买商品，扣除灵石并将卡牌加入 Run 牌组。
- 灵石不足时拒绝购买，不改变牌组和已购记录。
- 玩家可离开坊市并推进到后续节点。
- 原型 UI 可显示并点击购买坊市商品。

## 2. 实际改动清单

- 更新 `CultivationCardEnums`：
  - 新增 `CultivationRunNodeType.Market`。
  - 新增 `CultivationRunStatus.Market`。
- 更新 `CultivationRunReward.cs`：
  - 新增轻量商品模型 `CultivationMarketItem`。
- 更新 `CultivationRunNode`：
  - 新增 `MarketItems`。
  - 非战斗节点校验允许 `Rest` 和 `Market` 不配置敌人。
- 更新 `CultivationRunState`：
  - 新增 `CurrentMarketItems`。
  - 新增 `PurchasedMarketItems`。
- 更新 `CultivationRunEngine`：
  - 进入坊市节点时加载商品并切换到 `Market` 状态。
  - 新增 `BuyMarketItem()`。
  - 新增 `LeaveMarket()`。
  - 推进节点时清理当前坊市商品。
- 更新 `CultivationSeedData`：
  - 新增 `CreatePrototypeMarketItems()`。
  - 分支原型路线加入 `山脚坊市`，闭关后可选择直接打精英或先去坊市。
- 更新 `CultivationRunPrototypePresenter` 与 `CultivationRunPrototypeUI`：
  - 文本显示坊市商品。
  - 快照包含当前商品数与已购买商品数。
  - UI 生成购买按钮和离开坊市按钮。
- 更新 EditMode 测试：
  - 覆盖坊市进入与商品加载。
  - 覆盖购买扣灵石、加牌、记录已购和移除商品。
  - 覆盖余额不足拒绝购买。
  - 覆盖离开坊市进入下一节点。
  - 覆盖文本和 UI 原型可走通坊市购买。

## 3. 当前覆盖能力

- 灵石已经形成“战斗获得 -> 坊市消费 -> 牌组变化”的最小闭环。
- 分支路线出现了第一个非战斗/非闭关资源选择：先去坊市会增加构筑质量，但也需要已有灵石支持。
- 后续可继续扩展删牌、商品价格权重、丹药槽、法宝和市场刷新。

## 4. 验证与结果

已执行：

```powershell
dotnet build UnityProject\UnityProject.sln --no-restore
git diff --check
```

结果：

- `dotnet build UnityProject\UnityProject.sln --no-restore`：通过；保留既有 AdditionalFile、nullable 注释上下文和 Unity/MCP 程序集版本 warning，无新增编译错误。
- `git diff --check`：通过。

Unity MCP 验收：

- `refresh_unity(mode=force, scope=scripts, compile=request, wait_for_ready=true)`：通过。
- `GameLogic.EditModeTests`：50/50 通过。
- `execute_code` 探针：进入坊市、购买 `market_cloud_guard`，返回 `PASS status=Market, stones=10, deck=6, bought=1`。
- 最终 `read_console(types=["error","warning"])`：0 条。

## 5. 假设与风险

- 坊市商品目前是固定列表，不做随机库存和稀有度权重。
- 当前商品类型仍是卡牌；丹药槽、法宝装备和删牌服务还没有接入。
- 市场购买后停留在坊市，允许连续购买；离开坊市需要单独操作。
- 市场商品模型先放在 `CultivationRunReward.cs` 同文件中，避免 Unity 工程生成未及时纳入新脚本导致编译失败；后续如脚本导入稳定，可再拆成单独文件。
