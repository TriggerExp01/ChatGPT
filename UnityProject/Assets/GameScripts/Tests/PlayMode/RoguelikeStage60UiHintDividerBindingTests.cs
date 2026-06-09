using System;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic.Tests
{
    public sealed class RoguelikeStage60UiHintDividerBindingTests
    {
        [Test]
        public void BattleHintPanelUsesSystemG6PanelAndDivider()
        {
            RoguelikeGame.Instance.StartNewRun(60001);

            using (WindowFixture fixture = WindowFixture.Create("GameLogic.BattleHudUI", "BattleHudUI"))
            {
                InvokeWindowMethod(fixture.Window, "InternalRefresh");

                AssertSprite(FindImage(fixture.Root, "战斗提示面板"), "Roguelike_StarUI_PanelFrame", Image.Type.Sliced);
                AssertSprite(FindImage(fixture.Root, "战斗提示外部装饰线"), "Roguelike_StarUI_DividerLine", Image.Type.Simple);
            }
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

        private static void AssertSprite(Image image, string spriteName, Image.Type imageType)
        {
            Assert.NotNull(image);
            Assert.NotNull(image.sprite);
            Assert.That(image.sprite.name, Is.EqualTo(spriteName));
            Assert.That(image.type, Is.EqualTo(imageType));
        }

        private static void InvokeWindowMethod(object window, string methodName)
        {
            MethodInfo method = window.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            Assert.NotNull(method);
            method.Invoke(window, null);
        }

        private sealed class WindowFixture : IDisposable
        {
            public RectTransform Root { get; private set; }
            public object Window { get; private set; }

            public static WindowFixture Create(string windowName, string assetName)
            {
                GameObject rootObject = new GameObject(assetName + "Stage60Root", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
                RectTransform root = rootObject.GetComponent<RectTransform>();
                root.anchorMin = Vector2.zero;
                root.anchorMax = Vector2.one;
                root.offsetMin = Vector2.zero;
                root.offsetMax = Vector2.zero;

                Type type = typeof(RoguelikeGame).Assembly.GetType(windowName);
                Assert.NotNull(type);
                object window = Activator.CreateInstance(type);

                MethodInfo init = typeof(UIWindow).GetMethod("Init", BindingFlags.Instance | BindingFlags.Public);
                Assert.NotNull(init);
                init.Invoke(window, new object[] { windowName, (int)UILayer.UI, false, assetName, false, 10 });

                MethodInfo completed = typeof(UIWindow).GetMethod("Handle_Completed", BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.NotNull(completed);
                completed.Invoke(window, new object[] { rootObject });
                InvokeWindowMethod(window, "InternalCreate");
                InvokeWindowMethod(window, "InternalRefresh");

                return new WindowFixture { Root = root, Window = window };
            }

            public void Dispose()
            {
                if (Root != null)
                {
                    UnityEngine.Object.DestroyImmediate(Root.gameObject);
                    Root = null;
                }
            }
        }
    }
}
