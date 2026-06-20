# Phase60 交付说明 - UI 资产基础接入

## 结果摘要

本阶段响应“UI资产也记得加上”，为《仙途·天命》当前 Unity + TEngine 项目建立第一批可替换的 UI 占位资产，并让原型 Run UI 能实际引用这些 Sprite。资产不复用旧肉鸽资源，也未引入外部素材包；本阶段生成的 PNG 均为项目内程序化占位图，后续正式美术可以按同名路径替换。

资产根目录：

- `UnityProject/Assets/AssetRaw/UIRaw/Single/CultivationCardGame`

当前覆盖：

- 战斗背景占位图。
- 深色/金色面板底图。
- 按钮正常、悬停、按下、禁用四状态。
- 五档卡牌边框占位图。
- 灵力、护盾、雷属性图标。
- 攻击、防御、眩晕敌人意图图标。
- 战斗、精英、坊市、闭关、Boss 地图节点图标。
- `ui_asset_manifest.json` 记录阶段、来源、授权状态、根目录和资产列表。

## 实际改动清单

- 新增 `UnityProject/Assets/AssetRaw/UIRaw/Single/CultivationCardGame`
  - 生成 23 张 PNG 占位 UI 资产。
  - Unity 已导入为 Sprite，并生成对应 `.meta`。
  - 新增 `ui_asset_manifest.json`，明确 `source=procedural_placeholder_owned_by_project` 与 `license=project_generated_placeholder_replaceable`。
- 新增 `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/CultivationUIAssetCatalog.cs`
  - 固化 UI 资产根路径和每个资源的稳定路径常量。
  - 提供 `RequiredSpritePaths` 作为测试与后续打包清单入口。
  - 编辑器环境下提供 `LoadEditorSprite(...)`，用于当前原型 UI 和 EditMode 测试读取 `AssetRaw` Sprite。
- 修改 `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/CultivationRunPrototypeUI.cs`
  - 原型 UI 初始化时尝试加载 Phase60 UI 占位 Sprite。
  - 根背景使用战斗背景占位图。
  - 面板、按钮、战斗卡、信息卡使用对应占位 Sprite。
  - 按钮补上 SpriteSwap 状态图；资产缺失时继续回退到原有纯色 UI。
- 新增 `UnityProject/Assets/Tests/EditMode/CultivationUIAssetCatalogTests.cs`
  - 验证清单文件存在。
  - 验证所有必需 PNG 文件存在并可导入为 Sprite。
  - 验证运行时生成的原型 UI 会实际挂载背景、按钮、战斗卡 Sprite。
- 修改 `Doc/文档索引.md`
  - 最新阶段交付说明更新为 Phase60。

## 验证与结果

- `dotnet build UnityProject\UnityProject.sln --no-restore`
  - 结果：通过，0 Error。
  - 仍有既有 Unity/dotnet 警告，包括 `USG0001`、`CS8632`、`MSB3277`，本阶段未新增阻断错误。
- `git diff --check`
  - 结果：通过。
- Unity MCP
  - `refresh_unity(mode="force", scope="all", compile="request", wait_for_ready=true)`：通过；期间 Unity 域重载导致一次短暂断连，工具自动恢复。
  - `run_tests(EditMode, assembly_names=["GameLogic.EditModeTests"])`：通过，182/182 Passed，0 Failed，0 Skipped。

## 假设与风险

- 本阶段只提供可替换占位资产和稳定路径，不代表最终美术品质。
- 资产先放在 `AssetRaw/UIRaw/Single/CultivationCardGame`，符合当前工程资源目录形态；后续若进入 YooAsset/图集打包阶段，需要再把该目录纳入正式打包规则。
- 当前原型 UI 在编辑器/测试环境下通过 `AssetDatabase` 读取 `AssetRaw` Sprite；玩家包中若未配置打包加载，会自动回退到纯色 UI，不会因为缺资源崩溃。
- 没有导入外部免费素材包，因此不存在第三方授权风险；后续若使用外部 UI 包，需要单独记录来源、许可证和可商用条件。

## 可选下一步

- Phase61：继续按设计文档补下一批卡牌/法宝/敌人内容，或把 Phase60 的占位资产接入 YooAsset 图集打包规则。
- UI 正式化阶段：基于 `Doc/设计文档/UI_UX设计规范.md` 制作黄金样板 Prefab，再替换当前占位图。
