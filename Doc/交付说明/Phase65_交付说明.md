# Phase65 交付说明 - 魔道血祭循环最小可玩切片

## 结果摘要

本阶段补齐六门派目标中的第六个门派：魔道。当前按“可选门派开局、可运行血祭/噬血/低血爆发/损血档位伤害/受击回血核心循环、可进入奖励池、可被固定种子自动走局跑通”的最小范围落地。

魔道当前战斗定位为 HP 资源化、高风险高回报和低血爆发。实现重点是先让魔道拥有区别于剑宗、火云宗、天雷阁、玄黄宗和药王谷的战斗闭环，而不是一次性补完整设计文档中的全部 27 张卡、献祭 UI、永久削弱、禁术法宝或完整平衡。

## 实际改动清单

- 魔道核心机制
  - `CardEffectType` 新增 `BloodSacrifice`、`LowHpDamage`、`MissingHpDamage`、`LowHpShield`、`BloodGuardHeal`。
  - `CombatantState` 新增血祭扣血保底逻辑，血祭最多扣到 1 HP，不会直接由出牌代价导致战斗失败。
  - 新增低血百分比判断和损失 HP 10% 档位计算，用于暗影刺和血祭狂战。
  - `BattleState` 新增受击回血状态，`BattleEngine` 在敌人攻击命中后触发噬血护体回复。
- 魔道门派入口
  - `CultivationSect` 新增 `Demonic`。
  - 原型 Run UI 新增“魔道开局”按钮。
  - Presenter 和战斗 UI 增加“魔道”“血祭”“低血伤害”“损血档位”“血护”等展示。
  - 固定种子自动走局支持指定魔道门派。
- 魔道卡牌池
  - 新增起始牌：血祭掌、噬血爪、暗影刺、血肉盾、暗影遁、噬血护体。
  - 新增奖励牌：血雾、血祭大法、血祭狂战、天魔解体。
  - 新增 `CreateDemonicSectStarterDeck()` 与 `CreateDemonicSectRewardPool()`。
  - `CreateStarterDeck`、`CreateRewardPool` 和分支路线奖励池接入魔道。
- 自动走局
  - `CultivationRunAutoPlayer` 增加对血祭代价、低血伤害、损血档位伤害、低血护盾和受击回血的评分。
  - 血祭在低血危险区会被显著扣分，避免固定种子自动走局无脑自伤翻车。

## 验证与结果

- 已执行 `dotnet build UnityProject\UnityProject.sln --no-restore`。
  - 结果：通过，0 Error。
  - 仍有项目既有 warning：AdditionalFile、nullable 注释上下文、`System.Net.Http` / `System.IO.Compression` 版本冲突。
- 已新增 EditMode 覆盖：
  - 血祭不会把玩家扣到 0，且后续伤害正常结算。
  - 暗影刺低血额外伤害。
  - 血祭狂战按损血 10% 档位增伤。
  - 噬血护体受击回血。
  - 魔道起始牌组数量、核心卡和核心效果。
  - 魔道 Run 起始牌组和奖励池分发。
  - 魔道 UI 开局按钮和 Presenter 门派展示。
- 已新增 PlayMode 覆盖：
  - `FixedSeedAutoPlayCanDriveDemonicSectPrototypeRoute`，验证原型 UI 可以从魔道开局推进到当前路线完成。

## 假设与风险

- 本阶段将“血祭”设计为扣到最低 1 HP，不允许出牌代价直接杀死玩家。这是为了保证当前最小可玩闭环和自动走局稳定，后续如果要实现更硬核的禁术自杀风险，需要单独设计死亡/保命/确认 UI。
- “献祭”牌库/手牌资源、“永久削弱”“完整禁术法宝”“完整 27 张卡”和完整魔道平衡不在本阶段范围。
- 当前魔道数值是原型可玩值，后续需要结合固定种子战斗日志、截图和人工试玩进一步调参。

## 可选下一步

- Phase66 可继续做六门派完整卡池补齐，优先把魔道 27 张卡扩完整。
- 也可以先做一次六门派平衡回归，输出固定种子通关率、平均剩余 HP、平均出牌数和死亡点统计。
- 若要强化魔道特色，可单独开阶段实现“献祭”选择 UI 与禁术永久代价。
