# Phase55 交付说明 - 五雷正法中期卡接入

## 结果摘要

本阶段开始补天雷阁灵品中期卡池，接入设计文档中的 `五雷正法`。这张卡作为天雷阁招牌中期牌，已经具备伤害、概率眩晕、概率连锁三重威胁，并进入天雷阁奖励池。

当前实现范围：

- `五雷正法`：3 灵，造成 8 伤害，50% 概率连锁 4 伤害，50% 概率眩晕 1 回合。
- `五雷正法·强`：伤害提升到 13。
- `五雷正法·极`：在强分支上将眩晕概率提升到 75%。
- `五雷正法·爆`：在强分支上将连锁伤害提升到 7。
- `五雷正法·稳`：眩晕概率和连锁概率都提升到 75%。
- `五雷正法·速`：在稳分支上将灵力消耗降为 2。
- `五雷正法·灭`：在稳分支上改为必定眩晕，伤害降为 5。

## 实际改动清单

- 修改 `UnityProject/Assets/GameScripts/HotFix/GameLogic/CultivationCardGame/BattleEngine.cs`
  - `CultivationSeedData` 新增 `FiveThunderOrthodoxy`。
  - 使用现有 `ChanceChainDamage` 表达“首段伤害 + 概率连锁”。
  - 使用现有 `ChanceStun` / `Stun` 表达概率眩晕和必定眩晕。
  - `CreateThunderSectRewardPool()` 新增 `reward_five_thunder_orthodoxy`。
- 修改 `UnityProject/Assets/Tests/EditMode/CultivationBattleEngineTests.cs`
  - 新增 `FiveThunderOrthodoxyCanDamageStunAndChain`，覆盖伤害、眩晕、连锁三类效果同牌触发。
- 修改 `UnityProject/Assets/Tests/EditMode/CultivationRunEngineTests.cs`
  - 天雷阁开局测试确认 `五雷正法` 进入奖励池但不进入初始牌组。
  - 新增 `FiveThunderOrthodoxyFirstLayerUpgradesKeepSecondLayerChoices`，覆盖完整二层升级树。

## 验证与结果

- `dotnet build UnityProject\UnityProject.sln --no-restore`
  - 结果：通过，0 Error。
  - 备注：仍存在项目既有 warning，包括 `USG0001`、`CS8632`、`System.Net.Http` / `System.IO.Compression` 版本冲突。
- Unity MCP
  - 结果：未通过正式验收。
  - `mcpforunity://editor/state` 可读取，但返回 `ready_for_tools=false`，阻塞原因为 `stale_status`。
  - `read_console(action="get", types=["error"])` 超时。
  - `manage_scene(action="get_active")` 超时。
  - 定向 `run_tests(EditMode)` 未启动成功，返回 `No Unity Editor instances found. Please ensure Unity is running with MCP for Unity bridge.`，目标测试包括：
    - `GameLogic.Tests.CultivationBattleEngineTests.FiveThunderOrthodoxyCanDamageStunAndChain`
    - `GameLogic.Tests.CultivationRunEngineTests.FiveThunderOrthodoxyFirstLayerUpgradesKeepSecondLayerChoices`
    - `GameLogic.Tests.CultivationRunEngineTests.StartRunCanUseThunderSectStarterDeck`
  - 结论：本阶段新增代码和测试已通过 C# 编译，但 Unity Test Framework 实跑与控制台无 Error 验收仍待 MCP 恢复后补齐。

## 假设与风险

- `五雷正法` 的“伤害 + 连锁”用 `ChanceChainDamage` 合并表达，卡牌摘要会显示为一条“造成 X 伤害，Y% 概率连锁 Z 伤害”，再显示一条概率眩晕文本。
- `五雷正法·灭` 使用必定眩晕 `Stun`，不再经过概率判定；这符合设计文档“眩晕变为必定触发但伤害降为 5”。
- 当前仍缺少 Unity Editor 内测试实跑和控制台无 Error 验收，原因是 Unity MCP 连接状态不稳定。

## 可选下一步

- Phase56：继续接入天雷阁灵品卡 `雷霆万钧`，补多段攻击与概率连锁表达。
- MCP 恢复后补跑 Phase49 到 Phase55 的天雷阁定向 EditMode 测试与完整 `GameLogic.EditModeTests`。
