using System;
using System.Collections.Generic;

namespace GameLogic.Cultivation
{
    public sealed class CultivationRunState
    {
        public CultivationRunState(IEnumerable<CardDefinition> deck, IEnumerable<CultivationRunNode> route)
        {
            Deck = new List<CardDefinition>(deck ?? throw new ArgumentNullException(nameof(deck)));
            Route = new List<CultivationRunNode>(route ?? throw new ArgumentNullException(nameof(route))).AsReadOnly();
            CurrentRewards = new List<CultivationRunReward>();
            ClaimedRewards = new List<CultivationRunReward>();

            if (Deck.Count == 0)
            {
                throw new ArgumentException("Run deck cannot be empty.", nameof(deck));
            }

            if (Route.Count == 0)
            {
                throw new ArgumentException("Run route cannot be empty.", nameof(route));
            }
        }

        public List<CardDefinition> Deck { get; }

        public IReadOnlyList<CultivationRunNode> Route { get; }

        public int CurrentNodeIndex { get; set; }

        public CultivationRunStatus Status { get; set; }

        public BattleState CurrentBattle { get; set; }

        public List<CultivationRunReward> CurrentRewards { get; }

        public List<CultivationRunReward> ClaimedRewards { get; }

        public CultivationRunNode CurrentNode => Route[CurrentNodeIndex];
    }
}
