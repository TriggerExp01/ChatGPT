using System;
using System.Collections.Generic;

namespace GameLogic.Cultivation
{
    public sealed class BattleState
    {
        public BattleState(CombatantState player, IEnumerable<CardDefinition> drawPile, IEnumerable<EnemyState> enemies, int spiritMax = 3, int handLimit = 5)
        {
            Player = player ?? throw new ArgumentNullException(nameof(player));
            DrawPile = new List<CardDefinition>(drawPile ?? throw new ArgumentNullException(nameof(drawPile)));
            Enemies = new List<EnemyState>(enemies ?? throw new ArgumentNullException(nameof(enemies)));
            DiscardPile = new List<CardDefinition>();
            ExhaustPile = new List<CardDefinition>();
            Hand = new List<CardDefinition>();
            Logs = new List<BattleLogEntry>();
            SpiritMax = spiritMax;
            HandLimit = handLimit;
            Spirit = spiritMax;
        }

        public CombatantState Player { get; }

        public List<CardDefinition> DrawPile { get; }

        public List<CardDefinition> DiscardPile { get; }

        public List<CardDefinition> ExhaustPile { get; }

        public List<CardDefinition> Hand { get; }

        public List<EnemyState> Enemies { get; }

        public List<BattleLogEntry> Logs { get; }

        public int SpiritMax { get; }

        public int Spirit { get; set; }

        public int HandLimit { get; }

        public int SpiritCostReduction { get; private set; }

        public int TurnNumber { get; set; }

        public BattleOutcome Outcome { get; set; } = BattleOutcome.InProgress;

        public void AddSpiritCostReduction(int amount)
        {
            SpiritCostReduction += Math.Max(0, amount);
        }

        public int GetEffectiveSpiritCost(CardDefinition card)
        {
            if (card == null)
            {
                throw new ArgumentNullException(nameof(card));
            }

            return Math.Max(0, card.SpiritCost - SpiritCostReduction);
        }
    }
}
