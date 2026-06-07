using System.Collections.Generic;
using System.IO;
using GameConfig;
using Luban;
using NUnit.Framework;
using UnityEngine;

namespace GameLogic.Tests
{
    public sealed class RoguelikeStage36ConfigValidationTests
    {
        [Test]
        public void Stage36GeneratedRoguelikeTablesPassConfigValidation()
        {
            Tables tables = LoadTables();

            List<string> issues = RoguelikeConfigValidator.Validate(tables);

            Assert.That(issues, Is.Empty);
        }

        [Test]
        public void Stage36RequiredRuntimeResourcesExistInAssetRawCollectors()
        {
            Tables tables = LoadTables();

            List<string> issues = RoguelikeConfigValidator.Validate(tables, ResourceExistsInAssetRaw);

            Assert.That(issues, Is.Empty);
            Assert.That(RoguelikeConfigValidator.GetRequiredResourceAddresses(), Does.Contain(RoguelikeGame.HitSoundPath));
            Assert.That(RoguelikeConfigValidator.GetRequiredResourceAddresses(), Does.Contain(RoguelikeBattleStageView.CommonEnemyPrefabAddress));
        }

        [Test]
        public void Stage36MissingResourceAddressReportsExplicitIssue()
        {
            Tables tables = LoadTables();

            List<string> issues = RoguelikeConfigValidator.Validate(
                tables,
                address => address != RoguelikeBattleStageView.CommonEnemyPrefabAddress && ResourceExistsInAssetRaw(address));

            Assert.That(issues, Has.Count.EqualTo(1));
            Assert.That(issues[0], Does.Contain(RoguelikeBattleStageView.CommonEnemyPrefabAddress));
            Assert.That(issues[0], Does.Contain("资源地址"));
        }

        [Test]
        public void Stage36RoguelikeGameCachesValidationIssuesAfterTablesAssigned()
        {
            Tables tables = LoadTables();
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(36001);

            game.DebugSetConfigTables(tables);

            Assert.That(game.IsUsingLubanConfig, Is.True);
            Assert.That(game.ConfigValidationIssues, Is.Empty);
        }

        private static Tables LoadTables()
        {
            string configPath = Path.Combine(Application.dataPath, "AssetRaw", "Configs", "bytes");
            return new Tables(file =>
            {
                string path = Path.Combine(configPath, file + ".bytes");
                Assert.IsTrue(File.Exists(path), $"Config bytes not found: {path}");
                return new ByteBuf(File.ReadAllBytes(path));
            });
        }

        private static bool ResourceExistsInAssetRaw(string address)
        {
            if (string.IsNullOrEmpty(address))
            {
                return false;
            }

            string assetRawPath = Path.Combine(Application.dataPath, "AssetRaw");
            return File.Exists(Path.Combine(assetRawPath, "Audios", address + ".wav"))
                || File.Exists(Path.Combine(assetRawPath, "Effects", address + ".prefab"))
                || File.Exists(Path.Combine(assetRawPath, "Actor", address + ".prefab"));
        }
    }
}
