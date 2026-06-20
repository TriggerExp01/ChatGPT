# Phase 18 交付说明：坊市升级卡牌服务闭环

> 日期：2026-06-20  
> 阶段：Phase 18  
> 范围：在坊市中花费灵石升级 Run 牌组卡牌  
> 状态：已通过当前阶段验收

## 1. 结果摘要

本阶段继续扩展坊市经济，让灵石除购买和删牌外，也能用于卡牌升级。

当前 Run 已支持：

- 坊市升级固定价格为 50 灵石，对应设计文档中的“升级卡牌 / 50 / 固定”。
- 玩家可在 `Market` 状态选择 Run 牌组中可升级的卡牌和升级分支。
- 成功升级会扣除灵石、替换 Run 牌组中的卡牌，并记录已升级牌。
- 灵石不足、卡牌不可升级、升级分支索引非法时拒绝操作且不改变状态。
- 原型 UI 在坊市状态下方牌区展示删牌按钮，并为可升级牌额外展示升级按钮。

## 2. 实际改动清单

- 更新 `CultivationRunState`：
  - 新增 `MarketUpgradedCards`，记录坊市升级历史。
- 更新 `CultivationRunEngine`：
  - 新增 `MarketCardUpgradeCost = 50`。
  - 新增 `UpgradeDeckCardAtMarket()`。
  - 抽出升级结果解析逻辑，供闭关升级和坊市升级复用。
- 更新 `CultivationRunPrototypePresenter`：
  - 坊市节点文本显示升级价格和已升级数量。
  - 快照新增 `MarketUpgradedCardCount`。
- 更新 `CultivationRunPrototypeUI`：
  - 新增 `UpgradeDeckCardAtMarket()` 交互入口。
  - 坊市状态下为可升级牌生成升级按钮。
  - 灵石不足时禁用升级按钮。
- 更新 EditMode 测试：
  - 覆盖坊市升级扣灵石、替换卡牌和记录升级历史。
  - 覆盖灵石不足拒绝升级。
  - 覆盖不可升级牌拒绝升级。
  - 覆盖非法升级分支拒绝升级。
  - 覆盖 Presenter 文本/快照。
  - 覆盖原型 UI 可在坊市升级。

## 3. 当前覆盖能力

- 坊市现在提供三类最小经济操作：购买、删牌、升级。
- 闭关升级仍保持免费但消耗节点推进；坊市升级消耗灵石但停留在坊市，形成两种升级来源差异。
- 升级逻辑复用同一套卡牌升级分支校验，避免两处规则分叉。

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
- `GameLogic.EditModeTests`：62/62 通过。
- `execute_code` 探针：进入坊市、升级第 1 张 `sword_qi`，返回 `PASS status=Market, stones=5, card=sword_qi_damage_1, upgraded=1`。
- 最终 `read_console(types=["error","warning"])`：0 条。

## 5. 假设与风险

- 本阶段只实现“花费灵石升级卡牌”，不做升级折扣、升级次数限制、随机分支、三分支法宝或稀有度价格浮动。
- 坊市升级后仍停留在坊市，允许连续交易；后续如果需要限制每次坊市只能升级一次，可在交易历史或节点状态上加约束。
- UI 仍是原型 uGUI，按钮数量会随可升级牌数量增加；正式 UI 需要单独做黄金样板。
