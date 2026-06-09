using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic.Tests
{
    public sealed class RoguelikeStage58UiSlotFrameBindingTests
    {
        [Test]
        public void HudBuildSlotsUseSystemG6FramesAroundFormalIcons()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.DebugPopulateShowcaseRuntimeObjects(58001);

            using (HudFixture fixture = HudFixture.Create())
            {
                InvokeWindowMethod(fixture.Window, "InternalRefresh");

                AssertSprite(FindImage(fixture.Root, "武器槽_1框"), "Roguelike_StarUI_SlotFrame", Image.Type.Sliced);
                AssertSprite(FindImage(fixture.Root, "武器槽_1"), "Roguelike_UI_Icon_MagicBolt", Image.Type.Simple);
                AssertSprite(FindImage(fixture.Root, "武器槽_2框"), "Roguelike_StarUI_SlotFrame", Image.Type.Sliced);
                AssertSprite(FindImage(fixture.Root, "武器槽_2"), "Roguelike_UI_Icon_SpinningBlade", Image.Type.Simple);
                AssertSprite(FindImage(fixture.Root, "被动槽_1框"), "Roguelike_StarUI_SlotFrame", Image.Type.Sliced);
            }
        }

        [Test]
        public void RewardCardIconPedestalUsesSystemG6SlotFrame()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(58002);
            SetRewardChoices(game,
                new RoguelikeChoiceOption("weapon_magic_bolt_upgrade", "魔弹升级", "追踪魔弹升至 2 级", run => { }),
                new RoguelikeChoiceOption("passive_magnet_core", "磁力核心", "拾取范围提升", run => { }),
                new RoguelikeChoiceOption("paid_field_ration", "战地补给", "立即恢复生命", run => { }, 8));

            using (ChoiceFixture fixture = ChoiceFixture.Create())
            {
                InvokeWindowMethod(fixture.Window, "InternalRefresh");

                AssertSprite(FindImage(fixture.Root, "奖励图标底座_1"), "Roguelike_StarUI_SlotFrame", Image.Type.Sliced);
                AssertSprite(FindImage(fixture.Root, "奖励图标底座_2"), "Roguelike_StarUI_SlotFrame", Image.Type.Sliced);
                AssertSprite(FindImage(fixture.Root, "奖励图标底座_3"), "Roguelike_StarUI_SlotFrame", Image.Type.Sliced);
            }
        }

        private static void SetRewardChoices(RoguelikeGame game, params RoguelikeChoiceOption[] choices)
        {
            FieldInfo field = typeof(RoguelikeGame).GetField("_rewardOptions", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            var options = (System.Collections.Generic.List<RoguelikeChoiceOption>)field.GetValue(game);
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

        private static void InvokeWindowMethod(object window, string methodName)
        {
            MethodInfo method = window.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            Assert.NotNull(method);
            method.Invoke(window, null);
        }

        private static void AssertSprite(Image image, string spriteName, Image.Type imageType)
        {
            Assert.NotNull(image);
            Assert.NotNull(image.sprite);
            Assert.That(image.sprite.name, Is.EqualTo(spriteName));
            Assert.That(image.type, Is.EqualTo(imageType));
        }

        private sealed class HudFixture : IDisposable
        {
            public RectTransform Root { get; private set; }
            public object Window { get; private set; }

            public static HudFixture Create()
            {
                GameObject rootObject = new GameObject("Stage58HudRoot", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
                RectTransform root = rootObject.GetComponent<RectTransform>();
                root.anchorMin = Vector2.zero;
                root.anchorMax = Vector2.one;
                root.offsetMin = Vector2.zero;
                root.offsetMax = Vector2.zero;

                Type type = typeof(RoguelikeGame).Assembly.GetType("GameLogic.BattleHudUI");
                Assert.NotNull(type);
                object window = Activator.CreateInstance(type);
                InitializeWindow(window, rootObject, "GameLogic.BattleHudUI", "BattleHudUI");
                return new HudFixture { Root = root, Window = window };
            }

            public void Dispose()
            {
                if (Root != null)
                {
                    UnityEngine.Object.DestroyImmediate(Root.gameObject);
                }
            }
        }

        private sealed class ChoiceFixture : IDisposable
        {
            public RectTransform Root { get; private set; }
            public object Window { get; private set; }

            public static ChoiceFixture Create()
            {
                GameObject rootObject = new GameObject("Stage58ChoiceRoot", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
                RectTransform root = rootObject.GetComponent<RectTransform>();
                root.anchorMin = Vector2.zero;
                root.anchorMax = Vector2.one;
                root.offsetMin = Vector2.zero;
                root.offsetMax = Vector2.zero;

                Type type = typeof(RoguelikeGame).Assembly.GetType("GameLogic.BattleChoiceUI");
                Assert.NotNull(type);
                object window = Activator.CreateInstance(type);
                InitializeWindow(window, rootObject, "GameLogic.BattleChoiceUI", "BattleChoiceUI");
                return new ChoiceFixture { Root = root, Window = window };
            }

            public void Dispose()
            {
                if (Root != null)
                {
                    UnityEngine.Object.DestroyImmediate(Root.gameObject);
                }
            }
        }

        private static void InitializeWindow(object window, GameObject rootObject, string windowName, string assetName)
        {
            MethodInfo init = typeof(UIWindow).GetMethod("Init", BindingFlags.Instance | BindingFlags.Public);
            Assert.NotNull(init);
            init.Invoke(window, new object[] { windowName, (int)UILayer.UI, false, assetName, false, 10 });

            MethodInfo completed = typeof(UIWindow).GetMethod("Handle_Completed", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(completed);
            completed.Invoke(window, new object[] { rootObject });
            InvokeWindowMethod(window, "InternalCreate");
            InvokeWindowMethod(window, "InternalRefresh");
        }
    }
}
