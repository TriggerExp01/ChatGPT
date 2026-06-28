namespace GameLogic.Cultivation.Flow
{
    public sealed class CultivationRouteNodeRuntimeData
    {
        public CultivationRouteNodeRuntimeData(string id, string label, CultivationRouteNodeType type, string description)
        {
            Id = id ?? string.Empty;
            Label = label ?? string.Empty;
            Type = type;
            Description = description ?? string.Empty;
        }

        public string Id { get; }

        public string Label { get; }

        public CultivationRouteNodeType Type { get; }

        public string Description { get; }
    }
}
