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
            IEnumerable<CultivationRunReward> rewardPool)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Run node id is required.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Run node name is required.", nameof(name));
            }

            Id = id;
            Name = name;
            Type = type;
            Enemy = enemy ?? throw new ArgumentNullException(nameof(enemy));
            RewardPool = new List<CultivationRunReward>(rewardPool ?? throw new ArgumentNullException(nameof(rewardPool))).AsReadOnly();
        }

        public string Id { get; }

        public string Name { get; }

        public CultivationRunNodeType Type { get; }

        public EnemyDefinition Enemy { get; }

        public IReadOnlyList<CultivationRunReward> RewardPool { get; }
    }
}
