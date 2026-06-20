using System;
using System.Collections.Generic;

namespace GameLogic.Cultivation
{
    public sealed class CultivationRunState
    {
        public CultivationRunState(IEnumerable<CardDefinition> deck, IEnumerable<CultivationRunNode> route, int playerMaxHp = 100, int? playerCurrentHp = null, int initialSpiritStones = 0)
        {
            Deck = new List<CardDefinition>(deck ?? throw new ArgumentNullException(nameof(deck)));
            Route = new List<CultivationRunNode>(route ?? throw new ArgumentNullException(nameof(route))).AsReadOnly();
            CurrentRewards = new List<CultivationRunReward>();
            ClaimedRewards = new List<CultivationRunReward>();
            RestUpgradeChoices = new List<CultivationRestUpgradeChoice>();
            CurrentRouteChoices = new List<CultivationRunRouteChoice>();

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
        }

        public List<CardDefinition> Deck { get; }

        public IReadOnlyList<CultivationRunNode> Route { get; }

        public int CurrentNodeIndex { get; set; }

        public int PlayerMaxHp { get; }

        public int PlayerCurrentHp { get; set; }

        public int SpiritStones { get; set; }

        public CultivationRunStatus Status { get; set; }

        public BattleState CurrentBattle { get; set; }

        public List<CultivationRunReward> CurrentRewards { get; }

        public List<CultivationRunReward> ClaimedRewards { get; }

        public List<CultivationRestUpgradeChoice> RestUpgradeChoices { get; }

        public List<CultivationRunRouteChoice> CurrentRouteChoices { get; }

        public CultivationRunNode CurrentNode => Route[CurrentNodeIndex];
    }
}
