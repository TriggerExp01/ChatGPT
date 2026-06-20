# Phase 19 交付说明：坊市出售卡牌闭环

> 日期：2026-06-20  
> 阶段：Phase 19  
> 范围：在坊市中出售 Run 牌组卡牌并回收灵石  
> 状态：已通过当前阶段验收

## 1. 结果摘要

本阶段继续补齐坊市经济，把设计文档中的“出售卡牌：半价回收”接入 Run 原型。

当前 Run 已支持：

- 玩家可在 `Market` 状态出售 Run 牌组中的一张牌。
- 出售后从 Run 牌组删除该牌，增加灵石，并记录已出售牌。
- 如果被出售牌与当前坊市商品同 ID，则按该商品价格半价回收。
- 如果当前坊市没有同 ID 商品，则按当前坊市最低商品价的一半作为凡品基准回收。
- 试图出售非法索引或最后一张牌时拒绝操作且不改变状态。
- 原型 UI 在坊市状态下方牌区展示出售按钮和本次回收灵石。

## 2. 实际改动清单

- 更新 `CultivationRunState`：
  - 新增 `SoldMarketCards`，记录坊市出售历史。
- 更新 `CultivationRunEngine`：
  - 新增 `GetMarketSellValue()`。
  - 新增 `SellDeckCardAtMarket()`。
  - 抽出坊市牌离开牌组的公共校验，供删牌和出售复用。
- 更新 `CultivationRunPrototypePresenter`：
  - Run 文本显示坊市售牌数量。
  - 坊市节点文本显示“出售卡牌：半价回收”和已出售数量。
  - 快照新增 `SoldMarketCardCount`。
- 更新 `CultivationRunPrototypeUI`：
  - 新增 `SellDeckCardAtMarket()` 交互入口。
  - 坊市状态下为每张牌生成出售按钮。
  - 牌组只剩 1 张时禁用出售按钮。
- 更新 EditMode 测试：
  - 覆盖同 ID 商品按半价回收。
  - 覆盖无同 ID 商品时按最低坊市价格半价回收。
  - 覆盖非法索引和最后一张牌保护。
  - 覆盖 Presenter 文本/快照。
  - 覆盖原型 UI 可在坊市出售牌。

## 3. 当前覆盖能力

- 坊市现在具备购买、出售、删牌、升级四类基础经济操作。
- 灵石可在“战斗获得 -> 坊市消费/回收 -> 牌组变化”之间双向流动。
- 出售与删牌都保护牌组不能为空，避免破坏后续战斗初始化。

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

- `refresh_unity(mode=force, scope=scripts, compile=request, wait_for_ready=true)`：通过；刷新期间出现一次 Unity disconnect/retry，随后恢复 ready。
- `GameLogic.EditModeTests`：68/68 通过。
- `execute_code` 探针：进入坊市、出售 `cloud_guard`，返回 `PASS status=Market, value=10, stones=13, deck=4, sold=1`。
- 最终 `read_console(types=["error","warning"])`：0 条。

## 5. 假设与风险

- 当前卡牌模型尚未接入稀有度字段，因此非商品同 ID 卡牌先使用当前坊市最低商品价作为凡品基准估价。
- 出售后不进入商品列表，不支持回购。
- UI 仍是原型 uGUI，正式坊市界面需要后续单独设计和验收。
