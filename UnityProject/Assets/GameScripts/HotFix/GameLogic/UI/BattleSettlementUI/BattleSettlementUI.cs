using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    [Window(UILayer.Top, location: "BattleSettlementUI")]
    class BattleSettlementUI : UIWindow
    {
        private Text _titleText;
        private Text _resultText;
        private Text _statsText;
        private Text _earningsText;
        private Text _growthText;
        private Text _hintText;
        private Image _progressFill;
        private Button _restartButton;
        private RectTransform _panel;
        private RectTransform _portrait;

        protected override void ScriptGenerator()
        {
            RectTransform root = RoguelikeUIFactory.ResolveContainer(this);
            _panel = RoguelikeUIFactory.CreatePanel("结算成长面板", root, new Vector2(0.12f, 0.14f), new Vector2(0.88f, 0.84f), new Color(0.024f, 0.028f, 0.052f, 0.97f));
            RoguelikeUIFactory.CreateImage("结算星辉装饰", _panel, new Vector2(0.04f, 0.86f), new Vector2(0.14f, 0.96f), new Color(1f, 0.72f, 0.92f, 0.72f));
            RoguelikeUIFactory.CreateImage("结算月辉装饰", _panel, new Vector2(0.86f, 0.86f), new Vector2(0.96f, 0.96f), new Color(0.52f, 0.82f, 1f, 0.66f));

            _portrait = RoguelikeUIFactory.CreateImage("角色剪影区", _panel, new Vector2(0.06f, 0.18f), new Vector2(0.30f, 0.78f), new Color(0.12f, 0.15f, 0.24f, 0.94f));
            RoguelikeUIFactory.CreateImage("角色剪影高光", _portrait, new Vector2(0.24f, 0.50f), new Vector2(0.76f, 0.86f), new Color(1f, 0.80f, 0.48f, 0.62f));
            RoguelikeUIFactory.CreateImage("角色剪影身体", _portrait, new Vector2(0.18f, 0.10f), new Vector2(0.82f, 0.58f), new Color(0.74f, 0.52f, 0.98f, 0.62f));

            _titleText = RoguelikeUIFactory.CreateText("结算标题", _panel, 30, TextAnchor.MiddleCenter, new Vector2(0.32f, 0.78f), new Vector2(0.84f, 0.93f), Vector2.zero, Vector2.zero);
            _resultText = RoguelikeUIFactory.CreateText("结算结果", _panel, 18, TextAnchor.MiddleCenter, new Vector2(0.32f, 0.69f), new Vector2(0.84f, 0.78f), Vector2.zero, Vector2.zero);
            _statsText = RoguelikeUIFactory.CreateText("结算数据", _panel, 17, TextAnchor.MiddleLeft, new Vector2(0.34f, 0.52f), new Vector2(0.62f, 0.67f), Vector2.zero, Vector2.zero);
            _earningsText = RoguelikeUIFactory.CreateText("结算收益", _panel, 17, TextAnchor.MiddleLeft, new Vector2(0.64f, 0.52f), new Vector2(0.84f, 0.67f), Vector2.zero, Vector2.zero);
            _growthText = RoguelikeUIFactory.CreateText("永久成长", _panel, 17, TextAnchor.MiddleLeft, new Vector2(0.34f, 0.35f), new Vector2(0.84f, 0.48f), Vector2.zero, Vector2.zero);

            RectTransform progressRoot = RoguelikeUIFactory.CreatePanel("永久成长进度条", _panel, new Vector2(0.34f, 0.29f), new Vector2(0.84f, 0.34f), new Color(0.07f, 0.08f, 0.12f, 1f));
            RectTransform progressFill = RoguelikeUIFactory.CreateImage("永久成长进度填充", progressRoot, Vector2.zero, new Vector2(0.01f, 1f), new Color(1f, 0.74f, 0.30f, 0.96f));
            _progressFill = progressFill.GetComponent<Image>();

            _hintText = RoguelikeUIFactory.CreateText("结算提示", _panel, 15, TextAnchor.MiddleCenter, new Vector2(0.34f, 0.19f), new Vector2(0.84f, 0.27f), Vector2.zero, Vector2.zero);
            _restartButton = RoguelikeUIFactory.CreateButton("重新开始按钮", _panel, "使用永久成长重新开始", new Vector2(0.38f, 0.08f), new Vector2(0.78f, 0.17f), new Color(0.42f, 0.20f, 0.36f, 0.96f), 18);
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
                RoguelikeGame game = RoguelikeGame.Instance;
                int runGold = game.CurrentRun?.Gold ?? 0;
                bool victory = game.Phase == RoguelikeGamePhase.Victory;
                RoguelikeUIFactory.SetText(_titleText, game.SettlementTitle);
                RoguelikeUIFactory.SetText(_resultText, victory ? "地牢之心已沉寂，本局奖励已入账。" : "旅人倒下了，但星辉会留到下一局。");
                RoguelikeUIFactory.SetText(_statsText, $"战斗数据\n生存 {game.ElapsedTime:0.0} 秒\n等级 Lv.{game.Level}\n击杀 {game.KillCount}");
                RoguelikeUIFactory.SetText(_earningsText, $"本局收益\n本局金币 +{runGold}\n永久金币 {game.MetaGold}");
                RoguelikeUIFactory.SetText(_growthText, $"局外成长\n下局初始攻击 +{game.PermanentAttackBonus}\n距离下一点攻击还需 {game.PermanentGoldToNextAttack} 金币");
                RoguelikeUIFactory.SetText(_hintText, game.OperationHint);
                RefreshProgress(game.PermanentGoldProgress);
            }
        }

        private void RefreshProgress(int progress)
        {
            if (_progressFill == null)
            {
                return;
            }

            RectTransform rect = _progressFill.rectTransform;
            float normalized = Mathf.Clamp01(progress / (float)RoguelikeGame.MetaGoldPerAttackBonus);
            rect.anchorMax = new Vector2(Mathf.Max(0.01f, normalized), 1f);
        }

        private void OnRestart()
        {
            RoguelikeGame.Instance.PlayUiConfirmSound();
            RoguelikeGame.Instance.StartNewRun();
            RefreshSettlement();
        }
    }
}
