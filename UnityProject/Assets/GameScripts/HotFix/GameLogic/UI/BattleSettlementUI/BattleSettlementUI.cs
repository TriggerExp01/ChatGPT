using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    [Window(UILayer.Top, location: "BattleSettlementUI")]
    class BattleSettlementUI : UIWindow
    {
        private Text _summaryText;
        private Button _restartButton;
        private RectTransform _panel;

        protected override void ScriptGenerator()
        {
            RectTransform root = RoguelikeUIFactory.ResolveContainer(this);
            _panel = RoguelikeUIFactory.CreatePanel("SettlementPanel", root, new Vector2(0.22f, 0.30f), new Vector2(0.78f, 0.64f), new Color(0.02f, 0.035f, 0.055f, 0.98f));
            _summaryText = RoguelikeUIFactory.CreateText("Summary", _panel, 22, TextAnchor.MiddleCenter, new Vector2(0.06f, 0.32f), new Vector2(0.94f, 0.92f), Vector2.zero, Vector2.zero);
            _restartButton = RoguelikeUIFactory.CreateButton("Restart", _panel, "使用永久成长重新开始", new Vector2(0.25f, 0.08f), new Vector2(0.75f, 0.26f), new Color(0.35f, 0.18f, 0.18f, 0.96f), 18);
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
            _panel.gameObject.SetActive(ended);
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
