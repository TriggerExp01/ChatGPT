using System.Collections.Generic;
using System.Reflection;
using GameLogic;
using GameLogic.Cultivation;
#if ENABLE_OBFUZ
using Obfuz;
#endif
using TEngine;
#pragma warning disable CS0436


/// <summary>
/// 游戏App。
/// </summary>
#if ENABLE_OBFUZ
[ObfuzIgnore(ObfuzScope.TypeName | ObfuzScope.MethodName)]
#endif
public partial class GameApp
{
    private static List<Assembly> _hotfixAssembly;

    /// <summary>
    /// 热更域App主入口。
    /// </summary>
    /// <param name="objects"></param>
    public static void Entrance(object[] objects)
    {
        GameEventHelper.Init();
        _hotfixAssembly = (List<Assembly>)objects[0];
        Log.Warning("======= 看到此条日志代表你成功运行了热更新代码 =======");
        Log.Warning("======= Entrance GameApp =======");
        Utility.Unity.AddDestroyListener(Release);
        Log.Warning("======= StartGameLogic =======");
        StartGameLogic();
    }
    
    private static void StartGameLogic()
    {
        var engine = new BattleEngine(20260619);
        var state = engine.CreateBattle(CultivationSeedData.CreateSwordSectStarterDeck(), CultivationSeedData.StoneDemon);
        var enemy = state.Enemies[0];
        Log.Warning(
            $"======= Cultivation Phase 1 battle core ready: hand={state.Hand.Count}, spirit={state.Spirit}/{state.SpiritMax}, enemy={enemy.Body.Name}, intent={enemy.CurrentIntent.Description} =======");
    }
    
    private static void Release()
    {
        SingletonSystem.Release();
        Log.Warning("======= Release GameApp =======");
    }
}
