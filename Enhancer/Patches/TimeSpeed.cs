using HarmonyLib;

namespace Enhancer.Patches;

public class TimeSpeed : IPatch
{
    [HarmonyPatch(typeof(TimeOfDay), "Start")]
    [HarmonyPostfix]
    public static void TimeOfDayOnStartPost(TimeOfDay __instance)
    {
        Plugin.Logger.LogInfo($"Multiplying time speed by {Plugin.BoundConfig.TimeSpeed.Value}...");
        __instance.globalTimeSpeedMultiplier *= Plugin.BoundConfig.TimeSpeed.Value;
    }
}