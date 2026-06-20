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
            return CreateBattle(deck, enemy, playerCurrentHp, playerMaxHp, CultivationRunState.DefaultSpiritMax, CultivationRunState.DefaultHandLimit);
        }

        public BattleState CreateBattle(IEnumerable<CardDefinition> deck, EnemyDefinition enemy, int playerCurrentHp, int playerMaxHp, int spiritMax, int handLimit)
        {
            return CreateBattle(deck, enemy, playerCurrentHp, playerMaxHp, spiritMax, handLimit, null);
        }

        public BattleState CreateBattle(IEnumerable<CardDefinition> deck, EnemyDefinition enemy, int playerCurrentHp, int playerMaxHp, int spiritMax, int handLimit, GoldenCorePassiveDefinition passive)
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

            if (spiritMax <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(spiritMax), "Spirit max must be positive.");
            }

            if (handLimit <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(handLimit), "Hand limit must be positive.");
            }

            var player = new CombatantState("修士", playerMaxHp, currentHp: playerCurrentHp);
            var state = new BattleState(player, deck, new[] { new EnemyState(enemy) }, spiritMax, handLimit);
            ApplyGoldenCorePassive(state, passive);
            Shuffle(state.DrawPile);
            StartPlayerTurn(state);
            return state;
        }

        public void StartPlayerTurn(BattleState state)
        {
            EnsureBattleActive(state);

            state.TurnNumber++;
            state.ResetTurnCriticalState();
            state.ClearAttackCounter();
            state.Player.ClearShield();
            state.Player.ResolveSharpnessAtTurnStart();
            state.Spirit = state.SpiritMax;

            var freezePenalty = state.Player.ResolveFreezeAtTurnStart();
            if (freezePenalty > 0)
            {
                state.Spirit = Math.Max(0, state.Spirit - freezePenalty);
                state.Logs.Add(new BattleLogEntry($"冰冻使本回合灵力 -{freezePenalty}。"));
            }

            var burnDamage = state.Player.ResolveBurnAtTurnStart();
            if (burnDamage > 0)
            {
                state.Logs.Add(new BattleLogEntry($"玩家受到灼烧 {burnDamage} 点伤害。"));
            }

            var stunned = state.Player.ResolveStunAtTurnStart();
            if (stunned)
            {
                state.Spirit = 0;
                state.Hand.Clear();
                state.Logs.Add(new BattleLogEntry("眩晕使本回合无法行动。"));
            }

            foreach (var enemy in state.Enemies)
            {
                var enemyBurnDamage = enemy.Body.ResolveBurnAtTurnStart();
                if (enemyBurnDamage > 0)
                {
                    state.Logs.Add(new BattleLogEntry($"{enemy.Body.Name} 受到灼烧 {enemyBurnDamage} 点伤害。"));
                }
            }

            if (!stunned)
            {
                DrawToHandLimit(state);
            }

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
                if (enemy.Body.ResolveStunAtTurnStart())
                {
                    state.Logs.Add(new BattleLogEntry($"{enemy.Body.Name} 因眩晕跳过行动。"));
                    enemy.AdvanceIntent();
                    continue;
                }

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
                case CardEffectType.Dodge:
                    state.AddDodgeCharges(effect.Value);
                    state.Logs.Add(new BattleLogEntry($"{card.Name} 获得 {effect.Value} 次闪避。"));
                    break;
                case CardEffectType.DodgeCounter:
                    state.AddDodgeCounterDamage(effect.Value);
                    state.Logs.Add(new BattleLogEntry($"{card.Name} 闪避成功时反击 {effect.Value} 伤害。"));
                    break;
                case CardEffectType.AttackCounter:
                    state.AddAttackCounter(effect.Value, effect.ChancePercent);
                    state.Logs.Add(new BattleLogEntry($"{card.Name} counter-on-hit {effect.ChancePercent}% for {effect.Value}."));
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
                case CardEffectType.Stun:
                    foreach (var enemy in SelectTargets(state, effect, explicitTarget))
                    {
                        enemy.Body.AddStun(Math.Max(1, effect.Duration));
                        state.Logs.Add(new BattleLogEntry($"{card.Name} 使 {enemy.Body.Name} 眩晕 {Math.Max(1, effect.Duration)} 回合。"));
                    }

                    break;
                case CardEffectType.ChanceDamage:
                    foreach (var enemy in SelectTargets(state, effect, explicitTarget))
                    {
                        ResolveChanceDamageHits(state, card, effect, enemy);
                    }

                    break;
                case CardEffectType.ChanceDamageWithStun:
                    foreach (var enemy in SelectTargets(state, effect, explicitTarget))
                    {
                        ResolveChanceDamageHits(state, card, effect, enemy, stunOnHit: true);
                    }

                    break;
                case CardEffectType.ChanceDamageWithChain:
                    foreach (var enemy in SelectTargets(state, effect, explicitTarget))
                    {
                        ResolveChanceDamageHits(state, card, effect, enemy, chainOnEachHit: true);
                    }

                    break;
                case CardEffectType.ChainOnChanceDamage:
                    foreach (var enemy in SelectTargets(state, effect, explicitTarget))
                    {
                        var triggered = RollChance(effect.ChancePercent);
                        var damage = triggered ? effect.Value : effect.FallbackValue;
                        if (damage <= 0)
                        {
                            state.Logs.Add(new BattleLogEntry($"{card.Name} 的暴击判定未触发。"));
                            continue;
                        }

                        var multiplier = state.TryConsumeChargedDamageMultiplier();
                        var dealt = enemy.Body.TakeDamage(damage * multiplier, state.Player.Sharpness);
                        var resultText = triggered ? "暴击触发" : "暴击未触发";
                        var chargeText = multiplier > 1 ? $" 蓄力 x{multiplier}" : string.Empty;
                        state.Logs.Add(new BattleLogEntry($"{card.Name} {resultText}{chargeText}，对 {enemy.Body.Name} 造成 {dealt} 点伤害。"));

                        if (triggered)
                        {
                            state.MarkCriticalTriggered();
                            ChainToOneAdditionalEnemy(state, card, enemy, effect.SecondaryValue);
                        }
                    }

                    break;
                case CardEffectType.DamageAfterCriticalTriggered:
                    foreach (var enemy in SelectTargets(state, effect, explicitTarget))
                    {
                        DealDamageAfterCriticalTriggered(state, card, effect, enemy, stunOnBonus: false, chainBonusToAll: false);
                    }

                    break;
                case CardEffectType.DamageAfterCriticalTriggeredWithStun:
                    foreach (var enemy in SelectTargets(state, effect, explicitTarget))
                    {
                        DealDamageAfterCriticalTriggered(state, card, effect, enemy, stunOnBonus: true, chainBonusToAll: false);
                    }

                    break;
                case CardEffectType.DamageAfterCriticalTriggeredChainAll:
                    foreach (var enemy in SelectTargets(state, effect, explicitTarget))
                    {
                        DealDamageAfterCriticalTriggered(state, card, effect, enemy, stunOnBonus: false, chainBonusToAll: true);
                    }

                    break;
                case CardEffectType.ChanceStun:
                    foreach (var enemy in SelectTargets(state, effect, explicitTarget))
                    {
                        if (RollChance(effect.ChancePercent))
                        {
                            enemy.Body.AddStun(Math.Max(1, effect.Duration));
                            state.Logs.Add(new BattleLogEntry($"{card.Name} 眩晕判定成功，使 {enemy.Body.Name} 眩晕 {Math.Max(1, effect.Duration)} 回合。"));
                        }
                        else
                        {
                            state.Logs.Add(new BattleLogEntry($"{card.Name} 眩晕判定未触发。"));
                        }
                    }

                    break;
                case CardEffectType.ChainOnChanceStun:
                    foreach (var enemy in SelectTargets(state, effect, explicitTarget))
                    {
                        var dealt = enemy.Body.TakeDamage(effect.Value, state.Player.Sharpness);
                        state.Logs.Add(new BattleLogEntry($"{card.Name} 对 {enemy.Body.Name} 造成 {dealt} 点伤害。"));

                        if (RollChance(effect.ChancePercent))
                        {
                            enemy.Body.AddStun(Math.Max(1, effect.Duration));
                            state.Logs.Add(new BattleLogEntry($"{card.Name} 眩晕判定成功，使 {enemy.Body.Name} 眩晕 {Math.Max(1, effect.Duration)} 回合。"));
                            ChainToOneAdditionalEnemy(state, card, enemy, effect.SecondaryValue);
                        }
                        else
                        {
                            state.Logs.Add(new BattleLogEntry($"{card.Name} 眩晕判定未触发，雷击连锁未触发。"));
                        }
                    }

                    break;
                case CardEffectType.ChanceChainDamage:
                    foreach (var enemy in SelectTargets(state, effect, explicitTarget))
                    {
                        ResolveChanceChainDamage(state, card, effect, enemy, false);
                    }

                    break;
                case CardEffectType.ChanceChainDamageWithStun:
                    foreach (var enemy in SelectTargets(state, effect, explicitTarget))
                    {
                        ResolveChanceChainDamage(state, card, effect, enemy, true);
                    }

                    break;
                case CardEffectType.ChanceChainDamageRepeatTarget:
                    foreach (var enemy in SelectTargets(state, effect, explicitTarget))
                    {
                        ResolveChanceChainDamage(state, card, effect, enemy, false, true);
                    }

                    break;
                case CardEffectType.ChargeDamage:
                    state.AddChargedDamage(effect.Value, Math.Max(1, effect.RepeatCount));
                    state.Logs.Add(new BattleLogEntry($"{card.Name} 蓄力完成，下次攻击伤害 x{effect.Value}。"));
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

        private bool RollChance(int chancePercent)
        {
            if (chancePercent <= 0)
            {
                return false;
            }

            if (chancePercent >= 100)
            {
                return true;
            }

            return Random.Next(100) < chancePercent;
        }

        private void ResolveChanceDamageHits(BattleState state, CardDefinition card, CardEffect effect, EnemyState enemy, bool stunOnHit = false, bool chainOnEachHit = false)
        {
            for (var hit = 0; hit < effect.RepeatCount; hit++)
            {
                if (enemy.Body.IsDefeated)
                {
                    return;
                }

                var triggered = RollChance(effect.ChancePercent);
                var damage = stunOnHit || chainOnEachHit
                    ? effect.Value
                    : triggered ? effect.Value : effect.FallbackValue;
                var hitText = effect.RepeatCount > 1 ? $" 第 {hit + 1}/{effect.RepeatCount} 击" : string.Empty;
                if (damage <= 0)
                {
                    state.Logs.Add(new BattleLogEntry($"{card.Name}{hitText} 的暴击判定未触发。"));
                    continue;
                }

                var multiplier = state.TryConsumeChargedDamageMultiplier();
                var dealt = enemy.Body.TakeDamage(damage * multiplier, state.Player.Sharpness);
                var resultText = triggered ? "暴击触发" : "暴击未触发";
                var chargeText = multiplier > 1 ? $" 蓄力 x{multiplier}" : string.Empty;
                state.Logs.Add(new BattleLogEntry($"{card.Name}{hitText} {resultText}{chargeText}，对 {enemy.Body.Name} 造成 {dealt} 点伤害。"));

                if (triggered)
                {
                    state.MarkCriticalTriggered();
                }

                if (triggered && stunOnHit && RollChance(effect.FallbackValue))
                {
                    enemy.Body.AddStun(Math.Max(1, effect.Duration));
                    state.Logs.Add(new BattleLogEntry($"{card.Name}{hitText} 暴击附加眩晕，使 {enemy.Body.Name} 眩晕 {Math.Max(1, effect.Duration)} 回合。"));
                }

                if (chainOnEachHit && RollChance(effect.SecondaryValue))
                {
                    ChainToOneAdditionalEnemy(state, card, enemy, Math.Max(0, effect.FallbackValue));
                }
            }
        }

        private static void DealDamageAfterCriticalTriggered(
            BattleState state,
            CardDefinition card,
            CardEffect effect,
            EnemyState enemy,
            bool stunOnBonus,
            bool chainBonusToAll)
        {
            if (enemy == null || enemy.Body.IsDefeated)
            {
                return;
            }

            var multiplier = state.TryConsumeChargedDamageMultiplier();
            var dealt = enemy.Body.TakeDamage(effect.Value * multiplier, state.Player.Sharpness);
            var chargeText = multiplier > 1 ? $" 蓄力 x{multiplier}" : string.Empty;
            state.Logs.Add(new BattleLogEntry($"{card.Name}{chargeText} 对 {enemy.Body.Name} 造成 {dealt} 点伤害。"));

            if (!state.HasTriggeredCriticalThisTurn || effect.FallbackValue <= 0)
            {
                state.Logs.Add(new BattleLogEntry($"{card.Name} 未检测到本回合暴击，暴击奖励未触发。"));
                return;
            }

            if (chainBonusToAll)
            {
                foreach (var chainedEnemy in state.Enemies.Where(target => !target.Body.IsDefeated).ToList())
                {
                    var chainDealt = chainedEnemy.Body.TakeDamage(effect.FallbackValue, state.Player.Sharpness);
                    state.Logs.Add(new BattleLogEntry($"{card.Name} 暴击奖励连锁至 {chainedEnemy.Body.Name}，造成 {chainDealt} 点伤害。"));
                }

                return;
            }

            var bonusDealt = enemy.Body.TakeDamage(effect.FallbackValue, state.Player.Sharpness);
            state.Logs.Add(new BattleLogEntry($"{card.Name} 触发暴击奖励，对 {enemy.Body.Name} 额外造成 {bonusDealt} 点伤害。"));

            if (stunOnBonus)
            {
                enemy.Body.AddStun(Math.Max(1, effect.Duration));
                state.Logs.Add(new BattleLogEntry($"{card.Name} 暴击奖励附加眩晕，使 {enemy.Body.Name} 眩晕 {Math.Max(1, effect.Duration)} 回合。"));
            }
        }

        private void ResolveChanceChainDamage(BattleState state, CardDefinition card, CardEffect effect, EnemyState firstTarget, bool stunOnChain, bool allowRepeatTarget = false)
        {
            if (firstTarget == null || firstTarget.Body.IsDefeated)
            {
                return;
            }

            var multiplier = state.TryConsumeChargedDamageMultiplier();
            var dealt = firstTarget.Body.TakeDamage(effect.Value * multiplier, state.Player.Sharpness);
            var chargeText = multiplier > 1 ? $" 蓄力 x{multiplier}" : string.Empty;
            state.Logs.Add(new BattleLogEntry($"{card.Name}{chargeText} 对 {firstTarget.Body.Name} 造成 {dealt} 点伤害。"));

            var chainedTargets = new HashSet<EnemyState> { firstTarget };
            var current = firstTarget;
            var chainCount = 0;
            var maxChainCount = allowRepeatTarget ? Math.Max(1, effect.RepeatCount) : int.MaxValue;
            while (chainCount < maxChainCount && RollChance(effect.ChancePercent))
            {
                var candidates = state.Enemies
                    .Where(enemy => !enemy.Body.IsDefeated && (allowRepeatTarget || !chainedTargets.Contains(enemy)))
                    .ToList();
                if (candidates.Count == 0)
                {
                    state.Logs.Add(new BattleLogEntry($"{card.Name} 雷击连锁没有可用目标。"));
                    return;
                }

                var next = candidates[Random.Next(candidates.Count)];
                var chainDamage = Math.Max(0, effect.SecondaryValue);
                if (chainDamage <= 0)
                {
                    state.Logs.Add(new BattleLogEntry($"{card.Name} 雷击连锁未造成伤害。"));
                    return;
                }

                var chainDealt = next.Body.TakeDamage(chainDamage, state.Player.Sharpness);
                state.Logs.Add(new BattleLogEntry($"{card.Name} 从 {current.Body.Name} 连锁至 {next.Body.Name}，造成 {chainDealt} 点伤害。"));
                if (stunOnChain && RollChance(effect.FallbackValue))
                {
                    next.Body.AddStun(Math.Max(1, effect.Duration));
                    state.Logs.Add(new BattleLogEntry($"{card.Name} 连锁雷击使 {next.Body.Name} 眩晕 {Math.Max(1, effect.Duration)} 回合。"));
                }

                chainedTargets.Add(next);
                current = next;
                chainCount++;
            }

            if (allowRepeatTarget && chainCount >= maxChainCount)
            {
                state.Logs.Add(new BattleLogEntry($"{card.Name} 雷击连锁达到上限 {maxChainCount} 次。"));
                return;
            }

            state.Logs.Add(new BattleLogEntry($"{card.Name} 雷击连锁未继续触发。"));
        }

        private void ChainToOneAdditionalEnemy(BattleState state, CardDefinition card, EnemyState sourceTarget, int damage)
        {
            var chainDamage = Math.Max(0, damage);
            if (chainDamage <= 0)
            {
                state.Logs.Add(new BattleLogEntry($"{card.Name} 雷击连锁未造成伤害。"));
                return;
            }

            var candidates = state.Enemies
                .Where(enemy => !enemy.Body.IsDefeated && enemy != sourceTarget)
                .ToList();
            if (candidates.Count == 0)
            {
                state.Logs.Add(new BattleLogEntry($"{card.Name} 雷击连锁没有可用目标。"));
                return;
            }

            var next = candidates[Random.Next(candidates.Count)];
            var dealt = next.Body.TakeDamage(chainDamage, state.Player.Sharpness);
            state.Logs.Add(new BattleLogEntry($"{card.Name} 从 {sourceTarget.Body.Name} 连锁至 {next.Body.Name}，造成 {dealt} 点伤害。"));
        }

        private static void DealCardDamage(BattleState state, CardDefinition card, CardEffect effect, EnemyState enemy)
        {
            for (var hit = 0; hit < effect.RepeatCount; hit++)
            {
                var multiplier = state.TryConsumeChargedDamageMultiplier();
                var dealt = enemy.Body.TakeDamage(effect.Value * multiplier, state.Player.Sharpness);
                var hitText = effect.RepeatCount > 1 ? $" 第 {hit + 1}/{effect.RepeatCount} 击" : string.Empty;
                var chargeText = multiplier > 1 ? $" 蓄力 x{multiplier}" : string.Empty;
                state.Logs.Add(new BattleLogEntry($"{card.Name}{hitText}{chargeText} 对 {enemy.Body.Name} 造成 {dealt} 点伤害。"));
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
                    if (DealEnemyDamage(state, enemy, intent.Value))
                    {
                        state.Player.AddBurn(intent.SecondaryValue, 2);
                        state.Logs.Add(new BattleLogEntry($"{enemy.Body.Name} 施加灼烧 {intent.SecondaryValue} 层。"));
                    }

                    break;
                case EnemyIntentType.AttackAndFreeze:
                    if (DealEnemyDamage(state, enemy, intent.Value))
                    {
                        state.Player.AddFreeze(intent.SecondaryValue, 2);
                        state.Logs.Add(new BattleLogEntry($"{enemy.Body.Name} 施加冰冻 {intent.SecondaryValue} 层。"));
                    }

                    break;
                case EnemyIntentType.AttackAndStun:
                    if (DealEnemyDamage(state, enemy, intent.Value))
                    {
                        state.Player.AddStun(Math.Max(1, intent.SecondaryValue));
                        state.Logs.Add(new BattleLogEntry($"{enemy.Body.Name} 施加眩晕 {Math.Max(1, intent.SecondaryValue)} 回合。"));
                    }

                    break;
                case EnemyIntentType.Buff:
                    state.Logs.Add(new BattleLogEntry($"{enemy.Body.Name} 正在蓄力。"));
                    break;
                case EnemyIntentType.Heal:
                    enemy.Body.Heal(intent.Value);
                    state.Logs.Add(new BattleLogEntry($"{enemy.Body.Name} 回复 {intent.Value} HP。"));
                    break;
                case EnemyIntentType.BuffAttack:
                    enemy.AddAttackBonus(intent.Value);
                    if (intent.SecondaryValue > 0)
                    {
                        enemy.Body.AddShield(intent.SecondaryValue);
                    }

                    state.Logs.Add(new BattleLogEntry($"{enemy.Body.Name} 强化攻击 +{intent.Value}，护盾 +{intent.SecondaryValue}。"));
                    break;
                case EnemyIntentType.Summon:
                    state.Logs.Add(new BattleLogEntry($"{enemy.Body.Name} 准备召唤。"));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(intent.Type), intent.Type, "Unsupported enemy intent type.");
            }
        }

        private bool DealEnemyDamage(BattleState state, EnemyState enemy, int damage)
        {
            if (state.TryConsumeDodge())
            {
                state.Logs.Add(new BattleLogEntry($"玩家闪避了 {enemy.Body.Name} 的攻击，剩余闪避 {state.DodgeCharges} 次。"));
                if (state.DodgeCounterDamage > 0)
                {
                    var counterDealt = enemy.Body.TakeDamage(state.DodgeCounterDamage, state.Player.Sharpness);
                    state.Logs.Add(new BattleLogEntry($"闪避反击对 {enemy.Body.Name} 造成 {counterDealt} 点伤害。"));
                }

                return false;
            }

            var dealt = state.Player.TakeDamage(damage + enemy.AttackBonus);
            ResolveAttackCounter(state, enemy);
            state.Logs.Add(new BattleLogEntry($"{enemy.Body.Name} 对玩家造成 {dealt} 点伤害。"));
            return true;
        }

        private void ResolveAttackCounter(BattleState state, EnemyState enemy)
        {
            if (state.AttackCounterDamage <= 0 || enemy.Body.IsDefeated)
            {
                return;
            }

            if (!RollChance(state.AttackCounterChancePercent))
            {
                state.Logs.Add(new BattleLogEntry($"attack-counter missed {state.AttackCounterChancePercent}%."));
                return;
            }

            var counterDealt = enemy.Body.TakeDamage(state.AttackCounterDamage, state.Player.Sharpness);
            state.Logs.Add(new BattleLogEntry($"attack-counter dealt {counterDealt} to {enemy.Body.Name}."));
        }

        private void DrawToHandLimit(BattleState state)
        {
            DrawCards(state, Math.Max(0, state.HandLimit - state.Hand.Count) + state.ExtraDrawPerTurn);
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

        private static void ApplyGoldenCorePassive(BattleState state, GoldenCorePassiveDefinition passive)
        {
            if (state == null || passive == null)
            {
                return;
            }

            switch (passive.Type)
            {
                case GoldenCorePassiveType.SwordHeart:
                    state.Player.AddSharpness(passive.EffectValue, 999);
                    state.Logs.Add(new BattleLogEntry($"金丹被动「{passive.Name}」生效：锋锐 +{passive.EffectValue}。"));
                    break;
                case GoldenCorePassiveType.FlowingWater:
                    state.AddExtraDrawPerTurn(passive.EffectValue);
                    state.Logs.Add(new BattleLogEntry($"金丹被动「{passive.Name}」生效：每回合额外抽 {passive.EffectValue} 张牌。"));
                    break;
                case GoldenCorePassiveType.ThunderSeed:
                    state.AddSpiritCostReduction(passive.EffectValue);
                    state.Logs.Add(new BattleLogEntry($"金丹被动「{passive.Name}」生效：功法灵力消耗 -{passive.EffectValue}。"));
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(passive.Type), passive.Type, "Unsupported golden core passive type.");
            }
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

        public static CardDefinition BurningPalm { get; } = new CardDefinition(
            "burning_palm",
            "焚天掌",
            1,
            new[]
            {
                new CardUpgradeOption(
                    "burning_palm_damage_1",
                    "赤焰掌",
                    "伤害提升到 8，灼烧保持 2 层。",
                    new CardDefinition("burning_palm_damage_1", "赤焰掌", 1, new CardEffect(CardEffectType.Damage, 8), new CardEffect(CardEffectType.Burn, 2, duration: 3))),
                new CardUpgradeOption(
                    "burning_palm_burn_1",
                    "燎原掌",
                    "伤害保持 5，灼烧提升到 4 层。",
                    new CardDefinition("burning_palm_burn_1", "燎原掌", 1, new CardEffect(CardEffectType.Damage, 5), new CardEffect(CardEffectType.Burn, 4, duration: 3))),
            },
            new CardEffect(CardEffectType.Damage, 5),
            new CardEffect(CardEffectType.Burn, 2, duration: 3));

        public static CardDefinition FlameFormula { get; } = new CardDefinition(
            "flame_formula",
            "烈火诀",
            1,
            new[]
            {
                new CardUpgradeOption(
                    "flame_formula_burn_1",
                    "烈焰诀",
                    "对全体敌人施加 3 层灼烧。",
                    new CardDefinition("flame_formula_burn_1", "烈焰诀", 1, new CardEffect(CardEffectType.Burn, 3, CardTarget.EnemyAll, 3))),
                new CardUpgradeOption(
                    "flame_formula_draw_1",
                    "心火诀",
                    "对全体敌人施加 2 层灼烧，并抽 1 张牌。",
                    new CardDefinition("flame_formula_draw_1", "心火诀", 1, new CardEffect(CardEffectType.Burn, 2, CardTarget.EnemyAll, 3), new CardEffect(CardEffectType.Draw, 1, CardTarget.Self))),
            },
            new CardEffect(CardEffectType.Burn, 2, CardTarget.EnemyAll, 3));

        public static CardDefinition FireCloudStep { get; } = new CardDefinition(
            "fire_cloud_step",
            "火云步",
            1,
            new[]
            {
                new CardUpgradeOption(
                    "fire_cloud_step_guard_1",
                    "火云护步",
                    "获得 9 护盾并抽 1 张牌。",
                    new CardDefinition("fire_cloud_step_guard_1", "火云护步", 1, new CardEffect(CardEffectType.Shield, 9, CardTarget.Self), new CardEffect(CardEffectType.Draw, 1, CardTarget.Self))),
                new CardUpgradeOption(
                    "fire_cloud_step_burn_1",
                    "踏火行",
                    "获得 6 护盾，抽 1 张牌，并对敌人施加 1 层灼烧。",
                    new CardDefinition("fire_cloud_step_burn_1", "踏火行", 1, new CardEffect(CardEffectType.Shield, 6, CardTarget.Self), new CardEffect(CardEffectType.Draw, 1, CardTarget.Self), new CardEffect(CardEffectType.Burn, 1, duration: 2))),
            },
            new CardEffect(CardEffectType.Shield, 6, CardTarget.Self),
            new CardEffect(CardEffectType.Draw, 1, CardTarget.Self));

        public static CardDefinition ThunderTalisman { get; } = new CardDefinition(
            "thunder_talisman",
            "雷击符",
            1,
            new[]
            {
                new CardUpgradeOption(
                    "thunder_talisman_damage_1",
                    "雷击符·强",
                    "基础造成 5 伤害，30% 概率暴击造成 15 伤害。",
                    new CardDefinition(
                        "thunder_talisman_damage_1",
                        "雷击符·强",
                        1,
                        new[]
                        {
                            new CardUpgradeOption(
                                "thunder_talisman_damage_2_chance",
                                "雷击符·极",
                                "暴击概率提升到 50%，暴击造成 15 伤害。",
                                new CardDefinition("thunder_talisman_damage_2_chance", "雷击符·极", 1, new CardEffect(CardEffectType.ChanceDamage, 15, chancePercent: 50, fallbackValue: 5))),
                            new CardUpgradeOption(
                                "thunder_talisman_damage_2_chain",
                                "雷击符·连",
                                "暴击时额外连锁 1 名敌人，造成 7 伤害。",
                                new CardDefinition("thunder_talisman_damage_2_chain", "雷击符·连", 1, new CardEffect(CardEffectType.ChainOnChanceDamage, 15, chancePercent: 30, fallbackValue: 5, secondaryValue: 7))),
                        },
                        new CardEffect(CardEffectType.ChanceDamage, 15, chancePercent: 30, fallbackValue: 5))),
                new CardUpgradeOption(
                    "thunder_talisman_break_1",
                    "雷击符·晕",
                    "基础造成 5 伤害，30% 概率暴击造成 10 伤害，并施加 1 层破防。",
                    new CardDefinition("thunder_talisman_break_1", "雷击符·晕", 1, new CardEffect(CardEffectType.ChanceDamage, 10, chancePercent: 30, fallbackValue: 5), new CardEffect(CardEffectType.BreakDefense, 1))),
            },
            new CardEffect(CardEffectType.ChanceDamage, 10, chancePercent: 30, fallbackValue: 5));

        public static CardDefinition HeavenlyThunderSpell { get; } = new CardDefinition(
            "heavenly_thunder_spell",
            "天雷咒",
            2,
            new[]
            {
                new CardUpgradeOption(
                    "heavenly_thunder_spell_stun_1",
                    "天雷咒·强",
                    "造成 10 伤害，60% 概率眩晕 1 回合。",
                    new CardDefinition("heavenly_thunder_spell_stun_1", "天雷咒·强", 2, new CardEffect(CardEffectType.Damage, 10), new CardEffect(CardEffectType.ChanceStun, 0, duration: 1, chancePercent: 60))),
                new CardUpgradeOption(
                    "heavenly_thunder_spell_cost_1",
                    "天雷咒·速",
                    "灵力消耗降为 1，造成 10 伤害，40% 概率眩晕 1 回合。",
                    new CardDefinition(
                        "heavenly_thunder_spell_cost_1",
                        "天雷咒·速",
                        1,
                        new[]
                        {
                            new CardUpgradeOption(
                                "heavenly_thunder_spell_cost_2_chain",
                                "天雷咒·连",
                                "眩晕成功时额外连锁 1 名敌人，造成 5 伤害。",
                                new CardDefinition("heavenly_thunder_spell_cost_2_chain", "天雷咒·连", 1, new CardEffect(CardEffectType.ChainOnChanceStun, 10, duration: 1, chancePercent: 40, secondaryValue: 5))),
                            new CardUpgradeOption(
                                "heavenly_thunder_spell_cost_2_break",
                                "天雷咒·晕",
                                "造成 10 伤害，40% 概率眩晕；并施加 1 层破防。",
                                new CardDefinition("heavenly_thunder_spell_cost_2_break", "天雷咒·晕", 1, new CardEffect(CardEffectType.Damage, 10), new CardEffect(CardEffectType.ChanceStun, 0, duration: 1, chancePercent: 40), new CardEffect(CardEffectType.BreakDefense, 1))),
                        },
                        new CardEffect(CardEffectType.Damage, 10),
                        new CardEffect(CardEffectType.ChanceStun, 0, duration: 1, chancePercent: 40))),
            },
            new CardEffect(CardEffectType.Damage, 10),
            new CardEffect(CardEffectType.ChanceStun, 0, duration: 1, chancePercent: 40));

        public static CardDefinition LightningChain { get; } = new CardDefinition(
            "lightning_chain",
            "雷电链",
            2,
            new[]
            {
                new CardUpgradeOption(
                    "lightning_chain_chance_1",
                    "雷电链·强",
                    "造成 8 伤害，75% 概率连锁至另一名敌人造成 4 伤害。",
                    new CardDefinition(
                        "lightning_chain_chance_1",
                        "雷电链·强",
                        2,
                        new[]
                        {
                            new CardUpgradeOption(
                                "lightning_chain_chance_2_damage",
                                "雷电链·极",
                                "造成 8 伤害，75% 概率连锁造成 6 伤害。",
                                new CardDefinition("lightning_chain_chance_2_damage", "雷电链·极", 2, new CardEffect(CardEffectType.ChanceChainDamage, 8, chancePercent: 75, secondaryValue: 6))),
                            new CardUpgradeOption(
                                "lightning_chain_chance_2_repeat",
                                "雷电链·多",
                                "造成 8 伤害，75% 概率连锁造成 4 伤害；可重复命中同一目标，最多连锁 4 次。",
                                new CardDefinition("lightning_chain_chance_2_repeat", "雷电链·多", 2, new CardEffect(CardEffectType.ChanceChainDamageRepeatTarget, 8, chancePercent: 75, secondaryValue: 4, repeatCount: 4))),
                        },
                        new CardEffect(CardEffectType.ChanceChainDamage, 8, chancePercent: 75, secondaryValue: 4))),
                new CardUpgradeOption(
                    "lightning_chain_damage_1",
                    "雷电链·广",
                    "造成 12 伤害，50% 概率连锁至另一名敌人造成 4 伤害。",
                    new CardDefinition(
                        "lightning_chain_damage_1",
                        "雷电链·广",
                        2,
                        new[]
                        {
                            new CardUpgradeOption(
                                "lightning_chain_damage_2_cost",
                                "雷电链·速",
                                "灵力消耗降为 1，造成 12 伤害，50% 概率连锁造成 4 伤害。",
                                new CardDefinition("lightning_chain_damage_2_cost", "雷电链·速", 1, new CardEffect(CardEffectType.ChanceChainDamage, 12, chancePercent: 50, secondaryValue: 4))),
                            new CardUpgradeOption(
                                "lightning_chain_damage_2_stun",
                                "雷电链·晕",
                                "造成 12 伤害，50% 概率连锁造成 4 伤害；连锁伤害有 20% 概率眩晕。",
                                new CardDefinition("lightning_chain_damage_2_stun", "雷电链·晕", 2, new CardEffect(CardEffectType.ChanceChainDamageWithStun, 12, duration: 1, chancePercent: 50, fallbackValue: 20, secondaryValue: 4))),
                        },
                        new CardEffect(CardEffectType.ChanceChainDamage, 12, chancePercent: 50, secondaryValue: 4))),
            },
            new CardEffect(CardEffectType.ChanceChainDamage, 8, chancePercent: 50, secondaryValue: 4));

        public static CardDefinition ThunderCharge { get; } = new CardDefinition(
            "thunder_charge",
            "蓄雷术",
            1,
            new[]
            {
                new CardUpgradeOption(
                    "thunder_charge_guard_1",
                    "蓄雷大法",
                    "下次攻击伤害翻倍，并获得 8 护盾。",
                    new CardDefinition("thunder_charge_guard_1", "蓄雷大法", 1, new CardEffect(CardEffectType.ChargeDamage, 2, CardTarget.Self), new CardEffect(CardEffectType.Shield, 8, CardTarget.Self))),
                new CardUpgradeOption(
                    "thunder_charge_quick_1",
                    "蓄雷速发",
                    "灵力消耗降为 0，下次攻击伤害翻倍，并获得 4 护盾。",
                    new CardDefinition("thunder_charge_quick_1", "蓄雷速发", 0, new CardEffect(CardEffectType.ChargeDamage, 2, CardTarget.Self), new CardEffect(CardEffectType.Shield, 4, CardTarget.Self))),
            },
            new CardEffect(CardEffectType.ChargeDamage, 2, CardTarget.Self),
            new CardEffect(CardEffectType.Shield, 4, CardTarget.Self));

        public static CardDefinition ThunderEscape { get; } = new CardDefinition(
            "thunder_escape",
            "雷遁术",
            0,
            new[]
            {
                new CardUpgradeOption(
                    "thunder_escape_draw_1",
                    "雷遁术·速",
                    "抽牌提升到 3 张。",
                    new CardDefinition("thunder_escape_draw_1", "雷遁术·速", 0, new CardEffect(CardEffectType.Draw, 3, CardTarget.Self))),
                new CardUpgradeOption(
                    "thunder_escape_guard_1",
                    "雷遁术·护",
                    "抽 2 张牌，并获得 3 护盾。",
                    new CardDefinition("thunder_escape_guard_1", "雷遁术·护", 0, new CardEffect(CardEffectType.Draw, 2, CardTarget.Self), new CardEffect(CardEffectType.Shield, 3, CardTarget.Self))),
            },
            new CardEffect(CardEffectType.Draw, 2, CardTarget.Self));

        public static CardDefinition FiveThunderOrthodoxy { get; } = new CardDefinition(
            "five_thunder_orthodoxy",
            "五雷正法",
            3,
            new[]
            {
                new CardUpgradeOption(
                    "five_thunder_orthodoxy_damage_1",
                    "五雷正法·强",
                    "造成 13 伤害，50% 概率眩晕 1 回合，50% 概率连锁造成 4 伤害。",
                    new CardDefinition(
                        "five_thunder_orthodoxy_damage_1",
                        "五雷正法·强",
                        3,
                        new[]
                        {
                            new CardUpgradeOption(
                                "five_thunder_orthodoxy_damage_2_stun",
                                "五雷正法·极",
                                "造成 13 伤害，75% 概率眩晕 1 回合，50% 概率连锁造成 4 伤害。",
                                new CardDefinition("five_thunder_orthodoxy_damage_2_stun", "五雷正法·极", 3, new CardEffect(CardEffectType.ChanceChainDamage, 13, chancePercent: 50, secondaryValue: 4), new CardEffect(CardEffectType.ChanceStun, 0, duration: 1, chancePercent: 75))),
                            new CardUpgradeOption(
                                "five_thunder_orthodoxy_damage_2_chain",
                                "五雷正法·爆",
                                "造成 13 伤害，50% 概率眩晕 1 回合，50% 概率连锁造成 7 伤害。",
                                new CardDefinition("five_thunder_orthodoxy_damage_2_chain", "五雷正法·爆", 3, new CardEffect(CardEffectType.ChanceChainDamage, 13, chancePercent: 50, secondaryValue: 7), new CardEffect(CardEffectType.ChanceStun, 0, duration: 1, chancePercent: 50))),
                        },
                        new CardEffect(CardEffectType.ChanceChainDamage, 13, chancePercent: 50, secondaryValue: 4),
                        new CardEffect(CardEffectType.ChanceStun, 0, duration: 1, chancePercent: 50))),
                new CardUpgradeOption(
                    "five_thunder_orthodoxy_stable_1",
                    "五雷正法·稳",
                    "造成 8 伤害，75% 概率眩晕 1 回合，75% 概率连锁造成 4 伤害。",
                    new CardDefinition(
                        "five_thunder_orthodoxy_stable_1",
                        "五雷正法·稳",
                        3,
                        new[]
                        {
                            new CardUpgradeOption(
                                "five_thunder_orthodoxy_stable_2_cost",
                                "五雷正法·速",
                                "灵力消耗降为 2，造成 8 伤害，75% 概率眩晕 1 回合，75% 概率连锁造成 4 伤害。",
                                new CardDefinition("five_thunder_orthodoxy_stable_2_cost", "五雷正法·速", 2, new CardEffect(CardEffectType.ChanceChainDamage, 8, chancePercent: 75, secondaryValue: 4), new CardEffect(CardEffectType.ChanceStun, 0, duration: 1, chancePercent: 75))),
                            new CardUpgradeOption(
                                "five_thunder_orthodoxy_stable_2_certain_stun",
                                "五雷正法·灭",
                                "伤害降为 5，必定眩晕 1 回合，75% 概率连锁造成 4 伤害。",
                                new CardDefinition("five_thunder_orthodoxy_stable_2_certain_stun", "五雷正法·灭", 3, new CardEffect(CardEffectType.ChanceChainDamage, 5, chancePercent: 75, secondaryValue: 4), new CardEffect(CardEffectType.Stun, 0, duration: 1))),
                        },
                        new CardEffect(CardEffectType.ChanceChainDamage, 8, chancePercent: 75, secondaryValue: 4),
                        new CardEffect(CardEffectType.ChanceStun, 0, duration: 1, chancePercent: 75))),
            },
            new CardEffect(CardEffectType.ChanceChainDamage, 8, chancePercent: 50, secondaryValue: 4),
            new CardEffect(CardEffectType.ChanceStun, 0, duration: 1, chancePercent: 50));

        public static CardDefinition ThunderousBarrage { get; } = new CardDefinition(
            "thunderous_barrage",
            "雷霆万钧",
            2,
            new[]
            {
                new CardUpgradeOption(
                    "thunderous_barrage_hits_1",
                    "雷霆万钧·多",
                    "造成 4 伤害 x5，每击独立 10% 暴击。",
                    new CardDefinition(
                        "thunderous_barrage_hits_1",
                        "雷霆万钧·多",
                        2,
                        new[]
                        {
                            new CardUpgradeOption(
                                "thunderous_barrage_hits_2_more",
                                "雷霆万钧·极",
                                "造成 4 伤害 x7，每击独立 10% 暴击。",
                                new CardDefinition("thunderous_barrage_hits_2_more", "雷霆万钧·极", 2, new CardEffect(CardEffectType.ChanceDamage, 4, repeatCount: 7, chancePercent: 10, fallbackValue: 4))),
                            new CardUpgradeOption(
                                "thunderous_barrage_hits_2_damage",
                                "雷霆万钧·强",
                                "造成 6 伤害 x3，每击独立 10% 暴击。",
                                new CardDefinition("thunderous_barrage_hits_2_damage", "雷霆万钧·强", 2, new CardEffect(CardEffectType.ChanceDamage, 6, repeatCount: 3, chancePercent: 10, fallbackValue: 6))),
                        },
                        new CardEffect(CardEffectType.ChanceDamage, 4, repeatCount: 5, chancePercent: 10, fallbackValue: 4))),
                new CardUpgradeOption(
                    "thunderous_barrage_critical_1",
                    "雷霆万钧·暴",
                    "造成 4 伤害 x3，每击独立 25% 暴击。",
                    new CardDefinition(
                        "thunderous_barrage_critical_1",
                        "雷霆万钧·暴",
                        2,
                        new[]
                        {
                            new CardUpgradeOption(
                                "thunderous_barrage_critical_2_stun",
                                "雷霆万钧·晕",
                                "造成 4 伤害 x3，每击独立 25% 暴击；暴击时 20% 概率眩晕。",
                                new CardDefinition("thunderous_barrage_critical_2_stun", "雷霆万钧·晕", 2, new CardEffect(CardEffectType.ChanceDamageWithStun, 4, duration: 1, repeatCount: 3, chancePercent: 25, fallbackValue: 20))),
                            new CardUpgradeOption(
                                "thunderous_barrage_critical_2_chain",
                                "雷霆万钧·连",
                                "造成 4 伤害 x3，每击独立 25% 暴击；每击 30% 概率连锁 2 伤害。",
                                new CardDefinition("thunderous_barrage_critical_2_chain", "雷霆万钧·连", 2, new CardEffect(CardEffectType.ChanceDamageWithChain, 4, repeatCount: 3, chancePercent: 25, fallbackValue: 2, secondaryValue: 30))),
                        },
                        new CardEffect(CardEffectType.ChanceDamage, 4, repeatCount: 3, chancePercent: 25, fallbackValue: 4))),
            },
            new CardEffect(CardEffectType.ChanceDamage, 4, repeatCount: 3, chancePercent: 10, fallbackValue: 4));

        public static CardDefinition ThunderHammer { get; } = new CardDefinition(
            "thunder_hammer",
            "雷神之锤",
            3,
            new[]
            {
                new CardUpgradeOption(
                    "thunder_hammer_bonus_1",
                    "雷神之锤·强",
                    "造成 22 伤害；若本回合已触发暴击，额外造成 18 伤害。",
                    new CardDefinition(
                        "thunder_hammer_bonus_1",
                        "雷神之锤·强",
                        3,
                        new[]
                        {
                            new CardUpgradeOption(
                                "thunder_hammer_bonus_2_damage",
                                "雷神之锤·极",
                                "造成 22 伤害；若本回合已触发暴击，额外造成 26 伤害。",
                                new CardDefinition("thunder_hammer_bonus_2_damage", "雷神之锤·极", 3, new CardEffect(CardEffectType.DamageAfterCriticalTriggered, 22, fallbackValue: 26))),
                            new CardUpgradeOption(
                                "thunder_hammer_bonus_2_stun",
                                "雷神之锤·晕",
                                "造成 22 伤害；若本回合已触发暴击，额外造成 18 伤害并眩晕。",
                                new CardDefinition("thunder_hammer_bonus_2_stun", "雷神之锤·晕", 3, new CardEffect(CardEffectType.DamageAfterCriticalTriggeredWithStun, 22, duration: 1, fallbackValue: 18))),
                        },
                        new CardEffect(CardEffectType.DamageAfterCriticalTriggered, 22, fallbackValue: 18))),
                new CardUpgradeOption(
                    "thunder_hammer_stable_1",
                    "雷神之锤·稳",
                    "造成 27 伤害；若本回合已触发暴击，额外造成 10 伤害。",
                    new CardDefinition(
                        "thunder_hammer_stable_1",
                        "雷神之锤·稳",
                        3,
                        new[]
                        {
                            new CardUpgradeOption(
                                "thunder_hammer_stable_2_cost",
                                "雷神之锤·速",
                                "灵力消耗降为 2，造成 27 伤害；若本回合已触发暴击，额外造成 10 伤害。",
                                new CardDefinition("thunder_hammer_stable_2_cost", "雷神之锤·速", 2, new CardEffect(CardEffectType.DamageAfterCriticalTriggered, 27, fallbackValue: 10))),
                            new CardUpgradeOption(
                                "thunder_hammer_stable_2_chain",
                                "雷神之锤·连",
                                "造成 27 伤害；若本回合已触发暴击，暴击奖励连锁全体敌人，各造成 10 伤害。",
                                new CardDefinition("thunder_hammer_stable_2_chain", "雷神之锤·连", 3, new CardEffect(CardEffectType.DamageAfterCriticalTriggeredChainAll, 27, fallbackValue: 10))),
                        },
                        new CardEffect(CardEffectType.DamageAfterCriticalTriggered, 27, fallbackValue: 10))),
            },
            new CardEffect(CardEffectType.DamageAfterCriticalTriggered, 22, fallbackValue: 10));

        public static CardDefinition ThunderDodgeStrike { get; } = new CardDefinition(
            "thunder_dodge_strike",
            "雷遁·瞬击",
            1,
            new[]
            {
                new CardUpgradeOption(
                    "thunder_dodge_strike_damage_1",
                    "雷遁·连击",
                    "造成 10 伤害，获得 1 次闪避。",
                    new CardDefinition(
                        "thunder_dodge_strike_damage_1",
                        "雷遁·连击",
                        1,
                        new[]
                        {
                            new CardUpgradeOption(
                                "thunder_dodge_strike_damage_2_multi",
                                "雷遁·乱击",
                                "造成 6 伤害 x2，获得 1 次闪避。",
                                new CardDefinition("thunder_dodge_strike_damage_2_multi", "雷遁·乱击", 1, new CardEffect(CardEffectType.Damage, 6, repeatCount: 2), new CardEffect(CardEffectType.Dodge, 1, CardTarget.Self))),
                            new CardUpgradeOption(
                                "thunder_dodge_strike_damage_2_dodge",
                                "雷遁·护击",
                                "造成 10 伤害，获得 2 次闪避。",
                                new CardDefinition("thunder_dodge_strike_damage_2_dodge", "雷遁·护击", 1, new CardEffect(CardEffectType.Damage, 10), new CardEffect(CardEffectType.Dodge, 2, CardTarget.Self))),
                        },
                        new CardEffect(CardEffectType.Damage, 10),
                        new CardEffect(CardEffectType.Dodge, 1, CardTarget.Self))),
                new CardUpgradeOption(
                    "thunder_dodge_strike_counter_1",
                    "雷遁·闪击",
                    "造成 6 伤害，获得 1 次闪避；闪避成功时额外造成 5 伤害。",
                    new CardDefinition(
                        "thunder_dodge_strike_counter_1",
                        "雷遁·闪击",
                        1,
                        new[]
                        {
                            new CardUpgradeOption(
                                "thunder_dodge_strike_counter_2_damage",
                                "雷遁·反击",
                                "造成 6 伤害，获得 1 次闪避；闪避成功时对攻击者造成 8 伤害。",
                                new CardDefinition("thunder_dodge_strike_counter_2_damage", "雷遁·反击", 1, new CardEffect(CardEffectType.Damage, 6), new CardEffect(CardEffectType.Dodge, 1, CardTarget.Self), new CardEffect(CardEffectType.DodgeCounter, 8, CardTarget.Self))),
                            new CardUpgradeOption(
                                "thunder_dodge_strike_counter_2_cost",
                                "雷遁·灵击",
                                "灵力消耗降为 0，造成 6 伤害，获得 1 次闪避。",
                                new CardDefinition("thunder_dodge_strike_counter_2_cost", "雷遁·灵击", 0, new CardEffect(CardEffectType.Damage, 6), new CardEffect(CardEffectType.Dodge, 1, CardTarget.Self))),
                        },
                        new CardEffect(CardEffectType.Damage, 6),
                        new CardEffect(CardEffectType.Dodge, 1, CardTarget.Self),
                        new CardEffect(CardEffectType.DodgeCounter, 5, CardTarget.Self))),
            },
            new CardEffect(CardEffectType.Damage, 6),
            new CardEffect(CardEffectType.Dodge, 1, CardTarget.Self));

        public static CardDefinition ThunderShield { get; } = new CardDefinition(
            "thunder_shield",
            "雷盾",
            1,
            new[]
            {
                new CardUpgradeOption(
                    "thunder_shield_guard_1",
                    "雷盾·强",
                    "获得 8 护盾，30% 概率对攻击者造成 3 伤害。",
                    new CardDefinition(
                        "thunder_shield_guard_1",
                        "雷盾·强",
                        1,
                        new[]
                        {
                            new CardUpgradeOption(
                                "thunder_shield_guard_2_heavy",
                                "雷盾·极",
                                "获得 12 护盾，30% 概率对攻击者造成 3 伤害。",
                                new CardDefinition("thunder_shield_guard_2_heavy", "雷盾·极", 1, new CardEffect(CardEffectType.Shield, 12, CardTarget.Self), new CardEffect(CardEffectType.AttackCounter, 3, CardTarget.Self, chancePercent: 30))),
                            new CardUpgradeOption(
                                "thunder_shield_guard_2_chance",
                                "雷盾·爆",
                                "获得 8 护盾，60% 概率对攻击者造成 3 伤害。",
                                new CardDefinition("thunder_shield_guard_2_chance", "雷盾·爆", 1, new CardEffect(CardEffectType.Shield, 8, CardTarget.Self), new CardEffect(CardEffectType.AttackCounter, 3, CardTarget.Self, chancePercent: 60))),
                        },
                        new CardEffect(CardEffectType.Shield, 8, CardTarget.Self),
                        new CardEffect(CardEffectType.AttackCounter, 3, CardTarget.Self, chancePercent: 30))),
                new CardUpgradeOption(
                    "thunder_shield_counter_1",
                    "雷盾·反",
                    "获得 5 护盾，30% 概率对攻击者造成 7 伤害。",
                    new CardDefinition(
                        "thunder_shield_counter_1",
                        "雷盾·反",
                        1,
                        new[]
                        {
                            new CardUpgradeOption(
                                "thunder_shield_counter_2_cost",
                                "雷盾·速",
                                "灵力消耗降为 0，获得 5 护盾，30% 概率对攻击者造成 7 伤害。",
                                new CardDefinition("thunder_shield_counter_2_cost", "雷盾·速", 0, new CardEffect(CardEffectType.Shield, 5, CardTarget.Self), new CardEffect(CardEffectType.AttackCounter, 7, CardTarget.Self, chancePercent: 30))),
                            new CardUpgradeOption(
                                "thunder_shield_counter_2_heavy",
                                "雷盾·震",
                                "获得 5 护盾，60% 概率对攻击者造成 7 伤害。",
                                new CardDefinition("thunder_shield_counter_2_heavy", "雷盾·震", 1, new CardEffect(CardEffectType.Shield, 5, CardTarget.Self), new CardEffect(CardEffectType.AttackCounter, 7, CardTarget.Self, chancePercent: 60))),
                        },
                        new CardEffect(CardEffectType.Shield, 5, CardTarget.Self),
                        new CardEffect(CardEffectType.AttackCounter, 7, CardTarget.Self, chancePercent: 30))),
            },
            new CardEffect(CardEffectType.Shield, 5, CardTarget.Self),
            new CardEffect(CardEffectType.AttackCounter, 3, CardTarget.Self, chancePercent: 30));

        public static CardDefinition ThunderStrikeRebound { get; } = new CardDefinition(
            "thunder_strike_rebound",
            "雷击反弹",
            1,
            new[]
            {
                new CardUpgradeOption(
                    "thunder_strike_rebound_damage_1",
                    "雷击反弹·强",
                    "获得 2 护盾，本回合受到攻击时必定对攻击者造成 7 伤害。",
                    new CardDefinition(
                        "thunder_strike_rebound_damage_1",
                        "雷击反弹·强",
                        1,
                        new[]
                        {
                            new CardUpgradeOption(
                                "thunder_strike_rebound_damage_2_heavy",
                                "雷击反弹·极",
                                "获得 2 护盾，本回合受到攻击时必定对攻击者造成 10 伤害。",
                                new CardDefinition("thunder_strike_rebound_damage_2_heavy", "雷击反弹·极", 1, new CardEffect(CardEffectType.Shield, 2, CardTarget.Self), new CardEffect(CardEffectType.AttackCounter, 10, CardTarget.Self))),
                            new CardUpgradeOption(
                                "thunder_strike_rebound_damage_2_cost",
                                "雷击反弹·速",
                                "灵力消耗降为 0，获得 2 护盾，本回合受到攻击时必定对攻击者造成 7 伤害。",
                                new CardDefinition("thunder_strike_rebound_damage_2_cost", "雷击反弹·速", 0, new CardEffect(CardEffectType.Shield, 2, CardTarget.Self), new CardEffect(CardEffectType.AttackCounter, 7, CardTarget.Self))),
                        },
                        new CardEffect(CardEffectType.Shield, 2, CardTarget.Self),
                        new CardEffect(CardEffectType.AttackCounter, 7, CardTarget.Self))),
                new CardUpgradeOption(
                    "thunder_strike_rebound_guard_1",
                    "雷击反弹·护",
                    "获得 7 护盾，本回合受到攻击时必定对攻击者造成 4 伤害。",
                    new CardDefinition(
                        "thunder_strike_rebound_guard_1",
                        "雷击反弹·护",
                        1,
                        new[]
                        {
                            new CardUpgradeOption(
                                "thunder_strike_rebound_guard_2_heavy",
                                "雷击反弹·坚",
                                "获得 10 护盾，本回合受到攻击时必定对攻击者造成 4 伤害。",
                                new CardDefinition("thunder_strike_rebound_guard_2_heavy", "雷击反弹·坚", 1, new CardEffect(CardEffectType.Shield, 10, CardTarget.Self), new CardEffect(CardEffectType.AttackCounter, 4, CardTarget.Self))),
                            new CardUpgradeOption(
                                "thunder_strike_rebound_guard_2_shock",
                                "雷击反弹·震",
                                "获得 7 护盾，本回合受到攻击时必定对攻击者造成 7 伤害。",
                                new CardDefinition("thunder_strike_rebound_guard_2_shock", "雷击反弹·震", 1, new CardEffect(CardEffectType.Shield, 7, CardTarget.Self), new CardEffect(CardEffectType.AttackCounter, 7, CardTarget.Self))),
                        },
                        new CardEffect(CardEffectType.Shield, 7, CardTarget.Self),
                        new CardEffect(CardEffectType.AttackCounter, 4, CardTarget.Self))),
            },
            new CardEffect(CardEffectType.Shield, 2, CardTarget.Self),
            new CardEffect(CardEffectType.AttackCounter, 4, CardTarget.Self));

        public static CardDefinition RockStrike { get; } = new CardDefinition(
            "rock_strike",
            "岩击",
            1,
            new[]
            {
                new CardUpgradeOption(
                    "rock_strike_damage_1",
                    "岩击·重",
                    "造成 9 伤害，获得 3 护盾。",
                    new CardDefinition(
                        "rock_strike_damage_1",
                        "岩击·重",
                        1,
                        new[]
                        {
                            new CardUpgradeOption(
                                "rock_strike_damage_2_break",
                                "裂岩击",
                                "造成 12 伤害，施加 1 层破防，获得 3 护盾。",
                                new CardDefinition("rock_strike_damage_2_break", "裂岩击", 1, new CardEffect(CardEffectType.Damage, 12), new CardEffect(CardEffectType.BreakDefense, 1), new CardEffect(CardEffectType.Shield, 3, CardTarget.Self))),
                            new CardUpgradeOption(
                                "rock_strike_damage_2_guard",
                                "玄岩击",
                                "造成 10 伤害，获得 7 护盾。",
                                new CardDefinition("rock_strike_damage_2_guard", "玄岩击", 1, new CardEffect(CardEffectType.Damage, 10), new CardEffect(CardEffectType.Shield, 7, CardTarget.Self))),
                        },
                        new CardEffect(CardEffectType.Damage, 9),
                        new CardEffect(CardEffectType.Shield, 3, CardTarget.Self))),
                new CardUpgradeOption(
                    "rock_strike_guard_1",
                    "岩击·守",
                    "造成 6 伤害，获得 6 护盾。",
                    new CardDefinition(
                        "rock_strike_guard_1",
                        "岩击·守",
                        1,
                        new[]
                        {
                            new CardUpgradeOption(
                                "rock_strike_guard_2_counter",
                                "守岩反震",
                                "造成 6 伤害，获得 6 护盾，本回合受到攻击时对攻击者造成 4 伤害。",
                                new CardDefinition("rock_strike_guard_2_counter", "守岩反震", 1, new CardEffect(CardEffectType.Damage, 6), new CardEffect(CardEffectType.Shield, 6, CardTarget.Self), new CardEffect(CardEffectType.AttackCounter, 4, CardTarget.Self))),
                            new CardUpgradeOption(
                                "rock_strike_guard_2_draw",
                                "流岩击",
                                "造成 6 伤害，获得 5 护盾，抽 1 张牌。",
                                new CardDefinition("rock_strike_guard_2_draw", "流岩击", 1, new CardEffect(CardEffectType.Damage, 6), new CardEffect(CardEffectType.Shield, 5, CardTarget.Self), new CardEffect(CardEffectType.Draw, 1, CardTarget.Self))),
                        },
                        new CardEffect(CardEffectType.Damage, 6),
                        new CardEffect(CardEffectType.Shield, 6, CardTarget.Self))),
            },
            new CardEffect(CardEffectType.Damage, 6),
            new CardEffect(CardEffectType.Shield, 3, CardTarget.Self));

        public static CardDefinition EarthSplittingPalm { get; } = new CardDefinition(
            "earth_splitting_palm",
            "裂地掌",
            2,
            new[]
            {
                new CardUpgradeOption(
                    "earth_splitting_palm_damage_1",
                    "裂地掌·崩",
                    "造成 14 伤害，施加 1 层破防。",
                    new CardDefinition(
                        "earth_splitting_palm_damage_1",
                        "裂地掌·崩",
                        2,
                        new[]
                        {
                            new CardUpgradeOption(
                                "earth_splitting_palm_damage_2_break",
                                "崩山掌",
                                "造成 16 伤害，施加 2 层破防。",
                                new CardDefinition("earth_splitting_palm_damage_2_break", "崩山掌", 2, new CardEffect(CardEffectType.Damage, 16), new CardEffect(CardEffectType.BreakDefense, 2))),
                            new CardUpgradeOption(
                                "earth_splitting_palm_damage_2_cost",
                                "裂地掌·疾",
                                "灵力消耗降为 1，造成 13 伤害，施加 1 层破防。",
                                new CardDefinition("earth_splitting_palm_damage_2_cost", "裂地掌·疾", 1, new CardEffect(CardEffectType.Damage, 13), new CardEffect(CardEffectType.BreakDefense, 1))),
                        },
                        new CardEffect(CardEffectType.Damage, 14),
                        new CardEffect(CardEffectType.BreakDefense, 1))),
                new CardUpgradeOption(
                    "earth_splitting_palm_guard_1",
                    "裂地掌·固",
                    "造成 10 伤害，施加 1 层破防，获得 6 护盾。",
                    new CardDefinition(
                        "earth_splitting_palm_guard_1",
                        "裂地掌·固",
                        2,
                        new[]
                        {
                            new CardUpgradeOption(
                                "earth_splitting_palm_guard_2_heavy",
                                "镇岳掌",
                                "造成 12 伤害，施加 1 层破防，获得 10 护盾。",
                                new CardDefinition("earth_splitting_palm_guard_2_heavy", "镇岳掌", 2, new CardEffect(CardEffectType.Damage, 12), new CardEffect(CardEffectType.BreakDefense, 1), new CardEffect(CardEffectType.Shield, 10, CardTarget.Self))),
                            new CardUpgradeOption(
                                "earth_splitting_palm_guard_2_counter",
                                "震岳掌",
                                "造成 10 伤害，施加 1 层破防，本回合受到攻击时对攻击者造成 8 伤害。",
                                new CardDefinition("earth_splitting_palm_guard_2_counter", "震岳掌", 2, new CardEffect(CardEffectType.Damage, 10), new CardEffect(CardEffectType.BreakDefense, 1), new CardEffect(CardEffectType.AttackCounter, 8, CardTarget.Self))),
                        },
                        new CardEffect(CardEffectType.Damage, 10),
                        new CardEffect(CardEffectType.BreakDefense, 1),
                        new CardEffect(CardEffectType.Shield, 6, CardTarget.Self))),
            },
            new CardEffect(CardEffectType.Damage, 10),
            new CardEffect(CardEffectType.BreakDefense, 1));

        public static CardDefinition RockWall { get; } = new CardDefinition(
            "rock_wall",
            "岩壁",
            1,
            new[]
            {
                new CardUpgradeOption(
                    "rock_wall_guard_1",
                    "岩壁·厚",
                    "获得 12 护盾。",
                    new CardDefinition(
                        "rock_wall_guard_1",
                        "岩壁·厚",
                        1,
                        new[]
                        {
                            new CardUpgradeOption(
                                "rock_wall_guard_2_heavy",
                                "玄岩壁",
                                "获得 16 护盾。",
                                new CardDefinition("rock_wall_guard_2_heavy", "玄岩壁", 1, new CardEffect(CardEffectType.Shield, 16, CardTarget.Self))),
                            new CardUpgradeOption(
                                "rock_wall_guard_2_draw",
                                "流转岩壁",
                                "获得 12 护盾，抽 1 张牌。",
                                new CardDefinition("rock_wall_guard_2_draw", "流转岩壁", 1, new CardEffect(CardEffectType.Shield, 12, CardTarget.Self), new CardEffect(CardEffectType.Draw, 1, CardTarget.Self))),
                        },
                        new CardEffect(CardEffectType.Shield, 12, CardTarget.Self))),
                new CardUpgradeOption(
                    "rock_wall_counter_1",
                    "岩壁·震",
                    "获得 8 护盾，本回合受到攻击时对攻击者造成 4 伤害。",
                    new CardDefinition(
                        "rock_wall_counter_1",
                        "岩壁·震",
                        1,
                        new[]
                        {
                            new CardUpgradeOption(
                                "rock_wall_counter_2_damage",
                                "震山壁",
                                "获得 8 护盾，本回合受到攻击时对攻击者造成 8 伤害。",
                                new CardDefinition("rock_wall_counter_2_damage", "震山壁", 1, new CardEffect(CardEffectType.Shield, 8, CardTarget.Self), new CardEffect(CardEffectType.AttackCounter, 8, CardTarget.Self))),
                            new CardUpgradeOption(
                                "rock_wall_counter_2_chance",
                                "碎岩壁",
                                "获得 8 护盾，60% 概率在受击时对攻击者造成 10 伤害。",
                                new CardDefinition("rock_wall_counter_2_chance", "碎岩壁", 1, new CardEffect(CardEffectType.Shield, 8, CardTarget.Self), new CardEffect(CardEffectType.AttackCounter, 10, CardTarget.Self, chancePercent: 60))),
                        },
                        new CardEffect(CardEffectType.Shield, 8, CardTarget.Self),
                        new CardEffect(CardEffectType.AttackCounter, 4, CardTarget.Self))),
            },
            new CardEffect(CardEffectType.Shield, 8, CardTarget.Self));

        public static CardDefinition StoneSkinArt { get; } = new CardDefinition(
            "stone_skin_art",
            "石皮术",
            1,
            new[]
            {
                new CardUpgradeOption(
                    "stone_skin_art_guard_1",
                    "石皮术·固",
                    "获得 7 护盾，本回合受到攻击时对攻击者造成 3 伤害。",
                    new CardDefinition(
                        "stone_skin_art_guard_1",
                        "石皮术·固",
                        1,
                        new[]
                        {
                            new CardUpgradeOption(
                                "stone_skin_art_guard_2_heavy",
                                "玄皮术",
                                "获得 10 护盾，本回合受到攻击时对攻击者造成 3 伤害。",
                                new CardDefinition("stone_skin_art_guard_2_heavy", "玄皮术", 1, new CardEffect(CardEffectType.Shield, 10, CardTarget.Self), new CardEffect(CardEffectType.AttackCounter, 3, CardTarget.Self))),
                            new CardUpgradeOption(
                                "stone_skin_art_guard_2_counter",
                                "反震石皮",
                                "获得 7 护盾，本回合受到攻击时对攻击者造成 6 伤害。",
                                new CardDefinition("stone_skin_art_guard_2_counter", "反震石皮", 1, new CardEffect(CardEffectType.Shield, 7, CardTarget.Self), new CardEffect(CardEffectType.AttackCounter, 6, CardTarget.Self))),
                        },
                        new CardEffect(CardEffectType.Shield, 7, CardTarget.Self),
                        new CardEffect(CardEffectType.AttackCounter, 3, CardTarget.Self))),
                new CardUpgradeOption(
                    "stone_skin_art_cycle_1",
                    "石皮术·转",
                    "获得 4 护盾，抽 1 张牌。",
                    new CardDefinition(
                        "stone_skin_art_cycle_1",
                        "石皮术·转",
                        1,
                        new[]
                        {
                            new CardUpgradeOption(
                                "stone_skin_art_cycle_2_cost",
                                "轻石皮",
                                "灵力消耗降为 0，获得 4 护盾，抽 1 张牌。",
                                new CardDefinition("stone_skin_art_cycle_2_cost", "轻石皮", 0, new CardEffect(CardEffectType.Shield, 4, CardTarget.Self), new CardEffect(CardEffectType.Draw, 1, CardTarget.Self))),
                            new CardUpgradeOption(
                                "stone_skin_art_cycle_2_draw",
                                "流石皮",
                                "获得 4 护盾，抽 2 张牌。",
                                new CardDefinition("stone_skin_art_cycle_2_draw", "流石皮", 1, new CardEffect(CardEffectType.Shield, 4, CardTarget.Self), new CardEffect(CardEffectType.Draw, 2, CardTarget.Self))),
                        },
                        new CardEffect(CardEffectType.Shield, 4, CardTarget.Self),
                        new CardEffect(CardEffectType.Draw, 1, CardTarget.Self))),
            },
            new CardEffect(CardEffectType.Shield, 4, CardTarget.Self),
            new CardEffect(CardEffectType.AttackCounter, 3, CardTarget.Self, chancePercent: 50));

        public static CardDefinition EarthEscape { get; } = new CardDefinition(
            "earth_escape",
            "岩遁",
            0,
            new[]
            {
                new CardUpgradeOption(
                    "earth_escape_draw_1",
                    "岩遁·行",
                    "获得 4 护盾，抽 1 张牌。",
                    new CardDefinition(
                        "earth_escape_draw_1",
                        "岩遁·行",
                        0,
                        new[]
                        {
                            new CardUpgradeOption(
                                "earth_escape_draw_2_more",
                                "穿岩遁",
                                "获得 4 护盾，抽 2 张牌。",
                                new CardDefinition("earth_escape_draw_2_more", "穿岩遁", 0, new CardEffect(CardEffectType.Shield, 4, CardTarget.Self), new CardEffect(CardEffectType.Draw, 2, CardTarget.Self))),
                            new CardUpgradeOption(
                                "earth_escape_draw_2_counter",
                                "反震岩遁",
                                "获得 4 护盾，抽 1 张牌，本回合受到攻击时对攻击者造成 3 伤害。",
                                new CardDefinition("earth_escape_draw_2_counter", "反震岩遁", 0, new CardEffect(CardEffectType.Shield, 4, CardTarget.Self), new CardEffect(CardEffectType.Draw, 1, CardTarget.Self), new CardEffect(CardEffectType.AttackCounter, 3, CardTarget.Self))),
                        },
                        new CardEffect(CardEffectType.Shield, 4, CardTarget.Self),
                        new CardEffect(CardEffectType.Draw, 1, CardTarget.Self))),
                new CardUpgradeOption(
                    "earth_escape_guard_1",
                    "岩遁·守",
                    "获得 8 护盾。",
                    new CardDefinition(
                        "earth_escape_guard_1",
                        "岩遁·守",
                        0,
                        new[]
                        {
                            new CardUpgradeOption(
                                "earth_escape_guard_2_heavy",
                                "厚土遁",
                                "获得 11 护盾。",
                                new CardDefinition("earth_escape_guard_2_heavy", "厚土遁", 0, new CardEffect(CardEffectType.Shield, 11, CardTarget.Self))),
                            new CardUpgradeOption(
                                "earth_escape_guard_2_draw",
                                "流土遁",
                                "获得 7 护盾，抽 1 张牌。",
                                new CardDefinition("earth_escape_guard_2_draw", "流土遁", 0, new CardEffect(CardEffectType.Shield, 7, CardTarget.Self), new CardEffect(CardEffectType.Draw, 1, CardTarget.Self))),
                        },
                        new CardEffect(CardEffectType.Shield, 8, CardTarget.Self))),
            },
            new CardEffect(CardEffectType.Shield, 3, CardTarget.Self),
            new CardEffect(CardEffectType.Draw, 1, CardTarget.Self));

        public static CardDefinition CounterSlash { get; } = new CardDefinition(
            "counter_slash",
            "反击斩",
            1,
            new[]
            {
                new CardUpgradeOption(
                    "counter_slash_counter_1",
                    "反击斩·烈",
                    "造成 5 伤害，本回合受到攻击时对攻击者造成 8 伤害。",
                    new CardDefinition(
                        "counter_slash_counter_1",
                        "反击斩·烈",
                        1,
                        new[]
                        {
                            new CardUpgradeOption(
                                "counter_slash_counter_2_damage",
                                "震返斩",
                                "造成 7 伤害，本回合受到攻击时对攻击者造成 10 伤害。",
                                new CardDefinition("counter_slash_counter_2_damage", "震返斩", 1, new CardEffect(CardEffectType.Damage, 7), new CardEffect(CardEffectType.AttackCounter, 10, CardTarget.Self))),
                            new CardUpgradeOption(
                                "counter_slash_counter_2_guard",
                                "守返斩",
                                "造成 5 伤害，获得 6 护盾，本回合受到攻击时对攻击者造成 8 伤害。",
                                new CardDefinition("counter_slash_counter_2_guard", "守返斩", 1, new CardEffect(CardEffectType.Damage, 5), new CardEffect(CardEffectType.Shield, 6, CardTarget.Self), new CardEffect(CardEffectType.AttackCounter, 8, CardTarget.Self))),
                        },
                        new CardEffect(CardEffectType.Damage, 5),
                        new CardEffect(CardEffectType.AttackCounter, 8, CardTarget.Self))),
                new CardUpgradeOption(
                    "counter_slash_guard_1",
                    "反击斩·守",
                    "获得 6 护盾，本回合受到攻击时对攻击者造成 6 伤害。",
                    new CardDefinition(
                        "counter_slash_guard_1",
                        "反击斩·守",
                        1,
                        new[]
                        {
                            new CardUpgradeOption(
                                "counter_slash_guard_2_cost",
                                "轻身返斩",
                                "灵力消耗降为 0，获得 5 护盾，本回合受到攻击时对攻击者造成 5 伤害。",
                                new CardDefinition("counter_slash_guard_2_cost", "轻身返斩", 0, new CardEffect(CardEffectType.Shield, 5, CardTarget.Self), new CardEffect(CardEffectType.AttackCounter, 5, CardTarget.Self))),
                            new CardUpgradeOption(
                                "counter_slash_guard_2_heavy",
                                "厚土返斩",
                                "获得 9 护盾，本回合受到攻击时对攻击者造成 6 伤害。",
                                new CardDefinition("counter_slash_guard_2_heavy", "厚土返斩", 1, new CardEffect(CardEffectType.Shield, 9, CardTarget.Self), new CardEffect(CardEffectType.AttackCounter, 6, CardTarget.Self))),
                        },
                        new CardEffect(CardEffectType.Shield, 6, CardTarget.Self),
                        new CardEffect(CardEffectType.AttackCounter, 6, CardTarget.Self))),
            },
            new CardEffect(CardEffectType.Damage, 4),
            new CardEffect(CardEffectType.AttackCounter, 5, CardTarget.Self));

        public static PillDefinition SmallRestorePillItem { get; } = new PillDefinition(
            "small_restore_pill",
            "小还丹",
            "战斗内消耗品：恢复 10 HP。",
            effectValue: 10);

        public static PillDefinition BigRestorePillItem { get; } = new PillDefinition(
            "big_restore_pill",
            "大还丹",
            "战斗内消耗品：恢复 15 HP。",
            effectValue: 15);

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

        public static PillDefinition FoundationPillItem { get; } = new PillDefinition(
            "foundation_pill",
            "筑基丹",
            "Run 内消耗品：永久增加最大 HP 10。",
            PillEffectType.MaxHp,
            10);

        public static ArtifactDefinition SpiritStoneMineArtifact { get; } = new ArtifactDefinition(
            "spirit_stone_mine",
            "灵石矿",
            "法宝：每场战斗胜利额外获得 5 灵石。",
            ArtifactEffectType.BonusSpiritStonesOnVictory,
            5);

        public static ArtifactDefinition RejuvenationJadeArtifact { get; } = new ArtifactDefinition(
            "rejuvenation_jade",
            "回春玉佩",
            "法宝：每场战斗结束后恢复 3 HP。",
            ArtifactEffectType.HealAfterVictory,
            3);

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

        public static IReadOnlyList<CardDefinition> CreateFireCloudSectStarterDeck()
        {
            return new List<CardDefinition>
            {
                BurningPalm, BurningPalm, BurningPalm,
                FlameFormula, FlameFormula, FlameFormula,
                GuardQi, GuardQi,
                FireCloudStep, FireCloudStep,
                HealingPill, HealingPill,
            };
        }

        public static IReadOnlyList<CardDefinition> CreateThunderSectStarterDeck()
        {
            return new List<CardDefinition>
            {
                ThunderTalisman, ThunderTalisman, ThunderTalisman,
                HeavenlyThunderSpell, HeavenlyThunderSpell,
                ThunderTalisman, ThunderTalisman,
                GuardQi, GuardQi,
                ThunderEscape,
                LightBody,
                HealingPill,
            };
        }

        public static IReadOnlyList<CardDefinition> CreateEarthSectStarterDeck()
        {
            return new List<CardDefinition>
            {
                RockStrike, RockStrike, RockStrike,
                EarthSplittingPalm, EarthSplittingPalm,
                RockWall, RockWall, RockWall,
                StoneSkinArt,
                EarthEscape,
                CounterSlash,
                HealingPill,
            };
        }

        public static IReadOnlyList<CardDefinition> CreateStarterDeck(CultivationSect sect)
        {
            switch (sect)
            {
                case CultivationSect.Sword:
                    return CreateSwordSectStarterDeck();
                case CultivationSect.FireCloud:
                    return CreateFireCloudSectStarterDeck();
                case CultivationSect.Thunder:
                    return CreateThunderSectStarterDeck();
                case CultivationSect.Earth:
                    return CreateEarthSectStarterDeck();
                default:
                    throw new ArgumentOutOfRangeException(nameof(sect), sect, "Unsupported cultivation sect.");
            }
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

        public static EnemyDefinition FoundationSwordCultivator { get; } = new EnemyDefinition(
            "foundation_sword_cultivator",
            "筑基剑修",
            82,
            4,
            new EnemyIntent(EnemyIntentType.Attack, 10, description: "御剑攻击 10"),
            new EnemyIntent(EnemyIntentType.Defend, 12, description: "剑阵护体 12"),
            new EnemyIntent(EnemyIntentType.Attack, 14, description: "飞剑连斩 14"));

        public static EnemyDefinition FrostSerpentDemon { get; } = new EnemyDefinition(
            "frost_serpent_demon",
            "冰霜蛇妖",
            55,
            2,
            new EnemyIntent(EnemyIntentType.AttackAndFreeze, 5, 1, "寒冰吐息 5 + 冰冻 1"),
            new EnemyIntent(EnemyIntentType.Attack, 8, description: "尾击 8"),
            new EnemyIntent(EnemyIntentType.AttackAndFreeze, 5, 1, "寒冰吐息 5 + 冰冻 1"));

        public static EnemyDefinition WindSpiritBird { get; } = new EnemyDefinition(
            "wind_spirit_bird",
            "风灵鸟",
            58,
            1,
            new EnemyIntent(EnemyIntentType.Attack, 6, description: "俯冲 6"),
            new EnemyIntent(EnemyIntentType.Defend, 8, description: "疾风护身 8"),
            new EnemyIntent(EnemyIntentType.Sweep, 4, description: "旋风 4x3"));

        public static EnemyDefinition DualHeadIceFireSerpent { get; } = new EnemyDefinition(
            "dual_head_ice_fire_serpent",
            "双头冰火蟒",
            100,
            3,
            new EnemyIntent(EnemyIntentType.AttackAndFreeze, 4, 1, "冰息 4 + 冰冻 1"),
            new EnemyIntent(EnemyIntentType.AttackAndBurn, 4, 1, "火息 4 + 灼烧 1"),
            new EnemyIntent(EnemyIntentType.Attack, 10, description: "冰火扑咬 10"),
            new EnemyIntent(EnemyIntentType.Defend, 12, description: "鳞甲护体 12"));

        public static EnemyDefinition GoldenCoreDemonicCultivator { get; } = new EnemyDefinition(
            "golden_core_demonic_cultivator",
            "金丹魔修",
            95,
            4,
            new EnemyIntent(EnemyIntentType.Attack, 14, description: "灵品功法 14"),
            new EnemyIntent(EnemyIntentType.Heal, 12, description: "吞丹调息 回复 12"),
            new EnemyIntent(EnemyIntentType.BuffAttack, 6, 8, "金丹之力 攻击+6 护盾8"),
            new EnemyIntent(EnemyIntentType.Sweep, 10, description: "金丹连击 10x2"));

        public static GoldenCorePassiveDefinition SwordHeartPassive { get; } = new GoldenCorePassiveDefinition(
            "golden_core_sword_heart",
            "剑心成丹",
            "每场战斗开始获得锋锐 2，剑修爆发更稳定。",
            GoldenCorePassiveType.SwordHeart,
            2);

        public static GoldenCorePassiveDefinition FlowingWaterPassive { get; } = new GoldenCorePassiveDefinition(
            "golden_core_flowing_water",
            "流水之势",
            "每回合额外抽 1 张牌，控制和回复流更顺滑。",
            GoldenCorePassiveType.FlowingWater,
            1);

        public static GoldenCorePassiveDefinition ThunderSeedPassive { get; } = new GoldenCorePassiveDefinition(
            "golden_core_thunder_seed",
            "雷种入体",
            "本场战斗功法灵力消耗 -1，预览天雷阁高方差节奏。",
            GoldenCorePassiveType.ThunderSeed,
            1);

        public static IReadOnlyList<GoldenCorePassiveDefinition> CreateGoldenCorePassiveChoices()
        {
            return new List<GoldenCorePassiveDefinition>
            {
                SwordHeartPassive,
                FlowingWaterPassive,
                ThunderSeedPassive,
            };
        }

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

        public static IReadOnlyList<CultivationRunReward> CreateFireCloudSectRewardPool()
        {
            return new List<CultivationRunReward>
            {
                new CultivationRunReward("reward_burning_palm", BurningPalm),
                new CultivationRunReward("reward_flame_formula", FlameFormula),
                new CultivationRunReward("reward_fire_cloud_step", FireCloudStep),
                new CultivationRunReward("reward_guard_qi", GuardQi),
                new CultivationRunReward("reward_healing_pill", HealingPill),
            };
        }

        public static IReadOnlyList<CultivationRunReward> CreateThunderSectRewardPool()
        {
            return new List<CultivationRunReward>
            {
                new CultivationRunReward("reward_thunder_talisman", ThunderTalisman),
                new CultivationRunReward("reward_heavenly_thunder_spell", HeavenlyThunderSpell),
                new CultivationRunReward("reward_lightning_chain", LightningChain),
                new CultivationRunReward("reward_thunder_charge", ThunderCharge),
                new CultivationRunReward("reward_thunder_escape", ThunderEscape),
                new CultivationRunReward("reward_five_thunder_orthodoxy", FiveThunderOrthodoxy),
                new CultivationRunReward("reward_thunderous_barrage", ThunderousBarrage),
                new CultivationRunReward("reward_thunder_hammer", ThunderHammer),
                new CultivationRunReward("reward_thunder_dodge_strike", ThunderDodgeStrike),
                new CultivationRunReward("reward_thunder_shield", ThunderShield),
                new CultivationRunReward("reward_thunder_strike_rebound", ThunderStrikeRebound),
                new CultivationRunReward("reward_guard_qi", GuardQi),
                new CultivationRunReward("reward_light_body", LightBody),
                new CultivationRunReward("reward_healing_pill", HealingPill),
            };
        }

        public static IReadOnlyList<CultivationRunReward> CreateEarthSectRewardPool()
        {
            return new List<CultivationRunReward>
            {
                new CultivationRunReward("reward_rock_strike", RockStrike),
                new CultivationRunReward("reward_earth_splitting_palm", EarthSplittingPalm),
                new CultivationRunReward("reward_rock_wall", RockWall),
                new CultivationRunReward("reward_stone_skin_art", StoneSkinArt),
                new CultivationRunReward("reward_earth_escape", EarthEscape),
                new CultivationRunReward("reward_counter_slash", CounterSlash),
                new CultivationRunReward("reward_guard_qi", GuardQi),
                new CultivationRunReward("reward_light_body", LightBody),
                new CultivationRunReward("reward_healing_pill", HealingPill),
            };
        }

        public static IReadOnlyList<CultivationRunReward> CreateRewardPool(CultivationSect sect)
        {
            switch (sect)
            {
                case CultivationSect.Sword:
                    return CreateSwordSectRewardPool();
                case CultivationSect.FireCloud:
                    return CreateFireCloudSectRewardPool();
                case CultivationSect.Thunder:
                    return CreateThunderSectRewardPool();
                case CultivationSect.Earth:
                    return CreateEarthSectRewardPool();
                default:
                    throw new ArgumentOutOfRangeException(nameof(sect), sect, "Unsupported cultivation sect.");
            }
        }

        public static IReadOnlyList<CultivationMarketItem> CreatePrototypeMarketItems()
        {
            return new List<CultivationMarketItem>
            {
                new CultivationMarketItem("market_cloud_guard", CloudGuard, 20),
                new CultivationMarketItem("market_thrust", Thrust, 25),
                new CultivationMarketItem("market_small_restore_pill", SmallRestorePillItem, 15),
                new CultivationMarketItem("market_big_restore_pill", BigRestorePillItem, 35),
                new CultivationMarketItem("market_spirit_boost_pill", SpiritBoostPillItem, 35),
                new CultivationMarketItem("market_cleanse_pill", CleansePillItem, 15),
                new CultivationMarketItem("market_breakthrough_pill", BreakthroughPillItem, 90),
                new CultivationMarketItem("market_foundation_pill", FoundationPillItem, 70),
                new CultivationMarketItem("market_spirit_stone_mine", SpiritStoneMineArtifact, 25),
                new CultivationMarketItem("market_rejuvenation_jade", RejuvenationJadeArtifact, 30),
            };
        }

        public static IReadOnlyList<ArtifactDefinition> CreatePrototypeArtifactRewardPool()
        {
            return new List<ArtifactDefinition>
            {
                SpiritStoneMineArtifact,
                RejuvenationJadeArtifact,
            };
        }

        public static MysticEventDefinition SpiritSpringMysticEvent { get; } = new MysticEventDefinition(
            "mystic_spirit_spring",
            "灵泉",
            "一眼灵气充沛的泉水，可以短暂休整，也可以收集泉水获得丹药。",
            new MysticEventOption(
                "drink_spirit_spring",
                "饮用灵泉",
                "恢复 30 HP。",
                MysticEventEffectType.Heal,
                30),
            new MysticEventOption(
                "collect_small_restore_pill",
                "收集泉水",
                "获得 1 颗小还丹。",
                MysticEventEffectType.GainPill,
                pillReward: SmallRestorePillItem),
            new MysticEventOption(
                "leave_spirit_spring",
                "离开",
                "不冒险，直接离开。",
                MysticEventEffectType.Leave));

        public static IReadOnlyList<CultivationRunNode> CreateFirstPrototypeRoute(CultivationSect sect = CultivationSect.Sword)
        {
            var rewards = CreateRewardPool(sect);
            var artifactRewards = CreatePrototypeArtifactRewardPool();
            return new List<CultivationRunNode>
            {
                new CultivationRunNode("node_stone_demon", "山门石魔", CultivationRunNodeType.Battle, StoneDemon, rewards, spiritStoneReward: 15),
                new CultivationRunNode("node_fire_bat", "火蝠洞", CultivationRunNodeType.Battle, FireBat, rewards, spiritStoneReward: 15),
                new CultivationRunNode("node_meditation", "闭关调息", CultivationRunNodeType.Rest, null, null, restHealAmount: 30),
                new CultivationRunNode("node_stone_demon_leader", "石魔首领", CultivationRunNodeType.Elite, StoneDemonLeader, rewards, spiritStoneReward: 35, artifactRewardPool: artifactRewards),
            };
        }

        public static IReadOnlyList<CultivationRunNode> CreateFirstPrototypeBranchingRoute(CultivationSect sect = CultivationSect.Sword)
        {
            var rewards = CreateRewardPool(sect);
            var marketItems = CreatePrototypeMarketItems();
            var artifactRewards = CreatePrototypeArtifactRewardPool();
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
                    nextNodeIndices: new[] { 3, 4, 5, 6 }),
                new CultivationRunNode(
                    "node_artifact_chest",
                    "遗迹宝箱",
                    CultivationRunNodeType.Chest,
                    null,
                    null,
                    nextNodeIndices: new[] { 6 },
                    artifactRewardPool: artifactRewards),
                new CultivationRunNode(
                    "node_market",
                    "山脚坊市",
                    CultivationRunNodeType.Market,
                    null,
                    null,
                    nextNodeIndices: new[] { 6 },
                    marketItems: marketItems),
                new CultivationRunNode(
                    "node_spirit_spring",
                    "灵泉秘境",
                    CultivationRunNodeType.Mystic,
                    null,
                    null,
                    nextNodeIndices: new[] { 6 },
                    mysticEvent: SpiritSpringMysticEvent),
                new CultivationRunNode(
                    "node_stone_demon_leader",
                    "石魔首领",
                    CultivationRunNodeType.Elite,
                    StoneDemonLeader,
                    rewards,
                    nextNodeIndices: new[] { 7 },
                    spiritStoneReward: 35,
                    artifactRewardPool: artifactRewards),
                new CultivationRunNode(
                    "node_foundation_sword_cultivator",
                    "筑基剑修",
                    CultivationRunNodeType.Battle,
                    FoundationSwordCultivator,
                    rewards,
                    nextNodeIndices: new[] { 8, 9 },
                    spiritStoneReward: 25,
                    realm: CultivationRealm.Foundation),
                new CultivationRunNode(
                    "node_frost_serpent_demon",
                    "冰霜蛇妖",
                    CultivationRunNodeType.Battle,
                    FrostSerpentDemon,
                    rewards,
                    nextNodeIndices: new[] { 10 },
                    spiritStoneReward: 20,
                    realm: CultivationRealm.Foundation),
                new CultivationRunNode(
                    "node_foundation_meditation",
                    "筑基闭关",
                    CultivationRunNodeType.Rest,
                    null,
                    null,
                    restHealAmount: 35,
                    nextNodeIndices: new[] { 10 },
                    realm: CultivationRealm.Foundation),
                new CultivationRunNode(
                    "node_wind_spirit_bird",
                    "风灵鸟",
                    CultivationRunNodeType.Battle,
                    WindSpiritBird,
                    rewards,
                    nextNodeIndices: new[] { 11 },
                    spiritStoneReward: 20,
                    realm: CultivationRealm.Foundation),
                new CultivationRunNode(
                    "node_dual_head_ice_fire_serpent",
                    "双头冰火蟒",
                    CultivationRunNodeType.Elite,
                    DualHeadIceFireSerpent,
                    rewards,
                    nextNodeIndices: new[] { 12 },
                    spiritStoneReward: 35,
                    artifactRewardPool: artifactRewards,
                    realm: CultivationRealm.Foundation),
                new CultivationRunNode(
                    "node_golden_core_demonic_cultivator",
                    "金丹魔修",
                    CultivationRunNodeType.Battle,
                    GoldenCoreDemonicCultivator,
                    rewards,
                    spiritStoneReward: 28,
                    realm: CultivationRealm.GoldenCore),
            };
        }
    }
}
