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
    }
}
