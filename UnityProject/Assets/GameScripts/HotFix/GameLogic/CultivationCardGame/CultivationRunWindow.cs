namespace GameLogic.Cultivation
{
    [Window(UILayer.UI, true, ResourceLocation, true, 0)]
    public sealed class CultivationRunWindow : UIWindow
    {
        public const string AssetLocation = "CultivationRunWindow";
        public const string ResourceLocation = "UIWindow/CultivationRunWindow";

        private CultivationRunPrototypeUI _prototypeUI;

        public CultivationRunPrototypeUI PrototypeUI => _prototypeUI;

        public RunPrototypeSnapshot Snapshot => _prototypeUI != null ? _prototypeUI.Snapshot : null;

        protected override void OnCreate()
        {
            _prototypeUI = CultivationRunPrototypeUI.Open(rectTransform);
        }

        protected override void OnRefresh()
        {
            if (_prototypeUI == null)
            {
                _prototypeUI = CultivationRunPrototypeUI.Open(rectTransform);
            }
        }

        protected override void OnDestroy()
        {
            _prototypeUI = null;
        }
    }
}
