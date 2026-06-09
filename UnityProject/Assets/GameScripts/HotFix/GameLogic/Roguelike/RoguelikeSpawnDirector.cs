using System;
using System.Collections.Generic;
using GameConfig;
using GameConfig.roguelike;

namespace GameLogic
{
    public sealed class RoguelikeSpawnDirector
    {
        private bool _bossSpawned;

        public bool BossSpawned => _bossSpawned;

        public void Reset()
        {
            _bossSpawned = false;
        }

        public RoguelikeSpawnStage GetActiveSpawnStage(Tables tables, float elapsedTime)
        {
            List<RoguelikeSpawnStage> stages = tables?.TbRoguelikeSpawnStage?.DataList;
            if (stages == null || stages.Count <= 0)
            {
                return null;
            }

            RoguelikeSpawnStage active = null;
            for (int i = 0; i < stages.Count; i++)
            {
                RoguelikeSpawnStage stage = stages[i];
                if (stage != null && elapsedTime >= stage.StartTime && (active == null || stage.StartTime > active.StartTime))
                {
                    active = stage;
                }
            }

            return active;
        }

        public RoguelikeEnemy PickEnemyConfig(Tables tables, RoguelikeSpawnStage spawnStage, int wave, float elapsedTime, Random random)
        {
            List<RoguelikeEnemy> enemies = tables?.TbRoguelikeEnemy?.DataList;
            if (enemies == null || enemies.Count <= 0)
            {
                return null;
            }

            random = random ?? new Random();
            if (spawnStage != null)
            {
                if (!_bossSpawned && !string.IsNullOrEmpty(spawnStage.BossEnemyId) && elapsedTime >= spawnStage.BossStartTime)
                {
                    RoguelikeEnemy boss = tables.TbRoguelikeEnemy.GetOrDefault(spawnStage.BossEnemyId);
                    if (boss != null)
                    {
                        _bossSpawned = true;
                        return boss;
                    }
                }

                if (!string.IsNullOrEmpty(spawnStage.EliteEnemyId) && elapsedTime >= spawnStage.EliteStartTime && random.NextDouble() < 0.18)
                {
                    RoguelikeEnemy elite = tables.TbRoguelikeEnemy.GetOrDefault(spawnStage.EliteEnemyId);
                    if (elite != null)
                    {
                        return elite;
                    }
                }

                RoguelikeEnemy weighted = PickWeightedEnemy(tables, spawnStage, random);
                if (weighted != null)
                {
                    return weighted;
                }
            }

            EEnemyTier maxTier = wave >= 8
                ? EEnemyTier.Boss
                : wave >= 4
                    ? EEnemyTier.Elite
                    : EEnemyTier.Common;
            List<RoguelikeEnemy> candidates = new List<RoguelikeEnemy>();
            for (int i = 0; i < enemies.Count; i++)
            {
                RoguelikeEnemy enemy = enemies[i];
                if (enemy != null && (int)enemy.Tier <= (int)maxTier)
                {
                    candidates.Add(enemy);
                }
            }

            return candidates.Count > 0 ? candidates[random.Next(candidates.Count)] : enemies[random.Next(enemies.Count)];
        }

        public RoguelikeEnemy PickWeightedEnemy(Tables tables, RoguelikeSpawnStage spawnStage, Random random)
        {
            if (tables == null || spawnStage?.CommonEnemyIds == null || spawnStage.CommonEnemyIds.Count <= 0)
            {
                return null;
            }

            random = random ?? new Random();
            int totalWeight = 0;
            for (int i = 0; i < spawnStage.CommonEnemyIds.Count; i++)
            {
                int weight = spawnStage.CommonWeights != null && i < spawnStage.CommonWeights.Count ? spawnStage.CommonWeights[i] : 1;
                totalWeight += Math.Max(1, weight);
            }

            int roll = random.Next(Math.Max(1, totalWeight));
            for (int i = 0; i < spawnStage.CommonEnemyIds.Count; i++)
            {
                int weight = spawnStage.CommonWeights != null && i < spawnStage.CommonWeights.Count ? spawnStage.CommonWeights[i] : 1;
                roll -= Math.Max(1, weight);
                if (roll >= 0)
                {
                    continue;
                }

                RoguelikeEnemy enemy = tables.TbRoguelikeEnemy.GetOrDefault(spawnStage.CommonEnemyIds[i]);
                if (enemy != null)
                {
                    return enemy;
                }
            }

            return null;
        }

        public static float GetEnemyMoveSpeed(RoguelikeEnemy config, int wave)
        {
            float baseSpeed = config.Tier == EEnemyTier.Boss
                ? 1.18f
                : config.Tier == EEnemyTier.Elite
                    ? 1.28f
                    : 1.35f;
            if (config.Id.Contains("bat"))
            {
                baseSpeed += 0.24f;
            }

            return baseSpeed + wave * 0.08f;
        }
    }
}
