using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    [Window(UILayer.UI, location: "BattleHudUI")]
    class BattleHudUI : UIWindow
    {
        private Text _titleText;
        private Text _statsText;
        private Text _messageText;
        private Slider _hpSlider;
        private Slider _expSlider;
        private RoguelikeBattleStageView _stageView;

        protected override void ScriptGenerator()
        {
            RectTransform root = RoguelikeUIFactory.ResolveContainer(this);
            RectTransform panel = RoguelikeUIFactory.CreatePanel("生存信息", root, new Vector2(0.02f, 0.80f), new Vector2(0.98f, 0.98f), new Color(0.02f, 0.035f, 0.055f, 0.94f));
            _titleText = RoguelikeUIFactory.CreateText("标题", panel, 25, TextAnchor.UpperCenter, new Vector2(0.25f, 0.60f), new Vector2(0.75f, 0.96f), Vector2.zero, Vector2.zero);
            _statsText = RoguelikeUIFactory.CreateText("属性", panel, 18, TextAnchor.UpperLeft, new Vector2(0.02f, 0.12f), new Vector2(0.48f, 0.92f), Vector2.zero, Vector2.zero);
            _messageText = RoguelikeUIFactory.CreateText("提示", panel, 18, TextAnchor.UpperRight, new Vector2(0.52f, 0.12f), new Vector2(0.98f, 0.92f), Vector2.zero, Vector2.zero);
            _hpSlider = RoguelikeUIFactory.CreateSlider("生命", panel, new Vector2(0.02f, 0.05f), new Vector2(0.47f, 0.14f), new Color(0.20f, 0.85f, 0.34f, 1f));
            _expSlider = RoguelikeUIFactory.CreateSlider("经验", panel, new Vector2(0.53f, 0.05f), new Vector2(0.98f, 0.14f), new Color(0.24f, 0.62f, 1f, 1f));
        }

        protected override void OnCreate()
        {
            _stageView = RoguelikeBattleStageView.Ensure();
        }

        protected override void OnUpdate()
        {
            RoguelikeGame.Instance.Tick(Time.deltaTime);
            RefreshBattleInfo();
        }

        protected override void OnRefresh()
        {
            RefreshBattleInfo();
        }

        private void RefreshBattleInfo()
        {
            RoguelikeGame game = RoguelikeGame.Instance;
            RoguelikeRunState run = game.CurrentRun;
            if (run == null)
            {
                return;
            }

            _stageView.Refresh(run);
            RoguelikeActorState player = run.Player;
            RoguelikeUIFactory.SetText(_titleText, $"2D 生存肉鸽　{game.ElapsedTime:0.0} 秒　敌人 {game.Enemies.Count}　掉落 {game.Pickups.Count}");
            RoguelikeUIFactory.SetText(_statsText, $"等级 {game.Level}　击杀 {game.KillCount}　金币 {run.Gold}　永久金币 {game.MetaGold}\n生命 {player.Health}/{player.Stats.MaxHealth}　攻击 {player.Stats.Attack}　移动 {game.MoveSpeed:0.0}");
            RoguelikeUIFactory.SetText(_messageText, $"{game.LastMessage}\n武器 {game.WeaponSummary}\n范围 {game.AttackRange:0.0}　攻击间隔 {game.AttackInterval:0.00} 秒　经验 {game.Experience}/{game.ExperienceToNextLevel}");
            RoguelikeUIFactory.SetSlider(_hpSlider, player.Health, player.Stats.MaxHealth);
            RoguelikeUIFactory.SetSlider(_expSlider, game.Experience, game.ExperienceToNextLevel);
        }
    }
}
