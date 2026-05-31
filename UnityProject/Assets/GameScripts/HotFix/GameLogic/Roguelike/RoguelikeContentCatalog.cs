using System.Collections.Generic;
using GameConfig;
using GameConfig.roguelike;

namespace GameLogic
{
    public sealed class RoguelikeContentCatalog
    {
        private static readonly Dictionary<string, string> EnemyNames = new Dictionary<string, string>
        {
            { "slime", "黏液怪" },
            { "bat", "洞窟蝙蝠" },
            { "guard", "遗迹守卫" },
            { "blade_dancer", "刃舞者" },
            { "stone_idol", "石像巨兵" },
            { "dungeon_heart", "地牢之心" },
        };

        private static readonly Dictionary<string, string> RelicNames = new Dictionary<string, string>
        {
            { "iron_bark", "铁木皮" },
            { "glass_knife", "玻璃短刃" },
            { "blood_vial", "血瓶" },
            { "lucky_coin", "幸运金币" },
        };

        private static readonly Dictionary<string, string> RelicDescriptions = new Dictionary<string, string>
        {
            { "iron_bark", "防御 +2" },
            { "glass_knife", "攻击 +4" },
            { "blood_vial", "最大生命 +12，并恢复 12 生命" },
            { "lucky_coin", "暴击 +8%" },
        };

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
                ResolveText(EnemyNames, row.Id, row.DisplayName),
                new RoguelikeStats(row.MaxHealth, row.Attack, row.Defense, row.CritChance, row.CritMultiplier));
        }

        private static RoguelikeRelicTemplate[] BuildRelics(IReadOnlyList<RoguelikeRelic> rows)
        {
            RoguelikeRelicTemplate[] result = new RoguelikeRelicTemplate[rows.Count];
            for (int i = 0; i < rows.Count; i++)
            {
                RoguelikeRelic row = rows[i];
                result[i] = new RoguelikeRelicTemplate(
                    row.Id,
                    ResolveText(RelicNames, row.Id, row.DisplayName),
                    ResolveText(RelicDescriptions, row.Id, row.Desc),
                    run => ApplyEffects(run, row.Effects));
            }

            return result;
        }

        private static string ResolveText(Dictionary<string, string> values, string id, string fallback)
        {
            return !string.IsNullOrEmpty(id) && values.TryGetValue(id, out string value) ? value : fallback;
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
