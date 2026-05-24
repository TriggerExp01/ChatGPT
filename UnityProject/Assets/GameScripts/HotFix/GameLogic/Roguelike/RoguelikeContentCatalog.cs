using System.Collections.Generic;

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

        public static RoguelikeContentCatalog CreateDefault()
        {
            return new RoguelikeContentCatalog(
                new[]
                {
                    new RoguelikeEnemyTemplate("slime", "软泥怪", new RoguelikeStats(22, 5, 0, 0.02f, 1.5f)),
                    new RoguelikeEnemyTemplate("bat", "洞穴蝙蝠", new RoguelikeStats(18, 7, 0, 0.12f, 1.5f)),
                    new RoguelikeEnemyTemplate("guard", "遗迹守卫", new RoguelikeStats(30, 6, 2, 0.04f, 1.5f)),
                },
                new[]
                {
                    new RoguelikeEnemyTemplate("blade_dancer", "刃舞者", new RoguelikeStats(42, 9, 1, 0.15f, 1.6f)),
                    new RoguelikeEnemyTemplate("stone_idol", "石像卫士", new RoguelikeStats(50, 8, 3, 0.06f, 1.5f)),
                },
                new RoguelikeEnemyTemplate("dungeon_heart", "地牢之心", new RoguelikeStats(92, 11, 2, 0.1f, 1.6f)),
                new[]
                {
                    new RoguelikeRelicTemplate("iron_bark", "铁木皮", "防御 +2", run => run.Player.Stats.AddDefense(2)),
                    new RoguelikeRelicTemplate("glass_knife", "玻璃短刃", "攻击 +4", run => run.Player.Stats.AddAttack(4)),
                    new RoguelikeRelicTemplate("blood_vial", "血瓶", "生命上限 +12，并恢复 12 点生命", run =>
                    {
                        run.Player.Stats.AddMaxHealth(12);
                        run.Player.Heal(12);
                    }),
                    new RoguelikeRelicTemplate("lucky_coin", "幸运硬币", "暴击 +8%", run => run.Player.Stats.AddCritChance(0.08f)),
                });
        }
    }
}
