using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic.Tests
{
    public sealed class RoguelikeStage32SettlementGrowthTests
    {
        private const string MetaGoldKey = "Roguelike.MetaGold";
        private const string PanelFrameSprite = "Roguelike_UI_PanelFrame";
        private const string HeroPortraitSprite = "Roguelike_UI_HeroPortrait";
        private const string GoldIconSprite = "Roguelike_UI_GoldIcon";
        private const string SliderFillYellowSprite = "Slider11_Fill_Yellow";

        [SetUp]
        public void ClearMetaGold()
        {
            PlayerPrefs.DeleteKey(MetaGoldKey);
            PlayerPrefs.Save();
        }

        [Test]
        public void SettlementBanksRunGoldOnceAndReportsPermanentGrowth()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(32001);
            game.CurrentRun.AddGold(35);

            game.DebugForceDefeat();
            game.DebugForceDefeat();

            Assert.That(game.MetaGold, Is.EqualTo(35));
            Assert.That(game.PermanentAttackBonus, Is.EqualTo(0));
            Assert.That(game.PermanentGoldProgress, Is.EqualTo(35));
            Assert.That(game.PermanentGoldToNextAttack, Is.EqualTo(65));
            Assert.That(game.SettlementSummary, Does.Contain("旅途暂歇"));
            Assert.That(game.SettlementSummary, Does.Contain("距离下一点攻击还需 65 金币"));
        }

        [Test]
        public void RestartUsesPermanentGoldAttackBonus()
        {
            PlayerPrefs.SetInt(MetaGoldKey, 205);
            PlayerPrefs.Save();

            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(32002);

            Assert.That(game.PermanentAttackBonus, Is.EqualTo(2));
            Assert.That(game.PermanentGoldProgress, Is.EqualTo(5));
            Assert.That(game.PermanentGoldToNextAttack, Is.EqualTo(95));
            Assert.That(game.CurrentRun.Player.Stats.Attack, Is.EqualTo(14));
        }

        [Test]
        public void SettlementUiShowsResultStatsEarningsGrowthAndRestart()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(32003);
            game.CurrentRun.AddGold(118);
            game.DebugSkipRoom(2);
            game.DebugForceVictory();

            using (SettlementFixture fixture = SettlementFixture.Create())
            {
                InvokeWindowMethod(fixture.Window, "InternalRefresh");

                RectTransform panel = FindRect(fixture.Root, "结算成长面板");
                Assert.NotNull(panel);
                Assert.IsTrue(panel.gameObject.activeSelf);
                Assert.NotNull(FindRect(panel, "角色剪影区"));
                Assert.NotNull(FindRect(panel, "永久成长进度条"));
                AssertSprite(FindImage(panel, "结算成长面板"), PanelFrameSprite, Image.Type.Sliced);
                AssertSprite(FindImage(panel, "角色剪影头像"), HeroPortraitSprite, Image.Type.Simple);
                AssertSprite(FindImage(panel, "金币成长徽章"), GoldIconSprite, Image.Type.Simple);

                Assert.That(FindText(panel, "结算标题").text, Does.Contain("星辉凯旋"));
                Assert.That(FindText(panel, "结算数据").text, Does.Contain("击杀"));
                Assert.That(FindText(panel, "结算收益").text, Does.Contain("永久金币 118"));
                Assert.That(FindText(panel, "永久成长").text, Does.Contain("下局初始攻击 +1"));
                Assert.That(FindText(panel, "永久成长").text, Does.Contain("还需 82 金币"));

                RectTransform fill = FindRect(panel, "永久成长进度填充");
                Assert.That(fill.anchorMax.x, Is.EqualTo(0.18f).Within(0.01f));
                AssertSprite(fill.GetComponent<Image>(), SliderFillYellowSprite, Image.Type.Sliced);

                FindButton(panel, "重新开始按钮").onClick.Invoke();

                Assert.That(game.Phase, Is.EqualTo(RoguelikeGamePhase.Running));
                Assert.That(game.CurrentRun.Player.Stats.Attack, Is.EqualTo(13));
                Assert.IsFalse(panel.gameObject.activeSelf);
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

        private sealed class SettlementFixture : IDisposable
        {
            public object Window { get; private set; }
            public RectTransform Root { get; private set; }

            public static SettlementFixture Create()
            {
                Type settlementType = typeof(RoguelikeGame).Assembly.GetType("GameLogic.BattleSettlementUI");
                Assert.NotNull(settlementType);

                GameObject panel = new GameObject("BattleSettlementUITest", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
                RectTransform root = panel.GetComponent<RectTransform>();
                root.anchorMin = Vector2.zero;
                root.anchorMax = Vector2.one;
                root.offsetMin = Vector2.zero;
                root.offsetMax = Vector2.zero;

                object window = Activator.CreateInstance(settlementType);
                MethodInfo init = typeof(UIWindow).GetMethod("Init", BindingFlags.Instance | BindingFlags.Public);
                Assert.NotNull(init);
                init.Invoke(window, new object[] { "GameLogic.BattleSettlementUI", (int)UILayer.Top, false, "BattleSettlementUI", false, 10 });

                MethodInfo completed = typeof(UIWindow).GetMethod("Handle_Completed", BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.NotNull(completed);
                completed.Invoke(window, new object[] { panel });
                InvokeWindowMethod(window, "InternalCreate");
                InvokeWindowMethod(window, "InternalRefresh");

                return new SettlementFixture
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
