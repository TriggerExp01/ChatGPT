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
                maxRooms: 12,
                eliteInterval: 4,
                playerStats: new RoguelikeStats(maxHealth: 105, attack: 12, defense: 3, critChance: 0.1f, critMultiplier: 1.5f));
        }
    }
}
