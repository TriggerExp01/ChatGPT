using GameLogic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace GameLogic.Cultivation
{
    public static class CultivationRunUIService
    {
        public static CultivationRunPrototypeUI OpenMainRunUI(Transform parent = null)
        {
            return CultivationRunPrototypeUI.Open(parent ?? ResolveMainRunParent());
        }

        public static bool IsMainRunUIOpen(Transform parent = null)
        {
            return FindMainRunUI(parent) != null;
        }

        public static bool CloseMainRunUI(Transform parent = null)
        {
            var ui = FindMainRunUI(parent);
            if (ui == null)
            {
                return false;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(ui.gameObject);
            }
            else
            {
                Object.DestroyImmediate(ui.gameObject);
            }

            return true;
        }

        public static Transform ResolveMainRunParent()
        {
            if (UIModule.UIRoot != null)
            {
                return UIModule.UIRoot;
            }

            var uiRoot = GameObject.Find("UIRoot");
            if (uiRoot != null)
            {
                var canvas = uiRoot.GetComponentInChildren<Canvas>();
                if (canvas != null)
                {
                    return canvas.transform;
                }
            }

            var fallback = GameObject.Find("RuntimePrototypeCanvas");
            if (fallback != null)
            {
                return fallback.transform;
            }

            return null;
        }

        private static CultivationRunPrototypeUI FindMainRunUI(Transform parent)
        {
            if (parent != null)
            {
                var child = parent.Find(CultivationRunPrototypeUI.RootName);
                return child != null ? child.GetComponent<CultivationRunPrototypeUI>() : null;
            }

            var resolvedParent = ResolveMainRunParent();
            if (resolvedParent != null)
            {
                var child = resolvedParent.Find(CultivationRunPrototypeUI.RootName);
                if (child != null)
                {
                    return child.GetComponent<CultivationRunPrototypeUI>();
                }
            }

            return Object.FindObjectOfType<CultivationRunPrototypeUI>();
        }
    }
}
