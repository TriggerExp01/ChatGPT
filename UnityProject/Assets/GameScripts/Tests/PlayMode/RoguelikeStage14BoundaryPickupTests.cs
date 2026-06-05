using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using TEngine;
using UnityEngine;

namespace GameLogic.Tests
{
    public sealed class RoguelikeStage14BoundaryPickupTests
    {
        [Test]
        public void PlayerPositionIsClampedToArenaBoundary()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(14001);

            game.SyncPlayerPosition(new Vector2(100f, -100f));

            Assert.AreEqual(RoguelikeGame.ArenaHalfWidth, game.PlayerPosition.x, 0.001f);
            Assert.AreEqual(-RoguelikeGame.ArenaHalfHeight, game.PlayerPosition.y, 0.001f);
        }

        [Test]
        public void NearbyExperiencePickupIsAttractedAndCollected()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(14002);

            List<RoguelikeSurvivalPickup> pickups = GetPickupList(game);
            RoguelikeSurvivalPickup pickup = MemoryPool.Acquire<RoguelikeSurvivalPickup>();
            pickup.Init(14101, RoguelikePickupType.Experience, new Vector2(1f, 0f), 4);
            pickups.Add(pickup);

            game.Tick(0.1f);
            Assert.Less(game.Pickups[0].Position.x, 1f);

            game.Tick(0.1f);

            Assert.AreEqual(0, game.Pickups.Count);
            Assert.AreEqual(4, game.Experience);
        }

        private static List<RoguelikeSurvivalPickup> GetPickupList(RoguelikeGame game)
        {
            FieldInfo field = typeof(RoguelikeGame).GetField("_pickups", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            return (List<RoguelikeSurvivalPickup>)field.GetValue(game);
        }
    }
}
