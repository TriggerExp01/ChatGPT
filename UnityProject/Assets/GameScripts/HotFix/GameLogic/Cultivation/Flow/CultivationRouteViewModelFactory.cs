using GameLogic.Cultivation.UI;

namespace GameLogic.Cultivation.Flow
{
    public static class CultivationRouteViewModelFactory
    {
        public static SteamMainRouteViewModel Create(CultivationRouteRuntimeData data, CultivationFlowState state, string detail)
        {
            var model = new SteamMainRouteViewModel
            {
                PlayerName = "凌云子",
                Realm = "筑基中期",
                Location = $"云雾秘境 路线 {data.NodeIndex + 1}",
                DayText = $"第 {data.Layer} 层 / 第 {data.Day} 天",
                Character = new CharacterStatusViewModel
                {
                    Name = "凌云子",
                    Realm = "筑基中期"
                }
            };

            model.TopResources.Add(new TopResourceViewModel { Label = "气血", Value = $"{data.Hp}/{CultivationRouteRuntimeData.MaxHp}", Icon = "HP" });
            model.TopResources.Add(new TopResourceViewModel { Label = "灵力", Value = $"{data.Spirit}/{CultivationRouteRuntimeData.MaxSpirit}", Icon = "SP" });
            model.TopResources.Add(new TopResourceViewModel { Label = "灵石", Value = data.SpiritStones.ToString(), Icon = "SS" });

            model.Character.Stats.Add(new StatLineViewModel { Label = "气血", Value = $"{data.Hp}/{CultivationRouteRuntimeData.MaxHp}", Icon = "+" });
            model.Character.Stats.Add(new StatLineViewModel { Label = "灵力", Value = $"{data.Spirit}/{CultivationRouteRuntimeData.MaxSpirit}", Icon = "*" });
            model.Character.Stats.Add(new StatLineViewModel { Label = "攻击", Value = "12", Icon = "/" });
            model.Character.Stats.Add(new StatLineViewModel { Label = "防御", Value = "5", Icon = "#" });
            model.Character.Stats.Add(new StatLineViewModel { Label = "身法", Value = "8", Icon = ">" });
            model.Character.Stats.Add(new StatLineViewModel { Label = "悟性", Value = "10", Icon = "!" });

            model.Character.StatusTags.Add(new StatusTagViewModel { Title = FormatState(state), Duration = $"节点\n{data.NodeIndex + 1}" });
            model.Character.StatusTags.Add(new StatusTagViewModel { Title = "日志", Duration = Shorten(detail) });
            model.Character.StatusTags.Add(new StatusTagViewModel { Title = "闭环", Duration = "可玩\n原型" });

            for (var i = 0; i < data.AvailableNodes.Count; i++)
            {
                var node = data.AvailableNodes[i];
                model.RouteNodes.Add(new RouteNodeViewModel
                {
                    Id = node.Id,
                    Label = $"{i + 1}.{node.Label}",
                    Type = node.Type.ToString(),
                    IsCurrent = i == 0,
                });
            }

            for (var i = 0; i < data.Deck.Count && i < 3; i++)
            {
                model.CurrentCards.Add(CreateCard(data.Deck[i], i));
            }

            return model;
        }

        private static CardItemViewModel CreateCard(string cardName, int index)
        {
            if (cardName == "清风剑诀")
            {
                return new CardItemViewModel
                {
                    Title = cardName,
                    Cost = "1",
                    Type = "剑法",
                    Description = "造成 12 点伤害。本轮最小战斗用固定伤害模拟。",
                    CountText = index == 0 ? "x2" : string.Empty,
                };
            }

            if (cardName == "幻影步")
            {
                return new CardItemViewModel
                {
                    Title = cardName,
                    Cost = "1",
                    Type = "身法",
                    Description = "获得护身步法。本轮用于牌组预览。",
                    CountText = string.Empty,
                };
            }

            return new CardItemViewModel
            {
                Title = cardName,
                Cost = "2",
                Type = "心法",
                Description = "恢复灵力。本轮用于牌组预览。",
                CountText = string.Empty,
            };
        }

        private static string FormatState(CultivationFlowState state)
        {
            switch (state)
            {
                case CultivationFlowState.Boot:
                    return "启动";
                case CultivationFlowState.MainRoute:
                    return "路线";
                case CultivationFlowState.Event:
                    return "奇遇";
                case CultivationFlowState.Battle:
                    return "战斗";
                case CultivationFlowState.Reward:
                    return "奖励";
                case CultivationFlowState.Finished:
                    return "完成";
                default:
                    return state.ToString();
            }
        }

        private static string Shorten(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            return value.Length <= 6 ? value : value.Substring(0, 6);
        }
    }
}
