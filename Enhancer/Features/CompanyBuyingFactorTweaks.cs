using BepInEx.Logging;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace Enhancer.Features;

public class CompanyBuyingFactorTweaks : IPatch
{
    protected static ManualLogSource Logger { get; set; } = null!;

    public void SetLogger(ManualLogSource logger)
    {
        logger.LogDebug("Logger assigned.");
        Logger = logger;
    }

    private static float GetRandomPriceFactor()
    {
        if (TimeOfDay.Instance.daysUntilDeadline < 1) return 1.0f;

        Logger.LogInfo("Choosing random buying factor...");

        //Company mood factor
        var moodFactor = GetMoodFactor();
        //Small increase each day
        var daysFactor =
            1.0f + 0.05f * (Plugin.BoundConfig.DaysPerQuotaAssignment.Value - TimeOfDay.Instance.daysUntilDeadline);

        //This maximum value should only happen after more than 10 days on a single quota
        daysFactor = Mathf.Clamp(daysFactor, 1.0f, 2.0f);

        //float Prices = Random.Range(MoodFactor * DaysFactor, 1.0f);

        //Use the level seed to get prices
        System.Random rng = new(StartOfRound.Instance.randomMapSeed + 77);
        var priceFactor = (float)rng.NextDouble() * (1.0f - moodFactor * daysFactor) + moodFactor;

        Logger.LogInfo($"New buying % set at {priceFactor}");
        Logger.LogDebug($"    factors {moodFactor} : {daysFactor} : {StartOfRound.Instance.randomMapSeed + 77}");

        return priceFactor;
    }

    private static string? GetCompanyMoodName()
    {
        if (TimeOfDay.Instance is null)
            return null;

        if (TimeOfDay.Instance.currentCompanyMood is null)
            return null;

        return TimeOfDay.Instance.currentCompanyMood.name;
    }

    private static float GetMoodFactor()
    {
        Logger.LogDebug("Getting mood factor");

        try {
            return GetCompanyMoodName() switch {
                "SilentCalm" => 0.35f,
                "SnoringGiant" => 0.45f,
                "Agitated" => 0.25f,
                _ => 0.40f
            };
        }
        finally {
            Logger.LogDebug("Got mood factor");
        }
    }

    [HarmonyPatch(typeof(TimeOfDay), nameof(TimeOfDay.SetBuyingRateForDay))]
    [HarmonyPostfix]
    public static void BuyingRatePost(TimeOfDay __instance)
    {
        Logger.LogInfo("Setting company buying rate ...");

        if (!NetworkManager.Singleton.IsHost && !NetworkManager.Singleton.IsServer) return;

        if (Plugin.BoundConfig.RandomiseCompanyBuyingFactor.Value)
            StartOfRound.Instance.companyBuyingRate = GetRandomPriceFactor();

        //Minimum sale rate fixes negative rates
        if (StartOfRound.Instance.companyBuyingRate < Plugin.BoundConfig.MinimumCompanyBuyingFactor.Value)
            StartOfRound.Instance.companyBuyingRate = Plugin.BoundConfig.MinimumCompanyBuyingFactor.Value;

        //Make sure clients are up to date
        StartOfRound.Instance.SyncCompanyBuyingRateServerRpc();
    }
}
