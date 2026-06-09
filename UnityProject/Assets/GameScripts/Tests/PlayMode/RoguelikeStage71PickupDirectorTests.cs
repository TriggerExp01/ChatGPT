using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace GameLogic.Tests
{
    public sealed class RoguelikeStage71PickupDirectorTests
    {
        [Test]
        public void PickupDirectorCollectsExperienceAndGoldThroughContext()
        {
            RoguelikePickupDirector director = new RoguelikePickupDirector();
            List<RoguelikeSurvivalPickup> pickups = new List<RoguelikeSurvivalPickup>
            {
                new RoguelikeSurvivalPickup(7001, RoguelikePickupType.Experience, Vector2.zero, 3),
                new RoguelikeSurvivalPickup(7002, RoguelikePickupType.Gold, new Vector2(0.2f, 0f), 4),
            };
            int experience = 0;
            int gold = 0;
            int feedbackCount = 0;

            director.UpdatePickups(new RoguelikePickupDirector.Context
            {
                Pickups = pickups,
                PlayerPosition = Vector2.zero,
                AttractRadius = 2.4f,
                GainExperience = amount => experience += amount,
                AddGold = amount => gold += amount,
                ScaleGoldPickupAmount = amount => amount + 1,
                OnPickupCollected = _ => feedbackCount++,
            }, 0.02f);

            Assert.That(pickups, Is.Empty);
            Assert.That(experience, Is.EqualTo(3));
            Assert.That(gold, Is.EqualTo(5));
            Assert.That(feedbackCount, Is.EqualTo(2));
        }

        [Test]
        public void PickupDirectorAttractsButDoesNotCollectDistantPickup()
        {
            RoguelikePickupDirector director = new RoguelikePickupDirector();
            RoguelikeSurvivalPickup pickup = new RoguelikeSurvivalPickup(7003, RoguelikePickupType.Experience, new Vector2(1.6f, 0f), 2);
            List<RoguelikeSurvivalPickup> pickups = new List<RoguelikeSurvivalPickup> { pickup };
            Vector2 before = pickup.Position;
            int experience = 0;

            director.UpdatePickups(new RoguelikePickupDirector.Context
            {
                Pickups = pickups,
                PlayerPosition = Vector2.zero,
                AttractRadius = 2.4f,
                GainExperience = amount => experience += amount,
            }, 0.1f);

            Assert.That(pickups.Count, Is.EqualTo(1));
            Assert.That(experience, Is.EqualTo(0));
            Assert.That(pickup.Position.x, Is.LessThan(before.x));
        }
    }
}
