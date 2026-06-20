# Phase 6 交付说明：最小路线分支选择

> 日期：2026-06-20  
> 阶段：Phase 6  
> 范围：在现有线性 Run 推进基础上，补齐纯逻辑层的路线分支选择  
> 状态：已通过当前阶段验收

## 1. 结果摘要

本阶段把 Run 路线从“只能按数组顺序前进”推进到“节点可声明多个后继，Run 在分叉处等待玩家选择”的最小闭环：

- 节点可配置后继节点索引。
- 默认线性路线保持原行为，不需要显式配置后继。
- 当当前节点有多个后继时，Run 会进入 `RouteChoice` 状态。
- `CurrentRouteChoices` 暴露当前可选目标节点，供后续 UI 展示。
- 玩家选择路线后，Run 进入目标节点并启动对应战斗或闭关。

这对应 GDD 中“地图完全可见，玩家选择路径前进”的中观循环方向，但本阶段只实现逻辑状态机，不做正式地图 UI。

## 2. 实际改动清单

- 扩展 `CultivationRunStatus`：
  - 新增 `RouteChoice`。
- 新增 `CultivationRunRouteChoice`：
  - 表达一个路线选项的目标节点索引和目标节点。
- 扩展 `CultivationRunNode`：
  - 新增 `NextNodeIndices`。
  - 构造时校验后继索引不能为负数。
- 扩展 `CultivationRunState`：
  - 新增 `CurrentRouteChoices`。
- 扩展 `CultivationRunEngine`：
  - 新增 `ChooseRoute(state, choiceIndex)`。
  - 节点推进时优先读取显式后继；没有显式后继时继续使用线性下一节点。
  - 多个后继时进入 `RouteChoice`，单个后继时直接进入目标节点，没有后继时完成 Run。
  - 路线目标必须指向当前路线内的后续节点，避免回环和越界。
- 扩展 `CultivationRunEngineTests`：
  - 验证战斗奖励后会进入路线选择状态。
  - 验证选择路线后启动目标节点。
  - 验证非法路线索引会被拒绝。

## 3. 当前覆盖能力

- 支持普通战斗、精英、闭关节点作为分支目标。
- 支持从一个节点分出多个后继目标。
- 保持现有默认原型路线线性，以免当前战斗原型 UI 因无路线选择界面而卡住。
- 后续 UI 可直接读取 `CurrentRouteChoices` 展示目标节点名称、类型和敌人信息。

## 4. 验证与结果

已执行：

```powershell
dotnet build UnityProject\UnityProject.sln --no-restore
git diff --check
```

结果：

- `dotnet build UnityProject\UnityProject.sln --no-restore`：通过；保留既有 Unity/MCP 引用版本警告，无新增编译错误。
- `git diff --check`：通过。

Unity MCP 验收：

- `read_console(types=["error","warning"])`：测试前 0 条。
- `GameLogic.EditModeTests`：24/24 通过。
- `execute_code` 探针：自定义分支路线在战斗奖励后进入 `RouteChoice`，选中精英分支后进入 `probe_elite` 战，玩家 HP 正确继承为 66。
- 最终 `read_console(types=["error","warning"])`：0 条。

## 5. 假设与风险

- 本阶段只做逻辑状态机，不做路线地图生成、路线 UI、图标、节点布局或点击交互。
- 默认种子路线暂时保持线性，避免当前原型 UI 缺少路线选择控件时无法继续。
- 当前分支目标使用节点索引，后续迁移配置表时可以改为节点 id 或配置 id 映射。
- 当前禁止路线回环，符合最小 Roguelite 地图向前推进模型。

## 6. 下一阶段建议

Phase 7 可继续推进以下之一：

- 做 Run 调试 UI，展示路线、HP、当前状态、奖励、闭关升级和路线选择。
- 把默认原型路线接入一个最小分支节点，并由 UI 驱动选择。
- 补第二层升级树。
- 开始把种子数据迁移到 Luban 配置表。
