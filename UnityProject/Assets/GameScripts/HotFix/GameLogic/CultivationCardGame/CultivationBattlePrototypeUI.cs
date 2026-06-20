using System;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace GameLogic.Cultivation
{
    public sealed class CultivationBattlePrototypeUI : MonoBehaviour
    {
        public const string RootName = "CultivationBattlePrototypeUI";

        private const int MaxLogLines = 8;

        private BattleEngine _engine;
        private BattleState _state;
        private RectTransform _handRoot;
        private Text _playerText;
        private Text _enemyText;
        private Text _pileText;
        private Text _logText;
        private Text _resultText;
        private Button _endTurnButton;
        private Button _resetButton;

        public BattlePrototypeSnapshot Snapshot => BattlePrototypeSnapshot.From(_state);

        public static CultivationBattlePrototypeUI Open(Transform parent = null)
        {
            var uiParent = parent != null ? parent : ResolveParent();
            var old = uiParent.Find(RootName);
            if (old != null)
            {
                var existing = old.GetComponent<CultivationBattlePrototypeUI>();
                if (existing != null)
                {
                    existing.ResetBattle();
                    return existing;
                }

                DestroyObject(old.gameObject);
            }

            EnsureEventSystem();
            var root = CreateRect(RootName, uiParent, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var view = root.gameObject.AddComponent<CultivationBattlePrototypeUI>();
            view.Initialize();
            return view;
        }

        public void ResetBattle()
        {
            _engine = new BattleEngine(20260620);
            _state = _engine.CreateBattle(CultivationSeedData.CreateSwordSectStarterDeck(), CultivationSeedData.StoneDemon);
            _state.Logs.Add(new BattleLogEntry("Phase 2 占位战斗界面已连接 BattleEngine。"));
            Refresh();
        }

        public void PlayCardAt(int handIndex)
        {
            if (_state == null || _state.Outcome != BattleOutcome.InProgress)
            {
                return;
            }

            if (handIndex < 0 || handIndex >= _state.Hand.Count)
            {
                return;
            }

            var card = _state.Hand[handIndex];
            var target = _state.Enemies.FirstOrDefault(enemy => !enemy.Body.IsDefeated);
            if (!_engine.CanPlay(_state, card))
            {
                _state.Logs.Add(new BattleLogEntry($"{card.Name} 灵力不足，无法打出。"));
                Refresh();
                return;
            }

            _engine.PlayCard(_state, card, target);
            Refresh();
        }

        public void EndTurn()
        {
            if (_state == null || _state.Outcome != BattleOutcome.InProgress)
            {
                return;
            }

            _engine.EndPlayerTurn(_state);
            Refresh();
        }

        private void Initialize()
        {
            if (_handRoot == null)
            {
                BuildView();
            }

            ResetBattle();
        }

        private void BuildView()
        {
            var background = gameObject.AddComponent<Image>();
            background.color = new Color(0.06f, 0.07f, 0.09f, 0.96f);

            var rootLayout = gameObject.AddComponent<VerticalLayoutGroup>();
            rootLayout.padding = new RectOffset(24, 24, 20, 20);
            rootLayout.spacing = 12;
            rootLayout.childForceExpandWidth = true;
            rootLayout.childForceExpandHeight = false;
            rootLayout.childControlWidth = true;
            rootLayout.childControlHeight = true;

            var title = CreateText("Title", transform, "仙途·天命  战斗原型", 30, FontStyle.Bold, TextAnchor.MiddleLeft);
            title.color = new Color(0.95f, 0.89f, 0.72f, 1f);
            SetLayout(title.gameObject, flexibleWidth: 1, preferredHeight: 40);

            var body = CreateRect("Body", transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var bodyLayout = body.gameObject.AddComponent<HorizontalLayoutGroup>();
            bodyLayout.spacing = 12;
            bodyLayout.childForceExpandWidth = true;
            bodyLayout.childForceExpandHeight = true;
            bodyLayout.childControlWidth = true;
            bodyLayout.childControlHeight = true;
            SetLayout(body.gameObject, flexibleWidth: 1, flexibleHeight: 1, preferredHeight: 420);

            var left = CreatePanel("PlayerPanel", body, new Color(0.10f, 0.12f, 0.16f, 0.95f));
            SetLayout(left.gameObject, flexibleWidth: 1, flexibleHeight: 1);
            _playerText = CreateText("PlayerText", left, string.Empty, 22, FontStyle.Bold, TextAnchor.UpperLeft);
            _playerText.color = Color.white;
            SetLayout(_playerText.gameObject, flexibleWidth: 1, preferredHeight: 150);
            _pileText = CreateText("PileText", left, string.Empty, 18, FontStyle.Normal, TextAnchor.UpperLeft);
            _pileText.color = new Color(0.80f, 0.85f, 0.90f, 1f);
            SetLayout(_pileText.gameObject, flexibleWidth: 1, preferredHeight: 120);

            var center = CreatePanel("EnemyPanel", body, new Color(0.16f, 0.10f, 0.10f, 0.95f));
            SetLayout(center.gameObject, flexibleWidth: 1, flexibleHeight: 1);
            _enemyText = CreateText("EnemyText", center, string.Empty, 22, FontStyle.Bold, TextAnchor.UpperLeft);
            _enemyText.color = Color.white;
            SetLayout(_enemyText.gameObject, flexibleWidth: 1, preferredHeight: 220);
            _resultText = CreateText("ResultText", center, string.Empty, 24, FontStyle.Bold, TextAnchor.MiddleCenter);
            _resultText.color = new Color(1f, 0.82f, 0.32f, 1f);
            SetLayout(_resultText.gameObject, flexibleWidth: 1, preferredHeight: 80);

            var right = CreatePanel("LogPanel", body, new Color(0.08f, 0.10f, 0.12f, 0.95f));
            SetLayout(right.gameObject, flexibleWidth: 1, flexibleHeight: 1);
            _logText = CreateText("LogText", right, string.Empty, 16, FontStyle.Normal, TextAnchor.UpperLeft);
            _logText.color = new Color(0.86f, 0.90f, 0.92f, 1f);
            SetLayout(_logText.gameObject, flexibleWidth: 1, flexibleHeight: 1);

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

            _resetButton = CreateButton("ResetButton", actionBar, "重开战斗");
            _resetButton.onClick.AddListener(ResetBattle);
            SetLayout(_resetButton.gameObject, preferredWidth: 180, preferredHeight: 48);

            _endTurnButton = CreateButton("EndTurnButton", actionBar, "结束回合");
            _endTurnButton.onClick.AddListener(EndTurn);
            SetLayout(_endTurnButton.gameObject, preferredWidth: 180, preferredHeight: 48);
        }

        private void Refresh()
        {
            if (_state == null)
            {
                return;
            }

            var snapshot = Snapshot;
            _playerText.text = $"玩家\nHP {snapshot.PlayerHp}/{snapshot.PlayerMaxHp}\n护盾 {snapshot.PlayerShield}\n灵力 {snapshot.Spirit}/{snapshot.SpiritMax}\n回合 {snapshot.TurnNumber}\n闪避 {snapshot.DodgeCharges} / 反击 {snapshot.DodgeCounterDamage}\n受击反伤 {snapshot.AttackCounterDamage}/{snapshot.AttackCounterChancePercent}%\n中毒 {snapshot.PlayerPoisonStacks}  生生不息 {snapshot.RegenerationPerTurn}/{snapshot.RegenerationTurns}  毒瘴 {snapshot.PoisonCounterStacks}/{snapshot.PoisonCounterTurns}";
            _pileText.text = $"牌堆 {snapshot.DrawPileCount}    弃牌 {snapshot.DiscardPileCount}\n手牌 {snapshot.HandCount}\n状态：{snapshot.Outcome}";
            _enemyText.text = $"敌人：{snapshot.EnemyName}\nHP {snapshot.EnemyHp}/{snapshot.EnemyMaxHp}\n护盾 {snapshot.EnemyShield}\n破防 {snapshot.EnemyBreakDefenseStacks}\n灼烧 {snapshot.EnemyBurnStacks} / {snapshot.EnemyBurnTurns} 回合\n中毒 {snapshot.EnemyPoisonStacks}\n意图：{snapshot.EnemyIntent}";
            _resultText.text = snapshot.Outcome == BattleOutcome.InProgress ? string.Empty : snapshot.Outcome == BattleOutcome.Victory ? "胜利" : "失败";
            _logText.text = BuildLogText();
            _endTurnButton.interactable = _state.Outcome == BattleOutcome.InProgress;

            RebuildHand();
        }

        private void RebuildHand()
        {
            for (var i = _handRoot.childCount - 1; i >= 0; i--)
            {
                DestroyObject(_handRoot.GetChild(i).gameObject);
            }

            for (var i = 0; i < _state.Hand.Count; i++)
            {
                var index = i;
                var card = _state.Hand[i];
                var button = CreateButton($"Card_{i}_{card.Id}", _handRoot, FormatCard(card));
                button.interactable = _engine.CanPlay(_state, card);
                button.onClick.AddListener(() => PlayCardAt(index));
                SetLayout(button.gameObject, flexibleWidth: 1, preferredHeight: 130);
            }
        }

        private string BuildLogText()
        {
            var logs = _state.Logs.Skip(Math.Max(0, _state.Logs.Count - MaxLogLines)).ToArray();
            if (logs.Length == 0)
            {
                return "等待行动...";
            }

            var builder = new StringBuilder();
            foreach (var log in logs)
            {
                builder.Append("- ").Append(log.Message).AppendLine();
            }

            return builder.ToString();
        }

        private static string FormatCard(CardDefinition card)
        {
            return $"{card.Name}\n灵力 {card.SpiritCost}\n{string.Join("\n", card.Effects.Select(FormatEffect))}";
        }

        private static string FormatEffect(CardEffect effect)
        {
            switch (effect.Type)
            {
                case CardEffectType.Damage:
                    return $"造成 {effect.Value} 伤害";
                case CardEffectType.Shield:
                    return $"获得 {effect.Value} 护盾";
                case CardEffectType.Dodge:
                    return $"获得 {effect.Value} 次闪避";
                case CardEffectType.DodgeCounter:
                    return $"闪避成功时反击 {effect.Value} 伤害";
                case CardEffectType.AttackCounter:
                    return effect.ChancePercent >= 100
                        ? $"受击反伤 {effect.Value} 伤害"
                        : $"{effect.ChancePercent}% 概率受击反伤 {effect.Value} 伤害";
                case CardEffectType.Draw:
                    return $"抽 {effect.Value} 张牌";
                case CardEffectType.Heal:
                    return $"恢复 {effect.Value} HP";
                case CardEffectType.BreakDefense:
                    return $"破防 {effect.Value}";
                case CardEffectType.Burn:
                    return $"灼烧 {effect.Value} / {effect.Duration} 回合";
                case CardEffectType.Poison:
                    return $"中毒 {effect.Value}";
                case CardEffectType.PoisonBurst:
                    return effect.SecondaryValue > 0
                        ? $"毒爆：消耗中毒 ×{effect.Value} 伤害，留下 {effect.SecondaryValue} 层余毒"
                        : $"毒爆：消耗中毒 ×{effect.Value} 伤害";
                case CardEffectType.Leech:
                    return $"吸灵 {effect.Value} 伤害，恢复伤害的 {effect.SecondaryValue}%";
                case CardEffectType.Regeneration:
                    return $"生生不息 {effect.Value} HP / {Math.Max(1, effect.Duration)} 回合";
                case CardEffectType.PoisonAttackCounter:
                    return $"受击施加中毒 {effect.Value} / {Math.Max(1, effect.Duration)} 回合";
                case CardEffectType.BloodSacrifice:
                    return $"血祭：失去 {effect.Value} HP";
                case CardEffectType.SacrificeHandCardDamage:
                    return $"献祭 1 张手牌，造成其灵力 ×{effect.Value} 伤害";
                case CardEffectType.SacrificeHandCardHeal:
                    return $"献祭 1 张手牌，恢复 {effect.Value} HP";
                case CardEffectType.LowHpDamage:
                    return $"造成 {effect.Value} 伤害，HP ≤ {effect.ChancePercent}% 时 +{(effect.SecondaryValue > 0 ? effect.SecondaryValue : effect.Value)} 伤害";
                case CardEffectType.MissingHpDamage:
                    return $"造成 {effect.Value} 伤害，每损失 10% HP +{effect.SecondaryValue} 伤害";
                case CardEffectType.LowHpShield:
                    return $"获得 {effect.Value} 护盾，HP ≤ {effect.ChancePercent}% 时 +{effect.SecondaryValue} 护盾";
                case CardEffectType.BloodGuardHeal:
                    return $"受击后回复 {effect.Value} HP / {Math.Max(1, effect.Duration)} 回合";
                case CardEffectType.SpiritGain:
                    return $"本回合灵力 +{effect.Value}";
                case CardEffectType.FlatDamageBonus:
                    return $"所有出牌伤害 +{effect.Value} / {Math.Max(1, effect.Duration)} 回合";
                case CardEffectType.FrenzyDamageBonus:
                    return $"癫狂：每损失 10% HP，出牌伤害 +{effect.Value}% / {Math.Max(1, effect.Duration)} 回合";
                case CardEffectType.BloodlossRetaliation:
                    return $"失去 HP 时反击等量伤害的 {effect.Value}% / {Math.Max(1, effect.Duration)} 回合";
                case CardEffectType.LowHpDodge:
                    return $"获得 {effect.Value} 次闪避，HP ≤ {effect.ChancePercent}% 时 +{effect.SecondaryValue} 次";
                case CardEffectType.SelfDamageDodgeDraw:
                    return $"本回合每自伤 {effect.Value} HP，获得 1 闪避并抽 1 张牌";
                case CardEffectType.DeathWard:
                    return $"免死：触发时恢复 {effect.Value} HP / {Math.Max(1, effect.Duration)} 回合";
                case CardEffectType.DamageTakenHeal:
                    return $"受伤后恢复伤害的 {effect.Value}% / {Math.Max(1, effect.Duration)} 回合";
                case CardEffectType.ChanceDamage:
                    return effect.RepeatCount > 1
                        ? $"造成 {effect.FallbackValue} 伤害 × {effect.RepeatCount}，每击 {effect.ChancePercent}% 概率暴击"
                        : $"{effect.ChancePercent}% 概率造成 {effect.Value} 伤害，失败造成 {effect.FallbackValue} 伤害";
                case CardEffectType.ChanceDamageWithStun:
                    return $"造成 {effect.Value} 伤害 × {effect.RepeatCount}，每击 {effect.ChancePercent}% 概率暴击；暴击时 {effect.FallbackValue}% 概率眩晕";
                case CardEffectType.ChanceDamageWithChain:
                    return $"造成 {effect.Value} 伤害 × {effect.RepeatCount}，每击 {effect.ChancePercent}% 概率暴击；每击 {effect.SecondaryValue}% 概率连锁 {effect.FallbackValue} 伤害";
                case CardEffectType.ChainOnChanceDamage:
                    return $"{effect.ChancePercent}% 概率造成 {effect.Value} 伤害，失败造成 {effect.FallbackValue} 伤害；命中连锁 {effect.SecondaryValue} 伤害";
                case CardEffectType.DamageAfterCriticalTriggered:
                    return $"造成 {effect.Value} 伤害；本回合已暴击时额外造成 {effect.FallbackValue} 伤害";
                case CardEffectType.DamageAfterCriticalTriggeredWithStun:
                    return $"造成 {effect.Value} 伤害；本回合已暴击时额外造成 {effect.FallbackValue} 伤害并眩晕 {Math.Max(1, effect.Duration)} 回合";
                case CardEffectType.DamageAfterCriticalTriggeredChainAll:
                    return $"造成 {effect.Value} 伤害；本回合已暴击时奖励连锁全体，各造成 {effect.FallbackValue} 伤害";
                case CardEffectType.ChanceStun:
                    return $"{effect.ChancePercent}% 概率眩晕 {Math.Max(1, effect.Duration)} 回合";
                case CardEffectType.ChainOnChanceStun:
                    return $"造成 {effect.Value} 伤害，{effect.ChancePercent}% 概率眩晕 {Math.Max(1, effect.Duration)} 回合；成功连锁 {effect.SecondaryValue} 伤害";
                case CardEffectType.ChanceChainDamage:
                    return $"造成 {effect.Value} 伤害，{effect.ChancePercent}% 概率连锁 {effect.SecondaryValue} 伤害";
                case CardEffectType.ChanceChainDamageWithStun:
                    return $"造成 {effect.Value} 伤害，{effect.ChancePercent}% 概率连锁 {effect.SecondaryValue} 伤害；连锁有 {effect.FallbackValue}% 概率眩晕";
                case CardEffectType.ChanceChainDamageRepeatTarget:
                    return $"造成 {effect.Value} 伤害，{effect.ChancePercent}% 概率连锁 {effect.SecondaryValue} 伤害；可重复目标，最多 {Math.Max(1, effect.RepeatCount)} 次";
                case CardEffectType.ChargeDamage:
                    return $"下次攻击伤害 ×{effect.Value}";
                default:
                    return effect.Type.ToString();
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

            var text = CreateText("Label", rect, label, 16, FontStyle.Bold, TextAnchor.MiddleCenter);
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

    public sealed class BattlePrototypeSnapshot
    {
        public int TurnNumber { get; private set; }

        public int PlayerHp { get; private set; }

        public int PlayerMaxHp { get; private set; }

        public int PlayerShield { get; private set; }

        public int DodgeCharges { get; private set; }

        public int DodgeCounterDamage { get; private set; }

        public int AttackCounterDamage { get; private set; }

        public int AttackCounterChancePercent { get; private set; }

        public int PlayerPoisonStacks { get; private set; }

        public int PoisonCounterStacks { get; private set; }

        public int PoisonCounterTurns { get; private set; }

        public int RegenerationPerTurn { get; private set; }

        public int RegenerationTurns { get; private set; }

        public int Spirit { get; private set; }

        public int SpiritMax { get; private set; }

        public int DrawPileCount { get; private set; }

        public int DiscardPileCount { get; private set; }

        public int HandCount { get; private set; }

        public string EnemyName { get; private set; }

        public int EnemyHp { get; private set; }

        public int EnemyMaxHp { get; private set; }

        public int EnemyShield { get; private set; }

        public int EnemyBreakDefenseStacks { get; private set; }

        public int EnemyBurnStacks { get; private set; }

        public int EnemyBurnTurns { get; private set; }

        public int EnemyPoisonStacks { get; private set; }

        public string EnemyIntent { get; private set; }

        public BattleOutcome Outcome { get; private set; }

        public static BattlePrototypeSnapshot From(BattleState state)
        {
            if (state == null)
            {
                return new BattlePrototypeSnapshot();
            }

            var enemy = state.Enemies.FirstOrDefault();
            return new BattlePrototypeSnapshot
            {
                TurnNumber = state.TurnNumber,
                PlayerHp = state.Player.CurrentHp,
                PlayerMaxHp = state.Player.MaxHp,
                PlayerShield = state.Player.Shield,
                DodgeCharges = state.DodgeCharges,
                DodgeCounterDamage = state.DodgeCounterDamage,
                AttackCounterDamage = state.AttackCounterDamage,
                AttackCounterChancePercent = state.AttackCounterChancePercent,
                PlayerPoisonStacks = state.Player.PoisonStacks,
                PoisonCounterStacks = state.PoisonCounterStacks,
                PoisonCounterTurns = state.PoisonCounterTurns,
                RegenerationPerTurn = state.RegenerationPerTurn,
                RegenerationTurns = state.RegenerationTurns,
                Spirit = state.Spirit,
                SpiritMax = state.SpiritMax,
                DrawPileCount = state.DrawPile.Count,
                DiscardPileCount = state.DiscardPile.Count,
                HandCount = state.Hand.Count,
                EnemyName = enemy?.Body.Name ?? string.Empty,
                EnemyHp = enemy?.Body.CurrentHp ?? 0,
                EnemyMaxHp = enemy?.Body.MaxHp ?? 0,
                EnemyShield = enemy?.Body.Shield ?? 0,
                EnemyBreakDefenseStacks = enemy?.Body.BreakDefenseStacks ?? 0,
                EnemyBurnStacks = enemy?.Body.BurnStacks ?? 0,
                EnemyBurnTurns = enemy?.Body.BurnTurns ?? 0,
                EnemyPoisonStacks = enemy?.Body.PoisonStacks ?? 0,
                EnemyIntent = enemy?.CurrentIntent.Description ?? string.Empty,
                Outcome = state.Outcome,
            };
        }
    }
}
