using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic.Tests
{
    public sealed class RoguelikeStage53PlayerVisualPresentationTests
    {
        private const string PlayerSprite = "Roguelike_Kenney_PlayerAdventurer";
        private const string PlayerPortrait = "Roguelike_Kenney_PlayerPortrait";

        [Test]
        public void Stage53KenneyPlayerAssetsAreImportedAndRegistered()
        {
            string actorPath = Path.Combine(Application.dataPath, "AssetRaw", "Actor");
            string atlasPath = Path.Combine(Application.dataPath, "AssetRaw", "UIRaw", "Atlas", "Roguelike");

            Assert.IsTrue(File.Exists(Path.Combine(actorPath, PlayerSprite + ".png")), "Missing player actor sprite");
            Assert.IsTrue(File.Exists(Path.Combine(atlasPath, PlayerPortrait + ".png")), "Missing player portrait sprite");

            string manifest = File.ReadAllText(GetVisualAssetManifestPath());
            Assert.That(manifest, Does.Contain("Kenney Roguelike Characters"));
            Assert.That(manifest, Does.Contain("https://kenney.nl/assets/roguelike-characters"));
            Assert.That(manifest, Does.Contain("https://creativecommons.org/publicdomain/zero/1.0/"));
            Assert.That(manifest, Does.Contain(PlayerSprite + ".png"));
            Assert.That(manifest, Does.Contain(PlayerPortrait + ".png"));
        }

        [Test]
        public void Stage53UiFactoryUsesKenneyPlayerPortraitForHudAndSettlement()
        {
            Assert.That(GetUiFactoryConstant("OriginalHeroPortraitSprite"), Is.EqualTo("Roguelike_UI_HeroPortrait"));
            Assert.That(GetUiFactoryConstant("HeroPortraitSprite"), Is.EqualTo(PlayerPortrait));

            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(53001);

            using (WindowFixture hud = WindowFixture.Create("GameLogic.BattleHudUI", UILayer.UI, "BattleHudUI"))
            {
                AssertSprite(FindImage(hud.Root, "角色头像"), PlayerPortrait, Image.Type.Simple);
            }

            game.DebugForceVictory();
            using (WindowFixture settlement = WindowFixture.Create("GameLogic.BattleSettlementUI", UILayer.Top, "BattleSettlementUI"))
            {
                AssertSprite(FindImage(settlement.Root, "角色剪影头像"), PlayerPortrait, Image.Type.Simple);
            }
        }

        [Test]
        public void Stage53StagePlayerUsesKenneySpriteWithoutChangingPhysics()
        {
            GameObject oldStage = GameObject.Find("Roguelike2DSurvivalStage");
            if (oldStage != null)
            {
                UnityEngine.Object.DestroyImmediate(oldStage);
            }

            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(53002);

            RoguelikeBattleStageView view = RoguelikeBattleStageView.Ensure();
            view.Refresh(game.CurrentRun);

            Transform player = FindTransform(view.transform, "玩家");
            Assert.NotNull(player);

            SpriteRenderer renderer = player.GetComponent<SpriteRenderer>();
            Assert.NotNull(renderer);
            Assert.NotNull(renderer.sprite);
            Assert.That(renderer.sprite.name, Is.EqualTo(PlayerSprite));
            Assert.That(renderer.color, Is.EqualTo(Color.white));
            Assert.That(renderer.sortingOrder, Is.EqualTo(10));

            Rigidbody2D body = player.GetComponent<Rigidbody2D>();
            CircleCollider2D collider = player.GetComponent<CircleCollider2D>();
            RoguelikePlayerMotor motor = player.GetComponent<RoguelikePlayerMotor>();
            Assert.NotNull(body);
            Assert.NotNull(collider);
            Assert.NotNull(motor);
            Assert.That(body.gravityScale, Is.EqualTo(0f));
            Assert.That(collider.radius, Is.EqualTo(0.48f).Within(0.001f));
        }

        private static Transform FindTransform(Transform root, string name)
        {
            Transform[] children = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i].name == name)
                {
                    return children[i];
                }
            }

            return null;
        }

        private static RectTransform FindRect(Transform root, string name)
        {
            Transform target = FindTransform(root, name);
            return target as RectTransform;
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

        private static string GetUiFactoryConstant(string fieldName)
        {
            Type factoryType = typeof(RoguelikeGame).Assembly.GetType("GameLogic.RoguelikeUIFactory");
            Assert.NotNull(factoryType);
            FieldInfo field = factoryType.GetField(fieldName, BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(field);
            return (string)field.GetRawConstantValue();
        }

        private static string GetVisualAssetManifestPath()
        {
            string unityProjectPath = Directory.GetParent(Application.dataPath).FullName;
            string repoRoot = Directory.GetParent(unityProjectPath).FullName;
            string path = Path.Combine(repoRoot, "Doc", "视觉资产来源与许可清单.md");
            Assert.IsTrue(File.Exists(path), "Missing visual asset manifest: " + path);
            return path;
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

                GameObject panel = new GameObject(location + "Stage53Test", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
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
