using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using TEngine;
using UnityEngine;

namespace GameLogic.Tests
{
    public sealed class RoguelikeStage50PickupPresentationTests
    {
        [Test]
        public void Stage50PickupViewsUseFormalSpritesForExperienceAndGold()
        {
            RoguelikeGame game = PrepareRun(50001);
            List<RoguelikeSurvivalPickup> pickups = GetPickupList(game);
            pickups.Add(CreatePickup(50011, RoguelikePickupType.Experience, new Vector2(-0.6f, 0.2f), 4));
            pickups.Add(CreatePickup(50012, RoguelikePickupType.Gold, new Vector2(0.6f, 0.2f), 2));

            RoguelikeBattleStageView view = RoguelikeBattleStageView.Ensure();
            view.Refresh(game.CurrentRun);

            SpriteRenderer experience = FindPickupRenderer(view.transform, "掉落_50011");
            SpriteRenderer gold = FindPickupRenderer(view.transform, "掉落_50012");

            Assert.That(experience.sprite.name, Is.EqualTo("Roguelike_UI_Icon_RelicGrowth"));
            Assert.That(gold.sprite.name, Is.EqualTo("Roguelike_UI_GoldIcon"));
            Assert.That(Mathf.Abs(experience.transform.localEulerAngles.z), Is.LessThan(0.01f));
            Assert.That(Mathf.Abs(gold.transform.localEulerAngles.z), Is.LessThan(0.01f));
            Assert.That(experience.sortingOrder, Is.EqualTo(7));
            Assert.That(gold.sortingOrder, Is.EqualTo(7));

            ReleaseAndClear(pickups);
        }

        [Test]
        public void Stage50PickupViewReuseReappliesSpriteAfterFallback()
        {
            RoguelikeGame game = PrepareRun(50002);
            RoguelikeBattleStageView view = RoguelikeBattleStageView.Ensure();
            List<RoguelikeSurvivalPickup> pickups = GetPickupList(game);

            view.DebugSetPickupSpriteAddressOverride(RoguelikePickupType.Gold, "Missing_Roguelike_GoldPickup");
            pickups.Add(CreatePickup(50021, RoguelikePickupType.Gold, new Vector2(0.2f, 0f), 2));
            view.Refresh(game.CurrentRun);

            int fallbackCount = view.PresentationFallbackCount;
            SpriteRenderer fallback = FindPickupRenderer(view.transform, "掉落_50021");
            Assert.That(fallbackCount, Is.GreaterThanOrEqualTo(1));
            Assert.That(fallback.sprite.name, Is.Not.EqualTo("Roguelike_UI_GoldIcon"));
            Assert.That(Mathf.DeltaAngle(fallback.transform.localEulerAngles.z, 45f), Is.EqualTo(0f).Within(0.01f));

            ReleaseAndClear(pickups);
            view.Refresh(game.CurrentRun);
            Assert.That(view.PickupViewPoolCount, Is.GreaterThanOrEqualTo(1));

            int reuseBefore = view.ViewReuseCount;
            pickups.Add(CreatePickup(50022, RoguelikePickupType.Experience, new Vector2(-0.2f, 0f), 4));
            view.Refresh(game.CurrentRun);

            SpriteRenderer reused = FindPickupRenderer(view.transform, "掉落_50022");
            Assert.That(view.ViewReuseCount, Is.GreaterThanOrEqualTo(reuseBefore + 1));
            Assert.That(reused.sprite.name, Is.EqualTo("Roguelike_UI_Icon_RelicGrowth"));
            Assert.That(Mathf.Abs(reused.transform.localEulerAngles.z), Is.LessThan(0.01f));
            Assert.That(view.PickupViewPoolCount, Is.EqualTo(0));

            ReleaseAndClear(pickups);
        }

        private static RoguelikeGame PrepareRun(int seed)
        {
            GameObject oldStage = GameObject.Find("Roguelike2DSurvivalStage");
            if (oldStage != null)
            {
                Object.DestroyImmediate(oldStage);
            }

            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(seed);
            MemoryPool.ClearAll();
            return game;
        }

        private static RoguelikeSurvivalPickup CreatePickup(int id, RoguelikePickupType type, Vector2 position, int amount)
        {
            RoguelikeSurvivalPickup pickup = MemoryPool.Acquire<RoguelikeSurvivalPickup>();
            pickup.Init(id, type, position, amount);
            return pickup;
        }

        private static SpriteRenderer FindPickupRenderer(Transform root, string name)
        {
            Transform[] children = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i].name == name)
                {
                    SpriteRenderer renderer = children[i].GetComponent<SpriteRenderer>();
                    Assert.NotNull(renderer);
                    Assert.NotNull(renderer.sprite);
                    return renderer;
                }
            }

            Assert.Fail("Missing pickup view: " + name);
            return null;
        }

        private static List<RoguelikeSurvivalPickup> GetPickupList(RoguelikeGame game)
        {
            FieldInfo field = typeof(RoguelikeGame).GetField("_pickups", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            return (List<RoguelikeSurvivalPickup>)field.GetValue(game);
        }

        private static void ReleaseAndClear(List<RoguelikeSurvivalPickup> pickups)
        {
            for (int i = 0; i < pickups.Count; i++)
            {
                MemoryPool.Release(pickups[i]);
            }

            pickups.Clear();
        }
    }
}
