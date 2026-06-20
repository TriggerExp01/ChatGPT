# Phase 4 交付说明：Run 持久生命与闭关恢复

> 日期：2026-06-20  
> 阶段：Phase 4  
> 范围：在 Phase 3 最小 Run 推进核心基础上，补齐 Run 内 HP 持久化与闭关恢复节点  
> 状态：已通过当前阶段验收

## 1. 结果摘要

本阶段把 HP 从“每场战斗重置”的临时战斗资源，推进为 Run 内持久资源：

- Run 开始时记录玩家最大 HP 与当前 HP。
- 每场战斗按 Run 当前 HP 创建玩家状态。
- 战斗胜利后保存战斗结束时的当前 HP。
- 下一场战斗继承上一次战斗后的 HP。
- 战斗失败后 Run 标记失败，Run 当前 HP 归零。
- 路线支持 `Rest` 闭关节点。
- 闭关节点执行恢复后推进到下一节点，并把恢复后的 HP 带入下一场战斗。

这对应 GDD 中“生命（HP）是不刷新持久资源”和“闭关节点用于恢复 HP”的要求，也对齐纸面测试中“连续战斗继承 HP 状态”的前提。

## 2. 实际改动清单

- 扩展 `CombatantState`：
  - 构造函数支持传入 `currentHp`。
  - 校验当前 HP 必须在 1 到最大 HP 之间。
- 扩展 `BattleEngine`：
  - 新增带 `playerCurrentHp` / `playerMaxHp` 的 `CreateBattle` 重载。
  - 原有 `CreateBattle(deck, enemy)` 保持兼容，默认仍以 100/100 开始。
- 扩展 `CultivationRunState`：
  - 增加 `PlayerMaxHp` 与 `PlayerCurrentHp`。
- 扩展 `CultivationRunNode`：
  - 支持 `Rest` 节点不绑定敌人。
  - 增加 `RestHealAmount`。
- 扩展 `CultivationRunEngine`：
  - `StartRun` 支持传入初始最大 HP 与当前 HP。
  - 进入节点时按节点类型分发：战斗/精英进入战斗，闭关进入恢复状态。
  - 战斗胜利保存当前 HP。
  - 闭关恢复后推进下一节点。
- 扩展默认原型路线：
  - 石魔 -> 火蝠 -> 闭关调息 -> 石魔首领。
- 扩展 `CultivationRunEngineTests`：
  - 覆盖默认路线 HP 初始状态。
  - 覆盖 HP 继承到下一场战斗。
  - 覆盖闭关恢复并进入下一场战斗。
  - 覆盖闭关恢复不超过最大 HP。
  - 覆盖战败后 Run 当前 HP 归零。

## 3. 当前覆盖能力

- Run 中 HP 可跨战斗保存。
- Run 中可插入最小闭关节点。
- 闭关节点可恢复 HP 并继续推进路线。
- Boss 前已有一个闭关准备节点。

## 4. 验证与结果

已执行并通过：

```powershell
dotnet build UnityProject\UnityProject.sln --no-restore
git diff --check
```

Unity MCP 验收结果：

- `refresh_unity`：强制刷新脚本并等待 Editor ready；过程中出现一次 MCP 断连恢复，最终 ready。
- `mcpforunity://editor/state`：Editor ready，未处于 Play Mode，未编译中。
- `GameLogic.EditModeTests`：18 个 EditMode 测试全部通过。
- `execute_code` 默认 Run 探针：
  - 默认路线节点数为 4。
  - 第二战后进入 `node_meditation`，状态为 `Rest`。
  - `playerCurrentHp=60` 时闭关恢复到 90。
  - 闭关后进入 `node_stone_demon_leader`。
  - 石魔首领战斗中的玩家 HP 为 90。
- `read_console`：最终 0 Error / 0 Warning。

当前 `dotnet build` 仍存在项目既有 warning，主要是 Unity/MCP 程序集版本冲突；本阶段未引入新的构建错误。

## 5. 假设与风险

- 闭关目前只实现“恢复 HP”，尚未实现卡牌升级二选一。
- 当前恢复量是路线节点上的固定值，尚未接入配置表或 Luban。
- 只持久化玩家 HP；护盾、灼烧、破防等战斗状态仍在战斗结束后清空。
- 默认路线仍是线性原型路线，不代表正式地图分支。

## 6. 下一阶段建议

Phase 5 可在以下方向中选一个继续推进：

- 闭关节点加入卡牌升级选择。
- 最小路线分支选择。
- 奖励随机抽取与稀有度。
- 正式 Run 调试 UI，展示路线、当前 HP、节点状态与奖励。
