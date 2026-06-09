using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic.Tests
{
    public sealed class RoguelikeUiPrefabSpriteBindingTests
    {
        private const string UiAtlasPath = "Assets/AssetRaw/UIRaw/Atlas/Roguelike";
        private const string LoginPanelSprite = "Roguelike_StarUI_LoginPanel";
        private const string InputFrameSprite = "Roguelike_StarUI_InputFrame";
        private const string TestPanelSprite = "Roguelike_StarUI_PanelFrame";
        private const string ButtonFrameSprite = "Roguelike_StarUI_ButtonFrame";
        private const string SliderFillBlueSprite = "Roguelike_StarUI_SliderFillBlue";
        private static readonly string[] UiPrefabPaths =
        {
            "Assets/AssetRaw/UI/BattleChoiceUI.prefab",
            "Assets/AssetRaw/UI/BattleControlUI.prefab",
            "Assets/AssetRaw/UI/BattleDebugUI.prefab",
            "Assets/AssetRaw/UI/BattleHudUI.prefab",
            "Assets/AssetRaw/UI/BattleSettlementUI.prefab",
            "Assets/AssetRaw/UI/LoginUI.prefab",
            "Assets/AssetRaw/UI/TestUI.prefab",
            "Assets/GameScripts/HotFix/GameLogic/Module/UIModule/Resources/LogUI.prefab",
            "Assets/Launcher/Resources/UIWindow/LoadTipsUI.prefab"
        };

        [Test]
        public void GeneratedUiSpritesAreImportedAsSlicedSprites()
        {
            AssertSpriteAsset(LoginPanelSprite, new Vector4(42f, 42f, 42f, 42f));
            AssertSpriteAsset(InputFrameSprite, new Vector4(28f, 18f, 28f, 18f));
            AssertSpriteAsset(TestPanelSprite, new Vector4(32f, 32f, 32f, 32f));
            AssertSpriteAsset(SliderFillBlueSprite, new Vector4(10f, 0f, 10f, 0f));
        }

        [Test]
        public void LoginAndTestPrefabsUseProjectUiSpritesInsteadOfBuiltinSprites()
        {
            GameObject loginPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AssetRaw/UI/LoginUI.prefab");
            Assert.NotNull(loginPrefab);
            AssertImage(loginPrefab.GetComponent<Image>(), LoginPanelSprite, Image.Type.Sliced);
            AssertImage(loginPrefab.transform.Find("m_inputAccount").GetComponent<Image>(), InputFrameSprite, Image.Type.Sliced);
            AssertImage(loginPrefab.transform.Find("m_inputPassword").GetComponent<Image>(), InputFrameSprite, Image.Type.Sliced);
            AssertImage(loginPrefab.transform.Find("m_btnLogin").GetComponent<Image>(), ButtonFrameSprite, Image.Type.Sliced);

            GameObject testPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/AssetRaw/UI/TestUI.prefab");
            Assert.NotNull(testPrefab);
            AssertImage(testPrefab.GetComponent<Image>(), TestPanelSprite, Image.Type.Sliced);
        }

        [Test]
        public void AllUiPrefabsAvoidUnityBuiltinUiSprites()
        {
            foreach (string prefabPath in UiPrefabPaths)
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                Assert.NotNull(prefab, prefabPath);

                Image[] images = prefab.GetComponentsInChildren<Image>(true);
                foreach (Image image in images)
                {
                    if (image.sprite == null)
                    {
                        continue;
                    }

                    string spritePath = AssetDatabase.GetAssetPath(image.sprite);
                    Assert.That(spritePath, Is.Not.EqualTo("Resources/unity_builtin_extra"), prefabPath + " :: " + image.name);
                }
            }
        }

        [Test]
        public void HudExperienceBarUsesFormalBlueSystemG6Sprite()
        {
            Assert.That(GetUiFactoryConstant("SliderFillBlueSprite"), Is.EqualTo(SliderFillBlueSprite));

            GameObject rootObject = new GameObject("UiPrefabBlueSliderRoot", typeof(RectTransform));
            try
            {
                RectTransform root = rootObject.GetComponent<RectTransform>();
                Slider slider = CreateFactorySlider(
                    "经验条验证",
                    root,
                    Vector2.zero,
                    Vector2.one,
                    Color.white,
                    GetUiFactoryConstant("SliderFillBlueSprite"));

                Image fill = slider.fillRect.GetComponent<Image>();
                AssertImage(fill, SliderFillBlueSprite, Image.Type.Sliced);
            }
            finally
            {
                Object.DestroyImmediate(rootObject);
            }
        }

        private static void AssertSpriteAsset(string spriteName, Vector4 expectedBorder)
        {
            string path = Path.Combine(UiAtlasPath, spriteName + ".png").Replace("\\", "/");
            Assert.IsTrue(File.Exists(path), path);

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            Assert.NotNull(sprite, path);
            Assert.That(sprite.border, Is.EqualTo(expectedBorder));
        }

        private static void AssertImage(Image image, string expectedSpriteName, Image.Type expectedType)
        {
            Assert.NotNull(image);
            Assert.NotNull(image.sprite);
            Assert.That(image.sprite.name, Is.EqualTo(expectedSpriteName));
            Assert.That(image.type, Is.EqualTo(expectedType));
            Assert.That(image.sprite.name, Does.Not.StartWith("UISprite"));
            Assert.That(AssetDatabase.GetAssetPath(image.sprite), Does.StartWith(UiAtlasPath));
        }

        private static string GetUiFactoryConstant(string fieldName)
        {
            System.Type factoryType = GetUiFactoryType();
            Assert.NotNull(factoryType);
            System.Reflection.FieldInfo field = factoryType.GetField(fieldName);
            Assert.NotNull(field);
            return (string)field.GetValue(null);
        }

        private static Slider CreateFactorySlider(string name, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax, Color fillColor, string fillSpriteAddress)
        {
            System.Type factoryType = GetUiFactoryType();
            System.Reflection.MethodInfo method = factoryType.GetMethod(
                "CreateSlider",
                new[]
                {
                    typeof(string),
                    typeof(RectTransform),
                    typeof(Vector2),
                    typeof(Vector2),
                    typeof(Color),
                    typeof(string)
                });
            Assert.NotNull(method);
            return (Slider)method.Invoke(null, new object[] { name, parent, anchorMin, anchorMax, fillColor, fillSpriteAddress });
        }

        private static System.Type GetUiFactoryType()
        {
            return typeof(RoguelikeGame).Assembly.GetType("GameLogic.RoguelikeUIFactory");
        }
    }
}
