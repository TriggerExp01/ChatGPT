using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic.Tests
{
    public sealed class RoguelikeStage39TutorialHintTests
    {
        private const string PanelFrameSprite = "Roguelike_UI_PanelFrame";
        private const string BattleLightSprite = "Roguelike_UI_LightBand";

        [Test]
        public void StartHintExplainsMovementAutoAttackAndExperience()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(39001);

            Assert.That(game.OperationHint, Does.Contain("WASD"));
            Assert.That(game.OperationHint, Does.Contain("自动攻击"));
            Assert.That(game.OperationHint, Does.Contain("经验"));
            Assert.That(game.LastMessage, Does.Contain("生存挑战开始"));
        }

        [Test]
        public void HudAndControlUiShowCurrentTutorialHint()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(39002);

            using (HudFixture hud = HudFixture.Create())
            using (ControlFixture control = ControlFixture.Create())
            {
                InvokeWindowMethod(hud.Window, "InternalRefresh");
                InvokeWindowMethod(control.Window, "InternalUpdate");

                Assert.That(FindText(hud.Root, "战斗提示").text, Does.Contain("WASD"));
                Assert.That(FindText(hud.Root, "战斗提示").text, Does.Contain("自动攻击"));
                Assert.That(FindText(control.Root, "提示").text, Does.Contain("WASD"));
                Assert.That(FindText(control.Root, "提示").text, Does.Contain("经验"));
                AssertSprite(FindImage(control.Root, "操作提示"), PanelFrameSprite, Image.Type.Sliced);
                AssertSprite(FindImage(control.Root, "操作提示光带"), BattleLightSprite, Image.Type.Simple);
            }
        }

        [Test]
        public void RewardChoiceHintExplainsCardSelectionAndUnaffordableCards()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(39003);
            SetRewardChoices(game,
                new RoguelikeChoiceOption("paid_field_ration", "战地补给", "立即恢复生命", run => run.Player.Heal(35), 8),
                new RoguelikeChoiceOption("attack", "锋利武器", "攻击力 +3", run => run.Player.Stats.AddAttack(3)));

            using (ChoiceFixture fixture = ChoiceFixture.Create())
            {
                string prompt = FindText(fixture.Root, "奖励提示").text;

                Assert.That(game.OperationHint, Does.Contain("星辉卡牌"));
                Assert.That(prompt, Does.Contain("星辉卡牌"));
                Assert.That(prompt, Does.Contain("金币不足"));
            }
        }

        [Test]
        public void SettlementHintExplainsRestartAndPermanentGrowth()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(39004);
            game.CurrentRun.AddGold(30);
            game.DebugForceDefeat();

            using (SettlementFixture fixture = SettlementFixture.Create())
            {
                InvokeWindowMethod(fixture.Window, "InternalRefresh");

                string hint = FindText(fixture.Root, "结算提示").text;
                Assert.That(game.OperationHint, Does.Contain("重新开始"));
                Assert.That(hint, Does.Contain("永久金币"));
                Assert.That(hint, Does.Contain("下一局初始攻击"));

                FindButton(fixture.Root, "重新开始按钮").onClick.Invoke();

                Assert.That(game.Phase, Is.EqualTo(RoguelikeGamePhase.Running));
                Assert.That(game.OperationHint, Does.Contain("WASD"));
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

                GameObject stage = GameObject.Find("Roguelike2DSurvivalStage");
                if (stage != null)
                {
                    UnityEngine.Object.DestroyImmediate(stage);
                }
            }

            protected static (object window, RectTransform root) CreateWindow(string typeName, UILayer layer, string assetName)
            {
                Type windowType = typeof(RoguelikeGame).Assembly.GetType(typeName);
                Assert.NotNull(windowType);

                GameObject panel = new GameObject($"{assetName}Test", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
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

        private sealed class ChoiceFixture : WindowFixture
        {
            public static ChoiceFixture Create()
            {
                (object window, RectTransform root) = CreateWindow("GameLogic.BattleChoiceUI", UILayer.Top, "BattleChoiceUI");
                return new ChoiceFixture { Window = window, Root = root };
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
