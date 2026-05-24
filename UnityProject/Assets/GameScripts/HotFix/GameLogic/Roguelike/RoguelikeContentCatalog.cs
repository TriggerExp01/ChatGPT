using System.Collections.Generic;
using GameConfig;
using GameConfig.roguelike;

namespace GameLogic
{
    public sealed class RoguelikeContentCatalog
    {
        public IReadOnlyList<RoguelikeEnemyTemplate> CommonEnemies { get; }

        public IReadOnlyList<RoguelikeEnemyTemplate> EliteEnemies { get; }

        public RoguelikeEnemyTemplate BossEnemy { get; }

        public IReadOnlyList<RoguelikeRelicTemplate> Relics { get; }

        public RoguelikeContentCatalog(
            IReadOnlyList<RoguelikeEnemyTemplate> commonEnemies,
            IReadOnlyList<RoguelikeEnemyTemplate> eliteEnemies,
            RoguelikeEnemyTemplate bossEnemy,
            IReadOnlyList<RoguelikeRelicTemplate> relics)
        {
            CommonEnemies = commonEnemies;
            EliteEnemies = eliteEnemies;
            BossEnemy = bossEnemy;
            Relics = relics;
        }

        public static RoguelikeContentCatalog CreateFromLuban(Tables tables)
        {
            List<RoguelikeEnemyTemplate> commonEnemies = new List<RoguelikeEnemyTemplate>();
            List<RoguelikeEnemyTemplate> eliteEnemies = new List<RoguelikeEnemyTemplate>();
            RoguelikeEnemyTemplate bossEnemy = null;
            foreach (RoguelikeEnemy row in tables.TbRoguelikeEnemy.DataList)
            {
                RoguelikeEnemyTemplate enemy = BuildEnemy(row);
                switch (row.Tier)
                {
                    case EEnemyTier.Common:
                        commonEnemies.Add(enemy);
                        break;
                    case EEnemyTier.Elite:
                        eliteEnemies.Add(enemy);
                        break;
                    case EEnemyTier.Boss:
                        bossEnemy = enemy;
                        break;
                }
            }

            return new RoguelikeContentCatalog(
                commonEnemies,
                eliteEnemies,
                bossEnemy,
                BuildRelics(tables.TbRoguelikeRelic.DataList));
        }

        private static RoguelikeEnemyTemplate BuildEnemy(RoguelikeEnemy row)
        {
            return new RoguelikeEnemyTemplate(
                row.Id,
                row.DisplayName,
                new RoguelikeStats(row.MaxHealth, row.Attack, row.Defense, row.CritChance, row.CritMultiplier));
        }

        private static RoguelikeRelicTemplate[] BuildRelics(IReadOnlyList<RoguelikeRelic> rows)
        {
            RoguelikeRelicTemplate[] result = new RoguelikeRelicTemplate[rows.Count];
            for (int i = 0; i < rows.Count; i++)
            {
                RoguelikeRelic row = rows[i];
                result[i] = new RoguelikeRelicTemplate(row.Id, row.DisplayName, row.Desc, run => ApplyEffects(run, row.Effects));
            }

            return result;
        }

        private static void ApplyEffects(RoguelikeRunState run, IReadOnlyList<Effect> effects)
        {
            if (effects == null)
            {
                return;
            }

            for (int i = 0; i < effects.Count; i++)
            {
                RoguelikeEffectResolver.Apply(run, effects[i]);
            }
        }
    }
}
