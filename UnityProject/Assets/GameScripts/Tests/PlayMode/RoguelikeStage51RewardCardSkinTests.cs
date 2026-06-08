using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic.Tests
{
    public sealed class RoguelikeStage51RewardCardSkinTests
    {
        [Test]
        public void RewardCardsUseFormalBackgroundLayersAndCostBadges()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(51001);
            SetRewardChoices(game,
                new RoguelikeChoiceOption("weapon_magic_bolt_upgrade", "魔弹升级", "追踪魔弹升至 2 级", run => { }),
                new RoguelikeChoiceOption("paid_field_ration", "战地补给", "立即恢复生命", run => { }, 8),
                new RoguelikeChoiceOption("passive_magnet_core", "磁力核心", "拾取范围提升", run => { }));

            using (ChoiceFixture fixture = ChoiceFixture.Create())
            {
                RectTransform panel = FindRect(fixture.Root, "奖励选择面板");
                Assert.NotNull(panel);

                AssertSprite(FindImage(panel, "卡牌正式底纹_1"), "Roguelike_UI_LightBand", Image.Type.Simple);
                AssertSprite(FindImage(panel, "卡牌顶部徽带_1"), "Roguelike_SystemG6_UI_DividerMetal", Image.Type.Simple);
                AssertSprite(FindImage(panel, "奖励图标底座_1"), "Roguelike_SystemG6_UI_SlotFrame", Image.Type.Sliced);

                RectTransform freeCostBadge = FindRect(panel, "卡牌金币徽章_1框");
                RectTransform paidCostBadge = FindRect(panel, "卡牌金币徽章_2框");
                Assert.NotNull(freeCostBadge);
                Assert.NotNull(paidCostBadge);
                Assert.IsFalse(freeCostBadge.gameObject.activeSelf);
                Assert.IsTrue(paidCostBadge.gameObject.activeSelf);
                AssertSprite(paidCostBadge.GetComponent<Image>(), "Roguelike_SystemG6_UI_SlotFrame", Image.Type.Sliced);
                AssertSprite(FindImage(panel, "卡牌金币徽章_2"), "Roguelike_UI_GoldIcon", Image.Type.Simple);

                RectTransform freeOverlay = FindRect(panel, "卡牌不可选遮罩_1");
                RectTransform paidOverlay = FindRect(panel, "卡牌不可选遮罩_2");
                Assert.NotNull(freeOverlay);
                Assert.NotNull(paidOverlay);
                Assert.IsFalse(freeOverlay.gameObject.activeSelf);
                Assert.IsTrue(paidOverlay.gameObject.activeSelf);
                Assert.That(paidOverlay.GetSiblingIndex(), Is.GreaterThan(FindText(panel, "卡牌状态_2").rectTransform.GetSiblingIndex()));
            }
        }

        [Test]
        public void CardAccentColorsFollowRewardCategoryAndDisabledState()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(51002);
            SetRewardChoices(game,
                new RoguelikeChoiceOption("weapon_magic_bolt_upgrade", "魔弹升级", "追踪魔弹升至 2 级", run => { }),
                new RoguelikeChoiceOption("passive_magnet_core", "磁力核心", "拾取范围提升", run => { }),
                new RoguelikeChoiceOption("paid_field_ration", "战地补给", "立即恢复生命", run => { }, 8));

            using (ChoiceFixture fixture = ChoiceFixture.Create())
            {
                RectTransform panel = FindRect(fixture.Root, "奖励选择面板");
                Image weaponRibbon = FindImage(panel, "卡牌顶部徽带_1");
                Image passiveRibbon = FindImage(panel, "卡牌顶部徽带_2");
                Image unaffordableRibbon = FindImage(panel, "卡牌顶部徽带_3");

                Assert.NotNull(weaponRibbon);
                Assert.NotNull(passiveRibbon);
                Assert.NotNull(unaffordableRibbon);
                Assert.Greater(weaponRibbon.color.r, weaponRibbon.color.b);
                Assert.Greater(passiveRibbon.color.b, passiveRibbon.color.r);
                Assert.Less(unaffordableRibbon.color.a, weaponRibbon.color.a);
                Assert.That(FindText(panel, "卡牌状态_3").text, Does.Contain("金币不足"));
            }
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

                GameObject panel = new GameObject("BattleChoiceUIStage51Test", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
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
