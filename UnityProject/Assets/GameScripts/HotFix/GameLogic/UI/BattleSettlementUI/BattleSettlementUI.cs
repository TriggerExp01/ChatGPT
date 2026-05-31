using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    [Window(UILayer.Top, location: "BattleSettlementUI")]
    class BattleSettlementUI : UIWindow
    {
        private Text _summaryText;
        private Button _restartButton;

        protected override void ScriptGenerator()
        {
            RectTransform root = RoguelikeUIFactory.ResolveContainer(this);
            RectTransform panel = RoguelikeUIFactory.CreatePanel("SettlementPanel", root, new Vector2(0.24f, 0.32f), new Vector2(0.76f, 0.60f), new Color(0.02f, 0.025f, 0.03f, 0.90f));
            _summaryText = RoguelikeUIFactory.CreateText("Summary", panel, 22, TextAnchor.MiddleCenter, new Vector2(0.06f, 0.32f), new Vector2(0.94f, 0.92f), Vector2.zero, Vector2.zero);
            _restartButton = RoguelikeUIFactory.CreateButton("Restart", panel, "重新开始", new Vector2(0.35f, 0.08f), new Vector2(0.65f, 0.26f), new Color(0.35f, 0.18f, 0.18f, 0.96f), 20);
        }

        protected override void OnCreate()
        {
            _restartButton.onClick.AddListener(OnRestart);
            RefreshSettlement();
        }

        protected override void OnDestroy()
        {
            _restartButton.onClick.RemoveListener(OnRestart);
        }

        protected override void OnUpdate()
        {
            RefreshSettlement();
        }

        protected override void OnRefresh()
        {
            RefreshSettlement();
        }

        private void RefreshSettlement()
        {
            bool ended = RoguelikeGame.Instance.Phase == RoguelikeGamePhase.Defeated ||
                         RoguelikeGame.Instance.Phase == RoguelikeGamePhase.Victory;
            gameObject.SetActive(ended);
            if (ended)
            {
                RoguelikeUIFactory.SetText(_summaryText, RoguelikeGame.Instance.SettlementSummary);
            }
        }

        private void OnRestart()
        {
            RoguelikeGame.Instance.StartNewRun();
            RefreshSettlement();
        }
    }
}
