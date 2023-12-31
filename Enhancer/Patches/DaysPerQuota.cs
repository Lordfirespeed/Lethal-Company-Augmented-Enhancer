using HarmonyLib;

namespace Enhancer.Patches;

public class DaysPerQuota : IPatch
{
    [HarmonyPatch(typeof(StartOfRound), "Start")]
    [HarmonyPrefix]
    public static void StartOfRoundShipStartPre()
    {
        Plugin.Logger.LogInfo($"Setting days per quota to {Plugin.BoundConfig.DaysPerQuotaAssignment.Value}...");
        var quotaSettings = TimeOfDay.Instance.quotaVariables;
        quotaSettings.deadlineDaysAmount = Plugin.BoundConfig.DaysPerQuotaAssignment.Value;
    }
}