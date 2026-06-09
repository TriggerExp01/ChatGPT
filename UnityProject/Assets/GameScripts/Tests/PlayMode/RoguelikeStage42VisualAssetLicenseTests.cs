using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace GameLogic.Tests
{
    public sealed class RoguelikeStage42VisualAssetLicenseTests
    {
        private static readonly string[] RequiredRoguelikeUiAssets =
        {
            "Roguelike_UI_PanelFrame.png",
            "Roguelike_UI_CardFrame.png",
            "Roguelike_UI_HeroPortrait.png",
            "Roguelike_Kenney_PlayerPortrait.png",
            "Roguelike_UI_WeaponIcon.png",
            "Roguelike_UI_RelicIcon.png",
            "Roguelike_UI_GoldIcon.png",
            "Roguelike_StarUI_SliderFrame.png",
            "Roguelike_StarUI_SliderFillRed.png",
            "Roguelike_StarUI_SliderFillBlue.png",
            "Roguelike_StarUI_SliderFillYellow.png",
            "Roguelike_UI_LightBand.png",
        };

        private static readonly string[] RequiredRuntimeVisualAssets =
        {
            "Roguelike_EffectPixel.png",
            "Roguelike_Kenney_HitSpark.png",
            "Roguelike_Kenney_CriticalBolt.png",
            "Roguelike_Kenney_KillStar.png",
            "Roguelike_Kenney_PickupGlow.png",
            "Roguelike_HitEffect.prefab",
            "Roguelike_CriticalEffect.prefab",
            "Roguelike_KillEffect.prefab",
            "Roguelike_PickupEffect.prefab",
            "Roguelike_Enemy_Common.prefab",
            "Roguelike_Enemy_Boss.prefab",
            "Roguelike_Projectile_MagicBolt.prefab",
            "Roguelike_Projectile_PiercingDart.prefab",
            "Roguelike_Kenney_PlayerAdventurer.png",
        };

        [Test]
        public void Stage42RoguelikeUiNoLongerDependsOnBattleAtlasSprites()
        {
            Assert.That(GetUiFactoryConstant("BattleLightSprite"), Is.EqualTo("Roguelike_UI_LightBand"));
            Assert.That(GetUiFactoryConstant("SliderFrameSprite"), Is.EqualTo("Roguelike_StarUI_SliderFrame"));
            Assert.That(GetUiFactoryConstant("SliderFillRedSprite"), Is.EqualTo("Roguelike_StarUI_SliderFillRed"));
            Assert.That(GetUiFactoryConstant("SliderFillBlueSprite"), Is.EqualTo("Roguelike_StarUI_SliderFillBlue"));
            Assert.That(GetUiFactoryConstant("SliderFillYellowSprite"), Is.EqualTo("Roguelike_StarUI_SliderFillYellow"));
        }

        [Test]
        public void Stage42RoguelikeUiReplacementSpritesExistInDedicatedAtlas()
        {
            string atlasPath = Path.Combine(Application.dataPath, "AssetRaw", "UIRaw", "Atlas", "Roguelike");
            foreach (string asset in RequiredRoguelikeUiAssets)
            {
                Assert.IsTrue(File.Exists(Path.Combine(atlasPath, asset)), "Missing roguelike UI asset: " + asset);
            }
        }

        [Test]
        public void Stage42VisualAssetManifestCoversCurrentKeyAssetsAndLicenseFields()
        {
            string manifest = File.ReadAllText(GetVisualAssetManifestPath());
            List<string> missing = new List<string>();

            foreach (string asset in RequiredRoguelikeUiAssets)
            {
                AssertManifestEntry(manifest, asset, missing);
            }

            foreach (string asset in RequiredRuntimeVisualAssets)
            {
                AssertManifestEntry(manifest, asset, missing);
            }

            Assert.That(missing, Is.Empty);
            Assert.That(manifest, Does.Contain("来源"));
            Assert.That(manifest, Does.Contain("作者"));
            Assert.That(manifest, Does.Contain("许可"));
            Assert.That(manifest, Does.Contain("用途"));
            Assert.That(manifest, Does.Contain("禁止商用"));
            Assert.That(manifest, Does.Contain("许可不清"));
        }

        private static void AssertManifestEntry(string manifest, string asset, List<string> missing)
        {
            if (!manifest.Contains(asset))
            {
                missing.Add(asset);
            }
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
            System.Type factoryType = typeof(RoguelikeGame).Assembly.GetType("GameLogic.RoguelikeUIFactory");
            Assert.NotNull(factoryType);
            FieldInfo field = factoryType.GetField(fieldName, BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(field);
            return field.GetRawConstantValue() as string;
        }
    }
}
