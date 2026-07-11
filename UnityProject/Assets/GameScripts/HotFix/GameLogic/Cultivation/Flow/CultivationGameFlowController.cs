using System;
using GameLogic.Cultivation.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameLogic.Cultivation.Flow
{
    public sealed class CultivationGameFlowController : MonoBehaviour
    {
        private const string RootName = "CultivationMinimalGameplayLoop";
        private const string MainRoutePrefabPath = "Assets/AssetRaw/UIRaw/Theme/Cultivation/Templates/SteamDemo/Steam_MainRoute_Restore.prefab";

        private CultivationRouteRuntimeData _runtimeData;
        private CultivationFlowState _state;
        private SteamMainRouteDemoBinder _mainRouteBinder;
        private Text _stateText;
        private Text _hintText;
        private Text _detailText;
        private Text _optionsText;
        private string _detail;
        private CultivationRouteNodeRuntimeData _selectedNode;
        private int _battlePlayerHp;
        private int _battleEnemyHp;
        private bool _rewardFromBattle;

        public CultivationRouteRuntimeData RuntimeData => _runtimeData;

        public CultivationFlowState State => _state;

        public static CultivationGameFlowController OpenOrCreate(Transform parent = null)
        {
            var existing = FindObjectOfType<CultivationGameFlowController>();
            if (existing != null)
            {
                existing.EnsureInitialized();
                return existing;
            }

            EnsureEventSystem();
            var resolvedParent = parent != null ? parent : ResolveVisibleParent();
            var root = new GameObject(RootName, typeof(RectTransform));
            var rect = (RectTransform)root.transform;
            if (resolvedParent != null)
            {
                rect.SetParent(resolvedParent, false);
            }

            Stretch(rect);
            var controller = root.AddComponent<CultivationGameFlowController>();
            controller.EnsureInitialized();
            return controller;
        }

        private void EnsureInitialized()
        {
            var rebuiltRuntimeView = false;
            if (_mainRouteBinder == null || _stateText == null || _hintText == null || _detailText == null || _optionsText == null)
            {
                BuildRuntimeView();
                rebuiltRuntimeView = true;
            }

            if (_runtimeData == null)
            {
                EnterBoot();
            }
            else if (rebuiltRuntimeView)
            {
                RefreshMainRoute();
            }
        }

        private void Update()
        {
            if (_state == CultivationFlowState.MainRoute)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    SelectRouteNode(0);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    SelectRouteNode(1);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    SelectRouteNode(2);
                }
            }
            else if (_state == CultivationFlowState.Event)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    ChooseEventOption(0);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    ChooseEventOption(1);
                }
            }
            else if (_state == CultivationFlowState.Battle)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    AdvanceBattleTurn();
                }
            }
            else if (_state == CultivationFlowState.Reward)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    ChooseReward(0);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    ChooseReward(1);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    ChooseReward(2);
                }
            }
            else if (_state == CultivationFlowState.Finished && Input.GetKeyDown(KeyCode.Space))
            {
                EnterMainRoute("流程完成，Space 回到路线继续。");
            }
        }

        private void BuildRuntimeView()
        {
            var mainRouteRoot = CreateMainRouteInstance(transform);
            _mainRouteBinder = mainRouteRoot.GetComponent<SteamMainRouteDemoBinder>();
            if (_mainRouteBinder == null)
            {
                _mainRouteBinder = mainRouteRoot.AddComponent<SteamMainRouteDemoBinder>();
            }

            var overlay = CreateOverlay(transform);
            _stateText = CreateText("FlowStateText", overlay.transform, 26, FontStyle.Bold, TextAnchor.UpperLeft);
            _hintText = CreateText("FlowHintText", overlay.transform, 18, FontStyle.Normal, TextAnchor.UpperLeft);
            _detailText = CreateText("FlowDetailText", overlay.transform, 18, FontStyle.Normal, TextAnchor.UpperLeft);
            _optionsText = CreateText("FlowOptionsText", overlay.transform, 18, FontStyle.Normal, TextAnchor.UpperLeft);

            LayoutText(_stateText.rectTransform, new Vector2(16f, -10f), new Vector2(660f, 36f));
            LayoutText(_hintText.rectTransform, new Vector2(16f, -50f), new Vector2(660f, 60f));
            LayoutText(_detailText.rectTransform, new Vector2(16f, -118f), new Vector2(660f, 96f));
            LayoutText(_optionsText.rectTransform, new Vector2(16f, -220f), new Vector2(660f, 150f));
        }

        private void EnterBoot()
        {
            _runtimeData = CultivationRouteRuntimeData.CreateDefault();
            _selectedNode = null;
            _rewardFromBattle = false;
            Transition(CultivationFlowState.Boot, "Boot");
            EnterMainRoute("启动完成，进入修仙主路线界面。");
        }

        private void EnterMainRoute(string detail)
        {
            _selectedNode = null;
            _detail = detail;
            Transition(CultivationFlowState.MainRoute, "MainRoute");
            RefreshMainRoute();
        }

        private void SelectRouteNode(int index)
        {
            if (_runtimeData == null || index < 0 || index >= _runtimeData.AvailableNodes.Count)
            {
                return;
            }

            _selectedNode = _runtimeData.AvailableNodes[index];
            Debug.Log($"[CultivationFlow] MainRoute select node: {_selectedNode.Type}");

            switch (_selectedNode.Type)
            {
                case CultivationRouteNodeType.Battle:
                    EnterBattle();
                    break;
                case CultivationRouteNodeType.Event:
                    EnterEvent();
                    break;
                case CultivationRouteNodeType.Rest:
                    _runtimeData.Hp = Math.Min(CultivationRouteRuntimeData.MaxHp, _runtimeData.Hp + 12);
                    EnterReward("调息完成，恢复 12 气血。", false);
                    break;
                case CultivationRouteNodeType.Shop:
                    _runtimeData.SpiritStones += 5;
                    EnterReward("坊市占位：获得 5 灵石。", false);
                    break;
                case CultivationRouteNodeType.Boss:
                    EnterBattle();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void EnterEvent()
        {
            Transition(CultivationFlowState.Event, "Event");
            _detail = $"{_selectedNode.Label}：{_selectedNode.Description}\n选项 1：获得 20 灵石\n选项 2：恢复 18 气血";
            Debug.Log("[CultivationFlow] Event start: " + _selectedNode.Description);
            RefreshMainRoute();
        }

        private void ChooseEventOption(int optionIndex)
        {
            if (_state != CultivationFlowState.Event)
            {
                return;
            }

            if (optionIndex == 0)
            {
                _runtimeData.SpiritStones += 20;
                Debug.Log("[CultivationFlow] Event selected: SpiritStones");
                EnterReward("奇遇奖励：获得 20 灵石。", false);
                return;
            }

            _runtimeData.Hp = Math.Min(CultivationRouteRuntimeData.MaxHp, _runtimeData.Hp + 18);
            Debug.Log("[CultivationFlow] Event selected: Heal");
            EnterReward("奇遇奖励：恢复 18 气血。", false);
        }

        private void EnterBattle()
        {
            _battlePlayerHp = _runtimeData.Hp;
            _battleEnemyHp = _selectedNode != null && _selectedNode.Type == CultivationRouteNodeType.Boss ? 44 : 30;
            Transition(CultivationFlowState.Battle, "Battle");
            _detail = $"战斗开始：玩家 HP {_battlePlayerHp} / 敌人 HP {_battleEnemyHp}。按 Space 推进回合。";
            Debug.Log("[CultivationFlow] Battle start");
            RefreshMainRoute();
        }

        private void AdvanceBattleTurn()
        {
            if (_state != CultivationFlowState.Battle)
            {
                return;
            }

            _battleEnemyHp = Math.Max(0, _battleEnemyHp - 14);
            if (_battleEnemyHp > 0)
            {
                _battlePlayerHp = Math.Max(0, _battlePlayerHp - 6);
            }

            _runtimeData.Hp = Math.Max(1, _battlePlayerHp);
            Debug.Log($"[CultivationFlow] Battle turn: playerHp={_battlePlayerHp} enemyHp={_battleEnemyHp}");

            if (_battleEnemyHp <= 0)
            {
                Debug.Log("[CultivationFlow] Battle win");
                EnterReward("战斗胜利，选择 1/2/3 领取奖励。", true);
                return;
            }

            if (_battlePlayerHp <= 0)
            {
                Debug.Log("[CultivationFlow] Battle lose");
                _runtimeData.Hp = 1;
                EnterMainRoute("战斗失败占位：保留 1 气血返回路线，避免流程卡死。");
                return;
            }

            _detail = $"战斗中：玩家 HP {_battlePlayerHp} / 敌人 HP {_battleEnemyHp}。按 Space 继续。";
            RefreshMainRoute();
        }

        private void EnterReward(string detail, bool fromBattle)
        {
            _rewardFromBattle = fromBattle;
            _detail = detail;
            Transition(CultivationFlowState.Reward, "Reward");
            RefreshMainRoute();
        }

        private void ChooseReward(int rewardIndex)
        {
            if (_state != CultivationFlowState.Reward)
            {
                return;
            }

            switch (rewardIndex)
            {
                case 0:
                    _runtimeData.SpiritStones += _rewardFromBattle ? 18 : 8;
                    Debug.Log("[CultivationFlow] Reward selected: SpiritStones");
                    break;
                case 1:
                    var cardName = _rewardFromBattle ? "破云剑式" : "灵泉护体";
                    _runtimeData.Deck.Add(cardName);
                    Debug.Log("[CultivationFlow] Reward selected: Card");
                    break;
                case 2:
                    _runtimeData.Hp = Math.Min(CultivationRouteRuntimeData.MaxHp, _runtimeData.Hp + 16);
                    Debug.Log("[CultivationFlow] Reward selected: Heal");
                    break;
                default:
                    return;
            }

            _runtimeData.AdvanceAfterReward();
            Debug.Log("[CultivationFlow] Reward -> MainRoute");
            EnterMainRoute($"已领取奖励，推进到第 {_runtimeData.Layer} 层第 {_runtimeData.Day} 天，节点 {_runtimeData.NodeIndex + 1}。");
        }

        private void RefreshMainRoute()
        {
            if (_runtimeData == null)
            {
                return;
            }

            _mainRouteBinder.Bind(CultivationRouteViewModelFactory.Create(_runtimeData, _state, _detail));
            _stateText.text = $"Cultivation Flow / {_state}";
            _hintText.text = BuildHintText();
            _detailText.text = _detail ?? string.Empty;
            _optionsText.text = BuildOptionsText();
        }

        private string BuildHintText()
        {
            switch (_state)
            {
                case CultivationFlowState.MainRoute:
                    return "按 1 / 2 / 3 选择路线节点。";
                case CultivationFlowState.Event:
                    return "按 1 获得灵石，按 2 恢复气血。";
                case CultivationFlowState.Battle:
                    return "按 Space 推进一回合模拟战斗。";
                case CultivationFlowState.Reward:
                    return "按 1 获得灵石，按 2 获得卡牌，按 3 恢复气血。";
                case CultivationFlowState.Finished:
                    return "按 Space 回到路线。";
                default:
                    return "等待流程启动。";
            }
        }

        private string BuildOptionsText()
        {
            if (_runtimeData == null)
            {
                return string.Empty;
            }

            if (_state == CultivationFlowState.MainRoute)
            {
                var text = "当前可选节点：";
                for (var i = 0; i < _runtimeData.AvailableNodes.Count; i++)
                {
                    var node = _runtimeData.AvailableNodes[i];
                    text += $"\n{i + 1}. {node.Label} / {node.Type} / {node.Description}";
                }

                return text;
            }

            if (_state == CultivationFlowState.Reward)
            {
                return "奖励：\n1. 获得灵石\n2. 获得卡牌\n3. 恢复气血";
            }

            if (_state == CultivationFlowState.Event)
            {
                return "事件选项：\n1. 获得灵石\n2. 恢复气血";
            }

            if (_state == CultivationFlowState.Battle)
            {
                return $"模拟战斗：玩家 HP {_battlePlayerHp} / 敌人 HP {_battleEnemyHp}";
            }

            return string.Empty;
        }

        private void Transition(CultivationFlowState next, string label)
        {
            if (_state != next)
            {
                Debug.Log($"[CultivationFlow] {_state} -> {label}");
            }

            _state = next;
        }

        private static GameObject CreateMainRouteInstance(Transform parent)
        {
#if UNITY_EDITOR
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(MainRoutePrefabPath);
            if (prefab != null)
            {
                var instance = (GameObject)Object.Instantiate(prefab, parent, false);
                instance.name = "Steam_MainRoute_Restore_Runtime";
                var rect = instance.transform as RectTransform;
                if (rect != null)
                {
                    Stretch(rect);
                }

                return instance;
            }
#endif

            return CreateFallbackMainRoute(parent);
        }

        private static Transform ResolveVisibleParent()
        {
            var parent = CultivationRunUIService.ResolveMainRunParent();
            if (parent != null)
            {
                return parent;
            }

            var canvasObject = new GameObject("RuntimePrototypeCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var rect = (RectTransform)canvasObject.transform;
            Stretch(rect);

            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            Object.DontDestroyOnLoad(canvasObject);
            return rect;
        }

        private static GameObject CreateFallbackMainRoute(Transform parent)
        {
            var root = new GameObject("Steam_MainRoute_RuntimeFallback", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var rect = (RectTransform)root.transform;
            rect.SetParent(parent, false);
            Stretch(rect);

            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.overrideSorting = true;

            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            var background = root.AddComponent<Image>();
            background.color = new Color(0.035f, 0.050f, 0.045f, 1f);

            CreateNamedText(root.transform, "Top_Brand", new Vector2(42f, -26f), new Vector2(320f, 42f), 28, FontStyle.Bold);
            CreateNamedText(root.transform, "Top_Location", new Vector2(420f, -28f), new Vector2(700f, 40f), 20, FontStyle.Normal);
            CreateNamedText(root.transform, "Top_气血", new Vector2(1180f, -28f), new Vector2(190f, 38f), 20, FontStyle.Normal);
            CreateNamedText(root.transform, "Top_灵力", new Vector2(1380f, -28f), new Vector2(190f, 38f), 20, FontStyle.Normal);
            CreateNamedText(root.transform, "Top_灵石", new Vector2(1580f, -28f), new Vector2(190f, 38f), 20, FontStyle.Normal);
            CreateNamedText(root.transform, "Left_Name", new Vector2(42f, -140f), new Vector2(260f, 42f), 26, FontStyle.Bold);
            CreateNamedText(root.transform, "Left_RealmValue", new Vector2(42f, -186f), new Vector2(260f, 36f), 20, FontStyle.Normal);
            CreateNamedText(root.transform, "Left_Value_气血", new Vector2(42f, -240f), new Vector2(260f, 32f), 18, FontStyle.Normal);
            CreateNamedText(root.transform, "Left_Value_灵力", new Vector2(42f, -280f), new Vector2(260f, 32f), 18, FontStyle.Normal);
            CreateNamedText(root.transform, "Bottom_PrimaryLabel", new Vector2(780f, -940f), new Vector2(520f, 54f), 24, FontStyle.Bold);
            CreateNamedText(root.transform, "Right_Title", new Vector2(1500f, -140f), new Vector2(320f, 42f), 24, FontStyle.Bold);
            CreateNamedText(root.transform, "Right_CardTitle_清风剑诀", new Vector2(1480f, -210f), new Vector2(320f, 36f), 20, FontStyle.Bold);
            CreateNamedText(root.transform, "Right_CardTitle_幻影步", new Vector2(1480f, -340f), new Vector2(320f, 36f), 20, FontStyle.Bold);
            CreateNamedText(root.transform, "Right_CardTitle_混元归一诀", new Vector2(1480f, -470f), new Vector2(320f, 36f), 20, FontStyle.Bold);
            CreateNamedText(root.transform, "Right_Cost_清风剑诀", new Vector2(1810f, -210f), new Vector2(60f, 36f), 20, FontStyle.Bold);
            CreateNamedText(root.transform, "Right_Cost_幻影步", new Vector2(1810f, -340f), new Vector2(60f, 36f), 20, FontStyle.Bold);
            CreateNamedText(root.transform, "Right_Cost_混元归一诀", new Vector2(1810f, -470f), new Vector2(60f, 36f), 20, FontStyle.Bold);

            return root;
        }

        private static GameObject CreateOverlay(Transform parent)
        {
            var overlay = new GameObject("CultivationFlowOverlay", typeof(RectTransform), typeof(Image));
            var rect = (RectTransform)overlay.transform;
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.02f, 0.05f);
            rect.anchorMax = new Vector2(0.40f, 0.42f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var image = overlay.GetComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.68f);
            return overlay;
        }

        private static Text CreateText(string name, Transform parent, int fontSize, FontStyle style, TextAnchor anchor)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            var text = go.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.alignment = anchor;
            text.color = new Color(0.92f, 0.96f, 0.88f, 1f);
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private static void CreateNamedText(Transform parent, string name, Vector2 position, Vector2 size, int fontSize, FontStyle style)
        {
            var text = CreateText(name, parent, fontSize, style, TextAnchor.MiddleLeft);
            var rect = text.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void LayoutText(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
            rect.localPosition = Vector3.zero;
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
    }
}
