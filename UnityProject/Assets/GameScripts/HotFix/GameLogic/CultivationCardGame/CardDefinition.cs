using System;
using System.Collections.Generic;

namespace GameLogic.Cultivation
{
    public sealed class CardDefinition
    {
        public CardDefinition(string id, string name, int spiritCost, params CardEffect[] effects)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Card id is required.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Card name is required.", nameof(name));
            }

            if (spiritCost < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(spiritCost), "Spirit cost cannot be negative.");
            }

            Id = id;
            Name = name;
            SpiritCost = spiritCost;
            Effects = new List<CardEffect>(effects ?? Array.Empty<CardEffect>()).AsReadOnly();
        }

        public string Id { get; }

        public string Name { get; }

        public int SpiritCost { get; }

        public IReadOnlyList<CardEffect> Effects { get; }
    }
}
