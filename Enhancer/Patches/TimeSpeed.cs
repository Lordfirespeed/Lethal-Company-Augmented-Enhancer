using HarmonyLib;

namespace Enhancer.Patches;

public class TimeSpeed
{
    [HarmonyPatch(typeof(TimeOfDay), "Start")]
    [HarmonyPostfix]
    public static void TimeOfDayOnStartPost(TimeOfDay __instance)
    {
        Plugin.Log.LogInfo($"Multiplying time speed by {Plugin.BoundConfig.TimeSpeed.Value}...");
        __instance.globalTimeSpeedMultiplier *= Plugin.BoundConfig.TimeSpeed.Value;
    }
}