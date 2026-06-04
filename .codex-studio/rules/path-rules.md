# 路径级规则

## 根目录

### `AGENTS.md`

- Codex 项目规则总入口。
- 所有项目任务必须遵守。
- 如与用户当次明确指令冲突，以用户当次明确指令为准，但必须说明风险。

### `Doc/**`

- 当前有效文档集中在 `Doc/文档索引.md` 中列出的文件。
- 不要创建大量重复文档。
- 阶段结论优先更新 `Doc/开发日志.md` 和阶段路线。
- 文档优先使用中文。

## Unity 启动与流程

### `UnityProject/Assets/GameScripts/GameEntry.cs`

- 只负责初始化核心模块、启动流程和 `DontDestroyOnLoad`。
- 不写具体玩法逻辑。
- 不在这里直接打开大量业务 UI。

### `UnityProject/Assets/TEngine/Settings/ProcedureSetting.asset`

- 这是启动流程真实配置。
- 修改前必须确认当前流程链路。
- 不允许为了玩法功能随意切换入口流程。

### `UnityProject/Assets/GameScripts/HotFix/GameLogic/GameApp.cs`

- 热更游戏逻辑入口。
- 只做初始化和主逻辑分发。
- 不堆武器、敌人、升级、掉落等细节。
- 适合挂“进入主菜单”或“进入当前主玩法”的起点。

### `UnityProject/Assets/GameScripts/HotFix/GameLogic/GameModule.cs`

- 全局模块统一访问入口。
- 常用模块应通过 `GameModule` 获取。
- 不要在业务代码里散落 `ModuleSystem.GetModule<T>()`。

## 玩法逻辑

### `UnityProject/Assets/GameScripts/HotFix/GameLogic/**`

- 主玩法逻辑、单局流程、战斗、成长、结算放在这里。
- 保持最小实现，不提前复杂化。
- 如果 `RoguelikeGame` 变大，只在当前阶段受阻时做最小拆分。

## UI

### `UnityProject/Assets/GameScripts/HotFix/GameLogic/UI/**`

- UI 只负责显示、输入和反馈。
- UI 不直接拥有核心战斗状态。
- 打开窗口优先走 `GameModule.UI.ShowUIAsync<T>()`。
- 不要在 UI 中硬编码玩法规则。

### `UnityProject/Assets/GameScripts/HotFix/GameLogic/Module/UIModule/**`

- 通用 UI 框架能力放这里。
- 不为单个界面写特殊框架分支。
- 修改前先确认 UIRoot、Canvas、UI Camera 生命周期。

## 配置

### `Configs/GameConfig/**`

- Luban 配置源目录。
- 敌人、武器、升级选项、被动、刷怪规则最终应逐步迁移到这里。
- 阶段 13 前不强行全量迁移。
- 配置化时要保证运行时代码只解释和组装数据，不继续散落硬编码内容。

## 资源

### `UnityProject/Assets/AssetRaw/**`

- 热更资源目录。
- 使用 YooAsset 资源规则。
- 不主动引入 Addressables。
- 新资源必须注意命名、引用和释放。

### `UnityProject/Assets/AssetArt/**`

- 美术源资源目录。
- 不在未确认任务中批量重组资源目录。

## 框架

### `UnityProject/Assets/TEngine/**`

- 默认禁止修改。
- 只有当任务明确为修框架、扩展框架、修 TEngine 接入问题时才允许修改。
- 修改前必须先读取框架指南和相关源码。

## Unity 元数据

### `*.meta`

- 禁止无理由删除。
- 移动/新增资源时必须保持 meta 一致。
- 不要批量重建 meta 来“解决问题”。

## 构建与工具

### `Tools/**`

- 修改前明确当前脚本运行环境、参数和工作目录。
- Windows 批处理脚本要注意中文路径、空格、环境变量和退出码。

### `HybridCLRData/**`

- 不主动修改生成物。
- HybridCLR 问题应优先从配置、程序集、生成步骤和构建日志排查。
