using HarmonyLib;
using UnityEngine;

namespace Enhancer.Patches;

public class DeathPenalty : IPatch
{
    private static int CountTotalPlayersInSession()
    {
        return StartOfRound.Instance.connectedPlayersAmount + 1;
    }
    
    private static float DetermineMaximumPenaltyFactor()
    {
        return Mathf.Clamp(
            Mathf.Min(
                Plugin.BoundConfig.MaximumDeathPenalty.Value,
                CountTotalPlayersInSession() * Plugin.BoundConfig.MaximumDeathPenaltyPerPlayer.Value
            ), 
            0, 
            1
        );
    }

    private static float ComputePenaltyFactor(float deathCoefficient, float maximumPenaltyFactor)
    {
        float exponent = Mathf.Pow(2, Plugin.BoundConfig.DeathPenaltyScalingCurvature.Value);
        float penaltyFactor = Mathf.Pow(deathCoefficient, exponent) * Mathf.Pow(CountTotalPlayersInSession(), -exponent) * maximumPenaltyFactor;
        return Mathf.Clamp(penaltyFactor, 0f, 1f);
    }

    private static float ComputeDeathCoefficient(int playersDead, int bodiesInsured)
    {
        return playersDead - bodiesInsured + (1f - Mathf.Max(0, Plugin.BoundConfig.DeadBodyRecoveryDiscount.Value)) * bodiesInsured;
    }
    
    [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.ApplyPenalty))]
    [HarmonyPrefix]
    public static bool ApplyPenaltyPrefix(ref int playersDead, ref int bodiesInsured)
    {   
        playersDead = Mathf.Max(playersDead, 0);
        bodiesInsured = Mathf.Max(bodiesInsured, 0);

        float deathCoefficient = ComputeDeathCoefficient(playersDead, bodiesInsured);
        float maximumPenaltyFactor = DetermineMaximumPenaltyFactor();
        float penaltyFactor = ComputePenaltyFactor(deathCoefficient, maximumPenaltyFactor);
        
        Terminal terminalInstance = UnityEngine.Object.FindObjectOfType<Terminal>();
        int penaltyTotal = (int)(terminalInstance.groupCredits * penaltyFactor);
        
        Plugin.Log.LogInfo($"Death Penalty is {penaltyFactor:p1} of {terminalInstance.groupCredits} = {penaltyTotal} credits.");
        
        terminalInstance.groupCredits = Mathf.Max(0, terminalInstance.groupCredits - penaltyTotal);

        HUDManager.Instance.statsUIElements.penaltyAddition.text = $"{playersDead} casualties: -{penaltyFactor:p0}\n({bodiesInsured} bodies recovered)";
        HUDManager.Instance.statsUIElements.penaltyTotal.text = $"DUE: {penaltyTotal}";
        
        Plugin.Log.LogInfo($"Death penalty has been applied. New group credits: {terminalInstance.groupCredits}.");
        
        return false;
    }
}