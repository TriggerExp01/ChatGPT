using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic.Tests
{
    public sealed class RoguelikeStage30HudPolishTests
    {
        private const string PanelFrameSprite = "Roguelike_UI_PanelFrame";
        private const string CardFrameSprite = "Roguelike_UI_CardFrame";
        private const string HeroPortraitSprite = "Roguelike_UI_HeroPortrait";
        private const string WeaponIconSprite = "Roguelike_UI_WeaponIcon";
        private const string RelicIconSprite = "Roguelike_UI_RelicIcon";
        private const string MagicBoltIconSprite = "Roguelike_UI_Icon_MagicBolt";
        private const string GoldIconSprite = "Roguelike_UI_GoldIcon";
        private const string SliderFrameSprite = "Roguelike_UI_SliderFrame";
        private const string SliderFillRedSprite = "Roguelike_UI_SliderFill_Red";
        private const string SliderFillBlueSprite = "Roguelike_UI_SliderFill_Blue";
        private const string SliderFillYellowSprite = "Roguelike_UI_SliderFill_Yellow";
        private const string LightBandSprite = "Roguelike_UI_LightBand";

        [Test]
        public void HudCreatesLowObstructionAnimeFantasyLayout()
        {
            using (HudFixture fixture = HudFixture.Create())
            {
                RectTransform hero = FindRect(fixture.Root, "角色状态面板");
                RectTransform pressure = FindRect(fixture.Root, "时间压力面板");
                RectTransform build = FindRect(fixture.Root, "构筑槽面板");
                RectTransform hint = FindRect(fixture.Root, "战斗提示面板");

                Assert.That(hero.anchorMax.y, Is.GreaterThan(0.95f));
                Assert.That(hero.anchorMax.x, Is.LessThan(0.36f));
                Assert.That(pressure.anchorMin.x, Is.GreaterThan(0.35f));
                Assert.That(pressure.anchorMax.x, Is.LessThan(0.65f));
                Assert.That(build.anchorMin.x, Is.GreaterThan(0.62f));
                Assert.That(hint.anchorMax.y, Is.LessThan(0.12f));
                Assert.NotNull(FindRect(hero, "角色头像"));
            }
        }

        [Test]
        public void HudRefreshShowsCharacterPressureAndBuildSummary()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(30001);
            game.DebugAddGold(12);

            using (HudFixture fixture = HudFixture.Create())
            {
                InvokeWindowMethod(fixture.Window, "InternalRefresh");

                string hero = FindText(fixture.Root, "角色数值").text;
                string pressure = FindText(fixture.Root, "时间压力").text;
                string build = FindText(fixture.Root, "构筑摘要").text;
                string hint = FindText(fixture.Root, "战斗提示").text;

                Assert.That(hero, Does.Contain("星辉旅人"));
                Assert.That(hero, Does.Contain("Lv.1"));
                Assert.That(pressure, Does.Contain("敌人"));
                Assert.That(pressure, Does.Contain("击杀"));
                Assert.That(build, Does.Contain("武器"));
                Assert.That(build, Does.Contain("被动"));
                Assert.That(hint, Does.Contain("生存挑战开始"));
            }
        }

        [Test]
        public void HudBuildPanelContainsWeaponAndPassiveIconSlots()
        {
            using (HudFixture fixture = HudFixture.Create())
            {
                RectTransform build = FindRect(fixture.Root, "构筑槽面板");

                for (int i = 1; i <= 4; i++)
                {
                    Assert.NotNull(FindRect(build, $"武器槽_{i}"));
                    Assert.NotNull(FindRect(build, $"被动槽_{i}"));
                }
            }
        }

        [Test]
        public void HudUsesRoguelikeUiSpriteAssets()
        {
            using (HudFixture fixture = HudFixture.Create())
            {
                AssertSprite(FindImage(fixture.Root, "角色状态面板"), PanelFrameSprite, Image.Type.Sliced);
                AssertSprite(FindImage(fixture.Root, "角色头像"), HeroPortraitSprite, Image.Type.Simple);
                AssertSprite(FindImage(fixture.Root, "头像星辉"), LightBandSprite, Image.Type.Simple);
                AssertSprite(FindImage(fixture.Root, "时间压力高光"), LightBandSprite, Image.Type.Simple);
                AssertSprite(FindSliderBackground(fixture.Root, "生命条"), SliderFrameSprite, Image.Type.Sliced);
                AssertSprite(FindSliderFill(fixture.Root, "生命条"), SliderFillRedSprite, Image.Type.Sliced);
                AssertSprite(FindSliderBackground(fixture.Root, "经验条"), SliderFrameSprite, Image.Type.Sliced);
                AssertSprite(FindSliderFill(fixture.Root, "经验条"), SliderFillBlueSprite, Image.Type.Sliced);
                AssertSprite(FindImage(fixture.Root, "武器槽_1"), MagicBoltIconSprite, Image.Type.Simple);
                AssertSprite(FindImage(fixture.Root, "被动槽_1"), RelicIconSprite, Image.Type.Simple);
            }
        }

        [Test]
        public void RoguelikeUiSpriteAssetsExistInAtlasFolder()
        {
            string atlasPath = Path.Combine(Application.dataPath, "AssetRaw", "UIRaw", "Atlas", "Roguelike");

            Assert.IsTrue(File.Exists(Path.Combine(atlasPath, PanelFrameSprite + ".png")));
            Assert.IsTrue(File.Exists(Path.Combine(atlasPath, CardFrameSprite + ".png")));
            Assert.IsTrue(File.Exists(Path.Combine(atlasPath, HeroPortraitSprite + ".png")));
            Assert.IsTrue(File.Exists(Path.Combine(atlasPath, WeaponIconSprite + ".png")));
            Assert.IsTrue(File.Exists(Path.Combine(atlasPath, RelicIconSprite + ".png")));
            Assert.IsTrue(File.Exists(Path.Combine(atlasPath, GoldIconSprite + ".png")));
            Assert.IsTrue(File.Exists(Path.Combine(atlasPath, SliderFrameSprite + ".png")));
            Assert.IsTrue(File.Exists(Path.Combine(atlasPath, SliderFillRedSprite + ".png")));
            Assert.IsTrue(File.Exists(Path.Combine(atlasPath, SliderFillBlueSprite + ".png")));
            Assert.IsTrue(File.Exists(Path.Combine(atlasPath, SliderFillYellowSprite + ".png")));
            Assert.IsTrue(File.Exists(Path.Combine(atlasPath, LightBandSprite + ".png")));
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

        private static Image FindSliderFill(Transform root, string name)
        {
            RectTransform sliderRoot = FindRect(root, name);
            RectTransform fill = sliderRoot != null ? FindRect(sliderRoot, "Fill") : null;
            return fill != null ? fill.GetComponent<Image>() : null;
        }

        private static Image FindSliderBackground(Transform root, string name)
        {
            RectTransform sliderRoot = FindRect(root, name);
            return sliderRoot != null ? sliderRoot.GetComponent<Image>() : null;
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

        private sealed class HudFixture : IDisposable
        {
            public object Window { get; private set; }
            public RectTransform Root { get; private set; }

            public static HudFixture Create()
            {
                Type hudType = typeof(RoguelikeGame).Assembly.GetType("GameLogic.BattleHudUI");
                Assert.NotNull(hudType);

                GameObject panel = new GameObject("BattleHudUITest", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
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
