using GameLogic.Cultivation;
using TEngine;
using UnityEngine;

public class GameEntry : MonoBehaviour
{
    [SerializeField]
    private bool useCultivationPrototypeInEditor = true;

#if UNITY_EDITOR
    internal bool UseCultivationPrototypeInEditor => useCultivationPrototypeInEditor;
#endif

    void Awake()
    {
#if UNITY_EDITOR
        if (useCultivationPrototypeInEditor)
        {
            CultivationRunUIService.OpenMinimalGameplayLoop();
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
            CultivationRunUIService.OpenMinimalGameplayLoop();
        }
    }
#endif
}
