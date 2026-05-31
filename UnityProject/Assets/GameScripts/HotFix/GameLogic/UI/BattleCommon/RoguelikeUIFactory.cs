using UnityEngine;
using UnityEngine.UI;

namespace GameLogic
{
    internal static class RoguelikeUIFactory
    {
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
            image.raycastTarget = false;
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
            RectTransform rect = CreateRect(name, parent, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
            Image image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            Button button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            Text text = CreateText("Label", rect, fontSize, TextAnchor.MiddleCenter, Vector2.zero, Vector2.one, new Vector2(8f, 6f), new Vector2(-8f, -6f));
            text.text = label;
            text.raycastTarget = false;
            return button;
        }

        public static Slider CreateSlider(string name, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax, Color fillColor)
        {
            RectTransform root = CreateRect(name, parent, anchorMin, anchorMax, Vector2.zero, Vector2.zero);
            Image bg = root.gameObject.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.08f, 0.10f, 1f);

            RectTransform fillArea = CreateRect("Fill Area", root, Vector2.zero, Vector2.one, new Vector2(5f, 4f), new Vector2(-5f, -4f));
            RectTransform fill = CreateRect("Fill", fillArea, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image fillImage = fill.gameObject.AddComponent<Image>();
            fillImage.color = fillColor;

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
    }
}
