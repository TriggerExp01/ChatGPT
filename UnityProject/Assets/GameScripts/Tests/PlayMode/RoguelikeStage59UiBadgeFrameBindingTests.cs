using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic.Tests
{
    public sealed class RoguelikeStage59UiBadgeFrameBindingTests
    {
        [Test]
        public void RewardCostBadgeUsesSystemG6SlotFrameAroundGoldIcon()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(59001);
            SetRewardChoices(game,
                new RoguelikeChoiceOption("weapon_magic_bolt_upgrade", "魔弹升级", "追踪魔弹升至 2 级", run => { }),
                new RoguelikeChoiceOption("paid_field_ration", "战地补给", "立即恢复生命", run => { }, 8),
                new RoguelikeChoiceOption("passive_magnet_core", "磁力核心", "拾取范围提升", run => { }));

            using (WindowFixture fixture = WindowFixture.Create("GameLogic.BattleChoiceUI", "BattleChoiceUI"))
            {
                InvokeWindowMethod(fixture.Window, "InternalRefresh");

                Image freeFrame = FindImage(fixture.Root, "卡牌金币徽章_1框");
                Image paidFrame = FindImage(fixture.Root, "卡牌金币徽章_2框");
                Assert.NotNull(freeFrame);
                Assert.NotNull(paidFrame);
                Assert.IsFalse(freeFrame.gameObject.activeSelf);
                Assert.IsTrue(paidFrame.gameObject.activeSelf);
                AssertSprite(paidFrame, "Roguelike_StarUI_SlotFrame", Image.Type.Sliced);
                AssertSprite(FindImage(fixture.Root, "卡牌金币徽章_2"), "Roguelike_UI_GoldIcon", Image.Type.Simple);
            }
        }

        [Test]
        public void SettlementGrowthBadgeUsesSystemG6SlotFrameAroundGoldIcon()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(59002);
            game.CurrentRun.AddGold(60);
            game.DebugForceDefeat();

            using (WindowFixture fixture = WindowFixture.Create("GameLogic.BattleSettlementUI", "BattleSettlementUI"))
            {
                InvokeWindowMethod(fixture.Window, "InternalRefresh");

                AssertSprite(FindImage(fixture.Root, "金币成长徽章框"), "Roguelike_StarUI_SlotFrame", Image.Type.Sliced);
                AssertSprite(FindImage(fixture.Root, "金币成长徽章"), "Roguelike_UI_GoldIcon", Image.Type.Simple);
            }
        }

        private static void SetRewardChoices(RoguelikeGame game, params RoguelikeChoiceOption[] choices)
        {
            FieldInfo field = typeof(RoguelikeGame).GetField("_rewardOptions", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            var options = (List<RoguelikeChoiceOption>)field.GetValue(game);
            options.Clear();
            options.AddRange(choices);

            PropertyInfo property = typeof(RoguelikeGame).GetProperty("Phase", BindingFlags.Instance | BindingFlags.Public);
            Assert.NotNull(property);
            property.SetValue(game, RoguelikeGamePhase.RewardChoice);
        }

        private static Image FindImage(Transform root, string name)
        {
            Transform[] children = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i].name == name)
                {
                    return children[i].GetComponent<Image>();
                }
            }

            return null;
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
            MethodInfo method = window.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            Assert.NotNull(method);
            method.Invoke(window, null);
        }

        private sealed class WindowFixture : IDisposable
        {
            public RectTransform Root { get; private set; }
            public object Window { get; private set; }

            public static WindowFixture Create(string windowName, string assetName)
            {
                GameObject rootObject = new GameObject(assetName + "Stage59Root", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
                RectTransform root = rootObject.GetComponent<RectTransform>();
                root.anchorMin = Vector2.zero;
                root.anchorMax = Vector2.one;
                root.offsetMin = Vector2.zero;
                root.offsetMax = Vector2.zero;

                Type type = typeof(RoguelikeGame).Assembly.GetType(windowName);
                Assert.NotNull(type);
                object window = Activator.CreateInstance(type);

                MethodInfo init = typeof(UIWindow).GetMethod("Init", BindingFlags.Instance | BindingFlags.Public);
                Assert.NotNull(init);
                init.Invoke(window, new object[] { windowName, (int)UILayer.UI, false, assetName, false, 10 });

                MethodInfo completed = typeof(UIWindow).GetMethod("Handle_Completed", BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.NotNull(completed);
                completed.Invoke(window, new object[] { rootObject });
                InvokeWindowMethod(window, "InternalCreate");
                InvokeWindowMethod(window, "InternalRefresh");

                return new WindowFixture { Root = root, Window = window };
            }

            public void Dispose()
            {
                if (Root != null)
                {
                    UnityEngine.Object.DestroyImmediate(Root.gameObject);
                    Root = null;
                }
            }
        }
    }
}
