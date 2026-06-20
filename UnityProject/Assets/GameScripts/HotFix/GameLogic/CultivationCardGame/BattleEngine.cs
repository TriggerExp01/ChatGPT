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
            if (deck == null)
            {
                throw new ArgumentNullException(nameof(deck));
            }

            if (enemy == null)
            {
                throw new ArgumentNullException(nameof(enemy));
            }

            var player = new CombatantState("修士", 100);
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
                   && state.Spirit >= card.SpiritCost;
        }

        public void PlayCard(BattleState state, CardDefinition card, EnemyState target = null)
        {
            if (!CanPlay(state, card))
            {
                throw new InvalidOperationException("Card cannot be played in the current battle state.");
            }

            state.Spirit -= card.SpiritCost;
            state.Hand.Remove(card);
            state.DiscardPile.Add(card);

            foreach (var effect in card.Effects)
            {
                ResolveCardEffect(state, card, effect, target);
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
                        var dealt = enemy.Body.TakeDamage(effect.Value);
                        state.Logs.Add(new BattleLogEntry($"{card.Name} 对 {enemy.Body.Name} 造成 {dealt} 点伤害。"));
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
                default:
                    throw new ArgumentOutOfRangeException(nameof(effect.Type), effect.Type, "Unsupported card effect type.");
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
            new CardEffect(CardEffectType.Damage, 8));

        public static CardDefinition BreakArmor { get; } = new CardDefinition(
            "break_armor",
            "破甲符",
            1,
            new CardEffect(CardEffectType.Damage, 3),
            new CardEffect(CardEffectType.BreakDefense, 1));

        public static CardDefinition GuardQi { get; } = new CardDefinition(
            "guard_qi",
            "护体真气",
            1,
            new CardEffect(CardEffectType.Shield, 8, CardTarget.Self));

        public static CardDefinition SwordStep { get; } = new CardDefinition(
            "sword_step",
            "剑步",
            1,
            new CardEffect(CardEffectType.Shield, 4, CardTarget.Self),
            new CardEffect(CardEffectType.Draw, 1, CardTarget.Self));

        public static CardDefinition LightBody { get; } = new CardDefinition(
            "light_body",
            "轻身术",
            0,
            new CardEffect(CardEffectType.Draw, 2, CardTarget.Self));

        public static CardDefinition HealingPill { get; } = new CardDefinition(
            "healing_pill",
            "回春丹",
            1,
            new CardEffect(CardEffectType.Heal, 6, CardTarget.Self));

        public static CardDefinition FlyingSword { get; } = new CardDefinition(
            "flying_sword",
            "御剑式",
            2,
            new CardEffect(CardEffectType.Damage, 16));

        public static CardDefinition CloudGuard { get; } = new CardDefinition(
            "cloud_guard",
            "流云护身",
            1,
            new CardEffect(CardEffectType.Shield, 6, CardTarget.Self),
            new CardEffect(CardEffectType.Draw, 1, CardTarget.Self));

        public static CardDefinition SmallRestorePill { get; } = new CardDefinition(
            "small_restore_pill",
            "小还丹",
            1,
            new CardEffect(CardEffectType.Heal, 10, CardTarget.Self));

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
            };
        }

        public static IReadOnlyList<CultivationRunNode> CreateFirstPrototypeRoute()
        {
            var rewards = CreateSwordSectRewardPool();
            return new List<CultivationRunNode>
            {
                new CultivationRunNode("node_stone_demon", "山门石魔", CultivationRunNodeType.Battle, StoneDemon, rewards),
                new CultivationRunNode("node_fire_bat", "火蝠洞", CultivationRunNodeType.Battle, FireBat, rewards),
                new CultivationRunNode("node_stone_demon_leader", "石魔首领", CultivationRunNodeType.Elite, StoneDemonLeader, rewards),
            };
        }
    }
}
