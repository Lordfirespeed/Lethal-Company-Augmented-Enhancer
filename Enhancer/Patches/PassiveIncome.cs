using HarmonyLib;

namespace Enhancer.Patches;

public class PassiveIncome : IPatch
{
    [HarmonyPatch(typeof(TimeOfDay), nameof(TimeOfDay.OnDayChanged))]
    [HarmonyPostfix]
    public static void AddPassiveIncome(TimeOfDay __instance)
    {
        if (!__instance.IsServer) return;
        
        Plugin.Log.LogInfo($"Adding {Plugin.BoundConfig.PassiveIncomeQuantity.Value} passive income credits...");
        Terminal? objectOfType = UnityEngine.Object.FindObjectOfType<Terminal>();
        objectOfType!.groupCredits += Plugin.BoundConfig.PassiveIncomeQuantity.Value;
        objectOfType.SyncGroupCreditsServerRpc(objectOfType.groupCredits, objectOfType.numberOfItemsInDropship);
    } 
}