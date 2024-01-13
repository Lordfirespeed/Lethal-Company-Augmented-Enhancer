using System;
using System.Linq;
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

    private static float DifficultyScalar {
        get {
            var quotasCompleted = TimeOfDay.Instance.timesFulfilledQuota;
            var quotasToCompleteForMaximumScalar = Plugin.BoundConfig.AssignmentsToReachMaximumTargetIncomePerDay.Value;
            if (quotasCompleted >= quotasToCompleteForMaximumScalar)
                return Plugin.BoundConfig.MaxTargetIncomePerDayScalar.Value;
            if (quotasCompleted == 0) return 1;

            var exponent = Mathf.Pow(2, Plugin.BoundConfig.TargetIncomePerDayScalarCurvature.Value);
            var difficulty = 1 + Mathf.Pow((float)quotasCompleted / quotasToCompleteForMaximumScalar, exponent) * (Plugin.BoundConfig.MaxTargetIncomePerDayScalar.Value - 1);
            Logger.LogInfo($"Difficulty scalar calculated to be {difficulty:f1}");
            return difficulty;
        }
    }

    private static float RandomnessScalar {
        get {
            var randomnessScalar = Plugin.BoundConfig.TargetIncomePerDayRandomnessScalar.Value;
            if (randomnessScalar == 0) return 0;
            var randomness = 1 + randomnessScalar * 2 * TimeOfDay.Instance.quotaVariables.randomizerCurve.Evaluate(UnityEngine.Random.Range(0f, 1f));
            Logger.LogInfo($"Randomness scalar selected to be {randomness:f1}");
            return randomness;
        }
    }

    private static float BaseTargetDailyIncome {
        get {
            switch (Plugin.BoundConfig.DaysPerQuotaAssignmentBehaviour.Value)
            {
                case QuotaDurationBehaviour.Constant:
                    throw new InvalidOperationException();
                case QuotaDurationBehaviour.Variable:
                    return Plugin.BoundConfig.BaseTargetIncomePerDay.Value;
                case QuotaDurationBehaviour.DynamicVariable:
                    var averageIncomePerDay = QuotaFormula.Instance!.PastAssignments.Average(info => (float)info.Income / info.Duration);
                    Logger.LogInfo($"Average income is currently {averageIncomePerDay:f1}");
                    return averageIncomePerDay;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    private static float TargetDailyIncome => BaseTargetDailyIncome * DifficultyScalar * RandomnessScalar;

    private static float QuotaMagnitude {
        get {
            var quotaFormula = QuotaFormula.Instance!;
            var totalAdditions = quotaFormula.PastAssignments.Sum(info => info.BaseIncreaseCoefficient * info.PolynomialIncreaseCoefficient);
            return quotaFormula.RunQuotaVariables.StartingQuota + totalAdditions;
        }
    }

    private static int AssignmentDuration {
        get {
            if (Plugin.BoundConfig.DaysPerQuotaAssignmentBehaviour.Value is QuotaDurationBehaviour.Constant)
                return Plugin.BoundConfig.DaysPerQuotaAssignment.Value;

            var duration = Mathf.CeilToInt(QuotaMagnitude / TargetDailyIncome);
            return Plugin.BoundConfig.DaysPerQuotaAssignmentBounds.Value.Clamp(duration);
        }
    }

    private static void SetQuotaDuration()
    {
        var newDuration = AssignmentDuration;
        Logger.LogInfo($"Setting quota duration to {newDuration}...");
        var quotaSettings = TimeOfDay.Instance.quotaVariables;
        quotaSettings.deadlineDaysAmount = newDuration;
    }

    private static void TrySetQuotaDuration()
    {
        try
        {
            SetQuotaDuration();
        }
        catch (Exception error)
        {
            Logger.LogError("Failed to set new quota duration.");
            Logger.LogError(error);
        }
    }

    [HarmonyPatch(typeof(Terminal), nameof(Terminal.Start))]
    [HarmonyPrefix]
    public static void StartOfRoundShipStartPre()
    {
        TrySetQuotaDuration();
    }

    [HarmonyPatch(typeof(TimeOfDay), nameof(TimeOfDay.SetNewProfitQuota))]
    [HarmonyPostfix]
    static void SetDeadline()
    {
        TrySetQuotaDuration();
    }
}

public enum QuotaDurationBehaviour
{
    Constant,
    Variable,
    DynamicVariable,
}
