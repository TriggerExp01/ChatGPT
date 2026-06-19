namespace GameLogic.Cultivation
{
    public sealed class EnemyState
    {
        private int _intentIndex;

        public EnemyState(EnemyDefinition definition)
        {
            Definition = definition;
            Body = new CombatantState(definition.Name, definition.MaxHp, definition.Defense);
        }

        public EnemyDefinition Definition { get; }

        public CombatantState Body { get; }

        public EnemyIntent CurrentIntent => Definition.IntentLoop[_intentIndex % Definition.IntentLoop.Count];

        public void AdvanceIntent()
        {
            _intentIndex = (_intentIndex + 1) % Definition.IntentLoop.Count;
        }
    }
}
