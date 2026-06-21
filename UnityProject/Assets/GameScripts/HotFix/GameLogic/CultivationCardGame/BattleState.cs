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

        public int BloodGuardHealAmount { get; private set; }

        public int BloodGuardHealTurns { get; private set; }

        public int DeathWardHealAmount { get; private set; }

        public int DeathWardTurns { get; private set; }

        public int DamageTakenHealPercent { get; private set; }

        public int DamageTakenHealTurns { get; private set; }

        public int FlatDamageBonus { get; private set; }

        public int FlatDamageBonusTurns { get; private set; }

        public int FrenzyDamageBonusPerStepPercent { get; private set; }

        public int FrenzyDamageBonusTurns { get; private set; }

        public int BloodlossRetaliationPercent { get; private set; }

        public int BloodlossRetaliationTurns { get; private set; }

        public int PreventSelfHpLossCharges { get; private set; }

        public int ArtifactMissingHpDamageBonusPerStepPercent { get; private set; }

        public int ArtifactFirstAttackFlatDamageBonus { get; private set; }

        public bool HasTriggeredArtifactFirstAttackFlatDamageBonus { get; private set; }

        public int ArtifactFirstAttackDamageMultiplier { get; private set; } = 1;

        public bool HasTriggeredArtifactFirstAttackDamageMultiplier { get; private set; }

        public int ArtifactFirstAttackSwordMarkStacks { get; private set; }

        public bool HasTriggeredArtifactFirstAttackSwordMarkThisTurn { get; private set; }

        public int ArtifactAttackCardCounter { get; private set; }

        public int ArtifactEveryThirdAttackCardDamageBonusPercent { get; private set; }

        public int ArtifactCurrentAttackCardDamageBonusPercent { get; private set; }

        public int ArtifactTurnStartBurnStacks { get; private set; }

        public int ArtifactBurnDamageBonusPercent { get; private set; }

        public int ArtifactTurnStartAllEnemyDamage { get; private set; }

        public int ArtifactHealOnEnemyKillAmount { get; private set; }

        public int ArtifactFirstBattlePillDoubleNoConsumeCharges { get; private set; }

        public int ArtifactPoisonStackLimitBonus { get; private set; }

        public int ArtifactFirstChanceFailureOverrideCharges { get; private set; }

        public bool ArtifactChainDamageNoDecay { get; private set; }

        public int ArtifactAttackCounterPierceDamageReductionPercent { get; private set; }

        public bool HasTriggeredArtifactFirstDamageReductionThisTurn { get; private set; }

        public int ArtifactFirstDamageReductionPercent { get; private set; }

        public bool HasTriggeredArtifactFirstDamageReductionEachBattle { get; private set; }

        public int SelfHpLostThisTurn { get; private set; }

        public bool PoisonDamageTriggeredThisTurn { get; private set; }

        public int TurnNumber { get; set; }

        public BattleOutcome Outcome { get; set; } = BattleOutcome.InProgress;

        private readonly HashSet<EnemyState> _artifactKillHealClaimedEnemies = new HashSet<EnemyState>();

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

        public void AddBloodGuardHeal(int amount, int turns)
        {
            BloodGuardHealAmount += Math.Max(0, amount);
            BloodGuardHealTurns = Math.Max(BloodGuardHealTurns, Math.Max(1, turns));
        }

        public void ResolveBloodGuardHealDurationAtTurnStart()
        {
            if (BloodGuardHealTurns <= 0)
            {
                BloodGuardHealAmount = 0;
                return;
            }

            BloodGuardHealTurns--;
            if (BloodGuardHealTurns == 0)
            {
                BloodGuardHealAmount = 0;
            }
        }

        public int TriggerBloodGuardHeal()
        {
            if (BloodGuardHealAmount <= 0 || BloodGuardHealTurns <= 0)
            {
                return 0;
            }

            var before = Player.CurrentHp;
            Player.Heal(BloodGuardHealAmount);
            return Player.CurrentHp - before;
        }

        public void AddDeathWard(int healAmount, int turns)
        {
            DeathWardHealAmount = Math.Max(DeathWardHealAmount, Math.Max(0, healAmount));
            DeathWardTurns = Math.Max(DeathWardTurns, Math.Max(1, turns));
        }

        public void ResolveDeathWardDurationAtTurnStart()
        {
            if (DeathWardTurns <= 0)
            {
                DeathWardHealAmount = 0;
                return;
            }

            DeathWardTurns--;
            if (DeathWardTurns == 0)
            {
                DeathWardHealAmount = 0;
            }
        }

        public int TryTriggerDeathWard()
        {
            if (DeathWardHealAmount <= 0 || DeathWardTurns <= 0 || Player.CurrentHp > 0)
            {
                return 0;
            }

            Player.Heal(Math.Max(1, DeathWardHealAmount));
            DeathWardHealAmount = 0;
            DeathWardTurns = 0;
            return Player.CurrentHp;
        }

        public void AddDamageTakenHeal(int percent, int turns)
        {
            DamageTakenHealPercent += Math.Max(0, percent);
            DamageTakenHealTurns = Math.Max(DamageTakenHealTurns, Math.Max(1, turns));
        }

        public void ResolveDamageTakenHealDurationAtTurnStart()
        {
            if (DamageTakenHealTurns <= 0)
            {
                DamageTakenHealPercent = 0;
                return;
            }

            DamageTakenHealTurns--;
            if (DamageTakenHealTurns == 0)
            {
                DamageTakenHealPercent = 0;
            }
        }

        public int TriggerDamageTakenHeal(int hpDamage)
        {
            if (DamageTakenHealPercent <= 0 || DamageTakenHealTurns <= 0 || hpDamage <= 0)
            {
                return 0;
            }

            var before = Player.CurrentHp;
            Player.Heal(hpDamage * DamageTakenHealPercent / 100);
            return Player.CurrentHp - before;
        }

        public void AddFlatDamageBonus(int amount, int turns)
        {
            FlatDamageBonus += Math.Max(0, amount);
            FlatDamageBonusTurns = Math.Max(FlatDamageBonusTurns, Math.Max(1, turns));
        }

        public void ResolveFlatDamageBonusDurationAtTurnStart()
        {
            if (FlatDamageBonusTurns <= 0)
            {
                FlatDamageBonus = 0;
                return;
            }

            FlatDamageBonusTurns--;
            if (FlatDamageBonusTurns == 0)
            {
                FlatDamageBonus = 0;
            }
        }

        public void AddFrenzyDamageBonus(int percentPerMissingHpStep, int turns)
        {
            FrenzyDamageBonusPerStepPercent += Math.Max(0, percentPerMissingHpStep);
            FrenzyDamageBonusTurns = Math.Max(FrenzyDamageBonusTurns, Math.Max(1, turns));
        }

        public void ResolveFrenzyDamageBonusDurationAtTurnStart()
        {
            if (FrenzyDamageBonusTurns <= 0)
            {
                FrenzyDamageBonusPerStepPercent = 0;
                return;
            }

            FrenzyDamageBonusTurns--;
            if (FrenzyDamageBonusTurns == 0)
            {
                FrenzyDamageBonusPerStepPercent = 0;
            }
        }

        public void AddBloodlossRetaliation(int percent, int turns)
        {
            BloodlossRetaliationPercent += Math.Max(0, percent);
            BloodlossRetaliationTurns = Math.Max(BloodlossRetaliationTurns, Math.Max(1, turns));
        }

        public void ResolveBloodlossRetaliationDurationAtTurnStart()
        {
            if (BloodlossRetaliationTurns <= 0)
            {
                BloodlossRetaliationPercent = 0;
                return;
            }

            BloodlossRetaliationTurns--;
            if (BloodlossRetaliationTurns == 0)
            {
                BloodlossRetaliationPercent = 0;
            }
        }

        public void AddPreventSelfHpLossCharges(int amount)
        {
            PreventSelfHpLossCharges += Math.Max(0, amount);
        }

        public bool TryConsumePreventSelfHpLossCharge()
        {
            if (PreventSelfHpLossCharges <= 0)
            {
                return false;
            }

            PreventSelfHpLossCharges--;
            return true;
        }

        public void AddArtifactMissingHpDamageBonus(int percentPerMissingHpStep)
        {
            ArtifactMissingHpDamageBonusPerStepPercent += Math.Max(0, percentPerMissingHpStep);
        }

        public void AddArtifactFirstAttackFlatDamageBonus(int amount)
        {
            ArtifactFirstAttackFlatDamageBonus += Math.Max(0, amount);
        }

        public int TryConsumeArtifactFirstAttackFlatDamageBonus()
        {
            if (ArtifactFirstAttackFlatDamageBonus <= 0 || HasTriggeredArtifactFirstAttackFlatDamageBonus)
            {
                return 0;
            }

            HasTriggeredArtifactFirstAttackFlatDamageBonus = true;
            return ArtifactFirstAttackFlatDamageBonus;
        }

        public void AddArtifactFirstAttackDamageMultiplier(int multiplier)
        {
            ArtifactFirstAttackDamageMultiplier = Math.Max(ArtifactFirstAttackDamageMultiplier, Math.Max(1, multiplier));
        }

        public int TryConsumeArtifactFirstAttackDamageMultiplier()
        {
            if (ArtifactFirstAttackDamageMultiplier <= 1 || HasTriggeredArtifactFirstAttackDamageMultiplier)
            {
                return 1;
            }

            HasTriggeredArtifactFirstAttackDamageMultiplier = true;
            return ArtifactFirstAttackDamageMultiplier;
        }

        public void AddArtifactFirstAttackSwordMark(int stacks)
        {
            ArtifactFirstAttackSwordMarkStacks += Math.Max(0, stacks);
        }

        public bool TryTriggerArtifactFirstAttackSwordMark()
        {
            if (ArtifactFirstAttackSwordMarkStacks <= 0 || HasTriggeredArtifactFirstAttackSwordMarkThisTurn)
            {
                return false;
            }

            HasTriggeredArtifactFirstAttackSwordMarkThisTurn = true;
            return true;
        }

        public void AddArtifactEveryThirdAttackCardDamageBonus(int percent)
        {
            ArtifactEveryThirdAttackCardDamageBonusPercent += Math.Max(0, percent);
        }

        public int RegisterAttackCardAndGetArtifactBonusPercent()
        {
            if (ArtifactEveryThirdAttackCardDamageBonusPercent <= 0)
            {
                return 0;
            }

            ArtifactAttackCardCounter++;
            return ArtifactAttackCardCounter % 3 == 0 ? ArtifactEveryThirdAttackCardDamageBonusPercent : 0;
        }

        public void SetArtifactCurrentAttackCardDamageBonus(int percent)
        {
            ArtifactCurrentAttackCardDamageBonusPercent = Math.Max(0, percent);
        }

        public void ClearArtifactCurrentAttackCardDamageBonus()
        {
            ArtifactCurrentAttackCardDamageBonusPercent = 0;
        }

        public void AddArtifactTurnStartBurn(int stacks)
        {
            ArtifactTurnStartBurnStacks += Math.Max(0, stacks);
        }

        public void AddArtifactBurnDamageBonus(int percent)
        {
            ArtifactBurnDamageBonusPercent += Math.Max(0, percent);
        }

        public void AddArtifactTurnStartAllEnemyDamage(int amount)
        {
            ArtifactTurnStartAllEnemyDamage += Math.Max(0, amount);
        }

        public void AddArtifactHealOnEnemyKill(int amount)
        {
            ArtifactHealOnEnemyKillAmount += Math.Max(0, amount);
        }

        public int TryTriggerArtifactHealOnEnemyKill(EnemyState enemy)
        {
            if (ArtifactHealOnEnemyKillAmount <= 0 || enemy == null || !enemy.Body.IsDefeated || !_artifactKillHealClaimedEnemies.Add(enemy))
            {
                return 0;
            }

            var before = Player.CurrentHp;
            Player.Heal(ArtifactHealOnEnemyKillAmount);
            return Player.CurrentHp - before;
        }

        public void AddArtifactFirstBattlePillDoubleNoConsumeCharges(int amount)
        {
            ArtifactFirstBattlePillDoubleNoConsumeCharges += Math.Max(0, amount);
        }

        public bool TryConsumeArtifactFirstBattlePillDoubleNoConsumeCharge()
        {
            if (ArtifactFirstBattlePillDoubleNoConsumeCharges <= 0)
            {
                return false;
            }

            ArtifactFirstBattlePillDoubleNoConsumeCharges--;
            return true;
        }

        public void AddArtifactPoisonStackLimitBonus(int amount)
        {
            ArtifactPoisonStackLimitBonus += Math.Max(0, amount);
        }

        public void AddArtifactFirstChanceFailureOverrideCharges(int amount)
        {
            ArtifactFirstChanceFailureOverrideCharges += Math.Max(0, amount);
        }

        public bool TryConsumeArtifactFirstChanceFailureOverrideCharge()
        {
            if (ArtifactFirstChanceFailureOverrideCharges <= 0)
            {
                return false;
            }

            ArtifactFirstChanceFailureOverrideCharges--;
            return true;
        }

        public void EnableArtifactChainDamageNoDecay()
        {
            ArtifactChainDamageNoDecay = true;
        }

        public void AddArtifactAttackCounterPierceAndDamageReduction(int percent)
        {
            ArtifactAttackCounterPierceDamageReductionPercent += Math.Max(0, percent);
        }

        public bool TryTriggerArtifactFirstDamageReduction()
        {
            if (ArtifactAttackCounterPierceDamageReductionPercent <= 0 || HasTriggeredArtifactFirstDamageReductionThisTurn)
            {
                return false;
            }

            HasTriggeredArtifactFirstDamageReductionThisTurn = true;
            return true;
        }

        public void AddArtifactFirstDamageReductionEachBattle(int percent)
        {
            ArtifactFirstDamageReductionPercent += Math.Max(0, percent);
        }

        public int TryConsumeArtifactFirstDamageReductionEachBattle()
        {
            if (ArtifactFirstDamageReductionPercent <= 0 || HasTriggeredArtifactFirstDamageReductionEachBattle)
            {
                return 0;
            }

            HasTriggeredArtifactFirstDamageReductionEachBattle = true;
            return ArtifactFirstDamageReductionPercent;
        }

        public void AddSelfHpLostThisTurn(int amount)
        {
            SelfHpLostThisTurn += Math.Max(0, amount);
        }

        public void ResetSelfHpLostThisTurn()
        {
            SelfHpLostThisTurn = 0;
        }

        public void ResetArtifactTurnFlags()
        {
            HasTriggeredArtifactFirstAttackSwordMarkThisTurn = false;
            HasTriggeredArtifactFirstDamageReductionThisTurn = false;
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
