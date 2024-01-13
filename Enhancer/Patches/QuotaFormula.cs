using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using NetworkManager = Unity.Netcode.NetworkManager;
using Object = UnityEngine.Object;
using OpCodes = System.Reflection.Emit.OpCodes;

namespace Enhancer.Patches;

public class QuotaFormula : IPatch
{
    protected static ManualLogSource Logger { get; set; } = null!;
    public static QuotaFormula? Instance { get; private set; }
    protected static readonly string QuotaVariablesKey = $"{MyPluginInfo.PLUGIN_GUID}-QuotaVariables";
    protected static readonly string PastAssignmentsKey = $"{MyPluginInfo.PLUGIN_GUID}-PastAssignments";

    private static readonly MethodInfo FindObjectOfTypeTimeOfDayMethodInfo =
        AccessTools.Method(typeof(Object), nameof(Object.FindObjectOfType), generics: [typeof(TimeOfDay),]);

    private AugmentedQuotaVariables? _runQuotaVariables;
    public AugmentedQuotaVariables RunQuotaVariables {
        get {
            if (!NetworkManager.Singleton.IsConnectedClient)
                throw new InvalidOperationException("Not currently in-game!");

            _runQuotaVariables ??= ES3.Load<AugmentedQuotaVariables>(
                QuotaVariablesKey,
                GameNetworkManager.Instance.currentSaveFileName,
                defaultValue: null!
            );

            _runQuotaVariables ??= ConfiguredQuotaVariables;

            return _runQuotaVariables;
        }
    }

    private QuotaAssignmentInfo? _currentAssignmentInfo;
    private QuotaAssignmentInfo? CurrentAssignmentInfo {
        get {
            if (!NetworkManager.Singleton.IsConnectedClient)
                throw new InvalidOperationException("Not currently in-game!");

            return _currentAssignmentInfo;
        }
    }

    private List<QuotaAssignmentInfo>? _pastAssignments;

    public List<QuotaAssignmentInfo> PastAssignments {
        get {
            if (!NetworkManager.Singleton.IsConnectedClient)
                throw new InvalidOperationException("Not currently in-game!");

            _pastAssignments ??= ES3.Load<List<QuotaAssignmentInfo>>(
                PastAssignmentsKey,
                GameNetworkManager.Instance.currentSaveFileName,
                defaultValue: null!
            );

            _pastAssignments ??= new();

            return _pastAssignments;
        }
    }

    public static AugmentedQuotaVariables ConfiguredQuotaVariables => new() {
        StartingQuota = Plugin.BoundConfig.StartingQuota.Value,
        IncreaseSteepness = Plugin.BoundConfig.QuotaIncreaseSteepnessCoefficient.Value,
        IncreaseExponent = Plugin.BoundConfig.QuotaIncreaseSteepnessExponent.Value,
        BaseIncrease = Plugin.BoundConfig.QuotaBaseIncrease.Value,
        RandomizerMultiplier = Plugin.BoundConfig.QuotaIncreaseRandomFactor.Value * 2,
    };

    public void SetLogger(ManualLogSource logger)
    {
        logger.LogDebug("Logger assigned.");
        Logger = logger;
    }

    public void OnPatch()
    {
        Instance = this;
    }

    public void OnUnpatch()
    {
        Instance = null;
    }

    public static void ResetQuotaVariables()
    {
        var variables = ConfiguredQuotaVariables;
        Instance!._runQuotaVariables = variables;
        ES3.Save(QuotaVariablesKey, variables, GameNetworkManager.Instance.currentSaveFileName);
        Logger.LogDebug("Saved quota variables to current save file.");
    }

    public static void ResetPastAssignments()
    {
        var assignments = new List<QuotaAssignmentInfo>();
        Instance!._pastAssignments = assignments;
        ES3.Save(PastAssignmentsKey, assignments, GameNetworkManager.Instance.currentSaveFileName);
        Logger.LogDebug("Saved past assignments to current save file.");
    }

