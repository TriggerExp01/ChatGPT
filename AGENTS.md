# AGENTS.md - TEngine_Game Codex Game Studio 规则

本文件是当前 Unity 项目 `TEngine_Game` 的 Codex 总入口规则。目标是让 Codex 像一个精简游戏开发工作室一样执行任务，但不引入过重流程。

## 0. 项目定位

- 当前项目是基于 Unity + TEngine 的类《吸血鬼幸存者》2D 自动战斗肉鸽。
- 当前工程不是裸 Unity 项目，必须优先遵守 TEngine 框架、热更入口、模块边界、UI 模块、资源模块和配置表链路。
- 当前开发策略是阶段制推进：每次只推进一个阶段，通过 Unity 运行验收后再进入下一阶段。
- 用户可见文档、项目说明、开发日志优先使用中文。

## 1. 每次任务开始前必须读取的文档

除非用户明确要求只回答常识或只做极小修改，否则 Codex 执行项目任务前必须先读取：

1. `Doc/文档索引.md`
2. `Doc/类吸血鬼幸存者阶段开发路线.md`
3. `Doc/肉鸽系统设计总览.md`
4. `Doc/框架使用指南.md`
5. `Doc/开发日志.md`

涉及 Unity MCP、Play Mode、Editor 状态读取、控制台检查时，再读取：

6. `Doc/Unity MCP 在 Codex 中连接固化说明.md`

如果上述文档与实际代码不一致，以实际代码为准，但必须在最终回复中说明差异，并建议更新文档。

## 2. 阶段推进规则

- 以 `Doc/类吸血鬼幸存者阶段开发路线.md` 作为唯一阶段依据。
- 每次只推进当前阶段，不主动跨阶段堆功能。
- 当前阶段未通过 Unity 运行验收前，不进入下一阶段。
- 不为未来阶段提前建设复杂系统。
- 对象池、Luban 配置化、内容扩展等必须按照路线文档后置，除非用户明确要求提前处理。

当前路线中如阶段 7、8、9、13 等仍未完成，Codex 必须严格按路线顺序处理。

## 3. TEngine 优先原则

实现任何新功能前，先检查并复用：

1. 当前项目已有业务代码和 `Doc`
2. `GameModule` 暴露的模块入口
3. `Books`
4. `UnityProject/repowiki/zh/content`
5. `UnityProject/Assets/TEngine`

以下能力优先使用 TEngine 或项目已有方案，不重复建设第二套：

- UI
- 资源加载
- 配置表
- 事件
- 流程
- 计时器
- 对象池
- 音频
- 本地化
- 调试能力

## 4. 禁止事项

除非用户明确要求，否则禁止：

- 主动引入 Addressables。
- 随意修改 `UnityProject/Assets/TEngine/**`。
- 删除或批量重建 `.meta` 文件。
- 把玩法规则长期写死在 UI 脚本中。
- 把内容定义堆在 `GameApp` 中。
- 绕开 `GameModule` 到处直接调用 `ModuleSystem.GetModule<T>()`。
- 为未确认需求提前增加复杂抽象、接口、基类、技能树、词条系统、稀有度系统、武器进化系统。
- 一次性大重构 `RoguelikeGame`，除非当前阶段已经被其结构明显阻碍。
- 修改构建、热更、资源流程前不先读取框架说明。

## 5. 推荐角色分工

当前项目使用精简 Game Studio 角色，不使用完整 49 Agent 模型。

- Producer：阶段推进、范围控制、验收边界。
- Game Designer：玩法设计、构筑、数值节奏。
- Unity Architect：TEngine 边界、热更边界、模块边界。
- Gameplay Programmer：战斗、敌人、武器、升级、掉落。
- UI Programmer：HUD、三选一、结算、提示反馈。
- Resource Pipeline Engineer：YooAsset、Prefab、美术资源、音频资源。
- Config Engineer：Luban 配置化、敌人表、武器表、升级表、刷怪表。
- QA Tester：Unity Play Mode、控制台、验收记录。
- Performance Analyst：GC、对象池、投射物、敌人数量、运行时实例数量。

Codex 不需要在回复中模拟多人对话，但要在内部按这些职责审查方案。

## 6. 任务等级

- L1：小修、文案、注释、日志。可直接修改，执行最小验证。
- L2：单一模块小功能。读取相关文档和代码后修改。
- L3：新玩法功能。必须读取阶段路线、系统总览、框架指南、开发日志和相关代码。
- L4：架构、重构、资源管线、配置化、热更、CI。必须先给方案，再执行最小安全改动。

如果用户要求“一步到位”，仍然必须保持当前阶段边界，不把未验收阶段混在一起。

## 7. 编码原则

- 简洁、高效、易读。
- 优先采用最直接且符合当前架构的实现。
- 不为未确认的未来需求提前增加抽象层。
- 简单逻辑不拆成大量零散类和文件。
- 只有实际重复、复杂度上升、性能瓶颈或框架既有模式要求时，才做拆分。
- 性能优化必须有依据；没有测量结果时，优先保证正确性和可维护性。

## 8. 验证要求

完成代码任务后，尽量执行：

```bash
dotnet build UnityProject.sln --no-restore
git diff --check
```

涉及 Unity 运行行为时，还应使用 Unity MCP 或手动方式验证：

- Unity Editor 已打开当前项目。
- `editor/state` 中 `ready_for_tools = true`。
- 控制台无新增 Error。
- Play Mode 验证当前阶段核心行为。

Unity 验收优先级：

1. 优先使用 Unity MCP 读取 `editor/state`、检查控制台并执行 Play Mode 验收。
2. Unity MCP 必须是真实可用连接：仅 `codex mcp list` 显示 enabled 不算通过；必须至少能读取 `mcpforunity://editor/state`，并且关键操作工具如 `read_console`、`manage_editor`、`find_gameobjects` 或 `manage_camera` 能实际执行。
3. 如果当前 Codex 线程没有 Unity MCP 工具，必须先尝试按 `Doc/Unity MCP 在 Codex 中连接固化说明.md` 恢复或连接 MCP。
4. 如果 Unity MCP 只能读取资源但操作型工具持续返回取消或不可用，视为 MCP 未满足验收条件，必须停止阶段推进并报告卡点。
5. 在 Unity MCP 未恢复前，不使用自建 Editor 验收器、batchmode、桌面按键模拟等方式替代项目既定验收。
6. 如果仍无法实际执行 Unity Play Mode，必须在最终回复中明确说明失败原因，并停止进入下一阶段。

每个阶段完成代码修改并通过验收后，必须执行一次 `git commit`，提交信息使用中文并说明阶段编号与核心改动。

## 9. 文档维护

完成阶段性任务后，应更新：

- `Doc/开发日志.md`
- 必要时更新 `Doc/类吸血鬼幸存者阶段开发路线.md`
- 必要时更新 `Doc/2D肉鸽最小可玩版说明.md`
- 必要时更新 `Doc/肉鸽系统设计总览.md`

不要创建大量重复文档。当前结论优先收敛到 `Doc/文档索引.md` 中列出的有效文档。

## 10. 最终回复格式

完成项目任务后，最终回复必须包含：

1. 完成内容
2. 改动文件
3. 为什么这样改
4. 验证结果
5. 风险点或未完成项
6. 下一步建议

如果执行失败，必须说明失败位置、已完成部分和下一步修复建议。
