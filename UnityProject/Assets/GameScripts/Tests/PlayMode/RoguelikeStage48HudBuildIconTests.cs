using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic.Tests
{
    public sealed class RoguelikeStage48HudBuildIconTests
    {
        [Test]
        public void Stage48HudBuildSlotsUseDedicatedWeaponIconsInShowcase()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.DebugPopulateShowcaseRuntimeObjects(48001);

            using (HudFixture fixture = HudFixture.Create())
            {
                InvokeWindowMethod(fixture.Window, "InternalRefresh");

                AssertSprite(FindImage(fixture.Root, "武器槽_1"), "Roguelike_UI_Icon_MagicBolt");
                AssertSprite(FindImage(fixture.Root, "武器槽_2"), "Roguelike_UI_Icon_SpinningBlade");
                AssertSprite(FindImage(fixture.Root, "武器槽_3"), "Roguelike_UI_Icon_PiercingDart");
                AssertSprite(FindImage(fixture.Root, "武器槽_4"), "Roguelike_UI_Icon_StarRingPulse");
            }
        }

        [Test]
        public void Stage48HudBuildSlotsUseDedicatedRelicIconsAndFallbacks()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(48002);
            game.CurrentRun.AddRelic(new RoguelikeRelicTemplate("passive_vital_sigil", "生息符文", "最大生命提升", null));
            game.CurrentRun.AddRelic(new RoguelikeRelicTemplate("passive_eagle_eye", "鹰眼纹章", "攻击与暴击提升", null));
            game.CurrentRun.AddRelic(new RoguelikeRelicTemplate("passive_magnet_core", "磁力核心", "拾取范围提升", null));

            using (HudFixture fixture = HudFixture.Create())
            {
                InvokeWindowMethod(fixture.Window, "InternalRefresh");

                AssertSprite(FindImage(fixture.Root, "被动槽_1"), "Roguelike_UI_Icon_RelicGrowth");
                AssertSprite(FindImage(fixture.Root, "被动槽_2"), "Roguelike_UI_Icon_RelicPower");
                AssertSprite(FindImage(fixture.Root, "被动槽_3"), "Roguelike_UI_Icon_RelicUtility");
                AssertSprite(FindImage(fixture.Root, "被动槽_4"), "Roguelike_UI_RelicIcon");
            }
        }

        [Test]
        public void Stage48FactoryResolvesBuildIconAddresses()
        {
            Assert.That(ResolveWeaponIcon(RoguelikeWeaponType.MagicBolt), Is.EqualTo("Roguelike_UI_Icon_MagicBolt"));
            Assert.That(ResolveWeaponIcon(RoguelikeWeaponType.SpinningBlade), Is.EqualTo("Roguelike_UI_Icon_SpinningBlade"));
            Assert.That(ResolveWeaponIcon(RoguelikeWeaponType.PiercingDart), Is.EqualTo("Roguelike_UI_Icon_PiercingDart"));
            Assert.That(ResolveWeaponIcon(RoguelikeWeaponType.StarRingPulse), Is.EqualTo("Roguelike_UI_Icon_StarRingPulse"));
            Assert.That(ResolveRelicIcon("passive_starlight_soles"), Is.EqualTo("Roguelike_UI_Icon_RelicGrowth"));
            Assert.That(ResolveRelicIcon("passive_focus_charm"), Is.EqualTo("Roguelike_UI_Icon_RelicPower"));
            Assert.That(ResolveRelicIcon("passive_magnet_core"), Is.EqualTo("Roguelike_UI_Icon_RelicUtility"));
        }

        private static Image FindImage(Transform root, string name)
        {
            RectTransform rect = FindRect(root, name);
            return rect != null ? rect.GetComponent<Image>() : null;
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

        private static void AssertSprite(Image image, string spriteName)
        {
            Assert.NotNull(image);
            Assert.NotNull(image.sprite);
            Assert.That(image.sprite.name, Is.EqualTo(spriteName));
            Assert.That(image.type, Is.EqualTo(Image.Type.Simple));
            Assert.IsTrue(image.preserveAspect);
        }

        private static string ResolveWeaponIcon(RoguelikeWeaponType weaponType)
        {
            Type factoryType = typeof(RoguelikeGame).Assembly.GetType("GameLogic.RoguelikeUIFactory");
            Assert.NotNull(factoryType);
            MethodInfo method = factoryType.GetMethod("ResolveWeaponIconSprite", BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(method);
            return method.Invoke(null, new object[] { weaponType }) as string;
        }

        private static string ResolveRelicIcon(string relicId)
        {
            Type factoryType = typeof(RoguelikeGame).Assembly.GetType("GameLogic.RoguelikeUIFactory");
            Assert.NotNull(factoryType);
            MethodInfo method = factoryType.GetMethod("ResolveRelicIconSprite", BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(method);
            return method.Invoke(null, new object[] { relicId }) as string;
        }

        private static void InvokeWindowMethod(object window, string methodName)
        {
            MethodInfo method = typeof(UIWindow).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(method);
            method.Invoke(window, null);
        }

        private sealed class HudFixture : IDisposable
        {
            public object Window { get; private set; }
            public RectTransform Root { get; private set; }

            public static HudFixture Create()
            {
                Type hudType = typeof(RoguelikeGame).Assembly.GetType("GameLogic.BattleHudUI");
                Assert.NotNull(hudType);

                GameObject panel = new GameObject("BattleHudUIStage48Test", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
                RectTransform root = panel.GetComponent<RectTransform>();
                root.anchorMin = Vector2.zero;
                root.anchorMax = Vector2.one;
                root.offsetMin = Vector2.zero;
                root.offsetMax = Vector2.zero;

                object window = Activator.CreateInstance(hudType);
                MethodInfo init = typeof(UIWindow).GetMethod("Init", BindingFlags.Instance | BindingFlags.Public);
                Assert.NotNull(init);
                init.Invoke(window, new object[] { "GameLogic.BattleHudUI", (int)UILayer.UI, false, "BattleHudUI", false, 10 });

                MethodInfo completed = typeof(UIWindow).GetMethod("Handle_Completed", BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.NotNull(completed);
                completed.Invoke(window, new object[] { panel });
                InvokeWindowMethod(window, "InternalCreate");
                InvokeWindowMethod(window, "InternalRefresh");

                return new HudFixture
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

                GameObject stage = GameObject.Find("Roguelike2DSurvivalStage");
                if (stage != null)
                {
                    UnityEngine.Object.DestroyImmediate(stage);
                }
            }
        }
    }
}
