namespace GameLogic.Cultivation
{
    public enum CardTarget
    {
        Self,
        EnemySingle,
        EnemyAll,
    }

    public enum CardEffectType
    {
        Damage,
        Shield,
        Draw,
        Heal,
        BreakDefense,
        Burn,
        Poison,
        PoisonBurst,
        Leech,
        Regeneration,
        PoisonAttackCounter,
        SwordMark,
        Sharpness,
        Exhaust,
        DamagePerSwordMark,
        Stun,
        ChanceDamage,
        ChanceStun,
        ChanceChainDamage,
        ChargeDamage,
        ChainOnChanceDamage,
        ChainOnChanceStun,
        ChanceChainDamageWithStun,
        ChanceChainDamageRepeatTarget,
        ChanceDamageWithStun,
        ChanceDamageWithChain,
        DamageAfterCriticalTriggered,
        DamageAfterCriticalTriggeredWithStun,
        DamageAfterCriticalTriggeredChainAll,
        Dodge,
        DodgeCounter,
        AttackCounter,
    }

    public enum EnemyIntentType
    {
        Attack,
        Defend,
        AttackAndBurn,
        AttackAndFreeze,
        Buff,
        Summon,
        Sweep,
        Heal,
        BuffAttack,
        AttackAndStun,
    }

    public enum BattleOutcome
    {
        InProgress,
        Victory,
        Defeat,
    }

    public enum CultivationRunNodeType
    {
        Battle,
        Elite,
        Rest,
        Market,
        Chest,
        Mystic,
    }

    public enum CultivationRunStatus
    {
        InBattle,
        Reward,
        Rest,
        Market,
        Chest,
        Mystic,
        GoldenCorePassiveChoice,
        RouteChoice,
        Completed,
        Defeated,
    }

    public enum CultivationRealm
    {
        QiRefining,
        Foundation,
        GoldenCore,
        NascentSoul,
        SoulTransformation,
        Tribulation,
    }

    public enum CultivationSect
    {
        Sword,
        FireCloud,
        Thunder,
        Earth,
        Medicine,
    }
}
