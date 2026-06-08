using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic.Tests
{
    public sealed class RoguelikeStage47ShowcaseScreenshotModeTests
    {
        [Test]
        public void ShowcaseScreenshotModeDefaultsOffAndShowcaseEntryEnablesIt()
        {
            RoguelikeGame game = RoguelikeGame.Instance;

            game.StartNewRun(47001);

            Assert.IsFalse(game.IsShowcaseScreenshotMode);
            Assert.IsTrue(game.ShouldShowOperationHintUi);
            Assert.That(game.CurrentHudPanelAlpha, Is.EqualTo(1f));

            game.DebugPopulateShowcaseRuntimeObjects(47001);

            Assert.IsTrue(game.IsShowcaseScreenshotMode);
            Assert.IsFalse(game.ShouldShowOperationHintUi);
            Assert.That(game.CurrentHudPanelAlpha, Is.EqualTo(RoguelikeGame.ShowcaseScreenshotHudAlpha));

            game.StartNewRun(47002);

            Assert.IsFalse(game.IsShowcaseScreenshotMode);
            Assert.IsTrue(game.ShouldShowOperationHintUi);
            Assert.That(game.CurrentHudPanelAlpha, Is.EqualTo(1f));
        }

        [Test]
        public void ShowcaseScreenshotModeHidesHintPanelsAndSoftensHud()
        {
            DestroyOldStage();

            RoguelikeGame game = RoguelikeGame.Instance;
            game.DebugPopulateShowcaseRuntimeObjects(47003);

            using (HudFixture hud = HudFixture.Create())
            using (ControlFixture control = ControlFixture.Create())
            {
                InvokeWindowMethod(hud.Window, "InternalRefresh");
                InvokeWindowMethod(control.Window, "InternalRefresh");

                Assert.IsFalse(FindRect(hud.Root, "战斗提示面板").gameObject.activeSelf);
                Assert.IsFalse(FindRect(control.Root, "操作提示").gameObject.activeSelf);
                Assert.That(GetCanvasGroupAlpha(hud.Root, "角色状态面板"), Is.EqualTo(RoguelikeGame.ShowcaseScreenshotHudAlpha).Within(0.001f));
                Assert.That(GetCanvasGroupAlpha(hud.Root, "时间压力面板"), Is.EqualTo(RoguelikeGame.ShowcaseScreenshotHudAlpha).Within(0.001f));
                Assert.That(GetCanvasGroupAlpha(hud.Root, "构筑槽面板"), Is.EqualTo(RoguelikeGame.ShowcaseScreenshotHudAlpha).Within(0.001f));

                game.DebugSetShowcaseScreenshotMode(false);
                InvokeWindowMethod(hud.Window, "InternalRefresh");
                InvokeWindowMethod(control.Window, "InternalRefresh");

                Assert.IsTrue(FindRect(hud.Root, "战斗提示面板").gameObject.activeSelf);
                Assert.IsTrue(FindRect(control.Root, "操作提示").gameObject.activeSelf);
                Assert.That(GetCanvasGroupAlpha(hud.Root, "角色状态面板"), Is.EqualTo(1f).Within(0.001f));
                Assert.That(FindText(control.Root, "提示").text, Does.Contain("强化构筑"));
            }
        }

        [Test]
        public void ShowcaseScreenshotModeKeepsSettlementScreenshotClean()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(47004);
            game.DebugSetShowcaseScreenshotMode(true);
            game.CurrentRun.AddGold(30);
            game.DebugForceDefeat();

            using (SettlementFixture fixture = SettlementFixture.Create())
            {
                InvokeWindowMethod(fixture.Window, "InternalRefresh");

                Text hint = FindText(fixture.Root, "结算提示");
                Assert.NotNull(hint);
                Assert.IsFalse(hint.gameObject.activeSelf);
                Assert.That(hint.text, Is.Empty);

                game.DebugSetShowcaseScreenshotMode(false);
                InvokeWindowMethod(fixture.Window, "InternalRefresh");

                Assert.IsTrue(hint.gameObject.activeSelf);
                Assert.That(hint.text, Does.Contain("永久金币"));
            }
        }

        private static void DestroyOldStage()
        {
            GameObject oldStage = GameObject.Find("Roguelike2DSurvivalStage");
            if (oldStage != null)
            {
                UnityEngine.Object.DestroyImmediate(oldStage);
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

        private static float GetCanvasGroupAlpha(Transform root, string name)
        {
            RectTransform rect = FindRect(root, name);
            Assert.NotNull(rect);
            CanvasGroup canvasGroup = rect.GetComponent<CanvasGroup>();
            Assert.NotNull(canvasGroup);
            return canvasGroup.alpha;
        }

        private static void InvokeWindowMethod(object window, string methodName)
        {
            MethodInfo method = typeof(UIWindow).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(method);
            method.Invoke(window, null);
        }

        private abstract class WindowFixture : IDisposable
        {
            public object Window { get; protected set; }
            public RectTransform Root { get; protected set; }

            public void Dispose()
            {
                if (Window != null)
                {
                    MethodInfo destroy = typeof(UIWindow).GetMethod("InternalDestroy", BindingFlags.Instance | BindingFlags.NonPublic);
                    destroy?.Invoke(Window, new object[] { true });
                    Window = null;
                }

                DestroyOldStage();
            }

            protected static (object window, RectTransform root) CreateWindow(string typeName, UILayer layer, string assetName)
            {
                Type windowType = typeof(RoguelikeGame).Assembly.GetType(typeName);
                Assert.NotNull(windowType);

                GameObject panel = new GameObject($"{assetName}Stage47Test", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
                RectTransform root = panel.GetComponent<RectTransform>();
                root.anchorMin = Vector2.zero;
                root.anchorMax = Vector2.one;
                root.offsetMin = Vector2.zero;
                root.offsetMax = Vector2.zero;

                object window = Activator.CreateInstance(windowType);
                MethodInfo init = typeof(UIWindow).GetMethod("Init", BindingFlags.Instance | BindingFlags.Public);
                Assert.NotNull(init);
                init.Invoke(window, new object[] { typeName, (int)layer, false, assetName, false, 10 });

                MethodInfo completed = typeof(UIWindow).GetMethod("Handle_Completed", BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.NotNull(completed);
                completed.Invoke(window, new object[] { panel });
                InvokeWindowMethod(window, "InternalCreate");
                InvokeWindowMethod(window, "InternalRefresh");

                return (window, root);
            }
        }

        private sealed class HudFixture : WindowFixture
        {
            public static HudFixture Create()
            {
                (object window, RectTransform root) = CreateWindow("GameLogic.BattleHudUI", UILayer.UI, "BattleHudUI");
                return new HudFixture { Window = window, Root = root };
            }
        }

        private sealed class ControlFixture : WindowFixture
        {
            public static ControlFixture Create()
            {
                (object window, RectTransform root) = CreateWindow("GameLogic.BattleControlUI", UILayer.Top, "BattleControlUI");
                return new ControlFixture { Window = window, Root = root };
            }
        }

        private sealed class SettlementFixture : WindowFixture
        {
            public static SettlementFixture Create()
            {
                (object window, RectTransform root) = CreateWindow("GameLogic.BattleSettlementUI", UILayer.Top, "BattleSettlementUI");
                return new SettlementFixture { Window = window, Root = root };
            }
        }
    }
}
