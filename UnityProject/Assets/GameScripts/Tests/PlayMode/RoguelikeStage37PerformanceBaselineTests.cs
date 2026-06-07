using System.IO;
using GameConfig;
using Luban;
using NUnit.Framework;
using TEngine;
using UnityEngine;

namespace GameLogic.Tests
{
    public sealed class RoguelikeStage37PerformanceBaselineTests
    {
        [Test]
        public void Stage37SnapshotReflectsCurrentRuntimeObjectCounts()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(37001);
            MemoryPool.ClearAll();

            game.DebugSetConfigTables(LoadTables());
            game.Tick(0.1f);
            game.Tick(0.1f);

            RoguelikePerformanceSnapshot snapshot = game.CapturePerformanceSnapshot(0.2f);

            Assert.That(snapshot.ElapsedTime, Is.EqualTo(game.ElapsedTime).Within(0.001f));
            Assert.That(snapshot.EnemyCount, Is.EqualTo(game.Enemies.Count));
            Assert.That(snapshot.ProjectileCount, Is.EqualTo(game.Projectiles.Count));
            Assert.That(snapshot.PickupCount, Is.EqualTo(game.Pickups.Count));
            Assert.That(snapshot.ManagedMemoryBytes, Is.GreaterThan(0));
            Assert.That(snapshot.ToBaselineLine(), Does.Contain("敌人"));
            Assert.That(snapshot.ToBaselineLine(), Does.Contain("平均帧率"));
        }

        [Test]
        public void Stage37FixedStepBaselineProducesComparableRuntimeMetrics()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(37002);
            MemoryPool.ClearAll();
            game.DebugSetConfigTables(LoadTables());

            RoguelikePerformanceSnapshot snapshot = game.RunPerformanceBaselineSimulation(37003, 40, 0.1f);

            Assert.That(snapshot.ElapsedTime, Is.EqualTo(4f).Within(0.001f));
            Assert.That(snapshot.EnemyCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(snapshot.ProjectileCount, Is.GreaterThanOrEqualTo(0));
            Assert.That(snapshot.EnemyPoolUsingCount, Is.EqualTo(snapshot.EnemyCount));
            Assert.That(snapshot.ProjectilePoolUsingCount, Is.EqualTo(snapshot.ProjectileCount));
            Assert.That(snapshot.AverageFps, Is.EqualTo(10f).Within(0.001f));
        }

        [Test]
        public void Stage37BaselineCanIncludeStageViewPoolAndReuseCounters()
        {
            RoguelikeBattleStageView view = CreateFreshStage();
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(37004);
            MemoryPool.ClearAll();
            game.DebugSetConfigTables(LoadTables());

            RoguelikePerformanceSnapshot activeSnapshot = game.RunPerformanceBaselineSimulation(37005, 24, 0.1f, view);
            game.StartNewRun(37006);
            view.Refresh(game.CurrentRun);
            RoguelikePerformanceSnapshot pooledSnapshot = game.CapturePerformanceSnapshot(0.1f, view);

            Assert.That(activeSnapshot.EnemyCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(activeSnapshot.PooledViewCount, Is.GreaterThanOrEqualTo(0));
            Assert.That(pooledSnapshot.PooledViewCount, Is.GreaterThanOrEqualTo(activeSnapshot.EnemyCount));
            Assert.That(pooledSnapshot.ViewReuseCount, Is.GreaterThanOrEqualTo(activeSnapshot.ViewReuseCount));
        }

        private static RoguelikeBattleStageView CreateFreshStage()
        {
            GameObject oldStage = GameObject.Find("Roguelike2DSurvivalStage");
            if (oldStage != null)
            {
                Object.DestroyImmediate(oldStage);
            }

            return RoguelikeBattleStageView.Ensure();
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
    }
}
