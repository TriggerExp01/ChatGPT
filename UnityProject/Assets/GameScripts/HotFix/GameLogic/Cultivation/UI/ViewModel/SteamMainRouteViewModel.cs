using System;
using System.Collections.Generic;

namespace GameLogic.Cultivation.UI
{
    [Serializable]
    public sealed class SteamMainRouteViewModel
    {
        public string PlayerName { get; set; }
        public string Realm { get; set; }
        public string Location { get; set; }
        public string DayText { get; set; }
        public CharacterStatusViewModel Character { get; set; }
        public List<TopResourceViewModel> TopResources { get; } = new List<TopResourceViewModel>();
        public List<RouteNodeViewModel> RouteNodes { get; } = new List<RouteNodeViewModel>();
        public List<CardItemViewModel> CurrentCards { get; } = new List<CardItemViewModel>();
    }

    [Serializable]
    public sealed class TopResourceViewModel
    {
        public string Label { get; set; }
        public string Value { get; set; }
        public string Icon { get; set; }
    }

    [Serializable]
    public sealed class CharacterStatusViewModel
    {
        public string Name { get; set; }
        public string Realm { get; set; }
        public List<StatLineViewModel> Stats { get; } = new List<StatLineViewModel>();
        public List<StatusTagViewModel> StatusTags { get; } = new List<StatusTagViewModel>();
    }

    [Serializable]
    public sealed class StatLineViewModel
    {
        public string Label { get; set; }
        public string Value { get; set; }
        public string Icon { get; set; }
    }

    [Serializable]
    public sealed class StatusTagViewModel
    {
        public string Title { get; set; }
        public string Duration { get; set; }
    }

    [Serializable]
    public sealed class RouteNodeViewModel
    {
        public string Id { get; set; }
        public string Label { get; set; }
        public string Type { get; set; }
        public bool IsCurrent { get; set; }
    }

    [Serializable]
    public sealed class CardItemViewModel
    {
        public string Title { get; set; }
        public string Cost { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
        public string CountText { get; set; }
    }

    [Serializable]
    public sealed class RewardItemViewModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Rarity { get; set; }
    }

    [Serializable]
    public sealed class PopupWindowViewModel
    {
        public string Title { get; set; }
        public string Body { get; set; }
    }
}
