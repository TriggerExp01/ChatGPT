# Phase39 交付说明 - 跑团 UI Prefab 骨架资源化

## 结果摘要

本阶段把 Phase38 的运行时窗口面板向真实 Unity 资源推进：新增 `Resources/UIWindow/CultivationRunWindow.prefab`，并让 `CultivationRunWindow` 通过 `Resources.Load` 路径加载该 Prefab 骨架。

当前 Prefab 仍是最小窗口骨架，包含可被 `UIWindow` 接管的 `Canvas`、`GraphicRaycaster` 和 `CanvasScaler`。完整跑团内容仍由 `CultivationRunPrototypeUI` 在窗口创建时挂载生成，但入口已经从纯运行时面板工厂前进到真实 Prefab 资产。后续可以在这个 Prefab 上继续补视觉结构、图集、动画和可编辑节点。

## 实际改动清单

- 新增 `UnityProject/Assets/Resources/UIWindow/CultivationRunWindow.prefab`
  - 资源名：`CultivationRunWindow`。
  - 路径：`Resources/UIWindow/CultivationRunWindow`。
  - 组件：`RectTransform`、`Canvas`、`GraphicRaycaster`、`CanvasScaler`。
  - 布局：铺满父级，`CanvasScaler` 使用 1920x1080 参考分辨率。
- 修改 `CultivationRunWindow`
  - `WindowAttribute` 改为 `fromResources: true`。
  - 资源定位从运行时工厂名切换为 `UIWindow/CultivationRunWindow`。
- 保留 `CultivationRunWindowPrefabFactory`
  - 继续作为生成 Prefab 骨架、测试面板兼容性和后续重建资源的工具。
- 修改 EditMode 测试
  - 新增 `RunWindowPrefabAssetCanLoadFromResources`，验证 Prefab 能通过 `Resources.Load` 加载，并具备 `Canvas` 与 `GraphicRaycaster`。
  - 更新窗口元数据断言，覆盖 `FromResources` 和新资源路径。

## 验证与结果

- `dotnet build UnityProject\UnityProject.sln --no-restore`
  - 结果：通过，0 Error。
  - 备注：仍存在项目既有 warning，主要为 `System.Net.Http` / `System.IO.Compression` 引用冲突、`LUSG0001`、`CS8632`。
- Unity MCP `refresh_unity`
  - 结果：通过。
  - 备注：刷新期间出现一次可恢复 domain reload 断连，最终 Editor ready。
- Unity MCP 定向 EditMode 测试
  - `RunWindowMetadataUsesGeneratedMainRunPanel`
  - `RunWindowPrefabAssetCanLoadFromResources`
  - `RunWindowResourceLoaderCreatesWindowCompatiblePanel`
  - `RunWindowEntryFallsBackOutsidePlayMode`
  - 结果：4/4 Passed。
- Unity MCP 完整 `GameLogic.EditModeTests`
  - 结果：133/133 Passed。
- Unity MCP `execute_code` 操作探针
  - 结果：通过。
  - 观察值：`Prefab=True; PrefabName=CultivationRunWindow; UI=True; Parent=UICanvas; Snapshot=InBattle/山门石魔`。
- `git diff --check`
  - 结果：通过。
- Unity MCP `read_console`
  - 首次检查出现 1 条 MCP transport 可恢复断连日志：`Cannot access a disposed object. Object name: 'System.Net.Sockets.NetworkStream'.`
  - 清理控制台后复查：0 Error / 0 Warning。

## 假设与风险

- 本阶段完成的是 Prefab 骨架资源化，不是最终视觉 Prefab。
- `CultivationRunWindow.prefab` 目前只承载窗口根节点和基础 Canvas 组件，完整 UI 子树仍由代码生成。
- 资源路径使用 Unity `Resources`，适合作为当前骨架阶段的稳定入口；后续正式热更资源应迁移到 YooAsset / TEngine 资源加载链路。
- 视觉规范文件在当前工作区未找到，因此本阶段未做颜色、字体、图集和截图级视觉验收。

## 可选下一步

- Phase40：在 `CultivationRunWindow.prefab` 内补可编辑布局锚点，例如 Header、Body、Lower、ActionBar、MapBar、StatsBar。
- Phase41：接入视觉样板、图集和截图验收，把当前代码生成控件逐步迁移到 Prefab 节点。
- Phase42：把 Resources 路径迁移到 YooAsset / TEngine 资源包加载。
