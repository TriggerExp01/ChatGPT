namespace GameLogic
{
    public static class RoguelikeText
    {
        public static string GetRoomName(RoguelikeRoomType type)
        {
            switch (type)
            {
                case RoguelikeRoomType.Start:
                    return "起点";
                case RoguelikeRoomType.Combat:
                    return "战斗";
                case RoguelikeRoomType.Elite:
                    return "精英";
                case RoguelikeRoomType.Treasure:
                    return "宝箱";
                case RoguelikeRoomType.Rest:
                    return "营火";
                case RoguelikeRoomType.Shop:
                    return "商店";
                case RoguelikeRoomType.Boss:
                    return "首领";
                default:
                    return type.ToString();
            }
        }

        public static string GetChoicePrompt(RoguelikeRoomType roomType)
        {
            switch (roomType)
            {
                case RoguelikeRoomType.Rest:
                    return "营火：选择恢复方式。";
                case RoguelikeRoomType.Treasure:
                    return "宝箱：选择带走的奖励。";
                case RoguelikeRoomType.Shop:
                    return "商店：购买一件物品，或直接离开。";
                default:
                    return "请选择一个奖励继续。";
            }
        }

        public static string GetPhaseName(RoguelikeGamePhase phase)
        {
            switch (phase)
            {
                case RoguelikeGamePhase.Running:
                    return "探索中";
                case RoguelikeGamePhase.RewardChoice:
                    return "选择奖励";
                case RoguelikeGamePhase.Victory:
                    return "胜利";
                case RoguelikeGamePhase.Defeated:
                    return "失败";
                default:
                    return phase.ToString();
            }
        }
    }
}
