using BepInEx.Logging;
using HarmonyLib;

namespace Enhancer.Patches;

public class DaysPerQuota : IPatch
{
    protected static ManualLogSource Logger { get; set; } = null!;

    public void SetLogger(ManualLogSource logger)
    {
        logger.LogDebug("Logger assigned.");
        Logger = logger;
    }

    [HarmonyPatch(typeof(StartOfRound), "Start")]
    [HarmonyPrefix]
    public static void StartOfRoundShipStartPre()
    {
        Logger.LogInfo($"Setting days per quota to {Plugin.BoundConfig.DaysPerQuotaAssignment.Value}...");
        var quotaSettings = TimeOfDay.Instance.quotaVariables;
        quotaSettings.deadlineDaysAmount = Plugin.BoundConfig.DaysPerQuotaAssignment.Value;
    }
}