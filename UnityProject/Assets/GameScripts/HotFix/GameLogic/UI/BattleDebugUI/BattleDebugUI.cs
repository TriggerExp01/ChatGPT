using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    [Window(UILayer.Top, location: "BattleDebugUI")]
    class BattleDebugUI : UIWindow
    {
        private Button _skipButton;
        private Button _goldButton;
        private Button _relicButton;
        private Button _winButton;
        private Button _loseButton;
        private Button _smokeButton;

        protected override void ScriptGenerator()
        {
            RectTransform root = RoguelikeUIFactory.ResolveContainer(this);
            RectTransform panel = RoguelikeUIFactory.CreatePanel("DebugPanel", root, new Vector2(0.03f, 0.52f), new Vector2(0.12f, 0.72f), new Color(0.02f, 0.025f, 0.03f, 0.58f));
            _skipButton = RoguelikeUIFactory.CreateButton("Skip", panel, "跳房", new Vector2(0.08f, 0.70f), new Vector2(0.92f, 0.96f), new Color(0.18f, 0.18f, 0.18f, 0.96f), 16);
            _goldButton = RoguelikeUIFactory.CreateButton("Gold", panel, "+金", new Vector2(0.08f, 0.52f), new Vector2(0.92f, 0.68f), new Color(0.18f, 0.18f, 0.18f, 0.96f), 16);
            _relicButton = RoguelikeUIFactory.CreateButton("Relic", panel, "+遗", new Vector2(0.08f, 0.34f), new Vector2(0.92f, 0.50f), new Color(0.18f, 0.18f, 0.18f, 0.96f), 16);
            _winButton = RoguelikeUIFactory.CreateButton("Win", panel, "胜", new Vector2(0.08f, 0.18f), new Vector2(0.48f, 0.32f), new Color(0.22f, 0.30f, 0.18f, 0.96f), 15);
            _loseButton = RoguelikeUIFactory.CreateButton("Lose", panel, "败", new Vector2(0.52f, 0.18f), new Vector2(0.92f, 0.32f), new Color(0.30f, 0.18f, 0.18f, 0.96f), 15);
            _smokeButton = RoguelikeUIFactory.CreateButton("Smoke", panel, "冒烟", new Vector2(0.08f, 0.02f), new Vector2(0.92f, 0.16f), new Color(0.25f, 0.25f, 0.16f, 0.96f), 15);
        }

        protected override void OnCreate()
        {
            _skipButton.onClick.AddListener(OnSkip);
            _goldButton.onClick.AddListener(OnGold);
            _relicButton.onClick.AddListener(OnRelic);
            _winButton.onClick.AddListener(OnWin);
            _loseButton.onClick.AddListener(OnLose);
            _smokeButton.onClick.AddListener(OnSmoke);
        }

        protected override void OnDestroy()
        {
            _skipButton.onClick.RemoveListener(OnSkip);
            _goldButton.onClick.RemoveListener(OnGold);
            _relicButton.onClick.RemoveListener(OnRelic);
            _winButton.onClick.RemoveListener(OnWin);
            _loseButton.onClick.RemoveListener(OnLose);
            _smokeButton.onClick.RemoveListener(OnSmoke);
        }

        private void OnSkip()
        {
            RoguelikeGame.Instance.DebugSkipRoom();
        }

        private void OnGold()
        {
            RoguelikeGame.Instance.DebugAddGold(50);
        }

        private void OnRelic()
        {
            RoguelikeGame.Instance.DebugAddRandomRelic();
        }

        private void OnWin()
        {
            RoguelikeGame.Instance.DebugForceVictory();
        }

        private void OnLose()
        {
            RoguelikeGame.Instance.DebugForceDefeat();
        }

        private void OnSmoke()
        {
            RoguelikeGame.Instance.RunSmokeSimulation(20);
        }
    }
}
