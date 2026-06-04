using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    [Window(UILayer.Top, location: "BattleChoiceUI")]
    class BattleChoiceUI : UIWindow
    {
        private readonly Button[] _choiceButtons = new Button[3];
        private readonly Text[] _choiceTexts = new Text[3];
        private Text _promptText;
        private RectTransform _panel;

        protected override void ScriptGenerator()
        {
            RectTransform root = RoguelikeUIFactory.ResolveContainer(this);
            _panel = RoguelikeUIFactory.CreatePanel("ChoicePanel", root, new Vector2(0.10f, 0.27f), new Vector2(0.90f, 0.62f), new Color(0.02f, 0.035f, 0.055f, 0.97f));
            _promptText = RoguelikeUIFactory.CreateText("Prompt", _panel, 22, TextAnchor.MiddleCenter, new Vector2(0.04f, 0.72f), new Vector2(0.96f, 0.96f), Vector2.zero, Vector2.zero);
            for (int i = 0; i < _choiceButtons.Length; i++)
            {
                int captured = i;
                float xMin = 0.03f + i * 0.325f;
                float xMax = xMin + 0.29f;
                _choiceButtons[i] = RoguelikeUIFactory.CreateButton($"Choice{i + 1}", _panel, "-", new Vector2(xMin, 0.10f), new Vector2(xMax, 0.76f), new Color(0.17f, 0.27f, 0.35f, 0.96f), 17);
                _choiceTexts[i] = _choiceButtons[i].GetComponentInChildren<Text>(true);
                _choiceButtons[i].onClick.AddListener(() => ChooseReward(captured));
            }
        }

        protected override void OnUpdate()
        {
            RefreshChoiceButtons();
        }

        protected override void OnRefresh()
        {
            RefreshChoiceButtons();
        }

        protected override void OnDestroy()
        {
            for (int i = 0; i < _choiceButtons.Length; i++)
            {
                _choiceButtons[i].onClick.RemoveAllListeners();
            }
        }

        private void RefreshChoiceButtons()
        {
            bool choosing = RoguelikeGame.Instance.Phase == RoguelikeGamePhase.RewardChoice;
            _panel.gameObject.SetActive(choosing);
            if (!choosing)
            {
                return;
            }

            RoguelikeUIFactory.SetText(_promptText, $"{RoguelikeGame.Instance.LastMessage}\n战斗暂停，选择后继续。");
            for (int i = 0; i < _choiceButtons.Length; i++)
            {
                if (i < RoguelikeGame.Instance.RewardOptions.Count)
                {
                    RoguelikeChoiceOption option = RoguelikeGame.Instance.RewardOptions[i];
                    bool canAfford = option.CanAfford(RoguelikeGame.Instance.CurrentRun);
                    _choiceButtons[i].interactable = canAfford;
                    string costText = option.Cost > 0 ? $"\n价格 {option.Cost} 金币" : string.Empty;
                    string lockedText = canAfford ? string.Empty : "\n金币不足";
                    _choiceTexts[i].text = $"{option.Title}\n{option.Description}{costText}{lockedText}";
                }
                else
                {
                    _choiceButtons[i].interactable = false;
                    _choiceTexts[i].text = "-";
                }
            }
        }

        private void ChooseReward(int index)
        {
            RoguelikeGame.Instance.ChooseReward(index);
            RefreshChoiceButtons();
        }
    }
}
