# Phase 12 交付说明：Run 内消耗牌规则

> 日期：2026-06-20  
> 阶段：Phase 12  
> 范围：补齐“消耗”关键词的战斗区与 Run 牌组生命周期规则  
> 状态：已通过当前阶段验收

## 1. 结果摘要

本阶段把 GDD 和卡池设计中反复出现的“消耗”关键词落到可运行规则中：

- 战斗中打出的消耗牌不再进入弃牌堆，而是进入单独的消耗区。
- 当前战斗结束并胜利结算时，Run 牌组会移除本战斗消耗过的卡牌。
- 非消耗牌仍保持原有抽牌、弃牌、洗牌循环。
- 原型文本摘要可以显示“消耗”关键词。

## 2. 实际改动清单

- 扩展 `CardEffectType`：
  - 新增 `Exhaust`。
- 扩展 `BattleState`：
  - 新增 `ExhaustPile`，用于记录当前战斗中已消耗的卡牌。
- 更新 `BattleEngine`：
  - 出牌后根据卡牌效果判断进入 `DiscardPile` 或 `ExhaustPile`。
  - `Exhaust` 本身不产生数值效果，只决定卡牌去向。
- 更新 `CultivationRunEngine`：
  - 胜利结算时读取 `CurrentBattle.ExhaustPile`，从 Run 牌组移除对应卡牌。
  - 优先按卡牌实例移除，实例找不到时按卡牌 ID 兜底。
- 更新 `CultivationSeedData`：
  - `大回春丹` 改为恢复 10 HP 且使用后消耗，作为当前最小可验证消耗牌。
- 更新 `CultivationRunPrototypePresenter`：
  - 卡牌摘要显示“消耗”。
- 更新 EditMode 测试：
  - 覆盖消耗牌进入消耗区、不进入弃牌堆。
  - 覆盖战斗胜利后 Run 牌组移除消耗牌。
  - 覆盖非消耗牌仍保留在 Run 牌组。
  - 覆盖文本摘要显示“消耗”。

## 3. 当前覆盖能力

- 通用丹药、禁术、一次性强力牌已经有可复用的生命周期规则。
- 后续实现 `诛仙剑阵`、`人剑合一`、`不灭剑体` 等高稀有度牌时，可以直接复用 `Exhaust` 效果。
- 规则目前只在胜利结算时同步回 Run 牌组；失败结算不再关心当前 Run 牌组变化。

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
- `GameLogic.EditModeTests`：39/39 通过。
- `execute_code` 探针：构造 0 费消耗终结牌，打出后确认战斗 `ExhaustPile=1`，胜利结算后 Run 牌组中该牌被移除；返回 `PASS deck=4, exhausted=1`。
- 最终 `read_console(types=["error","warning"])`：0 条。

## 5. 假设与风险

- 当前消耗规则是“本 Run 永久移除”，符合设计文档对禁术和一次性牌的说明；尚未支持“仅本场战斗移除但 Run 内保留”的牌。
- 同 ID 多实例卡牌通过实例优先移除，正常牌组可准确删除被打出的那一张；序列化/配置化后仍需确认实例身份是否可保持。
- 当前 `Exhaust` 用 `CardEffect` 表示，为了快速接入现有效果系统；后续如果卡牌标签增多，可考虑拆成 `CardKeyword` 或 `CardTrait`。
