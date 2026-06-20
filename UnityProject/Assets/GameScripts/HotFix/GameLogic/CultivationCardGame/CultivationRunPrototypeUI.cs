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
        private string _lastRunPillMessage;
        private RectTransform _statsRoot;
        private RectTransform _mapRoot;
        private RectTransform _handRoot;
        private RectTransform _choiceRoot;
        private Text _runText;
        private Text _nodeText;
        private Text _battleText;
        private Text _deckText;
        private Text _logText;
        private Text _titleText;
        private Text _stageText;
        private Text _statusText;
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

        public void UsePillInBattle(int pillIndex)
        {
            if (_run == null || _run.Status != CultivationRunStatus.InBattle || _run.CurrentBattle == null || _run.CurrentBattle.Outcome != BattleOutcome.InProgress)
            {
                return;
            }

            _runEngine.UsePillInBattle(_run, pillIndex);
            Refresh();
        }

        public void UsePillInRun(int pillIndex)
        {
            if (_run == null || _run.Status == CultivationRunStatus.InBattle)
            {
                return;
            }

            _lastRunPillMessage = _runEngine.UsePillInRun(_run, pillIndex);
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

        public void OpenChest()
        {
            if (_run == null || _run.Status != CultivationRunStatus.Chest)
            {
                return;
            }

            _runEngine.OpenChest(_run);
            Refresh();
        }

        public void ChooseMysticEventOption(int optionIndex)
        {
            if (_run == null || _run.Status != CultivationRunStatus.Mystic)
            {
                return;
            }

            _runEngine.ChooseMysticEventOption(_run, optionIndex);
            Refresh();
        }

        public void ChooseGoldenCorePassive(int passiveIndex)
        {
            if (_run == null || _run.Status != CultivationRunStatus.GoldenCorePassiveChoice)
            {
                return;
            }

            _runEngine.ChooseGoldenCorePassive(_run, passiveIndex);
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
            background.color = new Color(0.035f, 0.042f, 0.050f, 0.98f);

            var rootLayout = gameObject.AddComponent<VerticalLayoutGroup>();
            rootLayout.padding = new RectOffset(22, 22, 18, 18);
            rootLayout.spacing = 10;
            rootLayout.childForceExpandWidth = true;
            rootLayout.childForceExpandHeight = false;
            rootLayout.childControlWidth = true;
            rootLayout.childControlHeight = true;

            var header = CreatePanel("Header", transform, new Color(0.09f, 0.105f, 0.125f, 0.98f));
            var headerLayout = header.GetComponent<VerticalLayoutGroup>();
            headerLayout.padding = new RectOffset(18, 18, 10, 10);
            SetLayout(header.gameObject, flexibleWidth: 1, preferredHeight: 166);

            var titleRow = CreateRect("TitleRow", header, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var titleRowLayout = titleRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            titleRowLayout.spacing = 12;
            titleRowLayout.childForceExpandWidth = true;
            titleRowLayout.childForceExpandHeight = true;
            titleRowLayout.childControlWidth = true;
            titleRowLayout.childControlHeight = true;
            SetLayout(titleRow.gameObject, flexibleWidth: 1, preferredHeight: 56);

            var titleBox = CreateRect("TitleBox", titleRow, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var titleBoxLayout = titleBox.gameObject.AddComponent<VerticalLayoutGroup>();
            titleBoxLayout.spacing = 4;
            titleBoxLayout.childForceExpandWidth = true;
            titleBoxLayout.childForceExpandHeight = false;
            titleBoxLayout.childControlWidth = true;
            titleBoxLayout.childControlHeight = true;
            SetLayout(titleBox.gameObject, flexibleWidth: 1, flexibleHeight: 1);

            _titleText = CreateText("Title", titleBox, "仙途·天命", 28, FontStyle.Bold, TextAnchor.MiddleLeft);
            _titleText.color = new Color(0.96f, 0.86f, 0.56f, 1f);
            SetLayout(_titleText.gameObject, flexibleWidth: 1, preferredHeight: 30);

            _stageText = CreateText("StageText", titleBox, string.Empty, 15, FontStyle.Normal, TextAnchor.MiddleLeft);
            _stageText.color = new Color(0.58f, 0.78f, 0.72f, 1f);
            SetLayout(_stageText.gameObject, flexibleWidth: 1, preferredHeight: 22);

            _statusText = CreateText("StatusText", titleRow, string.Empty, 18, FontStyle.Bold, TextAnchor.MiddleRight);
            _statusText.color = new Color(0.96f, 0.78f, 0.42f, 1f);
            SetLayout(_statusText.gameObject, preferredWidth: 420, flexibleHeight: 1);

            _statsRoot = CreateRowContent("StatsBar", header, "资源", 52);
            _mapRoot = CreateRowContent("MapBar", header, "路线图", 52);

            var body = CreateRect("Body", transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var bodyLayout = body.gameObject.AddComponent<HorizontalLayoutGroup>();
            bodyLayout.spacing = 12;
            bodyLayout.childForceExpandWidth = true;
            bodyLayout.childForceExpandHeight = true;
            bodyLayout.childControlWidth = true;
            bodyLayout.childControlHeight = true;
            SetLayout(body.gameObject, flexibleWidth: 1, flexibleHeight: 1, preferredHeight: 520);

            var left = CreatePanel("RunPanel", body, new Color(0.085f, 0.105f, 0.13f, 0.96f));
            SetLayout(left.gameObject, preferredWidth: 350, flexibleHeight: 1);
            CreateSectionLabel(left, "运行总览");
            _runText = CreateText("RunText", left, string.Empty, 16, FontStyle.Bold, TextAnchor.UpperLeft);
            _runText.color = Color.white;
            SetLayout(_runText.gameObject, flexibleWidth: 1, preferredHeight: 210);
            CreateSectionLabel(left, "当前节点");
            _nodeText = CreateText("NodeText", left, string.Empty, 15, FontStyle.Normal, TextAnchor.UpperLeft);
            _nodeText.color = new Color(0.80f, 0.88f, 0.88f, 1f);
            SetLayout(_nodeText.gameObject, flexibleWidth: 1, flexibleHeight: 1);

            var center = CreatePanel("BattlePanel", body, new Color(0.13f, 0.085f, 0.075f, 0.96f));
            SetLayout(center.gameObject, flexibleWidth: 1.45f, flexibleHeight: 1);
            CreateSectionLabel(center, "战斗详情");
            _battleText = CreateText("BattleText", center, string.Empty, 18, FontStyle.Bold, TextAnchor.UpperLeft);
            _battleText.color = Color.white;
            SetLayout(_battleText.gameObject, flexibleWidth: 1, flexibleHeight: 1);

            var right = CreatePanel("DeckPanel", body, new Color(0.07f, 0.095f, 0.085f, 0.96f));
            SetLayout(right.gameObject, preferredWidth: 420, flexibleHeight: 1);
            CreateSectionLabel(right, "牌组 / 行囊");
            _deckText = CreateText("DeckText", right, string.Empty, 15, FontStyle.Normal, TextAnchor.UpperLeft);
            _deckText.color = new Color(0.84f, 0.88f, 0.90f, 1f);
            SetLayout(_deckText.gameObject, flexibleWidth: 1, flexibleHeight: 1);
            CreateSectionLabel(right, "日志");
            _logText = CreateText("LogText", right, string.Empty, 14, FontStyle.Normal, TextAnchor.UpperLeft);
            _logText.color = new Color(0.72f, 0.78f, 0.82f, 1f);
            SetLayout(_logText.gameObject, flexibleWidth: 1, preferredHeight: 170);

            var lower = CreateRect("Lower", transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var lowerLayout = lower.gameObject.AddComponent<VerticalLayoutGroup>();
            lowerLayout.spacing = 10;
            lowerLayout.childForceExpandWidth = true;
            lowerLayout.childForceExpandHeight = false;
            lowerLayout.childControlWidth = true;
            lowerLayout.childControlHeight = true;
            SetLayout(lower.gameObject, flexibleWidth: 1, preferredHeight: 278);

            _choiceRoot = CreateScrollContent("ChoicesScroll", lower, "操作", 124);
            _handRoot = CreateScrollContent("HandScroll", lower, "手牌 / 背包 / 坊市操作", 144);

            var actionBar = CreateRect("ActionBar", lower, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
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

            var view = CultivationRunPrototypePresenter.BuildViewModel(_run);
            var text = view.Text;
            _titleText.text = view.Title;
            _stageText.text = view.PhaseTitle;
            _statusText.text = view.StatusSummary;
            _runText.text = text.RunText;
            _nodeText.text = text.NodeText;
            _battleText.text = text.BattleText;
            _deckText.text = text.DeckText;
            _logText.text = text.LogText;
            _endTurnButton.interactable = _run.Status == CultivationRunStatus.InBattle && _run.CurrentBattle != null && _run.CurrentBattle.Outcome == BattleOutcome.InProgress;

            RebuildStats(view);
            RebuildMap(view);
            RebuildChoices();
            RebuildHand();
        }

        private void RebuildStats(RunPrototypeViewModel view)
        {
            ClearChildren(_statsRoot);

            foreach (var stat in view.Stats)
            {
                var card = CreateInfoCard($"Stat_{stat.Label}", _statsRoot, stat.Label, stat.Value, stat.Note, new Color(0.12f, 0.15f, 0.18f, 1f), new Color(0.96f, 0.86f, 0.56f, 1f));
                SetLayout(card.gameObject, preferredWidth: 156, preferredHeight: 44);
            }
        }

        private void RebuildMap(RunPrototypeViewModel view)
        {
            ClearChildren(_mapRoot);

            foreach (var node in view.MapNodes)
            {
                var color = node.IsCurrent
                    ? new Color(0.48f, 0.30f, 0.12f, 1f)
                    : node.IsChoice
                        ? new Color(0.16f, 0.30f, 0.28f, 1f)
                        : node.IsPast
                            ? new Color(0.10f, 0.16f, 0.18f, 1f)
                            : new Color(0.075f, 0.085f, 0.098f, 1f);
                var note = $"{node.RealmName} / {node.TypeName}";
                var card = CreateInfoCard($"MapNode_{node.Index}_{node.Id}", _mapRoot, $"{node.Index + 1}. {node.Name}", node.IsCurrent ? "当前" : node.IsChoice ? "可选" : node.IsPast ? "已走过" : "未探索", note, color, Color.white);
                SetLayout(card.gameObject, preferredWidth: 176, preferredHeight: 44);
            }
        }

        private void RebuildChoices()
        {
            ClearChildren(_choiceRoot);

            switch (_run.Status)
            {
                case CultivationRunStatus.InBattle:
                    if (_run.CurrentBattle != null && _run.CurrentBattle.Outcome != BattleOutcome.InProgress)
                    {
                        var resolveLabel = _run.CurrentBattle.Outcome == BattleOutcome.Victory ? "结算胜利" : "结算失败";
                        var resolve = CreateButton("ResolveBattleButton", _choiceRoot, resolveLabel, "进入奖励或失败结算");
                        resolve.onClick.AddListener(ResolveBattle);
                        SetLayout(resolve.gameObject, preferredWidth: 220, preferredHeight: 96);
                    }

                    break;
                case CultivationRunStatus.Reward:
                    for (var i = 0; i < _run.CurrentRewards.Count; i++)
                    {
                        var index = i;
                        var reward = _run.CurrentRewards[i];
                        var button = CreateButton($"Reward_{i}_{reward.Id}", _choiceRoot, $"奖励：{reward.Card.Name}", CultivationRunPrototypePresenter.FormatCardSummary(reward.Card));
                        button.onClick.AddListener(() => ChooseReward(index));
                        SetLayout(button.gameObject, preferredWidth: 240, preferredHeight: 96);
                    }

                    var skip = CreateButton("SkipRewardButton", _choiceRoot, "跳过奖励", "保持牌组精简");
                    skip.onClick.AddListener(SkipReward);
                    SetLayout(skip.gameObject, preferredWidth: 180, preferredHeight: 96);
                    break;
                case CultivationRunStatus.Rest:
                    var restOnly = CreateButton("RestOnlyButton", _choiceRoot, "闭关恢复", $"+{_run.CurrentNode.RestHealAmount} HP");
                    restOnly.onClick.AddListener(Rest);
                    SetLayout(restOnly.gameObject, preferredWidth: 220, preferredHeight: 96);
                    foreach (var choice in _run.RestUpgradeChoices)
                    {
                        for (var i = 0; i < choice.SourceCard.UpgradeOptions.Count; i++)
                        {
                            var optionIndex = i;
                            var option = choice.SourceCard.UpgradeOptions[i];
                            var deckIndex = choice.DeckIndex;
                            var button = CreateButton($"Upgrade_{deckIndex}_{optionIndex}", _choiceRoot, $"{choice.SourceCard.Name} → {option.UpgradedCard.Name}", option.Description);
                            button.onClick.AddListener(() => RestAndUpgrade(deckIndex, optionIndex));
                            SetLayout(button.gameObject, preferredWidth: 260, preferredHeight: 96);
                        }
                    }

                    break;
                case CultivationRunStatus.RouteChoice:
                    for (var i = 0; i < _run.CurrentRouteChoices.Count; i++)
                    {
                        var index = i;
                        var choice = _run.CurrentRouteChoices[i];
                        var button = CreateButton($"Route_{i}_{choice.TargetNode.Id}", _choiceRoot, $"前往：{choice.TargetNode.Name}", $"{CultivationRunPrototypePresenter.FormatRealmName(choice.TargetNode.Realm)} / {CultivationRunPrototypePresenter.FormatNodeTypeName(choice.TargetNode.Type)}");
                        button.onClick.AddListener(() => ChooseRoute(index));
                        SetLayout(button.gameObject, preferredWidth: 240, preferredHeight: 96);
                    }

                    break;
                case CultivationRunStatus.Market:
                    for (var i = 0; i < _run.CurrentMarketItems.Count; i++)
                    {
                        var index = i;
                        var item = _run.CurrentMarketItems[i];
                        var detail = item.IsCard
                            ? CultivationRunPrototypePresenter.FormatCardSummary(item.Card)
                            : item.IsPill
                                ? item.Pill.Description
                                : item.Artifact.Description;
                        var button = CreateButton($"Market_{i}_{item.Id}", _choiceRoot, $"购买：{CultivationRunPrototypePresenter.FormatMarketItemName(item)}", $"{item.Price} 灵石\n{detail}");
                        button.interactable = _run.SpiritStones >= item.Price && (!item.IsPill || _run.Pills.Count < _run.PillSlotLimit);
                        button.onClick.AddListener(() => BuyMarketItem(index));
                        SetLayout(button.gameObject, preferredWidth: 260, preferredHeight: 96);
                    }

                    var leave = CreateButton("LeaveMarketButton", _choiceRoot, "离开坊市", "进入下个节点");
                    leave.onClick.AddListener(LeaveMarket);
                    SetLayout(leave.gameObject, preferredWidth: 180, preferredHeight: 96);
                    break;
                case CultivationRunStatus.Chest:
                    var openChest = CreateButton("OpenChestButton", _choiceRoot, "打开宝箱", $"获得 1 件法宝\n法宝池 {_run.CurrentNode.ArtifactRewardPool.Count} 件");
                    openChest.interactable = _run.CurrentNode.ArtifactRewardPool.Count > 0;
                    openChest.onClick.AddListener(OpenChest);
                    SetLayout(openChest.gameObject, preferredWidth: 260, preferredHeight: 96);
                    break;
                case CultivationRunStatus.Mystic:
                    for (var i = 0; i < _run.MysticEventChoices.Count; i++)
                    {
                        var index = i;
                        var option = _run.MysticEventChoices[i];
                        var button = CreateButton($"Mystic_{i}_{option.Id}", _choiceRoot, $"秘境：{option.Name}", option.Description);
                        button.onClick.AddListener(() => ChooseMysticEventOption(index));
                        SetLayout(button.gameObject, preferredWidth: 260, preferredHeight: 96);
                    }

                    break;
                case CultivationRunStatus.GoldenCorePassiveChoice:
                    for (var i = 0; i < _run.CurrentGoldenCorePassiveChoices.Count; i++)
                    {
                        var index = i;
                        var passive = _run.CurrentGoldenCorePassiveChoices[i];
                        var button = CreateButton($"GoldenCorePassive_{i}_{passive.Id}", _choiceRoot, $"金丹被动：{passive.Name}", passive.Description);
                        button.onClick.AddListener(() => ChooseGoldenCorePassive(index));
                        SetLayout(button.gameObject, preferredWidth: 280, preferredHeight: 96);
                    }

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
                    var button = CreateButton($"RemoveDeck_{i}_{card.Id}", _handRoot, $"移除：{card.Name}", $"{CultivationRunEngine.MarketCardRemovalCost} 灵石");
                    button.interactable = _run.Deck.Count > 1 && _run.SpiritStones >= CultivationRunEngine.MarketCardRemovalCost;
                    button.onClick.AddListener(() => RemoveDeckCardAtMarket(index));
                    SetLayout(button.gameObject, preferredWidth: 220, preferredHeight: 130);

                    var sellValue = _runEngine.GetMarketSellValue(_run, index);
                    var sellButton = CreateButton($"SellDeck_{i}_{card.Id}", _handRoot, $"出售：{card.Name}", $"+{sellValue} 灵石");
                    sellButton.interactable = _run.Deck.Count > 1;
                    sellButton.onClick.AddListener(() => SellDeckCardAtMarket(index));
                    SetLayout(sellButton.gameObject, preferredWidth: 220, preferredHeight: 130);

                    for (var optionIndex = 0; optionIndex < card.UpgradeOptions.Count; optionIndex++)
                    {
                        var selectedOptionIndex = optionIndex;
                        var option = card.UpgradeOptions[optionIndex];
                        var upgradeButton = CreateButton($"MarketUpgrade_{i}_{optionIndex}_{option.Id}", _handRoot, $"升级：{card.Name}", $"→ {option.UpgradedCard.Name}\n{CultivationRunEngine.MarketCardUpgradeCost} 灵石");
                        upgradeButton.interactable = _run.SpiritStones >= CultivationRunEngine.MarketCardUpgradeCost;
                        upgradeButton.onClick.AddListener(() => UpgradeDeckCardAtMarket(index, selectedOptionIndex));
                        SetLayout(upgradeButton.gameObject, preferredWidth: 260, preferredHeight: 130);
                    }
                }

                return;
            }

            if (_run.Status != CultivationRunStatus.InBattle || _run.CurrentBattle == null)
            {
                for (var i = 0; i < _run.Pills.Count; i++)
                {
                    var index = i;
                    var pill = _run.Pills[i];
                    var button = CreateButton($"RunPill_{i}_{pill.Id}", _handRoot, $"丹药：{pill.Name}", pill.Description);
                    button.interactable = pill.IsRunEffect;
                    button.onClick.AddListener(() => UsePillInRun(index));
                    SetLayout(button.gameObject, preferredWidth: 240, preferredHeight: 130);
                }

                if (!string.IsNullOrEmpty(_lastRunPillMessage))
                {
                    var message = CreateText("RunPillMessage", _handRoot, _lastRunPillMessage, 15, FontStyle.Bold, TextAnchor.MiddleCenter);
                    message.color = new Color(0.95f, 0.89f, 0.72f, 1f);
                    SetLayout(message.gameObject, preferredWidth: 300, preferredHeight: 130);
                }

                return;
            }

            for (var i = 0; i < _run.Pills.Count; i++)
            {
                var index = i;
                var pill = _run.Pills[i];
                var button = CreateButton($"Pill_{i}_{pill.Id}", _handRoot, $"丹药：{pill.Name}", pill.Description);
                button.interactable = _run.CurrentBattle.Outcome == BattleOutcome.InProgress && pill.EffectValue > 0 && pill.IsBattleEffect;
                button.onClick.AddListener(() => UsePillInBattle(index));
                SetLayout(button.gameObject, preferredWidth: 240, preferredHeight: 130);
            }

            for (var i = 0; i < _run.CurrentBattle.Hand.Count; i++)
            {
                var index = i;
                var card = _run.CurrentBattle.Hand[i];
                var costText = FormatBattleCardCost(_run.CurrentBattle, card);
                var button = CreateButton($"Card_{i}_{card.Id}", _handRoot, card.Name, $"{costText}\n{string.Join("\n", card.Effects.Select(CultivationRunPrototypePresenter.FormatEffect))}");
                button.interactable = _run.CurrentBattle.Outcome == BattleOutcome.InProgress && _battleEngine.CanPlay(_run.CurrentBattle, card);
                button.onClick.AddListener(() => PlayCardAt(index));
                SetLayout(button.gameObject, preferredWidth: 240, preferredHeight: 130);
            }
        }

        private static string FormatBattleCardCost(BattleState battle, CardDefinition card)
        {
            var effectiveCost = battle.GetEffectiveSpiritCost(card);
            if (effectiveCost == card.SpiritCost)
            {
                return $"灵力 {effectiveCost}";
            }

            return $"灵力 {effectiveCost}（原 {card.SpiritCost}）";
        }

        private static string FormatStageLine(CultivationRunState run)
        {
            if (run == null)
            {
                return string.Empty;
            }

            var battle = run.CurrentBattle;
            var outcome = battle == null ? string.Empty : $" / 战斗：{battle.Outcome}";
            return $"{CultivationRunPrototypePresenter.FormatRealmName(run.CurrentRealm)} · 节点 {run.CurrentNodeIndex + 1}/{run.Route.Count} · {run.CurrentNode.Name} · {run.Status}{outcome}";
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

        private static RectTransform CreateRowContent(string name, Transform parent, string label, float height)
        {
            var row = CreateRect(name, parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var rowLayout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 10;
            rowLayout.childForceExpandWidth = false;
            rowLayout.childForceExpandHeight = true;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            SetLayout(row.gameObject, flexibleWidth: 1, preferredHeight: height);

            var labelText = CreateText($"{label}Label", row, label, 14, FontStyle.Bold, TextAnchor.MiddleLeft);
            labelText.color = new Color(0.58f, 0.78f, 0.72f, 1f);
            SetLayout(labelText.gameObject, preferredWidth: 58, flexibleHeight: 1);
            return row;
        }

        private static RectTransform CreateInfoCard(string name, Transform parent, string title, string value, string note, Color backgroundColor, Color valueColor)
        {
            var rect = CreateRect(name, parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = backgroundColor;
            var layout = rect.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(8, 8, 3, 3);
            layout.spacing = 1;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = true;
            layout.childControlHeight = true;

            var titleText = CreateText("Title", rect, title, 11, FontStyle.Bold, TextAnchor.MiddleLeft);
            titleText.color = new Color(0.62f, 0.72f, 0.74f, 1f);
            SetLayout(titleText.gameObject, flexibleWidth: 1, preferredHeight: 12);

            var valueText = CreateText("Value", rect, value, 14, FontStyle.Bold, TextAnchor.MiddleLeft);
            valueText.color = valueColor;
            valueText.resizeTextForBestFit = true;
            valueText.resizeTextMinSize = 10;
            valueText.resizeTextMaxSize = 14;
            SetLayout(valueText.gameObject, flexibleWidth: 1, preferredHeight: 16);

            var noteText = CreateText("Note", rect, note, 10, FontStyle.Normal, TextAnchor.MiddleLeft);
            noteText.color = new Color(0.66f, 0.70f, 0.72f, 1f);
            noteText.resizeTextForBestFit = true;
            noteText.resizeTextMinSize = 8;
            noteText.resizeTextMaxSize = 10;
            SetLayout(noteText.gameObject, flexibleWidth: 1, preferredHeight: 12);
            return rect;
        }

        private static void CreateSectionLabel(Transform parent, string label)
        {
            var text = CreateText($"{label}Label", parent, label, 14, FontStyle.Bold, TextAnchor.MiddleLeft);
            text.color = new Color(0.58f, 0.78f, 0.72f, 1f);
            SetLayout(text.gameObject, flexibleWidth: 1, preferredHeight: 22);
        }

        private static RectTransform CreateScrollContent(string name, Transform parent, string label, float height)
        {
            var wrapper = CreatePanel($"{name}Panel", parent, new Color(0.075f, 0.085f, 0.098f, 0.97f));
            var wrapperLayout = wrapper.GetComponent<VerticalLayoutGroup>();
            wrapperLayout.padding = new RectOffset(14, 14, 8, 8);
            wrapperLayout.spacing = 6;
            SetLayout(wrapper.gameObject, flexibleWidth: 1, preferredHeight: height);

            CreateSectionLabel(wrapper, label);

            var viewport = CreateRect("Viewport", wrapper, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var viewportImage = viewport.gameObject.AddComponent<Image>();
            viewportImage.color = new Color(0.025f, 0.03f, 0.036f, 0.70f);
            var mask = viewport.gameObject.AddComponent<Mask>();
            mask.showMaskGraphic = false;
            SetLayout(viewport.gameObject, flexibleWidth: 1, flexibleHeight: 1);

            var content = CreateRect("Content", viewport, new Vector2(0f, 0f), new Vector2(0f, 1f), Vector2.zero, Vector2.zero);
            content.pivot = new Vector2(0f, 0.5f);
            var contentLayout = content.gameObject.AddComponent<HorizontalLayoutGroup>();
            contentLayout.padding = new RectOffset(8, 8, 8, 8);
            contentLayout.spacing = 10;
            contentLayout.childForceExpandWidth = false;
            contentLayout.childForceExpandHeight = true;
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = true;
            var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

            var scroll = wrapper.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = true;
            scroll.vertical = false;
            scroll.viewport = viewport;
            scroll.content = content;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 28f;

            return content;
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
            return CreateButton(name, parent, label, string.Empty);
        }

        private static Button CreateButton(string name, Transform parent, string label, string detail)
        {
            var rect = CreateRect(name, parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = new Color(0.17f, 0.20f, 0.23f, 1f);
            var button = rect.gameObject.AddComponent<Button>();
            var colors = button.colors;
            colors.normalColor = image.color;
            colors.highlightedColor = new Color(0.27f, 0.33f, 0.36f, 1f);
            colors.pressedColor = new Color(0.10f, 0.13f, 0.15f, 1f);
            colors.disabledColor = new Color(0.09f, 0.10f, 0.11f, 0.75f);
            button.colors = colors;

            var layout = rect.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(10, 10, 8, 8);
            layout.spacing = 4;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = true;
            layout.childControlHeight = true;

            var text = CreateText("Label", rect, label, 15, FontStyle.Bold, TextAnchor.MiddleCenter);
            text.color = Color.white;
            text.raycastTarget = false;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 10;
            text.resizeTextMaxSize = 15;
            SetLayout(text.gameObject, flexibleWidth: 1, preferredHeight: 34);

            var detailText = CreateText("Detail", rect, detail, 12, FontStyle.Normal, TextAnchor.UpperCenter);
            detailText.color = new Color(0.78f, 0.84f, 0.86f, 1f);
            detailText.raycastTarget = false;
            detailText.resizeTextForBestFit = true;
            detailText.resizeTextMinSize = 8;
            detailText.resizeTextMaxSize = 12;
            SetLayout(detailText.gameObject, flexibleWidth: 1, flexibleHeight: 1);
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
