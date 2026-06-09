using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic.Tests
{
    public sealed class RoguelikeStage52ExternalUiAssetTests
    {
        private const string ExternalPanelSprite = "Roguelike_StarUI_PanelFrame";
        private const string ExternalCardSprite = "Roguelike_StarUI_CardFrame";
        private const string ExternalButtonSprite = "Roguelike_StarUI_ButtonFrame";
        private const string ExternalDividerSprite = "Roguelike_StarUI_DividerLine";

        [Test]
        public void KenneyFantasyUiAssetsAreImportedAndRegistered()
        {
            string atlasPath = Path.Combine(Application.dataPath, "AssetRaw", "UIRaw", "Atlas", "Roguelike");
            string[] requiredAssets =
            {
                ExternalPanelSprite + ".png",
                ExternalCardSprite + ".png",
                ExternalButtonSprite + ".png",
                ExternalDividerSprite + ".png",
            };

            foreach (string asset in requiredAssets)
            {
                Assert.IsTrue(File.Exists(Path.Combine(atlasPath, asset)), "Missing external UI asset: " + asset);
            }

            string manifest = File.ReadAllText(GetVisualAssetManifestPath());
            Assert.That(manifest, Does.Contain("Kenney Fantasy UI Borders"));
            Assert.That(manifest, Does.Contain("https://kenney.nl/assets/fantasy-ui-borders"));
            Assert.That(manifest, Does.Contain("https://creativecommons.org/publicdomain/zero/1.0/"));
            Assert.That(manifest, Does.Contain("Roguelike_StarUI_PanelFrame.png"));
            Assert.That(manifest, Does.Contain("Roguelike_StarUI_CardFrame.png"));
            Assert.That(manifest, Does.Contain("Roguelike_StarUI_ButtonFrame.png"));
            Assert.That(manifest, Does.Contain("Roguelike_StarUI_DividerLine.png"));
        }

        [Test]
        public void UiFactoryUsesExternalKenneySpritesForCoreFrames()
        {
            Assert.That(GetUiFactoryConstant("PanelFrameSprite"), Is.EqualTo(ExternalPanelSprite));
            Assert.That(GetUiFactoryConstant("CardFrameSprite"), Is.EqualTo(ExternalCardSprite));
            Assert.That(GetUiFactoryConstant("ButtonFrameSprite"), Is.EqualTo(ExternalButtonSprite));
            Assert.That(GetUiFactoryConstant("DividerFadeSprite"), Is.EqualTo(ExternalDividerSprite));
        }

        [Test]
        public void HudControlChoiceAndSettlementUseExternalUiSprites()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(52001);
            SetRewardChoices(game,
                new RoguelikeChoiceOption("weapon_magic_bolt_upgrade", "魔弹升级", "追踪魔弹升至 2 级", run => { }),
                new RoguelikeChoiceOption("passive_magnet_core", "磁力核心", "拾取范围提升", run => { }),
                new RoguelikeChoiceOption("paid_field_ration", "战地补给", "立即恢复生命", run => { }, 8));

            using (WindowFixture hud = WindowFixture.Create("GameLogic.BattleHudUI", UILayer.UI, "BattleHudUI"))
            {
                AssertSprite(FindImage(hud.Root, "角色状态面板"), ExternalPanelSprite, Image.Type.Sliced);
                AssertSprite(FindImage(hud.Root, "构筑槽面板"), ExternalPanelSprite, Image.Type.Sliced);
                AssertSprite(FindImage(hud.Root, "角色状态外部装饰线"), ExternalDividerSprite, Image.Type.Simple);
            }

            using (WindowFixture control = WindowFixture.Create("GameLogic.BattleControlUI", UILayer.UI, "BattleControlUI"))
            {
                AssertSprite(FindImage(control.Root, "操作提示"), ExternalPanelSprite, Image.Type.Sliced);
                AssertSprite(FindImage(control.Root, "操作提示外部装饰线"), ExternalDividerSprite, Image.Type.Simple);
                AssertSprite(FindImage(control.Root, "暂停"), ExternalButtonSprite, Image.Type.Sliced);
                AssertSprite(FindImage(control.Root, "重开"), ExternalButtonSprite, Image.Type.Sliced);
            }

            using (WindowFixture choice = WindowFixture.Create("GameLogic.BattleChoiceUI", UILayer.Top, "BattleChoiceUI"))
            {
                AssertSprite(FindImage(choice.Root, "奖励选择面板"), ExternalPanelSprite, Image.Type.Sliced);
                AssertSprite(FindImage(choice.Root, "奖励卡_1"), ExternalCardSprite, Image.Type.Sliced);
                AssertSprite(FindImage(choice.Root, "奖励面板外部装饰线"), ExternalDividerSprite, Image.Type.Simple);
            }

            game.DebugForceVictory();
            using (WindowFixture settlement = WindowFixture.Create("GameLogic.BattleSettlementUI", UILayer.Top, "BattleSettlementUI"))
            {
                AssertSprite(FindImage(settlement.Root, "结算成长面板"), ExternalPanelSprite, Image.Type.Sliced);
                AssertSprite(FindImage(settlement.Root, "结算外部标题线"), ExternalDividerSprite, Image.Type.Simple);
                AssertSprite(FindImage(settlement.Root, "重新开始按钮"), ExternalButtonSprite, Image.Type.Sliced);
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

        private static string GetVisualAssetManifestPath()
        {
            string unityProjectPath = Directory.GetParent(Application.dataPath).FullName;
            string repoRoot = Directory.GetParent(unityProjectPath).FullName;
            string path = Path.Combine(repoRoot, "Doc", "视觉资产来源与许可清单.md");
            Assert.IsTrue(File.Exists(path), "Missing visual asset manifest: " + path);
            return path;
        }

        private static string GetUiFactoryConstant(string fieldName)
        {
            Type factoryType = typeof(RoguelikeGame).Assembly.GetType("GameLogic.RoguelikeUIFactory");
            Assert.NotNull(factoryType);
            FieldInfo field = factoryType.GetField(fieldName, BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(field);
            return (string)field.GetRawConstantValue();
        }

        private static void InvokeWindowMethod(object window, string methodName)
        {
            MethodInfo method = typeof(UIWindow).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(method);
            method.Invoke(window, null);
        }

        private sealed class WindowFixture : IDisposable
        {
            public object Window { get; private set; }
            public RectTransform Root { get; private set; }

            public static WindowFixture Create(string typeName, UILayer layer, string location)
            {
                Type windowType = typeof(RoguelikeGame).Assembly.GetType(typeName);
                Assert.NotNull(windowType);

                GameObject panel = new GameObject(location + "Stage52Test", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
                RectTransform root = panel.GetComponent<RectTransform>();
                root.anchorMin = Vector2.zero;
                root.anchorMax = Vector2.one;
                root.offsetMin = Vector2.zero;
                root.offsetMax = Vector2.zero;

                object window = Activator.CreateInstance(windowType);
                MethodInfo init = typeof(UIWindow).GetMethod("Init", BindingFlags.Instance | BindingFlags.Public);
                Assert.NotNull(init);
                init.Invoke(window, new object[] { typeName, (int)layer, false, location, false, 10 });

                MethodInfo completed = typeof(UIWindow).GetMethod("Handle_Completed", BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.NotNull(completed);
                completed.Invoke(window, new object[] { panel });
                InvokeWindowMethod(window, "InternalCreate");
                InvokeWindowMethod(window, "InternalRefresh");

                return new WindowFixture
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
