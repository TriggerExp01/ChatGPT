using System;

namespace GameLogic.Cultivation
{
    public sealed class CultivationRunRouteChoice
    {
        public CultivationRunRouteChoice(int targetNodeIndex, CultivationRunNode targetNode)
        {
            if (targetNodeIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(targetNodeIndex), "Target node index cannot be negative.");
            }

            TargetNodeIndex = targetNodeIndex;
            TargetNode = targetNode ?? throw new ArgumentNullException(nameof(targetNode));
        }

        public int TargetNodeIndex { get; }

        public CultivationRunNode TargetNode { get; }
    }
}
