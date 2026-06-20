# Phase40 交付说明 - 跑团 UI Prefab 可编辑锚点

## 结果摘要

本阶段把 Phase39 的 `CultivationRunWindow.prefab` 从“只有 Canvas 根节点的窗口骨架”推进为“具备可编辑布局锚点的 UI Prefab”。Prefab 内现在预置了 `CultivationRunPrototypeUI` 空壳节点，以及 `Header`、`Body`、`Lower`、`Lower/ActionBar` 和 `PrefabAnchors` 等关键锚点。

同时，`CultivationRunPrototypeUI.Open()` 不再遇到同名空壳节点就销毁重建，而是会复用 Prefab 中已有的 `CultivationRunPrototypeUI` 节点并挂载组件。布局生成函数也会优先复用同名直系子节点，避免后续在 Prefab 中制作的锚点被运行时代码覆盖掉。

## 实际改动清单

- 修改 `CultivationRunPrototypeUI`
  - `Open()` 发现同名 `CultivationRunPrototypeUI` 空壳时，直接复用并挂载组件。
  - `CreateRect()` 优先复用父节点下同名直系子节点。
  - 新增 `GetOrAdd<T>()`，避免在复用锚点时重复添加 `Image`、`Text`、`Button`、Layout、ScrollRect 等组件。
  - `CreatePanel()`、`CreateRowContent()`、`CreateInfoCard()`、`CreateScrollContent()`、`CreateText()`、`CreateButton()` 改为组件复用友好。
- 修改 `CultivationRunWindow.prefab`
  - 新增 `CultivationRunPrototypeUI` 锚点。
  - 新增 `Header`、`Body`、`Lower`、`Lower/ActionBar` 布局锚点。
  - 新增 `PrefabAnchors` 预留节点，用于后续迁移更多可编辑结构。
- 修改 EditMode 测试
  - `RunWindowPrefabAssetCanLoadFromResources` 覆盖 Prefab 锚点存在性。
  - 新增 `PrototypeUiReusesPrefabAnchors`，验证运行时会复用 Prefab 中的锚点，并仍生成完整 UI 子结构。

## 验证与结果

- `dotnet build UnityProject\UnityProject.sln --no-restore`
  - 结果：通过，0 Error。
  - 备注：仍存在项目既有 warning，主要为 `System.Net.Http` / `System.IO.Compression` 引用冲突、`LUSG0001`、`CS8632`。
- Unity MCP `refresh_unity`
  - 结果：通过。
  - 备注：刷新期间出现一次可恢复 domain reload 断连，最终 Editor ready。
- Unity MCP 定向 EditMode 测试
  - `OpenBuildsCompleteRunUiLayout`
  - `RunWindowPrefabAssetCanLoadFromResources`
  - `PrototypeUiReusesPrefabAnchors`
  - `RunWindowEntryFallsBackOutsidePlayMode`
  - 结果：4/4 Passed。
- Unity MCP 完整 `GameLogic.EditModeTests`
  - 结果：134/134 Passed。
- Unity MCP `execute_code` 操作探针
  - 结果：通过。
  - 观察值：`Prefab=True; Shell=True; ReusedHeader=True; Snapshot=InBattle/山门石魔; HasAction=True`。
- `git diff --check`
  - 结果：通过。
- Unity MCP `read_console`
  - 结果：0 Error / 0 Warning。

## 假设与风险

- 本阶段完成的是 Prefab 锚点复用，不是最终视觉 UI。
- 当前 `Header`、`Body`、`Lower` 等锚点只保证命名、层级和复用，不代表最终样式、动画或图集已经完成。
- `CreateRect()` 目前按同名直系子节点复用，适合当前逐步迁移路径；如果后续同一父节点下需要多个同名动态列表项，应继续保持动态项命名唯一。
- 当前 Prefab 仍通过 `Resources` 路径加载，后续正式资源包阶段应迁移到 YooAsset / TEngine 资源加载链路。

## 可选下一步

- Phase41：继续把 `TitleRow`、`StatsBar`、`MapBar`、`RunPanel`、`BattlePanel`、`DeckPanel`、`ChoicesScrollPanel`、`HandScrollPanel` 等二级结构迁移进 Prefab。
- Phase42：为 Prefab 锚点补视觉样板、字体层级、背景图和截图验收。
- Phase43：把 Resources 加载迁移到 YooAsset / TEngine 资源加载链路。
