using System;

namespace GameLogic.Cultivation
{
    public sealed class PillDefinition
    {
        public PillDefinition(string id, string name, string description, int healAmount = 0)
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
            HealAmount = Math.Max(0, healAmount);
        }

        public string Id { get; }

        public string Name { get; }

        public string Description { get; }

        public int HealAmount { get; }
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

    public sealed class CultivationMarketItem
    {
        public CultivationMarketItem(string id, CardDefinition card, int price)
            : this(id, card, null, price)
        {
        }

        public CultivationMarketItem(string id, PillDefinition pill, int price)
            : this(id, null, pill, price)
        {
        }

        private CultivationMarketItem(string id, CardDefinition card, PillDefinition pill, int price)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Market item id is required.", nameof(id));
            }

            if ((card == null && pill == null) || (card != null && pill != null))
            {
                throw new ArgumentException("Market item requires exactly one payload.");
            }

            Id = id;
            Card = card;
            Pill = pill;
            Price = Math.Max(0, price);
        }

        public string Id { get; }

        public CardDefinition Card { get; }

        public PillDefinition Pill { get; }

        public int Price { get; }

        public bool IsCard => Card != null;

        public bool IsPill => Pill != null;
    }
}
