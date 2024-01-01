using BepInEx.Logging;
using HarmonyLib;

namespace Enhancer.Patches;

public class PassiveIncome : IPatch
{
    protected static ManualLogSource Logger { get; set; } = null!;

    public void SetLogger(ManualLogSource logger)
    {
        logger.LogDebug("Logger assigned.");
        Logger = logger;
    }

    [HarmonyPatch(typeof(TimeOfDay), nameof(TimeOfDay.OnDayChanged))]
    [HarmonyPostfix]
    public static void AddPassiveIncome(TimeOfDay __instance)
    {
        if (!__instance.IsServer) return;

        Logger.LogInfo($"Adding {Plugin.BoundConfig.PassiveIncomeQuantity.Value} passive income credits...");
        var objectOfType = UnityEngine.Object.FindObjectOfType<Terminal>();
        objectOfType!.groupCredits += Plugin.BoundConfig.PassiveIncomeQuantity.Value;
        objectOfType.SyncGroupCreditsServerRpc(objectOfType.groupCredits, objectOfType.numberOfItemsInDropship);
    }
}