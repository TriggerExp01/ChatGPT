using System;

namespace GameLogic.Cultivation
{
    public enum GoldenCorePassiveType
    {
        SwordHeart,
        FlowingWater,
        ThunderSeed,
    }

    public sealed class GoldenCorePassiveDefinition
    {
        public GoldenCorePassiveDefinition(string id, string name, string description, GoldenCorePassiveType type, int effectValue)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Passive id is required.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Passive name is required.", nameof(name));
            }

            Id = id;
            Name = name;
            Description = description ?? string.Empty;
            Type = type;
            EffectValue = Math.Max(0, effectValue);
        }

        public string Id { get; }

        public string Name { get; }

        public string Description { get; }

        public GoldenCorePassiveType Type { get; }

        public int EffectValue { get; }
    }

    public enum PillEffectType
    {
        Heal,
        Spirit,
        Cleanse,
        CostReduction,
        MaxHp
    }

    public sealed class PillDefinition
    {
        public PillDefinition(string id, string name, string description, PillEffectType effectType = PillEffectType.Heal, int effectValue = 0)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Pill id is required.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Pill name is required.", nameof(name));
            }

            Id = id;
            Name = name;
            Description = description ?? string.Empty;
            EffectType = effectType;
            EffectValue = Math.Max(0, effectValue);
        }

        public string Id { get; }

        public string Name { get; }

        public string Description { get; }

        public PillEffectType EffectType { get; }

        public int EffectValue { get; }

        public int HealAmount => EffectType == PillEffectType.Heal ? EffectValue : 0;

        public int SpiritAmount => EffectType == PillEffectType.Spirit ? EffectValue : 0;

        public int CleanseHealAmount => EffectType == PillEffectType.Cleanse ? EffectValue : 0;

        public int CostReductionAmount => EffectType == PillEffectType.CostReduction ? EffectValue : 0;

        public int MaxHpAmount => EffectType == PillEffectType.MaxHp ? EffectValue : 0;

        public bool IsBattleEffect => EffectType != PillEffectType.MaxHp;

        public bool IsRunEffect => EffectType == PillEffectType.MaxHp;
    }

    public sealed class CultivationRunReward
    {
        public CultivationRunReward(string id, CardDefinition card)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Reward id is required.", nameof(id));
            }

            Id = id;
            Card = card ?? throw new ArgumentNullException(nameof(card));
        }

        public string Id { get; }

        public CardDefinition Card { get; }
    }

    public enum ArtifactEffectType
    {
        DeckLimitBonus,
        FirstTurnSpiritBonus,
        BonusSpiritStonesOnVictory,
        HealAfterVictory,
        FirstDamageReductionEachBattle,
        FirstAttackFlatDamageBonusEachBattle,
        PreventFirstSelfHpLossEachBattle,
        MissingHpDamageBonus,
        FirstAttackSwordMarkEachTurn,
        EveryThirdAttackCardDamageBonus,
        TurnStartBurn,
        BurnDamageBonus,
        FirstBattlePillDoubleNoConsume,
        PoisonStackLimitBonus,
        FirstChanceFailureOverride,
        ChainDamageNoDecay,
        BattleStartShield,
        AttackCounterPierceAndFirstDamageReduction,
        FirstAttackDamageMultiplierEachBattle,
        TurnStartAllEnemyDamage,
        HealOnEnemyKill,
        ExtraDrawPerTurn,
        FatalDamageSurviveOncePerRun,
        PillEveryThirdVictory
    }

    public sealed class ArtifactDefinition
    {
        public ArtifactDefinition(string id, string name, string description, ArtifactEffectType effectType, int effectValue)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Artifact id is required.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Artifact name is required.", nameof(name));
            }

            Id = id;
            Name = name;
            Description = description ?? string.Empty;
            EffectType = effectType;
            EffectValue = Math.Max(0, effectValue);
        }

        public string Id { get; }

        public string Name { get; }

        public string Description { get; }

        public ArtifactEffectType EffectType { get; }

        public int EffectValue { get; }

        public int DeckLimitBonus => EffectType == ArtifactEffectType.DeckLimitBonus ? EffectValue : 0;

        public int FirstTurnSpiritBonus => EffectType == ArtifactEffectType.FirstTurnSpiritBonus ? EffectValue : 0;

        public int BonusSpiritStonesOnVictory => EffectType == ArtifactEffectType.BonusSpiritStonesOnVictory ? EffectValue : 0;

        public int HealAfterVictoryAmount => EffectType == ArtifactEffectType.HealAfterVictory ? EffectValue : 0;

        public int FirstDamageReductionEachBattlePercent => EffectType == ArtifactEffectType.FirstDamageReductionEachBattle ? EffectValue : 0;

        public int FirstAttackFlatDamageBonusEachBattle => EffectType == ArtifactEffectType.FirstAttackFlatDamageBonusEachBattle ? EffectValue : 0;

        public int PreventFirstSelfHpLossEachBattleCharges => EffectType == ArtifactEffectType.PreventFirstSelfHpLossEachBattle ? Math.Max(1, EffectValue) : 0;

        public int MissingHpDamageBonusPerStepPercent => EffectType == ArtifactEffectType.MissingHpDamageBonus ? EffectValue : 0;

        public int FirstAttackSwordMarkStacksEachTurn => EffectType == ArtifactEffectType.FirstAttackSwordMarkEachTurn ? Math.Max(1, EffectValue) : 0;

        public int EveryThirdAttackCardDamageBonusPercent => EffectType == ArtifactEffectType.EveryThirdAttackCardDamageBonus ? EffectValue : 0;

        public int TurnStartBurnStacks => EffectType == ArtifactEffectType.TurnStartBurn ? Math.Max(1, EffectValue) : 0;

        public int BurnDamageBonusPercent => EffectType == ArtifactEffectType.BurnDamageBonus ? EffectValue : 0;

        public int FirstBattlePillDoubleNoConsumeCharges => EffectType == ArtifactEffectType.FirstBattlePillDoubleNoConsume ? Math.Max(1, EffectValue) : 0;

        public int PoisonStackLimitBonus => EffectType == ArtifactEffectType.PoisonStackLimitBonus ? EffectValue : 0;

        public int FirstChanceFailureOverrideCharges => EffectType == ArtifactEffectType.FirstChanceFailureOverride ? Math.Max(1, EffectValue) : 0;

        public bool ChainDamageNoDecay => EffectType == ArtifactEffectType.ChainDamageNoDecay && EffectValue > 0;

        public int BattleStartShield => EffectType == ArtifactEffectType.BattleStartShield ? EffectValue : 0;

        public int AttackCounterPierceAndFirstDamageReductionPercent => EffectType == ArtifactEffectType.AttackCounterPierceAndFirstDamageReduction ? EffectValue : 0;

        public int FirstAttackDamageMultiplierEachBattle => EffectType == ArtifactEffectType.FirstAttackDamageMultiplierEachBattle ? Math.Max(2, EffectValue) : 0;

        public int TurnStartAllEnemyDamage => EffectType == ArtifactEffectType.TurnStartAllEnemyDamage ? EffectValue : 0;

        public int HealOnEnemyKillAmount => EffectType == ArtifactEffectType.HealOnEnemyKill ? EffectValue : 0;

        public int ExtraDrawPerTurn => EffectType == ArtifactEffectType.ExtraDrawPerTurn ? EffectValue : 0;

        public int FatalDamageSurviveOncePerRunCharges => EffectType == ArtifactEffectType.FatalDamageSurviveOncePerRun ? Math.Max(1, EffectValue) : 0;

        public int PillEveryThirdVictoryInterval => EffectType == ArtifactEffectType.PillEveryThirdVictory ? Math.Max(1, EffectValue) : 0;
    }

    public sealed class CultivationMarketItem
    {
        public CultivationMarketItem(string id, CardDefinition card, int price)
            : this(id, card, null, null, price)
        {
        }

        public CultivationMarketItem(string id, PillDefinition pill, int price)
            : this(id, null, pill, null, price)
        {
        }

        public CultivationMarketItem(string id, ArtifactDefinition artifact, int price)
            : this(id, null, null, artifact, price)
        {
        }

        private CultivationMarketItem(string id, CardDefinition card, PillDefinition pill, ArtifactDefinition artifact, int price)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Market item id is required.", nameof(id));
            }

            var payloadCount = (card != null ? 1 : 0) + (pill != null ? 1 : 0) + (artifact != null ? 1 : 0);
            if (payloadCount != 1)
            {
                throw new ArgumentException("Market item requires exactly one payload.");
            }

            Id = id;
            Card = card;
            Pill = pill;
            Artifact = artifact;
            Price = Math.Max(0, price);
        }

        public string Id { get; }

        public CardDefinition Card { get; }

        public PillDefinition Pill { get; }

        public ArtifactDefinition Artifact { get; }

        public int Price { get; }

        public bool IsCard => Card != null;

        public bool IsPill => Pill != null;

        public bool IsArtifact => Artifact != null;
    }

    public enum MysticEventEffectType
    {
        GainSpiritStones,
        Heal,
        GainCard,
        GainPill,
        GainArtifact,
        Leave,
    }

    public sealed class MysticEventOption
    {
        public MysticEventOption(
            string id,
            string name,
            string description,
            MysticEventEffectType effectType,
            int effectValue = 0,
            CardDefinition cardReward = null,
            PillDefinition pillReward = null,
            ArtifactDefinition artifactReward = null)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Mystic event option id is required.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Mystic event option name is required.", nameof(name));
            }

            Id = id;
            Name = name;
            Description = description ?? string.Empty;
            EffectType = effectType;
            EffectValue = Math.Max(0, effectValue);
            CardReward = cardReward;
            PillReward = pillReward;
            ArtifactReward = artifactReward;
        }

        public string Id { get; }

        public string Name { get; }

        public string Description { get; }

        public MysticEventEffectType EffectType { get; }

        public int EffectValue { get; }

        public CardDefinition CardReward { get; }

        public PillDefinition PillReward { get; }

        public ArtifactDefinition ArtifactReward { get; }
    }

    public sealed class MysticEventDefinition
    {
        public MysticEventDefinition(string id, string name, string description, params MysticEventOption[] options)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Mystic event id is required.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Mystic event name is required.", nameof(name));
            }

            if (options == null || options.Length == 0)
            {
                throw new ArgumentException("Mystic event requires at least one option.", nameof(options));
            }

            Id = id;
            Name = name;
            Description = description ?? string.Empty;
            Options = Array.AsReadOnly(options);
        }

        public string Id { get; }

        public string Name { get; }

        public string Description { get; }

        public System.Collections.ObjectModel.ReadOnlyCollection<MysticEventOption> Options { get; }
    }
}
