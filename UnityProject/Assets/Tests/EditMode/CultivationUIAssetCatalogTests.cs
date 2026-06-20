using GameLogic.Cultivation;
using NUnit.Framework;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic.Tests
{
    public sealed class CultivationUIAssetCatalogTests
    {
        [Test]
        public void RequiredUiPlaceholderAssetsExistAndImportAsSprites()
        {
            Assert.IsTrue(File.Exists(CultivationUIAssetCatalog.Manifest), CultivationUIAssetCatalog.Manifest);
            var manifest = File.ReadAllText(CultivationUIAssetCatalog.Manifest);
            StringAssert.Contains("Icons/ui_icon_counter.png", manifest);

            foreach (var path in CultivationUIAssetCatalog.RequiredSpritePaths)
            {
                Assert.IsTrue(File.Exists(path), path);
                Assert.NotNull(CultivationUIAssetCatalog.LoadEditorSprite(path), path);
            }
        }

        [Test]
        public void PrototypeUiAppliesUiPlaceholderSpritesWhenAvailable()
        {
            var root = new GameObject("CultivationUIAssetCatalogTestRoot", typeof(RectTransform));
            try
            {
                var ui = CultivationRunPrototypeUI.Open(root.transform);
                var shellImage = ui.GetComponent<Image>();
                var resetImage = ui.transform.Find("Lower/ActionBar/ResetButton").GetComponent<Image>();
                var firstCard = FindFirstChildWithPrefix(ui.transform.Find("Lower/HandScrollPanel/Viewport/Content"), "Card_");

                Assert.AreSame(CultivationUIAssetCatalog.LoadEditorSprite(CultivationUIAssetCatalog.CombatQiRefiningBackground), shellImage.sprite);
                Assert.AreSame(CultivationUIAssetCatalog.LoadEditorSprite(CultivationUIAssetCatalog.ButtonNormal), resetImage.sprite);
                Assert.NotNull(firstCard);
                Assert.AreSame(CultivationUIAssetCatalog.LoadEditorSprite(CultivationUIAssetCatalog.CardFrameSpirit), firstCard.GetComponent<Image>().sprite);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        private static Transform FindFirstChildWithPrefix(Transform parent, string prefix)
        {
            foreach (Transform child in parent)
            {
                if (child.name.StartsWith(prefix))
                {
                    return child;
                }
            }

            return null;
        }
    }
}
