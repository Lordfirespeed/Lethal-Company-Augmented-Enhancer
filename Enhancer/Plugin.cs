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
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using Enhancer.Patches;
using HarmonyLib;
using MonoMod.RuntimeDetour;
using UnityEngine.UIElements.Collections;

namespace Enhancer;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInDependency("mom.llama.enhancer", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("Haha.DynamicDeadline", BepInDependency.DependencyFlags.SoftDependency)]
public class Plugin : BaseUnityPlugin
{
    public static ManualLogSource Log { get; private set; } = null!;
    public static PluginConfig BoundConfig { get; private set; } = null!;
    
    private PatchInfo[] GetPatches() => new[]
    {
        new PatchInfo.Builder()
            .SetName("Always show terminal")
            .SetPatchType(typeof(AlwaysShowTerminal))
            .SetEnabledCondition(() => BoundConfig.KeepConsoleEnabled.Value)
            .ListenTo(BoundConfig.KeepConsoleEnabled)
            .AddModGuidToDelegateTo("mom.llama.enhancer")
            .Build(),
        new PatchInfo.Builder()
            .SetName("Days per quota")
            .SetPatchType(typeof(DaysPerQuota))
            .SetEnabledCondition(() => BoundConfig.DaysPerQuotaEnabled.Value)
            .ListenTo(BoundConfig.DaysPerQuotaEnabled)
            .AddModGuidToDelegateTo("mom.llama.enhancer")
            .AddModGuidToDelegateTo("Haha.DynamicDeadline")
            .Build(),
        new PatchInfo.Builder()
            .SetName("Death penalty")
            .SetPatchType(typeof(DeathPenalty))
            .SetEnabledCondition(() => BoundConfig.DeathPenaltyFormulaEnabled.Value)
            .ListenTo(BoundConfig.DeathPenaltyFormulaEnabled)
            .Build(),
        new PatchInfo.Builder()
            .SetName("Hangar door close duration")
            .SetPatchType(typeof(HangarDoorCloseDuration))
            .SetEnabledCondition(() => BoundConfig.DoorPowerDurationEnabled.Value)
            .ListenTo(BoundConfig.DoorPowerDurationEnabled)
            .AddModGuidToDelegateTo("mom.llama.enhancer")
            .Build(),
        new PatchInfo.Builder()
            .SetName("Threat scanner")
            .SetPatchType(typeof(ThreatScannerInScanCommand))
            .SetEnabledCondition(() => BoundConfig.ThreatScanner.Value is not ThreatScannerMode.Disabled)
            .ListenTo(BoundConfig.ThreatScanner)
            .AddModGuidToDelegateTo("mom.llama.enhancer")
            .Build(),
        new PatchInfo.Builder()
            .SetName("Item protection")
            .SetPatchType(typeof(ItemProtection))
            .SetEnabledCondition(() => BoundConfig.ScrapProtectionEnabled.Value)
            .ListenTo(BoundConfig.ScrapProtectionEnabled)
            .AddModGuidToDelegateTo("mom.llama.enhancer")
            .Build(),
        new PatchInfo.Builder()
            .SetName("Price randomizer")
            .SetPatchType(typeof(CompanyBuyingFactorRandomizer))
            .AddModGuidToDelegateTo("mom.llama.enhancer")
            .Build(),
        new PatchInfo.Builder()
            .SetName("Quota formula")
            .SetPatchType(typeof(QuotaFormula))
            .SetEnabledCondition(() => BoundConfig.QuotaFormulaEnabled.Value)
            .ListenTo(BoundConfig.QuotaFormulaEnabled)
            .Build(),
        new PatchInfo.Builder()
            .SetName("Starting credits")
            .SetPatchType(typeof(StartingCredits))
            .SetEnabledCondition(() => BoundConfig.StartingCreditsEnabled.Value)
            .ListenTo(BoundConfig.StartingCreditsEnabled)
            .Build(),
        new PatchInfo.Builder()
            .SetName("Passive income")
            .SetPatchType(typeof(PassiveIncome))
            .SetEnabledCondition(() => BoundConfig.PassiveIncomeEnabled.Value)
            .ListenTo(BoundConfig.PassiveIncomeEnabled)
            .Build(),
        new PatchInfo.Builder()
            .SetName("Suit unlock")
            .SetPatchType(typeof(UnlockSuits))
            .SetEnabledCondition(() => BoundConfig.SuitUnlocksEnabled.Value)
            .ListenTo(BoundConfig.SuitUnlocksEnabled)
            .AddModGuidToDelegateTo("mom.llama.enhancer")
            .Build(),
        new PatchInfo.Builder()
            .SetName("Time speed")
            .SetPatchType(typeof(TimeSpeed))
            .SetEnabledCondition(() => BoundConfig.TimeSpeedEnabled.Value)
            .ListenTo(BoundConfig.TimeSpeedEnabled)
            .AddModGuidToDelegateTo("mom.llama.enhancer")
            .Build(),
    };
    
