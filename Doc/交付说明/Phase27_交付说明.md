# Phase 27 交付说明：法宝系统最小闭环
> 日期：2026-06-20  
> 范围：建立 Run 内永久法宝模型，并接入灵石矿经济法宝  
> 状态：已完成并通过 Unity MCP 验收

## 目标

本阶段开始接入设计文档中的“法宝系统”。法宝是 Run 内永久被动装备，类比遗物；本阶段先做最小可验收骨架，不一次性铺完整 15 件法宝。

首个接入的法宝是 `灵石矿`：

- 设计效果：每场战斗胜利额外获得 5 灵石。
- 获取方式：当前先接入原型坊市。
- 坊市价格：25 灵石。

## 实际改动

- 新增法宝定义模型：
  - `ArtifactDefinition`：法宝 ID、名称、描述、效果类型、效果数值。
  - `ArtifactEffectType.BonusSpiritStonesOnVictory`：胜利额外灵石效果。
- 扩展坊市商品模型：
  - `CultivationMarketItem` 支持卡牌、丹药、法宝三类载荷。
  - 继续保证每个商品只能有一种载荷。
- 扩展 Run 状态：
  - `Artifacts`：当前 Run 已持有法宝。
  - `PurchasedMarketArtifacts`：通过坊市购买的法宝记录。
- 接入 `CultivationSeedData.SpiritStoneMineArtifact`：
  - ID：`spirit_stone_mine`。
  - 名称：灵石矿。
  - 效果：每场战斗胜利额外获得 5 灵石。
  - 原型坊市商品：`market_spirit_stone_mine`，价格 25 灵石。
- 扩展战斗胜利结算：
  - 保留节点基础灵石奖励日志。
  - 持有灵石矿时追加法宝额外灵石奖励。
  - 战斗日志写入“法宝额外获得 5 灵石。”。
- 扩展原型展示：
  - Run 文本显示当前法宝数量。
  - 坊市文本显示“灵石矿（法宝）”。
  - UI 按钮显示法宝描述。
  - Snapshot 暴露 `ArtifactCount` 与 `PurchasedMarketArtifactCount`。
- 补充测试：
  - 引擎层覆盖购买灵石矿、持有法宝、胜利额外灵石和日志。
  - Presenter 覆盖法宝商品文本与法宝数量。
  - 原型 UI 覆盖从坊市购买灵石矿并在后续战斗获得额外经济收益。

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
- Unity MCP `GameLogic.EditModeTests`：92/92 passed。
- Unity MCP `execute_code` 探针通过：
  - 返回 `PASS:Artifacts=1,Stones=25`。
  - 证明购买灵石矿后进入下一场战斗，胜利时获得基础灵石 + 法宝额外 5 灵石。
- Unity MCP `read_console(types=["error","warning"])`：0 条。

## 假设与风险

- 当前只接入灵石矿一个法宝，尚未实现完整 15 件法宝表、稀有度、掉落权重、宝箱或精英战法宝掉落。
- 当前法宝允许重复购买同名法宝；本阶段没有加唯一性限制，后续接入正式法宝池时应统一处理重复策略。
- 灵石矿当前只影响战斗胜利结算，不影响事件、坊市出售或其他经济入口。

## 后续建议

- 下一阶段可继续接入 `回春玉佩`，覆盖“战斗结束后恢复 3 HP”的续航型法宝。
- 也可以先建立法宝掉落入口，把精英战或宝箱节点接入法宝奖励，再继续扩展法宝效果。
