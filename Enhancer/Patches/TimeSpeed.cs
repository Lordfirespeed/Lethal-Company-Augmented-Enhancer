using BepInEx.Logging;
using HarmonyLib;

namespace Enhancer.Patches;

public class TimeSpeed : IPatch
{
    protected static ManualLogSource Logger { get; set; } = null!;

    public void SetLogger(ManualLogSource logger)
    {
        logger.LogDebug("Logger assigned.");
        Logger = logger;
    }

    [HarmonyPatch(typeof(TimeOfDay), "Start")]
    [HarmonyPostfix]
    public static void TimeOfDayOnStartPost(TimeOfDay __instance)
    {
        Logger.LogInfo($"Multiplying time speed by {Plugin.BoundConfig.TimeSpeed.Value}...");
        __instance.globalTimeSpeedMultiplier *= Plugin.BoundConfig.TimeSpeed.Value;
    }
}