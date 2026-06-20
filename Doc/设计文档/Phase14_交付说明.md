# Phase 14 交付说明：随机三选一奖励抽取

> 日期：2026-06-20  
> 阶段：Phase 14  
> 范围：把 Run 战斗胜利奖励从固定前三张改为可复现的随机 3 选 1  
> 状态：已通过当前阶段验收

## 1. 结果摘要

本阶段收敛 Phase 13 的遗留风险：奖励池已经扩展到剑气印记牌，但奖励候选仍固定取前 3 张，导致后置奖励牌无法在默认流程中自然出现。

现在战斗胜利后的奖励候选会从当前节点奖励池中随机抽取 3 张，且同一轮候选不重复：

- 保留 3 选 1 的原型交互。
- 支持通过 `rewardSeed` 注入奖励随机种子，便于测试和复盘。
- 抽取结果来自节点自己的 `RewardPool`，后续不同节点可以配置不同奖励池。
- `刺击`、`七绝剑气` 等后置奖励牌已经能通过随机抽取进入实际候选。

## 2. 实际改动清单

- 更新 `CultivationRunEngine`：
  - 新增 `_rewardRandom` 奖励随机源。
  - 构造函数新增可选参数 `rewardSeed`，默认保持现有调用兼容。
  - `CreateRewardChoices()` 改为对节点奖励池做局部洗牌并取前 3 张。
  - 奖励池不足 3 张时按实际数量返回，不制造空奖励或重复奖励。
- 更新 `CultivationRunEngineTests`：
  - 原“固定第一奖励”断言改为验证 3 个奖励不重复且全部来自当前节点奖励池。
  - 新增种子随机测试，证明固定种子下可复现抽到后置剑气奖励牌。
  - 更新选奖励推进测试，验证被选中的随机奖励会加入 Run 牌组并进入下一节点。

## 3. 当前覆盖能力

- 奖励系统不再把卡牌池顺序等同于奖励候选顺序。
- 后续扩展更多剑宗牌、通用牌、丹药牌时，不需要手动挪到奖励池前三位才能被玩家看到。
- 随机性仍停留在纯逻辑层，可被 EditMode 测试和 Unity MCP 探针复现。

## 4. 验证与结果

已执行：

```powershell
dotnet build UnityProject\UnityProject.sln --no-restore
git diff --check
```

结果：

- `dotnet build UnityProject\UnityProject.sln --no-restore`：通过；保留既有 AdditionalFile、nullable 注释上下文和 Unity/MCP 程序集版本 warning，无新增编译错误。
- `git diff --check`：通过。

Unity MCP 验收：

- `refresh_unity(mode=force, scope=scripts, compile=request, wait_for_ready=true)`：通过。
- `GameLogic.EditModeTests`：42/42 通过。
- `execute_code` 探针：使用 `rewardSeed: 3` 结算奖励，返回 `PASS rewards=reward_small_restore_pill,reward_thrust,reward_sevenfold_sword_qi`。
- 最终 `read_console(types=["error","warning"])`：0 条。

## 5. 假设与风险

- 当前随机抽取仍是均匀抽取，没有接入 GDD 中的稀有度权重、境界解锁和丹药槽限制；这些应在后续奖励权重阶段单独实现。
- `rewardSeed` 只控制 Run 奖励抽取，战斗洗牌仍由 `BattleEngine` 的种子控制，二者保持分离。
- 本阶段只改奖励候选生成，不改 UI 布局和奖励展示样式。
