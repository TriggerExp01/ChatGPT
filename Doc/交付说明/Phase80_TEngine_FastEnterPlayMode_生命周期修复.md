# Phase80 TEngine Fast Enter Play Mode 生命周期修复交付说明

## 背景

当前 Unity 工程启用了 Enter Play Mode Options：

- `m_EnterPlayModeOptionsEnabled: 1`
- `m_EnterPlayModeOptions: 3`

这表示 Editor Play Mode 禁用了 Domain Reload 和 Scene Reload。重复进入 Play Mode 时，Unity 不会自动重建 C# AppDomain，也不会自动重新加载场景，因此 TEngine 内部的 static 状态可能跨 Play Mode 残留。

## 问题表现

重复进入 Play Mode 时，曾出现以下重复注册或重复对象池创建异常：

- `Already exist object pool 'TEngine.ResourceModule+AssetObject.Asset Pool'`
- `Debugger window has been registered`
- `Already exist object pool 'TEngine.AssetItemObject.SetAssetPool'`

## 根因分析

- `ModuleSystem` 使用 static `Dictionary` / `LinkedList` / `List` 管理模块实例、轮询模块和执行列表。
- `RootModule` 原本在 Editor 下 `OnDestroy()` 不执行 `ModuleSystem.Shutdown()`，导致禁用 Domain Reload 后模块状态不能在退出 Play Mode 时稳定清理。
- `Debugger` 存在 static `_instance` 和 static `s_TextEditor`，重复进入 Play Mode 时可能残留旧实例状态。
- `ResourceExtComponent` 存在 static `Instance` 和 static `_resourceModule`，重复进入 Play Mode 时可能残留旧资源模块引用。
- 禁用 Domain Reload 后，上述 static 状态不会随 Play Mode 退出自动清空，下一次进入 Play Mode 可能复用旧状态并触发重复初始化。

## 修复内容

### ModuleSystem.cs

- 在 Editor Play Mode 的 `SubsystemRegistration` 阶段调用 `ModuleSystem.Shutdown()`，用于清理上一轮 Play Mode 遗留的静态模块状态。
- 在 `Shutdown()` 中额外复位 `_isExecuteListDirty = false`。
- 未修改模块创建逻辑、模块优先级和业务模块注册规则。

### RootModule.cs

- `RootModule.OnDestroy()` 在 Play Mode 下执行 `ModuleSystem.Shutdown()`。
- `RootModule.OnDestroy()` 清理 `RootModule._instance`。
- 使用 `Application.isPlaying` 限制清理范围，避免 Edit Mode 非 Play 状态下误触发过重清理。
- 未修改 `Awake` / `Update` / `FixedUpdate` / `LateUpdate` 的业务行为。

### Debugger.cs

- `Debugger.OnDestroy()` 保留 `PlayerPrefs.Save()`。
- `Debugger.OnDestroy()` 在当前实例匹配时清理 `Debugger._instance`。
- `Debugger.OnDestroy()` 清理 static `s_TextEditor`。
- 未修改 Debugger 窗口注册列表和 Debugger UI 行为。

### ResourceExtComponent.Resource.cs

- `ResourceExtComponent.OnDestroy()` 保留原有 loading state 清理逻辑。
- `ResourceExtComponent.OnDestroy()` 在当前实例匹配时清理 `ResourceExtComponent.Instance`。
- `ResourceExtComponent.OnDestroy()` 清理 static `_resourceModule`。
- 未修改资源加载逻辑、`AssetItemObject` 对象池创建逻辑、YooAsset 初始化逻辑和 `SetAssetByResources` 异步加载流程。

## 明确未修改范围

- 未修改 UI Prefab。
- 未修改 MainRoute 视觉。
- 未修改修仙玩法逻辑。
- 未修改路线逻辑。
- 未修改战斗逻辑。
- 未修改事件逻辑。
- 未修改奖励逻辑。
- 未修改 Luban。
- 未修改 UIModule。
- 未修改 ProjectSettings。
- 未恢复旧肉鸽玩法、旧 UI Prefab、旧视觉资产。

## 验证结果

- `dotnet build UnityProject\UnityProject.sln --no-restore`：0 Error。
- `git diff --check`：通过。
- Unity Editor 第一次 Play Mode：Console Error = 0。
- Unity Editor 第二次 Play Mode：Console Error = 0，Warning = 0。

说明：Unity Editor Play Mode 结果来自本阶段本地人工已验证记录；本文档不声明本轮重新执行了 Unity Play Mode。
