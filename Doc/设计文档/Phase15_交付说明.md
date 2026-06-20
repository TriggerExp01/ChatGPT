# Phase 15 交付说明：战斗胜利灵石奖励

> 日期：2026-06-20  
> 阶段：Phase 15  
> 范围：为 Run 接入最小经济状态，让战斗胜利能够发放灵石  
> 状态：已通过当前阶段验收

## 1. 结果摘要

本阶段为后续坊市、购买丹药、购买法宝和删牌服务补上最小经济骨架：

- Run 状态新增 `SpiritStones`，用于记录当前灵石数量。
- Run 启动时可传入 `initialSpiritStones`，便于测试、调试和后续起始道基效果。
- 战斗节点新增 `SpiritStoneReward`，胜利结算时自动发放到 Run。
- 种子路线中普通战斗奖励 15 灵石，精英战奖励 35 灵石，落在设计文档的目标区间内。
- 原型文本和快照现在会显示灵石数量。

## 2. 实际改动清单

- 更新 `CultivationRunState`：
  - 新增 `SpiritStones` 字段。
  - 构造函数新增 `initialSpiritStones` 可选参数，低于 0 时归零。
- 更新 `CultivationRunNode`：
  - 新增 `SpiritStoneReward` 字段。
  - 构造函数新增可选参数，低于 0 时归零。
- 更新 `CultivationRunEngine`：
  - `StartRun()` 透传 `initialSpiritStones`。
  - 战斗胜利结算时发放当前节点灵石奖励。
  - 奖励发放写入战斗日志，便于原型观察和测试断言。
- 更新 `CultivationSeedData`：
  - 普通战斗节点配置 15 灵石。
  - 精英战斗节点配置 35 灵石。
- 更新 `CultivationRunPrototypePresenter`：
  - Run 文本增加灵石显示。
  - 快照增加 `SpiritStones`。
- 更新 EditMode 测试：
  - 覆盖初始灵石。
  - 覆盖普通战斗胜利发放 15 灵石。
  - 覆盖精英战斗胜利发放 35 灵石。
  - 覆盖失败不发放灵石。
  - 覆盖展示文本与快照可读取灵石。

## 3. 当前覆盖能力

- Run 已经能积累可持久到下一节点的经济资源。
- 战斗路线和闭关路线开始有资源层面的差异：跳过战斗会跳过灵石收入。
- 后续可以在不改战斗核心的前提下继续接入坊市购买、删牌、丹药槽和法宝购买。

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
- `GameLogic.EditModeTests`：45/45 通过。
- `execute_code` 探针：完成普通战斗、普通战斗、闭关、精英战，最终返回 `PASS stones=65, status=Reward`。
- 最终 `read_console(types=["error","warning"])`：0 条。

## 5. 假设与风险

- 当前灵石奖励为固定值，不做 10-20 / 25-40 区间随机；这是为了先稳定经济字段和结算点。
- 尚未实现 Boss 节点和 40-60 灵石奖励区间。
- 灵石目前只增加不消费；消费会在坊市/删牌/购买阶段单独接入。
