using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace GameLogic.Cultivation
{
    public sealed class CultivationRunPrototypeUI : MonoBehaviour
    {
        public const string RootName = "CultivationRunPrototypeUI";

        private const int MaxLogLines = 8;

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

        public RunPrototypeSnapshot Snapshot => RunPrototypeSnapshot.From(_run);

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
            _run = _runEngine.StartRun(CultivationSeedData.CreateSwordSectStarterDeck(), CreatePrototypeRoute());
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

            _runText.text = BuildRunText();
            _nodeText.text = BuildNodeText();
            _battleText.text = BuildBattleText();
            _deckText.text = BuildDeckText();
            _logText.text = BuildLogText();
            _endTurnButton.interactable = _run.Status == CultivationRunStatus.InBattle && _run.CurrentBattle != null && _run.CurrentBattle.Outcome == BattleOutcome.InProgress;

            RebuildChoices();
            RebuildHand();
        }

        private string BuildRunText()
        {
            return $"状态：{_run.Status}\n节点：{_run.CurrentNodeIndex + 1}/{_run.Route.Count}\nHP：{_run.PlayerCurrentHp}/{_run.PlayerMaxHp}\n牌组：{_run.Deck.Count} 张\n已拿奖励：{_run.ClaimedRewards.Count}";
        }

        private string BuildNodeText()
        {
            var builder = new StringBuilder();
            builder.Append("当前节点：").Append(_run.CurrentNode.Name).AppendLine();
            builder.Append("类型：").Append(_run.CurrentNode.Type).AppendLine();
            if (_run.CurrentNode.Enemy != null)
            {
                builder.Append("敌人：").Append(_run.CurrentNode.Enemy.Name).AppendLine();
            }

            if (_run.Status == CultivationRunStatus.RouteChoice)
            {
                builder.AppendLine();
                builder.AppendLine("可选路线：");
                for (var i = 0; i < _run.CurrentRouteChoices.Count; i++)
                {
                    var choice = _run.CurrentRouteChoices[i];
                    builder.Append(i + 1).Append(". ").Append(choice.TargetNode.Name).Append(" / ").Append(choice.TargetNode.Type).AppendLine();
                }
            }

            return builder.ToString();
        }

        private string BuildBattleText()
        {
            if (_run.Status == CultivationRunStatus.Completed)
            {
                return "本轮修行完成。";
            }

            if (_run.Status == CultivationRunStatus.Defeated)
            {
                return "本轮修行失败。";
            }

            if (_run.Status != CultivationRunStatus.InBattle || _run.CurrentBattle == null)
            {
                return $"等待操作：{_run.Status}";
            }

            var battle = _run.CurrentBattle;
            var enemy = battle.Enemies.FirstOrDefault();
            return $"玩家\nHP {battle.Player.CurrentHp}/{battle.Player.MaxHp}  护盾 {battle.Player.Shield}\n灵力 {battle.Spirit}/{battle.SpiritMax}  回合 {battle.TurnNumber}\n\n敌人：{enemy?.Body.Name ?? string.Empty}\nHP {enemy?.Body.CurrentHp ?? 0}/{enemy?.Body.MaxHp ?? 0}  护盾 {enemy?.Body.Shield ?? 0}\n破防 {enemy?.Body.BreakDefenseStacks ?? 0}  灼烧 {enemy?.Body.BurnStacks ?? 0}/{enemy?.Body.BurnTurns ?? 0}\n意图：{enemy?.CurrentIntent.Description ?? string.Empty}\n\n战斗结果：{battle.Outcome}";
        }

        private string BuildDeckText()
        {
            var builder = new StringBuilder();
            builder.AppendLine("当前牌组：");
            for (var i = 0; i < _run.Deck.Count; i++)
            {
                var card = _run.Deck[i];
                builder.Append(i + 1).Append(". ").Append(card.Name).Append("  灵力 ").Append(card.SpiritCost);
                if (card.CanUpgrade)
                {
                    builder.Append("  可升级");
                }

                builder.AppendLine();
            }

            return builder.ToString();
        }

        private string BuildLogText()
        {
            if (_run.CurrentBattle == null || _run.CurrentBattle.Logs.Count == 0)
            {
                return "日志：等待行动...";
            }

            var logs = _run.CurrentBattle.Logs.Skip(Math.Max(0, _run.CurrentBattle.Logs.Count - MaxLogLines)).ToArray();
            var builder = new StringBuilder();
            builder.AppendLine("战斗日志：");
            foreach (var log in logs)
            {
                builder.Append("- ").Append(log.Message).AppendLine();
            }

            return builder.ToString();
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
                        var button = CreateButton($"Reward_{i}_{reward.Id}", _choiceRoot, $"奖励\n{reward.Card.Name}\n{FormatCardSummary(reward.Card)}");
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
            }
        }

        private void RebuildHand()
        {
            ClearChildren(_handRoot);

            if (_run.Status != CultivationRunStatus.InBattle || _run.CurrentBattle == null)
            {
                return;
            }

            for (var i = 0; i < _run.CurrentBattle.Hand.Count; i++)
            {
                var index = i;
                var card = _run.CurrentBattle.Hand[i];
                var button = CreateButton($"Card_{i}_{card.Id}", _handRoot, $"{card.Name}\n{FormatCardSummary(card)}");
                button.interactable = _run.CurrentBattle.Outcome == BattleOutcome.InProgress && _battleEngine.CanPlay(_run.CurrentBattle, card);
                button.onClick.AddListener(() => PlayCardAt(index));
                SetLayout(button.gameObject, flexibleWidth: 1, preferredHeight: 130);
            }
        }

        private static string FormatCardSummary(CardDefinition card)
        {
            return $"灵力 {card.SpiritCost}\n{string.Join("\n", card.Effects.Select(FormatEffect))}";
        }

        private static string FormatEffect(CardEffect effect)
        {
            switch (effect.Type)
            {
                case CardEffectType.Damage:
                    return $"造成 {effect.Value} 伤害";
                case CardEffectType.Shield:
                    return $"获得 {effect.Value} 护盾";
                case CardEffectType.Draw:
                    return $"抽 {effect.Value} 张牌";
                case CardEffectType.Heal:
                    return $"恢复 {effect.Value} HP";
                case CardEffectType.BreakDefense:
                    return $"破防 {effect.Value}";
                case CardEffectType.Burn:
                    return $"灼烧 {effect.Value} / {effect.Duration} 回合";
                default:
                    return effect.Type.ToString();
            }
        }

        private static IReadOnlyList<CultivationRunNode> CreatePrototypeRoute()
        {
            var rewards = CultivationSeedData.CreateSwordSectRewardPool();
            return new List<CultivationRunNode>
            {
                new CultivationRunNode("node_stone_demon", "山门石魔", CultivationRunNodeType.Battle, CultivationSeedData.StoneDemon, rewards, nextNodeIndices: new[] { 1, 2 }),
                new CultivationRunNode("node_fire_bat", "火蝠洞", CultivationRunNodeType.Battle, CultivationSeedData.FireBat, rewards, nextNodeIndices: new[] { 3 }),
                new CultivationRunNode("node_meditation", "闭关调息", CultivationRunNodeType.Rest, null, null, restHealAmount: 30, nextNodeIndices: new[] { 3 }),
                new CultivationRunNode("node_stone_demon_leader", "石魔首领", CultivationRunNodeType.Elite, CultivationSeedData.StoneDemonLeader, rewards),
            };
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

    public sealed class RunPrototypeSnapshot
    {
        public CultivationRunStatus Status { get; private set; }

        public int CurrentNodeIndex { get; private set; }

        public string CurrentNodeName { get; private set; }

        public int PlayerHp { get; private set; }

        public int PlayerMaxHp { get; private set; }

        public int DeckCount { get; private set; }

        public int HandCount { get; private set; }

        public int RewardCount { get; private set; }

        public int RestUpgradeChoiceCount { get; private set; }

        public int RouteChoiceCount { get; private set; }

        public BattleOutcome BattleOutcome { get; private set; }

        public static RunPrototypeSnapshot From(CultivationRunState state)
        {
            if (state == null)
            {
                return new RunPrototypeSnapshot();
            }

            return new RunPrototypeSnapshot
            {
                Status = state.Status,
                CurrentNodeIndex = state.CurrentNodeIndex,
                CurrentNodeName = state.CurrentNode.Name,
                PlayerHp = state.PlayerCurrentHp,
                PlayerMaxHp = state.PlayerMaxHp,
                DeckCount = state.Deck.Count,
                HandCount = state.CurrentBattle?.Hand.Count ?? 0,
                RewardCount = state.CurrentRewards.Count,
                RestUpgradeChoiceCount = state.RestUpgradeChoices.Count,
                RouteChoiceCount = state.CurrentRouteChoices.Count,
                BattleOutcome = state.CurrentBattle?.Outcome ?? BattleOutcome.InProgress,
            };
        }
    }
}
