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

        public int AttackBonus { get; private set; }

        public EnemyIntent CurrentIntent => Definition.IntentLoop[_intentIndex % Definition.IntentLoop.Count];

        public void AddAttackBonus(int amount)
        {
            AttackBonus += System.Math.Max(0, amount);
        }

        public void AdvanceIntent()
        {
            _intentIndex = (_intentIndex + 1) % Definition.IntentLoop.Count;
        }
    }
}
