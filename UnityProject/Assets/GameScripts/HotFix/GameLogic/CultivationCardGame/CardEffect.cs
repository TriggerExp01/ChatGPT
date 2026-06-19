using System;

namespace GameLogic.Cultivation
{
    public sealed class CardEffect
    {
        public CardEffect(CardEffectType type, int value, CardTarget target = CardTarget.EnemySingle, int duration = 0)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Card effect value cannot be negative.");
            }

            Type = type;
            Value = value;
            Target = target;
            Duration = duration;
        }

        public CardEffectType Type { get; }

        public int Value { get; }

        public CardTarget Target { get; }

        public int Duration { get; }
    }
}
