using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using TEngine;
using UnityEngine;

namespace GameLogic.Tests
{
    public sealed class RoguelikeStage26GoldLoopTests
    {
        private const string MetaGoldKey = "Roguelike.MetaGold";

        [SetUp]
        public void ClearMetaGold()
        {
            PlayerPrefs.DeleteKey(MetaGoldKey);
            PlayerPrefs.Save();
        }

        [Test]
        public void NearbyGoldPickupAddsSpendableRunGold()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(26001);

            List<RoguelikeSurvivalPickup> pickups = GetPickupList(game);
            RoguelikeSurvivalPickup pickup = MemoryPool.Acquire<RoguelikeSurvivalPickup>();
            pickup.Init(26001, RoguelikePickupType.Gold, new Vector2(0.4f, 0f), 6);
            pickups.Add(pickup);

            game.Tick(0.1f);

            Assert.That(game.Pickups.Count, Is.EqualTo(0));
            Assert.That(game.CurrentRun.Gold, Is.EqualTo(6));
        }

        [Test]
        public void PaidRewardChoiceSpendsGoldAndContinuesRun()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(26002);
            game.CurrentRun.Player.TakeDamage(50);
            int damagedHealth = game.CurrentRun.Player.Health;
            game.CurrentRun.AddGold(10);

            SetRewardChoice(game, new RoguelikeChoiceOption("paid_test_heal", "战地补给", "恢复生命", run => run.Player.Heal(35), 8));

            game.ChooseReward(0);

            Assert.That(game.CurrentRun.Gold, Is.EqualTo(2));
            Assert.That(game.CurrentRun.Player.Health, Is.EqualTo(damagedHealth + 35));
            Assert.That(game.Phase, Is.EqualTo(RoguelikeGamePhase.Running));
            Assert.That(game.LastMessage, Does.Contain("已购买"));
        }

        [Test]
        public void UnaffordableRewardChoiceKeepsChoiceOpenAndDoesNotApply()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(26003);
            int attack = game.CurrentRun.Player.Stats.Attack;

            SetRewardChoice(game, new RoguelikeChoiceOption("paid_test_attack", "锋刃委托", "攻击 +10", run => run.Player.Stats.AddAttack(10), 8));

            game.ChooseReward(0);

            Assert.That(game.CurrentRun.Gold, Is.EqualTo(0));
            Assert.That(game.CurrentRun.Player.Stats.Attack, Is.EqualTo(attack));
            Assert.That(game.Phase, Is.EqualTo(RoguelikeGamePhase.RewardChoice));
            Assert.That(game.RewardOptions.Count, Is.EqualTo(1));
            Assert.That(game.LastMessage, Does.Contain("金币不足"));
        }

        [Test]
        public void SettlementBanksUnspentRunGoldOnce()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(26004);
            game.CurrentRun.AddGold(14);

            game.DebugForceDefeat();
            game.DebugForceDefeat();

            Assert.That(game.MetaGold, Is.EqualTo(14));
            Assert.That(game.SettlementSummary, Does.Contain("永久金币 14"));
        }

        private static List<RoguelikeSurvivalPickup> GetPickupList(RoguelikeGame game)
        {
            FieldInfo field = typeof(RoguelikeGame).GetField("_pickups", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            return (List<RoguelikeSurvivalPickup>)field.GetValue(game);
        }

        private static void SetRewardChoice(RoguelikeGame game, RoguelikeChoiceOption option)
        {
            List<RoguelikeChoiceOption> options = GetRewardOptions(game);
            options.Clear();
            options.Add(option);
            SetPhase(game, RoguelikeGamePhase.RewardChoice);
        }

        private static List<RoguelikeChoiceOption> GetRewardOptions(RoguelikeGame game)
        {
            FieldInfo field = typeof(RoguelikeGame).GetField("_rewardOptions", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            return (List<RoguelikeChoiceOption>)field.GetValue(game);
        }

        private static void SetPhase(RoguelikeGame game, RoguelikeGamePhase phase)
        {
            PropertyInfo property = typeof(RoguelikeGame).GetProperty("Phase", BindingFlags.Instance | BindingFlags.Public);
            Assert.NotNull(property);
            property.SetValue(game, phase);
        }
    }
}
