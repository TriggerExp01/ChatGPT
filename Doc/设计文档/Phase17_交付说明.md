# Phase 17 交付说明：坊市删牌服务闭环

> 日期：2026-06-20  
> 阶段：Phase 17  
> 范围：在坊市中花费灵石移除 Run 牌组卡牌  
> 状态：已通过当前阶段验收

## 1. 结果摘要

本阶段在 Phase 16 的坊市购买基础上，补齐第二个灵石消费场景：坊市删牌。

当前 Run 已支持：

- 坊市删牌固定价格为 35 灵石，对应设计文档中的“移除卡牌 / 35 / 固定”。
- 玩家可在 `Market` 状态选择 Run 牌组中的一张牌移除。
- 成功移除会扣除灵石、从 Run 牌组删除该牌，并记录已移除牌。
- 灵石不足、牌组索引非法、试图移除最后一张牌时拒绝操作且不改变状态。
- 原型 UI 在坊市状态下复用下方牌区展示可移除的牌。

## 2. 实际改动清单

- 更新 `CultivationRunState`：
  - 新增 `RemovedMarketCards`，记录坊市删牌历史。
- 更新 `CultivationRunEngine`：
  - 新增 `MarketCardRemovalCost = 35`。
  - 新增 `RemoveDeckCardAtMarket()`。
  - 校验必须处于坊市状态、牌组索引有效、灵石足够，且不会移除最后一张牌。
- 更新 `CultivationRunPrototypePresenter`：
  - Run 文本显示坊市删牌数量。
  - 坊市节点文本显示删牌价格和已移除数量。
  - 快照新增 `RemovedMarketCardCount`。
- 更新 `CultivationRunPrototypeUI`：
  - 新增 `RemoveDeckCardAtMarket()` 交互入口。
  - 坊市状态下方牌区显示“移除”按钮。
  - 灵石不足或牌组只剩 1 张时禁用移除按钮。
- 更新 EditMode 测试：
  - 覆盖删牌扣灵石、删除牌组卡牌和记录已移除牌。
  - 覆盖灵石不足拒绝删牌。
  - 覆盖非法牌组索引拒绝删牌。
  - 覆盖不能移除最后一张牌。
  - 覆盖 Presenter 文本/快照。
  - 覆盖原型 UI 可在坊市删牌。

## 3. 当前覆盖能力

- 灵石现在形成两个可消费方向：购买卡牌扩大构筑、移除卡牌精简构筑。
- 坊市节点从“只买卡”扩展为最小牌组管理节点。
- 删除最后一张牌的保护维持了 Run 战斗初始化的基本不变量。

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
- `GameLogic.EditModeTests`：56/56 通过。
- `execute_code` 探针：进入坊市、移除第 2 张牌，返回 `PASS status=Market, stones=5, deck=4, removed=1`。
- 最终 `read_console(types=["error","warning"])`：0 条。

## 5. 假设与风险

- 本阶段只实现“花费灵石移除卡牌”，不做出售卡牌、半价回收、市场价格波动、免费删牌天赋或删牌次数限制。
- 删除历史当前只记录卡牌定义对象，暂不区分删除来源和交易流水；如后续需要成就、敌人惩罚或经济统计，可扩展为交易记录模型。
- UI 仍是原型 uGUI，不做正式坊市界面视觉方案。
