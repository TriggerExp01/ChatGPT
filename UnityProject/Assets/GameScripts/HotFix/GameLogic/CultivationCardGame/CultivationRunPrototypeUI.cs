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

        private CultivationRunSession _session;
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
        private Button _swordSectButton;
        private Button _fireCloudSectButton;
        private Button _thunderSectButton;
        private GameObject _infoCardTemplate;
        private GameObject _statCardTemplate;
        private GameObject _mapNodeTemplate;
        private GameObject _actionButtonTemplate;
        private GameObject _rewardCardTemplate;
        private GameObject _routeChoiceTemplate;
        private GameObject _restChoiceTemplate;
        private GameObject _marketItemTemplate;
        private GameObject _chestRewardTemplate;
        private GameObject _mysticEventTemplate;
        private GameObject _goldenCorePassiveTemplate;
        private GameObject _battleCardTemplate;
        private GameObject _pillItemTemplate;
        private GameObject _marketDeckActionTemplate;
        private GameObject _messageTextTemplate;

        private static bool _visualAssetsLoaded;
        private static Sprite _combatBackgroundSprite;
        private static Sprite _darkPanelSprite;
        private static Sprite _goldPanelSprite;
        private static Sprite _buttonNormalSprite;
        private static Sprite _buttonHoverSprite;
        private static Sprite _buttonPressedSprite;
        private static Sprite _buttonDisabledSprite;
        private static Sprite _cardFrameCommonSprite;
        private static Sprite _cardFrameSpiritSprite;

        private static readonly Color ThemeRootInk = new Color(0.026f, 0.032f, 0.030f, 0.99f);
        private static readonly Color ThemeHeaderInk = new Color(0.070f, 0.088f, 0.082f, 0.98f);
        private static readonly Color ThemeRunPanelInk = new Color(0.060f, 0.083f, 0.085f, 0.97f);
        private static readonly Color ThemeBattlePanelInk = new Color(0.115f, 0.070f, 0.055f, 0.97f);
        private static readonly Color ThemeDeckPanelInk = new Color(0.052f, 0.080f, 0.067f, 0.97f);
        private static readonly Color ThemeScrollPanelInk = new Color(0.056f, 0.070f, 0.066f, 0.97f);
        private static readonly Color ThemeViewportInk = new Color(0.018f, 0.024f, 0.024f, 0.72f);
        private static readonly Color ThemeCardInk = new Color(0.100f, 0.128f, 0.122f, 1f);
        private static readonly Color ThemeButtonInk = new Color(0.175f, 0.145f, 0.090f, 1f);
        private static readonly Color ThemeJade = new Color(0.44f, 0.78f, 0.68f, 1f);
        private static readonly Color ThemeGold = new Color(0.96f, 0.80f, 0.46f, 1f);
        private static readonly Color ThemePanelOutline = new Color(0.28f, 0.46f, 0.38f, 0.40f);
        private static readonly Color ThemeGoldOutline = new Color(0.78f, 0.55f, 0.24f, 0.55f);
        private static readonly Color ThemeShadow = new Color(0f, 0f, 0f, 0.52f);
        private static readonly Color ThemeTextMain = new Color(0.93f, 0.96f, 0.90f, 1f);
        private static readonly Color ThemeTextMuted = new Color(0.72f, 0.80f, 0.78f, 1f);
        private static readonly Color ThemeRewardButtonInk = new Color(0.190f, 0.145f, 0.088f, 1f);
        private static readonly Color ThemeRouteButtonInk = new Color(0.118f, 0.205f, 0.185f, 1f);
        private static readonly Color ThemeRestButtonInk = new Color(0.135f, 0.190f, 0.145f, 1f);
        private static readonly Color ThemeMarketButtonInk = new Color(0.170f, 0.128f, 0.080f, 1f);
        private static readonly Color ThemeChestButtonInk = new Color(0.205f, 0.160f, 0.080f, 1f);
        private static readonly Color ThemeMysticButtonInk = new Color(0.120f, 0.105f, 0.185f, 1f);
        private static readonly Color ThemeGoldenCoreButtonInk = new Color(0.235f, 0.185f, 0.070f, 1f);
        private static readonly Color ThemeBattleCardButtonInk = new Color(0.155f, 0.095f, 0.075f, 1f);
        private static readonly Color ThemePillButtonInk = new Color(0.095f, 0.165f, 0.128f, 1f);
        private static readonly Color ThemeMarketDeckButtonInk = new Color(0.135f, 0.115f, 0.095f, 1f);

        private enum InfoTemplateKind
        {
            Stat,
            MapNode,
        }

        private enum ActionTemplateKind
        {
            Generic,
            RewardCard,
            RouteChoice,
            RestChoice,
            MarketItem,
            ChestReward,
            MysticEvent,
            GoldenCorePassive,
            BattleCard,
            PillItem,
            MarketDeckAction,
        }

        private enum PanelVisualKind
        {
            Dark,
            Gold,
        }

        public RunPrototypeSnapshot Snapshot => CultivationRunPrototypePresenter.CreateSnapshot(_run);

        public CultivationRunState DebugRunState => _run;

        public CultivationRunSession DebugSession => _session;

        public static CultivationRunPrototypeUI Open(Transform parent = null)
        {
            var uiParent = parent != null ? parent : ResolveParent();
            var old = uiParent.Find(RootName);
            RectTransform root;
            if (old != null)
            {
                var existing = old.GetComponent<CultivationRunPrototypeUI>();
                if (existing != null)
                {
                    existing.Initialize();
                    existing.EnsureRun();
                    return existing;
                }

                root = old as RectTransform ?? old.gameObject.AddComponent<RectTransform>();
                SetStretch(root);
            }
            else
            {
                root = CreateRect(RootName, uiParent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            }

            EnsureEventSystem();
            var view = root.gameObject.AddComponent<CultivationRunPrototypeUI>();
            view.Initialize();
            view.EnsureRun();
            return view;
        }

        public void ResetRun()
        {
            ResetRun(CultivationSect.Sword);
        }

        public void ResetRun(CultivationSect sect)
        {
            _session = new CultivationRunSession();
            _run = _session.StartNewRun(sect);
            _run.CurrentBattle.Logs.Add(new BattleLogEntry("Phase 7 Run 原型界面已连接战斗、奖励、闭关升级和路线选择。"));
            Refresh();
        }

        public CultivationRunAutoPlayReport RunFixedSeedAutoPlay(CultivationSect sect = CultivationSect.Sword, int maxSteps = CultivationRunAutoPlayer.DefaultMaxSteps)
        {
            ResetRun(sect);
            var report = new CultivationRunAutoPlayer().Run(this, maxSteps);
            Refresh();
            return report;
        }

        public void PlayCardAt(int handIndex)
        {
            if (_session == null)
            {
                return;
            }

            _session.PlayCardAt(handIndex);
            Refresh();
        }

        public void EndTurn()
        {
            if (_session == null)
            {
                return;
            }

            _session.EndTurn();
            Refresh();
        }

        public void UsePillInBattle(int pillIndex)
        {
            if (_session == null)
            {
                return;
            }

            _session.UsePillInBattle(pillIndex);
            Refresh();
        }

        public void UsePillInRun(int pillIndex)
        {
            if (_session == null)
            {
                return;
            }

            _lastRunPillMessage = _session.UsePillInRun(pillIndex);
            Refresh();
        }

        public void ResolveBattle()
        {
            if (_session == null)
            {
                return;
            }

            _session.ResolveBattle();
            Refresh();
        }

        public void ChooseReward(int rewardIndex)
        {
            if (_session == null)
            {
                return;
            }

            _session.ChooseReward(rewardIndex);
            Refresh();
        }

        public void SkipReward()
        {
            if (_session == null)
            {
                return;
            }

            _session.SkipReward();
            Refresh();
        }

        public void Rest()
        {
            if (_session == null)
            {
                return;
            }

            _session.Rest();
            Refresh();
        }

        public void RestAndUpgrade(int deckIndex, int upgradeOptionIndex)
        {
            if (_session == null)
            {
                return;
            }

            _session.RestAndUpgrade(deckIndex, upgradeOptionIndex);
            Refresh();
        }

        public void ChooseRoute(int choiceIndex)
        {
            if (_session == null)
            {
                return;
            }

            _session.ChooseRoute(choiceIndex);
            Refresh();
        }

        public void BuyMarketItem(int itemIndex)
        {
            if (_session == null)
            {
                return;
            }

            _session.BuyMarketItem(itemIndex);
            Refresh();
        }

        public void RemoveDeckCardAtMarket(int deckIndex)
        {
            if (_session == null)
            {
                return;
            }

            _session.RemoveDeckCardAtMarket(deckIndex);
            Refresh();
        }

        public void SellDeckCardAtMarket(int deckIndex)
        {
            if (_session == null)
            {
                return;
            }

            _session.SellDeckCardAtMarket(deckIndex);
            Refresh();
        }

        public void UpgradeDeckCardAtMarket(int deckIndex, int upgradeOptionIndex)
        {
            if (_session == null)
            {
                return;
            }

            _session.UpgradeDeckCardAtMarket(deckIndex, upgradeOptionIndex);
            Refresh();
        }

        public void LeaveMarket()
        {
            if (_session == null)
            {
                return;
            }

            _session.LeaveMarket();
            Refresh();
        }

        public void OpenChest()
        {
            if (_session == null)
            {
                return;
            }

            _session.OpenChest();
            Refresh();
        }

        public void ChooseMysticEventOption(int optionIndex)
        {
            if (_session == null)
            {
                return;
            }

            _session.ChooseMysticEventOption(optionIndex);
            Refresh();
        }

        public void ChooseGoldenCorePassive(int passiveIndex)
        {
            if (_session == null)
            {
                return;
            }

            _session.ChooseGoldenCorePassive(passiveIndex);
            Refresh();
        }

        private void EnsureRun()
        {
            if (_session == null || _run == null)
            {
                ResetRun();
            }
        }

        private void Initialize()
        {
            if (_handRoot == null)
            {
                BuildView();
            }

            BindTemplates();
        }

        private void BuildView()
        {
            EnsureVisualAssetsLoaded();

            var background = GetOrAdd<Image>(gameObject);
            background.color = ThemeRootInk;
            ApplySprite(background, _combatBackgroundSprite, Image.Type.Sliced);
            ApplyGraphicChrome(gameObject, new Color(0.18f, 0.27f, 0.23f, 0.34f), ThemeShadow, new Vector2(2f, -2f));

            var rootLayout = GetOrAdd<VerticalLayoutGroup>(gameObject);
            rootLayout.padding = new RectOffset(24, 24, 20, 20);
            rootLayout.spacing = 12;
            rootLayout.childForceExpandWidth = true;
            rootLayout.childForceExpandHeight = false;
            rootLayout.childControlWidth = true;
            rootLayout.childControlHeight = true;

            var header = CreatePanel("Header", transform, ThemeHeaderInk);
            var headerLayout = header.GetComponent<VerticalLayoutGroup>();
            headerLayout.padding = new RectOffset(20, 20, 12, 12);
            ApplyPanelSprite(header.GetComponent<Image>(), PanelVisualKind.Gold);
            ApplyGraphicChrome(header.gameObject, ThemeGoldOutline, ThemeShadow, new Vector2(3f, -3f));
            SetLayout(header.gameObject, flexibleWidth: 1, preferredHeight: 172);

            var titleRow = CreateRect("TitleRow", header, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var titleRowLayout = GetOrAdd<HorizontalLayoutGroup>(titleRow.gameObject);
            titleRowLayout.spacing = 12;
            titleRowLayout.childForceExpandWidth = true;
            titleRowLayout.childForceExpandHeight = true;
            titleRowLayout.childControlWidth = true;
            titleRowLayout.childControlHeight = true;
            SetLayout(titleRow.gameObject, flexibleWidth: 1, preferredHeight: 56);

            var titleBox = CreateRect("TitleBox", titleRow, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var titleBoxLayout = GetOrAdd<VerticalLayoutGroup>(titleBox.gameObject);
            titleBoxLayout.spacing = 4;
            titleBoxLayout.childForceExpandWidth = true;
            titleBoxLayout.childForceExpandHeight = false;
            titleBoxLayout.childControlWidth = true;
            titleBoxLayout.childControlHeight = true;
            SetLayout(titleBox.gameObject, flexibleWidth: 1, flexibleHeight: 1);

            _titleText = CreateText("Title", titleBox, "仙途·天命", 28, FontStyle.Bold, TextAnchor.MiddleLeft);
            _titleText.color = ThemeGold;
            ApplyTextChrome(_titleText, new Color(0.82f, 0.58f, 0.22f, 0.45f), new Vector2(1.2f, -1.2f));
            SetLayout(_titleText.gameObject, flexibleWidth: 1, preferredHeight: 30);

            _stageText = CreateText("StageText", titleBox, string.Empty, 15, FontStyle.Normal, TextAnchor.MiddleLeft);
            _stageText.color = ThemeJade;
            SetLayout(_stageText.gameObject, flexibleWidth: 1, preferredHeight: 22);

            _statusText = CreateText("StatusText", titleRow, string.Empty, 18, FontStyle.Bold, TextAnchor.MiddleRight);
            _statusText.color = ThemeGold;
            ApplyTextChrome(_statusText, new Color(0.82f, 0.58f, 0.22f, 0.42f), new Vector2(1.2f, -1.2f));
            SetLayout(_statusText.gameObject, preferredWidth: 420, flexibleHeight: 1);

            _statsRoot = CreateRowContent("StatsBar", header, "资源", 52);
            _mapRoot = CreateRowContent("MapBar", header, "路线图", 52);

            var body = CreateRect("Body", transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var bodyLayout = GetOrAdd<HorizontalLayoutGroup>(body.gameObject);
            bodyLayout.spacing = 12;
            bodyLayout.childForceExpandWidth = true;
            bodyLayout.childForceExpandHeight = true;
            bodyLayout.childControlWidth = true;
            bodyLayout.childControlHeight = true;
            SetLayout(body.gameObject, flexibleWidth: 1, flexibleHeight: 1, preferredHeight: 520);

            var left = CreatePanel("RunPanel", body, ThemeRunPanelInk);
            SetLayout(left.gameObject, preferredWidth: 350, flexibleHeight: 1);
            CreateSectionLabel(left, "运行总览");
            _runText = CreateText("RunText", left, string.Empty, 16, FontStyle.Bold, TextAnchor.UpperLeft);
            _runText.color = ThemeTextMain;
            ConfigureBodyText(_runText, 11, 15);
            SetLayout(_runText.gameObject, flexibleWidth: 1, preferredHeight: 210);
            CreateSectionLabel(left, "当前节点");
            _nodeText = CreateText("NodeText", left, string.Empty, 15, FontStyle.Normal, TextAnchor.UpperLeft);
            _nodeText.color = new Color(0.80f, 0.88f, 0.84f, 1f);
            ConfigureBodyText(_nodeText, 10, 14);
            SetLayout(_nodeText.gameObject, flexibleWidth: 1, flexibleHeight: 1);

            var center = CreatePanel("BattlePanel", body, ThemeBattlePanelInk);
            ApplyPanelSprite(center.GetComponent<Image>(), PanelVisualKind.Gold);
            ApplyGraphicChrome(center.gameObject, ThemeGoldOutline, ThemeShadow, new Vector2(3f, -3f));
            SetLayout(center.gameObject, flexibleWidth: 1.45f, flexibleHeight: 1);
            CreateSectionLabel(center, "战斗详情");
            _battleText = CreateText("BattleText", center, string.Empty, 18, FontStyle.Bold, TextAnchor.UpperLeft);
            _battleText.color = ThemeTextMain;
            ConfigureBodyText(_battleText, 12, 17);
            SetLayout(_battleText.gameObject, flexibleWidth: 1, flexibleHeight: 1);

            var right = CreatePanel("DeckPanel", body, ThemeDeckPanelInk);
            SetLayout(right.gameObject, preferredWidth: 420, flexibleHeight: 1);
            CreateSectionLabel(right, "牌组 / 行囊");
            _deckText = CreateText("DeckText", right, string.Empty, 15, FontStyle.Normal, TextAnchor.UpperLeft);
            _deckText.color = new Color(0.84f, 0.89f, 0.86f, 1f);
            ConfigureBodyText(_deckText, 10, 14);
            SetLayout(_deckText.gameObject, flexibleWidth: 1, flexibleHeight: 1);
            CreateSectionLabel(right, "日志");
            _logText = CreateText("LogText", right, string.Empty, 14, FontStyle.Normal, TextAnchor.UpperLeft);
            _logText.color = ThemeTextMuted;
            ConfigureBodyText(_logText, 10, 13);
            SetLayout(_logText.gameObject, flexibleWidth: 1, preferredHeight: 170);

            var lower = CreateRect("Lower", transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var lowerLayout = GetOrAdd<VerticalLayoutGroup>(lower.gameObject);
            lowerLayout.spacing = 10;
            lowerLayout.childForceExpandWidth = true;
            lowerLayout.childForceExpandHeight = false;
            lowerLayout.childControlWidth = true;
            lowerLayout.childControlHeight = true;
            SetLayout(lower.gameObject, flexibleWidth: 1, preferredHeight: 278);

            _choiceRoot = CreateScrollContent("ChoicesScroll", lower, "操作", 124);
            _handRoot = CreateScrollContent("HandScroll", lower, "手牌 / 背包 / 坊市操作", 144);

            var actionBar = CreateRect("ActionBar", lower, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var actionLayout = GetOrAdd<HorizontalLayoutGroup>(actionBar.gameObject);
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

            _swordSectButton = CreateButton("SwordSectButton", actionBar, "剑宗开局");
            _swordSectButton.onClick.AddListener(() => ResetRun(CultivationSect.Sword));
            SetLayout(_swordSectButton.gameObject, preferredWidth: 170, preferredHeight: 48);

            _fireCloudSectButton = CreateButton("FireCloudSectButton", actionBar, "火云宗开局");
            _fireCloudSectButton.onClick.AddListener(() => ResetRun(CultivationSect.FireCloud));
            SetLayout(_fireCloudSectButton.gameObject, preferredWidth: 190, preferredHeight: 48);

            _thunderSectButton = CreateButton("ThunderSectButton", actionBar, "天雷阁开局");
            _thunderSectButton.onClick.AddListener(() => ResetRun(CultivationSect.Thunder));
            SetLayout(_thunderSectButton.gameObject, preferredWidth: 190, preferredHeight: 48);

            var earthSectButton = CreateButton("EarthSectButton", actionBar, "玄黄宗开局");
            earthSectButton.onClick.AddListener(() => ResetRun(CultivationSect.Earth));
            SetLayout(earthSectButton.gameObject, preferredWidth: 190, preferredHeight: 48);

            var medicineSectButton = CreateButton("MedicineSectButton", actionBar, "药王谷开局");
            medicineSectButton.onClick.AddListener(() => ResetRun(CultivationSect.Medicine));
            SetLayout(medicineSectButton.gameObject, preferredWidth: 190, preferredHeight: 48);

            var demonicSectButton = CreateButton("DemonicSectButton", actionBar, "魔道开局");
            demonicSectButton.onClick.AddListener(() => ResetRun(CultivationSect.Demonic));
            SetLayout(demonicSectButton.gameObject, preferredWidth: 170, preferredHeight: 48);

            _endTurnButton = CreateButton("EndTurnButton", actionBar, "结束回合");
            _endTurnButton.onClick.AddListener(EndTurn);
            SetLayout(_endTurnButton.gameObject, preferredWidth: 170, preferredHeight: 48);

            BindTemplates();
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
                var card = CreateInfoCardItem($"Stat_{stat.Label}", _statsRoot, stat.Label, stat.Value, stat.Note, ThemeCardInk, ThemeGold, InfoTemplateKind.Stat);
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
                var card = CreateInfoCardItem($"MapNode_{node.Index}_{node.Id}", _mapRoot, $"{node.Index + 1}. {node.Name}", node.IsCurrent ? "当前" : node.IsChoice ? "可选" : node.IsPast ? "已走过" : "未探索", note, color, Color.white, InfoTemplateKind.MapNode);
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
                        var resolve = CreateActionButton("ResolveBattleButton", _choiceRoot, resolveLabel, "进入奖励或失败结算", ActionTemplateKind.Generic);
                        resolve.onClick.AddListener(ResolveBattle);
                        SetLayout(resolve.gameObject, preferredWidth: 220, preferredHeight: 96);
                    }

                    break;
                case CultivationRunStatus.Reward:
                    for (var i = 0; i < _run.CurrentRewards.Count; i++)
                    {
                        var index = i;
                        var reward = _run.CurrentRewards[i];
                        var button = CreateActionButton($"Reward_{i}_{reward.Id}", _choiceRoot, $"奖励：{reward.Card.Name}", CultivationRunPrototypePresenter.FormatCardSummary(reward.Card), ActionTemplateKind.RewardCard);
                        button.onClick.AddListener(() => ChooseReward(index));
                        SetLayout(button.gameObject, preferredWidth: 240, preferredHeight: 96);
                    }

                    var skip = CreateActionButton("SkipRewardButton", _choiceRoot, "跳过奖励", "保持牌组精简", ActionTemplateKind.RewardCard);
                    skip.onClick.AddListener(SkipReward);
                    SetLayout(skip.gameObject, preferredWidth: 180, preferredHeight: 96);
                    break;
                case CultivationRunStatus.Rest:
                    var restOnly = CreateActionButton("RestOnlyButton", _choiceRoot, "闭关恢复", $"+{_run.CurrentNode.RestHealAmount} HP", ActionTemplateKind.RestChoice);
                    restOnly.onClick.AddListener(Rest);
                    SetLayout(restOnly.gameObject, preferredWidth: 220, preferredHeight: 96);
                    foreach (var choice in _run.RestUpgradeChoices)
                    {
                        for (var i = 0; i < choice.SourceCard.UpgradeOptions.Count; i++)
                        {
                            var optionIndex = i;
                            var option = choice.SourceCard.UpgradeOptions[i];
                            var deckIndex = choice.DeckIndex;
                            var button = CreateActionButton($"Upgrade_{deckIndex}_{optionIndex}", _choiceRoot, $"{choice.SourceCard.Name} → {option.UpgradedCard.Name}", option.Description, ActionTemplateKind.RestChoice);
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
                        var button = CreateActionButton($"Route_{i}_{choice.TargetNode.Id}", _choiceRoot, $"前往：{choice.TargetNode.Name}", $"{CultivationRunPrototypePresenter.FormatRealmName(choice.TargetNode.Realm)} / {CultivationRunPrototypePresenter.FormatNodeTypeName(choice.TargetNode.Type)}", ActionTemplateKind.RouteChoice);
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
                        var button = CreateActionButton($"Market_{i}_{item.Id}", _choiceRoot, $"购买：{CultivationRunPrototypePresenter.FormatMarketItemName(item)}", $"{item.Price} 灵石\n{detail}", ActionTemplateKind.MarketItem);
                        button.interactable = _run.SpiritStones >= item.Price && (!item.IsPill || _run.Pills.Count < _run.PillSlotLimit);
                        button.onClick.AddListener(() => BuyMarketItem(index));
                        SetLayout(button.gameObject, preferredWidth: 260, preferredHeight: 96);
                    }

                    var leave = CreateActionButton("LeaveMarketButton", _choiceRoot, "离开坊市", "进入下个节点", ActionTemplateKind.MarketItem);
                    leave.onClick.AddListener(LeaveMarket);
                    SetLayout(leave.gameObject, preferredWidth: 180, preferredHeight: 96);
                    break;
                case CultivationRunStatus.Chest:
                    var openChest = CreateActionButton("OpenChestButton", _choiceRoot, "打开宝箱", $"获得 1 件法宝\n法宝池 {_run.CurrentNode.ArtifactRewardPool.Count} 件", ActionTemplateKind.ChestReward);
                    openChest.interactable = _run.CurrentNode.ArtifactRewardPool.Count > 0;
                    openChest.onClick.AddListener(OpenChest);
                    SetLayout(openChest.gameObject, preferredWidth: 260, preferredHeight: 96);
                    break;
                case CultivationRunStatus.Mystic:
                    for (var i = 0; i < _run.MysticEventChoices.Count; i++)
                    {
                        var index = i;
                        var option = _run.MysticEventChoices[i];
                        var button = CreateActionButton($"Mystic_{i}_{option.Id}", _choiceRoot, $"秘境：{option.Name}", CultivationRunPrototypePresenter.FormatMysticEventOptionSummary(_run, option), ActionTemplateKind.MysticEvent);
                        button.onClick.AddListener(() => ChooseMysticEventOption(index));
                        SetLayout(button.gameObject, preferredWidth: 260, preferredHeight: 96);
                    }

                    break;
                case CultivationRunStatus.GoldenCorePassiveChoice:
                    for (var i = 0; i < _run.CurrentGoldenCorePassiveChoices.Count; i++)
                    {
                        var index = i;
                        var passive = _run.CurrentGoldenCorePassiveChoices[i];
                        var button = CreateActionButton($"GoldenCorePassive_{i}_{passive.Id}", _choiceRoot, $"金丹被动：{passive.Name}", passive.Description, ActionTemplateKind.GoldenCorePassive);
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
                    var button = CreateActionButton($"RemoveDeck_{i}_{card.Id}", _handRoot, $"移除：{card.Name}", $"{CultivationRunEngine.MarketCardRemovalCost} 灵石", ActionTemplateKind.MarketDeckAction);
                    button.interactable = _run.Deck.Count > 1 && _run.SpiritStones >= CultivationRunEngine.MarketCardRemovalCost;
                    button.onClick.AddListener(() => RemoveDeckCardAtMarket(index));
                    SetLayout(button.gameObject, preferredWidth: 220, preferredHeight: 130);

                    var sellValue = _session.GetMarketSellValue(index);
                    var sellButton = CreateActionButton($"SellDeck_{i}_{card.Id}", _handRoot, $"出售：{card.Name}", $"+{sellValue} 灵石", ActionTemplateKind.MarketDeckAction);
                    sellButton.interactable = _run.Deck.Count > 1;
                    sellButton.onClick.AddListener(() => SellDeckCardAtMarket(index));
                    SetLayout(sellButton.gameObject, preferredWidth: 220, preferredHeight: 130);

                    for (var optionIndex = 0; optionIndex < card.UpgradeOptions.Count; optionIndex++)
                    {
                        var selectedOptionIndex = optionIndex;
                        var option = card.UpgradeOptions[optionIndex];
                        var upgradeButton = CreateActionButton($"MarketUpgrade_{i}_{optionIndex}_{option.Id}", _handRoot, $"升级：{card.Name}", $"→ {option.UpgradedCard.Name}\n{CultivationRunEngine.MarketCardUpgradeCost} 灵石", ActionTemplateKind.MarketDeckAction);
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
                    var button = CreateActionButton($"RunPill_{i}_{pill.Id}", _handRoot, $"丹药：{pill.Name}", pill.Description, ActionTemplateKind.PillItem);
                    button.interactable = pill.IsRunEffect;
                    button.onClick.AddListener(() => UsePillInRun(index));
                    SetLayout(button.gameObject, preferredWidth: 240, preferredHeight: 130);
                }

                if (!string.IsNullOrEmpty(_lastRunPillMessage))
                {
                    var message = CreateMessageText("RunPillMessage", _handRoot, _lastRunPillMessage, 15, FontStyle.Bold, TextAnchor.MiddleCenter);
                    message.color = new Color(0.95f, 0.89f, 0.72f, 1f);
                    SetLayout(message.gameObject, preferredWidth: 300, preferredHeight: 130);
                }

                return;
            }

            for (var i = 0; i < _run.Pills.Count; i++)
            {
                var index = i;
                var pill = _run.Pills[i];
                var button = CreateActionButton($"Pill_{i}_{pill.Id}", _handRoot, $"丹药：{pill.Name}", pill.Description, ActionTemplateKind.PillItem);
                button.interactable = _run.CurrentBattle.Outcome == BattleOutcome.InProgress && pill.EffectValue > 0 && pill.IsBattleEffect;
                button.onClick.AddListener(() => UsePillInBattle(index));
                SetLayout(button.gameObject, preferredWidth: 240, preferredHeight: 130);
            }

            for (var i = 0; i < _run.CurrentBattle.Hand.Count; i++)
            {
                var index = i;
                var card = _run.CurrentBattle.Hand[i];
                var costText = FormatBattleCardCost(_run.CurrentBattle, card);
                var button = CreateActionButton($"Card_{i}_{card.Id}", _handRoot, card.Name, $"{costText}\n{string.Join("\n", card.Effects.Select(CultivationRunPrototypePresenter.FormatEffect))}", ActionTemplateKind.BattleCard);
                button.interactable = _run.CurrentBattle.Outcome == BattleOutcome.InProgress && _session.CanPlayCardAt(index);
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

        private void BindTemplates()
        {
            var templateRoot = transform.parent != null
                ? transform.parent.Find("PrefabAnchors/UIItemTemplates")
                : null;
            EnsureSemanticTemplates(templateRoot);
            _infoCardTemplate = FindTemplate(templateRoot, "InfoCardTemplate");
            _statCardTemplate = FindTemplate(templateRoot, "StatCardTemplate");
            _mapNodeTemplate = FindTemplate(templateRoot, "MapNodeTemplate");
            _actionButtonTemplate = FindTemplate(templateRoot, "ActionButtonTemplate");
            _rewardCardTemplate = FindTemplate(templateRoot, "RewardCardTemplate");
            _routeChoiceTemplate = FindTemplate(templateRoot, "RouteChoiceTemplate");
            _restChoiceTemplate = FindTemplate(templateRoot, "RestChoiceTemplate");
            _marketItemTemplate = FindTemplate(templateRoot, "MarketItemTemplate");
            _chestRewardTemplate = FindTemplate(templateRoot, "ChestRewardTemplate");
            _mysticEventTemplate = FindTemplate(templateRoot, "MysticEventTemplate");
            _goldenCorePassiveTemplate = FindTemplate(templateRoot, "GoldenCorePassiveTemplate");
            _battleCardTemplate = FindTemplate(templateRoot, "BattleCardTemplate");
            _pillItemTemplate = FindTemplate(templateRoot, "PillItemTemplate");
            _marketDeckActionTemplate = FindTemplate(templateRoot, "MarketDeckActionTemplate");
            _messageTextTemplate = FindTemplate(templateRoot, "MessageTextTemplate");
        }

        private static GameObject FindTemplate(Transform templateRoot, string templateName)
        {
            var template = templateRoot != null ? templateRoot.Find(templateName) : null;
            return template != null ? template.gameObject : null;
        }

        private RectTransform CreateInfoCardItem(string name, Transform parent, string title, string value, string note, Color backgroundColor, Color valueColor, InfoTemplateKind templateKind)
        {
            InstantiateTemplate(ResolveInfoTemplate(templateKind), parent, name);
            return CreateInfoCard(name, parent, title, value, note, backgroundColor, valueColor);
        }

        private Button CreateActionButton(string name, Transform parent, string label, string detail, ActionTemplateKind templateKind)
        {
            InstantiateTemplate(ResolveActionTemplate(templateKind), parent, name);
            var button = CreateButton(name, parent, label, detail);
            ApplyActionButtonStyle(button, templateKind);
            return button;
        }

        private Text CreateMessageText(string name, Transform parent, string value, int fontSize, FontStyle style, TextAnchor anchor)
        {
            InstantiateTemplate(_messageTextTemplate, parent, name);
            return CreateText(name, parent, value, fontSize, style, anchor);
        }

        private GameObject ResolveInfoTemplate(InfoTemplateKind templateKind)
        {
            switch (templateKind)
            {
                case InfoTemplateKind.Stat:
                    return _statCardTemplate != null ? _statCardTemplate : _infoCardTemplate;
                case InfoTemplateKind.MapNode:
                    return _mapNodeTemplate != null ? _mapNodeTemplate : _infoCardTemplate;
                default:
                    return _infoCardTemplate;
            }
        }

        private GameObject ResolveActionTemplate(ActionTemplateKind templateKind)
        {
            switch (templateKind)
            {
                case ActionTemplateKind.RewardCard:
                    return _rewardCardTemplate != null ? _rewardCardTemplate : _actionButtonTemplate;
                case ActionTemplateKind.RouteChoice:
                    return _routeChoiceTemplate != null ? _routeChoiceTemplate : _actionButtonTemplate;
                case ActionTemplateKind.RestChoice:
                    return _restChoiceTemplate != null ? _restChoiceTemplate : _actionButtonTemplate;
                case ActionTemplateKind.MarketItem:
                    return _marketItemTemplate != null ? _marketItemTemplate : _actionButtonTemplate;
                case ActionTemplateKind.ChestReward:
                    return _chestRewardTemplate != null ? _chestRewardTemplate : _actionButtonTemplate;
                case ActionTemplateKind.MysticEvent:
                    return _mysticEventTemplate != null ? _mysticEventTemplate : _actionButtonTemplate;
                case ActionTemplateKind.GoldenCorePassive:
                    return _goldenCorePassiveTemplate != null ? _goldenCorePassiveTemplate : _actionButtonTemplate;
                case ActionTemplateKind.BattleCard:
                    return _battleCardTemplate != null ? _battleCardTemplate : _actionButtonTemplate;
                case ActionTemplateKind.PillItem:
                    return _pillItemTemplate != null ? _pillItemTemplate : _actionButtonTemplate;
                case ActionTemplateKind.MarketDeckAction:
                    return _marketDeckActionTemplate != null ? _marketDeckActionTemplate : _actionButtonTemplate;
                default:
                    return _actionButtonTemplate;
            }
        }

        private static void EnsureSemanticTemplates(Transform templateRoot)
        {
            if (templateRoot == null)
            {
                return;
            }

            EnsureTemplate(templateRoot, "StatCardTemplate", "InfoCardTemplate", ThemeCardInk);
            EnsureTemplate(templateRoot, "MapNodeTemplate", "InfoCardTemplate", new Color(0.075f, 0.100f, 0.110f, 1f));
            EnsureTemplate(templateRoot, "RewardCardTemplate", "ActionButtonTemplate", ThemeRewardButtonInk);
            EnsureTemplate(templateRoot, "RouteChoiceTemplate", "ActionButtonTemplate", ThemeRouteButtonInk);
            EnsureTemplate(templateRoot, "RestChoiceTemplate", "ActionButtonTemplate", ThemeRestButtonInk);
            EnsureTemplate(templateRoot, "MarketItemTemplate", "ActionButtonTemplate", ThemeMarketButtonInk);
            EnsureTemplate(templateRoot, "ChestRewardTemplate", "ActionButtonTemplate", ThemeChestButtonInk);
            EnsureTemplate(templateRoot, "MysticEventTemplate", "ActionButtonTemplate", ThemeMysticButtonInk);
            EnsureTemplate(templateRoot, "GoldenCorePassiveTemplate", "ActionButtonTemplate", ThemeGoldenCoreButtonInk);
            EnsureTemplate(templateRoot, "BattleCardTemplate", "ActionButtonTemplate", ThemeBattleCardButtonInk);
            EnsureTemplate(templateRoot, "PillItemTemplate", "ActionButtonTemplate", ThemePillButtonInk);
            EnsureTemplate(templateRoot, "MarketDeckActionTemplate", "ActionButtonTemplate", ThemeMarketDeckButtonInk);
        }

        private static void EnsureTemplate(Transform templateRoot, string templateName, string fallbackTemplateName, Color color)
        {
            EnsureVisualAssetsLoaded();
            var existing = templateRoot.Find(templateName);
            var target = existing != null ? existing.gameObject : null;
            if (target == null)
            {
                var fallback = templateRoot.Find(fallbackTemplateName);
                if (fallback == null)
                {
                    return;
                }

                target = Instantiate(fallback.gameObject, templateRoot, false);
                target.name = templateName;
            }

            target.SetActive(false);
            var image = target.GetComponent<Image>();
            if (image != null)
            {
                image.color = color;
                var sprite = templateName == "BattleCardTemplate" ? _cardFrameSpiritSprite : _buttonNormalSprite;
                ApplySprite(image, sprite, Image.Type.Sliced);
            }
        }

        private static void ApplyActionButtonStyle(Button button, ActionTemplateKind templateKind)
        {
            if (button == null)
            {
                return;
            }

            ApplyButtonColors(button, ResolveActionButtonColor(templateKind));
            if (templateKind == ActionTemplateKind.BattleCard)
            {
                ApplySprite(button.targetGraphic as Image ?? button.GetComponent<Image>(), _cardFrameSpiritSprite, Image.Type.Sliced);
            }
        }

        private static Color ResolveActionButtonColor(ActionTemplateKind templateKind)
        {
            switch (templateKind)
            {
                case ActionTemplateKind.RewardCard:
                    return ThemeRewardButtonInk;
                case ActionTemplateKind.RouteChoice:
                    return ThemeRouteButtonInk;
                case ActionTemplateKind.RestChoice:
                    return ThemeRestButtonInk;
                case ActionTemplateKind.MarketItem:
                    return ThemeMarketButtonInk;
                case ActionTemplateKind.ChestReward:
                    return ThemeChestButtonInk;
                case ActionTemplateKind.MysticEvent:
                    return ThemeMysticButtonInk;
                case ActionTemplateKind.GoldenCorePassive:
                    return ThemeGoldenCoreButtonInk;
                case ActionTemplateKind.BattleCard:
                    return ThemeBattleCardButtonInk;
                case ActionTemplateKind.PillItem:
                    return ThemePillButtonInk;
                case ActionTemplateKind.MarketDeckAction:
                    return ThemeMarketDeckButtonInk;
                default:
                    return ThemeButtonInk;
            }
        }

        private static void InstantiateTemplate(GameObject template, Transform parent, string name)
        {
            if (template == null || parent == null || parent.Find(name) != null)
            {
                return;
            }

            var instance = Instantiate(template, parent, false);
            instance.name = name;
            instance.SetActive(true);
        }

        private static RectTransform CreatePanel(string name, Transform parent, Color color)
        {
            EnsureVisualAssetsLoaded();
            var rect = CreateRect(name, parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var image = GetOrAdd<Image>(rect.gameObject);
            image.color = color;
            ApplyPanelSprite(image, PanelVisualKind.Dark);
            GetOrAdd<CanvasGroup>(rect.gameObject).alpha = 1f;
            ApplyGraphicChrome(rect.gameObject, ThemePanelOutline, ThemeShadow, new Vector2(2.5f, -2.5f));
            var layout = GetOrAdd<VerticalLayoutGroup>(rect.gameObject);
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
            var rowLayout = GetOrAdd<HorizontalLayoutGroup>(row.gameObject);
            rowLayout.spacing = 10;
            rowLayout.childForceExpandWidth = false;
            rowLayout.childForceExpandHeight = true;
            rowLayout.childControlWidth = true;
            rowLayout.childControlHeight = true;
            SetLayout(row.gameObject, flexibleWidth: 1, preferredHeight: height);

            var labelText = CreateText($"{label}Label", row, label, 14, FontStyle.Bold, TextAnchor.MiddleLeft);
            labelText.color = ThemeJade;
            ApplyTextChrome(labelText, new Color(0.20f, 0.54f, 0.43f, 0.42f), new Vector2(1f, -1f));
            SetLayout(labelText.gameObject, preferredWidth: 58, flexibleHeight: 1);
            return row;
        }

        private static RectTransform CreateInfoCard(string name, Transform parent, string title, string value, string note, Color backgroundColor, Color valueColor)
        {
            EnsureVisualAssetsLoaded();
            var rect = CreateRect(name, parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var image = GetOrAdd<Image>(rect.gameObject);
            image.color = backgroundColor;
            ApplySprite(image, _cardFrameCommonSprite, Image.Type.Sliced);
            GetOrAdd<CanvasGroup>(rect.gameObject).alpha = 1f;
            ApplyGraphicChrome(rect.gameObject, ThemeGoldOutline, ThemeShadow, new Vector2(2f, -2f));
            var layout = GetOrAdd<VerticalLayoutGroup>(rect.gameObject);
            layout.padding = new RectOffset(10, 10, 4, 4);
            layout.spacing = 1;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = true;
            layout.childControlHeight = true;

            var titleText = CreateText("Title", rect, title, 11, FontStyle.Bold, TextAnchor.MiddleLeft);
            titleText.color = ThemeJade;
            SetLayout(titleText.gameObject, flexibleWidth: 1, preferredHeight: 12);

            var valueText = CreateText("Value", rect, value, 14, FontStyle.Bold, TextAnchor.MiddleLeft);
            valueText.color = valueColor;
            valueText.resizeTextForBestFit = true;
            valueText.resizeTextMinSize = 10;
            valueText.resizeTextMaxSize = 14;
            SetLayout(valueText.gameObject, flexibleWidth: 1, preferredHeight: 16);

            var noteText = CreateText("Note", rect, note, 10, FontStyle.Normal, TextAnchor.MiddleLeft);
            noteText.color = ThemeTextMuted;
            noteText.resizeTextForBestFit = true;
            noteText.resizeTextMinSize = 8;
            noteText.resizeTextMaxSize = 10;
            SetLayout(noteText.gameObject, flexibleWidth: 1, preferredHeight: 12);
            return rect;
        }

        private static void CreateSectionLabel(Transform parent, string label)
        {
            var text = CreateText($"{label}Label", parent, label, 14, FontStyle.Bold, TextAnchor.MiddleLeft);
            text.color = ThemeJade;
            ApplyTextChrome(text, new Color(0.20f, 0.54f, 0.43f, 0.42f), new Vector2(1f, -1f));
            SetLayout(text.gameObject, flexibleWidth: 1, preferredHeight: 22);
        }

        private static RectTransform CreateScrollContent(string name, Transform parent, string label, float height)
        {
            var wrapper = CreatePanel($"{name}Panel", parent, ThemeScrollPanelInk);
            var wrapperLayout = wrapper.GetComponent<VerticalLayoutGroup>();
            wrapperLayout.padding = new RectOffset(14, 14, 8, 8);
            wrapperLayout.spacing = 6;
            SetLayout(wrapper.gameObject, flexibleWidth: 1, preferredHeight: height);

            CreateSectionLabel(wrapper, label);

            var viewport = CreateRect("Viewport", wrapper, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var viewportImage = GetOrAdd<Image>(viewport.gameObject);
            viewportImage.color = ThemeViewportInk;
            var mask = GetOrAdd<Mask>(viewport.gameObject);
            mask.showMaskGraphic = false;
            SetLayout(viewport.gameObject, flexibleWidth: 1, flexibleHeight: 1);

            var content = CreateRect("Content", viewport, new Vector2(0f, 0f), new Vector2(0f, 1f), Vector2.zero, Vector2.zero);
            content.pivot = new Vector2(0f, 0.5f);
            var contentLayout = GetOrAdd<HorizontalLayoutGroup>(content.gameObject);
            contentLayout.padding = new RectOffset(8, 8, 8, 8);
            contentLayout.spacing = 10;
            contentLayout.childForceExpandWidth = false;
            contentLayout.childForceExpandHeight = true;
            contentLayout.childControlWidth = true;
            contentLayout.childControlHeight = true;
            var fitter = GetOrAdd<ContentSizeFitter>(content.gameObject);
            fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.verticalFit = ContentSizeFitter.FitMode.Unconstrained;

            var scroll = GetOrAdd<ScrollRect>(wrapper.gameObject);
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
            var text = GetOrAdd<Text>(rect.gameObject);
            text.text = value;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.alignment = anchor;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.lineSpacing = 1.08f;
            ApplyTextChrome(text, ThemeShadow, new Vector2(1f, -1f));
            return text;
        }

        private static void ConfigureBodyText(Text text, int minSize, int maxSize)
        {
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = minSize;
            text.resizeTextMaxSize = maxSize;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.lineSpacing = 1.03f;
        }

        private static Button CreateButton(string name, Transform parent, string label)
        {
            return CreateButton(name, parent, label, string.Empty);
        }

        private static Button CreateButton(string name, Transform parent, string label, string detail)
        {
            EnsureVisualAssetsLoaded();
            var rect = CreateRect(name, parent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var image = GetOrAdd<Image>(rect.gameObject);
            image.color = ThemeButtonInk;
            ApplySprite(image, _buttonNormalSprite, Image.Type.Sliced);
            GetOrAdd<CanvasGroup>(rect.gameObject).alpha = 1f;
            ApplyGraphicChrome(rect.gameObject, ThemeGoldOutline, ThemeShadow, new Vector2(2f, -2f));
            var button = GetOrAdd<Button>(rect.gameObject);
            button.targetGraphic = image;
            ApplyButtonColors(button, image.color);

            var layout = GetOrAdd<VerticalLayoutGroup>(rect.gameObject);
            layout.padding = new RectOffset(10, 10, 8, 8);
            layout.spacing = 4;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            layout.childControlWidth = true;
            layout.childControlHeight = true;

            var text = CreateText("Label", rect, label, 15, FontStyle.Bold, TextAnchor.MiddleCenter);
            text.color = ThemeTextMain;
            text.raycastTarget = false;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = 10;
            text.resizeTextMaxSize = 15;
            SetLayout(text.gameObject, flexibleWidth: 1, preferredHeight: 34);

            var detailText = CreateText("Detail", rect, detail, 12, FontStyle.Normal, TextAnchor.UpperCenter);
            detailText.color = new Color(0.82f, 0.86f, 0.80f, 1f);
            detailText.raycastTarget = false;
            detailText.resizeTextForBestFit = true;
            detailText.resizeTextMinSize = 8;
            detailText.resizeTextMaxSize = 12;
            SetLayout(detailText.gameObject, flexibleWidth: 1, flexibleHeight: 1);
            return button;
        }

        private static void ApplyButtonColors(Button button, Color baseColor)
        {
            EnsureVisualAssetsLoaded();
            var image = button.targetGraphic as Image ?? button.GetComponent<Image>();
            if (image != null)
            {
                image.color = baseColor;
                ApplySprite(image, _buttonNormalSprite, Image.Type.Sliced);
                button.targetGraphic = image;
            }

            var colors = button.colors;
            colors.normalColor = baseColor;
            colors.highlightedColor = Color.Lerp(baseColor, ThemeGold, 0.20f);
            colors.pressedColor = Color.Lerp(baseColor, Color.black, 0.45f);
            colors.selectedColor = Color.Lerp(baseColor, ThemeJade, 0.16f);
            colors.disabledColor = new Color(0.075f, 0.078f, 0.074f, 0.76f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;
            if (_buttonHoverSprite != null || _buttonPressedSprite != null || _buttonDisabledSprite != null)
            {
                button.transition = Selectable.Transition.SpriteSwap;
                var spriteState = button.spriteState;
                spriteState.highlightedSprite = _buttonHoverSprite;
                spriteState.pressedSprite = _buttonPressedSprite;
                spriteState.selectedSprite = _buttonHoverSprite;
                spriteState.disabledSprite = _buttonDisabledSprite;
                button.spriteState = spriteState;
            }
        }

        private static void EnsureVisualAssetsLoaded()
        {
            if (_visualAssetsLoaded)
            {
                return;
            }

            _combatBackgroundSprite = CultivationUIAssetCatalog.LoadEditorSprite(CultivationUIAssetCatalog.CombatQiRefiningBackground);
            _darkPanelSprite = CultivationUIAssetCatalog.LoadEditorSprite(CultivationUIAssetCatalog.DarkPanel);
            _goldPanelSprite = CultivationUIAssetCatalog.LoadEditorSprite(CultivationUIAssetCatalog.GoldPanel);
            _buttonNormalSprite = CultivationUIAssetCatalog.LoadEditorSprite(CultivationUIAssetCatalog.ButtonNormal);
            _buttonHoverSprite = CultivationUIAssetCatalog.LoadEditorSprite(CultivationUIAssetCatalog.ButtonHover);
            _buttonPressedSprite = CultivationUIAssetCatalog.LoadEditorSprite(CultivationUIAssetCatalog.ButtonPressed);
            _buttonDisabledSprite = CultivationUIAssetCatalog.LoadEditorSprite(CultivationUIAssetCatalog.ButtonDisabled);
            _cardFrameCommonSprite = CultivationUIAssetCatalog.LoadEditorSprite(CultivationUIAssetCatalog.CardFrameCommon);
            _cardFrameSpiritSprite = CultivationUIAssetCatalog.LoadEditorSprite(CultivationUIAssetCatalog.CardFrameSpirit);
            _visualAssetsLoaded = true;
        }

        private static void ApplyPanelSprite(Image image, PanelVisualKind visualKind)
        {
            ApplySprite(image, visualKind == PanelVisualKind.Gold ? _goldPanelSprite : _darkPanelSprite, Image.Type.Sliced);
        }

        private static void ApplySprite(Image image, Sprite sprite, Image.Type imageType)
        {
            if (image == null || sprite == null)
            {
                return;
            }

            image.sprite = sprite;
            image.type = imageType;
            image.preserveAspect = false;
        }

        private static void ApplyGraphicChrome(GameObject target, Color outlineColor, Color shadowColor, Vector2 shadowDistance)
        {
            var outline = GetOrAddExact<Outline>(target);
            outline.effectColor = outlineColor;
            outline.effectDistance = new Vector2(1.25f, -1.25f);
            outline.useGraphicAlpha = true;

            var shadow = GetOrAddExact<Shadow>(target);
            shadow.effectColor = shadowColor;
            shadow.effectDistance = shadowDistance;
            shadow.useGraphicAlpha = true;
        }

        private static void ApplyTextChrome(Text text, Color shadowColor, Vector2 shadowDistance)
        {
            var shadow = GetOrAddExact<Shadow>(text.gameObject);
            shadow.effectColor = shadowColor;
            shadow.effectDistance = shadowDistance;
            shadow.useGraphicAlpha = true;
        }

        private static RectTransform CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var existing = parent != null ? parent.Find(name) : null;
            RectTransform rect;
            if (existing != null)
            {
                rect = existing as RectTransform ?? existing.gameObject.AddComponent<RectTransform>();
            }
            else
            {
                var go = new GameObject(name, typeof(RectTransform));
                rect = go.GetComponent<RectTransform>();
                rect.SetParent(parent, false);
            }

            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            rect.localScale = Vector3.one;
            return rect;
        }

        private static T GetOrAdd<T>(GameObject target) where T : Component
        {
            var component = target.GetComponent<T>();
            return component != null ? component : target.AddComponent<T>();
        }

        private static T GetOrAddExact<T>(GameObject target) where T : Component
        {
            foreach (var component in target.GetComponents<T>())
            {
                if (component.GetType() == typeof(T))
                {
                    return component;
                }
            }

            return target.AddComponent<T>();
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
