using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GameLogic.Cultivation
{
    public static class CultivationUIAssetCatalog
    {
        public const string Root = "Assets/AssetRaw/UIRaw/Single/CultivationCardGame";
        public const string Manifest = Root + "/ui_asset_manifest.json";

        public const string CombatQiRefiningBackground = Root + "/Backgrounds/bg_combat_qi_refining_placeholder.png";
        public const string DarkPanel = Root + "/Panels/ui_panel_dark.png";
        public const string GoldPanel = Root + "/Panels/ui_panel_gold.png";
        public const string ButtonNormal = Root + "/Buttons/ui_button_normal.png";
        public const string ButtonHover = Root + "/Buttons/ui_button_hover.png";
        public const string ButtonPressed = Root + "/Buttons/ui_button_pressed.png";
        public const string ButtonDisabled = Root + "/Buttons/ui_button_disabled.png";
        public const string CardFrameCommon = Root + "/Cards/ui_card_frame_common.png";
        public const string CardFrameSpirit = Root + "/Cards/ui_card_frame_spirit.png";
        public const string CardFrameImmortal = Root + "/Cards/ui_card_frame_immortal.png";
        public const string CardFrameSaint = Root + "/Cards/ui_card_frame_saint.png";
        public const string CardFrameDao = Root + "/Cards/ui_card_frame_dao.png";
        public const string SpiritGemIcon = Root + "/Icons/ui_icon_spirit_gem.png";
        public const string ShieldIcon = Root + "/Icons/ui_icon_shield.png";
        public const string ThunderIcon = Root + "/Icons/ui_icon_thunder.png";
        public const string CounterIcon = Root + "/Icons/ui_icon_counter.png";
        public const string IntentAttack = Root + "/Intents/ui_intent_attack.png";
        public const string IntentDefend = Root + "/Intents/ui_intent_defend.png";
        public const string IntentStun = Root + "/Intents/ui_intent_stun.png";
        public const string NodeBattle = Root + "/MapNodes/ui_node_battle.png";
        public const string NodeElite = Root + "/MapNodes/ui_node_elite.png";
        public const string NodeMarket = Root + "/MapNodes/ui_node_market.png";
        public const string NodeRest = Root + "/MapNodes/ui_node_rest.png";
        public const string NodeBoss = Root + "/MapNodes/ui_node_boss.png";

        public static readonly string[] RequiredSpritePaths =
        {
            CombatQiRefiningBackground,
            DarkPanel,
            GoldPanel,
            ButtonNormal,
            ButtonHover,
            ButtonPressed,
            ButtonDisabled,
            CardFrameCommon,
            CardFrameSpirit,
            CardFrameImmortal,
            CardFrameSaint,
            CardFrameDao,
            SpiritGemIcon,
            ShieldIcon,
            ThunderIcon,
            CounterIcon,
            IntentAttack,
            IntentDefend,
            IntentStun,
            NodeBattle,
            NodeElite,
            NodeMarket,
            NodeRest,
            NodeBoss,
        };

        public static Sprite LoadEditorSprite(string path)
        {
#if UNITY_EDITOR
            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
#else
            return null;
#endif
        }
    }
}
