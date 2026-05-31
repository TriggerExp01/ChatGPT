using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    [Window(UILayer.Top, location: "BattleControlUI")]
    class BattleControlUI : UIWindow
    {
        private Button _moveLeftButton;
        private Button _moveStopButton;
        private Button _moveRightButton;
        private Button _skillButton;
        private Button _dashButton;
        private Button _pauseButton;
        private Text _cooldownText;

        protected override void ScriptGenerator()
        {
            RectTransform root = RoguelikeUIFactory.ResolveContainer(this);
            RectTransform panel = RoguelikeUIFactory.CreatePanel("ControlPanel", root, new Vector2(0.06f, 0.03f), new Vector2(0.94f, 0.17f), new Color(0.02f, 0.025f, 0.03f, 0.74f));
            _moveLeftButton = RoguelikeUIFactory.CreateButton("MoveLeft", panel, "左移", new Vector2(0.02f, 0.14f), new Vector2(0.16f, 0.86f), new Color(0.15f, 0.18f, 0.33f, 0.96f));
            _moveStopButton = RoguelikeUIFactory.CreateButton("MoveStop", panel, "停", new Vector2(0.18f, 0.14f), new Vector2(0.30f, 0.86f), new Color(0.15f, 0.18f, 0.25f, 0.96f));
            _moveRightButton = RoguelikeUIFactory.CreateButton("MoveRight", panel, "右移", new Vector2(0.32f, 0.14f), new Vector2(0.46f, 0.86f), new Color(0.15f, 0.18f, 0.33f, 0.96f));
            _skillButton = RoguelikeUIFactory.CreateButton("Skill", panel, "技能", new Vector2(0.58f, 0.14f), new Vector2(0.72f, 0.86f), new Color(0.28f, 0.22f, 0.16f, 0.96f));
            _dashButton = RoguelikeUIFactory.CreateButton("Dash", panel, "闪避", new Vector2(0.74f, 0.14f), new Vector2(0.86f, 0.86f), new Color(0.22f, 0.30f, 0.18f, 0.96f));
            _pauseButton = RoguelikeUIFactory.CreateButton("Pause", panel, "暂停", new Vector2(0.88f, 0.14f), new Vector2(0.98f, 0.86f), new Color(0.18f, 0.22f, 0.28f, 0.96f), 18);
            _cooldownText = RoguelikeUIFactory.CreateText("Cooldown", root, 18, TextAnchor.MiddleCenter, new Vector2(0.38f, 0.18f), new Vector2(0.62f, 0.23f), Vector2.zero, Vector2.zero);
        }

        protected override void OnCreate()
        {
            _moveLeftButton.onClick.AddListener(OnMoveLeft);
            _moveStopButton.onClick.AddListener(OnMoveStop);
            _moveRightButton.onClick.AddListener(OnMoveRight);
            _skillButton.onClick.AddListener(OnSkill);
            _dashButton.onClick.AddListener(OnDash);
            _pauseButton.onClick.AddListener(OnPause);
            RefreshControlButtons();
        }

        protected override void OnDestroy()
        {
            _moveLeftButton.onClick.RemoveListener(OnMoveLeft);
            _moveStopButton.onClick.RemoveListener(OnMoveStop);
            _moveRightButton.onClick.RemoveListener(OnMoveRight);
            _skillButton.onClick.RemoveListener(OnSkill);
            _dashButton.onClick.RemoveListener(OnDash);
            _pauseButton.onClick.RemoveListener(OnPause);
        }

        protected override void OnUpdate()
        {
            RefreshControlButtons();
        }

        private void RefreshControlButtons()
        {
            bool running = RoguelikeGame.Instance.Phase == RoguelikeGamePhase.Running;
            bool realtime = running && RoguelikeGame.Instance.InRealtimeCombat;
            gameObject.SetActive(running);
            _moveLeftButton.interactable = realtime;
            _moveStopButton.interactable = realtime;
            _moveRightButton.interactable = realtime;
            _skillButton.interactable = realtime;
            _dashButton.interactable = realtime;
            _pauseButton.interactable = running;
            _pauseButton.GetComponentInChildren<Text>(true).text = RoguelikeGame.Instance.IsPaused ? "继续" : "暂停";
            RoguelikeUIFactory.SetText(_cooldownText, $"技能 {RoguelikeGame.Instance.SkillCooldownRemaining:0.0}s  闪避 {RoguelikeGame.Instance.DashCooldownRemaining:0.0}s");
        }

        private void OnMoveLeft()
        {
            RoguelikeGame.Instance.SetMoveInput(-1f);
        }

        private void OnMoveStop()
        {
            RoguelikeGame.Instance.SetMoveInput(0f);
        }

        private void OnMoveRight()
        {
            RoguelikeGame.Instance.SetMoveInput(1f);
        }

        private void OnSkill()
        {
            RoguelikeGame.Instance.RequestSkill();
        }

        private void OnDash()
        {
            RoguelikeGame.Instance.RequestDash();
        }

        private void OnPause()
        {
            RoguelikeGame.Instance.TogglePause();
            RefreshControlButtons();
        }
    }
}
