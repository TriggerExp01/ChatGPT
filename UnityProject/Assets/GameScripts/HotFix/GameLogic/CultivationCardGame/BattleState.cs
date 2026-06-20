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

        public int ExtraDrawPerTurn { get; private set; }

        public int ChargedDamageMultiplier { get; private set; } = 1;

        public int ChargedDamageUses { get; private set; }

        public bool HasTriggeredCriticalThisTurn { get; private set; }

        public int DodgeCharges { get; private set; }

        public int DodgeCounterDamage { get; private set; }

        public int AttackCounterDamage { get; private set; }

        public int AttackCounterChancePercent { get; private set; }

        public int PoisonCounterStacks { get; private set; }

        public int PoisonCounterTurns { get; private set; }

        public int RegenerationPerTurn { get; private set; }

        public int RegenerationTurns { get; private set; }

        public bool PoisonDamageTriggeredThisTurn { get; private set; }

        public int TurnNumber { get; set; }

        public BattleOutcome Outcome { get; set; } = BattleOutcome.InProgress;

        public void AddSpiritCostReduction(int amount)
        {
            SpiritCostReduction += Math.Max(0, amount);
        }

        public void AddExtraDrawPerTurn(int amount)
        {
            ExtraDrawPerTurn += Math.Max(0, amount);
        }

        public void AddChargedDamage(int multiplier, int uses = 1)
        {
            if (multiplier <= 1 || uses <= 0)
            {
                return;
            }

            ChargedDamageMultiplier = Math.Max(ChargedDamageMultiplier, multiplier);
            ChargedDamageUses += uses;
        }

        public int TryConsumeChargedDamageMultiplier()
        {
            if (ChargedDamageUses <= 0 || ChargedDamageMultiplier <= 1)
            {
                return 1;
            }

            var multiplier = ChargedDamageMultiplier;
            ChargedDamageUses--;
            if (ChargedDamageUses == 0)
            {
                ChargedDamageMultiplier = 1;
            }

            return multiplier;
        }

        public void MarkCriticalTriggered()
        {
            HasTriggeredCriticalThisTurn = true;
        }

        public void ResetTurnCriticalState()
        {
            HasTriggeredCriticalThisTurn = false;
        }

        public void MarkPoisonDamageTriggered()
        {
            PoisonDamageTriggeredThisTurn = true;
        }

        public void ResetPoisonDamageTriggered()
        {
            PoisonDamageTriggeredThisTurn = false;
        }

        public void AddDodgeCharges(int amount)
        {
            DodgeCharges += Math.Max(0, amount);
        }

        public void AddDodgeCounterDamage(int amount)
        {
            DodgeCounterDamage += Math.Max(0, amount);
        }

        public void AddAttackCounter(int damage, int chancePercent)
        {
            AttackCounterDamage += Math.Max(0, damage);
            var clampedChance = Math.Min(100, Math.Max(0, chancePercent));
            AttackCounterChancePercent = Math.Max(AttackCounterChancePercent, clampedChance);
        }

        public void ClearAttackCounter()
        {
            AttackCounterDamage = 0;
            AttackCounterChancePercent = 0;
        }

        public void AddPoisonCounter(int stacks, int turns)
        {
            PoisonCounterStacks += Math.Max(0, stacks);
            PoisonCounterTurns = Math.Max(PoisonCounterTurns, Math.Max(1, turns));
        }

        public void ResolvePoisonCounterDurationAtTurnStart()
        {
            if (PoisonCounterTurns <= 0)
            {
                PoisonCounterStacks = 0;
                return;
            }

            PoisonCounterTurns--;
            if (PoisonCounterTurns == 0)
            {
                PoisonCounterStacks = 0;
            }
        }

        public void AddRegeneration(int amount, int turns)
        {
            RegenerationPerTurn += Math.Max(0, amount);
            RegenerationTurns = Math.Max(RegenerationTurns, Math.Max(1, turns));
        }

        public int ResolveRegenerationAtTurnStart()
        {
            if (RegenerationPerTurn <= 0 || RegenerationTurns <= 0)
            {
                return 0;
            }

            var before = Player.CurrentHp;
            Player.Heal(RegenerationPerTurn);
            var healed = Player.CurrentHp - before;
            RegenerationTurns--;
            if (RegenerationTurns == 0)
            {
                RegenerationPerTurn = 0;
            }

            return healed;
        }

        public bool TryConsumeDodge()
        {
            if (DodgeCharges <= 0)
            {
                return false;
            }

            DodgeCharges--;
            return true;
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
