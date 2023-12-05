using HarmonyLib;

namespace Enhancer.Patches;

public class StartingQuota
{
    [HarmonyPatch(typeof(StartOfRound), "Start")]
    [HarmonyPrefix]
    public static void StartOfRoundShipStartPre()
    {
        Plugin.Log.LogInfo($"Setting starting quota to {Plugin.BoundConfig.StartingQuota}...");
        var quotaSettings = TimeOfDay.Instance.quotaVariables;
        quotaSettings.startingQuota = Plugin.BoundConfig.StartingQuota;
    }
}