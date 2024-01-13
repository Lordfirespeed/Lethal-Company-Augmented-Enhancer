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

using System.Collections.Generic;
using BepInEx;
using BepInEx.Logging;
using Enhancer.Patches;
using Enhancer.PatchInfo;
using HarmonyLib;

namespace Enhancer;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("LC_API", "3.3.2")]
[BepInDependency("atomic.terminalapi", "1.5.0")]
[BepInDependency("mom.llama.enhancer", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("Haha.DynamicDeadline", BepInDependency.DependencyFlags.SoftDependency)]
public class Plugin : BaseUnityPlugin
{
    public new static ManualLogSource Logger { get; private set; } = null!;
    public static PluginConfig BoundConfig { get; private set; } = null!;

    private IEnumerable<IPatchInfo<IPatch>> GetPatches() => [
        new PatchInfo<AlwaysShowTerminal> {
            Name = "Always show terminal",
            EnabledCondition = () => BoundConfig.Enabled.Value && BoundConfig.KeepConsoleEnabled.Value,
            ListenToConfigEntries = [BoundConfig.KeepConsoleEnabled],
            DelegateToModGuids = ["mom.llama.enhancer"],
        },
        new PatchInfo<DaysPerQuota> {
            Name = "Days per quota",
            EnabledCondition = () => BoundConfig.Enabled.Value && BoundConfig.DaysPerQuotaAssignmentEnabled.Value && BoundConfig.QuotaFormulaEnabled.Value,
            ListenToConfigEntries = [BoundConfig.DaysPerQuotaAssignmentEnabled, BoundConfig.QuotaFormulaEnabled],
            DelegateToModGuids = ["mom.llama.enhancer", "Haha.DynamicDeadline"],
        },
        new PatchInfo<DeathPenalty> {
            Name = "Death penalty",
            EnabledCondition = () => BoundConfig.Enabled.Value && BoundConfig.DeathPenaltyFormulaEnabled.Value,
            ListenToConfigEntries = [BoundConfig.DeathPenaltyFormulaEnabled],
        },
        new PatchInfo<HangarDoorCloseDuration> {
            Name = "Hangar door close duration",
            EnabledCondition = () => BoundConfig.Enabled.Value && BoundConfig.DoorPowerDurationEnabled.Value,
            ListenToConfigEntries = [BoundConfig.DoorPowerDurationEnabled],
            DelegateToModGuids = ["mom.llama.enhancer"],
        },
        new PatchInfo<ThreatScannerInScanCommand> {
            Name = "Threat scanner",
            EnabledCondition = () =>
                BoundConfig.Enabled.Value && BoundConfig.ThreatScanner.Value is not ThreatScannerMode.Disabled,
            ListenToConfigEntries = [BoundConfig.ThreatScanner],
            DelegateToModGuids = ["mom.llama.enhancer"],
        },
        new PatchInfo<ItemProtection> {
            Name = "Item protection",
            EnabledCondition = () => BoundConfig.Enabled.Value && BoundConfig.ScrapProtectionEnabled.Value,
            ListenToConfigEntries = [BoundConfig.ScrapProtectionEnabled],
            DelegateToModGuids = ["mom.llama.enhancer"],
        },
        new PatchInfo<CompanyBuyingFactorTweaks> {
            Name = "Company buying factor tweaks",
            EnabledCondition = () => BoundConfig.Enabled.Value && BoundConfig.CompanyBuyingFactorTweaksEnabled.Value,
            DelegateToModGuids = ["mom.llama.enhancer"],
        },
        new PatchInfo<QuotaFormula> {
            Name = "Quota formula",
            EnabledCondition = () => BoundConfig.Enabled.Value && BoundConfig.QuotaFormulaEnabled.Value,
            ListenToConfigEntries = [BoundConfig.QuotaFormulaEnabled],
        },
        new PatchInfo<ScrapTweaks> {
            Name = "Scrap Tweaks",
            EnabledCondition = () => BoundConfig.Enabled.Value && BoundConfig.ScrapTweaksEnabled.Value,
            ListenToConfigEntries = [BoundConfig.ScrapPlayercountScaling, BoundConfig.ScrapQuantityScalar, BoundConfig.ScrapValueScalar]
        },
        new PatchInfo<StartingCredits> {
            Name = "Starting credits",
            EnabledCondition = () => BoundConfig.Enabled.Value && BoundConfig.StartingCreditsEnabled.Value,
            ListenToConfigEntries = [BoundConfig.StartingCreditsEnabled],
        },
        new PatchInfo<PassiveIncome> {
            Name = "Passive income",
            EnabledCondition = () => BoundConfig.Enabled.Value && BoundConfig.PassiveIncomeEnabled.Value,
            ListenToConfigEntries = [BoundConfig.PassiveIncomeEnabled],
        },
        new PatchInfo<UnlockSuits> {
            Name = "Suit unlock",
            EnabledCondition = () => BoundConfig.Enabled.Value && BoundConfig.SuitUnlocksEnabled.Value,
            ListenToConfigEntries = [BoundConfig.SuitUnlocksEnabled],
            DelegateToModGuids = ["mom.llama.enhancer"],
        },
        new PatchInfo<TimeSpeed> {
            Name = "Time speed",
            EnabledCondition = () => BoundConfig.Enabled.Value && BoundConfig.TimeSpeedEnabled.Value,
            ListenToConfigEntries = [BoundConfig.TimeSpeedEnabled],
            DelegateToModGuids = ["mom.llama.enhancer"],
        }
    ];

    private void Awake()
    {
        Logger = base.Logger;
        Logger.LogInfo("Binding config...");
        PluginConfig.RegisterTypeConverters();
        BoundConfig = new PluginConfig(this);

        /*
        var go = new GameObject("Enhancer");
        go.hideFlags = HideFlags.HideAndDontSave;
        go.AddComponent<EnhancerPatcher>();
        DontDestroyOnLoad(go);
        */

        PatchInfoInitializers.HarmonyFactory =
            harmonyName => new Harmony($"{MyPluginInfo.PLUGIN_GUID}-{harmonyName}");
        PatchInfoInitializers.LogSourceFactory =
            patchName => BepInEx.Logging.Logger.CreateLogSource($"{MyPluginInfo.PLUGIN_NAME}/{patchName}");

        Logger.LogInfo("Initialising patches...");
        GetPatches().Do(patch => patch.Initialise());
        Logger.LogInfo("Done!");
    }

    private void OnDestroy()
    {
        GetPatches().Do(patch => patch.Dispose());
    }
}
