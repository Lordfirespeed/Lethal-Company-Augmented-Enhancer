using HarmonyLib;

namespace Enhancer.Patches;

public class QuotaFormula
{
    [HarmonyPatch(typeof(StartOfRound), "Start")]
    [HarmonyPrefix]
    public static void StartOfRoundShipStartPre()
    {
        Plugin.Log.LogInfo("Setting quota formula variables...");
        var quotaSettings = TimeOfDay.Instance.quotaVariables;
        quotaSettings.startingQuota = Plugin.BoundConfig.StartingQuota;
        // vanilla 'increase steepness' is actually 'increase shallowness', so we reciprocate (1/x) the value
        quotaSettings.increaseSteepness = 1f / Plugin.BoundConfig.QuotaIncreaseSteepness;
        quotaSettings.baseIncrease = Plugin.BoundConfig.QuotaBaseIncrease;
        quotaSettings.randomizerMultiplier = Plugin.BoundConfig.QuotaIncreaseRandomFactor;
    }
}