# Phase41 交付说明 - 完整跑团 UI 二级结构 Prefab 化

## 结果摘要

本阶段把 Phase40 的一级 Prefab 锚点继续推进为完整跑团 UI 的二级静态结构。`CultivationRunWindow.prefab` 现在不再只是 `Header`、`Body`、`Lower` 的空壳，而是预置了标题栏、资源栏、路线图栏、三栏主体、两个横向滚动区、底部操作栏、重开按钮和结束回合按钮等关键 UI 节点。

运行时代码仍负责刷新动态内容，例如属性卡、路线节点、奖励按钮、手牌按钮、坊市操作和日志文本；但这些内容会挂到 Prefab 已存在的 `Content`、文本和按钮锚点下，避免完整 UI 结构继续只存在于脚本生成逻辑中。

## 实际改动清单

- 修改 `CultivationRunPrototypeUI`
  - `Open()` 遇到已挂载 `CultivationRunPrototypeUI` 组件的 Prefab 节点时，会重新执行初始化绑定，再重置 Run。
  - `Initialize()` 与 `ResetRun()` 解耦，避免 Prefab 资产生成静态布局时意外写入一局动态 Run 内容。
  - `BuildView()` 中的根背景、根布局、标题行、标题盒、主体布局、底部布局和操作栏布局都改为 `GetOrAdd<T>()`，使完整 Prefab 结构可被复用而不是重复添加组件。
- 修改 `CultivationRunWindow.prefab`
  - 在 `Header` 下预置 `TitleRow`、`TitleBox`、`Title`、`StageText`、`StatusText`、`StatsBar`、`MapBar`。
  - 在 `Body` 下预置 `RunPanel`、`BattlePanel`、`DeckPanel` 及对应标题和文本锚点。
  - 在 `Lower` 下预置 `ChoicesScrollPanel`、`HandScrollPanel`、`Viewport/Content`、`ActionBar`、`ResetButton`、`EndTurnButton`。
  - 保留 `PrefabAnchors`，用于后续继续放置可编辑样板、模板节点或视觉资源引用。
- 修改 EditMode 测试
  - 扩展 `RunWindowPrefabAssetCanLoadFromResources`，要求 Prefab 资产直接包含完整二级 UI 结构。
  - 扩展 `PrototypeUiReusesPrefabAnchors`，验证运行时复用标题文本、滚动内容、底部按钮等 Prefab 锚点。

## 验证与结果

- `dotnet build UnityProject\UnityProject.sln --no-restore`
  - 结果：通过，0 Error。
  - 备注：仍存在项目既有 warning，主要为 `System.Net.Http` / `System.IO.Compression` 引用冲突、`USG0001`、`CS8632`。
- `git diff --check`
  - 结果：通过。
- Unity MCP 定向 EditMode 测试
  - `RunWindowPrefabAssetCanLoadFromResources`
  - `PrototypeUiReusesPrefabAnchors`
  - `OpenBuildsCompleteRunUiLayout`
  - `RunWindowEntryFallsBackOutsidePlayMode`
  - 结果：4/4 Passed。
- Unity MCP 完整 `GameLogic.EditModeTests`
  - 结果：134/134 Passed。
- Unity MCP `execute_code` 操作探针
  - 结果：通过。
  - 观察值：`Prefab=True; Shell=True; TitleReused=True; ChoicesReused=True; HandReused=True; ResetReused=True; Snapshot=InBattle/山门石魔`。
- Unity MCP `read_console`
  - 结果：0 Error / 0 Warning。

## 假设与风险

- 本阶段完成的是“完整 UI 结构 Prefab 化”，不是最终美术品质 UI。
- 动态列表项仍由运行时代码生成，包括属性卡、地图节点卡、奖励/路线/坊市按钮、手牌和丹药按钮。
- Prefab 目前仍走 `Resources/UIWindow/CultivationRunWindow`，后续正式热更资源阶段仍应迁移到 YooAsset / TEngine 资源加载链路。
- 当前 UI 使用 Unity 内置 `Text` 和默认运行时字体，后续做视觉样板时应替换为正式字体、图集、按钮状态、音效和截图验收。

## 可选下一步

- Phase42：制作完整跑团 UI 的视觉样板，包括卡牌模板、路线节点模板、资源栏图标、按钮状态和截图验收。
- Phase43：把动态卡片/按钮进一步拆成 Prefab 模板，减少运行时代码直接创建控件的比例。
- Phase44：把 `Resources` 加载迁移到 YooAsset / TEngine 资源加载链路。
