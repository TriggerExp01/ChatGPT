using System;

namespace GameLogic.Cultivation
{
    public sealed class CultivationRestUpgradeChoice
    {
        public CultivationRestUpgradeChoice(int deckIndex, CardDefinition sourceCard)
        {
            if (deckIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(deckIndex), "Deck index cannot be negative.");
            }

            if (sourceCard == null)
            {
                throw new ArgumentNullException(nameof(sourceCard));
            }

            if (!sourceCard.CanUpgrade)
            {
                throw new ArgumentException("Source card has no upgrade options.", nameof(sourceCard));
            }

            DeckIndex = deckIndex;
            SourceCard = sourceCard;
        }

        public int DeckIndex { get; }

        public CardDefinition SourceCard { get; }
    }
}
