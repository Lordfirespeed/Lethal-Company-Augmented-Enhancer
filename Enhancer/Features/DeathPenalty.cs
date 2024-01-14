using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;

namespace Enhancer.Features;

public class DeathPenalty : IPatch
{
    protected static ManualLogSource Logger { get; set; } = null!;

    public void SetLogger(ManualLogSource logger)
    {
        logger.LogDebug("Logger assigned.");
        Logger = logger;
    }

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
        var exponent = Mathf.Pow(2, Plugin.BoundConfig.DeathPenaltyScalingCurvature.Value);
        var penaltyFactor = Mathf.Pow(deathCoefficient, exponent) * Mathf.Pow(CountTotalPlayersInSession(), -exponent) *
                            maximumPenaltyFactor;
        return Mathf.Clamp(penaltyFactor, 0f, 1f);
    }

    private static float ComputeDeathCoefficient(int playersDead, int bodiesInsured)
    {
        return playersDead - bodiesInsured +
               (1f - Mathf.Max(0, Plugin.BoundConfig.DeadBodyRecoveryDiscount.Value)) * bodiesInsured;
    }

    [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.ApplyPenalty))]
    [HarmonyPrefix]
    public static bool ApplyPenaltyPrefix(ref int playersDead, ref int bodiesInsured, HUDManager __instance)
    {
        playersDead = Mathf.Max(playersDead, 0);
        bodiesInsured = Mathf.Max(bodiesInsured, 0);

        var deathCoefficient = ComputeDeathCoefficient(playersDead, bodiesInsured);
        var maximumPenaltyFactor = DetermineMaximumPenaltyFactor();
        var penaltyFactor = ComputePenaltyFactor(deathCoefficient, maximumPenaltyFactor);

        var terminalInstance = Object.FindObjectOfType<Terminal>();
        var penaltyTotal = (int)(terminalInstance.groupCredits * penaltyFactor);

        Logger.LogInfo(
            $"Death Penalty is {penaltyFactor:p1} of {terminalInstance.groupCredits} = {penaltyTotal} credits.");

        terminalInstance.groupCredits = Mathf.Max(0, terminalInstance.groupCredits - penaltyTotal);

        __instance.statsUIElements.penaltyAddition.text =
            $"{playersDead} casualties: -{penaltyFactor:p0}\n({bodiesInsured} bodies recovered)";
        __instance.statsUIElements.penaltyTotal.text = $"DUE: {penaltyTotal}";

        Logger.LogInfo($"Death penalty has been applied. New group credits: {terminalInstance.groupCredits}.");

        return false;
    }
}
