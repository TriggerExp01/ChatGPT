# Phase38 交付说明 - 跑团 UI 接入 TEngine 窗口生命周期

## 结果摘要

本阶段在 Phase37 统一 UI 服务入口的基础上，补齐完整跑团 UI 与 TEngine `UIModule` / `UIWindow` 生命周期之间的适配层。热更入口现在会优先尝试通过 `CultivationRunWindow` 打开主跑团 UI；如果当前不是播放态或没有运行时 UI 环境，则自动回落到 Phase37 的直接原型 UI 入口。

当前主跑团 UI 仍复用已有代码生成 uGUI 原型，但已经具备 TEngine 窗口元数据、窗口面板工厂和资源加载器包装。后续把 `CultivationRunWindowPrefabFactory` 替换为真实 Prefab 或 YooAsset 资源时，可以保留 `GameApp` 和业务打开入口不变。

## 实际改动清单

- 新增 `CultivationRunWindow`
  - 标记为 `UILayer.UI` 的全屏 `UIWindow`。
  - 使用 `CultivationRunWindow` 作为当前窗口资源定位名。
  - 在窗口创建和刷新时挂载 `CultivationRunPrototypeUI`，让完整跑团 UI 进入 TEngine 窗口生命周期。
- 新增 `CultivationRunWindowPrefabFactory`
  - 生成兼容 `UIWindow` 的运行时面板根节点。
  - 面板包含 `RectTransform`、`Canvas`、`GraphicRaycaster`，并铺满父级。
- 新增 `CultivationRunWindowResourceLoader`
  - 对 `CultivationRunWindow` 资源名进行拦截，生成当前阶段的运行时面板。
  - 其他 UI 资源继续转发到原始 `IUIResourceLoader`，避免影响框架默认加载链路。
- 修改 `CultivationRunUIService`
  - 新增 `OpenMainRunWindowOrFallback()`。
  - 新增 `TryOpenMainRunWindow()`，只在播放态尝试走 `UIModule`，避免 Editor 非播放态触发 `UIModule` 的 `DontDestroyOnLoad` 运行时假设。
  - 新增 `EnsureMainRunWindowResourceLoader()` 与 `CloseMainRunWindow()`。
  - 查找主 UI 时支持从 `UIModule.UIRoot` 下递归查找嵌套的 `CultivationRunPrototypeUI`。
- 修改 `GameApp`
  - 热更入口改为优先调用 `CultivationRunUIService.OpenMainRunWindowOrFallback()`。
- 修改 EditMode 测试
  - 覆盖窗口元数据。
  - 覆盖窗口资源加载器生成兼容面板。
  - 覆盖 Editor 非播放态下入口自动回落，不初始化运行时 `UIModule`。

## 验证与结果

- `dotnet build UnityProject\UnityProject.sln --no-restore`
  - 结果：通过，0 Error。
  - 备注：仍存在项目既有 warning，主要为 `System.Net.Http` / `System.IO.Compression` 引用冲突、`LUSG0001`、`CS8632`。
- `git diff --check`
  - 结果：通过。
- Unity MCP `refresh_unity`
  - 结果：通过。
  - 备注：刷新期间出现一次可恢复 domain reload 断连，最终 Editor ready。
- Unity MCP 定向 EditMode 测试
  - `RunWindowMetadataUsesGeneratedMainRunPanel`
  - `RunWindowResourceLoaderCreatesWindowCompatiblePanel`
  - `RunWindowEntryFallsBackOutsidePlayMode`
  - `MainRunUiServiceReusesAndClosesPrototypeUi`
  - 结果：4/4 Passed。
- Unity MCP 完整 `GameLogic.EditModeTests`
  - 结果：132/132 Passed。
- Unity MCP `execute_code` 操作探针
  - 结果：通过。
  - 观察值：`UI=True; UIModuleValid=False; Parent=UICanvas; Snapshot=InBattle/山门石魔`。
  - 含义：Editor 非播放态不会错误初始化运行时 `UIModule`，仍可打开完整跑团 UI 进行验证。
- Unity MCP `read_console`
  - 结果：0 Error / 0 Warning。

## 假设与风险

- 本阶段完成的是 TEngine 窗口生命周期适配，不是最终 UI Prefab 视觉生产。
- `CultivationRunWindowPrefabFactory` 目前是运行时面板工厂，后续应替换为真实 Prefab 或资源加载地址。
- `TryOpenMainRunWindow()` 只在播放态走 `UIModule`，这是为了符合当前 `UIModule.OnInit()` 对 `DontDestroyOnLoad` 的运行时假设；Editor 非播放态工具验证继续走直接原型 UI。
- 当前完整 UI 已具备核心状态覆盖，但视觉质量、动画、图集资源和截图验收仍需要后续专门阶段推进。

## 可选下一步

- Phase39：制作真实 `CultivationRunWindow` Prefab 骨架，把运行时工厂替换为可编辑资源。
- Phase40：按 UI 视觉规范做黄金样板和截图验收，提升完整跑团 UI 的商业演示质量。
- Phase41：继续扩展金丹层地图、敌人池、奖励池和事件池，让完整 UI 承载更多中后期内容。
