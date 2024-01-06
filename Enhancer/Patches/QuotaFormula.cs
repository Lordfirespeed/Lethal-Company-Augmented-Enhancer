using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
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
    protected static QuotaFormula? Instance { get; private set; }
    protected static readonly string QuotaVariablesKey = $"{MyPluginInfo.PLUGIN_GUID}-QuotaVariables";
    
    private static readonly MethodInfo FindObjectOfTypeTimeOfDayMethodInfo =
        AccessTools.Method(typeof(Object), nameof(Object.FindObjectOfType), generics: [typeof(TimeOfDay),]);

    private AugmentedQuotaVariables? _runQuotaVariables;
    public AugmentedQuotaVariables RunQuotaVariables {
        get {
            if (!NetworkManager.Singleton.IsConnectedClient)
                throw new InvalidOperationException("Not currently in-game!");
            
            _runQuotaVariables ??= ES3.Load<AugmentedQuotaVariables>(QuotaVariablesKey,
                GameNetworkManager.Instance.currentSaveFileName, defaultValue: null!);

            _runQuotaVariables ??= ConfiguredQuotaVariables;

            return _runQuotaVariables;
        }
    }

    public static AugmentedQuotaVariables ConfiguredQuotaVariables => new() {
        startingQuota = Plugin.BoundConfig.StartingQuota.Value,
        increaseSteepness = Plugin.BoundConfig.QuotaIncreaseSteepnessCoefficient.Value,
        increaseExponent = Plugin.BoundConfig.QuotaIncreaseSteepnessExponent.Value,
        baseIncrease = Plugin.BoundConfig.QuotaBaseIncrease.Value,
        randomizerMultiplier = Plugin.BoundConfig.QuotaIncreaseRandomFactor.Value,
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

    public static void SaveQuotaVariables()
    {
        if (ES3.KeyExists(QuotaVariablesKey, GameNetworkManager.Instance.currentSaveFileName)) return;
        ResetQuotaVariables();
    }

    public float ComputeBaseCoefficient(int timesFulfilledQuota)
    {
        // vanilla 'increase steepness' is actually 'increase shallowness', so we multiply by our value instead of dividing
        var value = 1f + Mathf.Pow(timesFulfilledQuota, RunQuotaVariables.increaseExponent) * RunQuotaVariables.increaseSteepness;
        Logger.LogDebug($"Computed base coefficient: {value}");
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
                    AccessTools.Method(typeof(QuotaFormula), nameof(ResetQuotaVariables))
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
                    AccessTools.Field(typeof(AugmentedQuotaVariables), nameof(AugmentedQuotaVariables.startingQuota))
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
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(QuotaFormula), nameof(SaveQuotaVariables)))
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
                    AccessTools.Field(typeof(AugmentedQuotaVariables), nameof(AugmentedQuotaVariables.startingQuota))
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
                    AccessTools.Field(typeof(AugmentedQuotaVariables), nameof(AugmentedQuotaVariables.startingQuota))
                )
            );
       
        return matcher.InstructionEnumeration();
    }
    
    [HarmonyPatch(typeof(TimeOfDay), nameof(TimeOfDay.SetNewProfitQuota))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> TranspileProfitQuota(IEnumerable<CodeInstruction> instructions)
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
            // Insert call to augmented base coefficient calculator
            .Insert(
                new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(QuotaFormula), nameof(Instance))),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(
                    OpCodes.Ldfld,
                    AccessTools.Field(typeof(TimeOfDay), nameof(TimeOfDay.timesFulfilledQuota))
                ),
                new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(QuotaFormula), nameof(ComputeBaseCoefficient)))
            )
            // Advance to vanilla variables baseIncrease reference
            .MatchForward(
                false,
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(TimeOfDay), nameof(TimeOfDay.quotaVariables))),
                new CodeMatch(
                    OpCodes.Ldfld,
                    AccessTools.Field(typeof(QuotaSettings), nameof(QuotaSettings.baseIncrease))
                )
            )
            // Remove vanilla variables baseIncrease reference
            .RemoveInstructions(3)
            // Insert reference to augmented variables baseIncrease
            .Insert(
                new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(QuotaFormula), nameof(Instance))),
                new CodeInstruction(
                    OpCodes.Call,
                    AccessTools.PropertyGetter(typeof(QuotaFormula), nameof(RunQuotaVariables))
                ),
                new CodeInstruction(
                    OpCodes.Ldfld,
                    AccessTools.Field(typeof(AugmentedQuotaVariables), nameof(AugmentedQuotaVariables.baseIncrease))
                )
            )
            // Advance to vanilla variables randomizerMultiplier reference
            .MatchForward(
                false,
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(TimeOfDay), nameof(TimeOfDay.quotaVariables))),
                new CodeMatch(
                    OpCodes.Ldfld,
                    AccessTools.Field(typeof(QuotaSettings), nameof(QuotaSettings.randomizerMultiplier))
                )
            )
            // Remove vanilla variables randomizerMultiplier reference
            .RemoveInstructions(3)
            // Insert reference to augmented variables randomizerMultiplier
            .Insert(
                new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(QuotaFormula), nameof(Instance))),
                new CodeInstruction(
                    OpCodes.Call,
                    AccessTools.PropertyGetter(typeof(QuotaFormula), nameof(RunQuotaVariables))
                ),
                new CodeInstruction(
                    OpCodes.Ldfld,
                    AccessTools.Field(typeof(AugmentedQuotaVariables), nameof(AugmentedQuotaVariables.randomizerMultiplier))
                )
            );

        return matcher.InstructionEnumeration();
    }
}

[Serializable]
public class AugmentedQuotaVariables
{
    public int startingQuota;
    public float increaseSteepness;
    public float increaseExponent;
    public float baseIncrease;
    public float randomizerMultiplier;
}