    public static void ResetValues()
    {
        ResetQuotaVariables();
        ResetPastAssignments();
    }

    public static void SaveQuotaVariables()
    {
        if (ES3.KeyExists(QuotaVariablesKey, GameNetworkManager.Instance.currentSaveFileName)) return;
        ResetValues();
    }

    public static void SavePastAssignments()
    {
        ES3.Save(PastAssignmentsKey, Instance!.PastAssignments, GameNetworkManager.Instance.currentSaveFileName);
    }

    public static void SaveValues()
    {
        SaveQuotaVariables();
        SavePastAssignments();
    }

    public float ComputePolynomialCoefficient(int timesFulfilledQuota)
    {
        // vanilla 'increase steepness' is actually 'increase shallowness', so we multiply by our value instead of dividing
        var value = 1f + Mathf.Pow(timesFulfilledQuota, RunQuotaVariables.IncreaseExponent) * RunQuotaVariables.IncreaseSteepness;
        Logger.LogDebug($"Computed polynomial coefficient: {value}");
        Instance!._currentAssignmentInfo!.PolynomialIncreaseCoefficient = value;
        return value;
    }

    [HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.ResetSavedGameValues))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> TranspileResetSavedGameValues(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions);

        matcher
            .Start()
            // Advance to where 'FindObjectOfType<TimeOfDay>' is called and null-checked
            .MatchForward(true,
                new CodeMatch(OpCodes.Call, FindObjectOfTypeTimeOfDayMethodInfo),
                new CodeMatch(OpCodes.Stloc_0),
                new CodeMatch(OpCodes.Ldloc_0),
                new CodeMatch(OpCodes.Ldnull),
                new CodeMatch(OpCodes.Call),
                new CodeMatch(OpCodes.Brfalse)
            )
            // Advance to the next block (the 'else' branch)
            .MatchForward(false,
                new CodeMatch { labels = [(Label)matcher.Operand] }
            )
            // Regress to where the starting quota is referenced
            .MatchBack(false,
                new CodeMatch(OpCodes.Ldstr, "ProfitQuota"),
                new CodeMatch(OpCodes.Ldloc_0),
                new CodeMatch(
                    OpCodes.Ldfld,
                    AccessTools.Field(typeof(TimeOfDay), nameof(TimeOfDay.quotaVariables))
                ),
                new CodeMatch(
                    OpCodes.Ldfld,
                    AccessTools.Field(typeof(QuotaSettings), nameof(QuotaSettings.startingQuota))
                    )
            )
            // Set the run quota settings *before* saving the current profit quota
            .Insert(
                new CodeInstruction(
                    OpCodes.Call,
                    AccessTools.Method(typeof(QuotaFormula), nameof(ResetValues))
                )
            )
            // Advance to `Ldloc_0`
            .Advance(2)
            // Remove the instructions
            .RemoveInstructions(3)
            .Insert(
                new CodeInstruction(OpCodes.Call,
                    AccessTools.PropertyGetter(typeof(QuotaFormula), nameof(Instance))
                ),
                new CodeInstruction(
                    OpCodes.Call,
                    AccessTools.PropertyGetter(typeof(QuotaFormula), nameof(RunQuotaVariables))
                ),
                new CodeInstruction(
                    OpCodes.Ldfld,
                    AccessTools.Field(typeof(AugmentedQuotaVariables), nameof(AugmentedQuotaVariables.StartingQuota))
                )
            );

        return matcher.InstructionEnumeration();
    }

    [HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.SaveGameValues))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> TranspileSaveGameValues(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions);

        matcher
            .Start()
            .MatchForward(true,
                new CodeMatch(OpCodes.Call, FindObjectOfTypeTimeOfDayMethodInfo),
                new CodeMatch(OpCodes.Stloc_0),
                new CodeMatch(OpCodes.Ldloc_0),
                new CodeMatch(OpCodes.Ldnull),
                new CodeMatch(OpCodes.Call),
                new CodeMatch(OpCodes.Brfalse)
            )
            .MatchForward(false,
                new CodeMatch { labels = [(Label)matcher.Operand] }
            )
            .MatchBack(false,
                new CodeMatch(OpCodes.Call)
            )
            .Insert(
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(QuotaFormula), nameof(SaveValues)))
            );

        return matcher.InstructionEnumeration();
    }

    [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.SetTimeAndPlanetToSavedSettings))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> TranspileSetTimeAndPlanetToSavedSettings(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions);

        matcher
            .Start()
            .MatchForward(
                false,
                new CodeMatch(
                    OpCodes.Call,
                    AccessTools.PropertyGetter(typeof(TimeOfDay), nameof(TimeOfDay.Instance))
                ),
                new CodeMatch(
                    OpCodes.Ldfld,
                    AccessTools.Field(typeof(TimeOfDay), nameof(TimeOfDay.quotaVariables))
                ),
                new CodeMatch(
                    OpCodes.Ldfld,
                    AccessTools.Field(typeof(QuotaSettings), nameof(QuotaSettings.startingQuota))
                )
            )
            .RemoveInstructions(3)
            .Insert(
                new CodeInstruction(OpCodes.Call,
                    AccessTools.PropertyGetter(typeof(QuotaFormula), nameof(Instance))
                ),
                new CodeInstruction(
                    OpCodes.Call,
                    AccessTools.PropertyGetter(typeof(QuotaFormula), nameof(RunQuotaVariables))
                ),
                new CodeInstruction(
                    OpCodes.Ldfld,
                    AccessTools.Field(typeof(AugmentedQuotaVariables), nameof(AugmentedQuotaVariables.StartingQuota))
                )
            );

        return matcher.InstructionEnumeration();
    }

    [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.ResetShip))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> TranspileResetShip(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions);

        matcher
            .Start()
            .MatchForward(
                false,
                new CodeMatch(
                    OpCodes.Call,
                    AccessTools.PropertyGetter(typeof(TimeOfDay), nameof(TimeOfDay.Instance))
                ),
                new CodeMatch(
                    OpCodes.Ldfld,
                    AccessTools.Field(typeof(TimeOfDay), nameof(TimeOfDay.quotaVariables))
                ),
                new CodeMatch(
                    OpCodes.Ldfld,
                    AccessTools.Field(typeof(QuotaSettings), nameof(QuotaSettings.startingQuota))
                )
            )
            .RemoveInstructions(3)
            .Insert(
                new CodeInstruction(OpCodes.Call,
                    AccessTools.PropertyGetter(typeof(QuotaFormula), nameof(Instance))
                ),
                new CodeInstruction(
                    OpCodes.Call,
                    AccessTools.PropertyGetter(typeof(QuotaFormula), nameof(RunQuotaVariables))
                ),
                new CodeInstruction(
                    OpCodes.Ldfld,
                    AccessTools.Field(typeof(AugmentedQuotaVariables), nameof(AugmentedQuotaVariables.StartingQuota))
                )
            );

        return matcher.InstructionEnumeration();
    }

    [HarmonyPatch(typeof(TimeOfDay), nameof(TimeOfDay.SetNewProfitQuota))]
    class SetNewProfitQuotaPatches
    {
        [HarmonyPrefix]
        public static void Prefix(TimeOfDay __instance)
        {
            if (Instance is null) return;

            Instance._currentAssignmentInfo = new QuotaAssignmentInfo
            {
                Quota = __instance.profitQuota,
                Income = __instance.quotaFulfilled,
                Duration = __instance.quotaVariables.deadlineDaysAmount,

                BaseIncreaseCoefficient = Instance.RunQuotaVariables.BaseIncrease,
            };
        }

        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var matcher = new CodeMatcher(instructions);

            matcher
                .Start()
                // Advance to vanilla base coefficient instructions
                .MatchForward(
                    false,
                    new CodeMatch(OpCodes.Ldc_R4, 1f),
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(
                        OpCodes.Ldfld,
                        AccessTools.Field(typeof(TimeOfDay), nameof(TimeOfDay.timesFulfilledQuota))
                    ),
                    new CodeMatch(OpCodes.Conv_R4),
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(
                        OpCodes.Ldfld,
                        AccessTools.Field(typeof(TimeOfDay), nameof(TimeOfDay.timesFulfilledQuota))
                    ),
                    new CodeMatch(OpCodes.Conv_R4),
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(
                        OpCodes.Ldfld,
                        AccessTools.Field(typeof(TimeOfDay), nameof(TimeOfDay.quotaVariables))
                    ),
                    new CodeMatch(
                        OpCodes.Ldfld,
                        AccessTools.Field(typeof(QuotaSettings), nameof(QuotaSettings.increaseSteepness))
                    ),
                    new CodeMatch(OpCodes.Div),
                    new CodeMatch(OpCodes.Mul),
                    new CodeMatch(OpCodes.Add)
                )
                // Remove vanilla base coefficient instructions
                .RemoveInstructions(13)
                // Insert call to augmented polynomial coefficient calculator
                .Insert(
                    new CodeInstruction(OpCodes.Call,
                        AccessTools.PropertyGetter(typeof(QuotaFormula), nameof(Instance))),
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Ldfld,
                        AccessTools.Field(typeof(TimeOfDay), nameof(TimeOfDay.timesFulfilledQuota))
                    ),
                    new CodeInstruction(OpCodes.Call,
                        AccessTools.Method(typeof(QuotaFormula), nameof(ComputePolynomialCoefficient)))
                )
                // Advance to vanilla variables baseIncrease reference
                .MatchForward(
                    false,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld,
                        AccessTools.Field(typeof(TimeOfDay), nameof(TimeOfDay.quotaVariables))),
                    new CodeMatch(
                        OpCodes.Ldfld,
                        AccessTools.Field(typeof(QuotaSettings), nameof(QuotaSettings.baseIncrease))
                    )
                )
                // Remove vanilla variables baseIncrease reference
                .RemoveInstructions(3)
                // Insert reference to augmented variables baseIncrease
                .Insert(
                    new CodeInstruction(OpCodes.Call,
                        AccessTools.PropertyGetter(typeof(QuotaFormula), nameof(Instance))),
                    new CodeInstruction(
                        OpCodes.Call,
                        AccessTools.PropertyGetter(typeof(QuotaFormula), nameof(RunQuotaVariables))
                    ),
                    new CodeInstruction(
                        OpCodes.Ldfld,
                        AccessTools.Field(typeof(AugmentedQuotaVariables), nameof(AugmentedQuotaVariables.BaseIncrease))
                    )
                )
                // Advance to multiplying base increase by polynomial increase
                .MatchForward(
                    true,
                    new CodeMatch(OpCodes.Mul)
                )
                .Advance(1)
                // Insert calls to augmented assignment info getter (for storing the random coefficient)
                .Insert(
                    new CodeInstruction(OpCodes.Call,
                        AccessTools.PropertyGetter(typeof(QuotaFormula), nameof(Instance))),
                    new CodeInstruction(OpCodes.Call,
                        AccessTools.PropertyGetter(typeof(QuotaFormula), nameof(CurrentAssignmentInfo))),
                    new CodeInstruction(OpCodes.Call,
                        AccessTools.PropertyGetter(typeof(QuotaFormula), nameof(Instance))),
                    new CodeInstruction(OpCodes.Call,
                        AccessTools.PropertyGetter(typeof(QuotaFormula), nameof(CurrentAssignmentInfo)))
                )
                // Advance to vanilla variables randomizerMultiplier reference
                .MatchForward(
                    false,
                    new CodeMatch(OpCodes.Ldarg_0),
                    new CodeMatch(OpCodes.Ldfld,
                        AccessTools.Field(typeof(TimeOfDay), nameof(TimeOfDay.quotaVariables))),
                    new CodeMatch(
                        OpCodes.Ldfld,
                        AccessTools.Field(typeof(QuotaSettings), nameof(QuotaSettings.randomizerMultiplier))
                    )
                )
                // Remove vanilla variables randomizerMultiplier reference
                .RemoveInstructions(3)
                // Insert reference to augmented variables randomizerMultiplier
                .Insert(
                    new CodeInstruction(OpCodes.Call,
                        AccessTools.PropertyGetter(typeof(QuotaFormula), nameof(Instance))),
                    new CodeInstruction(
                        OpCodes.Call,
                        AccessTools.PropertyGetter(typeof(QuotaFormula), nameof(RunQuotaVariables))
                    ),
                    new CodeInstruction(
                        OpCodes.Ldfld,
                        AccessTools.Field(typeof(AugmentedQuotaVariables),
                            nameof(AugmentedQuotaVariables.RandomizerMultiplier))
                    )
                )
                // Advance to multiplying random coefficient by everything else
                .MatchForward(
                    true,
                    new CodeMatch(OpCodes.Mul),
                    new CodeMatch(OpCodes.Ldc_R4, 1f),
                    new CodeMatch(OpCodes.Add),
                    new CodeMatch(OpCodes.Mul)
                )
                // Store the random coefficient just before multiplying by it
                .Insert(
                    new CodeInstruction(
                        OpCodes.Stfld,
                        AccessTools.Field(typeof(QuotaAssignmentInfo),
                            nameof(QuotaAssignmentInfo.RandomizedIncreaseCoefficient))
                    ),
                    new CodeInstruction(
                        OpCodes.Ldfld,
                        AccessTools.Field(typeof(QuotaAssignmentInfo),
                            nameof(QuotaAssignmentInfo.RandomizedIncreaseCoefficient))
                    )
                );

            return matcher.InstructionEnumeration();
        }

        [HarmonyPostfix]
        public static void Postfix(TimeOfDay __instance)
        {
            if (Instance?.CurrentAssignmentInfo is null) return;

            Instance.CurrentAssignmentInfo!.NextQuota = __instance.profitQuota;
            Instance.CurrentAssignmentInfo!.CombinedIncrease = __instance.profitQuota - Instance.CurrentAssignmentInfo!.Quota;

            Logger.LogDebug(Instance.CurrentAssignmentInfo);
        }
    }
}

