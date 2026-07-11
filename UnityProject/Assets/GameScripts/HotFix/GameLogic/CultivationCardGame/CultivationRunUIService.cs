using GameLogic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace GameLogic.Cultivation
{
    public static class CultivationRunUIService
    {
        public static CultivationRunPrototypeUI OpenGameRun(Transform parent = null)
        {
            return parent != null ? OpenMainRunUI(parent) : OpenMainRunWindowOrFallback();
        }

        public static CultivationRunPrototypeUI OpenMainRunUI(Transform parent = null)
        {
            return CultivationRunPrototypeUI.Open(parent ?? ResolveMainRunParent());
        }

        public static CultivationRunPrototypeUI OpenMainRunWindowOrFallback()
        {
            return TryOpenMainRunWindow(out var ui) ? ui : OpenMainRunUI();
        }

        public static bool TryOpenMainRunWindow(out CultivationRunPrototypeUI ui)
        {
            ui = null;

            if (!Application.isPlaying)
            {
                return false;
            }

            if (UIModule.UIRoot == null && GameObject.Find("UIRoot") == null)
            {
                return false;
            }

            var uiModule = GameModule.UI;
            if (UIModule.UIRoot == null)
            {
                return false;
            }

            EnsureMainRunWindowResourceLoader();
            uiModule.ShowUI<CultivationRunWindow>();
            ui = FindMainRunUI(UIModule.UIRoot);
            return ui != null;
        }

        public static void EnsureMainRunWindowResourceLoader()
        {
            if (UIModule.Resource is CultivationRunWindowResourceLoader)
            {
                return;
            }

            UIModule.Resource = new CultivationRunWindowResourceLoader(UIModule.Resource);
        }

        public static bool IsMainRunUIOpen(Transform parent = null)
        {
            return FindMainRunUI(parent) != null;
        }

        public static bool CloseMainRunUI(Transform parent = null)
        {
            if (parent == null && CloseMainRunWindow())
            {
                return true;
            }

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

        public static bool CloseMainRunWindow()
        {
            if (!UIModule.IsValid || !GameModule.UI.HasWindow<CultivationRunWindow>())
            {
                return false;
            }

            GameModule.UI.CloseUI<CultivationRunWindow>();
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
                if (child != null)
                {
                    return child.GetComponent<CultivationRunPrototypeUI>();
                }

                return parent.GetComponentInChildren<CultivationRunPrototypeUI>(true);
            }

            var resolvedParent = ResolveMainRunParent();
            if (resolvedParent != null)
            {
                var child = resolvedParent.Find(CultivationRunPrototypeUI.RootName);
                if (child != null)
                {
                    return child.GetComponent<CultivationRunPrototypeUI>();
                }

                var nested = resolvedParent.GetComponentInChildren<CultivationRunPrototypeUI>(true);
                if (nested != null)
                {
                    return nested;
                }
            }

            return Object.FindObjectOfType<CultivationRunPrototypeUI>();
        }
    }
}
