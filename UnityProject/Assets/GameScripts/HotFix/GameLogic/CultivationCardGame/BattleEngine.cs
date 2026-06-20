using System;
using System.Collections.Generic;
using System.Linq;

namespace GameLogic.Cultivation
{
    public sealed class BattleEngine
    {
        public BattleEngine(int seed = 0)
        {
            Random = new Random(seed);
        }

        private Random Random { get; }

        public BattleState CreateBattle(IEnumerable<CardDefinition> deck, EnemyDefinition enemy)
        {
            return CreateBattle(deck, enemy, 100, 100);
        }

        public BattleState CreateBattle(IEnumerable<CardDefinition> deck, EnemyDefinition enemy, int playerCurrentHp, int playerMaxHp = 100)
        {
            if (deck == null)
            {
                throw new ArgumentNullException(nameof(deck));
            }

            if (enemy == null)
            {
                throw new ArgumentNullException(nameof(enemy));
            }

            if (playerMaxHp <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(playerMaxHp), "Player max HP must be positive.");
            }

            if (playerCurrentHp <= 0 || playerCurrentHp > playerMaxHp)
            {
                throw new ArgumentOutOfRangeException(nameof(playerCurrentHp), "Player current HP must be between 1 and max HP.");
            }

            var player = new CombatantState("修士", playerMaxHp, currentHp: playerCurrentHp);
            var state = new BattleState(player, deck, new[] { new EnemyState(enemy) });
            Shuffle(state.DrawPile);
            StartPlayerTurn(state);
            return state;
        }

        public void StartPlayerTurn(BattleState state)
        {
            EnsureBattleActive(state);

            state.TurnNumber++;
            state.Player.ClearShield();
            state.Player.ResolveSharpnessAtTurnStart();
            state.Spirit = state.SpiritMax;

            var burnDamage = state.Player.ResolveBurnAtTurnStart();
            if (burnDamage > 0)
            {
                state.Logs.Add(new BattleLogEntry($"玩家受到灼烧 {burnDamage} 点伤害。"));
            }

            foreach (var enemy in state.Enemies)
            {
                var enemyBurnDamage = enemy.Body.ResolveBurnAtTurnStart();
                if (enemyBurnDamage > 0)
                {
                    state.Logs.Add(new BattleLogEntry($"{enemy.Body.Name} 受到灼烧 {enemyBurnDamage} 点伤害。"));
                }
            }

            DrawToHandLimit(state);
            RefreshOutcome(state);
        }

        public bool CanPlay(BattleState state, CardDefinition card)
        {
            return state != null
                   && card != null
                   && state.Outcome == BattleOutcome.InProgress
                   && state.Hand.Contains(card)
                   && state.Spirit >= state.GetEffectiveSpiritCost(card);
        }

        public void PlayCard(BattleState state, CardDefinition card, EnemyState target = null)
        {
            if (!CanPlay(state, card))
            {
                throw new InvalidOperationException("Card cannot be played in the current battle state.");
            }

            state.Spirit -= state.GetEffectiveSpiritCost(card);
            state.Hand.Remove(card);

            foreach (var effect in card.Effects)
            {
                ResolveCardEffect(state, card, effect, target);
            }

            if (card.Effects.Any(effect => effect.Type == CardEffectType.Exhaust))
            {
                state.ExhaustPile.Add(card);
                state.Logs.Add(new BattleLogEntry($"{card.Name} 已消耗，本场战斗不会再进入牌库循环。"));
            }
            else
            {
                state.DiscardPile.Add(card);
            }

            state.Logs.Add(new BattleLogEntry($"打出 {card.Name}，剩余灵力 {state.Spirit}。"));
            RefreshOutcome(state);
        }

        public void EndPlayerTurn(BattleState state)
        {
            EnsureBattleActive(state);

            state.DiscardPile.AddRange(state.Hand);
            state.Hand.Clear();

            foreach (var enemy in state.Enemies.Where(enemy => !enemy.Body.IsDefeated).ToList())
            {
                enemy.Body.ClearShield();
                ResolveEnemyIntent(state, enemy);
                enemy.AdvanceIntent();
                if (state.Player.IsDefeated)
                {
                    break;
                }
            }

            RefreshOutcome(state);
            if (state.Outcome == BattleOutcome.InProgress)
            {
                StartPlayerTurn(state);
            }
        }

