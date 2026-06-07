using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic.Tests
{
    public sealed class RoguelikeStage30HudPolishTests
    {
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
