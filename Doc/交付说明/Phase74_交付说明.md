# Phase74 交付说明：破障珠秘境风险修正接入

## 结果摘要

本阶段按设计文档接入通用法宝 `破障珠` 的核心效果：秘境事件中负面结果概率降低 15%。

由于当前秘境系统此前只有确定性选项，本阶段同步补了最小化的“风险秘境选项”基础设施：秘境选项可以配置成功概率、成功结果和失败结果，并在选择时用固定的 Run 奖励随机源判定。原型展示层会显示修正后的“成功 / 负面”概率。

本阶段不修改 UI Prefab、场景、美术资源、地图视觉，也不恢复旧 Unity 肉鸽玩法或旧 UI。

## 规则决策

- `破障珠` 进入通用法宝池。
- `破障珠` 进入默认坊市，价格为 45 灵石。
- `破障珠` 的法宝效果类型为 `MysticNegativeChanceReduction`，数值为 15。
- 风险秘境选项默认使用成功率表达，负面概率为 `100 - 成功率`。
- 有 `破障珠` 时，负面概率降低 15%，成功率等价提高 15%，上限不超过 100%。
- 当前示例风险秘境为 `Immortal Ruins`：
  - 探索成功率 50%。
  - 成功获得 `Cloud Guard`。
  - 失败损失 10 HP。
  - 离开选项保持确定性安全离开。
- 失败损失 HP 目前保留至少 1 HP，不在本阶段接入 Run 失败或致死处理。

## 实际改动清单

- `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/CultivationRunReward.cs`
  - 新增 `ArtifactEffectType.MysticNegativeChanceReduction`。
  - 新增 `ArtifactDefinition.MysticNegativeChanceReductionPercent`。
  - 为 `MysticEventOption` 新增成功概率、失败效果和失败奖励字段。
  - 新增秘境失败效果类型：`LoseHp`、`LoseSpiritStones`、`LoseArtifact`。
- `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/CultivationRunEngine.cs`
  - `ChooseMysticEventOption()` 接入秘境成功判定。
  - 新增风险选项判定和成功/失败结果分派逻辑。
  - 失败结果支持扣 HP、扣灵石、移除首个法宝。
- `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/BattleEngine.cs`
  - 新增 `CultivationSeedData.BarrierBreakingPearlArtifact`。
  - 将 `破障珠` 加入默认坊市和通用法宝池。
  - 新增示例风险秘境 `ImmortalRuinsMysticEvent`。
- `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/CultivationRunPrototypePresenter.cs`
  - 新增 `破障珠` 中文效果摘要。
  - 新增 `artifact_barrier_pearl` 图标键。
  - 新增秘境选项概率展示摘要，并体现 `破障珠` 后的概率修正。
- `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/CultivationRunPrototypeUI.cs`
  - 秘境按钮描述改用概率展示摘要。
- `UnityProject/Assets/Tests/EditMode/CultivationRunEngineTests.cs`
  - 覆盖法宝池包含 `破障珠`。
  - 覆盖坊市 45 灵石购买 `破障珠`。
  - 覆盖风险秘境失败会触发负面结果。
  - 覆盖 `破障珠` 可把同一秘境掷点从失败修正为成功。
- `UnityProject/Assets/Tests/EditMode/CultivationRunPrototypePresenterTests.cs`
  - 覆盖 `破障珠` 状态摘要、效果摘要和图标键。
  - 覆盖秘境概率展示从 `成功 50% / 负面 50%` 修正为 `成功 65% / 负面 35%`。

## 验证与结果

- `dotnet build UnityProject\UnityProject.sln --no-restore`
  - 结果：通过。
  - 说明：仍存在项目既有 warning，包括 `USG0001`、`CS8632`、`System.Net.Http` / `System.IO.Compression` 版本冲突 warning；本阶段未新增错误。
- `git diff --check`
  - 结果：通过。
  - 说明：仅有 Git 的 CRLF 转换提示，无空白错误。
- Unity MCP `refresh_unity(mode=if_dirty, scope=scripts, compile=request, wait_for_ready=true)`
  - 结果：通过。
  - 说明：刷新期间发生一次 Unity 断连，工具自动重连后 Editor ready。
- Unity MCP `editor/state`
  - 结果：通过。
  - 说明：`ready_for_tools=true`，无阻塞原因。
- Unity MCP 控制台 Error 查询
  - 结果：0 条 Error。
- Unity MCP 目标 EditMode 测试
  - 范围：
    - `GameLogic.Tests.CultivationRunEngineTests`
    - `GameLogic.Tests.CultivationRunPrototypePresenterTests`
  - 结果：138/138 passed。
- Unity MCP 全量 EditMode 测试
  - 结果：259/259 passed。

## 假设与风险

- 当前秘境风险模型是最小可用版本，只支持一个成功概率和一个失败结果；更复杂的多分支权重事件、连续判定、事件状态记忆不在本阶段范围内。
- `LoseHp` 失败结果目前保留至少 1 HP，不会直接导致 Run 失败；后续如果需要地图外损血触发死亡，需要先统一 Run 伤害入口。
- `破障珠` 在 `BattleEngine.cs` 中的名称和描述继续使用英文，避免扩大该文件历史中文编码显示问题；原型展示层已提供中文效果摘要。
- 本阶段没有接入正式图标或 UI Prefab，`artifact_barrier_pearl` 仍是原型展示用图标键，正式视觉需要在 UI 样板确认后处理。

## 可选下一步

- 继续扩展秘境事件池，把现有确定性秘境逐步拆成有风险、有代价、有收益的多种事件。
- 接入下一个通用法宝时，优先选择不依赖地图视觉和正式 Prefab 的纯逻辑效果，保持阶段可验证。
