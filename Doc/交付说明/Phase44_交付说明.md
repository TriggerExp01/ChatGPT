# Phase44 交付说明 - 跑团 UI 动态项语义模板拆分

## 结果摘要

本阶段在 Phase43 跑团 UI 视觉样板主题化的基础上，补齐运行时动态项的语义模板层。`CultivationRunPrototypeUI` 现在不再只依赖通用 `InfoCardTemplate` 和 `ActionButtonTemplate`，而是按使用场景区分状态卡、路线节点、战斗手牌、奖励卡、休整选项、坊市条目、宝箱、秘境、金丹被动、丹药和坊市牌组操作等动态模板。

本阶段不修改战斗、跑团、奖励、坊市、秘境或金丹被动规则，只推进 UI 动态项生成路径的结构可维护性和后续美术替换入口。

## 实际改动清单

- 修改 `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/CultivationRunPrototypeUI.cs`
  - 新增 `InfoTemplateKind` 和 `ActionTemplateKind`，让动态信息卡与按钮按语义选择模板。
  - 新增 `EnsureSemanticTemplates()`，当 Prefab 仍只有通用模板时，运行时会从通用模板克隆出语义模板并保持 inactive。
  - 新增语义按钮主题色，区分奖励、路线、休整、坊市、宝箱、秘境、金丹被动、战斗卡牌、丹药和坊市牌组操作。
  - 将手牌、奖励、路线、休整、坊市、宝箱、秘境、金丹被动、丹药等动态按钮切换到对应语义模板。
- 修改 `UnityProject/Assets/Tests/EditMode/CultivationRunPrototypeUITests.cs`
  - 新增 `PrototypeUiCreatesSemanticDynamicTemplatesWhenPrefabHasGenericTemplates`，验证通用 Prefab 模板可在运行时补齐语义模板。
  - 新增 `RuntimeDynamicItemsUseSemanticTemplateStyles`，验证战斗卡牌、奖励、路线等运行时动态项使用对应语义颜色。
- 整理交付说明目录
  - 既有 `Phase1` 到 `Phase43` 交付说明已统一移动到 `Doc/交付说明/`。
  - `AGENTS.md` 已固化规则：新增阶段交付说明统一放入 `Doc/交付说明/`。
  - `Doc/文档索引.md` 已指向交付说明统一目录，本阶段继续沿用该位置。

## 验证与结果

- `dotnet build UnityProject\UnityProject.sln --no-restore`
  - 结果：通过，0 Error。
  - 备注：仍存在项目既有 warning，主要为 `System.Net.Http` / `System.IO.Compression` 版本冲突。
- `git diff --check`
  - 结果：通过。
- Unity MCP `refresh_unity`
  - 结果：返回成功，但提示曾经过 Unity disconnect/retry 后恢复。
- Unity MCP `editor/state`
  - 结果：仍返回 `stale_status`，`ready_for_tools=false`。
- Unity MCP `read_console`
  - 结果：未完成，Unity 响应超时：`Timeout receiving Unity response`。
- Unity MCP 定向 EditMode 测试
  - 目标测试：
    - `GameLogic.Tests.CultivationRunPrototypeUITests.PrototypeUiCreatesSemanticDynamicTemplatesWhenPrefabHasGenericTemplates`
    - `GameLogic.Tests.CultivationRunPrototypeUITests.RuntimeDynamicItemsUseSemanticTemplateStyles`
    - `GameLogic.Tests.CultivationRunPrototypeUITests.PrototypeUiClonesDynamicItemsFromPrefabTemplates`
    - `GameLogic.Tests.CultivationRunPrototypeUITests.RuntimeGeneratedUiAppliesCultivationVisualTheme`
  - 结果：未完成，`run_tests` 启动阶段 Unity 响应超时。

## 假设与风险

- 当前 Prefab 资产仍保留通用模板作为基础结构，语义模板由运行时自动补齐；这能减少 YAML 手改风险，但正式美术替换前仍应把稳定语义模板落到 Prefab 资产中。
- 本阶段没有接入正式字体、图标、九宫格边框、图集或 YooAsset 资源链路。
- Unity MCP 当前存在传输或 Editor 响应不稳定，已通过 `dotnet build` 和 `git diff --check` 验证代码层面，但 Unity Test Framework 定向结果尚未拿到，不能视为完整 Unity 验收通过。

## 可选下一步

- Phase45：在 Unity MCP 稳定后补跑 Phase44 定向 EditMode 测试和完整 `GameLogic.EditModeTests`。
- Phase46：把语义模板真实落到 `CultivationRunWindow.prefab`，为正式字体、图标、九宫格和图集替换预留明确挂点。
- Phase47：继续迁移 `Resources/UIWindow/CultivationRunWindow` 到 YooAsset / TEngine 资源加载链路。