        private void ResolveCardEffect(BattleState state, CardDefinition card, CardEffect effect, EnemyState explicitTarget)
        {
            switch (effect.Type)
            {
                case CardEffectType.Damage:
                    foreach (var enemy in SelectTargets(state, effect, explicitTarget))
                    {
                        DealCardDamage(state, card, effect, enemy);
                    }

                    break;
                case CardEffectType.Shield:
                    state.Player.AddShield(effect.Value);
                    break;
                case CardEffectType.Draw:
                    DrawCards(state, effect.Value);
                    break;
                case CardEffectType.Heal:
                    state.Player.Heal(effect.Value);
                    break;
                case CardEffectType.BreakDefense:
                    foreach (var enemy in SelectTargets(state, effect, explicitTarget))
                    {
                        enemy.Body.AddBreakDefense(effect.Value);
                    }

                    break;
                case CardEffectType.Burn:
                    foreach (var enemy in SelectTargets(state, effect, explicitTarget))
                    {
                        enemy.Body.AddBurn(effect.Value, Math.Max(1, effect.Duration));
                    }

                    break;
                case CardEffectType.SwordMark:
                    foreach (var enemy in SelectTargets(state, effect, explicitTarget))
                    {
                        var explosionDamage = enemy.Body.AddSwordMark(effect.Value);
                        state.Logs.Add(new BattleLogEntry($"{card.Name} 对 {enemy.Body.Name} 施加 {effect.Value} 层剑气印记。"));
                        if (explosionDamage > 0)
                        {
                            var dealt = enemy.Body.TakeDamage(explosionDamage);
                            state.Logs.Add(new BattleLogEntry($"{enemy.Body.Name} 的剑气印记引爆，造成 {dealt} 点伤害。"));
                        }
                    }

                    break;
                case CardEffectType.DamagePerSwordMark:
                    foreach (var enemy in SelectTargets(state, effect, explicitTarget))
                    {
                        var bonusDamage = enemy.Body.SwordMarkStacks * effect.Value;
                        if (bonusDamage <= 0)
                        {
                            state.Logs.Add(new BattleLogEntry($"{card.Name} 未触发剑气印记追加伤害。"));
                            continue;
                        }

                        var dealt = enemy.Body.TakeDamage(bonusDamage, state.Player.Sharpness);
                        state.Logs.Add(new BattleLogEntry($"{card.Name} 根据 {enemy.Body.SwordMarkStacks} 层剑气印记追加 {dealt} 点伤害。"));
                    }

                    break;
                case CardEffectType.Sharpness:
                    state.Player.AddSharpness(effect.Value, Math.Max(1, effect.Duration));
                    state.Logs.Add(new BattleLogEntry($"{card.Name} 获得锋锐 {effect.Value}，持续 {Math.Max(1, effect.Duration)} 回合。"));
                    break;
                case CardEffectType.Exhaust:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(effect.Type), effect.Type, "Unsupported card effect type.");
            }
        }

        private static void DealCardDamage(BattleState state, CardDefinition card, CardEffect effect, EnemyState enemy)
        {
            for (var hit = 0; hit < effect.RepeatCount; hit++)
            {
                var dealt = enemy.Body.TakeDamage(effect.Value, state.Player.Sharpness);
                var hitText = effect.RepeatCount > 1 ? $" 第 {hit + 1}/{effect.RepeatCount} 击" : string.Empty;
                state.Logs.Add(new BattleLogEntry($"{card.Name}{hitText} 对 {enemy.Body.Name} 造成 {dealt} 点伤害。"));
                if (enemy.Body.IsDefeated)
                {
                    break;
                }
            }
        }

        private static IEnumerable<EnemyState> SelectTargets(BattleState state, CardEffect effect, EnemyState explicitTarget)
        {
            if (effect.Target == CardTarget.EnemyAll)
            {
                return state.Enemies.Where(enemy => !enemy.Body.IsDefeated);
            }

            var target = explicitTarget ?? state.Enemies.FirstOrDefault(enemy => !enemy.Body.IsDefeated);
            if (target == null || target.Body.IsDefeated)
            {
                return Array.Empty<EnemyState>();
            }

            return new[] { target };
        }

        private void ResolveEnemyIntent(BattleState state, EnemyState enemy)
        {
            var intent = enemy.CurrentIntent;
            switch (intent.Type)
            {
                case EnemyIntentType.Attack:
                case EnemyIntentType.Sweep:
                    DealEnemyDamage(state, enemy, intent.Value);
                    break;
                case EnemyIntentType.Defend:
                    enemy.Body.AddShield(intent.Value);
                    state.Logs.Add(new BattleLogEntry($"{enemy.Body.Name} 获得 {intent.Value} 点护盾。"));
                    break;
                case EnemyIntentType.AttackAndBurn:
                    DealEnemyDamage(state, enemy, intent.Value);
                    state.Player.AddBurn(intent.SecondaryValue, 2);
                    state.Logs.Add(new BattleLogEntry($"{enemy.Body.Name} 施加灼烧 {intent.SecondaryValue} 层。"));
                    break;
                case EnemyIntentType.Buff:
                    state.Logs.Add(new BattleLogEntry($"{enemy.Body.Name} 正在蓄力。"));
                    break;
                case EnemyIntentType.Summon:
                    state.Logs.Add(new BattleLogEntry($"{enemy.Body.Name} 准备召唤。"));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(intent.Type), intent.Type, "Unsupported enemy intent type.");
            }
        }

        private static void DealEnemyDamage(BattleState state, EnemyState enemy, int damage)
        {
            var dealt = state.Player.TakeDamage(damage);
            state.Logs.Add(new BattleLogEntry($"{enemy.Body.Name} 对玩家造成 {dealt} 点伤害。"));
        }

        private void DrawToHandLimit(BattleState state)
        {
            DrawCards(state, Math.Max(0, state.HandLimit - state.Hand.Count));
        }

        private void DrawCards(BattleState state, int count)
        {
            for (var i = 0; i < count; i++)
            {
                if (state.DrawPile.Count == 0)
                {
                    if (state.DiscardPile.Count == 0)
                    {
                        return;
                    }

                    state.DrawPile.AddRange(state.DiscardPile);
                    state.DiscardPile.Clear();
                    Shuffle(state.DrawPile);
                }

                var card = state.DrawPile[0];
                state.DrawPile.RemoveAt(0);
                state.Hand.Add(card);
            }
        }

        private void Shuffle<T>(IList<T> list)
        {
            for (var i = list.Count - 1; i > 0; i--)
            {
                var j = Random.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        private static void RefreshOutcome(BattleState state)
        {
            if (state.Player.IsDefeated)
            {
                state.Outcome = BattleOutcome.Defeat;
                return;
            }

            if (state.Enemies.All(enemy => enemy.Body.IsDefeated))
            {
                state.Outcome = BattleOutcome.Victory;
                return;
            }

            state.Outcome = BattleOutcome.InProgress;
        }

        private static void EnsureBattleActive(BattleState state)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (state.Outcome != BattleOutcome.InProgress)
            {
                throw new InvalidOperationException("Battle has already finished.");
            }
        }
    }

    public static class CultivationSeedData
    {
        public static CardDefinition SwordQi { get; } = new CardDefinition(
            "sword_qi",
            "剑气诀",
            1,
            new[]
            {
                new CardUpgradeOption(
                    "sword_qi_damage_1",
                    "追魂剑气",
                    "伤害提升到 11。",
                    new CardDefinition(
                        "sword_qi_damage_1",
                        "追魂剑气",
                        1,
                        new[]
                        {
                            new CardUpgradeOption(
                                "sword_qi_damage_2_break",
                                "破魂剑气",
                                "造成 11 伤害，并施加 1 层破防。",
                                new CardDefinition("sword_qi_damage_2_break", "破魂剑气", 1, new CardEffect(CardEffectType.Damage, 11), new CardEffect(CardEffectType.BreakDefense, 1))),
                            new CardUpgradeOption(
                                "sword_qi_damage_2_multi",
                                "连珠剑气",
                                "造成 6 伤害 2 次。",
                                new CardDefinition("sword_qi_damage_2_multi", "连珠剑气", 1, new CardEffect(CardEffectType.Damage, 6, repeatCount: 2))),
                        },
                        new CardEffect(CardEffectType.Damage, 11))),
                new CardUpgradeOption(
                    "sword_qi_cost_1",
                    "灵动剑气",
                    "灵力消耗降为 0。",
                    new CardDefinition(
                        "sword_qi_cost_1",
                        "灵动剑气",
                        0,
                        new[]
                        {
                            new CardUpgradeOption(
                                "sword_qi_cost_2_draw",
                                "无影剑气",
                                "保持 0 灵力，额外抽 1 张牌。",
                                new CardDefinition("sword_qi_cost_2_draw", "无影剑气", 0, new CardEffect(CardEffectType.Damage, 8), new CardEffect(CardEffectType.Draw, 1))),
                            new CardUpgradeOption(
                                "sword_qi_cost_2_sharpness",
                                "寒光剑气",
                                "保持 0 灵力，额外获得锋锐 3，持续 2 回合。",
                                new CardDefinition("sword_qi_cost_2_sharpness", "寒光剑气", 0, new CardEffect(CardEffectType.Damage, 8), new CardEffect(CardEffectType.Sharpness, 3, CardTarget.Self, 2))),
                        },
                        new CardEffect(CardEffectType.Damage, 8))),
            },
            new CardEffect(CardEffectType.Damage, 8));

        public static CardDefinition BreakArmor { get; } = new CardDefinition(
            "break_armor",
            "破甲符",
            1,
            new[]
            {
                new CardUpgradeOption(
                    "break_armor_damage_1",
                    "裂甲符",
                    "伤害提升到 5，破防保持 1 层。",
                    new CardDefinition(
                        "break_armor_damage_1",
                        "裂甲符",
                        1,
                        new[]
                        {
                            new CardUpgradeOption(
                                "break_armor_damage_2_duration",
                                "裂甲长符",
                                "造成 5 伤害，施加 1 层破防 3 回合。",
                                new CardDefinition("break_armor_damage_2_duration", "裂甲长符", 1, new CardEffect(CardEffectType.Damage, 5), new CardEffect(CardEffectType.BreakDefense, 1, duration: 3))),
                            new CardUpgradeOption(
                                "break_armor_damage_2_burst",
                                "穿甲符",
                                "伤害提升到 8，破防保持 1 层。",
                                new CardDefinition("break_armor_damage_2_burst", "穿甲符", 1, new CardEffect(CardEffectType.Damage, 8), new CardEffect(CardEffectType.BreakDefense, 1))),
                        },
                        new CardEffect(CardEffectType.Damage, 5),
                        new CardEffect(CardEffectType.BreakDefense, 1))),
                new CardUpgradeOption(
                    "break_armor_stack_1",
                    "碎甲符",
                    "破防提升到 2 层。",
                    new CardDefinition(
                        "break_armor_stack_1",
                        "碎甲符",
                        1,
                        new[]
                        {
                            new CardUpgradeOption(
                                "break_armor_stack_2_duration",
                                "碎甲长符",
                                "施加 2 层破防 3 回合。",
                                new CardDefinition("break_armor_stack_2_duration", "碎甲长符", 1, new CardEffect(CardEffectType.Damage, 3), new CardEffect(CardEffectType.BreakDefense, 2, duration: 3))),
                            new CardUpgradeOption(
                                "break_armor_stack_2_draw",
                                "连锁符",
                                "施加 2 层破防，并抽 1 张牌。",
                                new CardDefinition("break_armor_stack_2_draw", "连锁符", 1, new CardEffect(CardEffectType.Damage, 3), new CardEffect(CardEffectType.BreakDefense, 2), new CardEffect(CardEffectType.Draw, 1))),
                        },
                        new CardEffect(CardEffectType.Damage, 3),
                        new CardEffect(CardEffectType.BreakDefense, 2))),
            },
            new CardEffect(CardEffectType.Damage, 3),
            new CardEffect(CardEffectType.BreakDefense, 1));

        public static CardDefinition GuardQi { get; } = new CardDefinition(
            "guard_qi",
            "护体真气",
            1,
            new[]
            {
                new CardUpgradeOption(
                    "guard_qi_shield_1",
                    "护体罡气",
                    "护盾提升到 11。",
                    new CardDefinition(
                        "guard_qi_shield_1",
                        "护体罡气",
                        1,
                        new[]
                        {
                            new CardUpgradeOption(
                                "guard_qi_shield_2_guard",
                                "护体玄气",
                                "护盾提升到 16。",
                                new CardDefinition("guard_qi_shield_2_guard", "护体玄气", 1, new CardEffect(CardEffectType.Shield, 16, CardTarget.Self))),
                            new CardUpgradeOption(
                                "guard_qi_shield_2_draw",
                                "护体灵气",
                                "获得 11 护盾并抽 1 张牌。",
                                new CardDefinition("guard_qi_shield_2_draw", "护体灵气", 1, new CardEffect(CardEffectType.Shield, 11, CardTarget.Self), new CardEffect(CardEffectType.Draw, 1, CardTarget.Self))),
                        },
                        new CardEffect(CardEffectType.Shield, 11, CardTarget.Self))),
                new CardUpgradeOption(
                    "guard_qi_draw_1",
                    "流转真气",
                    "获得 7 护盾并抽 1 张牌。",
                    new CardDefinition(
                        "guard_qi_draw_1",
                        "流转真气",
                        1,
                        new[]
                        {
                            new CardUpgradeOption(
                                "guard_qi_draw_2_guard",
                                "流转罡气",
                                "获得 10 护盾并抽 1 张牌。",
                                new CardDefinition("guard_qi_draw_2_guard", "流转罡气", 1, new CardEffect(CardEffectType.Shield, 10, CardTarget.Self), new CardEffect(CardEffectType.Draw, 1, CardTarget.Self))),
                            new CardUpgradeOption(
                                "guard_qi_draw_2_cycle",
                                "周天真气",
                                "获得 7 护盾并抽 2 张牌。",
                                new CardDefinition("guard_qi_draw_2_cycle", "周天真气", 1, new CardEffect(CardEffectType.Shield, 7, CardTarget.Self), new CardEffect(CardEffectType.Draw, 2, CardTarget.Self))),
                        },
                        new CardEffect(CardEffectType.Shield, 7, CardTarget.Self),
                        new CardEffect(CardEffectType.Draw, 1, CardTarget.Self))),
            },
            new CardEffect(CardEffectType.Shield, 8, CardTarget.Self));

        public static CardDefinition SwordStep { get; } = new CardDefinition(
            "sword_step",
            "剑步",
            1,
            new[]
            {
                new CardUpgradeOption(
                    "sword_step_guard_1",
                    "御剑步",
                    "护盾提升到 7，仍抽 1 张牌。",
                    new CardDefinition(
                        "sword_step_guard_1",
                        "御剑步",
                        1,
                        new[]
                        {
                            new CardUpgradeOption(
                                "sword_step_guard_2_draw",
                                "剑光步",
                                "获得 7 护盾并抽 2 张牌。",
                                new CardDefinition("sword_step_guard_2_draw", "剑光步", 1, new CardEffect(CardEffectType.Shield, 7, CardTarget.Self), new CardEffect(CardEffectType.Draw, 2, CardTarget.Self))),
                            new CardUpgradeOption(
                                "sword_step_guard_2_guard",
                                "剑风步",
                                "获得 11 护盾并抽 1 张牌。",
                                new CardDefinition("sword_step_guard_2_guard", "剑风步", 1, new CardEffect(CardEffectType.Shield, 11, CardTarget.Self), new CardEffect(CardEffectType.Draw, 1, CardTarget.Self))),
                        },
                        new CardEffect(CardEffectType.Shield, 7, CardTarget.Self),
                        new CardEffect(CardEffectType.Draw, 1, CardTarget.Self))),
                new CardUpgradeOption(
                    "sword_step_draw_1",
                    "游龙剑步",
                    "护盾保持 4，抽牌提升到 2。",
                    new CardDefinition(
                        "sword_step_draw_1",
                        "游龙剑步",
                        1,
                        new[]
                        {
                            new CardUpgradeOption(
                                "sword_step_draw_2_heal",
                                "剑息步",
                                "获得 4 护盾，抽 2 张牌，并恢复 3 HP。",
                                new CardDefinition("sword_step_draw_2_heal", "剑息步", 1, new CardEffect(CardEffectType.Shield, 4, CardTarget.Self), new CardEffect(CardEffectType.Draw, 2, CardTarget.Self), new CardEffect(CardEffectType.Heal, 3, CardTarget.Self))),
                            new CardUpgradeOption(
                                "sword_step_draw_2_guard",
                                "剑意步",
                                "获得 4 护盾、抽 2 张牌，并施加 1 层剑气印记。",
                                new CardDefinition("sword_step_draw_2_guard", "剑意步", 1, new CardEffect(CardEffectType.Shield, 4, CardTarget.Self), new CardEffect(CardEffectType.Draw, 2, CardTarget.Self), new CardEffect(CardEffectType.SwordMark, 1))),
                        },
                        new CardEffect(CardEffectType.Shield, 4, CardTarget.Self),
                        new CardEffect(CardEffectType.Draw, 2, CardTarget.Self))),
            },
            new CardEffect(CardEffectType.Shield, 4, CardTarget.Self),
            new CardEffect(CardEffectType.Draw, 1, CardTarget.Self));

        public static CardDefinition LightBody { get; } = new CardDefinition(
            "light_body",
            "轻身术",
            0,
            new[]
            {
                new CardUpgradeOption(
                    "light_body_shield_1",
                    "轻身护法",
                    "抽 2 张牌并获得 3 护盾。",
                    new CardDefinition(
                        "light_body_shield_1",
                        "轻身护法",
                        0,
                        new[]
                        {
                            new CardUpgradeOption(
                                "light_body_shield_2_draw",
                                "疾风护法",
                                "抽 3 张牌并获得 3 护盾。",
                                new CardDefinition("light_body_shield_2_draw", "疾风护法", 0, new CardEffect(CardEffectType.Draw, 3, CardTarget.Self), new CardEffect(CardEffectType.Shield, 3, CardTarget.Self))),
                            new CardUpgradeOption(
                                "light_body_shield_2_guard",
                                "燕回闪",
                                "抽 2 张牌并获得 6 护盾。",
                                new CardDefinition("light_body_shield_2_guard", "燕回闪", 0, new CardEffect(CardEffectType.Draw, 2, CardTarget.Self), new CardEffect(CardEffectType.Shield, 6, CardTarget.Self))),
                        },
                        new CardEffect(CardEffectType.Draw, 2, CardTarget.Self),
                        new CardEffect(CardEffectType.Shield, 3, CardTarget.Self))),
                new CardUpgradeOption(
                    "light_body_draw_1",
                    "身随剑走",
                    "抽牌提升到 3。",
                    new CardDefinition(
                        "light_body_draw_1",
                        "身随剑走",
                        0,
                        new[]
                        {
                            new CardUpgradeOption(
                                "light_body_draw_2_more",
                                "追风步",
                                "抽牌提升到 4。",
                                new CardDefinition("light_body_draw_2_more", "追风步", 0, new CardEffect(CardEffectType.Draw, 4, CardTarget.Self))),
                            new CardUpgradeOption(
                                "light_body_draw_2_guard",
                                "蝶舞闪",
                                "抽 3 张牌并获得 2 护盾。",
                                new CardDefinition("light_body_draw_2_guard", "蝶舞闪", 0, new CardEffect(CardEffectType.Draw, 3, CardTarget.Self), new CardEffect(CardEffectType.Shield, 2, CardTarget.Self))),
                        },
                        new CardEffect(CardEffectType.Draw, 3, CardTarget.Self))),
            },
            new CardEffect(CardEffectType.Draw, 2, CardTarget.Self));

        public static CardDefinition HealingPill { get; } = new CardDefinition(
            "healing_pill",
            "回春丹",
            1,
            new[]
            {
                new CardUpgradeOption(
                    "healing_pill_heal_1",
                    "大回春丹",
                    "恢复提升到 10 HP，使用后消耗。",
                    new CardDefinition(
                        "healing_pill_heal_1",
                        "大回春丹",
                        1,
                        new[]
                        {
                            new CardUpgradeOption(
                                "healing_pill_heal_2_large",
                                "大还春丹",
                                "恢复提升到 14 HP。",
                                new CardDefinition("healing_pill_heal_2_large", "大还春丹", 1, new CardEffect(CardEffectType.Heal, 14, CardTarget.Self))),
                            new CardUpgradeOption(
                                "healing_pill_heal_2_guard",
                                "回春护脉丹",
                                "恢复 10 HP，并获得 5 护盾。",
                                new CardDefinition("healing_pill_heal_2_guard", "回春护脉丹", 1, new CardEffect(CardEffectType.Heal, 10, CardTarget.Self), new CardEffect(CardEffectType.Shield, 5, CardTarget.Self))),
                        },
                        new CardEffect(CardEffectType.Heal, 10, CardTarget.Self),
                        new CardEffect(CardEffectType.Exhaust, 1, CardTarget.Self))),
                new CardUpgradeOption(
                    "healing_pill_cycle_1",
                    "回春行气丹",
                    "恢复 6 HP 并抽 1 张牌。",
                    new CardDefinition(
                        "healing_pill_cycle_1",
                        "回春行气丹",
                        1,
                        new[]
                        {
                            new CardUpgradeOption(
                                "healing_pill_cycle_2_heal",
                                "回春养气丹",
                                "恢复 10 HP 并抽 1 张牌。",
                                new CardDefinition("healing_pill_cycle_2_heal", "回春养气丹", 1, new CardEffect(CardEffectType.Heal, 10, CardTarget.Self), new CardEffect(CardEffectType.Draw, 1, CardTarget.Self))),
                            new CardUpgradeOption(
                                "healing_pill_cycle_2_draw",
                                "回春灵丹",
                                "恢复 6 HP 并抽 2 张牌。",
                                new CardDefinition("healing_pill_cycle_2_draw", "回春灵丹", 1, new CardEffect(CardEffectType.Heal, 6, CardTarget.Self), new CardEffect(CardEffectType.Draw, 2, CardTarget.Self))),
                        },
                        new CardEffect(CardEffectType.Heal, 6, CardTarget.Self),
                        new CardEffect(CardEffectType.Draw, 1, CardTarget.Self))),
            },
            new CardEffect(CardEffectType.Heal, 6, CardTarget.Self));

        public static CardDefinition Thrust { get; } = new CardDefinition(
            "thrust",
            "刺击",
            1,
            new[]
            {
                new CardUpgradeOption(
                    "thrust_damage_1",
                    "深刺",
                    "造成 8 伤害并施加 1 层剑气印记。",
                    new CardDefinition("thrust_damage_1", "深刺", 1, new CardEffect(CardEffectType.Damage, 8), new CardEffect(CardEffectType.SwordMark, 1))),
                new CardUpgradeOption(
                    "thrust_mark_1",
                    "透骨刺",
                    "造成 5 伤害并施加 2 层剑气印记。",
                    new CardDefinition("thrust_mark_1", "透骨刺", 1, new CardEffect(CardEffectType.Damage, 5), new CardEffect(CardEffectType.SwordMark, 2))),
            },
            new CardEffect(CardEffectType.Damage, 5),
            new CardEffect(CardEffectType.SwordMark, 1));

        public static CardDefinition SevenfoldSwordQi { get; } = new CardDefinition(
            "sevenfold_sword_qi",
            "七绝剑气",
            2,
            new[]
            {
                new CardUpgradeOption(
                    "sevenfold_sword_qi_mark_bonus_1",
                    "九绝剑气",
                    "造成 7 伤害，目标每有 1 层剑气印记，额外造成 5 伤害。",
                    new CardDefinition("sevenfold_sword_qi_mark_bonus_1", "九绝剑气", 2, new CardEffect(CardEffectType.Damage, 7), new CardEffect(CardEffectType.DamagePerSwordMark, 5))),
                new CardUpgradeOption(
                    "sevenfold_sword_qi_base_1",
                    "连环剑气",
                    "造成 12 伤害，目标每有 1 层剑气印记，额外造成 3 伤害。",
                    new CardDefinition("sevenfold_sword_qi_base_1", "连环剑气", 2, new CardEffect(CardEffectType.Damage, 12), new CardEffect(CardEffectType.DamagePerSwordMark, 3))),
            },
            new CardEffect(CardEffectType.Damage, 7),
            new CardEffect(CardEffectType.DamagePerSwordMark, 3));

        public static CardDefinition FlyingSword { get; } = new CardDefinition(
            "flying_sword",
            "御剑式",
            2,
            new[]
            {
                new CardUpgradeOption(
                    "flying_sword_damage_1",
                    "御剑穿云",
                    "伤害提升到 20。",
                    new CardDefinition("flying_sword_damage_1", "御剑穿云", 2, new CardEffect(CardEffectType.Damage, 20))),
                new CardUpgradeOption(
                    "flying_sword_cost_1",
                    "御剑轻灵",
                    "灵力消耗降为 1。",
                    new CardDefinition("flying_sword_cost_1", "御剑轻灵", 1, new CardEffect(CardEffectType.Damage, 16))),
            },
            new CardEffect(CardEffectType.Damage, 16));

        public static CardDefinition CloudGuard { get; } = new CardDefinition(
            "cloud_guard",
            "流云护身",
            1,
            new[]
            {
                new CardUpgradeOption(
                    "cloud_guard_shield_1",
                    "流云固守",
                    "护盾提升到 9，仍抽 1 张牌。",
                    new CardDefinition("cloud_guard_shield_1", "流云固守", 1, new CardEffect(CardEffectType.Shield, 9, CardTarget.Self), new CardEffect(CardEffectType.Draw, 1, CardTarget.Self))),
                new CardUpgradeOption(
                    "cloud_guard_draw_1",
                    "流云回环",
                    "护盾保持 6，抽牌提升到 2。",
                    new CardDefinition("cloud_guard_draw_1", "流云回环", 1, new CardEffect(CardEffectType.Shield, 6, CardTarget.Self), new CardEffect(CardEffectType.Draw, 2, CardTarget.Self))),
            },
            new CardEffect(CardEffectType.Shield, 6, CardTarget.Self),
            new CardEffect(CardEffectType.Draw, 1, CardTarget.Self));

        public static CardDefinition SmallRestorePill { get; } = new CardDefinition(
            "small_restore_pill",
            "小还丹",
            1,
            new[]
            {
                new CardUpgradeOption(
                    "small_restore_pill_heal_1",
                    "还真丹",
                    "恢复提升到 14 HP。",
                    new CardDefinition("small_restore_pill_heal_1", "还真丹", 1, new CardEffect(CardEffectType.Heal, 14, CardTarget.Self))),
                new CardUpgradeOption(
                    "small_restore_pill_draw_1",
                    "还灵丹",
                    "恢复 10 HP 并抽 1 张牌。",
                    new CardDefinition("small_restore_pill_draw_1", "还灵丹", 1, new CardEffect(CardEffectType.Heal, 10, CardTarget.Self), new CardEffect(CardEffectType.Draw, 1, CardTarget.Self))),
            },
            new CardEffect(CardEffectType.Heal, 10, CardTarget.Self));

        public static PillDefinition SmallRestorePillItem { get; } = new PillDefinition(
            "small_restore_pill",
            "小还丹",
            "战斗内消耗品：恢复 10 HP。",
            effectValue: 10);

        public static PillDefinition SpiritBoostPillItem { get; } = new PillDefinition(
            "spirit_boost_pill",
            "增元丹",
            "战斗内消耗品：本回合灵力 +2。",
            PillEffectType.Spirit,
            2);

        public static PillDefinition CleansePillItem { get; } = new PillDefinition(
            "cleanse_pill",
            "解毒丹",
            "战斗内消耗品：清除所有负面状态，恢复 3 HP。",
            PillEffectType.Cleanse,
            3);

        public static PillDefinition BreakthroughPillItem { get; } = new PillDefinition(
            "breakthrough_pill",
            "破境丹",
            "战斗内消耗品：本场战斗所有功法灵力消耗 -1，最低 0。",
            PillEffectType.CostReduction,
            1);

        public static IReadOnlyList<CardDefinition> CreateSwordSectStarterDeck()
        {
            return new List<CardDefinition>
            {
                SwordQi, SwordQi, SwordQi, SwordQi,
                BreakArmor, BreakArmor,
                GuardQi, GuardQi, GuardQi,
                SwordStep,
                LightBody,
                HealingPill,
            };
        }

        public static EnemyDefinition StoneDemon { get; } = new EnemyDefinition(
            "stone_demon",
            "石魔",
            40,
            2,
            new EnemyIntent(EnemyIntentType.Attack, 6, description: "攻击 6"),
            new EnemyIntent(EnemyIntentType.Defend, 5, description: "防御 5"),
            new EnemyIntent(EnemyIntentType.Attack, 8, description: "攻击 8"));

        public static EnemyDefinition FireBat { get; } = new EnemyDefinition(
            "fire_bat",
            "火蝠",
            32,
            0,
            new EnemyIntent(EnemyIntentType.AttackAndBurn, 4, 1, "攻击 4 + 灼烧 1"),
            new EnemyIntent(EnemyIntentType.AttackAndBurn, 4, 1, "攻击 4 + 灼烧 1"),
            new EnemyIntent(EnemyIntentType.Buff, description: "强化"),
            new EnemyIntent(EnemyIntentType.Attack, 8, description: "攻击 8"));

        public static EnemyDefinition StoneDemonLeader { get; } = new EnemyDefinition(
            "stone_demon_leader",
            "石魔首领",
            65,
            3,
            new EnemyIntent(EnemyIntentType.Attack, 8, description: "攻击 8"),
            new EnemyIntent(EnemyIntentType.Summon, 20, description: "召唤石魔 20"),
            new EnemyIntent(EnemyIntentType.Sweep, 5, description: "横扫 5"),
            new EnemyIntent(EnemyIntentType.Defend, 10, description: "防御 10"));

        public static IReadOnlyList<CultivationRunReward> CreateSwordSectRewardPool()
        {
            return new List<CultivationRunReward>
            {
                new CultivationRunReward("reward_flying_sword", FlyingSword),
                new CultivationRunReward("reward_cloud_guard", CloudGuard),
                new CultivationRunReward("reward_small_restore_pill", SmallRestorePill),
                new CultivationRunReward("reward_sword_step", SwordStep),
                new CultivationRunReward("reward_break_armor", BreakArmor),
                new CultivationRunReward("reward_thrust", Thrust),
                new CultivationRunReward("reward_sevenfold_sword_qi", SevenfoldSwordQi),
            };
        }

        public static IReadOnlyList<CultivationMarketItem> CreatePrototypeMarketItems()
        {
            return new List<CultivationMarketItem>
            {
                new CultivationMarketItem("market_cloud_guard", CloudGuard, 20),
                new CultivationMarketItem("market_thrust", Thrust, 25),
                new CultivationMarketItem("market_small_restore_pill", SmallRestorePillItem, 15),
                new CultivationMarketItem("market_spirit_boost_pill", SpiritBoostPillItem, 35),
                new CultivationMarketItem("market_cleanse_pill", CleansePillItem, 15),
                new CultivationMarketItem("market_breakthrough_pill", BreakthroughPillItem, 90),
            };
        }

        public static IReadOnlyList<CultivationRunNode> CreateFirstPrototypeRoute()
        {
            var rewards = CreateSwordSectRewardPool();
            return new List<CultivationRunNode>
            {
                new CultivationRunNode("node_stone_demon", "山门石魔", CultivationRunNodeType.Battle, StoneDemon, rewards, spiritStoneReward: 15),
                new CultivationRunNode("node_fire_bat", "火蝠洞", CultivationRunNodeType.Battle, FireBat, rewards, spiritStoneReward: 15),
                new CultivationRunNode("node_meditation", "闭关调息", CultivationRunNodeType.Rest, null, null, restHealAmount: 30),
                new CultivationRunNode("node_stone_demon_leader", "石魔首领", CultivationRunNodeType.Elite, StoneDemonLeader, rewards, spiritStoneReward: 35),
            };
        }

        public static IReadOnlyList<CultivationRunNode> CreateFirstPrototypeBranchingRoute()
        {
            var rewards = CreateSwordSectRewardPool();
            var marketItems = CreatePrototypeMarketItems();
            return new List<CultivationRunNode>
            {
                new CultivationRunNode(
                    "node_stone_demon",
                    "山门石魔",
                    CultivationRunNodeType.Battle,
                    StoneDemon,
                    rewards,
                    nextNodeIndices: new[] { 1, 2 },
                    spiritStoneReward: 15),
                new CultivationRunNode(
                    "node_fire_bat",
                    "火蝠洞",
                    CultivationRunNodeType.Battle,
                    FireBat,
                    rewards,
                    nextNodeIndices: new[] { 4 },
                    spiritStoneReward: 15),
                new CultivationRunNode(
                    "node_meditation",
                    "闭关调息",
                    CultivationRunNodeType.Rest,
                    null,
                    null,
                    restHealAmount: 30,
                    nextNodeIndices: new[] { 3, 4 }),
                new CultivationRunNode(
                    "node_market",
                    "山脚坊市",
                    CultivationRunNodeType.Market,
                    null,
                    null,
                    nextNodeIndices: new[] { 4 },
                    marketItems: marketItems),
                new CultivationRunNode(
                    "node_stone_demon_leader",
                    "石魔首领",
                    CultivationRunNodeType.Elite,
                    StoneDemonLeader,
                    rewards,
                    spiritStoneReward: 35),
            };
        }
    }
}
