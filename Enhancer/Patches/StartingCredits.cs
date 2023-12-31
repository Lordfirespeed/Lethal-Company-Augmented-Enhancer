using HarmonyLib;

namespace Enhancer.Patches;

public class StartingCredits : IPatch
{
    [HarmonyPatch(typeof(Terminal), "Start")]
    [HarmonyPrefix]
    public static void TerminalStartPre()
    {
        Plugin.Logger.LogInfo($"Setting starting credits to {Plugin.BoundConfig.StartingCredits.Value}...");
        var quotaSettings = TimeOfDay.Instance.quotaVariables;
        quotaSettings.startingCredits = Plugin.BoundConfig.StartingCredits.Value;
    }
}