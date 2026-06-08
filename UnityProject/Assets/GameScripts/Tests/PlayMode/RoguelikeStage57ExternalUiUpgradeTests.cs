using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic.Tests
{
    public sealed class RoguelikeStage57ExternalUiUpgradeTests
    {
        private static readonly string[] SystemG6Assets =
        {
            "Roguelike_SystemG6_UI_PanelFrame.png",
            "Roguelike_SystemG6_UI_CardFrame.png",
            "Roguelike_SystemG6_UI_ButtonFrame.png",
            "Roguelike_SystemG6_UI_DividerMetal.png",
            "Roguelike_SystemG6_UI_SliderFrame.png",
            "Roguelike_SystemG6_UI_SliderFillRed.png",
            "Roguelike_SystemG6_UI_SliderFillYellow.png",
            "Roguelike_SystemG6_UI_SlotFrame.png",
        };

        [Test]
        public void SystemG6UiAssetsAreImportedAndRegistered()
        {
            string atlasPath = Path.Combine(Application.dataPath, "AssetRaw", "UIRaw", "Atlas", "Roguelike");
            foreach (string asset in SystemG6Assets)
            {
                Assert.IsTrue(File.Exists(Path.Combine(atlasPath, asset)), "Missing System G6 UI asset: " + asset);
            }

            string manifest = File.ReadAllText(GetVisualAssetManifestPath());
            Assert.That(manifest, Does.Contain("RPG UI"));
            Assert.That(manifest, Does.Contain("System G6"));
            Assert.That(manifest, Does.Contain("https://opengameart.org/content/rpg-ui-1"));
            Assert.That(manifest, Does.Contain("https://opengameart.org/sites/default/files/rpg_ui_0.7z"));
            Assert.That(manifest, Does.Contain("Creative Commons CC0"));

            foreach (string asset in SystemG6Assets)
            {
                Assert.That(manifest, Does.Contain(asset));
            }
        }

        [Test]
        public void TinyRpgManaSoulGuiIsRecordedAsSelectedUiCandidate()
        {
            string manifest = File.ReadAllText(GetVisualAssetManifestPath());
            Assert.That(manifest, Does.Contain("Tiny RPG - Mana Soul GUI"));
            Assert.That(manifest, Does.Contain("tiopalada"));
            Assert.That(manifest, Does.Contain("https://tiopalada.itch.io/tiny-rpg-mana-soul-gui"));
            Assert.That(manifest, Does.Contain("tinyRPG_manaSoulGUI_v_1_0.zip"));
            Assert.That(manifest, Does.Contain("上传 ID `13498579`"));
            Assert.That(manifest, Does.Contain("`/file/13498579` 端点仍返回 `invalid key`"));
            Assert.That(manifest, Does.Contain("待官方 zip 真正获取后再导入"));
        }

        [Test]
        public void UiFactoryUsesSystemG6SpritesForCoreFramesAndBars()
        {
            Assert.That(GetUiFactoryConstant("PanelFrameSprite"), Is.EqualTo("Roguelike_SystemG6_UI_PanelFrame"));
            Assert.That(GetUiFactoryConstant("CardFrameSprite"), Is.EqualTo("Roguelike_SystemG6_UI_CardFrame"));
            Assert.That(GetUiFactoryConstant("ButtonFrameSprite"), Is.EqualTo("Roguelike_SystemG6_UI_ButtonFrame"));
            Assert.That(GetUiFactoryConstant("DividerFadeSprite"), Is.EqualTo("Roguelike_SystemG6_UI_DividerMetal"));
            Assert.That(GetUiFactoryConstant("SliderFrameSprite"), Is.EqualTo("Roguelike_SystemG6_UI_SliderFrame"));
            Assert.That(GetUiFactoryConstant("SliderFillRedSprite"), Is.EqualTo("Roguelike_SystemG6_UI_SliderFillRed"));
            Assert.That(GetUiFactoryConstant("SliderFillYellowSprite"), Is.EqualTo("Roguelike_SystemG6_UI_SliderFillYellow"));
            Assert.That(GetUiFactoryConstant("SlotFrameSprite"), Is.EqualTo("Roguelike_SystemG6_UI_SlotFrame"));
        }

        [Test]
        public void CreatedPanelAndSliderUseSystemG6SpriteAssets()
        {
            GameObject rootObject = new GameObject("Stage57SystemG6UiRoot", typeof(RectTransform));
            RectTransform root = rootObject.GetComponent<RectTransform>();
            try
            {
                RectTransform panel = InvokeCreatePanel("正式面板", root, Vector2.zero, Vector2.one, Color.white);
                AssertSprite(panel.GetComponent<Image>(), "Roguelike_SystemG6_UI_PanelFrame", Image.Type.Sliced);

                Slider slider = InvokeCreateSlider("正式生命条", root, Vector2.zero, Vector2.one, Color.white, GetUiFactoryConstant("SliderFillRedSprite"));
                AssertSprite(slider.GetComponent<Image>(), "Roguelike_SystemG6_UI_SliderFrame", Image.Type.Sliced);

                Image fill = slider.fillRect.GetComponent<Image>();
                AssertSprite(fill, "Roguelike_SystemG6_UI_SliderFillRed", Image.Type.Sliced);

                Image icon = InvokeCreateFramedIconSlot("正式图标槽", root, Vector2.zero, Vector2.one, Color.white, Color.white, "Roguelike_UI_WeaponIcon");
                AssertSprite(FindImage(root, "正式图标槽框"), "Roguelike_SystemG6_UI_SlotFrame", Image.Type.Sliced);
                AssertSprite(icon, "Roguelike_UI_WeaponIcon", Image.Type.Simple);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(rootObject);
            }
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

        private static RectTransform InvokeCreatePanel(string name, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            Type factoryType = typeof(RoguelikeGame).Assembly.GetType("GameLogic.RoguelikeUIFactory");
            Assert.NotNull(factoryType);
            MethodInfo method = factoryType.GetMethod("CreatePanel", BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(method);
            return (RectTransform)method.Invoke(null, new object[] { name, parent, anchorMin, anchorMax, color });
        }

        private static Slider InvokeCreateSlider(string name, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax, Color fillColor, string fillSpriteAddress)
        {
            Type factoryType = typeof(RoguelikeGame).Assembly.GetType("GameLogic.RoguelikeUIFactory");
            Assert.NotNull(factoryType);
            MethodInfo method = factoryType.GetMethod(
                "CreateSlider",
                BindingFlags.Public | BindingFlags.Static,
                null,
                new[] { typeof(string), typeof(RectTransform), typeof(Vector2), typeof(Vector2), typeof(Color), typeof(string) },
                null);
            Assert.NotNull(method);
            return (Slider)method.Invoke(null, new object[] { name, parent, anchorMin, anchorMax, fillColor, fillSpriteAddress });
        }

        private static Image InvokeCreateFramedIconSlot(string name, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax, Color frameColor, Color iconColor, string iconSpriteAddress)
        {
            Type factoryType = typeof(RoguelikeGame).Assembly.GetType("GameLogic.RoguelikeUIFactory");
            Assert.NotNull(factoryType);
            MethodInfo method = factoryType.GetMethod("CreateFramedIconSlot", BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(method);
            return (Image)method.Invoke(null, new object[] { name, parent, anchorMin, anchorMax, frameColor, iconColor, iconSpriteAddress });
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
    }
}
