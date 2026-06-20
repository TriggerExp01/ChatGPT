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
    }

    public enum CultivationRunStatus
    {
        InBattle,
        Reward,
        Completed,
        Defeated,
    }
}
