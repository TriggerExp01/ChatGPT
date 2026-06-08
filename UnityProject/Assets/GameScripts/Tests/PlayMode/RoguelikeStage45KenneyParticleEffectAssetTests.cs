using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace GameLogic.Tests
{
    public sealed class RoguelikeStage45KenneyParticleEffectAssetTests
    {
        private static readonly string[] RequiredKenneyParticleAssets =
        {
            "Roguelike_Kenney_HitSpark.png",
            "Roguelike_Kenney_CriticalBolt.png",
            "Roguelike_Kenney_KillStar.png",
            "Roguelike_Kenney_PickupGlow.png",
        };

        private static readonly Dictionary<string, string> PrefabSpriteGuidChecks = new Dictionary<string, string>
        {
            { "Roguelike_HitEffect.prefab", "ddb1513f98fc8174cafd14c26aa4c3b8" },
            { "Roguelike_CriticalEffect.prefab", "a2f24097186702047a449ad6b85bd03e" },
            { "Roguelike_KillEffect.prefab", "c732f19c31e1a7146ac2d82239c78f6b" },
            { "Roguelike_PickupEffect.prefab", "42053d01c0c58474da81364bd03ab35f" },
        };

        [Test]
        public void Stage45ImportedKenneyParticleSpritesExistInEffectsFolder()
        {
            string effectPath = GetEffectsPath();
            foreach (string asset in RequiredKenneyParticleAssets)
            {
                Assert.IsTrue(File.Exists(Path.Combine(effectPath, asset)), "Missing Kenney particle asset: " + asset);
                Assert.IsTrue(File.Exists(Path.Combine(effectPath, asset + ".meta")), "Missing Kenney particle meta: " + asset);
            }
        }

        [Test]
        public void Stage45EffectPrefabsReferenceDedicatedKenneyParticleSprites()
        {
            string effectPath = GetEffectsPath();
            foreach (KeyValuePair<string, string> pair in PrefabSpriteGuidChecks)
            {
                string prefabPath = Path.Combine(effectPath, pair.Key);
                Assert.IsTrue(File.Exists(prefabPath), "Missing effect prefab: " + pair.Key);

                string prefab = File.ReadAllText(prefabPath);
                Assert.That(prefab, Does.Contain("guid: " + pair.Value), pair.Key + " should reference its Kenney particle sprite.");
            }
        }

        [Test]
        public void Stage45CriticalCueUsesDedicatedEffectPrefabAddress()
        {
            Assert.That(RoguelikeBattleStageView.CriticalEffectPrefabAddress, Is.EqualTo("Roguelike_CriticalEffect"));
        }

        [Test]
        public void Stage45VisualAssetManifestCoversKenneyParticleSourceAndLicense()
        {
            string manifest = File.ReadAllText(GetVisualAssetManifestPath());
            List<string> missing = new List<string>();

            foreach (string asset in RequiredKenneyParticleAssets)
            {
                if (!manifest.Contains(asset))
                {
                    missing.Add(asset);
                }
            }

            foreach (string prefab in PrefabSpriteGuidChecks.Keys)
            {
                if (!manifest.Contains(prefab))
                {
                    missing.Add(prefab);
                }
            }

            Assert.That(missing, Is.Empty);
            Assert.That(manifest, Does.Contain("Kenney Particle Pack"));
            Assert.That(manifest, Does.Contain("https://kenney.nl/assets/particle-pack"));
            Assert.That(manifest, Does.Contain("https://creativecommons.org/publicdomain/zero/1.0/"));
            Assert.That(manifest, Does.Contain("CC0"));
            Assert.That(manifest, Does.Contain("muzzle_03.png"));
            Assert.That(manifest, Does.Contain("spark_05.png"));
            Assert.That(manifest, Does.Contain("star_07.png"));
            Assert.That(manifest, Does.Contain("circle_05.png"));
        }

        private static string GetEffectsPath()
        {
            return Path.Combine(Application.dataPath, "AssetRaw", "Effects");
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
