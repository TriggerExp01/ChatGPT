# Phase72 交付说明：不灭金丹致死保护接入

## 结果摘要

本阶段按设计文档接入通用法宝 `不灭金丹` 的核心规则：受到致命伤害时保留 1 HP，每 Run 仅触发 1 次。

范围控制在纯逻辑、测试和展示数据，不修改 UI Prefab、场景、视觉资源、地图可视范围或秘境概率事件。

## 实际改动清单

- 法宝数据
  - 新增 `ImmortalGoldenCoreArtifact`，ID 为 `immortal_golden_core`。
  - 新增 `ArtifactEffectType.FatalDamageSurviveOncePerRun`。
  - 将 `不灭金丹` 加入通用法宝奖励池。
- Run 状态
  - `CultivationRunState` 新增 `FatalDamageSurviveCharges`。
  - 通过统一 `AddArtifact()` 入口获得法宝时初始化每 Run 触发次数。
  - 秘境获得法宝也改为走统一 `AddArtifact()` 入口，避免绕过 Run 级法宝效果。
- 战斗状态
  - `BattleState` 新增战斗内致死保护次数和触发状态。
  - 开战时将 Run 剩余次数注入当前战斗。
  - 战斗结算时将剩余次数回写到 Run。
- 战斗结算
  - 敌人攻击致死后可触发 `不灭金丹`。
  - 玩家回合开始的灼烧、中毒等持续伤害致死后也可触发 `不灭金丹`。
  - 触发优先级为临时免死 `不死魔身` 先结算，若玩家仍处于 0 HP，再尝试消耗 `不灭金丹`。
- Presenter 展示
  - 新增 `不灭金丹` 效果摘要。
  - 新增图标键 `artifact_immortal_core`。
  - 战斗中展示 `致命保护剩余 N 次` 或 `致命保护已消耗`。
- 测试
  - 覆盖敌人攻击致死时触发且只触发一次。
  - 覆盖灼烧/中毒这类回合开始伤害致死时触发。
  - 覆盖 Run 内获得、注入战斗、触发后回写、下一场不再生效。
  - 覆盖法宝池包含关系和 Presenter 展示数据。

## 验证与结果

- 已执行：`dotnet build UnityProject\UnityProject.sln --no-restore`
  - 结果：通过。
  - 备注：仍有项目既有 warning，包括 `USG0001`、`CS8632`、`MSB3277`，本阶段未新增构建错误。
- 已执行：`git diff --check`
  - 结果：通过。
  - 备注：仅有 Git CRLF 提示，无 whitespace error。
- 已执行：Unity MCP `refresh_unity(mode=if_dirty, scope=scripts, compile=request, wait_for_ready=true)`
  - 结果：通过；工具从 Unity domain reload 断连中自动恢复，Editor ready。
- 已执行：Unity MCP 目标 EditMode 测试：
  - `GameLogic.Tests.CultivationBattleEngineTests`
  - `GameLogic.Tests.CultivationRunEngineTests`
  - `GameLogic.Tests.CultivationRunPrototypePresenterTests`
  - 结果：`209/209 passed`。
- 已执行：Unity MCP `read_console(types=["error"])`
  - 结果：`0` 条 Error。

## 假设与风险

- 新增法宝定义里的名称和描述使用 ASCII，避免 `BattleEngine.cs` 现有历史编码文本在补丁中被破坏；正式中文展示由 Presenter 的效果摘要承担。
- 新增触发日志使用英文稳定字符串，延续 Phase71 对新增高阶法宝日志的处理方式；后续可集中整理战斗日志中文文案。
- `不灭金丹` 当前只覆盖战斗内玩家受到致死伤害的场景；秘境事件直接扣 HP、地图外损血等系统尚未形成统一伤害入口，后续接入这些系统时需要复用同一 Run 级致死保护规则。

## 可选下一步

- 继续接入 `灵兽袋`，需要明确随机丹药来源、丹药槽满时处理和战斗计数触发时机。
- 或接入 `破障珠`，需要先完成秘境事件负面结果概率模型。
- 或开始集中整理新增法宝的战斗日志中文文案。
