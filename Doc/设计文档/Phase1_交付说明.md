# Phase 1 交付说明：纯逻辑战斗核心

> 日期：2026-06-20  
> 阶段：Phase 1  
> 范围：单场卡牌战斗纯 C# 逻辑  
> 状态：已通过当前阶段验收

## 1. 结果摘要

本阶段已按 `Phase1_原型开发合同.md` 落地《仙途·天命》的最小战斗闭环：可以用代码创建“剑宗初始牌组 vs 炼气期敌人”的单场战斗，并验证抽牌、灵力、出牌、伤害、防御、敌人意图、破防、灼烧和胜负判定。

实现保持为纯逻辑层，不依赖 Unity 场景对象、Prefab、UI 或动画。

## 2. 实际改动清单

- 新增 `GameLogic.Cultivation` 纯逻辑命名空间。
- 新增战斗核心类型：
  - `BattleEngine`
  - `BattleState`
  - `CombatantState`
  - `CardDefinition`
  - `CardEffect`
  - `EnemyDefinition`
  - `EnemyState`
  - `EnemyIntent`
  - `BattleLogEntry`
- 新增种子数据 `CultivationSeedData`：
  - 剑宗初始牌组：剑气诀、破甲符、护体真气、剑步、轻身术、回春丹。
  - 炼气期敌人：石魔、火蝠。
- 新增 EditMode 测试程序集 `GameLogic.EditModeTests`。
- 修改 `GameApp.StartGameLogic()`，在热更入口创建一场 Phase 1 种子战斗并输出原型可调用日志。

## 3. 已覆盖规则

- 回合开始抽牌到手牌上限。
- 玩家灵力每回合刷新。
- 打牌消耗灵力。
- 攻击造成伤害。
- 防御获得护盾。
- 敌人意图可见。
- 结束回合后敌人按意图行动。
- 敌人防御意图能在下一玩家回合形成护盾压力。
- 破防会放大后续伤害。
- 灼烧会在回合开始造成持续伤害。
- 战斗能判定胜利或失败。

## 4. 验证与结果

已执行并通过：

```powershell
dotnet build UnityProject\UnityProject.sln --no-restore
git diff --check
```

Unity MCP 验收：

- `refresh_unity` 可刷新脚本并等待 Editor ready。
- `GameLogic.EditModeTests`：9 个 EditMode 测试全部通过。
- `read_console`：0 条 Error。

当前 `dotnet build` 仍存在项目既有警告，包括 AdditionalFile、nullable 注释上下文和 Unity/MCP 相关程序集版本冲突；这些警告不阻塞 Phase 1，且本阶段未扩大处理。

## 5. 假设与风险

- 当前数值按纸面玩测试方案的最小范围落地，尚未经过真实游玩平衡验证。
- `CultivationSeedData` 是临时种子数据，后续迁移到 Luban 配置表时应保持数据结构可替换。
- 当前只实现单敌人核心路径，多敌人、召唤、AOE、奖励、Run 地图等不在 Phase 1 范围。
- Godot 参考项目本阶段只作为玩法与效果参考，不直接迁移源码或资源。

## 6. 下一阶段边界

Phase 2 的第一目标是把同一套 `BattleEngine` 绑定到最小测试界面，用于可视化验证战斗闭环。

Phase 2 不应重写 Phase 1 规则；如发现规则问题，应先补测试并在逻辑层修正，再接 UI。
