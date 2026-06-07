using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    [Window(UILayer.Top, location: "BattleChoiceUI")]
    class BattleChoiceUI : UIWindow
    {
        private readonly Button[] _choiceButtons = new Button[3];
        private readonly Text[] _titleTexts = new Text[3];
        private readonly Text[] _descriptionTexts = new Text[3];
        private readonly Text[] _costTexts = new Text[3];
        private readonly Text[] _stateTexts = new Text[3];
        private readonly RectTransform[] _iconRects = new RectTransform[3];
        private Text _promptText;
        private RectTransform _panel;

        protected override void ScriptGenerator()
        {
            RectTransform root = RoguelikeUIFactory.ResolveContainer(this);
            _panel = RoguelikeUIFactory.CreatePanel("奖励选择面板", root, new Vector2(0.08f, 0.20f), new Vector2(0.92f, 0.78f), new Color(0.028f, 0.032f, 0.058f, 0.95f));
            RoguelikeUIFactory.CreateImage("奖励面板星辉", _panel, new Vector2(0.02f, 0.84f), new Vector2(0.12f, 0.96f), new Color(1f, 0.72f, 0.92f, 0.76f));
            RoguelikeUIFactory.CreateImage("奖励面板月辉", _panel, new Vector2(0.88f, 0.84f), new Vector2(0.98f, 0.96f), new Color(0.52f, 0.82f, 1f, 0.70f));
            _promptText = RoguelikeUIFactory.CreateText("奖励提示", _panel, 22, TextAnchor.MiddleCenter, new Vector2(0.10f, 0.80f), new Vector2(0.90f, 0.96f), Vector2.zero, Vector2.zero);
            for (int i = 0; i < _choiceButtons.Length; i++)
            {
                int captured = i;
                float xMin = 0.04f + i * 0.315f;
                float xMax = xMin + 0.275f;
                _choiceButtons[i] = RoguelikeUIFactory.CreateButton($"奖励卡_{i + 1}", _panel, string.Empty, new Vector2(xMin, 0.09f), new Vector2(xMax, 0.76f), new Color(0.13f, 0.16f, 0.25f, 0.98f), 16);
                Text label = _choiceButtons[i].GetComponentInChildren<Text>(true);
                if (label != null)
                {
                    label.gameObject.SetActive(false);
                }

                RectTransform card = _choiceButtons[i].GetComponent<RectTransform>();
                RoguelikeUIFactory.CreateImage($"卡牌边框_{i + 1}", card, new Vector2(0.03f, 0.035f), new Vector2(0.97f, 0.965f), new Color(0.92f, 0.66f, 1f, 0.18f));
                _iconRects[i] = RoguelikeUIFactory.CreateImage($"奖励图标_{i + 1}", card, new Vector2(0.34f, 0.68f), new Vector2(0.66f, 0.92f), new Color(1f, 0.72f, 0.38f, 0.94f));
                RoguelikeUIFactory.CreateImage($"图标高光_{i + 1}", _iconRects[i], new Vector2(0.18f, 0.58f), new Vector2(0.82f, 0.84f), new Color(1f, 0.94f, 0.72f, 0.82f));
                _titleTexts[i] = RoguelikeUIFactory.CreateText($"卡牌标题_{i + 1}", card, 18, TextAnchor.MiddleCenter, new Vector2(0.07f, 0.55f), new Vector2(0.93f, 0.67f), Vector2.zero, Vector2.zero);
                _descriptionTexts[i] = RoguelikeUIFactory.CreateText($"卡牌描述_{i + 1}", card, 15, TextAnchor.UpperCenter, new Vector2(0.08f, 0.28f), new Vector2(0.92f, 0.53f), Vector2.zero, Vector2.zero);
                _costTexts[i] = RoguelikeUIFactory.CreateText($"卡牌价格_{i + 1}", card, 14, TextAnchor.MiddleCenter, new Vector2(0.10f, 0.15f), new Vector2(0.90f, 0.25f), Vector2.zero, Vector2.zero);
                _stateTexts[i] = RoguelikeUIFactory.CreateText($"卡牌状态_{i + 1}", card, 15, TextAnchor.MiddleCenter, new Vector2(0.10f, 0.045f), new Vector2(0.90f, 0.14f), Vector2.zero, Vector2.zero);
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

            RoguelikeGame game = RoguelikeGame.Instance;
            RoguelikeUIFactory.SetText(_promptText, $"{game.LastMessage}\n{game.OperationHint}");
            for (int i = 0; i < _choiceButtons.Length; i++)
            {
                if (i < game.RewardOptions.Count)
                {
                    RoguelikeChoiceOption option = game.RewardOptions[i];
                    bool canAfford = option.CanAfford(game.CurrentRun);
                    _choiceButtons[i].interactable = canAfford;
                    Color iconColor = GetIconColor(option.Id, canAfford);
                    Image iconImage = _iconRects[i] != null ? _iconRects[i].GetComponent<Image>() : null;
                    if (iconImage != null)
                    {
                        iconImage.color = iconColor;
                    }

                    RoguelikeUIFactory.SetText(_titleTexts[i], option.Title);
                    RoguelikeUIFactory.SetText(_descriptionTexts[i], option.Description);
                    RoguelikeUIFactory.SetText(_costTexts[i], option.Cost > 0 ? $"花费 {option.Cost} 金币" : "无消耗");
                    RoguelikeUIFactory.SetText(_stateTexts[i], canAfford ? "点击选择" : "金币不足");
                }
                else
                {
                    _choiceButtons[i].interactable = false;
                    RoguelikeUIFactory.SetText(_titleTexts[i], "空卡位");
                    RoguelikeUIFactory.SetText(_descriptionTexts[i], "等待新的星辉奖励。");
                    RoguelikeUIFactory.SetText(_costTexts[i], "-");
                    RoguelikeUIFactory.SetText(_stateTexts[i], "不可选");
                }
            }
        }

        private void ChooseReward(int index)
        {
            RoguelikeGame.Instance.PlayUiConfirmSound();
            RoguelikeGame.Instance.ChooseReward(index);
            RefreshChoiceButtons();
        }

        private static Color GetIconColor(string id, bool enabled)
        {
            if (!enabled)
            {
                return new Color(0.36f, 0.36f, 0.42f, 0.86f);
            }

            if (!string.IsNullOrEmpty(id) && id.Contains("weapon"))
            {
                return new Color(1f, 0.62f, 0.24f, 0.96f);
            }

            if (!string.IsNullOrEmpty(id) && id.Contains("passive"))
            {
                return new Color(0.48f, 0.82f, 1f, 0.96f);
            }

            if (!string.IsNullOrEmpty(id) && id.Contains("paid"))
            {
                return new Color(1f, 0.86f, 0.32f, 0.96f);
            }

            return new Color(0.92f, 0.56f, 1f, 0.96f);
        }
    }
}
