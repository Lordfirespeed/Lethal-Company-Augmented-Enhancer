using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using BepInEx.Logging;
using HarmonyLib;
using NetworkManager = Unity.Netcode.NetworkManager;
using Object = UnityEngine.Object;

namespace Enhancer.Patches;

public class QuotaFormula : IPatch
{
    protected static ManualLogSource Logger { get; set; } = null!;
    protected static QuotaFormula? Instance = null;
    protected static string QuotaVariablesKey = $"{MyPluginInfo.PLUGIN_GUID}-QuotaVariables";
    
    private static readonly MethodInfo FindObjectOfTypeTimeOfDayMethodInfo =
        AccessTools.Method(typeof(Object), nameof(Object.FindObjectOfType), generics: [typeof(TimeOfDay),]);

    private static readonly MethodInfo SaveQuotaVariablesMethodInfo =
        AccessTools.Method(typeof(QuotaFormula), nameof(SaveQuotaVariables));
    
    private static readonly MethodInfo SaveQuotaVariablesIfNotFoundMethodInfo =
        AccessTools.Method(typeof(QuotaFormula), nameof(SaveQuotaVariablesIfNotFound));

    private AugmentedQuotaVariables? _quotaVariables;
    public AugmentedQuotaVariables QuotaVariables {
        get {
            if (!NetworkManager.Singleton.IsConnectedClient)
                throw new InvalidOperationException("Not currently in-game!");
            
            _quotaVariables ??= ES3.Load<AugmentedQuotaVariables>(QuotaVariablesKey,
                GameNetworkManager.Instance.currentSaveFileName, defaultValue: null!);

            _quotaVariables ??= new AugmentedQuotaVariables {
                startingQuota = Plugin.BoundConfig.StartingQuota.Value,
                increaseSteepness = Plugin.BoundConfig.QuotaIncreaseSteepnessCoefficient.Value,
                increaseExponent = Plugin.BoundConfig.QuotaIncreaseSteepnessExponent.Value,
                baseIncrease = Plugin.BoundConfig.QuotaBaseIncrease.Value,
                randomizerMultiplier = Plugin.BoundConfig.QuotaIncreaseRandomFactor.Value,
            };

            return _quotaVariables;
        }
    }

    public void SetLogger(ManualLogSource logger)
    {
        logger.LogDebug("Logger assigned.");
        Logger = logger;
        
        _quotaVariables ??= ES3.Load<AugmentedQuotaVariables>(QuotaVariablesKey,
            GameNetworkManager.Instance.currentSaveFileName, defaultValue: null!);

        _quotaVariables ??= new AugmentedQuotaVariables {
            startingQuota = Plugin.BoundConfig.StartingQuota.Value,
            increaseSteepness = Plugin.BoundConfig.QuotaIncreaseSteepnessCoefficient.Value,
            increaseExponent = Plugin.BoundConfig.QuotaIncreaseSteepnessExponent.Value,
            baseIncrease = Plugin.BoundConfig.QuotaBaseIncrease.Value,
            randomizerMultiplier = Plugin.BoundConfig.QuotaIncreaseRandomFactor.Value,
        };
        
    }

    public void OnPatch()
    {
        Instance = this;
    }

    public void OnUnpatch()
    {
        Instance = null;
    }

    [HarmonyPatch(typeof(Terminal), nameof(Terminal.Start))]
    [HarmonyPrefix]
    public static void StartOfRoundShipStartPre()
    {
        Logger.LogInfo("Setting quota formula variables...");
        var quotaSettings = TimeOfDay.Instance.quotaVariables;
        quotaSettings.startingQuota = Plugin.BoundConfig.StartingQuota.Value;
        // vanilla 'increase steepness' is actually 'increase shallowness', so we reciprocate (1/x) the value
        quotaSettings.increaseSteepness = 1f / Plugin.BoundConfig.QuotaIncreaseSteepnessCoefficient.Value;
        quotaSettings.baseIncrease = Plugin.BoundConfig.QuotaBaseIncrease.Value;
        quotaSettings.randomizerMultiplier = Plugin.BoundConfig.QuotaIncreaseRandomFactor.Value;
    }

    private static AugmentedQuotaVariables CurrentQuotaVariables() => new AugmentedQuotaVariables {
        startingQuota = Plugin.BoundConfig.StartingQuota.Value,
        increaseSteepness = Plugin.BoundConfig.QuotaIncreaseSteepnessCoefficient.Value,
        increaseExponent = Plugin.BoundConfig.QuotaIncreaseSteepnessExponent.Value,
        baseIncrease = Plugin.BoundConfig.QuotaBaseIncrease.Value,
        randomizerMultiplier = Plugin.BoundConfig.QuotaIncreaseRandomFactor.Value,
    };
    
    public static void SaveQuotaVariables()
    {
        var variables = CurrentQuotaVariables();

        Instance!._quotaVariables = variables;
        ES3.Save(QuotaVariablesKey, variables, GameNetworkManager.Instance.currentSaveFileName);
        Logger.LogDebug("Saved quota variables to current save file.");
    }

    public static void SaveQuotaVariablesIfNotFound()
    {
        if (ES3.KeyExists(QuotaVariablesKey, GameNetworkManager.Instance.currentSaveFileName)) return;
        SaveQuotaVariables();
    }
    
    [HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.ResetSavedGameValues))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> TranspileResetSavedGameValues(IEnumerable<CodeInstruction> instructions)
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
            ).MatchForward(false,
                new CodeMatch { labels = [(Label)matcher.Operand] }
            ).MatchBack(false,
            new CodeMatch(OpCodes.Call)
            ).Insert(
                new CodeInstruction(OpCodes.Call, SaveQuotaVariablesMethodInfo)
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
            ).MatchForward(false,
                new CodeMatch { labels = [(Label)matcher.Operand] }
            ).MatchBack(false,
                new CodeMatch(OpCodes.Call)
            ).Insert(
                new CodeInstruction(OpCodes.Call, SaveQuotaVariablesIfNotFoundMethodInfo)
            );

        return matcher.InstructionEnumeration();
    }
    
    [HarmonyPatch(typeof(TimeOfDay), nameof(TimeOfDay.SetNewProfitQuota))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> TranspileProfitQuota(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions);

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