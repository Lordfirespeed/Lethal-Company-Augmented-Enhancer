using HarmonyLib;

namespace Enhancer.Patches;

public class PassiveIncome
{
    [HarmonyPatch(typeof(TimeOfDay), nameof(TimeOfDay.OnDayChanged))]
    [HarmonyPostfix]
    public static void AddPassiveIncome()
    {
        Terminal objectOfType = UnityEngine.Object.FindObjectOfType<Terminal>();
        if (objectOfType == null) return;
        objectOfType.groupCredits += Plugin.BoundConfig.PassiveIncomeQuantity.Value;
    } 
}