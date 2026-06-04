namespace GameLogic
{
    public sealed class RoguelikeRunConfig
    {
        public int Seed { get; }

        public int MaxRooms { get; }

        public int EliteInterval { get; }

        public RoguelikeStats PlayerStats { get; }

        public RoguelikeRunConfig(int seed, int maxRooms, int eliteInterval, RoguelikeStats playerStats)
        {
            Seed = seed;
            MaxRooms = maxRooms;
            EliteInterval = eliteInterval;
            PlayerStats = playerStats;
        }

        public static RoguelikeRunConfig CreateDefault(int seed)
        {
            return new RoguelikeRunConfig(
                seed,
                maxRooms: 7,
                eliteInterval: 3,
                playerStats: new RoguelikeStats(maxHealth: 120, attack: 14, defense: 4, critChance: 0.12f, critMultiplier: 1.5f));
        }
    }
}
