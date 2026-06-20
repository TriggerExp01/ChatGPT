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
        SwordMark,
        Sharpness,
        Exhaust,
        DamagePerSwordMark,
    }

    public enum EnemyIntentType
    {
        Attack,
        Defend,
        AttackAndBurn,
        Buff,
        Summon,
        Sweep,
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
    }

    public enum CultivationRunStatus
    {
        InBattle,
        Reward,
        Rest,
        Market,
        Chest,
        RouteChoice,
        Completed,
        Defeated,
    }
}
