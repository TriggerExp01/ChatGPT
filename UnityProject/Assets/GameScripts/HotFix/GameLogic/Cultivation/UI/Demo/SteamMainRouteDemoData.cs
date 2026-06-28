namespace GameLogic.Cultivation.UI
{
    public static class SteamMainRouteDemoData
    {
        public static SteamMainRouteViewModel Create()
        {
            var model = new SteamMainRouteViewModel
            {
                PlayerName = "凌云子",
                Realm = "筑基中期",
                Location = "云雾秘境 · 第一层",
                DayText = "第 7 天",
                Character = new CharacterStatusViewModel
                {
                    Name = "凌云子",
                    Realm = "筑基中期"
                }
            };

            model.TopResources.Add(new TopResourceViewModel { Label = "气血", Value = "72/72", Icon = "●" });
            model.TopResources.Add(new TopResourceViewModel { Label = "灵力", Value = "48/56", Icon = "◎" });
            model.TopResources.Add(new TopResourceViewModel { Label = "灵石", Value = "312", Icon = "◆" });

            model.Character.Stats.Add(new StatLineViewModel { Label = "气血", Value = "72/72", Icon = "♥" });
            model.Character.Stats.Add(new StatLineViewModel { Label = "灵力", Value = "48/56", Icon = "◎" });
            model.Character.Stats.Add(new StatLineViewModel { Label = "攻击", Value = "14", Icon = "╱" });
            model.Character.Stats.Add(new StatLineViewModel { Label = "防御", Value = "9", Icon = "◇" });
            model.Character.Stats.Add(new StatLineViewModel { Label = "身法", Value = "16", Icon = "↯" });
            model.Character.Stats.Add(new StatLineViewModel { Label = "悟性", Value = "12", Icon = "▰" });

            model.Character.StatusTags.Add(new StatusTagViewModel { Title = "清\n心", Duration = "剩余\n2天" });
            model.Character.StatusTags.Add(new StatusTagViewModel { Title = "御\n剑", Duration = "剩余\n3天" });
            model.Character.StatusTags.Add(new StatusTagViewModel { Title = "灵\n动", Duration = "剩余\n1天" });

            model.RouteNodes.Add(new RouteNodeViewModel { Id = "event", Label = "奇遇", Type = "Event" });
            model.RouteNodes.Add(new RouteNodeViewModel { Id = "battle", Label = "战斗", Type = "Battle" });
            model.RouteNodes.Add(new RouteNodeViewModel { Id = "chest", Label = "宝箱", Type = "Chest" });
            model.RouteNodes.Add(new RouteNodeViewModel { Id = "rest", Label = "休憩", Type = "Rest" });
            model.RouteNodes.Add(new RouteNodeViewModel { Id = "market", Label = "坊市", Type = "Market" });
            model.RouteNodes.Add(new RouteNodeViewModel { Id = "current", Label = string.Empty, Type = "Current", IsCurrent = true });

            model.CurrentCards.Add(new CardItemViewModel
            {
                Title = "清风剑诀",
                Cost = "1",
                Type = "剑法",
                Description = "造成12点剑系伤害。\n若本回合未使用攻击牌，则抽1张牌。",
                CountText = "x2"
            });
            model.CurrentCards.Add(new CardItemViewModel
            {
                Title = "幻影步",
                Cost = "1",
                Type = "身法",
                Description = "获得8点护甲。\n抽1张牌。",
                CountText = "x2"
            });
            model.CurrentCards.Add(new CardItemViewModel
            {
                Title = "混元归一诀",
                Cost = "2",
                Type = "心法",
                Description = "恢复10点灵力。\n若灵力全满，抽2张牌。",
                CountText = string.Empty
            });

            return model;
        }
    }
}
