# Phase 13 交付说明：剑气印记奖励牌闭环

> 日期：2026-06-20  
> 阶段：Phase 13  
> 范围：让剑气印记从战斗关键词扩展为可在奖励池中获得的入门牌与回报牌  
> 状态：已通过当前阶段验收

## 1. 结果摘要

本阶段在 Phase 11 的剑气印记基础规则上，补齐两张可被 Run 奖励获得的剑宗牌：

- `刺击`：1 灵力，造成 5 伤害并施加 1 层剑气印记，是剑气体系入门牌。
- `七绝剑气`：2 灵力，造成 7 伤害，目标每有 1 层剑气印记，额外造成 3 伤害，是剑气体系回报牌。
- 新增 `DamagePerSwordMark` 效果类型，用于表达“按目标当前剑气印记层数追加伤害”。
- 奖励池现在会包含 `刺击` 和 `七绝剑气`，后续 Run 可以围绕剑气印记形成更清晰的构筑方向。

## 2. 实际改动清单

- 扩展 `CardEffectType`：
  - 新增 `DamagePerSwordMark`。
- 更新 `BattleEngine`：
  - 新增按目标剑气印记层数追加伤害的结算分支。
  - 追加伤害作为独立伤害效果结算，会正常受到防御、护盾和锋锐影响。
- 更新 `CultivationSeedData`：
  - 新增 `Thrust` / `刺击`，并提供两条第一层升级分支。
  - 新增 `SevenfoldSwordQi` / `七绝剑气`，并提供两条第一层升级分支。
  - 将两张牌加入 `CreateSwordSectRewardPool()`。
- 更新 `CultivationRunPrototypePresenter`：
  - 卡牌摘要可显示“每层剑气印记 +N 伤害”。
- 更新 EditMode 测试：
  - 覆盖 `刺击 -> 七绝剑气` 的印记入门与回报结算。
  - 覆盖奖励池包含 `刺击` 与 `七绝剑气`。
  - 覆盖文本摘要显示 `DamagePerSwordMark`。

## 3. 当前覆盖能力

- 剑气体系不再只存在于二层升级终态中，已经可以从普通奖励进入牌组。
- `刺击` 负责叠印记，`七绝剑气` 负责根据印记层数兑现伤害。
- 后续可继续接入 `剑御`、`剑气护壁`、`剑气纵横` 等防御/秘术方向的印记牌。

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
- `GameLogic.EditModeTests`：41/41 通过。
- `execute_code` 探针：打出 `刺击` 后保留 1 层剑气印记，再打出 `七绝剑气`，最终返回 `PASS hp=31, marks=1, rewards=True`。
- 最终 `read_console(types=["error","warning"])`：0 条。

## 5. 假设与风险

- `DamagePerSwordMark` 暂不消耗印记，符合当前 `七绝剑气` 基础设计；“爆绝剑气”等消耗印记变体后续需要单独效果。
- 追加伤害目前作为独立伤害效果结算，因此会再次受到防御影响；这与当前引擎的多效果模型一致，但后续可根据平衡需要调整。
- 奖励池仍是固定顺序取前 3 张作为奖励候选，新增牌已进入池但不会在默认前三奖励中稳定出现；后续需要实现随机奖励抽取。