public class QuotaAssignmentInfo
{
    public int Quota;
    public int Income;
    public int Duration;

    public float BaseIncreaseCoefficient;
    public float PolynomialIncreaseCoefficient;
    public float RandomizedIncreaseCoefficient;
    public int CombinedIncrease;
    public int NextQuota;

    public override string ToString()
    {
        return new StringBuilder("\n")
            .AppendLine("QuotaAssignmentInfo {")
            .AppendLine($"    Quota: {Quota}")
            .AppendLine($"    Income: {Income}")
            .AppendLine($"    Duration: {Duration}")
            .AppendLine($"    BaseIncreaseCoefficient: {BaseIncreaseCoefficient}")
            .AppendLine($"    PolynomialIncreaseCoefficient: {PolynomialIncreaseCoefficient}")
            .AppendLine($"    RandomizedIncreaseCoefficient: {RandomizedIncreaseCoefficient}")
            .AppendLine($"    CombinedIncrease: {CombinedIncrease}")
            .AppendLine($"    NextQuota: {NextQuota}")
            .AppendLine("}")
            .ToString();
    }
}

public class AugmentedQuotaVariables
{
    public int StartingQuota;
    public float IncreaseSteepness;
    public float IncreaseExponent;
    public float BaseIncrease;
    public float RandomizerMultiplier;
}
