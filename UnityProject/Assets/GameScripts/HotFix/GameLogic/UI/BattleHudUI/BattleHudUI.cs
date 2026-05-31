using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    [Window(UILayer.UI, location: "BattleHudUI")]
    class BattleHudUI : UIWindow
    {
        private Text _titleText;
        private Text _playerText;
        private Text _enemyText;
        private Text _messageText;
        private Text _routeText;
        private Slider _playerHpSlider;
        private Slider _enemyHpSlider;
        private RoguelikeBattleStageView _stageView;

        protected override void ScriptGenerator()
        {
            RectTransform root = RoguelikeUIFactory.ResolveContainer(this);
            RectTransform top = RoguelikeUIFactory.CreatePanel("TopHud", root, new Vector2(0.03f, 0.74f), new Vector2(0.97f, 0.97f), new Color(0.02f, 0.025f, 0.03f, 0.72f));
            _titleText = RoguelikeUIFactory.CreateText("Title", top, 26, TextAnchor.MiddleCenter, new Vector2(0.22f, 0.62f), new Vector2(0.78f, 0.98f), Vector2.zero, Vector2.zero);
            _playerText = RoguelikeUIFactory.CreateText("Player", top, 20, TextAnchor.UpperLeft, new Vector2(0.02f, 0.24f), new Vector2(0.32f, 0.94f), Vector2.zero, Vector2.zero);
            _enemyText = RoguelikeUIFactory.CreateText("Enemy", top, 20, TextAnchor.UpperRight, new Vector2(0.68f, 0.24f), new Vector2(0.98f, 0.94f), Vector2.zero, Vector2.zero);
            _messageText = RoguelikeUIFactory.CreateText("Message", top, 21, TextAnchor.MiddleCenter, new Vector2(0.30f, 0.18f), new Vector2(0.70f, 0.56f), Vector2.zero, Vector2.zero);
            _playerHpSlider = RoguelikeUIFactory.CreateSlider("PlayerHp", top, new Vector2(0.02f, 0.08f), new Vector2(0.30f, 0.18f), new Color(0.20f, 0.85f, 0.34f, 1f));
            _enemyHpSlider = RoguelikeUIFactory.CreateSlider("EnemyHp", top, new Vector2(0.70f, 0.08f), new Vector2(0.98f, 0.18f), new Color(0.95f, 0.24f, 0.18f, 1f));
            _routeText = RoguelikeUIFactory.CreateText("Route", root, 18, TextAnchor.UpperCenter, new Vector2(0.08f, 0.62f), new Vector2(0.92f, 0.72f), Vector2.zero, Vector2.zero);
        }

        protected override void OnCreate()
        {
            _stageView = RoguelikeBattleStageView.Ensure();
            RefreshBattleInfo();
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
            RoguelikeRunState run = RoguelikeGame.Instance.CurrentRun;
            if (run == null)
            {
                return;
            }

            _stageView.Refresh(run);
            RoguelikeRoom room = run.CurrentRoom;
            RoguelikeActorState player = run.Player;
            RoguelikeUIFactory.SetText(_titleText, $"轻操作肉鸽 - {RoguelikeText.GetPhaseName(RoguelikeGame.Instance.Phase)}");
            RoguelikeUIFactory.SetText(_playerText, $"角色\n生命 {player.Health}/{player.Stats.MaxHealth}\n攻击 {player.Stats.Attack}  防御 {player.Stats.Defense}\n金币 {run.Gold}  遗物 {run.Relics.Count}");
            RoguelikeUIFactory.SetSlider(_playerHpSlider, player.Health, player.Stats.MaxHealth);

            if (room != null && room.Enemy != null)
            {
                float dist = Mathf.Abs(RoguelikeGame.Instance.EnemyLanePosition - RoguelikeGame.Instance.PlayerLanePosition);
                RoguelikeUIFactory.SetText(_enemyText, $"{room.Enemy.DisplayName}\n生命 {room.Enemy.Health}/{room.Enemy.Stats.MaxHealth}\n房间 {room.Index + 1}/{run.Rooms.Count}  {RoguelikeText.GetRoomName(room.Type)}\n距离 {dist:0.00}");
                RoguelikeUIFactory.SetSlider(_enemyHpSlider, room.Enemy.Health, room.Enemy.Stats.MaxHealth);
            }
            else if (room != null)
            {
                RoguelikeUIFactory.SetText(_enemyText, $"{RoguelikeText.GetRoomName(room.Type)}\n房间 {room.Index + 1}/{run.Rooms.Count}\n事件房间");
                RoguelikeUIFactory.SetSlider(_enemyHpSlider, 1, 1);
            }
            else
            {
                RoguelikeUIFactory.SetText(_enemyText, "无房间");
                RoguelikeUIFactory.SetSlider(_enemyHpSlider, 0, 1);
            }

            RoguelikeUIFactory.SetText(_messageText, RoguelikeGame.Instance.LastMessage);
            RoguelikeUIFactory.SetText(_routeText, BuildRouteText(run));
        }

        private static string BuildRouteText(RoguelikeRunState run)
        {
            StringBuilder builder = new StringBuilder(128);
            for (int i = 0; i < run.Rooms.Count; i++)
            {
                if (i > 0)
                {
                    builder.Append("  >  ");
                }

                if (i == run.CurrentRoomIndex)
                {
                    builder.Append("[");
                }

                builder.Append(RoguelikeText.GetRoomName(run.Rooms[i].Type));

                if (i == run.CurrentRoomIndex)
                {
                    builder.Append("]");
                }
            }

            return builder.ToString();
        }
    }
}
