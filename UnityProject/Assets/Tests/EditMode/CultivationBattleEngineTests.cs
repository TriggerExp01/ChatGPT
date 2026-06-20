using System.Linq;
using GameLogic.Cultivation;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic.Tests
{
    public sealed class CultivationBattleEngineTests
    {
        [Test]
        public void StartBattleDrawsToHandLimitAndShowsEnemyIntent()
        {
            var state = CreateStoneBattle();

            Assert.AreEqual(5, state.Hand.Count);
            Assert.AreEqual(3, state.Spirit);
            Assert.AreEqual(EnemyIntentType.Attack, state.Enemies[0].CurrentIntent.Type);
            Assert.AreEqual(6, state.Enemies[0].CurrentIntent.Value);
        }

        [Test]
        public void PlayingCardConsumesSpiritAndDealsDamage()
        {
            var engine = new BattleEngine(1);
            var state = CreateOrderedBattle(engine, CultivationSeedData.SwordQi);
            var enemy = state.Enemies[0];

            engine.PlayCard(state, state.Hand[0], enemy);

            Assert.AreEqual(2, state.Spirit);
            Assert.AreEqual(34, enemy.Body.CurrentHp);
            Assert.AreEqual(1, state.DiscardPile.Count);
        }

        [Test]
        public void BattleStateEffectiveCostUsesBattleWideReductionAndNeverBelowZero()
        {
            var state = CreateStoneBattle();
            var twoCostCard = new CardDefinition("two_cost_test", "二费测试", 2, new CardEffect(CardEffectType.Damage, 1));
            var zeroCostCard = new CardDefinition("zero_cost_test", "零费测试", 0, new CardEffect(CardEffectType.Damage, 1));

            state.AddSpiritCostReduction(1);

            Assert.AreEqual(1, state.GetEffectiveSpiritCost(twoCostCard));
            Assert.AreEqual(0, state.GetEffectiveSpiritCost(zeroCostCard));
        }

        [Test]
        public void DefenseCardAddsShieldAndEnemyAttackConsumesIt()
        {
            var engine = new BattleEngine(1);
            var state = CreateOrderedBattle(engine, CultivationSeedData.GuardQi);

            engine.PlayCard(state, state.Hand[0], state.Enemies[0]);
            Assert.AreEqual(8, state.Player.Shield);

            engine.EndPlayerTurn(state);

            Assert.AreEqual(100, state.Player.CurrentHp);
            Assert.AreEqual(0, state.Player.Shield);
        }

        [Test]
        public void BreakDefenseAmplifiesFollowingDamage()
        {
            var engine = new BattleEngine(1);
            var state = CreateOrderedBattle(engine, CultivationSeedData.BreakArmor, CultivationSeedData.SwordQi);
            var enemy = state.Enemies[0];

            engine.PlayCard(state, state.Hand[0], enemy);
            engine.PlayCard(state, state.Hand[0], enemy);

            Assert.AreEqual(1, enemy.Body.BreakDefenseStacks);
            Assert.AreEqual(31, enemy.Body.CurrentHp);
        }

        [Test]
        public void BurnDealsDamageAtTurnStart()
        {
            var engine = new BattleEngine(1);
            var burnCard = new CardDefinition(
                "burn_test",
                "灼烧测试",
                0,
                new CardEffect(CardEffectType.Burn, 2, CardTarget.EnemySingle, 2));
            var state = CreateOrderedBattle(engine, burnCard, CultivationSeedData.GuardQi, CultivationSeedData.GuardQi);
            var enemy = state.Enemies[0];

            engine.PlayCard(state, state.Hand[0], enemy);
            engine.EndPlayerTurn(state);

            Assert.AreEqual(38, enemy.Body.CurrentHp);
            Assert.AreEqual(1, enemy.Body.BurnTurns);
        }

        [Test]
        public void FireCloudSectStarterDeckUsesBurnAndCyclingCards()
        {
            var deck = CultivationSeedData.CreateFireCloudSectStarterDeck();

            Assert.AreEqual(12, deck.Count);
            Assert.AreEqual(3, deck.Count(card => card.Id == "burning_palm"));
            Assert.AreEqual(3, deck.Count(card => card.Id == "flame_formula"));
            Assert.AreEqual(2, deck.Count(card => card.Id == "fire_cloud_step"));
            Assert.AreEqual(2, deck.Count(card => card.Id == "guard_qi"));
            Assert.AreEqual(2, deck.Count(card => card.Id == "healing_pill"));
            Assert.IsTrue(deck.Any(card => card.Effects.Any(effect => effect.Type == CardEffectType.Burn)));
            Assert.IsTrue(deck.Any(card => card.Effects.Any(effect => effect.Type == CardEffectType.Draw)));
        }

        [Test]
        public void FireCloudSectCardsCanApplyAreaBurnInBattle()
        {
            var engine = new BattleEngine(1);
            var state = new BattleState(
                new CombatantState("修士", 100),
                CultivationSeedData.CreateFireCloudSectStarterDeck(),
                new[]
                {
                    new EnemyState(CultivationSeedData.StoneDemon),
                    new EnemyState(CultivationSeedData.FireBat),
                });
            engine.StartPlayerTurn(state);

            engine.PlayCard(state, state.Hand.First(card => card.Id == "flame_formula"));

            Assert.AreEqual(2, state.Spirit);
            Assert.AreEqual(2, state.Enemies[0].Body.BurnStacks);
            Assert.AreEqual(2, state.Enemies[1].Body.BurnStacks);

            engine.StartPlayerTurn(state);

            Assert.AreEqual(38, state.Enemies[0].Body.CurrentHp);
            Assert.AreEqual(30, state.Enemies[1].Body.CurrentHp);
        }

        [Test]
        public void PoisonStacksDealDamageAtTurnStartWithoutDecay()
        {
            var engine = new BattleEngine(1);
            var poisonCard = new CardDefinition(
                "poison_test",
                "中毒测试",
                0,
                new CardEffect(CardEffectType.Poison, 3));
            var state = CreateOrderedBattle(engine, poisonCard, CultivationSeedData.GuardQi, CultivationSeedData.GuardQi);
            var enemy = state.Enemies[0];

            engine.PlayCard(state, state.Hand[0], enemy);
            engine.EndPlayerTurn(state);

            Assert.AreEqual(37, enemy.Body.CurrentHp);
            Assert.AreEqual(3, enemy.Body.PoisonStacks);
            Assert.IsTrue(state.PoisonDamageTriggeredThisTurn);

            engine.EndPlayerTurn(state);

            Assert.AreEqual(34, enemy.Body.CurrentHp);
            Assert.AreEqual(3, enemy.Body.PoisonStacks);
        }

        [Test]
        public void PoisonBurstConsumesPoisonAndDealsScaledDamage()
        {
            var engine = new BattleEngine(1);
            var poisonCard = new CardDefinition(
                "poison_setup_test",
                "毒爆准备",
                0,
                new CardEffect(CardEffectType.Poison, 4));
            var burstCard = new CardDefinition(
                "poison_burst_test",
                "毒爆测试",
                0,
                new CardEffect(CardEffectType.PoisonBurst, 3, secondaryValue: 1));
            var state = CreateOrderedBattle(engine, poisonCard, burstCard);
            var enemy = state.Enemies[0];

            engine.PlayCard(state, state.Hand[0], enemy);
            engine.PlayCard(state, state.Hand[0], enemy);

            Assert.AreEqual(28, enemy.Body.CurrentHp);
            Assert.AreEqual(1, enemy.Body.PoisonStacks);
        }

        [Test]
        public void RegenerationHealsAtPlayerTurnStart()
        {
            var engine = new BattleEngine(1);
            var regenCard = new CardDefinition(
                "regen_test",
                "生生不息测试",
                0,
                new CardEffect(CardEffectType.Regeneration, 4, CardTarget.Self, duration: 2));
            var state = engine.CreateBattle(new[] { regenCard }.Concat(CultivationSeedData.CreateSwordSectStarterDeck()), CultivationSeedData.StoneDemon, 80);
            state.Hand.Clear();
            state.DrawPile.Remove(regenCard);
            state.Hand.Add(regenCard);

            engine.PlayCard(state, state.Hand[0], state.Enemies[0]);
            engine.EndPlayerTurn(state);

            Assert.AreEqual(78, state.Player.CurrentHp);
            Assert.AreEqual(4, state.RegenerationPerTurn);
            Assert.AreEqual(1, state.RegenerationTurns);
        }

        [Test]
        public void LeechDamageHealsFromDamageDealt()
        {
            var engine = new BattleEngine(1);
            var leechCard = new CardDefinition(
                "leech_test",
                "吸灵测试",
                0,
                new CardEffect(CardEffectType.Leech, 6, secondaryValue: 50));
            var state = engine.CreateBattle(new[] { leechCard }.Concat(CultivationSeedData.CreateSwordSectStarterDeck()), CultivationSeedData.StoneDemon, 80);
            state.Hand.Clear();
            state.DrawPile.Remove(leechCard);
            state.Hand.Add(leechCard);

            engine.PlayCard(state, state.Hand[0], state.Enemies[0]);

            Assert.AreEqual(36, state.Enemies[0].Body.CurrentHp);
            Assert.AreEqual(82, state.Player.CurrentHp);
        }

        [Test]
        public void LeechDoesNotHealWhenDefenseBlocksAllDamage()
        {
            var engine = new BattleEngine(1);
            var leechCard = new CardDefinition(
                "leech_blocked_test",
                "吸灵格挡测试",
                0,
                new CardEffect(CardEffectType.Leech, 4, secondaryValue: 100));
            var shieldedEnemy = new EnemyDefinition(
                "armored_dummy",
                "重甲傀儡",
                20,
                8,
                new EnemyIntent(EnemyIntentType.Defend, 0));
            var state = engine.CreateBattle(new[] { leechCard }.Concat(CultivationSeedData.CreateSwordSectStarterDeck()), shieldedEnemy, 80);
            state.Hand.Clear();
            state.DrawPile.Remove(leechCard);
            state.Hand.Add(leechCard);

            engine.PlayCard(state, state.Hand[0], state.Enemies[0]);

            Assert.AreEqual(20, state.Enemies[0].Body.CurrentHp);
            Assert.AreEqual(80, state.Player.CurrentHp);
        }

        [Test]
        public void PoisonCounterAppliesPoisonWhenEnemyAttackHits()
        {
            var engine = new BattleEngine(1);
            var poisonCounterCard = new CardDefinition(
                "poison_counter_test",
                "毒瘴护体测试",
                0,
                new CardEffect(CardEffectType.Shield, 8, CardTarget.Self),
                new CardEffect(CardEffectType.PoisonAttackCounter, 2, CardTarget.Self, duration: 1));
            var state = CreateOrderedBattle(engine, poisonCounterCard);

            engine.PlayCard(state, state.Hand[0], state.Enemies[0]);
            engine.EndPlayerTurn(state);

            Assert.AreEqual(100, state.Player.CurrentHp);
            Assert.AreEqual(2, state.Enemies[0].Body.PoisonStacks);
            Assert.IsTrue(state.Logs.Any(log => log.Message.Contains("毒瘴反噬")));
        }

        [Test]
        public void BloodSacrificeKeepsPlayerAliveAndStillResolvesFollowingDamage()
        {
            var engine = new BattleEngine(1);
            var bloodCard = new CardDefinition(
                "blood_sacrifice_test",
                "血祭测试",
                0,
                new CardEffect(CardEffectType.BloodSacrifice, 5, CardTarget.Self),
                new CardEffect(CardEffectType.Damage, 12));
            var state = CreateOrderedBattleWithPlayerHp(engine, 2, bloodCard);
            var enemy = state.Enemies[0];

            engine.PlayCard(state, state.Hand[0], enemy);

            Assert.AreEqual(1, state.Player.CurrentHp);
            Assert.AreEqual(30, enemy.Body.CurrentHp);
            Assert.AreEqual(BattleOutcome.InProgress, state.Outcome);
        }

        [Test]
        public void LowHpDamageAddsBonusWhenPlayerIsAtThreshold()
        {
            var engine = new BattleEngine(1);
            var lowHpCard = new CardDefinition(
                "low_hp_damage_test",
                "低血伤害测试",
                0,
                new CardEffect(CardEffectType.LowHpDamage, 14, chancePercent: 50, secondaryValue: 14));
            var highHpState = CreateOrderedBattleWithPlayerHp(engine, 100, lowHpCard);
            var lowHpState = CreateOrderedBattleWithPlayerHp(engine, 50, lowHpCard);

            engine.PlayCard(highHpState, highHpState.Hand[0], highHpState.Enemies[0]);
            engine.PlayCard(lowHpState, lowHpState.Hand[0], lowHpState.Enemies[0]);

            Assert.AreEqual(28, highHpState.Enemies[0].Body.CurrentHp);
            Assert.AreEqual(14, lowHpState.Enemies[0].Body.CurrentHp);
        }

        [Test]
        public void MissingHpDamageScalesByLostHpTenthSteps()
        {
            var engine = new BattleEngine(1);
            var state = CreateOrderedBattleWithPlayerHp(engine, 60, CultivationSeedData.BloodFrenzy);

            engine.PlayCard(state, state.Hand[0], state.Enemies[0]);

            Assert.AreEqual(56, state.Player.CurrentHp);
            Assert.AreEqual(28, state.Enemies[0].Body.CurrentHp);
        }

        [Test]
        public void BloodGuardHealRestoresHpAfterEnemyAttack()
        {
            var engine = new BattleEngine(1);
            var state = CreateOrderedBattleWithPlayerHp(engine, 80, CultivationSeedData.BloodLeechGuard);

            engine.PlayCard(state, state.Hand[0], state.Enemies[0]);
            engine.EndPlayerTurn(state);

            Assert.AreEqual(81, state.Player.CurrentHp);
            Assert.AreEqual(0, state.BloodGuardHealAmount);
            Assert.IsTrue(state.Logs.Any(log => log.Message.Contains("噬血护体")));
        }

        [Test]
        public void DemonicSectStarterDeckUsesBloodSacrificeLowHpAndLeechCards()
        {
            var deck = CultivationSeedData.CreateDemonicSectStarterDeck();

            Assert.AreEqual(12, deck.Count);
            Assert.AreEqual(3, deck.Count(card => card.Id == "blood_sacrifice_palm"));
            Assert.AreEqual(2, deck.Count(card => card.Id == "blood_leech_claw"));
            Assert.AreEqual(2, deck.Count(card => card.Id == "shadow_stab"));
            Assert.AreEqual(2, deck.Count(card => card.Id == "blood_flesh_shield"));
            Assert.AreEqual(1, deck.Count(card => card.Id == "shadow_escape"));
            Assert.AreEqual(1, deck.Count(card => card.Id == "blood_leech_guard"));
            Assert.AreEqual(1, deck.Count(card => card.Id == "healing_pill"));
            Assert.IsTrue(deck.Any(card => card.Effects.Any(effect => effect.Type == CardEffectType.BloodSacrifice)));
            Assert.IsTrue(deck.Any(card => card.Effects.Any(effect => effect.Type == CardEffectType.LowHpDamage)));
            Assert.IsTrue(deck.Any(card => card.Effects.Any(effect => effect.Type == CardEffectType.Leech)));
            Assert.IsTrue(deck.Any(card => card.Effects.Any(effect => effect.Type == CardEffectType.BloodGuardHeal)));
        }

        [Test]
        public void SacrificeArtConsumesHighestCostHandCardForDamage()
        {
            var engine = new BattleEngine(1);
            var fodder = new CardDefinition("sacrifice_fodder", "献祭素材", 2, new CardEffect(CardEffectType.Shield, 1, CardTarget.Self));
            var state = CreateOrderedBattle(engine, CultivationSeedData.SacrificeArt, fodder);
            var enemy = state.Enemies[0];

            engine.PlayCard(state, CultivationSeedData.SacrificeArt, enemy);

            Assert.AreEqual(26, enemy.Body.CurrentHp);
            Assert.IsFalse(state.Hand.Contains(fodder));
            Assert.IsTrue(state.ExhaustPile.Contains(fodder));
        }

        [Test]
        public void BloodSacrificeEmpowerAddsFlatDamageToFollowingAttack()
        {
            var engine = new BattleEngine(1);
            var strike = new CardDefinition("empower_strike", "强化后攻击", 0, new CardEffect(CardEffectType.Damage, 10));
            var state = CreateOrderedBattle(engine, CultivationSeedData.BloodSacrificeEmpower, strike);
            var enemy = state.Enemies[0];

            engine.PlayCard(state, CultivationSeedData.BloodSacrificeEmpower, enemy);
            engine.PlayCard(state, strike, enemy);

            Assert.AreEqual(95, state.Player.CurrentHp);
            Assert.AreEqual(27, enemy.Body.CurrentHp);
        }

        [Test]
        public void FrenzyBloodScalesLaterDamageByMissingHpSteps()
        {
            var engine = new BattleEngine(1);
            var strike = new CardDefinition("frenzy_strike", "癫狂后攻击", 0, new CardEffect(CardEffectType.Damage, 20));
            var state = CreateOrderedBattleWithPlayerHp(engine, 60, CultivationSeedData.FrenzyBlood, strike);
            var enemy = state.Enemies[0];

            engine.PlayCard(state, CultivationSeedData.FrenzyBlood, enemy);
            engine.PlayCard(state, strike, enemy);

            Assert.AreEqual(18, enemy.Body.CurrentHp);
        }

        [Test]
        public void DemonBloodBoilRetaliatesWhenBloodSacrificeLosesHp()
        {
            var engine = new BattleEngine(1);
            var state = CreateOrderedBattle(engine, CultivationSeedData.DemonBloodBoil, CultivationSeedData.BloodSacrificePalm);
            var enemy = state.Enemies[0];

            engine.PlayCard(state, CultivationSeedData.DemonBloodBoil, enemy);
            engine.PlayCard(state, CultivationSeedData.BloodSacrificePalm, enemy);

            Assert.AreEqual(29, enemy.Body.CurrentHp);
            Assert.IsTrue(state.Logs.Any(log => log.Message.Contains("魔血沸腾")));
        }

        [Test]
        public void UndyingDemonBodyTriggersDeathWardAfterLethalAttack()
        {
            var enemy = new EnemyDefinition(
                "lethal_attacker",
                "致命攻击者",
                30,
                0,
                new EnemyIntent(EnemyIntentType.Attack, 50));
            var engine = new BattleEngine(1);
            var state = engine.CreateBattle(new[] { CultivationSeedData.UndyingDemonBody }.Concat(CultivationSeedData.CreateSwordSectStarterDeck()), enemy, playerCurrentHp: 10);
            state.Hand.Clear();
            state.DrawPile.Remove(CultivationSeedData.UndyingDemonBody);
            state.Hand.Add(CultivationSeedData.UndyingDemonBody);

            engine.PlayCard(state, CultivationSeedData.UndyingDemonBody, state.Enemies[0]);
            engine.EndPlayerTurn(state);

            Assert.AreEqual(BattleOutcome.InProgress, state.Outcome);
            Assert.AreEqual(20, state.Player.CurrentHp);
        }

        [Test]
        public void HeavenlyDemonEscapeRewardsSelfDamageThisTurn()
        {
            var engine = new BattleEngine(1);
            var state = CreateOrderedBattle(engine, CultivationSeedData.HeavenlyDemonDisintegration, CultivationSeedData.HeavenlyDemonEscape);

            engine.PlayCard(state, CultivationSeedData.HeavenlyDemonDisintegration, state.Enemies[0]);
            engine.PlayCard(state, CultivationSeedData.HeavenlyDemonEscape, state.Enemies[0]);

            Assert.AreEqual(2, state.DodgeCharges);
            Assert.GreaterOrEqual(state.Hand.Count, 4);
        }

        [Test]
        public void MedicineSectStarterDeckUsesPoisonLeechAndRegenerationCards()
        {
            var deck = CultivationSeedData.CreateMedicineSectStarterDeck();

            Assert.AreEqual(12, deck.Count);
            Assert.AreEqual(3, deck.Count(card => card.Id == "poison_vine_art"));
            Assert.AreEqual(2, deck.Count(card => card.Id == "corrosive_poison_palm"));
            Assert.AreEqual(2, deck.Count(card => card.Id == "spirit_leech_art"));
            Assert.AreEqual(2, deck.Count(card => card.Id == "rejuvenation_art"));
            Assert.AreEqual(1, deck.Count(card => card.Id == "poison_miasma_guard"));
            Assert.AreEqual(1, deck.Count(card => card.Id == "wood_escape"));
            Assert.AreEqual(1, deck.Count(card => card.Id == "healing_pill"));
            Assert.IsTrue(deck.Any(card => card.Effects.Any(effect => effect.Type == CardEffectType.Poison)));
            Assert.IsTrue(deck.Any(card => card.Effects.Any(effect => effect.Type == CardEffectType.Leech)));
            Assert.IsTrue(deck.Any(card => card.Effects.Any(effect => effect.Type == CardEffectType.PoisonAttackCounter)));
        }

        [Test]
        public void ThunderSectStarterDeckUsesStunAndCyclingCards()
        {
            var deck = CultivationSeedData.CreateThunderSectStarterDeck();

            Assert.AreEqual(12, deck.Count);
            Assert.AreEqual(5, deck.Count(card => card.Id == "thunder_talisman"));
            Assert.AreEqual(2, deck.Count(card => card.Id == "heavenly_thunder_spell"));
            Assert.AreEqual(1, deck.Count(card => card.Id == "thunder_escape"));
            Assert.AreEqual(2, deck.Count(card => card.Id == "guard_qi"));
            Assert.AreEqual(1, deck.Count(card => card.Id == "light_body"));
            Assert.AreEqual(1, deck.Count(card => card.Id == "healing_pill"));
            Assert.IsTrue(deck.Any(card => card.Effects.Any(effect => effect.Type == CardEffectType.ChanceDamage && effect.ChancePercent == 30 && effect.FallbackValue == 5)));
            Assert.IsTrue(deck.Any(card => card.Effects.Any(effect => effect.Type == CardEffectType.ChanceStun && effect.ChancePercent == 40)));
            Assert.IsTrue(deck.Any(card => card.Effects.Any(effect => effect.Type == CardEffectType.Draw)));
        }

        [Test]
        public void ChanceDamageUsesFallbackWhenProbabilityMisses()
        {
            var engine = new BattleEngine(1);
            var chanceCard = new CardDefinition(
                "chance_damage_miss_test",
                "概率伤害失败测试",
                0,
                new CardEffect(CardEffectType.ChanceDamage, 10, chancePercent: 0, fallbackValue: 5));
            var state = CreateOrderedBattle(engine, chanceCard);
            var enemy = state.Enemies[0];

            engine.PlayCard(state, state.Hand[0], enemy);

            Assert.AreEqual(37, enemy.Body.CurrentHp);
            Assert.IsTrue(state.Logs.Any(log => log.Message.Contains("暴击未触发")));
        }

        [Test]
        public void ChanceDamageUsesCriticalValueWhenProbabilityHits()
        {
            var engine = new BattleEngine(1);
            var chanceCard = new CardDefinition(
                "chance_damage_hit_test",
                "概率伤害成功测试",
                0,
                new CardEffect(CardEffectType.ChanceDamage, 10, chancePercent: 100, fallbackValue: 5));
            var state = CreateOrderedBattle(engine, chanceCard);
            var enemy = state.Enemies[0];

            engine.PlayCard(state, state.Hand[0], enemy);

            Assert.AreEqual(32, enemy.Body.CurrentHp);
            Assert.IsTrue(state.Logs.Any(log => log.Message.Contains("暴击触发")));
        }

        [Test]
        public void CriticalStateResetsAtPlayerTurnStart()
        {
            var engine = new BattleEngine(1);
            var criticalCard = new CardDefinition(
                "critical_state_reset_test",
                "暴击状态重置测试",
                0,
                new CardEffect(CardEffectType.ChanceDamage, 10, chancePercent: 100, fallbackValue: 5));
            var state = CreateOrderedBattle(engine, criticalCard, CultivationSeedData.GuardQi);

            engine.PlayCard(state, state.Hand[0], state.Enemies[0]);

            Assert.IsTrue(state.HasTriggeredCriticalThisTurn);

            engine.EndPlayerTurn(state);

            Assert.IsFalse(state.HasTriggeredCriticalThisTurn);
        }

        [Test]
        public void ChanceStunOnlyAppliesWhenProbabilityHits()
        {
            var engine = new BattleEngine(1);
            var missedStun = new CardDefinition(
                "chance_stun_miss_test",
                "概率眩晕失败测试",
                0,
                new CardEffect(CardEffectType.ChanceStun, 0, duration: 1, chancePercent: 0));
            var guaranteedStun = new CardDefinition(
                "chance_stun_hit_test",
                "概率眩晕成功测试",
                0,
                new CardEffect(CardEffectType.ChanceStun, 0, duration: 1, chancePercent: 100));
            var state = CreateOrderedBattle(engine, missedStun, guaranteedStun);
            var enemy = state.Enemies[0];

            engine.PlayCard(state, state.Hand[0], enemy);

            Assert.AreEqual(0, enemy.Body.StunTurns);
            Assert.IsTrue(state.Logs.Any(log => log.Message.Contains("眩晕判定未触发")));

            engine.PlayCard(state, state.Hand[0], enemy);

            Assert.AreEqual(1, enemy.Body.StunTurns);
            Assert.IsTrue(state.Logs.Any(log => log.Message.Contains("眩晕判定成功")));

            engine.EndPlayerTurn(state);

            Assert.AreEqual(0, enemy.Body.StunTurns);
            Assert.AreEqual(100, state.Player.CurrentHp);
            Assert.IsTrue(state.Logs.Any(log => log.Message.Contains("因眩晕跳过行动")));
        }

        [Test]
        public void ChanceChainDamageCanHitAdditionalEnemies()
        {
            var engine = new BattleEngine(1);
            var chainCard = new CardDefinition(
                "chain_damage_hit_test",
                "连锁伤害成功测试",
                0,
                new CardEffect(CardEffectType.ChanceChainDamage, 8, chancePercent: 100, secondaryValue: 4));
            var state = CreateMultiEnemyBattle(engine, chainCard);

            engine.PlayCard(state, state.Hand[0], state.Enemies[0]);

            Assert.AreEqual(34, state.Enemies[0].Body.CurrentHp);
            Assert.AreEqual(38, state.Enemies[1].Body.CurrentHp);
            Assert.AreEqual(38, state.Enemies[2].Body.CurrentHp);
            Assert.IsTrue(state.Logs.Any(log => log.Message.Contains("连锁至")));
            Assert.IsTrue(state.Logs.Any(log => log.Message.Contains("没有可用目标")));
        }

        [Test]
        public void ChanceChainDamageCanStopAfterPrimaryTarget()
        {
            var engine = new BattleEngine(1);
            var chainCard = new CardDefinition(
                "chain_damage_miss_test",
                "连锁伤害失败测试",
                0,
                new CardEffect(CardEffectType.ChanceChainDamage, 8, chancePercent: 0, secondaryValue: 4));
            var state = CreateMultiEnemyBattle(engine, chainCard);

            engine.PlayCard(state, state.Hand[0], state.Enemies[0]);

            Assert.AreEqual(34, state.Enemies[0].Body.CurrentHp);
            Assert.AreEqual(40, state.Enemies[1].Body.CurrentHp);
            Assert.AreEqual(40, state.Enemies[2].Body.CurrentHp);
            Assert.IsTrue(state.Logs.Any(log => log.Message.Contains("雷击连锁未继续触发")));
        }

        [Test]
        public void ChanceChainDamageWithStunCanStunChainedTarget()
        {
            var engine = new BattleEngine(1);
            var chainCard = new CardDefinition(
                "chain_damage_stun_test",
                "连锁眩晕测试",
                0,
                new CardEffect(CardEffectType.ChanceChainDamageWithStun, 8, duration: 1, chancePercent: 100, fallbackValue: 100, secondaryValue: 4));
            var state = CreateMultiEnemyBattle(engine, chainCard);

            engine.PlayCard(state, state.Hand[0], state.Enemies[0]);

            Assert.AreEqual(34, state.Enemies[0].Body.CurrentHp);
            Assert.AreEqual(38, state.Enemies[1].Body.CurrentHp);
            Assert.AreEqual(1, state.Enemies[1].Body.StunTurns);
            Assert.AreEqual(38, state.Enemies[2].Body.CurrentHp);
            Assert.AreEqual(1, state.Enemies[2].Body.StunTurns);
            Assert.IsTrue(state.Logs.Any(log => log.Message.Contains("连锁雷击使")));
        }

        [Test]
        public void ChanceChainDamageRepeatTargetCanHitSameEnemyUntilLimit()
        {
            var engine = new BattleEngine(1);
            var chainCard = new CardDefinition(
                "chain_damage_repeat_target_test",
                "重复目标连锁测试",
                0,
                new CardEffect(CardEffectType.ChanceChainDamageRepeatTarget, 8, chancePercent: 100, secondaryValue: 4, repeatCount: 3));
            var state = CreateOrderedBattle(engine, chainCard);

            engine.PlayCard(state, state.Hand[0], state.Enemies[0]);

            Assert.AreEqual(28, state.Enemies[0].Body.CurrentHp);
            Assert.AreEqual(3, state.Logs.Count(log => log.Message.Contains("连锁至")));
            Assert.IsTrue(state.Logs.Any(log => log.Message.Contains("达到上限 3 次")));
        }

        [Test]
        public void FiveThunderOrthodoxyCanDamageStunAndChain()
        {
            var engine = new BattleEngine(1);
            var fiveThunder = new CardDefinition(
                "five_thunder_test",
                "五雷正法测试",
                0,
                new CardEffect(CardEffectType.ChanceChainDamage, 8, chancePercent: 100, secondaryValue: 4),
                new CardEffect(CardEffectType.ChanceStun, 0, duration: 1, chancePercent: 100));
            var state = CreateMultiEnemyBattle(engine, fiveThunder);

            engine.PlayCard(state, state.Hand[0], state.Enemies[0]);

            Assert.AreEqual(34, state.Enemies[0].Body.CurrentHp);
            Assert.AreEqual(1, state.Enemies[0].Body.StunTurns);
            Assert.AreEqual(38, state.Enemies[1].Body.CurrentHp);
            Assert.AreEqual(38, state.Enemies[2].Body.CurrentHp);
            Assert.IsTrue(state.Logs.Any(log => log.Message.Contains("眩晕判定成功")));
            Assert.IsTrue(state.Logs.Any(log => log.Message.Contains("连锁至")));
        }

        [Test]
        public void ThunderousBarrageHitsMultipleTimes()
        {
            var engine = new BattleEngine(1);
            var barrage = new CardDefinition(
                "thunderous_barrage_test",
                "雷霆万钧测试",
                0,
                new CardEffect(CardEffectType.ChanceDamage, 4, repeatCount: 3, chancePercent: 0, fallbackValue: 4));
            var state = CreateOrderedBattle(engine, barrage);

            engine.PlayCard(state, state.Hand[0], state.Enemies[0]);

            Assert.AreEqual(34, state.Enemies[0].Body.CurrentHp);
            Assert.AreEqual(3, state.Logs.Count(log => log.Message.Contains("暴击未触发")));
        }

        [Test]
        public void ThunderousBarrageCriticalCanStun()
        {
            var engine = new BattleEngine(1);
            var barrage = new CardDefinition(
                "thunderous_barrage_stun_test",
                "雷霆万钧眩晕测试",
                0,
                new CardEffect(CardEffectType.ChanceDamageWithStun, 4, duration: 1, repeatCount: 3, chancePercent: 100, fallbackValue: 100));
            var state = CreateOrderedBattle(engine, barrage);

            engine.PlayCard(state, state.Hand[0], state.Enemies[0]);

            Assert.AreEqual(34, state.Enemies[0].Body.CurrentHp);
            Assert.AreEqual(1, state.Enemies[0].Body.StunTurns);
            Assert.AreEqual(3, state.Logs.Count(log => log.Message.Contains("暴击附加眩晕")));
        }

        [Test]
        public void ThunderousBarrageChainCanTriggerPerHit()
        {
            var engine = new BattleEngine(1);
            var barrage = new CardDefinition(
                "thunderous_barrage_chain_test",
                "雷霆万钧连锁测试",
                0,
                new CardEffect(CardEffectType.ChanceDamageWithChain, 4, repeatCount: 3, chancePercent: 100, fallbackValue: 2, secondaryValue: 100));
            var state = CreateMultiEnemyBattle(engine, barrage);

            engine.PlayCard(state, state.Hand[0], state.Enemies[0]);

            Assert.AreEqual(34, state.Enemies[0].Body.CurrentHp);
            Assert.AreEqual(80, state.Enemies[1].Body.CurrentHp + state.Enemies[2].Body.CurrentHp);
            Assert.AreEqual(3, state.Logs.Count(log => log.Message.Contains("连锁至")));
        }

        [Test]
        public void ThunderHammerOnlyDealsBaseDamageBeforeCriticalTriggers()
        {
            var engine = new BattleEngine(1);
            var hammer = new CardDefinition(
                "thunder_hammer_base_test",
                "雷神之锤基础测试",
                0,
                new CardEffect(CardEffectType.DamageAfterCriticalTriggered, 22, fallbackValue: 10));
            var state = CreateOrderedBattle(engine, hammer);

            engine.PlayCard(state, state.Hand[0], state.Enemies[0]);

            Assert.AreEqual(20, state.Enemies[0].Body.CurrentHp);
            Assert.IsTrue(state.Logs.Any(log => log.Message.Contains("暴击奖励未触发")));
        }

        [Test]
        public void ThunderHammerAddsBonusAfterCriticalTriggersThisTurn()
        {
            var engine = new BattleEngine(1);
            var criticalCard = new CardDefinition(
                "thunder_hammer_critical_setup_test",
                "雷神之锤暴击前置测试",
                0,
                new CardEffect(CardEffectType.ChanceDamage, 10, chancePercent: 100, fallbackValue: 5));
            var hammer = new CardDefinition(
                "thunder_hammer_bonus_test",
                "雷神之锤奖励测试",
                0,
                new CardEffect(CardEffectType.DamageAfterCriticalTriggered, 22, fallbackValue: 10));
            var state = CreateOrderedBattle(engine, criticalCard, hammer);

            engine.PlayCard(state, state.Hand[0], state.Enemies[0]);
            engine.PlayCard(state, state.Hand[0], state.Enemies[0]);

            Assert.AreEqual(4, state.Enemies[0].Body.CurrentHp);
            Assert.IsTrue(state.Logs.Any(log => log.Message.Contains("触发暴击奖励")));
        }

        [Test]
        public void ThunderHammerStunUpgradeStunsWhenBonusTriggers()
        {
            var engine = new BattleEngine(1);
            var criticalCard = new CardDefinition(
                "thunder_hammer_stun_setup_test",
                "雷神之锤眩晕前置测试",
                0,
                new CardEffect(CardEffectType.ChanceDamage, 10, chancePercent: 100, fallbackValue: 5));
            var hammer = new CardDefinition(
                "thunder_hammer_stun_test",
                "雷神之锤眩晕测试",
                0,
                new CardEffect(CardEffectType.DamageAfterCriticalTriggeredWithStun, 22, duration: 1, fallbackValue: 18));
            var state = CreateOrderedBattle(engine, criticalCard, hammer);

            engine.PlayCard(state, state.Hand[0], state.Enemies[0]);
            engine.PlayCard(state, state.Hand[0], state.Enemies[0]);

            Assert.AreEqual(1, state.Enemies[0].Body.StunTurns);
            Assert.IsTrue(state.Logs.Any(log => log.Message.Contains("暴击奖励附加眩晕")));
        }

        [Test]
        public void ThunderHammerChainUpgradeChainsBonusToAllEnemies()
        {
            var engine = new BattleEngine(1);
            var criticalCard = new CardDefinition(
                "thunder_hammer_chain_setup_test",
                "雷神之锤连锁前置测试",
                0,
                new CardEffect(CardEffectType.ChanceDamage, 10, chancePercent: 100, fallbackValue: 5));
            var hammer = new CardDefinition(
                "thunder_hammer_chain_test",
                "雷神之锤连锁测试",
                0,
                new CardEffect(CardEffectType.DamageAfterCriticalTriggeredChainAll, 27, fallbackValue: 10));
            var state = CreateMultiEnemyBattle(engine, criticalCard, hammer);

            engine.PlayCard(state, state.Hand[0], state.Enemies[0]);
            engine.PlayCard(state, state.Hand[0], state.Enemies[0]);

            Assert.AreEqual(0, state.Enemies[0].Body.CurrentHp);
            Assert.AreEqual(32, state.Enemies[1].Body.CurrentHp);
            Assert.AreEqual(32, state.Enemies[2].Body.CurrentHp);
            Assert.AreEqual(3, state.Logs.Count(log => log.Message.Contains("暴击奖励连锁至")));
        }

        [Test]
        public void ChainOnChanceDamageOnlyChainsWhenCriticalHits()
        {
            var engine = new BattleEngine(1);
            var missedCard = new CardDefinition(
                "chain_on_chance_damage_miss_test",
                "暴击连锁失败测试",
                0,
                new CardEffect(CardEffectType.ChainOnChanceDamage, 10, chancePercent: 0, fallbackValue: 5, secondaryValue: 7));
            var hitCard = new CardDefinition(
                "chain_on_chance_damage_hit_test",
                "暴击连锁成功测试",
                0,
                new CardEffect(CardEffectType.ChainOnChanceDamage, 10, chancePercent: 100, fallbackValue: 5, secondaryValue: 7));
            var state = CreateMultiEnemyBattle(engine, missedCard, hitCard);

            engine.PlayCard(state, state.Hand[0], state.Enemies[0]);

            Assert.AreEqual(37, state.Enemies[0].Body.CurrentHp);
            Assert.AreEqual(40, state.Enemies[1].Body.CurrentHp);
            Assert.AreEqual(40, state.Enemies[2].Body.CurrentHp);

            engine.PlayCard(state, state.Hand[0], state.Enemies[0]);

            Assert.AreEqual(29, state.Enemies[0].Body.CurrentHp);
            Assert.AreEqual(35, state.Enemies[1].Body.CurrentHp);
            Assert.AreEqual(40, state.Enemies[2].Body.CurrentHp);
            Assert.IsTrue(state.Logs.Any(log => log.Message.Contains("暴击触发")));
            Assert.IsTrue(state.Logs.Any(log => log.Message.Contains("连锁至")));
        }

        [Test]
        public void ChainOnChanceStunOnlyChainsWhenStunHits()
        {
            var engine = new BattleEngine(1);
            var missedCard = new CardDefinition(
                "chain_on_chance_stun_miss_test",
                "眩晕连锁失败测试",
                0,
                new CardEffect(CardEffectType.ChainOnChanceStun, 10, duration: 1, chancePercent: 0, secondaryValue: 5));
            var hitCard = new CardDefinition(
                "chain_on_chance_stun_hit_test",
                "眩晕连锁成功测试",
                0,
                new CardEffect(CardEffectType.ChainOnChanceStun, 10, duration: 1, chancePercent: 100, secondaryValue: 5));
            var state = CreateMultiEnemyBattle(engine, missedCard, hitCard);

            engine.PlayCard(state, state.Hand[0], state.Enemies[0]);

            Assert.AreEqual(32, state.Enemies[0].Body.CurrentHp);
            Assert.AreEqual(0, state.Enemies[0].Body.StunTurns);
            Assert.AreEqual(40, state.Enemies[1].Body.CurrentHp);

            engine.PlayCard(state, state.Hand[0], state.Enemies[0]);

            Assert.AreEqual(24, state.Enemies[0].Body.CurrentHp);
            Assert.AreEqual(1, state.Enemies[0].Body.StunTurns);
            Assert.AreEqual(37, state.Enemies[1].Body.CurrentHp);
            Assert.AreEqual(40, state.Enemies[2].Body.CurrentHp);
            Assert.IsTrue(state.Logs.Any(log => log.Message.Contains("眩晕判定成功")));
            Assert.IsTrue(state.Logs.Any(log => log.Message.Contains("连锁至")));
        }

        [Test]
        public void ChargeDamageDoublesNextAttackAndThenExpires()
        {
            var engine = new BattleEngine(1);
            var chargeCard = new CardDefinition(
                "charge_damage_test",
                "蓄力伤害测试",
                0,
                new CardEffect(CardEffectType.ChargeDamage, 2, CardTarget.Self),
                new CardEffect(CardEffectType.Shield, 4, CardTarget.Self));
            var state = CreateOrderedBattle(engine, chargeCard, CultivationSeedData.SwordQi, CultivationSeedData.SwordQi);
            var enemy = state.Enemies[0];

            engine.PlayCard(state, state.Hand[0], enemy);

            Assert.AreEqual(4, state.Player.Shield);
            Assert.AreEqual(2, state.ChargedDamageMultiplier);
            Assert.AreEqual(1, state.ChargedDamageUses);

            engine.PlayCard(state, state.Hand[0], enemy);

            Assert.AreEqual(26, enemy.Body.CurrentHp);
            Assert.AreEqual(1, state.ChargedDamageMultiplier);
            Assert.AreEqual(0, state.ChargedDamageUses);
            Assert.IsTrue(state.Logs.Any(log => log.Message.Contains("蓄力 x2")));

            engine.PlayCard(state, state.Hand[0], enemy);

            Assert.AreEqual(20, enemy.Body.CurrentHp);
        }

        [Test]
        public void ChargeDamageDoesNotAffectNonDamageCards()
        {
            var engine = new BattleEngine(1);
            var chargeCard = new CardDefinition(
                "charge_non_damage_test",
                "蓄力非伤害测试",
                0,
                new CardEffect(CardEffectType.ChargeDamage, 2, CardTarget.Self));
            var shieldCard = new CardDefinition(
                "shield_non_damage_test",
                "护盾非伤害测试",
                0,
                new CardEffect(CardEffectType.Shield, 5, CardTarget.Self));
            var state = CreateOrderedBattle(engine, chargeCard, shieldCard, CultivationSeedData.SwordQi);
            var enemy = state.Enemies[0];

            engine.PlayCard(state, state.Hand[0], enemy);
            engine.PlayCard(state, state.Hand[0], enemy);

            Assert.AreEqual(5, state.Player.Shield);
            Assert.AreEqual(2, state.ChargedDamageMultiplier);
            Assert.AreEqual(1, state.ChargedDamageUses);

            engine.PlayCard(state, state.Hand[0], enemy);

            Assert.AreEqual(26, enemy.Body.CurrentHp);
            Assert.AreEqual(1, state.ChargedDamageMultiplier);
            Assert.AreEqual(0, state.ChargedDamageUses);
        }

        [Test]
        public void ChanceDamageUsesChargeMultiplierOnResolvedDamage()
        {
            var engine = new BattleEngine(1);
            var chargeCard = new CardDefinition(
                "charge_chance_damage_test",
                "蓄力概率伤害测试",
                0,
                new CardEffect(CardEffectType.ChargeDamage, 2, CardTarget.Self));
            var chanceCard = new CardDefinition(
                "charged_chance_damage_hit_test",
                "蓄力概率命中测试",
                0,
                new CardEffect(CardEffectType.ChanceDamage, 10, chancePercent: 100, fallbackValue: 5));
            var state = CreateOrderedBattle(engine, chargeCard, chanceCard);
            var enemy = state.Enemies[0];

            engine.PlayCard(state, state.Hand[0], enemy);
            engine.PlayCard(state, state.Hand[0], enemy);

            Assert.AreEqual(22, enemy.Body.CurrentHp);
            Assert.AreEqual(1, state.ChargedDamageMultiplier);
            Assert.AreEqual(0, state.ChargedDamageUses);
        }

        [Test]
        public void ChanceChainDamageUsesChargeOnPrimaryDamageOnly()
        {
            var engine = new BattleEngine(1);
            var chargeCard = new CardDefinition(
                "charge_chain_damage_test",
                "蓄力连锁伤害测试",
                0,
                new CardEffect(CardEffectType.ChargeDamage, 2, CardTarget.Self));
            var chainCard = new CardDefinition(
                "charged_chain_damage_test",
                "蓄力连锁测试",
                0,
                new CardEffect(CardEffectType.ChanceChainDamage, 8, chancePercent: 100, secondaryValue: 4));
            var state = CreateMultiEnemyBattle(engine, chargeCard, chainCard);

            engine.PlayCard(state, state.Hand[0], state.Enemies[0]);
            engine.PlayCard(state, state.Hand[0], state.Enemies[0]);

            Assert.AreEqual(26, state.Enemies[0].Body.CurrentHp);
            Assert.AreEqual(38, state.Enemies[1].Body.CurrentHp);
            Assert.AreEqual(38, state.Enemies[2].Body.CurrentHp);
            Assert.AreEqual(1, state.ChargedDamageMultiplier);
            Assert.AreEqual(0, state.ChargedDamageUses);
        }

        [Test]
        public void SwordMarkExplodesWhenReachingThreeStacks()
        {
            var engine = new BattleEngine(1);
            var markCard = new CardDefinition(
                "mark_test",
                "剑气印记测试",
                0,
                new CardEffect(CardEffectType.SwordMark, 3));
            var state = CreateOrderedBattle(engine, markCard);
            var enemy = state.Enemies[0];

            engine.PlayCard(state, state.Hand[0], enemy);

            Assert.AreEqual(30, enemy.Body.CurrentHp);
            Assert.AreEqual(0, enemy.Body.SwordMarkStacks);
        }

        [Test]
        public void SevenfoldSwordQiScalesWithSwordMarkStacks()
        {
            var engine = new BattleEngine(1);
            var state = CreateOrderedBattle(engine, CultivationSeedData.Thrust, CultivationSeedData.SevenfoldSwordQi);
            var enemy = state.Enemies[0];

            engine.PlayCard(state, state.Hand[0], enemy);
            engine.PlayCard(state, state.Hand[0], enemy);

            Assert.AreEqual(1, enemy.Body.SwordMarkStacks);
            Assert.AreEqual(31, enemy.Body.CurrentHp);
        }

        [Test]
        public void SharpnessIgnoresEnemyDefenseForCardDamage()
        {
            var engine = new BattleEngine(1);
            var sharpnessCard = new CardDefinition(
                "sharpness_test",
                "锋锐测试",
                0,
                new CardEffect(CardEffectType.Sharpness, 3, CardTarget.Self, 2));
            var strike = new CardDefinition(
                "strike_test",
                "攻击测试",
                0,
                new CardEffect(CardEffectType.Damage, 8));
            var state = CreateOrderedBattle(engine, sharpnessCard, strike);
            var enemy = state.Enemies[0];

            engine.PlayCard(state, state.Hand[0], enemy);
            engine.PlayCard(state, state.Hand[0], enemy);

            Assert.AreEqual(32, enemy.Body.CurrentHp);
            Assert.AreEqual(3, state.Player.Sharpness);
            Assert.AreEqual(2, state.Player.SharpnessTurns);
        }

        [Test]
        public void MultiHitDamageAppliesDefensePerHit()
        {
            var engine = new BattleEngine(1);
            var multiHit = new CardDefinition(
                "multi_hit_test",
                "多段测试",
                0,
                new CardEffect(CardEffectType.Damage, 6, repeatCount: 2));
            var state = CreateOrderedBattle(engine, multiHit);
            var enemy = state.Enemies[0];

            engine.PlayCard(state, state.Hand[0], enemy);

            Assert.AreEqual(32, enemy.Body.CurrentHp);
        }

        [Test]
        public void DodgePreventsNextEnemyAttack()
        {
            var engine = new BattleEngine(1);
            var dodgeCard = new CardDefinition(
                "dodge_test",
                "闪避测试",
                0,
                new CardEffect(CardEffectType.Dodge, 1, CardTarget.Self));
            var state = CreateOrderedBattle(engine, dodgeCard);

            engine.PlayCard(state, state.Hand[0], state.Enemies[0]);
            engine.EndPlayerTurn(state);

            Assert.AreEqual(100, state.Player.CurrentHp);
            Assert.AreEqual(0, state.DodgeCharges);
            Assert.IsTrue(state.Logs.Any(log => log.Message.Contains("闪避了")));
        }

        [Test]
        public void DodgePreventsAttackRiderStatus()
        {
            var enemy = new EnemyDefinition(
                "burn_attacker",
                "灼烧攻击者",
                30,
                0,
                new EnemyIntent(EnemyIntentType.AttackAndBurn, 6, 2, "攻击 6 + 灼烧 2"));
            var engine = new BattleEngine(1);
            var dodgeCard = new CardDefinition(
                "dodge_rider_test",
                "闪避附带状态测试",
                0,
                new CardEffect(CardEffectType.Dodge, 1, CardTarget.Self));
            var state = engine.CreateBattle(new[] { dodgeCard }.Concat(CultivationSeedData.CreateSwordSectStarterDeck()), enemy);
            state.Hand.Clear();
            state.DrawPile.Remove(dodgeCard);
            state.Hand.Add(dodgeCard);

            engine.PlayCard(state, state.Hand[0], state.Enemies[0]);
            engine.EndPlayerTurn(state);

            Assert.AreEqual(100, state.Player.CurrentHp);
            Assert.AreEqual(0, state.Player.BurnStacks);
            Assert.IsFalse(state.Logs.Any(log => log.Message.Contains("施加灼烧")));
        }

        [Test]
        public void DodgeCounterDamagesAttackerWhenDodgeSucceeds()
        {
            var enemy = new EnemyDefinition(
                "counter_attacker",
                "反击目标",
                30,
                0,
                new EnemyIntent(EnemyIntentType.Attack, 6, description: "攻击 6"));
            var engine = new BattleEngine(1);
            var dodgeCounterCard = new CardDefinition(
                "dodge_counter_test",
                "闪避反击测试",
                0,
                new CardEffect(CardEffectType.Dodge, 1, CardTarget.Self),
                new CardEffect(CardEffectType.DodgeCounter, 8, CardTarget.Self));
            var state = engine.CreateBattle(new[] { dodgeCounterCard }.Concat(CultivationSeedData.CreateSwordSectStarterDeck()), enemy);
            state.Hand.Clear();
            state.DrawPile.Remove(dodgeCounterCard);
            state.Hand.Add(dodgeCounterCard);

            engine.PlayCard(state, state.Hand[0], state.Enemies[0]);
            engine.EndPlayerTurn(state);

            Assert.AreEqual(100, state.Player.CurrentHp);
            Assert.AreEqual(22, state.Enemies[0].Body.CurrentHp);
            Assert.AreEqual(8, state.DodgeCounterDamage);
            Assert.IsTrue(state.Logs.Any(log => log.Message.Contains("闪避反击")));
        }

        [Test]
        public void AttackCounterDamagesAttackerAfterEnemyAttack()
        {
            var engine = new BattleEngine(1);
            var counterCard = new CardDefinition(
                "attack_counter_hit_test",
                "受击反伤测试",
                0,
                new CardEffect(CardEffectType.AttackCounter, 4, CardTarget.Self));
            var state = CreateOrderedBattle(engine, counterCard);

            engine.PlayCard(state, state.Hand[0], state.Enemies[0]);
            engine.EndPlayerTurn(state);

            Assert.AreEqual(94, state.Player.CurrentHp);
            Assert.AreEqual(38, state.Enemies[0].Body.CurrentHp);
            Assert.IsTrue(state.Logs.Any(log => log.Message.Contains("attack-counter dealt")));
        }

        [Test]
        public void AttackCounterCanMissByChance()
        {
            var engine = new BattleEngine(1);
            var counterCard = new CardDefinition(
                "attack_counter_miss_test",
                "受击反伤未触发测试",
                0,
                new CardEffect(CardEffectType.AttackCounter, 4, CardTarget.Self, chancePercent: 0));
            var state = CreateOrderedBattle(engine, counterCard);

            engine.PlayCard(state, state.Hand[0], state.Enemies[0]);
            engine.EndPlayerTurn(state);

            Assert.AreEqual(94, state.Player.CurrentHp);
            Assert.AreEqual(40, state.Enemies[0].Body.CurrentHp);
            Assert.IsTrue(state.Logs.Any(log => log.Message.Contains("attack-counter missed")));
        }

        [Test]
        public void AttackCounterTriggersWhenShieldAbsorbsAttackDamage()
        {
            var engine = new BattleEngine(1);
            var counterCard = new CardDefinition(
                "attack_counter_shield_test",
                "护盾受击反伤测试",
                0,
                new CardEffect(CardEffectType.Shield, 8, CardTarget.Self),
                new CardEffect(CardEffectType.AttackCounter, 4, CardTarget.Self));
            var state = CreateOrderedBattle(engine, counterCard);

            engine.PlayCard(state, state.Hand[0], state.Enemies[0]);
            engine.EndPlayerTurn(state);

            Assert.AreEqual(100, state.Player.CurrentHp);
            Assert.AreEqual(38, state.Enemies[0].Body.CurrentHp);
            Assert.IsTrue(state.Logs.Any(log => log.Message.Contains("attack-counter dealt")));
        }

        [Test]
        public void AttackCounterDoesNotTriggerOnNonAttackIntent()
        {
            var enemy = new EnemyDefinition(
                "defender",
                "防御者",
                30,
                0,
                new EnemyIntent(EnemyIntentType.Defend, 5, description: "防御 5"));
            var engine = new BattleEngine(1);
            var counterCard = new CardDefinition(
                "attack_counter_non_attack_test",
                "非攻击反伤测试",
                0,
                new CardEffect(CardEffectType.AttackCounter, 4, CardTarget.Self));
            var state = engine.CreateBattle(new[] { counterCard }.Concat(CultivationSeedData.CreateSwordSectStarterDeck()), enemy);
            state.Hand.Clear();
            state.DrawPile.Remove(counterCard);
            state.Hand.Add(counterCard);

            engine.PlayCard(state, state.Hand[0], state.Enemies[0]);
            engine.EndPlayerTurn(state);

            Assert.AreEqual(100, state.Player.CurrentHp);
            Assert.AreEqual(30, state.Enemies[0].Body.CurrentHp);
            Assert.AreEqual(5, state.Enemies[0].Body.Shield);
            Assert.IsFalse(state.Logs.Any(log => log.Message.Contains("attack-counter dealt")));
        }

        [Test]
        public void AttackCounterExpiresAtNextPlayerTurn()
        {
            var engine = new BattleEngine(1);
            var counterCard = new CardDefinition(
                "attack_counter_expire_test",
                "受击反伤过期测试",
                0,
                new CardEffect(CardEffectType.AttackCounter, 4, CardTarget.Self));
            var state = CreateOrderedBattle(engine, counterCard);

            engine.PlayCard(state, state.Hand[0], state.Enemies[0]);
            engine.EndPlayerTurn(state);

            Assert.AreEqual(0, state.AttackCounterDamage);
            Assert.AreEqual(0, state.AttackCounterChancePercent);
        }

        [Test]
        public void ThunderDefensiveCardsExposeAttackCounterUpgradeTrees()
        {
            Assert.AreEqual("thunder_shield", CultivationSeedData.ThunderShield.Id);
            Assert.AreEqual("thunder_strike_rebound", CultivationSeedData.ThunderStrikeRebound.Id);
            Assert.IsTrue(CultivationSeedData.ThunderShield.Effects.Any(effect => effect.Type == CardEffectType.AttackCounter && effect.ChancePercent == 30));
            Assert.IsTrue(CultivationSeedData.ThunderStrikeRebound.Effects.Any(effect => effect.Type == CardEffectType.AttackCounter && effect.ChancePercent == 100));
            Assert.GreaterOrEqual(CultivationSeedData.ThunderShield.UpgradeOptions.Sum(option => option.UpgradedCard.UpgradeOptions.Count), 4);
            Assert.GreaterOrEqual(CultivationSeedData.ThunderStrikeRebound.UpgradeOptions.Sum(option => option.UpgradedCard.UpgradeOptions.Count), 4);
        }

        [Test]
        public void ExhaustCardMovesToExhaustPileInsteadOfDiscardPile()
        {
            var engine = new BattleEngine(1);
            var exhaustCard = new CardDefinition(
                "exhaust_test",
                "消耗测试",
                0,
                new CardEffect(CardEffectType.Damage, 1),
                new CardEffect(CardEffectType.Exhaust, 1, CardTarget.Self));
            var state = CreateOrderedBattle(engine, exhaustCard);

            engine.PlayCard(state, state.Hand[0], state.Enemies[0]);

            Assert.AreEqual(0, state.DiscardPile.Count);
            Assert.AreEqual(1, state.ExhaustPile.Count);
            Assert.AreEqual("exhaust_test", state.ExhaustPile[0].Id);
        }

        [Test]
        public void EnemyIntentActionsAdvanceAfterTurnEnd()
        {
            var engine = new BattleEngine(1);
            var state = CreateOrderedBattle(engine, CultivationSeedData.GuardQi);

            engine.EndPlayerTurn(state);

            Assert.AreEqual(EnemyIntentType.Defend, state.Enemies[0].CurrentIntent.Type);
        }

        [Test]
        public void EnemyDefenseIntentAddsShieldForNextPlayerTurn()
        {
            var engine = new BattleEngine(1);
            var state = CreateOrderedBattle(engine, CultivationSeedData.GuardQi, CultivationSeedData.GuardQi);
            var enemy = state.Enemies[0];

            engine.EndPlayerTurn(state);
            engine.EndPlayerTurn(state);

            Assert.AreEqual(5, enemy.Body.Shield);
            Assert.AreEqual(EnemyIntentType.Attack, enemy.CurrentIntent.Type);
        }

        [Test]
        public void BattleCanEndInVictory()
        {
            var engine = new BattleEngine(1);
            var finisher = new CardDefinition(
                "finish",
                "终结",
                0,
                new CardEffect(CardEffectType.Damage, 80));
            var state = CreateOrderedBattle(engine, finisher);

            engine.PlayCard(state, state.Hand[0], state.Enemies[0]);

            Assert.AreEqual(BattleOutcome.Victory, state.Outcome);
        }

        [Test]
        public void BattleCanEndInDefeat()
        {
            var enemy = new EnemyDefinition(
                "test_enemy",
                "测试敌人",
                10,
                0,
                new EnemyIntent(EnemyIntentType.Attack, 150));
            var engine = new BattleEngine(1);
            var state = engine.CreateBattle(CultivationSeedData.CreateSwordSectStarterDeck(), enemy);

            engine.EndPlayerTurn(state);

            Assert.AreEqual(BattleOutcome.Defeat, state.Outcome);
        }

        [Test]
        public void PrototypeUICanCreateBattleAndPlayCard()
        {
            var parent = new GameObject("PrototypeTestCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            try
            {
                var ui = CultivationBattlePrototypeUI.Open(parent.transform);
                var before = ui.Snapshot;

                Assert.AreEqual(5, before.HandCount);
                Assert.AreEqual(3, before.Spirit);
                Assert.AreEqual("石魔", before.EnemyName);

                ui.PlayCardAt(0);
                var after = ui.Snapshot;

                Assert.Less(after.HandCount, before.HandCount);
                Assert.LessOrEqual(after.Spirit, before.Spirit);
            }
            finally
            {
                Object.DestroyImmediate(parent);
                var eventSystem = Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
                if (eventSystem != null)
                {
                    Object.DestroyImmediate(eventSystem.gameObject);
                }
            }
        }

        private static BattleState CreateStoneBattle()
        {
            var engine = new BattleEngine(1);
            return engine.CreateBattle(CultivationSeedData.CreateSwordSectStarterDeck(), CultivationSeedData.StoneDemon);
        }

        private static BattleState CreateOrderedBattle(BattleEngine engine, params CardDefinition[] firstCards)
        {
            var deck = firstCards
                .Concat(CultivationSeedData.CreateSwordSectStarterDeck())
                .Take(12)
                .ToArray();
            var state = new BattleState(
                new CombatantState("修士", 100),
                deck,
                new[] { new EnemyState(CultivationSeedData.StoneDemon) });
            engine.StartPlayerTurn(state);
            return state;
        }

        private static BattleState CreateOrderedBattleWithPlayerHp(BattleEngine engine, int playerCurrentHp, params CardDefinition[] firstCards)
        {
            var deck = firstCards
                .Concat(CultivationSeedData.CreateSwordSectStarterDeck())
                .Take(12)
                .ToArray();
            var state = new BattleState(
                new CombatantState("修士", 100, currentHp: playerCurrentHp),
                deck,
                new[] { new EnemyState(CultivationSeedData.StoneDemon) });
            engine.StartPlayerTurn(state);
            return state;
        }

        private static BattleState CreateMultiEnemyBattle(BattleEngine engine, params CardDefinition[] firstCards)
        {
            var deck = firstCards
                .Concat(CultivationSeedData.CreateSwordSectStarterDeck())
                .Take(12)
                .ToArray();
            var state = new BattleState(
                new CombatantState("修士", 100),
                deck,
                new[]
                {
                    new EnemyState(CultivationSeedData.StoneDemon),
                    new EnemyState(CultivationSeedData.StoneDemon),
                    new EnemyState(CultivationSeedData.StoneDemon),
                });
            engine.StartPlayerTurn(state);
            return state;
        }
    }
}
