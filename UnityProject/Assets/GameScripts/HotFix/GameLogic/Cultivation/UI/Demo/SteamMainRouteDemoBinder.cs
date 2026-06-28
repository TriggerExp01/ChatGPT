using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic.Cultivation.UI
{
    public sealed class SteamMainRouteDemoBinder : MonoBehaviour
    {
        private readonly Dictionary<string, Text> _texts = new Dictionary<string, Text>();

        private void Awake()
        {
            CacheTexts();
            Bind(SteamMainRouteDemoData.Create());
        }

        public void Bind(SteamMainRouteViewModel model)
        {
            if (model == null)
            {
                return;
            }

            if (_texts.Count == 0)
            {
                CacheTexts();
            }

            SetText("Top_Brand", model.PlayerName);
            SetText("Top_Location", model.Location + "，" + model.DayText);

            for (var i = 0; i < model.TopResources.Count; i++)
            {
                var resource = model.TopResources[i];
                SetText("TopResource_" + i + "_Icon", resource.Icon);
                SetText("TopResource_" + i + "_Text", resource.Label + "   " + resource.Value);
                SetText("Top_" + resource.Label + "_Icon", resource.Icon);
                SetText("Top_" + resource.Label, resource.Label + "   " + resource.Value);
            }

            if (model.Character != null)
            {
                SetText("Left_Name", model.Character.Name);
                SetText("Left_RealmValue", model.Character.Realm);
                BindStats(model.Character.Stats);
                BindStatusTags(model.Character.StatusTags);
            }

            for (var i = 0; i < model.RouteNodes.Count; i++)
            {
                var node = model.RouteNodes[i];
                if (!node.IsCurrent)
                {
                    SetText("RouteNode_" + node.Id + "_Label", node.Label);
                }
            }

            for (var i = 0; i < model.CurrentCards.Count; i++)
            {
                BindCard(i, model.CurrentCards[i]);
            }

            SetText("Right_Title", "\u5F53\u524D\u624B\u724C  (" + model.CurrentCards.Count + "/" + model.CurrentCards.Count + ")");
        }

        private void BindStats(IReadOnlyList<StatLineViewModel> stats)
        {
            for (var i = 0; i < stats.Count; i++)
            {
                var stat = stats[i];
                SetText("Left_Stat_" + i + "_Icon", stat.Icon);
                SetText("Left_Stat_" + i + "_Label", stat.Label);
                SetText("Left_Stat_" + i + "_Value", stat.Value);
                SetText("Left_Icon_" + stat.Label, stat.Icon);
                SetText("Left_Label_" + stat.Label, stat.Label);
                SetText("Left_Value_" + stat.Label, stat.Value);
            }
        }

        private void BindStatusTags(IReadOnlyList<StatusTagViewModel> tags)
        {
            for (var i = 0; i < tags.Count; i++)
            {
                SetText("Left_StatusTag_" + i + "_Title", tags[i].Title);
                SetText("Left_StatusTag_" + i + "_Duration", tags[i].Duration);
                var key = NormalizeKey(tags[i].Title);
                SetText("Left_StatusGlyph_" + key, tags[i].Title);
                SetText("Left_StatusDays_" + key, tags[i].Duration);
            }
        }

        private void BindCard(int index, CardItemViewModel card)
        {
            SetText("Right_Card_" + index + "_Cost", card.Cost);
            SetText("Right_Card_" + index + "_Title", card.Title);
            SetText("Right_Card_" + index + "_Type", card.Type);
            SetText("Right_Card_" + index + "_Desc", card.Description);
            SetText("Right_Card_" + index + "_Count", card.CountText);

            var key = NormalizeKey(card.Title);
            SetText("Right_Cost_" + key, card.Cost);
            SetText("Right_CardTitle_" + key, card.Title);
            SetText("Right_Type_" + key, card.Type);
            SetText("Right_Desc_" + key, card.Description);
        }

        private void CacheTexts()
        {
            _texts.Clear();
            foreach (var text in GetComponentsInChildren<Text>(true))
            {
                _texts[text.gameObject.name] = text;
            }
        }

        private void SetText(string name, string value)
        {
            if (_texts.TryGetValue(name, out var text))
            {
                text.text = value ?? string.Empty;
            }
        }

        private static string NormalizeKey(string value)
        {
            return (value ?? string.Empty)
                .Replace("\r", string.Empty)
                .Replace("\n", string.Empty)
                .Replace(" ", string.Empty);
        }
    }
}
