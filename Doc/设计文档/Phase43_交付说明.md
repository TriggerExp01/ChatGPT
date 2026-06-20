# Phase43 交付说明 - 跑团 UI 视觉样板主题化

## 结果摘要

本阶段在 Phase42 动态项 Prefab 模板化的基础上，完成主跑团 UI 的第一版可验收视觉样板。`CultivationRunWindow.prefab` 和运行时代码生成路径现在使用一致的东方玄幻主题：深墨底色、玉色信息层级、金色标题与重点状态、铜金按钮、面板描边和文本阴影。

本阶段不改动战斗、跑团、奖励、坊市、秘境或金丹被动规则，只推进完整 UI 的视觉层级、可读性和 Prefab 静态样板。

## 实际改动清单

- 修改 `CultivationRunPrototypeUI`
  - 新增统一主题色常量，覆盖根背景、Header、三栏主体、滚动区、动态信息卡和按钮。
  - 为运行时生成的面板、信息卡和按钮补 `CanvasGroup`、`Outline`、`Shadow`。
  - 为运行时生成的文字补 `Shadow` 和更稳定的行距，提升深色 UI 上的可读性。
  - 修正 Unity 组件获取逻辑，避免 `GetComponent<T>() ?? AddComponent<T>()` 触发 Unity 伪 null 组件问题。
- 修改 `CultivationRunWindow.prefab`
  - 同步静态 Prefab 视觉样板，使 Resources 直接加载时也具备主题色、描边、阴影和按钮状态色。
  - 保留 Phase42 的 `InfoCardTemplate`、`ActionButtonTemplate`、`MessageTextTemplate` 动态模板结构。
- 修改 EditMode 测试
  - 新增 `RunWindowPrefabCarriesCultivationVisualTheme`，验证 Prefab 静态主题组件和关键颜色。
  - 新增 `RuntimeGeneratedUiAppliesCultivationVisualTheme`，验证非 Prefab 兜底路径也会生成同样的主题组件。

## 验证与结果

- `dotnet build UnityProject\UnityProject.sln --no-restore`
  - 结果：通过，0 Error。
  - 备注：仍存在项目既有 warning，主要为 `System.Net.Http` / `System.IO.Compression` 版本冲突、`USG0001`、`CS8632`。
- `git diff --check`
  - 结果：通过。
- Unity MCP 定向 EditMode 测试
  - `RunWindowPrefabCarriesCultivationVisualTheme`
  - `RuntimeGeneratedUiAppliesCultivationVisualTheme`
  - `RunWindowPrefabAssetCanLoadFromResources`
  - `PrototypeUiClonesDynamicItemsFromPrefabTemplates`
  - 结果：4/4 Passed。
- Unity MCP 完整 `GameLogic.EditModeTests`
  - 结果：137/137 Passed。
- Unity MCP `execute_code` 操作探针
  - 结果：通过。
  - 观察值：`PASS Prefab=True; UI=True; HeaderOutline=True; BattleShadow=True; FirstCardCanvasGroup=True; Snapshot=InBattle/山门石魔`。
- Unity MCP 临时相机截图验收
  - 结果：通过。
  - 观察：首次截图发现左侧面板正文存在重叠风险，已通过 `ConfigureBodyText` 的自适应字号和垂直截断规则修正。
- Unity MCP `read_console`
  - 结果：0 Error / 0 Warning。

## 假设与风险

- 当前视觉样板仍使用 Unity 内置 `Text`、默认字体和纯色 `Image`，尚未接入正式字体、图集、九宫格边框、图标或动画。
- 本阶段使用 uGUI 组件完成视觉层级，不引入外部素材，也不继承旧肉鸽项目的视觉资源。
- `Resources/UIWindow/CultivationRunWindow` 仍是当前阶段加载入口，正式热更资源阶段仍应迁移到 YooAsset / TEngine 资源加载链路。

## 可选下一步

- Phase44：拆分更细的动态模板，例如手牌卡牌模板、路线节点模板、奖励模板、丹药模板和坊市条目模板。
- Phase45：补正式字体、图标、九宫格 UI 图集和截图验收流程。
- Phase46：把 `Resources` 路径迁移到 YooAsset / TEngine 资源加载链路。
