using BepInEx.Logging;
using HarmonyLib;

namespace Enhancer.Patches;

public class StartingCredits : IPatch
{
    protected static ManualLogSource Logger { get; set; } = null!;

    public void SetLogger(ManualLogSource logger)
    {
        logger.LogDebug("Logger assigned.");
        Logger = logger;
    }
    
    [HarmonyPatch(typeof(Terminal), "Start")]
    [HarmonyPrefix]
    public static void TerminalStartPre()
    {
        Logger.LogInfo($"Setting starting credits to {Plugin.BoundConfig.StartingCredits.Value}...");
        var quotaSettings = TimeOfDay.Instance.quotaVariables;
        quotaSettings.startingCredits = Plugin.BoundConfig.StartingCredits.Value;
    }
}