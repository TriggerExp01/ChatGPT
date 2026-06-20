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
