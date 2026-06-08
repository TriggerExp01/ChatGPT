using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic.Tests
{
    public sealed class RoguelikeStage31RewardCardTests
    {
        private const string CardFrameSprite = "Roguelike_UI_CardFrame";
        private const string WeaponIconSprite = "Roguelike_UI_WeaponIcon";
        private const string RelicIconSprite = "Roguelike_UI_RelicIcon";
        private const string GoldIconSprite = "Roguelike_UI_GoldIcon";

        [Test]
        public void ChoiceUiCreatesAnimeFantasyRewardCards()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(31001);
            SetRewardChoices(game,
                new RoguelikeChoiceOption("weapon_magic_bolt_upgrade", "魔弹升级", "追踪魔弹升至 2 级", run => { }),
                new RoguelikeChoiceOption("passive_star", "星辉轻靴", "移动速度提升", run => { }),
                new RoguelikeChoiceOption("paid_field_ration", "战地补给", "立即恢复生命", run => { }, 8));

            using (ChoiceFixture fixture = ChoiceFixture.Create())
            {
                RectTransform panel = FindRect(fixture.Root, "奖励选择面板");
                Assert.NotNull(panel);
                Assert.That(panel.anchorMin.x, Is.LessThanOrEqualTo(0.10f));
                Assert.That(panel.anchorMax.y, Is.GreaterThan(0.75f));

                for (int i = 1; i <= 3; i++)
                {
                    Assert.NotNull(FindRect(panel, $"奖励卡_{i}"));
                    Assert.NotNull(FindRect(panel, $"奖励图标_{i}"));
                    Assert.NotNull(FindText(panel, $"卡牌标题_{i}"));
                    Assert.NotNull(FindText(panel, $"卡牌描述_{i}"));
                    Assert.NotNull(FindText(panel, $"卡牌价格_{i}"));
                    Assert.NotNull(FindText(panel, $"卡牌状态_{i}"));
                }

                AssertSprite(FindImage(panel, "奖励卡_1"), CardFrameSprite, Image.Type.Sliced);
                AssertSprite(FindImage(panel, "奖励图标_1"), WeaponIconSprite, Image.Type.Simple);
                AssertSprite(FindImage(panel, "奖励图标_2"), RelicIconSprite, Image.Type.Simple);
                AssertSprite(FindImage(panel, "奖励图标_3"), GoldIconSprite, Image.Type.Simple);
            }
        }

        [Test]
        public void RewardCardsShowCostAndUnaffordableState()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(31002);
            SetRewardChoices(game,
                new RoguelikeChoiceOption("paid_field_ration", "战地补给", "立即恢复生命", run => run.Player.Heal(35), 8),
                new RoguelikeChoiceOption("attack", "锋利武器", "攻击力 +3", run => run.Player.Stats.AddAttack(3)));

            using (ChoiceFixture fixture = ChoiceFixture.Create())
            {
                Button paidCard = FindButton(fixture.Root, "奖励卡_1");
                Button freeCard = FindButton(fixture.Root, "奖励卡_2");

                Assert.IsFalse(paidCard.interactable);
                Assert.IsTrue(freeCard.interactable);
                Assert.That(FindText(fixture.Root, "卡牌价格_1").text, Does.Contain("花费 8 金币"));
                Assert.That(FindText(fixture.Root, "卡牌状态_1").text, Does.Contain("金币不足"));
                Assert.That(FindText(fixture.Root, "卡牌状态_2").text, Does.Contain("点击选择"));
            }
        }

        [Test]
        public void ChoosingAffordableCardRestoresRunAndHidesPanel()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(31003);
            game.DebugAddGold(10);
            game.CurrentRun.Player.TakeDamage(40);
            SetRewardChoices(game, new RoguelikeChoiceOption("paid_field_ration", "战地补给", "立即恢复生命", run => run.Player.Heal(35), 8));

            using (ChoiceFixture fixture = ChoiceFixture.Create())
            {
                FindButton(fixture.Root, "奖励卡_1").onClick.Invoke();

                Assert.That(game.Phase, Is.EqualTo(RoguelikeGamePhase.Running));
                Assert.That(game.CurrentRun.Gold, Is.EqualTo(2));
                Assert.IsFalse(FindRect(fixture.Root, "奖励选择面板").gameObject.activeSelf);
                Assert.That(game.LastMessage, Does.Contain("已购买"));
            }
        }

        [Test]
        public void MaxLevelWeaponUpgradeIsFilteredFromRewardPool()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(31004);
            RoguelikeSurvivalWeapon weapon = game.Weapons[0];
            while (!weapon.IsMaxLevel)
            {
                weapon.LevelUp();
            }

            List<RoguelikeChoiceOption> pool = new List<RoguelikeChoiceOption>();
            MethodInfo method = typeof(RoguelikeGame).GetMethod("AddWeaponChoice", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(method);
            method.Invoke(game, new object[] { pool, RoguelikeWeaponType.MagicBolt });

            Assert.That(pool.Count, Is.EqualTo(0));
        }

        private static void SetRewardChoices(RoguelikeGame game, params RoguelikeChoiceOption[] choices)
        {
            FieldInfo field = typeof(RoguelikeGame).GetField("_rewardOptions", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            List<RoguelikeChoiceOption> options = (List<RoguelikeChoiceOption>)field.GetValue(game);
            options.Clear();
            options.AddRange(choices);
            SetPhase(game, RoguelikeGamePhase.RewardChoice);
        }

        private static void SetPhase(RoguelikeGame game, RoguelikeGamePhase phase)
        {
            PropertyInfo property = typeof(RoguelikeGame).GetProperty("Phase", BindingFlags.Instance | BindingFlags.Public);
            Assert.NotNull(property);
            property.SetValue(game, phase);
        }

        private static RectTransform FindRect(Transform root, string name)
        {
            Transform[] children = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i].name == name)
                {
                    return children[i] as RectTransform;
                }
            }

            return null;
        }

        private static Text FindText(Transform root, string name)
        {
            RectTransform rect = FindRect(root, name);
            return rect != null ? rect.GetComponent<Text>() : null;
        }

        private static Button FindButton(Transform root, string name)
        {
            RectTransform rect = FindRect(root, name);
            return rect != null ? rect.GetComponent<Button>() : null;
        }

        private static Image FindImage(Transform root, string name)
        {
            RectTransform rect = FindRect(root, name);
            return rect != null ? rect.GetComponent<Image>() : null;
        }

        private static void AssertSprite(Image image, string spriteName, Image.Type imageType)
        {
            Assert.NotNull(image);
            Assert.NotNull(image.sprite);
            Assert.That(image.sprite.name, Is.EqualTo(spriteName));
            Assert.That(image.type, Is.EqualTo(imageType));
        }

        private static void InvokeWindowMethod(object window, string methodName)
        {
            MethodInfo method = typeof(UIWindow).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(method);
            method.Invoke(window, null);
        }

        private sealed class ChoiceFixture : IDisposable
        {
            public object Window { get; private set; }
            public RectTransform Root { get; private set; }

            public static ChoiceFixture Create()
            {
                Type choiceType = typeof(RoguelikeGame).Assembly.GetType("GameLogic.BattleChoiceUI");
                Assert.NotNull(choiceType);

                GameObject panel = new GameObject("BattleChoiceUITest", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
                RectTransform root = panel.GetComponent<RectTransform>();
                root.anchorMin = Vector2.zero;
                root.anchorMax = Vector2.one;
                root.offsetMin = Vector2.zero;
                root.offsetMax = Vector2.zero;

                object window = Activator.CreateInstance(choiceType);
                MethodInfo init = typeof(UIWindow).GetMethod("Init", BindingFlags.Instance | BindingFlags.Public);
                Assert.NotNull(init);
                init.Invoke(window, new object[] { "GameLogic.BattleChoiceUI", (int)UILayer.Top, false, "BattleChoiceUI", false, 10 });

                MethodInfo completed = typeof(UIWindow).GetMethod("Handle_Completed", BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.NotNull(completed);
                completed.Invoke(window, new object[] { panel });
                InvokeWindowMethod(window, "InternalCreate");
                InvokeWindowMethod(window, "InternalRefresh");

                return new ChoiceFixture
                {
                    Window = window,
                    Root = root,
                };
            }

            public void Dispose()
            {
                if (Window != null)
                {
                    MethodInfo destroy = typeof(UIWindow).GetMethod("InternalDestroy", BindingFlags.Instance | BindingFlags.NonPublic);
                    destroy?.Invoke(Window, new object[] { true });
                    Window = null;
                }
            }
        }
    }
}
