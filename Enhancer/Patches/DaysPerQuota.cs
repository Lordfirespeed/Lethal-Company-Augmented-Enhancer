using System;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace Enhancer.Patches;

public class DaysPerQuota : IPatch
{
    protected static ManualLogSource Logger { get; set; } = null!;
    
    public void SetLogger(ManualLogSource logger)
    {
        logger.LogDebug("Logger assigned.");
        Logger = logger;
    }
    
    [HarmonyPatch(typeof(Terminal), nameof(Terminal.Start))]
    [HarmonyPrefix]
    public static void StartOfRoundShipStartPre()
    {
        Logger.LogInfo($"Setting days per quota to {Plugin.BoundConfig.DaysPerQuotaAssignment.Value}...");
        var quotaSettings = TimeOfDay.Instance.quotaVariables;
        quotaSettings.deadlineDaysAmount = Plugin.BoundConfig.DaysPerQuotaAssignment.Value;
    }
    
    [HarmonyPatch(typeof(TimeOfDay), nameof(TimeOfDay.SetNewProfitQuota))]
    [HarmonyPostfix]
    static void SetDeadline(ref float timeUntilDeadline, ref float profitQuota)
    {
        timeUntilDeadline = Mathf.Clamp(Mathf.Ceil(profitQuota / 200), 4f, 10f);
    }

    [HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.ResetSavedGameValues))]
    [HarmonyPrefix]
    static void ResetSavedValues(ref bool isHostingGame)
    {
        if (!isHostingGame)
            return;
        string currentSaveFile = GameNetworkManager.Instance.currentSaveFileName;
        ES3.Save("previousDeadlines", Array.Empty<PastQuotaAssignmentInfo>(), currentSaveFile);
        ES3.Save("quotaVariables", TimeOfDay.Instance.quotaVariables, currentSaveFile);
    }
}

public struct PastQuotaAssignmentInfo
{
    
}

public enum QuotaDurationBehaviour
{
    Constant,
    Variable,
    DynamicVariable,
}