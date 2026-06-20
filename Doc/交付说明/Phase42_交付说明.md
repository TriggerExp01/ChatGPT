# Phase42 交付说明 - 跑团 UI 动态项 Prefab 模板化

## 结果摘要

本阶段把 Phase41 之后仍由脚本纯生成的动态 UI 项推进为 Prefab 模板克隆。`CultivationRunWindow.prefab` 现在在 `PrefabAnchors/UIItemTemplates` 下预置了隐藏模板：`InfoCardTemplate`、`ActionButtonTemplate`、`MessageTextTemplate`。

运行时刷新属性卡、地图节点、奖励按钮、路线按钮、坊市按钮、手牌按钮、丹药按钮和提示文本时，会优先克隆这些模板，再填充文本、颜色、布局和点击事件。找不到模板时仍保留原有代码生成路径，保证非 Prefab 测试环境和临时兜底入口不被破坏。

## 实际改动清单

- 修改 `CultivationRunPrototypeUI`
  - 新增 `BindTemplates()`，从窗口根下的 `PrefabAnchors/UIItemTemplates` 绑定动态 UI 模板。
  - 新增 `CreateInfoCardItem()`、`CreateActionButton()`、`CreateMessageText()`，优先克隆 Prefab 模板后复用既有填充逻辑。
  - 属性栏和地图栏动态卡片改为走 `InfoCardTemplate`。
  - 奖励、闭关、路线、坊市、宝箱、秘境、金丹被动、手牌、丹药等动态按钮改为走 `ActionButtonTemplate`。
  - Run 外丹药提示文本改为走 `MessageTextTemplate`。
- 修改 `CultivationRunWindow.prefab`
  - 新增 `PrefabAnchors/UIItemTemplates`。
  - 新增隐藏模板 `InfoCardTemplate`，包含 `Title`、`Value`、`Note`。
  - 新增隐藏模板 `ActionButtonTemplate`，包含 `Label`、`Detail`。
  - 新增隐藏模板 `MessageTextTemplate`。
- 修改 EditMode 测试
  - `RunWindowPrefabAssetCanLoadFromResources` 覆盖模板节点存在且默认隐藏。
  - 新增 `PrototypeUiClonesDynamicItemsFromPrefabTemplates`，验证属性卡、地图节点和手牌按钮会继承模板组件。

## 验证与结果

- `dotnet build UnityProject\UnityProject.sln --no-restore`
  - 结果：通过，0 Error。
  - 备注：仍存在项目既有 warning，主要为 `System.Net.Http` / `System.IO.Compression` 引用冲突、`USG0001`、`CS8632`。
- `git diff --check`
  - 结果：通过。
- Unity MCP 定向 EditMode 测试
  - `RunWindowPrefabAssetCanLoadFromResources`
  - `PrototypeUiClonesDynamicItemsFromPrefabTemplates`
  - `OpenBuildsCompleteRunUiLayout`
  - 结果：3/3 Passed。
- Unity MCP 完整 `GameLogic.EditModeTests`
  - 结果：135/135 Passed。
- Unity MCP `execute_code` 操作探针
  - 结果：通过。
  - 观察值：`Templates=True; StatTemplate=True; MapTemplate=True; FirstCard=Card_0_break_armor; FirstCardTemplate=True; Snapshot=InBattle/山门石魔`。
- Unity MCP `read_console`
  - 结果：0 Error / 0 Warning。

## 假设与风险

- 本阶段完成的是动态项模板化，不是最终美术资源替换。
- 模板当前仍使用 Unity 内置 `Text`、默认字体和纯色 `Image`，后续视觉样板阶段应替换为正式字体、图集、按钮状态和卡牌模板。
- 动态项模板通过 `PrefabAnchors` 绑定，要求 `CultivationRunPrototypeUI` 的父节点为窗口根；当前 `CultivationRunWindow` 和 Resources Prefab 路径满足该约束。
- `Resources` 加载仍是当前阶段的临时入口，正式资源包阶段仍应迁移到 YooAsset / TEngine。

## 可选下一步

- Phase43：制作视觉样板，替换信息卡、按钮和手牌模板的图集、字体、颜色层级与状态表现。
- Phase44：拆分更细的卡牌模板、路线节点模板和奖励模板，减少运行时按同一按钮模板承载所有语义。
- Phase45：把 `Resources` 路径迁移到 YooAsset / TEngine 资源加载链路。
