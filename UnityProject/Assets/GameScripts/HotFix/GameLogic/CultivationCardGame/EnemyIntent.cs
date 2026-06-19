namespace GameLogic.Cultivation
{
    public sealed class EnemyIntent
    {
        public EnemyIntent(EnemyIntentType type, int value = 0, int secondaryValue = 0, string description = null)
        {
            Type = type;
            Value = value;
            SecondaryValue = secondaryValue;
            Description = description ?? type.ToString();
        }

        public EnemyIntentType Type { get; }

        public int Value { get; }

        public int SecondaryValue { get; }

        public string Description { get; }
    }
}
