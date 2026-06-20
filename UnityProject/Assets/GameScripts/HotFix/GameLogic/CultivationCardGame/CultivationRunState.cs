using System;
using System.Collections.Generic;

namespace GameLogic.Cultivation
{
    public sealed class CultivationRunState
    {
        public const int DefaultPillSlotLimit = 3;

        public CultivationRunState(IEnumerable<CardDefinition> deck, IEnumerable<CultivationRunNode> route, int playerMaxHp = 100, int? playerCurrentHp = null, int initialSpiritStones = 0)
        {
            Deck = new List<CardDefinition>(deck ?? throw new ArgumentNullException(nameof(deck)));
            Route = new List<CultivationRunNode>(route ?? throw new ArgumentNullException(nameof(route))).AsReadOnly();
            CurrentRewards = new List<CultivationRunReward>();
            ClaimedRewards = new List<CultivationRunReward>();
            RestUpgradeChoices = new List<CultivationRestUpgradeChoice>();
            CurrentRouteChoices = new List<CultivationRunRouteChoice>();
            CurrentMarketItems = new List<CultivationMarketItem>();
            PurchasedMarketItems = new List<CultivationMarketItem>();
            Artifacts = new List<ArtifactDefinition>();
            PurchasedMarketArtifacts = new List<ArtifactDefinition>();
            DroppedArtifacts = new List<ArtifactDefinition>();
            ChestArtifacts = new List<ArtifactDefinition>();
            MysticEventChoices = new List<MysticEventOption>();
            ResolvedMysticEventOptions = new List<MysticEventOption>();
            Pills = new List<PillDefinition>();
            PurchasedMarketPills = new List<PillDefinition>();
            RemovedMarketCards = new List<CardDefinition>();
            MarketUpgradedCards = new List<CardDefinition>();
            SoldMarketCards = new List<CardDefinition>();

            if (Deck.Count == 0)
            {
                throw new ArgumentException("Run deck cannot be empty.", nameof(deck));
            }

            if (Route.Count == 0)
            {
                throw new ArgumentException("Run route cannot be empty.", nameof(route));
            }

            if (playerMaxHp <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(playerMaxHp), "Player max HP must be positive.");
            }

            if (playerCurrentHp.HasValue && (playerCurrentHp.Value <= 0 || playerCurrentHp.Value > playerMaxHp))
            {
                throw new ArgumentOutOfRangeException(nameof(playerCurrentHp), "Player current HP must be between 1 and max HP.");
            }

            PlayerMaxHp = playerMaxHp;
            PlayerCurrentHp = playerCurrentHp ?? playerMaxHp;
            SpiritStones = Math.Max(0, initialSpiritStones);
            PillSlotLimit = DefaultPillSlotLimit;
        }

        public List<CardDefinition> Deck { get; }

        public IReadOnlyList<CultivationRunNode> Route { get; }

        public int CurrentNodeIndex { get; set; }

        public int PlayerMaxHp { get; private set; }

        public int PlayerCurrentHp { get; set; }

        public int SpiritStones { get; set; }

        public CultivationRunStatus Status { get; set; }

        public BattleState CurrentBattle { get; set; }

        public List<CultivationRunReward> CurrentRewards { get; }

        public List<CultivationRunReward> ClaimedRewards { get; }

        public List<CultivationRestUpgradeChoice> RestUpgradeChoices { get; }

        public List<CultivationRunRouteChoice> CurrentRouteChoices { get; }

        public List<CultivationMarketItem> CurrentMarketItems { get; }

        public List<CultivationMarketItem> PurchasedMarketItems { get; }

        public List<ArtifactDefinition> Artifacts { get; }

        public List<ArtifactDefinition> PurchasedMarketArtifacts { get; }

        public List<ArtifactDefinition> DroppedArtifacts { get; }

        public List<ArtifactDefinition> ChestArtifacts { get; }

        public List<MysticEventOption> MysticEventChoices { get; }

        public List<MysticEventOption> ResolvedMysticEventOptions { get; }

        public int PillSlotLimit { get; }

        public List<PillDefinition> Pills { get; }

        public List<PillDefinition> PurchasedMarketPills { get; }

        public List<CardDefinition> RemovedMarketCards { get; }

        public List<CardDefinition> MarketUpgradedCards { get; }

        public List<CardDefinition> SoldMarketCards { get; }

        public CultivationRunNode CurrentNode => Route[CurrentNodeIndex];

        public void IncreasePlayerMaxHp(int amount)
        {
            var increase = Math.Max(0, amount);
            PlayerMaxHp += increase;
            PlayerCurrentHp = Math.Min(PlayerMaxHp, PlayerCurrentHp + increase);
        }
    }
}
