using System;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace GameLogic.Cultivation
{
    public sealed class CultivationRunPrototypeUI : MonoBehaviour
    {
        public const string RootName = "CultivationRunPrototypeUI";

        private BattleEngine _battleEngine;
        private CultivationRunEngine _runEngine;
        private CultivationRunState _run;
        private RectTransform _handRoot;
        private RectTransform _choiceRoot;
        private Text _runText;
        private Text _nodeText;
        private Text _battleText;
        private Text _deckText;
        private Text _logText;
        private Button _endTurnButton;
        private Button _resetButton;

        public RunPrototypeSnapshot Snapshot => CultivationRunPrototypePresenter.CreateSnapshot(_run);

        public static CultivationRunPrototypeUI Open(Transform parent = null)
        {
            var uiParent = parent != null ? parent : ResolveParent();
            var old = uiParent.Find(RootName);
            if (old != null)
            {
                var existing = old.GetComponent<CultivationRunPrototypeUI>();
                if (existing != null)
                {
                    existing.ResetRun();
                    return existing;
                }

                DestroyObject(old.gameObject);
            }

            EnsureEventSystem();
            var root = CreateRect(RootName, uiParent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var view = root.gameObject.AddComponent<CultivationRunPrototypeUI>();
            view.Initialize();
            return view;
        }

        public void ResetRun()
        {
            _battleEngine = new BattleEngine(20260620);
            _runEngine = new CultivationRunEngine(_battleEngine);
            _run = _runEngine.StartRun(CultivationSeedData.CreateSwordSectStarterDeck(), CultivationSeedData.CreateFirstPrototypeBranchingRoute());
            _run.CurrentBattle.Logs.Add(new BattleLogEntry("Phase 7 Run 原型界面已连接战斗、奖励、闭关升级和路线选择。"));
            Refresh();
        }

        public void PlayCardAt(int handIndex)
        {
            if (_run == null || _run.Status != CultivationRunStatus.InBattle || _run.CurrentBattle == null || _run.CurrentBattle.Outcome != BattleOutcome.InProgress)
            {
                return;
            }

            if (handIndex < 0 || handIndex >= _run.CurrentBattle.Hand.Count)
            {
                return;
            }

            var card = _run.CurrentBattle.Hand[handIndex];
            var target = _run.CurrentBattle.Enemies.FirstOrDefault(enemy => !enemy.Body.IsDefeated);
            if (!_battleEngine.CanPlay(_run.CurrentBattle, card))
            {
                _run.CurrentBattle.Logs.Add(new BattleLogEntry($"{card.Name} 灵力不足，无法打出。"));
                Refresh();
                return;
            }

            _battleEngine.PlayCard(_run.CurrentBattle, card, target);
            Refresh();
        }

        public void EndTurn()
        {
            if (_run == null || _run.Status != CultivationRunStatus.InBattle || _run.CurrentBattle == null || _run.CurrentBattle.Outcome != BattleOutcome.InProgress)
            {
                return;
            }

            _battleEngine.EndPlayerTurn(_run.CurrentBattle);
            Refresh();
        }

        public void ResolveBattle()
        {
            if (_run == null || _run.Status != CultivationRunStatus.InBattle || _run.CurrentBattle == null || _run.CurrentBattle.Outcome == BattleOutcome.InProgress)
            {
                return;
            }

            _runEngine.ResolveBattleResult(_run);
            Refresh();
        }

        public void ChooseReward(int rewardIndex)
        {
            if (_run == null || _run.Status != CultivationRunStatus.Reward)
            {
                return;
            }

            _runEngine.ChooseReward(_run, rewardIndex);
            Refresh();
        }

        public void SkipReward()
        {
            if (_run == null || _run.Status != CultivationRunStatus.Reward)
            {
                return;
            }

            _runEngine.SkipReward(_run);
            Refresh();
        }

        public void Rest()
        {
            if (_run == null || _run.Status != CultivationRunStatus.Rest)
            {
                return;
            }

            _runEngine.Rest(_run);
            Refresh();
        }

        public void RestAndUpgrade(int deckIndex, int upgradeOptionIndex)
        {
            if (_run == null || _run.Status != CultivationRunStatus.Rest)
            {
                return;
            }

            _runEngine.RestAndUpgrade(_run, deckIndex, upgradeOptionIndex);
            Refresh();
        }

        public void ChooseRoute(int choiceIndex)
        {
            if (_run == null || _run.Status != CultivationRunStatus.RouteChoice)
            {
                return;
            }

            _runEngine.ChooseRoute(_run, choiceIndex);
            Refresh();
        }

        public void BuyMarketItem(int itemIndex)
        {
            if (_run == null || _run.Status != CultivationRunStatus.Market)
            {
                return;
            }

            _runEngine.BuyMarketItem(_run, itemIndex);
            Refresh();
        }

        public void RemoveDeckCardAtMarket(int deckIndex)
        {
            if (_run == null || _run.Status != CultivationRunStatus.Market)
            {
                return;
            }

            _runEngine.RemoveDeckCardAtMarket(_run, deckIndex);
            Refresh();
        }

        public void SellDeckCardAtMarket(int deckIndex)
        {
            if (_run == null || _run.Status != CultivationRunStatus.Market)
            {
                return;
            }

            _runEngine.SellDeckCardAtMarket(_run, deckIndex);
            Refresh();
        }

        public void UpgradeDeckCardAtMarket(int deckIndex, int upgradeOptionIndex)
        {
            if (_run == null || _run.Status != CultivationRunStatus.Market)
            {
                return;
            }

            _runEngine.UpgradeDeckCardAtMarket(_run, deckIndex, upgradeOptionIndex);
            Refresh();
        }

        public void LeaveMarket()
        {
            if (_run == null || _run.Status != CultivationRunStatus.Market)
            {
                return;
            }

            _runEngine.LeaveMarket(_run);
            Refresh();
        }

        private void Initialize()
        {
            if (_handRoot == null)
            {
                BuildView();
            }

            ResetRun();
        }

        private void BuildView()
        {
            var background = gameObject.AddComponent<Image>();
            background.color = new Color(0.05f, 0.06f, 0.08f, 0.97f);

            var rootLayout = gameObject.AddComponent<VerticalLayoutGroup>();
            rootLayout.padding = new RectOffset(24, 24, 20, 20);
            rootLayout.spacing = 12;
            rootLayout.childForceExpandWidth = true;
            rootLayout.childForceExpandHeight = false;
            rootLayout.childControlWidth = true;
            rootLayout.childControlHeight = true;

            var title = CreateText("Title", transform, "仙途·天命  Run 原型", 30, FontStyle.Bold, TextAnchor.MiddleLeft);
            title.color = new Color(0.95f, 0.89f, 0.72f, 1f);
            SetLayout(title.gameObject, flexibleWidth: 1, preferredHeight: 40);

            var body = CreateRect("Body", transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var bodyLayout = body.gameObject.AddComponent<HorizontalLayoutGroup>();
            bodyLayout.spacing = 12;
            bodyLayout.childForceExpandWidth = true;
            bodyLayout.childForceExpandHeight = true;
            bodyLayout.childControlWidth = true;
            bodyLayout.childControlHeight = true;
            SetLayout(body.gameObject, flexibleWidth: 1, flexibleHeight: 1, preferredHeight: 430);

            var left = CreatePanel("RunPanel", body, new Color(0.10f, 0.12f, 0.16f, 0.95f));
            SetLayout(left.gameObject, flexibleWidth: 1, flexibleHeight: 1);
            _runText = CreateText("RunText", left, string.Empty, 18, FontStyle.Bold, TextAnchor.UpperLeft);
            _runText.color = Color.white;
            SetLayout(_runText.gameObject, flexibleWidth: 1, preferredHeight: 150);
            _nodeText = CreateText("NodeText", left, string.Empty, 16, FontStyle.Normal, TextAnchor.UpperLeft);
            _nodeText.color = new Color(0.80f, 0.86f, 0.92f, 1f);
            SetLayout(_nodeText.gameObject, flexibleWidth: 1, flexibleHeight: 1);

            var center = CreatePanel("BattlePanel", body, new Color(0.15f, 0.10f, 0.10f, 0.95f));
            SetLayout(center.gameObject, flexibleWidth: 1, flexibleHeight: 1);
            _battleText = CreateText("BattleText", center, string.Empty, 18, FontStyle.Bold, TextAnchor.UpperLeft);
            _battleText.color = Color.white;
            SetLayout(_battleText.gameObject, flexibleWidth: 1, flexibleHeight: 1);

            var right = CreatePanel("DeckPanel", body, new Color(0.08f, 0.10f, 0.12f, 0.95f));
            SetLayout(right.gameObject, flexibleWidth: 1, flexibleHeight: 1);
            _deckText = CreateText("DeckText", right, string.Empty, 15, FontStyle.Normal, TextAnchor.UpperLeft);
            _deckText.color = new Color(0.84f, 0.88f, 0.90f, 1f);
            SetLayout(_deckText.gameObject, flexibleWidth: 1, flexibleHeight: 1);
            _logText = CreateText("LogText", right, string.Empty, 14, FontStyle.Normal, TextAnchor.UpperLeft);
            _logText.color = new Color(0.72f, 0.78f, 0.82f, 1f);
            SetLayout(_logText.gameObject, flexibleWidth: 1, preferredHeight: 150);

            _choiceRoot = CreateRect("Choices", transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var choiceLayout = _choiceRoot.gameObject.AddComponent<HorizontalLayoutGroup>();
            choiceLayout.spacing = 10;
            choiceLayout.childForceExpandWidth = true;
            choiceLayout.childForceExpandHeight = true;
            choiceLayout.childControlWidth = true;
            choiceLayout.childControlHeight = true;
            SetLayout(_choiceRoot.gameObject, flexibleWidth: 1, preferredHeight: 110);

            _handRoot = CreateRect("Hand", transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var handLayout = _handRoot.gameObject.AddComponent<HorizontalLayoutGroup>();
            handLayout.spacing = 10;
            handLayout.childForceExpandWidth = true;
            handLayout.childForceExpandHeight = true;
            handLayout.childControlWidth = true;
            handLayout.childControlHeight = true;
            SetLayout(_handRoot.gameObject, flexibleWidth: 1, preferredHeight: 138);

            var actionBar = CreateRect("ActionBar", transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var actionLayout = actionBar.gameObject.AddComponent<HorizontalLayoutGroup>();
            actionLayout.spacing = 12;
            actionLayout.childAlignment = TextAnchor.MiddleRight;
            actionLayout.childForceExpandWidth = false;
            actionLayout.childForceExpandHeight = true;
            actionLayout.childControlWidth = true;
            actionLayout.childControlHeight = true;
            SetLayout(actionBar.gameObject, flexibleWidth: 1, preferredHeight: 54);

            _resetButton = CreateButton("ResetButton", actionBar, "重开 Run");
            _resetButton.onClick.AddListener(ResetRun);
            SetLayout(_resetButton.gameObject, preferredWidth: 170, preferredHeight: 48);

            _endTurnButton = CreateButton("EndTurnButton", actionBar, "结束回合");
            _endTurnButton.onClick.AddListener(EndTurn);
            SetLayout(_endTurnButton.gameObject, preferredWidth: 170, preferredHeight: 48);
        }

        private void Refresh()
        {
            if (_run == null)
            {
                return;
            }

            var text = CultivationRunPrototypePresenter.BuildText(_run);
            _runText.text = text.RunText;
            _nodeText.text = text.NodeText;
            _battleText.text = text.BattleText;
            _deckText.text = text.DeckText;
            _logText.text = text.LogText;
            _endTurnButton.interactable = _run.Status == CultivationRunStatus.InBattle && _run.CurrentBattle != null && _run.CurrentBattle.Outcome == BattleOutcome.InProgress;

            RebuildChoices();
            RebuildHand();
        }

        private void RebuildChoices()
        {
            ClearChildren(_choiceRoot);

            switch (_run.Status)
            {
                case CultivationRunStatus.InBattle:
                    if (_run.CurrentBattle != null && _run.CurrentBattle.Outcome != BattleOutcome.InProgress)
                    {
                        var resolve = CreateButton("ResolveBattleButton", _choiceRoot, _run.CurrentBattle.Outcome == BattleOutcome.Victory ? "结算胜利" : "结算失败");
                        resolve.onClick.AddListener(ResolveBattle);
                        SetLayout(resolve.gameObject, flexibleWidth: 1, preferredHeight: 96);
                    }

                    break;
                case CultivationRunStatus.Reward:
                    for (var i = 0; i < _run.CurrentRewards.Count; i++)
                    {
                        var index = i;
                        var reward = _run.CurrentRewards[i];
                        var button = CreateButton($"Reward_{i}_{reward.Id}", _choiceRoot, $"奖励\n{reward.Card.Name}\n{CultivationRunPrototypePresenter.FormatCardSummary(reward.Card)}");
                        button.onClick.AddListener(() => ChooseReward(index));
                        SetLayout(button.gameObject, flexibleWidth: 1, preferredHeight: 96);
                    }

                    var skip = CreateButton("SkipRewardButton", _choiceRoot, "跳过奖励");
                    skip.onClick.AddListener(SkipReward);
                    SetLayout(skip.gameObject, flexibleWidth: 1, preferredHeight: 96);
                    break;
                case CultivationRunStatus.Rest:
                    var restOnly = CreateButton("RestOnlyButton", _choiceRoot, $"只闭关恢复\n+{_run.CurrentNode.RestHealAmount} HP");
                    restOnly.onClick.AddListener(Rest);
                    SetLayout(restOnly.gameObject, flexibleWidth: 1, preferredHeight: 96);
                    foreach (var choice in _run.RestUpgradeChoices)
                    {
                        for (var i = 0; i < choice.SourceCard.UpgradeOptions.Count; i++)
                        {
                            var optionIndex = i;
                            var option = choice.SourceCard.UpgradeOptions[i];
                            var deckIndex = choice.DeckIndex;
                            var button = CreateButton($"Upgrade_{deckIndex}_{optionIndex}", _choiceRoot, $"{choice.SourceCard.Name}\n→ {option.UpgradedCard.Name}\n{option.Description}");
                            button.onClick.AddListener(() => RestAndUpgrade(deckIndex, optionIndex));
                            SetLayout(button.gameObject, flexibleWidth: 1, preferredHeight: 96);
                        }
                    }

                    break;
                case CultivationRunStatus.RouteChoice:
                    for (var i = 0; i < _run.CurrentRouteChoices.Count; i++)
                    {
                        var index = i;
                        var choice = _run.CurrentRouteChoices[i];
                        var button = CreateButton($"Route_{i}_{choice.TargetNode.Id}", _choiceRoot, $"前往\n{choice.TargetNode.Name}\n{choice.TargetNode.Type}");
                        button.onClick.AddListener(() => ChooseRoute(index));
                        SetLayout(button.gameObject, flexibleWidth: 1, preferredHeight: 96);
                    }

                    break;
                case CultivationRunStatus.Market:
                    for (var i = 0; i < _run.CurrentMarketItems.Count; i++)
                    {
                        var index = i;
                        var item = _run.CurrentMarketItems[i];
                        var button = CreateButton($"Market_{i}_{item.Id}", _choiceRoot, $"购买\n{item.Card.Name}\n{item.Price} 灵石\n{CultivationRunPrototypePresenter.FormatCardSummary(item.Card)}");
                        button.interactable = _run.SpiritStones >= item.Price;
                        button.onClick.AddListener(() => BuyMarketItem(index));
                        SetLayout(button.gameObject, flexibleWidth: 1, preferredHeight: 96);
                    }

                    var leave = CreateButton("LeaveMarketButton", _choiceRoot, "离开坊市");
                    leave.onClick.AddListener(LeaveMarket);
                    SetLayout(leave.gameObject, flexibleWidth: 1, preferredHeight: 96);
                    break;
            }
        }

        private void RebuildHand()
        {
            ClearChildren(_handRoot);

            if (_run.Status == CultivationRunStatus.Market)
            {
                for (var i = 0; i < _run.Deck.Count; i++)
                {
                    var index = i;
                    var card = _run.Deck[i];
                    var button = CreateButton($"RemoveDeck_{i}_{card.Id}", _handRoot, $"移除\n{card.Name}\n{CultivationRunEngine.MarketCardRemovalCost} 灵石");
                    button.interactable = _run.Deck.Count > 1 && _run.SpiritStones >= CultivationRunEngine.MarketCardRemovalCost;
                    button.onClick.AddListener(() => RemoveDeckCardAtMarket(index));
                    SetLayout(button.gameObject, flexibleWidth: 1, preferredHeight: 130);

                    var sellValue = _runEngine.GetMarketSellValue(_run, index);
                    var sellButton = CreateButton($"SellDeck_{i}_{card.Id}", _handRoot, $"出售\n{card.Name}\n+{sellValue} 灵石");
                    sellButton.interactable = _run.Deck.Count > 1;
                    sellButton.onClick.AddListener(() => SellDeckCardAtMarket(index));
                    SetLayout(sellButton.gameObject, flexibleWidth: 1, preferredHeight: 130);

                    for (var optionIndex = 0; optionIndex < card.UpgradeOptions.Count; optionIndex++)
                    {
                        var selectedOptionIndex = optionIndex;
                        var option = card.UpgradeOptions[optionIndex];
                        var upgradeButton = CreateButton($"MarketUpgrade_{i}_{optionIndex}_{option.Id}", _handRoot, $"升级\n{card.Name}\n→ {option.UpgradedCard.Name}\n{CultivationRunEngine.MarketCardUpgradeCost} 灵石");
                        upgradeButton.interactable = _run.SpiritStones >= CultivationRunEngine.MarketCardUpgradeCost;
                        upgradeButton.onClick.AddListener(() => UpgradeDeckCardAtMarket(index, selectedOptionIndex));
                        SetLayout(upgradeButton.gameObject, flexibleWidth: 1, preferredHeight: 130);
                    }
                }

                return;
            }

            if (_run.Status != CultivationRunStatus.InBattle || _run.CurrentBattle == null)
            {
                return;
            }

            for (var i = 0; i < _run.CurrentBattle.Hand.Count; i++)
            {
                var index = i;
                var card = _run.CurrentBattle.Hand[i];
                var button = CreateButton($"Card_{i}_{card.Id}", _handRoot, $"{card.Name}\n{CultivationRunPrototypePresenter.FormatCardSummary(card)}");
                button.interactable = _run.CurrentBattle.Outcome == BattleOutcome.InProgress && _battleEngine.CanPlay(_run.CurrentBattle, card);
                button.onClick.AddListener(() => PlayCardAt(index));
                SetLayout(button.gameObject, flexibleWidth: 1, preferredHeight: 130);
            }
        }

        private static Transform ResolveParent()
        {
            if (UIModule.UIRoot != null)
            {
                return UIModule.UIRoot;
            }

            var uiRoot = GameObject.Find("UIRoot");
            if (uiRoot != null)
            {
                var canvas = uiRoot.GetComponentInChildren<Canvas>();
                if (canvas != null)
                {
                    return canvas.transform;
                }
            }

            var fallback = new GameObject("RuntimePrototypeCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var fallbackCanvas = fallback.GetComponent<Canvas>();
            fallbackCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = fallback.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            if (Application.isPlaying)
            {
                Object.DontDestroyOnLoad(fallback);
            }

            return fallback.transform;
        }

        private static void EnsureEventSystem()
        {
            if (FindObjectOfType<EventSystem>() != null)
            {
                return;
            }

            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            if (Application.isPlaying)
            {
                Object.DontDestroyOnLoad(eventSystem);
            }
        }

        private static RectTransform CreatePanel(string name, Transform parent, Color color)
        {
            var rect = CreateRect(name, parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            var layout = rect.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(16, 16, 14, 14);
            layout.spacing = 10;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            return rect;
        }

        private static Text CreateText(string name, Transform parent, string value, int fontSize, FontStyle style, TextAnchor anchor)
        {
            var rect = CreateRect(name, parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var text = rect.gameObject.AddComponent<Text>();
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.alignment = anchor;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        private static Button CreateButton(string name, Transform parent, string label)
        {
            var rect = CreateRect(name, parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = new Color(0.22f, 0.25f, 0.31f, 1f);
            var button = rect.gameObject.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = image.color;
            colors.highlightedColor = new Color(0.34f, 0.38f, 0.46f, 1f);
            colors.pressedColor = new Color(0.16f, 0.18f, 0.22f, 1f);
            colors.disabledColor = new Color(0.12f, 0.13f, 0.15f, 0.72f);
            button.colors = colors;

            var text = CreateText("Label", rect, label, 15, FontStyle.Bold, TextAnchor.MiddleCenter);
            text.color = Color.white;
            text.raycastTarget = false;
            SetStretch(text.rectTransform);
            return button;
        }

        private static RectTransform CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            rect.localScale = Vector3.one;
            return rect;
        }

        private static void SetStretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void SetLayout(GameObject go, float preferredWidth = -1f, float preferredHeight = -1f, float flexibleWidth = -1f, float flexibleHeight = -1f)
        {
            var layout = go.GetComponent<LayoutElement>() ?? go.AddComponent<LayoutElement>();
            if (preferredWidth >= 0f)
            {
                layout.preferredWidth = preferredWidth;
            }

            if (preferredHeight >= 0f)
            {
                layout.preferredHeight = preferredHeight;
            }

            if (flexibleWidth >= 0f)
            {
                layout.flexibleWidth = flexibleWidth;
            }

            if (flexibleHeight >= 0f)
            {
                layout.flexibleHeight = flexibleHeight;
            }
        }

        private static void ClearChildren(Transform parent)
        {
            for (var i = parent.childCount - 1; i >= 0; i--)
            {
                DestroyObject(parent.GetChild(i).gameObject);
            }
        }

        private static void DestroyObject(GameObject target)
        {
            if (target == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(target);
            }
            else
            {
                DestroyImmediate(target);
            }
        }
    }
}
