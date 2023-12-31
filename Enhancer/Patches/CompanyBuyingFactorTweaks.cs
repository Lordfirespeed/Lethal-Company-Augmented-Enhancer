using UnityEngine;
using HarmonyLib;
using Unity.Netcode;

namespace Enhancer.Patches;

public class CompanyBuyingFactorTweaks : IPatch
{
    private static float GetRandomPriceFactor()
    {
        if (TimeOfDay.Instance.daysUntilDeadline < 1)
        {
            return 1.0f;
        }
            
        Plugin.Logger.LogInfo("Choosing random price factor");

        //Company mood factor
        float moodFactor = GetMoodFactor();
        //Small increase each day
        float daysFactor = (float)(1.0 + 0.05f * (Plugin.BoundConfig.DaysPerQuotaAssignment.Value - TimeOfDay.Instance.daysUntilDeadline));

        //This maximum value should only happen after more than 10 days on a single quota
        daysFactor = Mathf.Clamp(daysFactor, 1.0f, 2.0f);

        //float Prices = Random.Range(MoodFactor * DaysFactor, 1.0f);

        //Use the level seed to get prices
        System.Random rng = new(StartOfRound.Instance.randomMapSeed + 77);
        float priceFactor = (float)rng.NextDouble() * (1.0f - moodFactor * daysFactor) + moodFactor;
        
        Plugin.Logger.LogInfo("New price % set at" + priceFactor);
        Plugin.Logger.LogInfo("    factors " + moodFactor + " : " + daysFactor + " : " + (StartOfRound.Instance.randomMapSeed + 77));

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
        Plugin.Logger.LogInfo("Getting mood factor");
        
        try
        {
            return GetCompanyMoodName() switch
            {
                "SilentCalm" => 0.35f,
                "SnoringGiant" => 0.45f,
                "Agitated" => 0.25f,
                _ => 0.40f,
            };
        }
        finally
        {
            Plugin.Logger.LogInfo("Got mood factor");
        }
    }
    
    [HarmonyPatch(typeof(TimeOfDay), nameof(TimeOfDay.SetBuyingRateForDay))]
    [HarmonyPostfix]
    public static void BuyingRatePost(TimeOfDay __instance)
    {
        Plugin.Logger.LogInfo("TimeOfDay SetBuyingRateForDay");

        if (!NetworkManager.Singleton.IsHost && !NetworkManager.Singleton.IsServer)
        {
            return;
        }
        
        if (Plugin.BoundConfig.RandomiseCompanyBuyingFactor.Value)
        {
            StartOfRound.Instance.companyBuyingRate = GetRandomPriceFactor();
        }

        //Minimum sale rate fixes negative rates
        if (StartOfRound.Instance.companyBuyingRate < Plugin.BoundConfig.MinimumCompanyBuyingFactor.Value)
            StartOfRound.Instance.companyBuyingRate = Plugin.BoundConfig.MinimumCompanyBuyingFactor.Value;

        //Make sure clients are up to date
        StartOfRound.Instance.SyncCompanyBuyingRateServerRpc();
    }
}