using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace StardewSubtitles.Patches;

public class PatchGenerator
{
    private delegate void PatchDelegate();
    
    public static void PrefixPostfixPatch(Harmony harmony, MethodInfo original, string cueId, string subtitleId)
    {
        harmony.Patch(
            original: original,
            prefix: new HarmonyMethod(PrefixFactory(cueId, subtitleId)),
            postfix: new HarmonyMethod(PostfixFactory(cueId, subtitleId))
        );
    }

    private static MethodInfo PrefixFactory(string cueId, string subtitleId)
    {
        var helper = AccessTools.Method(typeof(PatchGenerator), nameof(PrefixHelper));
        
        var dm = new DynamicMethod("DynamicPrefix", typeof(void), Array.Empty<Type>());
        var generator = dm.GetILGenerator();
        generator.Emit(OpCodes.Ldstr, cueId);
        generator.Emit(OpCodes.Ldstr, subtitleId);
        generator.Emit(OpCodes.Call, helper);

        return dm;
    }

    private static void PrefixHelper(string cueId, string subtitleId)
    {
        ModEntry._subtitleManager.RegisterSubtitleForNextCue(cueId, subtitleId);
    }
    
    private static MethodInfo PostfixFactory(string cueId, string subtitleId)
    {
        var helper = AccessTools.Method(typeof(PatchGenerator), nameof(PostfixHelper));
        
        var dm = new DynamicMethod("DynamicPostfix", typeof(void), Array.Empty<Type>());
        var generator = dm.GetILGenerator();
        generator.Emit(OpCodes.Ldstr, cueId);
        generator.Emit(OpCodes.Ldstr, subtitleId);
        generator.Emit(OpCodes.Call, helper);
        
        

        return dm;
    }

    private static void PostfixHelper(string cueId, string subtitleId)
    {
        ModEntry._subtitleManager.UnRegisterSubtitleForNextCue(cueId, subtitleId);
    }
}