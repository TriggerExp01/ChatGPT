using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace GameLogic
{
    internal static class RoguelikeUIFactory
    {
        public const string PanelFrameSprite = "Roguelike_UI_PanelFrame";
        public const string CardFrameSprite = "Roguelike_UI_CardFrame";
        public const string HeroPortraitSprite = "Roguelike_UI_HeroPortrait";
        public const string WeaponIconSprite = "Roguelike_UI_WeaponIcon";
        public const string RelicIconSprite = "Roguelike_UI_RelicIcon";
        public const string GoldIconSprite = "Roguelike_UI_GoldIcon";
        public const string MagicBoltIconSprite = "Roguelike_UI_Icon_MagicBolt";
        public const string SpinningBladeIconSprite = "Roguelike_UI_Icon_SpinningBlade";
        public const string PiercingDartIconSprite = "Roguelike_UI_Icon_PiercingDart";
        public const string StarRingPulseIconSprite = "Roguelike_UI_Icon_StarRingPulse";
        public const string RelicPowerIconSprite = "Roguelike_UI_Icon_RelicPower";
        public const string RelicGrowthIconSprite = "Roguelike_UI_Icon_RelicGrowth";
        public const string RelicUtilityIconSprite = "Roguelike_UI_Icon_RelicUtility";
        public const string PaidSupplyIconSprite = "Roguelike_UI_Icon_PaidSupply";
        public const string BattleLightSprite = "Roguelike_UI_LightBand";
        public const string SliderFrameSprite = "Roguelike_UI_SliderFrame";
        public const string SliderFillRedSprite = "Roguelike_UI_SliderFill_Red";
        public const string SliderFillBlueSprite = "Roguelike_UI_SliderFill_Blue";
        public const string SliderFillYellowSprite = "Roguelike_UI_SliderFill_Yellow";

        private static readonly Dictionary<string, Sprite> SpriteCache = new Dictionary<string, Sprite>();

        public static RectTransform ResolveContainer(UIWindow window)
        {
            RectTransform container = window.FindChildComponent<RectTransform>("m_rectContainer");
            return container != null ? container : window.rectTransform;
        }

        public static RectTransform CreateRect(string name, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            GameObject go = new GameObject(name, typeof(RectTransform));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            go.layer = parent.gameObject.layer;
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            rect.localScale = Vector3.one;
            return rect;
        }

        public static RectTransform CreatePanel(string name, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            RectTransform rect = CreateRect(name, parent, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            ApplySprite(image, PanelFrameSprite, Image.Type.Sliced);
            image.raycastTarget = false;
            return rect;
        }

        public static RectTransform CreateImage(string name, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax, Color color)
        {
            return CreateImage(name, parent, anchorMin, anchorMax, color, null, Image.Type.Simple, false);
        }

        public static RectTransform CreateImage(string name, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax, Color color, string spriteAddress, Image.Type imageType = Image.Type.Simple, bool preserveAspect = false)
        {
            RectTransform rect = CreateRect(name, parent, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            if (!string.IsNullOrEmpty(spriteAddress))
            {
                ApplySprite(image, spriteAddress, imageType, preserveAspect);
            }

            return rect;
        }

        public static Text CreateText(string name, RectTransform parent, int size, TextAnchor alignment, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            RectTransform rect = CreateRect(name, parent, anchorMin, anchorMax, offsetMin, offsetMax);
            Text text = rect.gameObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.alignment = alignment;
            text.color = Color.white;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        public static Button CreateButton(string name, RectTransform parent, string label, Vector2 anchorMin, Vector2 anchorMax, Color color, int fontSize = 22)
        {
            return CreateButton(name, parent, label, anchorMin, anchorMax, color, fontSize, PanelFrameSprite);
        }

        public static Button CreateButton(string name, RectTransform parent, string label, Vector2 anchorMin, Vector2 anchorMax, Color color, int fontSize, string spriteAddress)
        {
            RectTransform rect = CreateRect(name, parent, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            ApplySprite(image, spriteAddress, Image.Type.Sliced);
            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.10f, 1.06f, 1.16f, 1f);
            colors.pressedColor = new Color(0.78f, 0.80f, 0.94f, 1f);
            colors.disabledColor = new Color(0.42f, 0.42f, 0.42f, 0.65f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;
            Text text = CreateText("Label", rect, fontSize, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(8f, 0f), new Vector2(-8f, 0f));
            text.text = label;
            text.raycastTarget = false;
            return button;
        }

        public static Slider CreateSlider(string name, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax, Color fillColor)
        {
            return CreateSlider(name, parent, anchorMin, anchorMax, fillColor, null);
        }

        public static Slider CreateSlider(string name, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax, Color fillColor, string fillSpriteAddress)
        {
            RectTransform root = CreateRect(name, parent, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
            Image bg = root.gameObject.AddComponent<Image>();
            bg.color = new Color(0.88f, 0.92f, 1f, 0.86f);
            ApplySprite(bg, SliderFrameSprite, Image.Type.Sliced);

            RectTransform fillArea = CreateRect("Fill Area", root, Vector2.zero, Vector2.one, new Vector2(6f, 4f), new Vector2(-6f, -4f));
            RectTransform fill = CreateRect("Fill", fillArea, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image fillImage = fill.gameObject.AddComponent<Image>();
            fillImage.color = fillColor;
            ApplySprite(fillImage, fillSpriteAddress, Image.Type.Sliced);

            Slider slider = root.gameObject.AddComponent<Slider>();
            slider.transition = Selectable.Transition.None;
            slider.fillRect = fill;
            slider.targetGraphic = bg;
            slider.direction = Slider.Direction.LeftToRight;
            return slider;
        }

        public static void SetText(Text text, string value)
        {
            if (text != null)
            {
                text.text = value;
            }
        }

        public static void SetSlider(Slider slider, int value, int maxValue)
        {
            if (slider == null)
            {
                return;
            }

            slider.minValue = 0;
            slider.maxValue = Mathf.Max(1, maxValue);
            slider.value = Mathf.Clamp(value, 0, maxValue);
        }

        public static void ApplySprite(RectTransform rect, string spriteAddress, Image.Type imageType = Image.Type.Simple, bool preserveAspect = false)
        {
            if (rect == null)
            {
                return;
            }

            ApplySprite(rect.GetComponent<Image>(), spriteAddress, imageType, preserveAspect);
        }

        public static void ApplySprite(Image image, string spriteAddress, Image.Type imageType = Image.Type.Simple, bool preserveAspect = false)
        {
            if (image == null || string.IsNullOrEmpty(spriteAddress))
            {
                return;
            }

            Sprite sprite = LoadSprite(spriteAddress);
            if (sprite == null)
            {
                return;
            }

            image.sprite = sprite;
            image.type = imageType;
            image.preserveAspect = preserveAspect;
        }

        public static string ResolveRewardIconSprite(string rewardId)
        {
            if (string.IsNullOrEmpty(rewardId))
            {
                return WeaponIconSprite;
            }

            if (rewardId.Contains("spinning_blade"))
            {
                return SpinningBladeIconSprite;
            }

            if (rewardId.Contains("piercing_dart"))
            {
                return PiercingDartIconSprite;
            }

            if (rewardId.Contains("star_ring_pulse"))
            {
                return StarRingPulseIconSprite;
            }

            if (rewardId.Contains("magic_bolt") || rewardId.Contains("weapon"))
            {
                return MagicBoltIconSprite;
            }

            if (rewardId.Contains("paid"))
            {
                return PaidSupplyIconSprite;
            }

            if (rewardId.Contains("vital_sigil") || rewardId.Contains("starlight_soles"))
            {
                return RelicGrowthIconSprite;
            }

            if (rewardId.Contains("eagle_eye") || rewardId.Contains("focus") || rewardId.Contains("power"))
            {
                return RelicPowerIconSprite;
            }

            if (rewardId.Contains("passive"))
            {
                return RelicUtilityIconSprite;
            }

            return WeaponIconSprite;
        }

        public static string ResolveWeaponIconSprite(RoguelikeWeaponType weaponType)
        {
            switch (weaponType)
            {
                case RoguelikeWeaponType.MagicBolt:
                    return MagicBoltIconSprite;
                case RoguelikeWeaponType.SpinningBlade:
                    return SpinningBladeIconSprite;
                case RoguelikeWeaponType.PiercingDart:
                    return PiercingDartIconSprite;
                case RoguelikeWeaponType.StarRingPulse:
                    return StarRingPulseIconSprite;
                default:
                    return WeaponIconSprite;
            }
        }

        public static string ResolveRelicIconSprite(string relicId)
        {
            if (string.IsNullOrEmpty(relicId))
            {
                return RelicIconSprite;
            }

            if (relicId.Contains("vital_sigil") || relicId.Contains("starlight_soles") || relicId.Contains("wind_boots"))
            {
                return RelicGrowthIconSprite;
            }

            if (relicId.Contains("eagle_eye") || relicId.Contains("focus") || relicId.Contains("power"))
            {
                return RelicPowerIconSprite;
            }

            return RelicUtilityIconSprite;
        }

        public static Sprite LoadSprite(string address)
        {
            if (string.IsNullOrEmpty(address))
            {
                return null;
            }

            if (SpriteCache.TryGetValue(address, out Sprite cached))
            {
                return cached;
            }

            Sprite sprite = null;
            try
            {
                TEngine.IResourceModule resource = GameModule.Resource;
                if (resource != null && resource.CheckLocationValid(address))
                {
                    sprite = resource.LoadAsset<Sprite>(address);
                }
            }
            catch (System.Exception)
            {
                sprite = null;
            }

#if UNITY_EDITOR
            if (sprite == null)
            {
                sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/AssetRaw/UIRaw/Atlas/Roguelike/{address}.png");
            }

            if (sprite == null)
            {
                sprite = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>($"Assets/AssetRaw/UIRaw/Atlas/Battle/{address}.png");
            }
#endif

            if (sprite != null)
            {
                SpriteCache[address] = sprite;
            }

            return sprite;
        }
    }
}
