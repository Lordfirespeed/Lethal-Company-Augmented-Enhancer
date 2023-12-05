using HarmonyLib;

namespace Enhancer.Patches;

public class TimeSpeed
{
    [HarmonyPatch(typeof(TimeOfDay), "Start")]
    [HarmonyPostfix]
    public static void TimeOfDayOnStartPost(TimeOfDay __instance)
    {
        Plugin.Log.LogInfo($"Multiplying time speed by {Plugin.BoundConfig.TimeSpeed}...");
        __instance.globalTimeSpeedMultiplier *= Plugin.BoundConfig.TimeSpeed;
    }
}