    private void Awake()
    {
        Log = Logger;
        BoundConfig = new(this);

        if (!BoundConfig.Enabled.Value)
        {
            Logger.LogInfo("Globally disabled, exiting. Goodbye!");
            return;
        }

        Harmony harmony = new(PluginInfo.PLUGIN_GUID);
        
        Logger.LogInfo("Enabled, initialising patches...");
        GetPatches().Do(patch => patch.Initialise(harmony));
        Logger.LogInfo("Done!");
    }

    private class PatchInfo
    {
        public string Name { get; }
        public Type PatchType { get; }
        private readonly Func<bool>? _enabledCondition;
        private ConfigEntryBase[] _listenToConfigEntries;
        private readonly string[] _delegateToModGuids;
        private List<MethodInfo>? _patchedMethods;
        private Harmony? _harmony;
        private readonly object _patchLock = new();

        private PatchInfo(
            string name, 
            Type patchType, 
            Func<bool>? enabledCondition, 
            ConfigEntryBase[] listenToConfigEntries, 
            string[] delegateToModGuids
        ) {
            Name = name;
            PatchType = patchType;
            _enabledCondition = enabledCondition;
            _listenToConfigEntries = listenToConfigEntries;
            _delegateToModGuids = delegateToModGuids;
        }

        public bool IsEnabled() => (_enabledCondition == null || _enabledCondition()) && !HasLoadedDelegate();
        public bool HasLoadedDelegate()
        {
            if (!BoundConfig.DelegationEnabled.Value) return false;
            
            var delegateToPluginInfosEnumerable = from delegateToModGuid in _delegateToModGuids 
                select Chainloader.PluginInfos.Get(delegateToModGuid);
            var delegateToPluginInfos = delegateToPluginInfosEnumerable as BepInEx.PluginInfo[] ?? delegateToPluginInfosEnumerable.ToArray();
            if (!delegateToPluginInfos.Any(info => info is not null)) return false;
            Log.LogWarning($"{Name} feature is disabled due to the presence of '{String.Join(", ", delegateToPluginInfos.Select(info => info.Metadata.Name))}'");
            return true;
        }

        public void Initialise(Harmony harmony)
        {
            if (_harmony is not null)
                throw new Exception("PatchInfo has already been initialised!");

            _harmony = harmony;
            var onChangeHandler = new EventHandler<SettingChangedEventArgs>(
                (sender, eventArgs) =>
                {
                    if (!_listenToConfigEntries.Contains(eventArgs.ChangedSetting)) return;
                    OnChange();
                }
            );
            _listenToConfigEntries
                .Do(entry => entry.ConfigFile.SettingChanged += onChangeHandler);

            OnChange();
        }

        private void OnChange()
        {
            if (IsEnabled())
            {
                Patch();
                return;
            }
            
            Unpatch();
        }

        private void Patch()
        {
            lock (_patchLock)
            {
                if (_harmony is null)
                    throw new Exception("PatchInfo has not been initialised. Cannot patch without a Harmony instance.");
                if (_patchedMethods is not null) return;
                
                Log.LogInfo($"Attaching {Name} patches...");
                _patchedMethods = _harmony.CreateClassProcessor(PatchType, true).Patch();
            }
        }

        private void Unpatch()
        {
            lock (_patchLock)
            {
                if (_harmony is null)
                    throw new Exception("PatchInfo has not been initialised. Cannot unpatch without a Harmony instance.");
                if (_patchedMethods is null) return;
                
                Log.LogInfo($"Detaching ${Name} patches...");
                _patchedMethods.Do(original => _harmony.Unpatch(original.GetIdentifiable(), HarmonyPatchType.All, _harmony.Id));
                _patchedMethods = null;
            }
        }

        public class Builder
        {
            private string? _thisName;
            private Type? _thisPatchType;
            private Func<bool>? _thisEnabledCondition;
            private readonly List<ConfigEntryBase> _thisListenToConfigEntries = new();
            private readonly List<string> _thisDelegateToModGuids = new();
            
            public Builder SetName(string newName)
            {
                 _thisName = newName;
                 return this;
            }

            public Builder SetPatchType(Type newPatchType)
            {
                _thisPatchType = newPatchType;
                return this;
            }

            public Builder ListenTo(ConfigEntryBase configEntry)
            {
                _thisListenToConfigEntries.Add(configEntry);
                return this;
            }

            public Builder SetEnabledCondition(Func<bool> loadCondition)
            {
                _thisEnabledCondition = loadCondition;
                return this;
            }

            public Builder AddModGuidToDelegateTo(string delegateToModGuid)
            {
                _thisDelegateToModGuids.Add(delegateToModGuid);
                return this;
            }

            public PatchInfo Build() => new(
                _thisName ?? throw new Exception("PatchInfo Name must be set."),
                _thisPatchType ?? throw new Exception("PatchInfo PatchType must be set."), 
                _thisEnabledCondition,
                _thisListenToConfigEntries.ToArray(),
                _thisDelegateToModGuids.ToArray()
            );
        }
    }
}