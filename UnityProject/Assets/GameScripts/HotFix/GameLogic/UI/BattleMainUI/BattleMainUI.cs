using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    [Window(UILayer.UI, location: "BattleMainUI")]
    class BattleMainUI : UIWindow
    {
        private RectTransform _rectContainer;
        private GameObject _itemTouch;
        private Button _touchButton;
        private Text _titleText;
        private Text _playerText;
        private Text _enemyText;
        private Text _messageText;
        private Text _routeText;
        private Slider _playerHpSlider;
        private Slider _enemyHpSlider;
        private Button[] _choiceButtons;
        private Text[] _choiceTexts;
        private Button _restartButton;
        private Text _restartText;
        private Button _moveLeftButton;
        private Button _moveStopButton;
        private Button _moveRightButton;
        private Button _skillButton;
        private Button _dashButton;
        private float _moveAxis;

        protected override void ScriptGenerator()
        {
            _rectContainer = FindChildComponent<RectTransform>("m_rectContainer");
            _itemTouch = FindChild("m_rectContainer/m_itemTouch").gameObject;
            _touchButton = FindOrCreateTouchButton();
            CreateMvpHud();
        }

        protected override void OnCreate()
        {
            if (_touchButton != null)
            {
                _touchButton.onClick.AddListener(OnTouchBattle);
            }
            BindControlEvents();

            RefreshBattleInfo();
        }

        protected override void OnRefresh()
        {
            RefreshBattleInfo();
        }

        protected override void OnUpdate()
        {
            RoguelikeRunState run = RoguelikeGame.Instance.CurrentRun;
            if (run == null)
            {
                RefreshBattleInfo();
                return;
            }

            if (RoguelikeGame.Instance.Phase == RoguelikeGamePhase.Running)
            {
                RoguelikeGame.Instance.SetMoveInput(_moveAxis);
                RoguelikeGame.Instance.UpdateRealtimeCombat(Time.deltaTime);
            }

            RefreshBattleInfo();
        }

        protected override void OnDestroy()
        {
            if (_touchButton != null)
            {
                _touchButton.onClick.RemoveListener(OnTouchBattle);
            }

            UnbindControlEvents();
        }

        private void OnTouchBattle()
        {
            if (RoguelikeGame.Instance.Phase == RoguelikeGamePhase.Defeated ||
                RoguelikeGame.Instance.Phase == RoguelikeGamePhase.Victory)
            {
                RestartRun();
                return;
            }

            if (RoguelikeGame.Instance.Phase == RoguelikeGamePhase.Running)
            {
                if (RoguelikeGame.Instance.InRealtimeCombat)
                {
                    RoguelikeGame.Instance.RequestSkill();
                }
                else
                {
                    RoguelikeGame.Instance.TickAutoBattle();
                }
                RefreshBattleInfo();
            }
        }

        private void RestartRun()
        {
            RoguelikeGame.Instance.StartNewRun();
            _moveAxis = 0f;
            RefreshBattleInfo();
        }

        private void ChooseReward(int index)
        {
            RoguelikeGame.Instance.ChooseReward(index);
            _moveAxis = 0f;
            RefreshBattleInfo();
        }

        private void RefreshBattleInfo()
        {
            RoguelikeRunState run = RoguelikeGame.Instance.CurrentRun;
            if (run == null)
            {
                return;
            }

            RoguelikeRoom room = run.CurrentRoom;
            RoguelikeActorState player = run.Player;
            SetText(_titleText, $"轻操作肉鸽 - {RoguelikeText.GetPhaseName(RoguelikeGame.Instance.Phase)}");
            SetText(_playerText, $"角色\n生命 {player.Health}/{player.Stats.MaxHealth}\n攻击 {player.Stats.Attack}  防御 {player.Stats.Defense}\n金币 {run.Gold}  遗物 {run.Relics.Count}\n技能CD {RoguelikeGame.Instance.SkillCooldownRemaining:0.0}s  闪避CD {RoguelikeGame.Instance.DashCooldownRemaining:0.0}s");
            SetSlider(_playerHpSlider, player.Health, player.Stats.MaxHealth);

            if (room != null && room.Enemy != null)
            {
                float dist = Mathf.Abs(RoguelikeGame.Instance.EnemyLanePosition - RoguelikeGame.Instance.PlayerLanePosition);
                SetText(_enemyText, $"{room.Enemy.DisplayName}\n生命 {room.Enemy.Health}/{room.Enemy.Stats.MaxHealth}\n房间 {room.Index + 1}/{run.Rooms.Count}  {RoguelikeText.GetRoomName(room.Type)}\n距离 {dist:0.00}  P:{RoguelikeGame.Instance.PlayerLanePosition:0.0} E:{RoguelikeGame.Instance.EnemyLanePosition:0.0}");
                SetSlider(_enemyHpSlider, room.Enemy.Health, room.Enemy.Stats.MaxHealth);
            }
            else if (room != null)
            {
                SetText(_enemyText, $"{RoguelikeText.GetRoomName(room.Type)}\n房间 {room.Index + 1}/{run.Rooms.Count}\n无敌人\n事件房间");
                SetSlider(_enemyHpSlider, 1, 1);
            }
            else
            {
                SetText(_enemyText, "无房间");
                SetSlider(_enemyHpSlider, 0, 1);
            }

            SetText(_messageText, RoguelikeGame.Instance.LastMessage);
            SetText(_routeText, BuildRouteText(run));
            RefreshChoiceButtons();
            RefreshRestartButton();
            RefreshControlButtons();
        }

        private void RefreshChoiceButtons()
        {
            bool choosing = RoguelikeGame.Instance.Phase == RoguelikeGamePhase.RewardChoice;
            for (int i = 0; i < _choiceButtons.Length; i++)
            {
                _choiceButtons[i].gameObject.SetActive(choosing);
                if (!choosing)
                {
                    continue;
                }

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

        private void RefreshRestartButton()
        {
            bool ended = RoguelikeGame.Instance.Phase == RoguelikeGamePhase.Defeated ||
                         RoguelikeGame.Instance.Phase == RoguelikeGamePhase.Victory;
            _restartButton.gameObject.SetActive(ended);
            _restartText.text = "重新开始";
        }

        private void RefreshControlButtons()
        {
            bool running = RoguelikeGame.Instance.Phase == RoguelikeGamePhase.Running;
            bool realtime = running && RoguelikeGame.Instance.InRealtimeCombat;

            _moveLeftButton.gameObject.SetActive(running);
            _moveStopButton.gameObject.SetActive(running);
            _moveRightButton.gameObject.SetActive(running);
            _skillButton.gameObject.SetActive(running);
            _dashButton.gameObject.SetActive(running);

            _moveLeftButton.interactable = realtime;
            _moveStopButton.interactable = realtime;
            _moveRightButton.interactable = realtime;
            _skillButton.interactable = realtime;
            _dashButton.interactable = realtime;
        }

        private void CreateMvpHud()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            RectTransform root = CreateRect("RoguelikeMvpHud", rectTransform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            root.SetAsLastSibling();

            Image bg = root.gameObject.AddComponent<Image>();
            bg.color = new Color(0.02f, 0.025f, 0.03f, 0.92f);
            bg.raycastTarget = false;

            _titleText = CreateText("Title", root, font, 30, TextAnchor.MiddleCenter, new Vector2(0.08f, 0.86f), new Vector2(0.92f, 0.97f), Vector2.zero, Vector2.zero);

            RectTransform playerPanel = CreatePanel("PlayerPanel", root, new Vector2(0.07f, 0.58f), new Vector2(0.42f, 0.82f));
            _playerText = CreateText("PlayerText", playerPanel, font, 24, TextAnchor.UpperLeft, Vector2.zero, Vector2.one, new Vector2(18f, 12f), new Vector2(-18f, -48f));
            _playerHpSlider = CreateSlider("PlayerHp", playerPanel, new Vector2(0.08f, 0.08f), new Vector2(0.92f, 0.22f));

            RectTransform enemyPanel = CreatePanel("EnemyPanel", root, new Vector2(0.58f, 0.58f), new Vector2(0.93f, 0.82f));
            _enemyText = CreateText("EnemyText", enemyPanel, font, 24, TextAnchor.UpperLeft, Vector2.zero, Vector2.one, new Vector2(18f, 12f), new Vector2(-18f, -48f));
            _enemyHpSlider = CreateSlider("EnemyHp", enemyPanel, new Vector2(0.08f, 0.08f), new Vector2(0.92f, 0.22f));

            _messageText = CreateText("Message", root, font, 24, TextAnchor.MiddleCenter, new Vector2(0.08f, 0.45f), new Vector2(0.92f, 0.55f), Vector2.zero, Vector2.zero);
            _routeText = CreateText("Route", root, font, 19, TextAnchor.UpperCenter, new Vector2(0.08f, 0.26f), new Vector2(0.92f, 0.43f), Vector2.zero, Vector2.zero);
            _moveLeftButton = CreateButton("MoveLeft", root, font, new Vector2(0.08f, 0.02f), new Vector2(0.23f, 0.08f), new Color(0.15f, 0.18f, 0.33f, 0.96f));
            _moveLeftButton.GetComponentInChildren<Text>(true).text = "左移";
            _moveStopButton = CreateButton("MoveStop", root, font, new Vector2(0.24f, 0.02f), new Vector2(0.39f, 0.08f), new Color(0.15f, 0.18f, 0.25f, 0.96f));
            _moveStopButton.GetComponentInChildren<Text>(true).text = "停";
            _moveRightButton = CreateButton("MoveRight", root, font, new Vector2(0.40f, 0.02f), new Vector2(0.55f, 0.08f), new Color(0.15f, 0.18f, 0.33f, 0.96f));
            _moveRightButton.GetComponentInChildren<Text>(true).text = "右移";
            _skillButton = CreateButton("SkillButton", root, font, new Vector2(0.63f, 0.02f), new Vector2(0.78f, 0.08f), new Color(0.28f, 0.22f, 0.16f, 0.96f));
            _skillButton.GetComponentInChildren<Text>(true).text = "技能";
            _dashButton = CreateButton("DashButton", root, font, new Vector2(0.79f, 0.02f), new Vector2(0.92f, 0.08f), new Color(0.22f, 0.30f, 0.18f, 0.96f));
            _dashButton.GetComponentInChildren<Text>(true).text = "闪避";

            _choiceButtons = new Button[3];
            _choiceTexts = new Text[3];
            for (int i = 0; i < _choiceButtons.Length; i++)
            {
                int captured = i;
                float xMin = 0.08f + i * 0.29f;
                float xMax = xMin + 0.25f;
                _choiceButtons[i] = CreateButton($"Choice{i + 1}", root, font, new Vector2(xMin, 0.09f), new Vector2(xMax, 0.22f), new Color(0.17f, 0.27f, 0.35f, 0.96f));
                _choiceTexts[i] = _choiceButtons[i].GetComponentInChildren<Text>(true);
                _choiceButtons[i].onClick.AddListener(() => ChooseReward(captured));
            }

            _restartButton = CreateButton("RestartButton", root, font, new Vector2(0.37f, 0.09f), new Vector2(0.63f, 0.20f), new Color(0.35f, 0.18f, 0.18f, 0.96f));
            _restartText = _restartButton.GetComponentInChildren<Text>(true);
            _restartButton.onClick.AddListener(RestartRun);
        }

        private void BindControlEvents()
        {
            _moveLeftButton.onClick.AddListener(OnMoveLeft);
            _moveStopButton.onClick.AddListener(OnMoveStop);
            _moveRightButton.onClick.AddListener(OnMoveRight);
            _skillButton.onClick.AddListener(OnSkill);
            _dashButton.onClick.AddListener(OnDash);
        }

        private void UnbindControlEvents()
        {
            _moveLeftButton.onClick.RemoveListener(OnMoveLeft);
            _moveStopButton.onClick.RemoveListener(OnMoveStop);
            _moveRightButton.onClick.RemoveListener(OnMoveRight);
            _skillButton.onClick.RemoveListener(OnSkill);
            _dashButton.onClick.RemoveListener(OnDash);
        }

        private void OnMoveLeft()
        {
            _moveAxis = -1f;
        }

        private void OnMoveStop()
        {
            _moveAxis = 0f;
        }

        private void OnMoveRight()
        {
            _moveAxis = 1f;
        }

        private void OnSkill()
        {
            RoguelikeGame.Instance.RequestSkill();
        }

        private void OnDash()
        {
            RoguelikeGame.Instance.RequestDash();
        }

        private Button FindOrCreateTouchButton()
        {
            Button button = _itemTouch.GetComponentInChildren<Button>(true);
            if (button != null)
            {
                return button;
            }

            Graphic graphic = _itemTouch.GetComponentInChildren<Graphic>(true);
            GameObject buttonObject = graphic != null ? graphic.gameObject : _itemTouch;
            button = buttonObject.AddComponent<Button>();
            if (graphic != null)
            {
                button.targetGraphic = graphic;
            }

            return button;
        }

        private static RectTransform CreatePanel(string name, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax)
        {
            RectTransform rect = CreateRect(name, parent, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = new Color(0.06f, 0.07f, 0.09f, 0.95f);
            image.raycastTarget = false;
            return rect;
        }

        private static Button CreateButton(string name, RectTransform parent, Font font, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            RectTransform rect = CreateRect(name, parent, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            Text text = CreateText("Label", rect, font, 22, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(8f, 6f), new Vector2(-8f, -6f));
            text.raycastTarget = false;
            return button;
        }

        private static Slider CreateSlider(string name, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax)
        {
            RectTransform root = CreateRect(name, parent, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
            Image bg = root.gameObject.AddComponent<Image>();
            bg.color = new Color(0.12f, 0.12f, 0.14f, 1f);

            RectTransform fillArea = CreateRect("Fill Area", root, Vector2.zero, Vector2.one, new Vector2(6f, 4f), new Vector2(-6f, -4f));
            RectTransform fill = CreateRect("Fill", fillArea, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image fillImage = fill.gameObject.AddComponent<Image>();
            fillImage.color = new Color(0.2f, 0.8f, 0.42f, 1f);

            Slider slider = root.gameObject.AddComponent<Slider>();
            slider.transition = Selectable.Transition.None;
            slider.fillRect = fill;
            slider.targetGraphic = bg;
            slider.direction = Slider.Direction.LeftToRight;
            return slider;
        }

        private static Text CreateText(string name, RectTransform parent, Font font, int size, TextAnchor alignment, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            RectTransform rect = CreateRect(name, parent, anchorMin, anchorMax, offsetMin, offsetMax);
            Text text = rect.gameObject.AddComponent<Text>();
            text.font = font;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private static RectTransform CreateRect(string name, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            rect.localScale = Vector3.one;
            return rect;
        }

        private static void SetText(Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }

        private static void SetSlider(Slider slider, int value, int maxValue)
        {
            if (slider == null)
            {
                return;
            }

            slider.minValue = 0;
            slider.maxValue = Mathf.Max(1, maxValue);
            slider.value = Mathf.Clamp(value, 0, maxValue);
        }

        private static string BuildRouteText(RoguelikeRunState run)
        {
            StringBuilder builder = new StringBuilder(128);
            for (int i = 0; i < run.Rooms.Count; i++)
            {
                RoguelikeRoom room = run.Rooms[i];
                if (i > 0)
                {
                    builder.Append("  >  ");
                }

                if (i == run.CurrentRoomIndex)
                {
                    builder.Append("[");
                }

                builder.Append(RoguelikeText.GetRoomName(room.Type));

                if (i == run.CurrentRoomIndex)
                {
                    builder.Append("]");
                }
            }

            return builder.ToString();
        }

    }
}
