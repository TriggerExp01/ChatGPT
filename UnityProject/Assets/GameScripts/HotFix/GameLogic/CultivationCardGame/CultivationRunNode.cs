using System;
using System.Collections.Generic;

namespace GameLogic.Cultivation
{
    public sealed class CultivationRunNode
    {
        public CultivationRunNode(
            string id,
            string name,
            CultivationRunNodeType type,
            EnemyDefinition enemy,
            IEnumerable<CultivationRunReward> rewardPool,
            int restHealAmount = 0,
            IEnumerable<int> nextNodeIndices = null,
            int spiritStoneReward = 0,
            IEnumerable<CultivationMarketItem> marketItems = null,
            IEnumerable<ArtifactDefinition> artifactRewardPool = null)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Run node id is required.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Run node name is required.", nameof(name));
            }

            if (type != CultivationRunNodeType.Rest && type != CultivationRunNodeType.Market && type != CultivationRunNodeType.Chest && enemy == null)
            {
                throw new ArgumentNullException(nameof(enemy), "Combat run nodes require an enemy.");
            }

            Id = id;
            Name = name;
            Type = type;
            Enemy = enemy;
            RewardPool = new List<CultivationRunReward>(rewardPool ?? Array.Empty<CultivationRunReward>()).AsReadOnly();
            MarketItems = new List<CultivationMarketItem>(marketItems ?? Array.Empty<CultivationMarketItem>()).AsReadOnly();
            ArtifactRewardPool = new List<ArtifactDefinition>(artifactRewardPool ?? Array.Empty<ArtifactDefinition>()).AsReadOnly();
            RestHealAmount = Math.Max(0, restHealAmount);
            SpiritStoneReward = Math.Max(0, spiritStoneReward);

            var nextIndices = new List<int>(nextNodeIndices ?? Array.Empty<int>());
            for (var i = 0; i < nextIndices.Count; i++)
            {
                if (nextIndices[i] < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(nextNodeIndices), "Next node indices cannot contain negative values.");
                }
            }

            NextNodeIndices = nextIndices.AsReadOnly();
        }

        public string Id { get; }

        public string Name { get; }

        public CultivationRunNodeType Type { get; }

        public EnemyDefinition Enemy { get; }

        public IReadOnlyList<CultivationRunReward> RewardPool { get; }

        public IReadOnlyList<CultivationMarketItem> MarketItems { get; }

        public IReadOnlyList<ArtifactDefinition> ArtifactRewardPool { get; }

        public int RestHealAmount { get; }

        public int SpiritStoneReward { get; }

        public IReadOnlyList<int> NextNodeIndices { get; }
    }
}
