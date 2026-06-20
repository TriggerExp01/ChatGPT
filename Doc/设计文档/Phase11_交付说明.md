# Phase 11 交付说明：剑宗战斗关键词最小切片

> 日期：2026-06-20  
> 阶段：Phase 11  
> 范围：为剑宗原型补齐剑气印记、锋锐、多段伤害三类战斗关键词  
> 状态：已通过当前阶段验收

## 1. 结果摘要

本阶段延续 Phase 10 的剑宗初始牌组升级切片，把部分二层终态从占位数值效果替换为真实战斗关键词：

- 新增 `剑气印记`：敌人每叠满 3 层引爆一次，造成 12 点伤害，并保留余数层数。
- 新增 `锋锐`：玩家获得临时无视防御能力，卡牌伤害结算时按锋锐值降低目标有效防御。
- 新增 `多段伤害`：同一个伤害效果可以重复结算，每一击独立吃防御、护盾和击杀判定。
- 原型文本现在可以显示多段伤害、剑气印记和锋锐，便于继续用运行时 UI 验证。

## 2. 实际改动清单

- 扩展 `CardEffect`：
  - 新增 `RepeatCount`，用于表达多段伤害。
  - 保持默认值为 1，不影响既有单段卡牌。
- 扩展 `CardEffectType`：
  - 新增 `SwordMark` 与 `Sharpness`。
- 扩展 `CombatantState`：
  - 新增剑气印记层数、锋锐值、锋锐持续回合。
  - 伤害结算支持按锋锐值无视防御。
- 更新 `BattleEngine`：
  - 伤害效果通过统一多段结算入口处理。
  - 剑气印记叠满 3 层时自动引爆。
  - 锋锐效果在玩家回合开始时递减持续回合。
- 更新 `CultivationSeedData`：
  - `连珠剑气` 改为 `6 伤害 × 2`。
  - `寒光剑气` 改为 `8 伤害 + 锋锐 3 / 2 回合`。
  - `剑意步` 改为 `4 护盾 + 抽 2 + 1 层剑气印记`。
- 更新 `CultivationRunPrototypePresenter`：
  - 补充关键词摘要显示。
  - 战斗状态文本显示玩家锋锐与敌人剑气印记。
- 更新 EditMode 测试：
  - 覆盖剑气印记引爆、锋锐无视防御、多段伤害逐击结算。
  - 覆盖关键词文本格式化。
  - 更新剑气诀二层升级 ID 期望。

## 3. 当前覆盖能力

- 剑宗已经具备“先叠印记、叠满引爆”的最小规则。
- 二层升级中的 `寒光剑气` 可以作为高防御敌人的对策牌。
- 二层升级中的 `连珠剑气` 可以表达未来连击/多次触发类设计的基础结算形式。
- 当前 `剑意步` 已经不只是数值防御牌，而是可以为剑气体系做准备。

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

- `refresh_unity(mode=force, scope=scripts, compile=request, wait_for_ready=true)`：通过；期间出现一次常见域重载断连并自动恢复。
- `GameLogic.EditModeTests`：36/36 通过。
- `execute_code` 探针：验证 `寒光剑气` 获得锋锐后，后续 `剑气诀` 对 2 防御石魔造成满额 8 伤害；再验证 `连珠剑气` 对 2 防御石魔按 6 伤害 2 次结算，最终返回 `PASS sharp=3, hpAfterSharp=32, hpAfterMulti=32`。
- 最终 `read_console(types=["error","warning"])`：0 条。

## 5. 假设与风险

- 剑气印记引爆伤害暂定为 12，后续需要进入平衡表或配置表。
- 锋锐目前只挂在玩家身上，尚未支持敌人锋锐或单张卡临时锋锐。
- 多段伤害目前只服务伤害效果，不额外触发“连击计数”“每击附加印记”等未来协同。
- `消耗` 牌属于 Run 内牌组生命周期规则，本阶段未实现，建议作为独立阶段推进。
