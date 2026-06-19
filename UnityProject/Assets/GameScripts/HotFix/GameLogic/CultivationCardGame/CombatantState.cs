using System;

namespace GameLogic.Cultivation
{
    public sealed class CombatantState
    {
        public CombatantState(string name, int maxHp, int defense = 0)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Combatant name is required.", nameof(name));
            }

            if (maxHp <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxHp), "Max HP must be positive.");
            }

            Name = name;
            MaxHp = maxHp;
            CurrentHp = maxHp;
            Defense = Math.Max(0, defense);
        }

        public string Name { get; }

        public int MaxHp { get; }

        public int CurrentHp { get; private set; }

        public int Shield { get; private set; }

        public int Defense { get; private set; }

        public int BreakDefenseStacks { get; private set; }

        public int BurnStacks { get; private set; }

        public int BurnTurns { get; private set; }

        public bool IsDefeated => CurrentHp <= 0;

        public void AddShield(int amount)
        {
            Shield += Math.Max(0, amount);
        }

        public void ClearShield()
        {
            Shield = 0;
        }

        public void Heal(int amount)
        {
            CurrentHp = Math.Min(MaxHp, CurrentHp + Math.Max(0, amount));
        }

        public int TakeDamage(int incomingDamage)
        {
            var mitigated = Math.Max(0, incomingDamage - Defense);
            if (BreakDefenseStacks > 0)
            {
                mitigated += BreakDefenseStacks * 2;
            }

            var shieldBlocked = Math.Min(Shield, mitigated);
            Shield -= shieldBlocked;

            var hpDamage = mitigated - shieldBlocked;
            CurrentHp = Math.Max(0, CurrentHp - hpDamage);
            return hpDamage;
        }

        public void AddBreakDefense(int stacks)
        {
            BreakDefenseStacks += Math.Max(0, stacks);
        }

        public void AddBurn(int stacks, int turns)
        {
            BurnStacks += Math.Max(0, stacks);
            BurnTurns = Math.Max(BurnTurns, turns);
        }

        public int ResolveBurnAtTurnStart()
        {
            if (BurnStacks <= 0 || BurnTurns <= 0)
            {
                return 0;
            }

            var damage = BurnStacks;
            CurrentHp = Math.Max(0, CurrentHp - damage);
            BurnTurns--;
            if (BurnTurns == 0)
            {
                BurnStacks = 0;
            }

            return damage;
        }
    }
}
