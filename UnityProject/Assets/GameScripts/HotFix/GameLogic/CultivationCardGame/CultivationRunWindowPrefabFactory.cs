using UnityEngine;
using UnityEngine.UI;

namespace GameLogic.Cultivation
{
    public static class CultivationRunWindowPrefabFactory
    {
        public static GameObject CreatePanel(Transform parent = null)
        {
            var panel = new GameObject(
                CultivationRunWindow.AssetLocation,
                typeof(RectTransform),
                typeof(Canvas),
                typeof(GraphicRaycaster));

            var rect = (RectTransform)panel.transform;
            if (parent != null)
            {
                rect.SetParent(parent, false);
            }

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
            rect.localPosition = Vector3.zero;

            var canvas = panel.GetComponent<Canvas>();
            canvas.overrideSorting = true;

            return panel;
        }
    }
}
