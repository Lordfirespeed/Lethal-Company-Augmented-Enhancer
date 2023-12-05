using HarmonyLib;

namespace Enhancer.Patches;

public class DaysPerQuota
{
    [HarmonyPatch(typeof(StartOfRound), "Start")]
    [HarmonyPrefix]
    public static void StartOfRoundShipStartPre()
    {
        Plugin.Log.LogInfo($"Setting days per quota to {Plugin.BoundConfig.DaysPerQuota}...");
        var quotaSettings = TimeOfDay.Instance.quotaVariables;
        quotaSettings.deadlineDaysAmount = Plugin.BoundConfig.DaysPerQuota;
    }
}