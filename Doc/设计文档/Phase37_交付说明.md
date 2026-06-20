# Phase37 交付说明 - 完整跑团 UI 统一入口服务

## 结果摘要

本阶段在 Phase35 完整跑团 UI 外壳和 Phase36 金丹被动闭环的基础上，把主运行 UI 的启动入口从 `CultivationRunPrototypeUI.Open()` 直连收束为统一服务 `CultivationRunUIService.OpenMainRunUI()`。

这样 `GameEntry`、热更入口 `GameApp` 和测试代码不再直接依赖具体原型 UI 的创建方式。当前仍复用已有代码生成 uGUI 原型作为主界面承载，但后续替换为 Prefab、TEngine `UIWindow` 或资源加载流程时，可以优先改服务层，而不需要再次改启动入口。

## 实际改动清单

- 新增 `CultivationRunUIService`
  - 提供 `OpenMainRunUI()` 作为完整跑团 UI 的统一打开入口。
  - 提供 `IsMainRunUIOpen()` 和 `CloseMainRunUI()`，用于测试、调试和后续窗口生命周期接入。
  - 优先解析 `UIModule.UIRoot`，其次查找场景 `UIRoot` 下的 `Canvas`，再回落到 `RuntimePrototypeCanvas` 或空父级。
  - 保持对现有 `CultivationRunPrototypeUI` 的复用，不重复创建同名根节点。
- 修改 `GameEntry`
  - Editor 原型入口改为调用 `CultivationRunUIService.OpenMainRunUI()`。
- 修改 `GameApp`
  - 热更逻辑入口改为调用 `CultivationRunUIService.OpenMainRunUI()`。
- 修改 EditMode 测试
  - 新增 `MainRunUiServiceReusesAndClosesPrototypeUi`，验证统一服务入口可复用已打开 UI，并能正确关闭主 UI。

## 验证与结果

- `dotnet build UnityProject\UnityProject.sln --no-restore`
  - 结果：通过，0 Error。
  - 备注：仍存在项目既有 warning，主要为程序集引用冲突、`LUSG0001`、`CS8632` 等，不属于本阶段新增错误。
- Unity MCP `refresh_unity`
  - 结果：通过。
  - 备注：刷新期间出现一次可恢复 domain reload 断连，最终 Editor ready。
- Unity MCP 定向 EditMode 测试
  - `GameLogic.Tests.CultivationRunPrototypeUITests.MainRunUiServiceReusesAndClosesPrototypeUi`
  - `GameLogic.Tests.CultivationRunPrototypeUITests.OpenCreatesInteractiveRunPrototype`
  - `GameLogic.Tests.CultivationRunPrototypeUITests.OpenBuildsCompleteRunUiLayout`
  - 结果：3/3 Passed。
- Unity MCP 完整 `GameLogic.EditModeTests`
  - 结果：129/129 Passed。
- Unity MCP `execute_code` 操作探针
  - 结果：通过。
  - 观察值：`Reused=True; BeforeClose=True; Closed=True; AfterClose=False; Snapshot=InBattle/山门石魔`。
- `git diff --check`
  - 结果：待最终提交前执行。
- Unity MCP `read_console`
  - 结果：待最终提交前执行。

## 假设与风险

- 本阶段是 UI 入口和生命周期服务化，不是最终商业 Prefab 生产阶段。
- 当前主界面仍是代码生成 uGUI 原型，已覆盖战斗、路线、闭关、坊市、宝箱、秘境、奖励、金丹被动等核心跑团状态，但视觉质量仍需要后续样板、Prefab 和截图验收阶段继续提升。
- `CultivationRunUIService` 当前只抽象主跑团 UI，不处理弹窗栈、异步资源句柄和 TEngine `UIWindow` 生命周期；这些应在后续正式 UI 资源化阶段接入。

## 可选下一步

- Phase38：把主跑团 UI 拆成可复用 Prefab 或 TEngine `UIWindow`，接入资源加载与窗口生命周期。
- Phase39：制作 UI 视觉黄金样板和截图验收，替换当前代码生成临时控件。
- Phase40：扩展金丹层地图、敌人池和奖励池，让完整 UI 承载更多中后期内容。
