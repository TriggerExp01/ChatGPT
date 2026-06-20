# Phase30 交付说明：宝箱法宝奖励入口

## 结果摘要

本阶段在首轮原型路线中接入“遗迹宝箱”节点，让玩家在闭关调息后可以选择打开宝箱，并稳定获得 1 件法宝后进入石魔首领战。该阶段只扩展 Run 路线、宝箱状态、法宝奖励记录、原型 UI 按钮和 EditMode 覆盖，不引入新美术、不改正式 UI 样式、不扩大到完整地图生成系统。

## 实际改动清单

- `CultivationRunNodeType` 新增 `Chest`，`CultivationRunStatus` 新增 `Chest`。
- `CultivationRunNode` 允许 `Chest` 节点没有敌人。
- `CultivationRunState` 新增 `ChestArtifacts`，用于区分宝箱来源法宝。
- `CultivationRunEngine` 新增 `OpenChest`：
  - 仅允许在 `Chest` 状态调用。
  - 从当前节点 `ArtifactRewardPool` 抽取 1 件法宝。
  - 同步写入 `Artifacts` 与 `ChestArtifacts`。
  - 打开后推进到下一节点。
  - 缺少宝箱法宝池时抛出明确异常。
- `CultivationRunEngine` 的节点进入逻辑支持 `Chest` 状态。
- 法宝奖励生成逻辑允许 `Elite` 与 `Chest` 共享法宝池抽取能力。
- `CultivationSeedData.CreateFirstPrototypeBranchingRoute` 在闭关后新增“遗迹宝箱”分支：
  - 休息后路线顺序为：`0=遗迹宝箱`、`1=山脚坊市`、`2=石魔首领`。
  - 宝箱与坊市结束后都进入石魔首领。
- `CultivationRunPrototypePresenter` 增加宝箱状态说明、宝箱法宝计数和快照字段。
- `CultivationRunPrototypeUI` 增加“打开宝箱”按钮与 `OpenChest()` 操作入口。
- EditMode 测试新增并校准：
  - 引擎层宝箱打开、非宝箱状态拒绝、缺少宝箱池拒绝。
  - Presenter 文案与快照宝箱计数。
  - Prototype UI 打开宝箱并获得法宝后进入首领战。
  - 既有市场和精英测试路线索引同步到新路线拓扑。

## 验证与结果

已执行并通过：

```powershell
dotnet build UnityProject\UnityProject.sln --no-restore
git diff --check
```

`dotnet build` 结果：0 Error，保留既有 `System.Net.Http` / `System.IO.Compression` 版本冲突 warning 与 `USG0001` warning。

Unity MCP 验证：

- `refresh_unity(scope=all, mode=if_dirty, compile=request, wait_for_ready=true)`：通过；期间出现一次可恢复 Unity disconnect/retry，最终 editor ready。
- `GameLogic.EditModeTests`：104/104 通过，0 failed，0 skipped。
- `execute_code` 宝箱链路探针：通过。
  - 首战胜利 -> 跳过奖励 -> 选择闭关 -> 休息 -> 选择宝箱 -> 打开宝箱。
  - 结果：`Artifacts=1`、`ChestArtifacts=1`、`Status=InBattle`、`CurrentNodeType=Elite`、`CurrentNodeId=node_stone_demon_leader`。
- `read_console(types=["error","warning"])`：0 条。

## 假设与风险

- 宝箱当前复用原型法宝池，暂未拆分独立宝箱掉落表。
- 宝箱当前是一次性立即领取入口，没有实现多选一、稀有度、动画或事件文本。
- 原型路线索引已随本阶段调整；后续如果继续扩展地图节点，建议把测试中的路线选择封装成具名 helper，减少硬编码索引维护成本。

## 可选下一步

- Phase31 可继续做“宝箱奖励多选一”或“事件节点基础入口”。
- 若优先增强可玩闭环，也可以先补路线节点命名、路线选择说明和更清晰的 UI 展示。
