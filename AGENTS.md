# AGENTS.md - TEngine 骨架与 Cultivation 原型规则

本仓库已从旧项目清理为可用于新游戏开工的 Unity + TEngine 工程骨架，并且当前 Dev 分支已经接入 `Cultivation / 修仙养成原型` 的入口与部分业务原型。

当前默认协作基线是：**TEngine 骨架 + Cultivation 修仙养成原型**。这表示后续阶段可以围绕修仙、养成、秘境事件、路线选择和卡牌战斗继续收束原型；但这不是最终产品方向锁死，正式产品名、商业化定位、美术风格、完整玩法边界仍需在阶段开始前确认。

旧肉鸽玩法、旧 UI Prefab、旧视觉资产、演示截图和阶段路线已移除，仍然禁止作为默认内容恢复。

## 项目定位

- 当前项目保留 TEngine 框架、启动流程、热更入口、基础 UI 模块、配置生成链路、Unity 包依赖和必要工程设置。
- 当前 Dev 分支已接入 Cultivation 原型入口；它是当前默认原型方向，不等于最终产品方向已经锁定。
- 后续新增阶段必须先确认该阶段的产品目标、玩法边界、UI 风格和验收标准。
- Editor 快速入口只视为原型调试入口；正式验收应优先走 Procedure -> GameApp -> Cultivation 业务入口，并区分两条链路的验证结果。
- 不允许把旧肉鸽项目的玩法、UI 风格或资源作为默认方向继续继承。

## 默认协作规则

- 涉及产品方向、玩法设计、UI 风格、资源选择、架构边界、批量删除或批量替换时，先确认需求和限制，再执行。
- 小修、只读检查、构建验证和明确的清理任务可以直接执行。
- 文档默认使用中文。
- 当前 Cultivation 原型相关工作优先先收束主循环、验收标准和配置/模块边界，再继续堆叠零散功能。
- 不允许让 UI Service 逐步变成业务总控；新增业务能力应逐步沉淀为清晰的 Cultivation 业务模型、系统或模块。
- 当前会话用户明确指令优先于本文件。

## 保留内容边界

优先保留和复用：

- `UnityProject/Assets/TEngine`
- `UnityProject/Assets/Launcher`
- `UnityProject/Assets/GameScripts/GameEntry.cs`
- `UnityProject/Assets/GameScripts/Procedure`
- `UnityProject/Assets/GameScripts/HotFix/GameLogic/GameApp.cs`
- `UnityProject/Assets/GameScripts/HotFix/GameLogic/GameModule.cs`
- `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame`
- `UnityProject/Assets/GameScripts/HotFix/GameLogic/Module/UIModule`
- `UnityProject/Assets/GameScripts/HotFix/GameProto`
- `UnityProject/Assets/Editor`
- `UnityProject/Assets/Scenes/main.unity`
- `UnityProject/Packages`
- `UnityProject/ProjectSettings`
- `Configs/GameConfig`
- `Books`
- `Doc/框架使用指南.md`
- `Doc/Unity MCP 在 Codex 中连接固化说明.md`
- `Doc/Unity MCP 增强操作入口.md`

默认不要恢复：

- 旧肉鸽玩法代码
- 旧 Battle 系列 UI
- 旧 UI Prefab
- `Roguelike_StarUI_*` 和其他旧肉鸽美术资源
- 旧演示截图、Steam 展示图、阶段路线和开发日志
- Unity `UserSettings` 布局噪声

## 新阶段开工流程

1. 先写一页产品合同或方向说明：题材、视角、核心循环、目标手感、明确禁区。
2. 再写阶段计划：每次只推进一个可验收阶段。
3. 先做无美术或低美术依赖的垂直切片。
4. UI 必须先做参考图和黄金样板，确认后再落 Prefab。
5. 新增配置时优先走 `Configs/GameConfig` 和 Luban 生成链路。
6. 新增全局模块时优先通过 `GameModule` 暴露。
7. 新增阶段交付说明统一放入 `Doc/交付说明/`，不要继续散放在 `Doc/设计文档/` 根目录。
8. 当前 Cultivation 原型阶段需要同时说明 Editor 快速入口和正式启动入口的验收差异。

## TEngine 优先原则

以下能力优先使用 TEngine 或当前骨架已有方案：

- 资源加载
- UI 窗口管理
- 配置表
- 事件
- 流程
- 计时器
- 对象池/内存池
- 音频
- 本地化
- 调试能力

不要为新玩法提前建设第二套平行框架。

## 验证要求

完成代码改动后优先执行：

```bash
dotnet build UnityProject\UnityProject.sln --no-restore
git diff --check
```

涉及 Unity 运行行为时，使用真实 Unity MCP 验证：

- 能读取 Editor 状态。
- 至少一个操作型工具可用，例如 `read_console`、`manage_scene`、`find_gameobjects`、`manage_camera` 或 `execute_code`。
- 控制台无新增 Error。
- 必要时运行 PlayMode 测试。

仅 MCP 配置显示 enabled 不代表验收通过。

## 提交建议

- 清理框架骨架、产品方向、新玩法阶段、UI 样板应拆成独立提交。
- 不要把 Unity 布局、ProjectSettings 漂移和业务改动混在一次提交里。
- Codex 创建的提交信息必须以 `codex:` 开头，用于和用户本人或其他来源的提交区分。
