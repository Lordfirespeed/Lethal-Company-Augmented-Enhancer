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
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using Enhancer.Patches;
using HarmonyLib;
using UnityEngine.UIElements.Collections;

namespace Enhancer;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
[BepInDependency("mom.llama.enhancer", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("Haha.DynamicDeadline", BepInDependency.DependencyFlags.SoftDependency)]
public class Plugin : BaseUnityPlugin
{
    public static ManualLogSource Log { get; private set; }
    public static PluginConfig BoundConfig { get; private set; }
    
    private static readonly PatchInfo[] Patches = new[]
    {
        new PatchInfo.Builder()
            .SetName("Always show terminal")
            .SetPatchType(typeof(AlwaysShowTerminal))
            .SetLoadCondition(() => BoundConfig.KeepConsoleEnabled)
            .AddModGuidToDelegateTo("mom.llama.enhancer")
            .Build(),
        new PatchInfo.Builder()
            .SetName("Days per quota")
            .SetPatchType(typeof(DaysPerQuota))
            .SetLoadCondition(() => BoundConfig.DaysPerQuotaEnabled)
            .AddModGuidToDelegateTo("mom.llama.enhancer")
            .AddModGuidToDelegateTo("Haha.DynamicDeadline")
            .Build(),
        new PatchInfo.Builder()
            .SetName("Death penalty")
            .SetPatchType(typeof(DeathPenalty))
            .SetLoadCondition(() => BoundConfig.DeathPenaltyFormulaEnabled)
            .Build(),
        new PatchInfo.Builder()
            .SetName("Hangar door close duration")
            .SetPatchType(typeof(HangarDoorCloseDuration))
            .SetLoadCondition(() => BoundConfig.DoorPowerDurationEnabled)
            .AddModGuidToDelegateTo("mom.llama.enhancer")
            .Build(),
        new PatchInfo.Builder()
            .SetName("Improved scan command")
            .SetPatchType(typeof(ImprovedScanCommand))
            .AddModGuidToDelegateTo("mom.llama.enhancer")
            .Build(),
        new PatchInfo.Builder()
            .SetName("Item protection")
            .SetPatchType(typeof(ItemProtection))
            .AddModGuidToDelegateTo("mom.llama.enhancer")
            .Build(),
        new PatchInfo.Builder()
            .SetName("Price randomizer")
            .SetPatchType(typeof(PriceRandomizer))
            .AddModGuidToDelegateTo("mom.llama.enhancer")
            .Build(),
        new PatchInfo.Builder()
            .SetName("Quota formula")
            .SetPatchType(typeof(QuotaFormula))
            .SetLoadCondition(() => BoundConfig.QuotaFormulaEnabled)
            .Build(),
        new PatchInfo.Builder()
            .SetName("Starting credits")
            .SetPatchType(typeof(StartingCredits))
            .SetLoadCondition(() => BoundConfig.StartingCreditsEnabled)
            .Build(),
        new PatchInfo.Builder()
            .SetName("Suit unlock")
            .SetPatchType(typeof(UnlockSuits))
            .SetLoadCondition(() => BoundConfig.SuitUnlocksEnabled)
            .AddModGuidToDelegateTo("mom.llama.enhancer")
            .Build(),
        new PatchInfo.Builder()
            .SetName("Time speed")
            .SetPatchType(typeof(TimeSpeed))
            .SetLoadCondition(() => BoundConfig.TimeSpeedEnabled)
            .AddModGuidToDelegateTo("mom.llama.enhancer")
            .Build(),
    };
    
    private void Awake()
    {
        Log = Logger;
        BoundConfig = new(this);

        if (!BoundConfig.Enabled)
        {
            Logger.LogInfo("Globally disabled, exiting. Goodbye!");
            return;
        }

        Harmony patcher = new(PluginInfo.PLUGIN_GUID);
        
        Logger.LogInfo("Enabled, applying patches");
        foreach (var patch in Patches)
        {
            if (!patch.ShouldLoad()) continue;
            Logger.LogInfo($"Applying {patch.Name} patches...");
            patcher.PatchAll(patch.PatchType);
        }
    }

    private class PatchInfo
    {
        public string Name { get; private set; }
        public Type PatchType { get; private set; }
        private Func<bool> _loadCondition;
        private string[] _delegateToModGuids;

        public bool ShouldLoad() => (_loadCondition == null || _loadCondition()) && !HasLoadedDelegate();
        public bool HasLoadedDelegate()
        {
            if (!BoundConfig.DelegationEnabled) return false;
            
            var delegateToPluginInfosEnumerable = from delegateToModGuid in _delegateToModGuids 
                select Chainloader.PluginInfos.Get(delegateToModGuid);
            var delegateToPluginInfos = delegateToPluginInfosEnumerable as BepInEx.PluginInfo[] ?? delegateToPluginInfosEnumerable.ToArray();
            if (!delegateToPluginInfos.Any()) return false;
            Log.LogWarning($"{Name} feature is disabled due to the presence of '{String.Join(", ", delegateToPluginInfos.Select(info => info.Metadata.Name))}'");
            return true;
        }

        public class Builder
        {
            private string _thisName;
            private Type _thisPatchType;
            private Func<bool> _thisLoadCondition;
            private List<string> _thisDelegateToModGuids = new();
            
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

            public Builder SetLoadCondition(Func<bool> loadCondition)
            {
                _thisLoadCondition = loadCondition;
                return this;
            }

            public Builder AddModGuidToDelegateTo(string delegateToModGuid)
            {
                _thisDelegateToModGuids.Add(delegateToModGuid);
                return this;
            }

            public PatchInfo Build() => new()
            {
                Name = _thisName,
                PatchType = _thisPatchType,
                _loadCondition = _thisLoadCondition,
                _delegateToModGuids = _thisDelegateToModGuids.ToArray(),
            };
        }
    }
}