# Phase 9 交付说明：Run 原型展示逻辑拆分

> 日期：2026-06-20  
> 阶段：Phase 9  
> 范围：把 Run 原型 UI 的状态快照和文本投影拆出为纯 C# Presenter  
> 状态：已通过当前阶段验收

## 1. 结果摘要

本阶段不改变战斗、奖励、闭关、路线选择等玩法规则，只整理 Phase 7/8 之后持续膨胀的 Run 原型 UI 脚本：

- 新增 `CultivationRunPrototypePresenter`，负责把 `CultivationRunState` 转成调试 UI 所需文本。
- 把 `RunPrototypeSnapshot` 从 `CultivationRunPrototypeUI` 文件中移到 Presenter 文件。
- `CultivationRunPrototypeUI` 保留 Unity 组件创建、按钮绑定和刷新流程，不再直接拼接主要状态文本。
- 新增 Presenter 的 EditMode 测试，覆盖路线分支文本、快照摘要和卡牌效果摘要。

## 2. 实际改动清单

- 新增 `CultivationRunPrototypePresenter.cs`：
  - `CreateSnapshot(state)`。
  - `BuildText(state, maxLogLines)`。
  - `FormatCardSummary(card)` / `FormatEffect(effect)`。
  - `RunPrototypeText`。
  - `RunPrototypeSnapshot`。
- 更新 `CultivationRunPrototypeUI.cs`：
  - `Snapshot` 改为通过 Presenter 生成。
  - `Refresh()` 改为消费 `RunPrototypeText`。
  - 奖励按钮和手牌按钮复用 Presenter 的卡牌摘要。
  - 移除 UI 文件内部的文本构建和快照类。
- 新增 `CultivationRunPrototypePresenterTests.cs`：
  - 验证首战后路线选择文本包含 `火蝠洞` 与 `闭关调息`。
  - 验证快照保留当前节点、HP、牌组数和手牌数。
  - 验证卡牌摘要包含灵力、伤害与破防效果。

## 3. 当前覆盖能力

- Run 原型 UI 的显示文本可在不创建 Unity GameObject 的情况下测试。
- 后续继续拆分按钮构建、路线地图、正式 UI ViewModel 时，可以沿 Presenter 继续收敛。
- 当前仍是调试/原型 UI，不代表正式视觉样板。

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

- `refresh_unity(mode=force, scope=scripts, compile=request, wait_for_ready=true)`：通过；期间出现一次常见域重载断连并自动恢复。
- `GameLogic.EditModeTests`：31/31 通过。
- `execute_code` 探针：打开 `CultivationRunPrototypeUI`，确认首节点有 2 个后继；执行首战结算、跳过奖励、选择闭关、闭关升级，最终进入 `石魔首领` 战；返回 `PASS node=石魔首领, deck=12, hand=5`。
- 最终 `read_console(types=["error","warning"])`：0 条。

## 5. 假设与风险

- 本阶段没有引入正式 UI 资源、Prefab 或美术方向。
- Presenter 仍服务于原型界面，后续正式 UI 可以复用其思路，但不应被视为最终 UI 架构。
- 新增 `.cs` 文件需要 Unity 刷新后生成 `.meta` 并更新项目文件。
