using System;

namespace GameLogic.Cultivation
{
    public sealed class CardEffect
    {
        public CardEffect(CardEffectType type, int value, CardTarget target = CardTarget.EnemySingle, int duration = 0, int repeatCount = 1, int chancePercent = 100, int fallbackValue = 0, int secondaryValue = 0)
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(value), "Card effect value cannot be negative.");
            }

            if (repeatCount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(repeatCount), "Card effect repeat count must be positive.");
            }

            if (chancePercent < 0 || chancePercent > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(chancePercent), "Card effect chance percent must be between 0 and 100.");
            }

            if (fallbackValue < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(fallbackValue), "Card effect fallback value cannot be negative.");
            }

            if (secondaryValue < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(secondaryValue), "Card effect secondary value cannot be negative.");
            }

            Type = type;
            Value = value;
            Target = target;
            Duration = duration;
            RepeatCount = repeatCount;
            ChancePercent = chancePercent;
            FallbackValue = fallbackValue;
            SecondaryValue = secondaryValue;
        }

        public CardEffectType Type { get; }

        public int Value { get; }

        public CardTarget Target { get; }

        public int Duration { get; }

        public int RepeatCount { get; }

        public int ChancePercent { get; }

        public int FallbackValue { get; }

        public int SecondaryValue { get; }
    }
}
