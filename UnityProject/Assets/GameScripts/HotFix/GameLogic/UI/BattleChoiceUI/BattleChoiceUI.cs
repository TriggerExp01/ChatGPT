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
        private readonly Image[] _cardCategoryTints = new Image[3];
        private readonly Image[] _cardTopRibbons = new Image[3];
        private readonly RectTransform[] _costBadgeRects = new RectTransform[3];
        private readonly RectTransform[] _disabledOverlayRects = new RectTransform[3];
        private Text _promptText;
        private RectTransform _panel;

        protected override void ScriptGenerator()
        {
            RectTransform root = RoguelikeUIFactory.ResolveContainer(this);
            _panel = RoguelikeUIFactory.CreatePanel("奖励选择面板", root, new Vector2(0.08f, 0.20f), new Vector2(0.92f, 0.78f), new Color(0.028f, 0.032f, 0.058f, 0.95f));
            RoguelikeUIFactory.CreateImage("奖励面板外部装饰线", _panel, new Vector2(0.32f, 0.795f), new Vector2(0.68f, 0.84f), new Color(0.92f, 0.96f, 1f, 0.38f), RoguelikeUIFactory.DividerFadeSprite, Image.Type.Simple, true);
            RoguelikeUIFactory.CreateImage("奖励面板星辉", _panel, new Vector2(0.02f, 0.82f), new Vector2(0.22f, 0.98f), new Color(1f, 0.72f, 0.92f, 0.58f), RoguelikeUIFactory.BattleLightSprite, Image.Type.Simple, true);
            RoguelikeUIFactory.CreateImage("奖励面板月辉", _panel, new Vector2(0.78f, 0.82f), new Vector2(0.98f, 0.98f), new Color(0.52f, 0.82f, 1f, 0.52f), RoguelikeUIFactory.BattleLightSprite, Image.Type.Simple, true);
            _promptText = RoguelikeUIFactory.CreateText("奖励提示", _panel, 22, TextAnchor.MiddleCenter, new Vector2(0.10f, 0.80f), new Vector2(0.90f, 0.96f), Vector2.zero, Vector2.zero);
            for (int i = 0; i < _choiceButtons.Length; i++)
            {
                int captured = i;
                float xMin = 0.04f + i * 0.315f;
                float xMax = xMin + 0.275f;
                _choiceButtons[i] = RoguelikeUIFactory.CreateButton($"奖励卡_{i + 1}", _panel, string.Empty, new Vector2(xMin, 0.09f), new Vector2(xMax, 0.76f), new Color(0.88f, 0.92f, 1f, 0.98f), 16, RoguelikeUIFactory.CardFrameSprite);
                Text label = _choiceButtons[i].GetComponentInChildren<Text>(true);
                if (label != null)
                {
                    label.gameObject.SetActive(false);
                }

                RectTransform card = _choiceButtons[i].GetComponent<RectTransform>();
                RoguelikeUIFactory.CreateImage($"卡牌正式底纹_{i + 1}", card, new Vector2(0.045f, 0.045f), new Vector2(0.955f, 0.955f), new Color(0.17f, 0.20f, 0.34f, 0.62f), RoguelikeUIFactory.BattleLightSprite, Image.Type.Simple, false);
                RectTransform categoryTint = RoguelikeUIFactory.CreateImage($"卡牌类别底纹_{i + 1}", card, new Vector2(0.08f, 0.52f), new Vector2(0.92f, 0.92f), new Color(0.65f, 0.72f, 1f, 0.20f), RoguelikeUIFactory.BattleLightSprite, Image.Type.Simple, false);
                _cardCategoryTints[i] = categoryTint.GetComponent<Image>();
                RectTransform topRibbon = RoguelikeUIFactory.CreateImage($"卡牌顶部徽带_{i + 1}", card, new Vector2(0.16f, 0.905f), new Vector2(0.84f, 0.965f), new Color(0.86f, 0.92f, 1f, 0.62f), RoguelikeUIFactory.DividerFadeSprite, Image.Type.Simple, true);
                _cardTopRibbons[i] = topRibbon.GetComponent<Image>();
                RoguelikeUIFactory.CreateImage($"卡牌边框_{i + 1}", card, new Vector2(0.05f, 0.05f), new Vector2(0.95f, 0.95f), new Color(1f, 0.76f, 0.94f, 0.20f), RoguelikeUIFactory.CardFrameSprite, Image.Type.Sliced, false);
                RoguelikeUIFactory.CreateImage($"奖励图标底座_{i + 1}", card, new Vector2(0.285f, 0.625f), new Vector2(0.715f, 0.965f), new Color(0.84f, 0.78f, 0.66f, 0.88f), RoguelikeUIFactory.SlotFrameSprite, Image.Type.Sliced, false);
                _disabledOverlayRects[i] = RoguelikeUIFactory.CreateImage($"卡牌不可选遮罩_{i + 1}", card, Vector2.zero, Vector2.one, new Color(0.04f, 0.045f, 0.075f, 0.48f), RoguelikeUIFactory.CardFrameSprite, Image.Type.Sliced, false);
                _iconRects[i] = RoguelikeUIFactory.CreateImage($"奖励图标_{i + 1}", card, new Vector2(0.34f, 0.67f), new Vector2(0.66f, 0.93f), Color.white, RoguelikeUIFactory.WeaponIconSprite, Image.Type.Simple, true);
                RoguelikeUIFactory.CreateImage($"图标高光_{i + 1}", _iconRects[i], new Vector2(0.10f, 0.56f), new Vector2(0.90f, 0.90f), new Color(1f, 0.94f, 0.72f, 0.50f), RoguelikeUIFactory.BattleLightSprite, Image.Type.Simple, true);
                _titleTexts[i] = RoguelikeUIFactory.CreateText($"卡牌标题_{i + 1}", card, 18, TextAnchor.MiddleCenter, new Vector2(0.07f, 0.55f), new Vector2(0.93f, 0.67f), Vector2.zero, Vector2.zero);
                _descriptionTexts[i] = RoguelikeUIFactory.CreateText($"卡牌描述_{i + 1}", card, 15, TextAnchor.UpperCenter, new Vector2(0.08f, 0.28f), new Vector2(0.92f, 0.53f), Vector2.zero, Vector2.zero);
                Image costBadge = RoguelikeUIFactory.CreateFramedIconSlot($"卡牌金币徽章_{i + 1}", card, new Vector2(0.10f, 0.145f), new Vector2(0.225f, 0.27f), new Color(0.90f, 0.74f, 0.46f, 0.94f), Color.white, RoguelikeUIFactory.GoldIconSprite);
                _costBadgeRects[i] = costBadge != null ? costBadge.rectTransform.parent as RectTransform : null;
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
                    ApplyCardPresentation(i, option.Id, canAfford, option.Cost > 0);
                    Color iconColor = GetIconColor(option.Id, canAfford);
                    Image iconImage = _iconRects[i] != null ? _iconRects[i].GetComponent<Image>() : null;
                    if (iconImage != null)
                    {
                        iconImage.color = iconColor;
                        RoguelikeUIFactory.ApplySprite(iconImage, GetIconSprite(option.Id), Image.Type.Simple, true);
                    }

                    RoguelikeUIFactory.SetText(_titleTexts[i], option.Title);
                    RoguelikeUIFactory.SetText(_descriptionTexts[i], option.Description);
                    RoguelikeUIFactory.SetText(_costTexts[i], option.Cost > 0 ? $"花费 {option.Cost} 金币" : "无消耗");
                    RoguelikeUIFactory.SetText(_stateTexts[i], canAfford ? "点击选择" : "金币不足");
                }
                else
                {
                    _choiceButtons[i].interactable = false;
                    ApplyCardPresentation(i, null, false, false);
                    Image iconImage = _iconRects[i] != null ? _iconRects[i].GetComponent<Image>() : null;
                    if (iconImage != null)
                    {
                        iconImage.color = new Color(0.40f, 0.42f, 0.50f, 0.70f);
                        RoguelikeUIFactory.ApplySprite(iconImage, RoguelikeUIFactory.CardFrameSprite, Image.Type.Simple, true);
                    }
                    RoguelikeUIFactory.SetText(_titleTexts[i], "空卡位");
                    RoguelikeUIFactory.SetText(_descriptionTexts[i], "等待新的星辉奖励。");
                    RoguelikeUIFactory.SetText(_costTexts[i], "-");
                    RoguelikeUIFactory.SetText(_stateTexts[i], "不可选");
                }
            }
        }

        private void ApplyCardPresentation(int index, string id, bool canAfford, bool hasCost)
        {
            Color accent = GetCardAccentColor(id, canAfford);
            if (_cardCategoryTints[index] != null)
            {
                _cardCategoryTints[index].color = new Color(accent.r, accent.g, accent.b, canAfford ? 0.30f : 0.12f);
            }

            if (_cardTopRibbons[index] != null)
            {
                _cardTopRibbons[index].color = new Color(accent.r, accent.g, accent.b, canAfford ? 0.76f : 0.34f);
            }

            if (_costBadgeRects[index] != null)
            {
                _costBadgeRects[index].gameObject.SetActive(hasCost);
            }

            if (_disabledOverlayRects[index] != null)
            {
                _disabledOverlayRects[index].gameObject.SetActive(!canAfford);
                if (!canAfford)
                {
                    _disabledOverlayRects[index].SetAsLastSibling();
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

        private static Color GetCardAccentColor(string id, bool enabled)
        {
            if (!enabled)
            {
                return new Color(0.46f, 0.48f, 0.58f, 1f);
            }

            if (!string.IsNullOrEmpty(id) && id.Contains("weapon"))
            {
                return new Color(1f, 0.55f, 0.22f, 1f);
            }

            if (!string.IsNullOrEmpty(id) && id.Contains("passive"))
            {
                return new Color(0.38f, 0.80f, 1f, 1f);
            }

            if (!string.IsNullOrEmpty(id) && id.Contains("paid"))
            {
                return new Color(1f, 0.82f, 0.22f, 1f);
            }

            return new Color(0.92f, 0.56f, 1f, 1f);
        }

        private static string GetIconSprite(string id)
        {
            return RoguelikeUIFactory.ResolveRewardIconSprite(id);
        }
    }
}
