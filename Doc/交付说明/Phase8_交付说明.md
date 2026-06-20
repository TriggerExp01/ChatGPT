# Phase 8 交付说明：Run 原型路线数据下沉

> 日期：2026-06-20  
> 阶段：Phase 8  
> 范围：把 Run 原型界面内的临时路线数据下沉到逻辑种子数据层  
> 状态：已通过当前阶段验收

## 1. 结果摘要

本阶段把 Phase 7 中写在 `CultivationRunPrototypeUI` 内的分支路线迁移到 `CultivationSeedData`：

- UI 不再直接构造玩法路线。
- `CultivationSeedData` 新增可操作的第一版分支原型路线。
- 保留原有线性 `CreateFirstPrototypeRoute()`，避免影响已有默认逻辑测试和线性 Run 原型。
- Run 原型 UI 改为调用 `CreateFirstPrototypeBranchingRoute()`。

这让玩法数据和界面职责分离，后续迁移 Luban 或正式路线配置时更容易替换。

## 2. 实际改动清单

- 扩展 `CultivationSeedData`：
  - 新增 `CreateFirstPrototypeBranchingRoute()`。
  - 路线为 `山门石魔 -> 火蝠洞/闭关调息 -> 石魔首领`。
- 更新 `CultivationRunPrototypeUI`：
  - 删除内部 `CreatePrototypeRoute()`。
  - 改为使用 `CultivationSeedData.CreateFirstPrototypeBranchingRoute()`。
- 扩展 `CultivationRunEngineTests`：
  - 验证种子分支路线在首战后提供 `火蝠洞` 与 `闭关调息` 两个选项。

## 3. 当前覆盖能力

- 线性原型路线和分支原型路线并存。
- UI 只消费路线数据，不再拥有路线定义。
- 分支路线仍支持 Phase 7 的完整操作闭环：战斗、奖励、路线选择、闭关升级、首领战。

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

- `refresh_unity(mode=force, scope=scripts, compile=request, wait_for_ready=true)`：通过；过程中发生一次 MCP 断连重试，最终恢复并 ready。
- `GameLogic.EditModeTests`：28/28 通过。
- `execute_code` 探针：打开 Run UI，确认首节点存在两个后继；执行首战结算、跳过奖励、选择闭关、闭关升级，最终进入 `石魔首领` 战。
- 最终 `read_console(types=["error","warning"])`：0 条。

## 5. 假设与风险

- 当前仍是种子数据，不是 Luban 配置表。
- 分支目标仍使用节点索引，后续配置化时应迁移为稳定 id。
- UI 仍是原型界面，正式 UI 与路线地图表现尚未开始。

## 6. 下一阶段建议

Phase 9 可继续推进以下之一：

- 把 Run 原型 UI 拆成更小的视图/控制组件，降低单脚本体量。
- 迁移卡牌、敌人或路线数据到 Luban 配置表。
- 补第二层升级树。
- 做正式路线图 UI 的最小视觉样板。
