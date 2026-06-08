using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    [Window(UILayer.Top, location: "BattleControlUI")]
    class BattleControlUI : UIWindow
    {
        private Button _pauseButton;
        private Button _restartButton;
        private Text _helpText;
        private RectTransform _panel;

        protected override void ScriptGenerator()
        {
            RectTransform root = RoguelikeUIFactory.ResolveContainer(this);
            _panel = RoguelikeUIFactory.CreatePanel("操作提示", root, new Vector2(0.16f, 0.02f), new Vector2(0.84f, 0.105f), new Color(0.02f, 0.035f, 0.055f, 0.90f));
            RoguelikeUIFactory.CreateImage("操作提示光带", _panel, new Vector2(0.02f, 0.46f), new Vector2(0.18f, 0.92f), new Color(0.52f, 0.86f, 1f, 0.28f), RoguelikeUIFactory.BattleLightSprite, Image.Type.Simple, true);
            _helpText = RoguelikeUIFactory.CreateText("提示", _panel, 17, TextAnchor.MiddleCenter, new Vector2(0.03f, 0.05f), new Vector2(0.71f, 0.95f), Vector2.zero, Vector2.zero);
            _helpText.text = "WASD 移动　武器自动攻击最近敌人　Esc 暂停　R 重开";
            _pauseButton = RoguelikeUIFactory.CreateButton("暂停", _panel, "暂停", new Vector2(0.74f, 0.14f), new Vector2(0.86f, 0.86f), new Color(0.48f, 0.64f, 0.86f, 0.96f), 17, RoguelikeUIFactory.PanelFrameSprite);
            _restartButton = RoguelikeUIFactory.CreateButton("重开", _panel, "重开", new Vector2(0.87f, 0.14f), new Vector2(0.98f, 0.86f), new Color(0.72f, 0.36f, 0.48f, 0.96f), 17, RoguelikeUIFactory.PanelFrameSprite);
        }

        protected override void OnCreate()
        {
            _pauseButton.onClick.AddListener(OnPause);
            _restartButton.onClick.AddListener(OnRestart);
        }

        protected override void OnDestroy()
        {
            _pauseButton.onClick.RemoveListener(OnPause);
            _restartButton.onClick.RemoveListener(OnRestart);
        }

        protected override void OnUpdate()
        {
            HandleInput();
            RefreshControlInfo();
        }

        protected override void OnRefresh()
        {
            RefreshControlInfo();
        }

        private void RefreshControlInfo()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            bool running = game.Phase == RoguelikeGamePhase.Running;
            _panel.gameObject.SetActive(running && game.ShouldShowOperationHintUi);
            RoguelikeUIFactory.SetText(_helpText, game.OperationHint);
            _pauseButton.GetComponentInChildren<Text>(true).text = game.IsPaused ? "继续" : "暂停";
        }

        private static void HandleInput()
        {
            float horizontal = 0f;
            float vertical = 0f;
            if (Input.GetKey(KeyCode.A)) horizontal -= 1f;
            if (Input.GetKey(KeyCode.D)) horizontal += 1f;
            if (Input.GetKey(KeyCode.S)) vertical -= 1f;
            if (Input.GetKey(KeyCode.W)) vertical += 1f;
            RoguelikeGame.Instance.SetMoveInput(new Vector2(horizontal, vertical));

            Vector2 arrowDirection = Vector2.zero;
            if (Input.GetKey(KeyCode.LeftArrow)) arrowDirection.x -= 1f;
            if (Input.GetKey(KeyCode.RightArrow)) arrowDirection.x += 1f;
            if (Input.GetKey(KeyCode.DownArrow)) arrowDirection.y -= 1f;
            if (Input.GetKey(KeyCode.UpArrow)) arrowDirection.y += 1f;
            if (arrowDirection.sqrMagnitude > 0.01f)
            {
                RoguelikeGame.Instance.SetAttackDirection(arrowDirection);
            }
            else if (Camera.main != null)
            {
                Vector3 mouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RoguelikeGame.Instance.SetAttackDirection((Vector2)mouse - RoguelikeGame.Instance.PlayerPosition);
            }

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                RoguelikeGame.Instance.PlayUiConfirmSound();
                RoguelikeGame.Instance.TogglePause();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                RoguelikeGame.Instance.PlayUiConfirmSound();
                RoguelikeGame.Instance.StartNewRun();
            }
        }

        private void OnPause()
        {
            RoguelikeGame.Instance.PlayUiConfirmSound();
            RoguelikeGame.Instance.TogglePause();
        }

        private void OnRestart()
        {
            RoguelikeGame.Instance.PlayUiConfirmSound();
            RoguelikeGame.Instance.StartNewRun();
        }
    }
}
