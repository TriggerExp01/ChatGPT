using System;

namespace GameLogic.Cultivation
{
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
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Market item id is required.", nameof(id));
            }

            Id = id;
            Card = card ?? throw new ArgumentNullException(nameof(card));
            Price = Math.Max(0, price);
        }

        public string Id { get; }

        public CardDefinition Card { get; }

        public int Price { get; }
    }
}
