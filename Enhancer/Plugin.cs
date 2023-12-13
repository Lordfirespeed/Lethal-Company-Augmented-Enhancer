/**********************************************************
    Single Player Enhancements Mod for Lethal Company

    Authors:
        Mama Llama
        Flowerful
        Lordfirespeed

    See LICENSE.md for information about copying
    distributing this project

    See Docs/Installation.md for information on 
    how to use this mod in your game
***********************************************************/

using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using Enhancer.Patches;
using Enhancer.PatchInfo;
using HarmonyLib;

namespace Enhancer;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("mom.llama.enhancer", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("Haha.DynamicDeadline", BepInDependency.DependencyFlags.SoftDependency)]
public class Plugin : BaseUnityPlugin
{
    public new static ManualLogSource Logger { get; private set; } = null!;
    public static PluginConfig BoundConfig { get; private set; } = null!;
    
    private IEnumerable<IPatchInfo<IPatch>> GetPatches() => new List<IPatchInfo<IPatch>>
    {
        new PatchInfo<AlwaysShowTerminal>.Builder()
            .SetName("Always show terminal")
            .SetEnabledCondition(() => BoundConfig.KeepConsoleEnabled.Value)
            .ListenTo(BoundConfig.KeepConsoleEnabled)
            .AddModGuidToDelegateTo("mom.llama.enhancer")
            .Build(),
        new PatchInfo<DaysPerQuota>.Builder()
            .SetName("Days per quota")
            .SetEnabledCondition(() => BoundConfig.DaysPerQuotaEnabled.Value)
            .ListenTo(BoundConfig.DaysPerQuotaEnabled)
            .AddModGuidToDelegateTo("mom.llama.enhancer")
            .AddModGuidToDelegateTo("Haha.DynamicDeadline")
            .Build(),
        new PatchInfo<DeathPenalty>.Builder()
            .SetName("Death penalty")
            .SetEnabledCondition(() => BoundConfig.DeathPenaltyFormulaEnabled.Value)
            .ListenTo(BoundConfig.DeathPenaltyFormulaEnabled)
            .Build(),
        new PatchInfo<HangarDoorCloseDuration>.Builder()
            .SetName("Hangar door close duration")
            .SetEnabledCondition(() => BoundConfig.DoorPowerDurationEnabled.Value)
            .ListenTo(BoundConfig.DoorPowerDurationEnabled)
            .AddModGuidToDelegateTo("mom.llama.enhancer")
            .Build(),
        new PatchInfo<ThreatScannerInScanCommand>.Builder()
            .SetName("Threat scanner")
            .SetEnabledCondition(() => BoundConfig.ThreatScanner.Value is not ThreatScannerMode.Disabled)
            .ListenTo(BoundConfig.ThreatScanner)
            .AddModGuidToDelegateTo("mom.llama.enhancer")
            .Build(),
        new PatchInfo<ItemProtection>.Builder()
            .SetName("Item protection")
            .SetEnabledCondition(() => BoundConfig.ScrapProtectionEnabled.Value)
            .ListenTo(BoundConfig.ScrapProtectionEnabled)
            .AddModGuidToDelegateTo("mom.llama.enhancer")
            .Build(),
        new PatchInfo<CompanyBuyingFactorRandomizer>.Builder()
            .SetName("Price randomizer")
            .AddModGuidToDelegateTo("mom.llama.enhancer")
            .Build(),
        new PatchInfo<QuotaFormula>.Builder()
            .SetName("Quota formula")
            .SetEnabledCondition(() => BoundConfig.QuotaFormulaEnabled.Value)
            .ListenTo(BoundConfig.QuotaFormulaEnabled)
            .Build(),
        new PatchInfo<StartingCredits>.Builder()
            .SetName("Starting credits")
            .SetEnabledCondition(() => BoundConfig.StartingCreditsEnabled.Value)
            .ListenTo(BoundConfig.StartingCreditsEnabled)
            .Build(),
        new PatchInfo<PassiveIncome>.Builder()
            .SetName("Passive income")
            .SetEnabledCondition(() => BoundConfig.PassiveIncomeEnabled.Value)
            .ListenTo(BoundConfig.PassiveIncomeEnabled)
            .Build(),
        new PatchInfo<UnlockSuits>.Builder()
            .SetName("Suit unlock")
            .SetEnabledCondition(() => BoundConfig.SuitUnlocksEnabled.Value)
            .ListenTo(BoundConfig.SuitUnlocksEnabled)
            .AddModGuidToDelegateTo("mom.llama.enhancer")
            .Build(),
        new PatchInfo<TimeSpeed>.Builder()
            .SetName("Time speed")
            .SetEnabledCondition(() => BoundConfig.TimeSpeedEnabled.Value)
            .ListenTo(BoundConfig.TimeSpeedEnabled)
            .AddModGuidToDelegateTo("mom.llama.enhancer")
            .Build(),
    };
    
    private void Awake()
    {
        Logger = base.Logger;
        Logger.LogInfo("Binding config...");
        BoundConfig = new(this);

        if (!BoundConfig.Enabled.Value)
        {
            Logger.LogInfo("Globally disabled, exiting. Goodbye!");
            return;
        }
        
        var harmonyFactory = (string harmonyName) => new Harmony(String.Join(MyPluginInfo.PLUGIN_GUID, ".", harmonyName));
        
        Logger.LogInfo("Enabled, initialising patches...");
        GetPatches().Do(patch => patch.Initialise(harmonyFactory));
        Logger.LogInfo("Done!");
    }
}