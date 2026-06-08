using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    [Window(UILayer.UI, location: "BattleHudUI")]
    class BattleHudUI : UIWindow
    {
        private Text _heroText;
        private Text _timeText;
        private Text _buildText;
        private Text _messageText;
        private Slider _hpSlider;
        private Slider _expSlider;
        private RectTransform _heroPanel;
        private RectTransform _pressurePanel;
        private RectTransform _buildPanel;
        private RectTransform _hintPanel;
        private RoguelikeBattleStageView _stageView;
        private readonly Image[] _weaponSlotImages = new Image[4];
        private readonly Image[] _relicSlotImages = new Image[4];

        protected override void ScriptGenerator()
        {
            RectTransform root = RoguelikeUIFactory.ResolveContainer(this);
            _heroPanel = RoguelikeUIFactory.CreatePanel("角色状态面板", root, new Vector2(0.018f, 0.79f), new Vector2(0.34f, 0.975f), new Color(0.045f, 0.052f, 0.085f, 0.88f));
            RoguelikeUIFactory.CreateImage("角色头像", _heroPanel, new Vector2(0.035f, 0.22f), new Vector2(0.22f, 0.88f), Color.white, RoguelikeUIFactory.HeroPortraitSprite, Image.Type.Simple, true);
            RoguelikeUIFactory.CreateImage("头像星辉", _heroPanel, new Vector2(0.015f, 0.70f), new Vector2(0.24f, 0.98f), new Color(1f, 0.76f, 0.46f, 0.46f), RoguelikeUIFactory.BattleLightSprite, Image.Type.Simple, true);
            _heroText = RoguelikeUIFactory.CreateText("角色数值", _heroPanel, 17, TextAnchor.UpperLeft, new Vector2(0.26f, 0.40f), new Vector2(0.96f, 0.92f), Vector2.zero, Vector2.zero);
            _hpSlider = RoguelikeUIFactory.CreateSlider("生命条", _heroPanel, new Vector2(0.26f, 0.24f), new Vector2(0.96f, 0.35f), Color.white, RoguelikeUIFactory.SliderFillRedSprite);
            _expSlider = RoguelikeUIFactory.CreateSlider("经验条", _heroPanel, new Vector2(0.26f, 0.10f), new Vector2(0.96f, 0.20f), Color.white, RoguelikeUIFactory.SliderFillBlueSprite);

            _pressurePanel = RoguelikeUIFactory.CreatePanel("时间压力面板", root, new Vector2(0.39f, 0.885f), new Vector2(0.61f, 0.975f), new Color(0.05f, 0.04f, 0.075f, 0.82f));
            RoguelikeUIFactory.CreateImage("时间压力高光", _pressurePanel, new Vector2(0.12f, 0.58f), new Vector2(0.88f, 0.98f), new Color(1f, 0.78f, 0.26f, 0.26f), RoguelikeUIFactory.BattleLightSprite, Image.Type.Simple, true);
            _timeText = RoguelikeUIFactory.CreateText("时间压力", _pressurePanel, 20, TextAnchor.MiddleCenter, new Vector2(0.04f, 0.10f), new Vector2(0.96f, 0.90f), Vector2.zero, Vector2.zero);

            _buildPanel = RoguelikeUIFactory.CreatePanel("构筑槽面板", root, new Vector2(0.66f, 0.79f), new Vector2(0.982f, 0.975f), new Color(0.045f, 0.052f, 0.085f, 0.88f));
            BuildIconSlots(_buildPanel, "武器槽", 0.70f, new Color(1f, 0.78f, 0.34f, 0.94f), RoguelikeUIFactory.WeaponIconSprite);
            BuildIconSlots(_buildPanel, "被动槽", 0.36f, new Color(0.55f, 0.88f, 1f, 0.94f), RoguelikeUIFactory.RelicIconSprite);
            _buildText = RoguelikeUIFactory.CreateText("构筑摘要", _buildPanel, 14, TextAnchor.UpperLeft, new Vector2(0.44f, 0.08f), new Vector2(0.96f, 0.92f), Vector2.zero, Vector2.zero);

            _hintPanel = RoguelikeUIFactory.CreatePanel("战斗提示面板", root, new Vector2(0.25f, 0.025f), new Vector2(0.75f, 0.10f), new Color(0.03f, 0.035f, 0.06f, 0.72f));
            RoguelikeUIFactory.CreateImage("战斗提示光带", _hintPanel, new Vector2(0.02f, 0.46f), new Vector2(0.18f, 0.92f), new Color(1f, 0.78f, 0.38f, 0.34f), RoguelikeUIFactory.BattleLightSprite, Image.Type.Simple, true);
            _messageText = RoguelikeUIFactory.CreateText("战斗提示", _hintPanel, 16, TextAnchor.MiddleCenter, new Vector2(0.04f, 0.08f), new Vector2(0.96f, 0.92f), Vector2.zero, Vector2.zero);
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
            ApplyShowcaseScreenshotMode(game);
            RefreshBuildIcons(game);
            RoguelikeActorState player = run.Player;
            RoguelikeUIFactory.SetText(_heroText, $"星辉旅人  Lv.{game.Level}\n生命 {player.Health}/{player.Stats.MaxHealth}  攻击 {player.Stats.Attack}\n金币 {run.Gold}  永久 {game.MetaGold}");
            RoguelikeUIFactory.SetText(_timeText, $"{game.ElapsedTime:0.0}s\n敌人 {game.Enemies.Count}  击杀 {game.KillCount}");
            RoguelikeUIFactory.SetText(_buildText, $"武器  {game.WeaponSummary}\n被动  {game.PassiveSummary}\n范围 {game.AttackRange:0.0}  间隔 {game.AttackInterval:0.00}s");
            RoguelikeUIFactory.SetText(_messageText, game.ShouldShowOperationHintUi ? $"{game.LastMessage}\n{game.OperationHint}" : game.LastMessage);
            RoguelikeUIFactory.SetSlider(_hpSlider, player.Health, player.Stats.MaxHealth);
            RoguelikeUIFactory.SetSlider(_expSlider, game.Experience, game.ExperienceToNextLevel);
        }

        private void ApplyShowcaseScreenshotMode(RoguelikeGame game)
        {
            SetPanelAlpha(_heroPanel, game.CurrentHudPanelAlpha);
            SetPanelAlpha(_pressurePanel, game.CurrentHudPanelAlpha);
            SetPanelAlpha(_buildPanel, game.CurrentHudPanelAlpha);
            _hintPanel.gameObject.SetActive(game.ShouldShowOperationHintUi);
        }

        private static void SetPanelAlpha(RectTransform panel, float alpha)
        {
            CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = panel.gameObject.AddComponent<CanvasGroup>();
            }

            canvasGroup.alpha = alpha;
        }

        private void RefreshBuildIcons(RoguelikeGame game)
        {
            for (int i = 0; i < _weaponSlotImages.Length; i++)
            {
                string spriteAddress = i < game.Weapons.Count
                    ? RoguelikeUIFactory.ResolveWeaponIconSprite(game.Weapons[i].Type)
                    : RoguelikeUIFactory.WeaponIconSprite;
                RoguelikeUIFactory.ApplySprite(_weaponSlotImages[i], spriteAddress, Image.Type.Simple, true);
            }

            RoguelikeRunState run = game.CurrentRun;
            for (int i = 0; i < _relicSlotImages.Length; i++)
            {
                string spriteAddress = run != null && i < run.Relics.Count
                    ? RoguelikeUIFactory.ResolveRelicIconSprite(run.Relics[i].Id)
                    : RoguelikeUIFactory.RelicIconSprite;
                RoguelikeUIFactory.ApplySprite(_relicSlotImages[i], spriteAddress, Image.Type.Simple, true);
            }
        }

        private void BuildIconSlots(RectTransform parent, string prefix, float yCenter, Color color, string spriteAddress)
        {
            for (int i = 0; i < 4; i++)
            {
                float xMin = 0.05f + i * 0.09f;
                RectTransform slot = RoguelikeUIFactory.CreateImage($"{prefix}_{i + 1}", parent, new Vector2(xMin, yCenter - 0.10f), new Vector2(xMin + 0.065f, yCenter + 0.10f), color, spriteAddress, Image.Type.Simple, true);
                Image image = slot.GetComponent<Image>();
                if (prefix == "武器槽")
                {
                    _weaponSlotImages[i] = image;
                }
                else
                {
                    _relicSlotImages[i] = image;
                }
            }
        }
    }
}
