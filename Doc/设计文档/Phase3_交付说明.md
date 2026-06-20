# Phase 3 交付说明：最小 Run 推进核心

> 日期：2026-06-20  
> 阶段：Phase 3  
> 范围：在 Phase 1 战斗核心和 Phase 2 原型 UI 之后，补齐可测试的最小 Roguelite Run 状态机  
> 状态：已通过当前阶段验收

## 1. 结果摘要

本阶段实现了一个纯逻辑的最小 Run 推进核心，用于把单场战斗串成第一条可验收路线：

- 从剑宗初始牌组开始 Run。
- 进入第一场战斗。
- 战斗胜利后进入奖励状态。
- 选择奖励会把卡牌加入 Run 牌组，并推进到下一节点。
- 跳过奖励会直接推进。
- 最后一场奖励处理完成后标记 Run 完成。
- 战斗失败会标记 Run 失败。

本阶段仍不进入正式 UI、正式地图、资源链、Prefab 或美术样板制作。

## 2. 实际改动清单

- 新增 `CultivationRunEngine`：
  - `StartRun()` 创建默认 Run 并启动当前节点战斗。
  - `ResolveBattleResult()` 将胜利/失败的战斗结果同步到 Run 状态。
  - `ChooseReward()` 领取奖励并推进。
  - `SkipReward()` 跳过奖励并推进。
- 新增 `CultivationRunState`：
  - 保存 Run 牌组、路线、当前节点、当前战斗、当前奖励、已领取奖励和状态。
- 新增 `CultivationRunNode`：
  - 表达路线节点、节点类型、敌人和奖励池。
- 新增 `CultivationRunReward`：
  - 表达奖励 id 与奖励卡牌。
- 扩展 `CultivationCardEnums`：
  - 增加 `CultivationRunNodeType` 与 `CultivationRunStatus`。
- 扩展 `CultivationSeedData`：
  - 增加精英敌人 `StoneDemonLeader`。
  - 增加奖励卡 `FlyingSword`、`CloudGuard`、`SmallRestorePill`。
  - 增加第一条原型路线：石魔 -> 火蝠 -> 石魔首领。
  - 增加剑宗奖励池。
- 新增 `CultivationRunEngineTests`：
  - 覆盖 Run 创建、胜利进入奖励、领取奖励推进、跳过最终奖励完成 Run、战败结束 Run。

## 3. 当前覆盖能力

- 具备最小路线状态机。
- 具备战斗结果到 Run 状态的衔接。
- 具备战后奖励三选逻辑的代码入口。
- 具备牌组成长的最小闭环。
- 具备第一条纸面测试路线的敌人骨架。

## 4. 验证与结果

已执行并通过：

```powershell
dotnet build UnityProject\UnityProject.sln --no-restore
git diff --check
```

Unity MCP 验收结果：

- `mcpforunity://editor/state`：Editor ready，未处于 Play Mode，未编译中。
- `read_console`：最终 0 Error / 0 Warning。
- `AssetDatabase.ImportAsset` 探针：新增 Run 逻辑脚本和测试脚本均能作为 `MonoScript` 载入。
- `GameLogic.EditModeTests`：15 个 EditMode 测试全部通过。
- `execute_code` Run 探针：
  - 默认路线节点数为 3。
  - 第一节点为 `node_stone_demon`，敌人为 `stone_demon`。
  - 战斗胜利后进入 `Reward`。
  - 奖励为 `flying_sword,cloud_guard,small_restore_pill`。
  - 选择第一个奖励后进入第二节点，牌组从 12 张增长到 13 张，当前敌人为 `fire_bat`。

当前 `dotnet build` 仍存在项目既有 warning，包括 AdditionalFile、nullable 注释上下文，以及 Unity/MCP 程序集版本冲突；本阶段未引入新的构建错误。

## 5. 假设与风险

- 当前 Run 中每场战斗仍以完整玩家血量开始，暂不跨战斗继承血量、护盾、灼烧或其他状态。这样能保持 Phase 3 的边界集中在路线与奖励推进。
- `EnemyIntentType.Summon` 目前仍只是意图日志，不会真正召唤小怪；这是石魔首领数据先接入后的已知简化。
- 当前奖励池是可验证占位，不代表正式掉落权重、稀有度或宗门平衡。
- Run 路线是线性三节点，不包含地图分支、事件、商店、休息或宝箱。

## 6. 下一阶段建议

Phase 4 建议继续保持纯逻辑优先，补齐以下之一：

- 跨战斗生命值与恢复规则。
- 最小路线分支选择。
- 奖励稀有度与随机抽取。
- 石魔首领召唤小怪的战斗规则。
