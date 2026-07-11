using GameLogic.Cultivation;
using TEngine;
using UnityEngine;

public class GameEntry : MonoBehaviour
{
    [SerializeField]
    private bool useCultivationPrototypeInEditor = true;

#if UNITY_EDITOR
    private CultivationRunPrototypeUI _cultivationRunUI;

    internal bool UseCultivationPrototypeInEditor => useCultivationPrototypeInEditor;
#endif

    void Awake()
    {
#if UNITY_EDITOR
        if (useCultivationPrototypeInEditor)
        {
            OpenCultivationPrototypeIfNeeded();
            DontDestroyOnLoad(this);
            return;
        }
#endif

        ModuleSystem.GetModule<IUpdateDriver>();
        ModuleSystem.GetModule<IResourceModule>();
        ModuleSystem.GetModule<IDebuggerModule>();
        ModuleSystem.GetModule<IFsmModule>();
        Settings.ProcedureSetting.StartProcedure().Forget();
        DontDestroyOnLoad(this);
    }

#if UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void OpenCultivationPrototypeInEditor()
    {
        var entry = FindObjectOfType<GameEntry>();
        if (entry != null && entry.UseCultivationPrototypeInEditor)
        {
            entry.OpenCultivationPrototypeIfNeeded();
        }
    }

    private void OpenCultivationPrototypeIfNeeded()
    {
        if (_cultivationRunUI != null)
        {
            return;
        }

        _cultivationRunUI = CultivationRunUIService.OpenGameRun();
    }
#endif
}
