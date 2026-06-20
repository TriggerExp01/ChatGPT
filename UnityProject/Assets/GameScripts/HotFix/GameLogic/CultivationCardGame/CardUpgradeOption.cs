using System;

namespace GameLogic.Cultivation
{
    public sealed class CardUpgradeOption
    {
        public CardUpgradeOption(string id, string name, string description, CardDefinition upgradedCard)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Upgrade id is required.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Upgrade name is required.", nameof(name));
            }

            Id = id;
            Name = name;
            Description = description ?? string.Empty;
            UpgradedCard = upgradedCard ?? throw new ArgumentNullException(nameof(upgradedCard));
        }

        public string Id { get; }

        public string Name { get; }

        public string Description { get; }

        public CardDefinition UpgradedCard { get; }
    }
}
