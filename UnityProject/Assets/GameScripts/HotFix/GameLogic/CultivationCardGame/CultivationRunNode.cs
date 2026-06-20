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
            int restHealAmount = 0)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("Run node id is required.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("Run node name is required.", nameof(name));
            }

            if (type != CultivationRunNodeType.Rest && enemy == null)
            {
                throw new ArgumentNullException(nameof(enemy), "Combat run nodes require an enemy.");
            }

            Id = id;
            Name = name;
            Type = type;
            Enemy = enemy;
            RewardPool = new List<CultivationRunReward>(rewardPool ?? Array.Empty<CultivationRunReward>()).AsReadOnly();
            RestHealAmount = Math.Max(0, restHealAmount);
        }

        public string Id { get; }

        public string Name { get; }

        public CultivationRunNodeType Type { get; }

        public EnemyDefinition Enemy { get; }

        public IReadOnlyList<CultivationRunReward> RewardPool { get; }

        public int RestHealAmount { get; }
    }
}
