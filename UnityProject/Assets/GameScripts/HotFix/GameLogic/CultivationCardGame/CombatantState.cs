using System;

namespace GameLogic.Cultivation
{
    public sealed class CombatantState
    {
        public const int SwordMarkExplosionDamage = 12;

        public CombatantState(string name, int maxHp, int defense = 0, int? currentHp = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Combatant name is required.", nameof(name));
            }

            if (maxHp <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxHp), "Max HP must be positive.");
            }

            if (currentHp.HasValue && (currentHp.Value <= 0 || currentHp.Value > maxHp))
            {
                throw new ArgumentOutOfRangeException(nameof(currentHp), "Current HP must be between 1 and max HP.");
            }

            Name = name;
            MaxHp = maxHp;
            CurrentHp = currentHp ?? maxHp;
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

        public int PoisonStacks { get; private set; }

        public int FreezeStacks { get; private set; }

        public int FreezeTurns { get; private set; }

        public int StunTurns { get; private set; }

        public int SwordMarkStacks { get; private set; }

        public int Sharpness { get; private set; }

        public int SharpnessTurns { get; private set; }

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

        public int TakeDamage(int incomingDamage, int defenseIgnore = 0)
        {
            var effectiveDefense = Math.Max(0, Defense - Math.Max(0, defenseIgnore));
            var mitigated = Math.Max(0, incomingDamage - effectiveDefense);
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

        public int TakeDirectDamage(int incomingDamage)
        {
            var hpDamage = Math.Max(0, incomingDamage);
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

        public void AddPoison(int stacks)
        {
            PoisonStacks = Math.Min(99, PoisonStacks + Math.Max(0, stacks));
        }

        public int ConsumePoison()
        {
            var consumed = PoisonStacks;
            PoisonStacks = 0;
            return consumed;
        }

        public void AddFreeze(int stacks, int turns)
        {
            FreezeStacks += Math.Max(0, stacks);
            FreezeTurns = Math.Max(FreezeTurns, turns);
        }

        public void AddStun(int turns)
        {
            StunTurns = Math.Max(StunTurns, Math.Max(0, turns));
        }

        public void ClearNegativeStatuses()
        {
            BreakDefenseStacks = 0;
            BurnStacks = 0;
            BurnTurns = 0;
            PoisonStacks = 0;
            FreezeStacks = 0;
            FreezeTurns = 0;
            StunTurns = 0;
        }

        public int AddSwordMark(int stacks)
        {
            SwordMarkStacks += Math.Max(0, stacks);
            var explosions = SwordMarkStacks / 3;
            SwordMarkStacks %= 3;
            return explosions * SwordMarkExplosionDamage;
        }

        public void AddSharpness(int value, int turns)
        {
            Sharpness = Math.Max(Sharpness, Math.Max(0, value));
            SharpnessTurns = Math.Max(SharpnessTurns, Math.Max(1, turns));
        }

        public void ResolveSharpnessAtTurnStart()
        {
            if (Sharpness <= 0 || SharpnessTurns <= 0)
            {
                return;
            }

            SharpnessTurns--;
            if (SharpnessTurns == 0)
            {
                Sharpness = 0;
            }
        }

        public int ResolveBurnAtTurnStart()
        {
            if (BurnStacks <= 0 || BurnTurns <= 0)
            {
                return 0;
            }

            var damage = BurnStacks;
            TakeDirectDamage(damage);
            BurnTurns--;
            if (BurnTurns == 0)
            {
                BurnStacks = 0;
            }

            return damage;
        }

        public int ResolvePoisonAtTurnStart()
        {
            if (PoisonStacks <= 0)
            {
                return 0;
            }

            var damage = PoisonStacks;
            TakeDirectDamage(damage);
            return damage;
        }

        public int ResolveFreezeAtTurnStart()
        {
            if (FreezeStacks <= 0 || FreezeTurns <= 0)
            {
                return 0;
            }

            var spiritPenalty = FreezeStacks;
            FreezeTurns--;
            if (FreezeTurns == 0)
            {
                FreezeStacks = 0;
            }

            return spiritPenalty;
        }

        public bool ResolveStunAtTurnStart()
        {
            if (StunTurns <= 0)
            {
                return false;
            }

            StunTurns--;
            return true;
        }
    }
}
