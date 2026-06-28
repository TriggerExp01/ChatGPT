using System.Collections.Generic;

namespace GameLogic.Cultivation.Flow
{
    public sealed class CultivationRouteRuntimeData
    {
        public const int MaxHp = 72;
        public const int MaxSpirit = 56;

        public int Layer { get; set; } = 1;

        public int Day { get; set; } = 1;

        public int NodeIndex { get; set; }

        public int Hp { get; set; } = MaxHp;

        public int Spirit { get; set; } = 48;

        public int SpiritStones { get; set; } = 120;

        public List<string> Deck { get; } = new List<string>
        {
            "清风剑诀",
            "幻影步",
            "混元归一诀",
        };

        public List<CultivationRouteNodeRuntimeData> AvailableNodes { get; } = new List<CultivationRouteNodeRuntimeData>();

        public static CultivationRouteRuntimeData CreateDefault()
        {
            var data = new CultivationRouteRuntimeData();
            data.RefreshAvailableNodes();
            return data;
        }

        public void RefreshAvailableNodes()
        {
            AvailableNodes.Clear();

            var pattern = NodeIndex % 4;
            if (pattern == 0)
            {
                AvailableNodes.Add(new CultivationRouteNodeRuntimeData("battle", "战斗", CultivationRouteNodeType.Battle, "遭遇巡山妖修"));
                AvailableNodes.Add(new CultivationRouteNodeRuntimeData("event", "奇遇", CultivationRouteNodeType.Event, "路边灵泉泛光"));
                AvailableNodes.Add(new CultivationRouteNodeRuntimeData("rest", "调息", CultivationRouteNodeType.Rest, "暂时恢复气血"));
                return;
            }

            if (pattern == 1)
            {
                AvailableNodes.Add(new CultivationRouteNodeRuntimeData("event", "奇遇", CultivationRouteNodeType.Event, "破旧洞府传来灵息"));
                AvailableNodes.Add(new CultivationRouteNodeRuntimeData("battle", "战斗", CultivationRouteNodeType.Battle, "妖兽守住去路"));
                AvailableNodes.Add(new CultivationRouteNodeRuntimeData("shop", "坊市", CultivationRouteNodeType.Shop, "占位节点"));
                return;
            }

            if (pattern == 2)
            {
                AvailableNodes.Add(new CultivationRouteNodeRuntimeData("battle", "战斗", CultivationRouteNodeType.Battle, "散修拦路试剑"));
                AvailableNodes.Add(new CultivationRouteNodeRuntimeData("rest", "调息", CultivationRouteNodeType.Rest, "占位节点"));
                AvailableNodes.Add(new CultivationRouteNodeRuntimeData("event", "奇遇", CultivationRouteNodeType.Event, "拾得无主纳戒"));
                return;
            }

            AvailableNodes.Add(new CultivationRouteNodeRuntimeData("boss", "首领", CultivationRouteNodeType.Boss, "占位节点"));
            AvailableNodes.Add(new CultivationRouteNodeRuntimeData("battle", "战斗", CultivationRouteNodeType.Battle, "护阵灵魄现身"));
            AvailableNodes.Add(new CultivationRouteNodeRuntimeData("event", "奇遇", CultivationRouteNodeType.Event, "古碑浮现金纹"));
        }

        public void AdvanceAfterReward()
        {
            NodeIndex++;
            Day++;

            if (Day > 7)
            {
                Day = 1;
                Layer++;
            }

            RefreshAvailableNodes();
        }
    }
}
