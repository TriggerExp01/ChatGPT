using NUnit.Framework;
using TEngine;
using UnityEngine;

namespace GameLogic.Tests
{
    public sealed class RoguelikeStage38PoolingStressTests
    {
        [Test]
        public void Stage38DenseRuntimeObjectsReturnToMemoryPoolsOnRestart()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(38001);
            MemoryPool.ClearAll();

            RoguelikePerformanceSnapshot active = game.DebugPopulateDenseRuntimeObjects(96, 48, 120);

            Assert.That(active.EnemyCount, Is.EqualTo(96));
            Assert.That(active.PickupCount, Is.EqualTo(48));
            Assert.That(active.ProjectileCount, Is.EqualTo(120));
            Assert.That(active.EnemyPoolUsingCount, Is.EqualTo(96));
            Assert.That(active.PickupPoolUsingCount, Is.EqualTo(48));
            Assert.That(active.ProjectilePoolUsingCount, Is.EqualTo(120));

            game.StartNewRun(38002);
            RoguelikePerformanceSnapshot released = game.CapturePerformanceSnapshot(0.1f);

            Assert.That(released.EnemyCount, Is.EqualTo(0));
            Assert.That(released.PickupCount, Is.EqualTo(0));
            Assert.That(released.ProjectileCount, Is.EqualTo(0));
            Assert.That(released.EnemyPoolUsingCount, Is.EqualTo(0));
            Assert.That(released.PickupPoolUsingCount, Is.EqualTo(0));
            Assert.That(released.ProjectilePoolUsingCount, Is.EqualTo(0));
            Assert.That(released.EnemyPoolUnusedCount, Is.GreaterThanOrEqualTo(96));
            Assert.That(released.PickupPoolUnusedCount, Is.GreaterThanOrEqualTo(48));
            Assert.That(released.ProjectilePoolUnusedCount, Is.GreaterThanOrEqualTo(120));
        }

        [Test]
        public void Stage38DenseStageViewsReturnToPoolsAndReuse()
        {
            RoguelikeBattleStageView view = CreateFreshStage();
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(38003);
            MemoryPool.ClearAll();

            game.DebugPopulateDenseRuntimeObjects(72, 36, 96);
            view.Refresh(game.CurrentRun);
            RoguelikePerformanceSnapshot active = game.CapturePerformanceSnapshot(0.1f, view);

            Assert.That(active.EnemyCount, Is.EqualTo(72));
            Assert.That(active.PickupCount, Is.EqualTo(36));
            Assert.That(active.ProjectileCount, Is.EqualTo(96));

            game.StartNewRun(38004);
            view.Refresh(game.CurrentRun);
            RoguelikePerformanceSnapshot pooled = game.CapturePerformanceSnapshot(0.1f, view);

            Assert.That(pooled.PooledViewCount, Is.GreaterThanOrEqualTo(72 + 36 + 96));
            int reuseBefore = pooled.ViewReuseCount;

            game.DebugPopulateDenseRuntimeObjects(24, 12, 32);
            view.Refresh(game.CurrentRun);
            RoguelikePerformanceSnapshot reused = game.CapturePerformanceSnapshot(0.1f, view);

            Assert.That(reused.ViewReuseCount, Is.GreaterThanOrEqualTo(reuseBefore + 24 + 12 + 32));
            Assert.That(reused.PooledViewCount, Is.GreaterThanOrEqualTo((72 - 24) + (36 - 12) + (96 - 32)));
        }

        [Test]
        public void Stage38DensePopulationClampsToSafeStressLimits()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(38005);
            MemoryPool.ClearAll();

            RoguelikePerformanceSnapshot snapshot = game.DebugPopulateDenseRuntimeObjects(999, 999, 999);

            Assert.That(snapshot.EnemyCount, Is.EqualTo(160));
            Assert.That(snapshot.PickupCount, Is.EqualTo(160));
            Assert.That(snapshot.ProjectileCount, Is.EqualTo(240));
            Assert.That(snapshot.EnemyPoolUsingCount, Is.EqualTo(160));
            Assert.That(snapshot.PickupPoolUsingCount, Is.EqualTo(160));
            Assert.That(snapshot.ProjectilePoolUsingCount, Is.EqualTo(240));
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
    }
}
