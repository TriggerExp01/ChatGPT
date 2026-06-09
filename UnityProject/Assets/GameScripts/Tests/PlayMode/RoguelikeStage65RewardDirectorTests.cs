using System.Collections.Generic;
using System.IO;
using System.Reflection;
using GameConfig;
using Luban;
using NUnit.Framework;
using UnityEngine;

namespace GameLogic.Tests
{
    public sealed class RoguelikeStage65RewardDirectorTests
    {
        [Test]
        public void Stage65RewardDirectorBuildsLevelUpPoolWithoutRoguelikeGameState()
        {
            RoguelikeRewardDirector director = new RoguelikeRewardDirector();
            RoguelikeRewardDirector.Context context = CreateMinimalContext(LoadTables(), new[]
            {
                new RoguelikeSurvivalWeapon(RoguelikeWeaponType.MagicBolt, "追踪魔弹", 1),
            });

            List<RoguelikeChoiceOption> pool = director.CreateLevelUpChoicePool(context);

            Assert.NotNull(FindChoice(pool, "atk_2"));
            Assert.NotNull(FindChoice(pool, "speed"));
            Assert.NotNull(FindChoice(pool, "range"));
            Assert.NotNull(FindChoice(pool, "frequency"));
            Assert.NotNull(FindChoice(pool, "paid_field_ration"));
            Assert.NotNull(FindChoice(pool, "weapon_magic_bolt_upgrade"));
            Assert.NotNull(FindChoice(pool, "weapon_spinning_blade_unlock"));
            Assert.NotNull(FindChoice(pool, "passive_vital_sigil"));
        }

        [Test]
        public void Stage65RewardDirectorDrawsThreeUniqueOptionsFromPool()
        {
            RoguelikeRewardDirector director = new RoguelikeRewardDirector();
            RoguelikeRewardDirector.Context context = CreateMinimalContext(LoadTables(), new[]
            {
                new RoguelikeSurvivalWeapon(RoguelikeWeaponType.MagicBolt, "追踪魔弹", 1),
            });
            List<RoguelikeChoiceOption> rewardOptions = new List<RoguelikeChoiceOption>();

            director.BuildLevelUpOptions(rewardOptions, context, new System.Random(65001));

            Assert.That(rewardOptions.Count, Is.EqualTo(3));
            Assert.That(new HashSet<string>(new[] { rewardOptions[0].Id, rewardOptions[1].Id, rewardOptions[2].Id }).Count, Is.EqualTo(3));
        }

        [Test]
        public void Stage65RoguelikeGamePrivateRewardBoundaryStillDelegatesToRewardDirector()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            game.StartNewRun(65002);
            SetConfigTables(game, LoadTables());

            List<RoguelikeChoiceOption> pool = BuildPassiveChoices(game);
            float moveSpeed = game.MoveSpeed;
            float pickupRadius = game.PickupAttractRadius;

            RoguelikeChoiceOption starlight = FindChoice(pool, "passive_starlight_soles");
            starlight.Apply.Invoke(game.CurrentRun);

            Assert.That(game.MoveSpeed, Is.EqualTo(moveSpeed + 0.2f).Within(0.001f));
            Assert.That(game.PickupAttractRadius, Is.EqualTo(pickupRadius + 0.4f).Within(0.001f));
            Assert.That(game.PassiveSummary, Does.Contain("星辉轻靴"));
        }

        private static RoguelikeRewardDirector.Context CreateMinimalContext(Tables tables, IReadOnlyList<RoguelikeSurvivalWeapon> weapons)
        {
            float moveSpeed = 4.5f;
            float attackRange = 4f;
            float attackInterval = 0.55f;
            float pickupRadius = 2.4f;
            float projectileDamageMultiplier = 1f;

            return new RoguelikeRewardDirector.Context
            {
                Tables = tables,
                Weapons = weapons,
                AddMoveSpeedReward = () => moveSpeed *= 1.1f,
                AddAttackRangeReward = () => attackRange *= 1.15f,
                AddAttackFrequencyReward = () => attackInterval = Mathf.Max(0.15f, attackInterval * 0.88f),
                AddFallbackPickupRadiusPassive = () => pickupRadius += 0.8f,
                AddFallbackProjectileDamagePassive = () => projectileDamageMultiplier += 0.12f,
                AddFallbackMoveSpeedPassive = () => moveSpeed *= 1.08f,
                ApplyConfigEffects = ApplyConfigEffects,
                AddPassive = (run, id, displayName, description, apply) => run?.AddRelic(new RoguelikeRelicTemplate(id, displayName, description, apply)),
                AddOrUpgradeWeapon = type => { },
            };
        }

        private static void ApplyConfigEffects(RoguelikeRunState run, IReadOnlyList<GameConfig.roguelike.Effect> effects)
        {
            if (run == null || effects == null)
            {
                return;
            }

            for (int i = 0; i < effects.Count; i++)
            {
                GameConfig.roguelike.Effect effect = effects[i];
                switch (effect.Type)
                {
                    case GameConfig.roguelike.EEffectType.AddAttack:
                        run.Player.Stats.AddAttack(Mathf.RoundToInt(effect.Value));
                        break;
                    case GameConfig.roguelike.EEffectType.AddDefense:
                        run.Player.Stats.AddDefense(Mathf.RoundToInt(effect.Value));
                        break;
                    case GameConfig.roguelike.EEffectType.Heal:
                        run.Player.Heal(Mathf.RoundToInt(effect.Value));
                        break;
                    case GameConfig.roguelike.EEffectType.AddMaxHealth:
                        run.Player.Stats.AddMaxHealth(Mathf.RoundToInt(effect.Value));
                        break;
                    case GameConfig.roguelike.EEffectType.AddCritChance:
                        run.Player.Stats.AddCritChance(effect.Value);
                        break;
                    case GameConfig.roguelike.EEffectType.AddGold:
                        run.AddGold(Mathf.RoundToInt(effect.Value));
                        break;
                }
            }
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

        private static List<RoguelikeChoiceOption> BuildPassiveChoices(RoguelikeGame game)
        {
            List<RoguelikeChoiceOption> pool = new List<RoguelikeChoiceOption>();
            MethodInfo method = typeof(RoguelikeGame).GetMethod("AddPassiveChoices", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(method);
            method.Invoke(game, new object[] { pool });
            return pool;
        }

        private static RoguelikeChoiceOption FindChoice(List<RoguelikeChoiceOption> choices, string id)
        {
            for (int i = 0; i < choices.Count; i++)
            {
                if (choices[i].Id == id)
                {
                    return choices[i];
                }
            }

            Assert.Fail($"Choice {id} not found.");
            return null;
        }

        private static void SetConfigTables(RoguelikeGame game, Tables tables)
        {
            FieldInfo field = typeof(RoguelikeGame).GetField("_configTables", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.NotNull(field);
            field.SetValue(game, tables);
        }
    }
}
