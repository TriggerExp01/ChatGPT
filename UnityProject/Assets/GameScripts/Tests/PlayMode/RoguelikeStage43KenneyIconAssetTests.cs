using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace GameLogic.Tests
{
    public sealed class RoguelikeStage43KenneyIconAssetTests
    {
        private static readonly string[] RequiredKenneyIconAssets =
        {
            "Roguelike_UI_Icon_MagicBolt.png",
            "Roguelike_UI_Icon_SpinningBlade.png",
            "Roguelike_UI_Icon_PiercingDart.png",
            "Roguelike_UI_Icon_StarRingPulse.png",
            "Roguelike_UI_Icon_RelicPower.png",
            "Roguelike_UI_Icon_RelicGrowth.png",
            "Roguelike_UI_Icon_RelicUtility.png",
            "Roguelike_UI_Icon_PaidSupply.png",
        };

        [Test]
        public void Stage43ImportedKenneyIconsExistInRoguelikeAtlas()
        {
            string atlasPath = Path.Combine(Application.dataPath, "AssetRaw", "UIRaw", "Atlas", "Roguelike");
            foreach (string asset in RequiredKenneyIconAssets)
            {
                Assert.IsTrue(File.Exists(Path.Combine(atlasPath, asset)), "Missing Kenney icon asset: " + asset);
            }
        }

        [Test]
        public void Stage43RewardIdsResolveToDedicatedKenneyIcons()
        {
            Assert.That(ResolveRewardIcon("weapon_magic_bolt_upgrade"), Is.EqualTo("Roguelike_UI_Icon_MagicBolt"));
            Assert.That(ResolveRewardIcon("weapon_spinning_blade_unlock"), Is.EqualTo("Roguelike_UI_Icon_SpinningBlade"));
            Assert.That(ResolveRewardIcon("weapon_piercing_dart_upgrade"), Is.EqualTo("Roguelike_UI_Icon_PiercingDart"));
            Assert.That(ResolveRewardIcon("weapon_star_ring_pulse_unlock"), Is.EqualTo("Roguelike_UI_Icon_StarRingPulse"));
            Assert.That(ResolveRewardIcon("passive_eagle_eye"), Is.EqualTo("Roguelike_UI_Icon_RelicPower"));
            Assert.That(ResolveRewardIcon("passive_vital_sigil"), Is.EqualTo("Roguelike_UI_Icon_RelicGrowth"));
            Assert.That(ResolveRewardIcon("passive_magnet_core"), Is.EqualTo("Roguelike_UI_Icon_RelicUtility"));
            Assert.That(ResolveRewardIcon("paid_field_ration"), Is.EqualTo("Roguelike_UI_Icon_PaidSupply"));
        }

        [Test]
        public void Stage43VisualAssetManifestCoversKenneyIconSourceAndLicense()
        {
            string manifest = File.ReadAllText(GetVisualAssetManifestPath());
            List<string> missing = new List<string>();

            foreach (string asset in RequiredKenneyIconAssets)
            {
                if (!manifest.Contains(asset))
                {
                    missing.Add(asset);
                }
            }

            Assert.That(missing, Is.Empty);
            Assert.That(manifest, Does.Contain("Kenney Game Icons"));
            Assert.That(manifest, Does.Contain("https://kenney.nl/assets/game-icons"));
            Assert.That(manifest, Does.Contain("https://creativecommons.org/publicdomain/zero/1.0/"));
            Assert.That(manifest, Does.Contain("CC0"));
        }

        private static string ResolveRewardIcon(string rewardId)
        {
            System.Type factoryType = typeof(RoguelikeGame).Assembly.GetType("GameLogic.RoguelikeUIFactory");
            Assert.NotNull(factoryType);
            MethodInfo method = factoryType.GetMethod("ResolveRewardIconSprite", BindingFlags.Public | BindingFlags.Static);
            Assert.NotNull(method);
            return method.Invoke(null, new object[] { rewardId }) as string;
        }

        private static string GetVisualAssetManifestPath()
        {
            string unityProjectPath = Directory.GetParent(Application.dataPath).FullName;
            string repoRoot = Directory.GetParent(unityProjectPath).FullName;
            string path = Path.Combine(repoRoot, "Doc", "视觉资产来源与许可清单.md");
            Assert.IsTrue(File.Exists(path), "Missing visual asset manifest: " + path);
            return path;
        }
    }
}
