using HarmonyLib;

namespace Enhancer.Patches;

public class StartingCredits
{
    [HarmonyPatch(typeof(StartOfRound), "Start")]
    [HarmonyPrefix]
    public static void StartOfRoundShipStartPre()
    {
        Plugin.Log.LogInfo($"Setting starting credits to {Plugin.BoundConfig.StartingCredits.Value}...");
        var quotaSettings = TimeOfDay.Instance.quotaVariables;
        quotaSettings.startingCredits = Plugin.BoundConfig.StartingCredits.Value;
    }
}