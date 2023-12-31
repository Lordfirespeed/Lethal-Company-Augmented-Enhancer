using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using Enhancer.Patches;
using HarmonyLib;
using UnityEngine.UIElements.Collections;

namespace Enhancer.PatchInfo;

internal class PatchInfo<TPatch> : IPatchInfo<TPatch> where TPatch : class, IPatch, new()
{
    public required string Name { get; set; }
    public Func<bool>? EnabledCondition { get; set; }
    public ConfigEntryBase[] ListenToConfigEntries { get; set; } = Array.Empty<ConfigEntryBase>();
    public string[] DelegateToModGuids { get; set; } = Array.Empty<string>();
    private object PatchingLock { get; } = new();
    private Harmony? PatchHarmony { get; set; }
    private TPatch? PatchInstance { get; set; }
    
    public bool IsEnabled => EnabledCondition == null || EnabledCondition();
    public bool ShouldLoad => IsEnabled && !HasLoadedDelegate();

    protected bool HasLoadedDelegate() {
        if (!Plugin.BoundConfig.DelegationEnabled.Value) return false;

        var delegateToPluginInfos = DelegateToModGuids
            .Select(guid => Chainloader.PluginInfos.Get(guid))
            .ToArray();
        if (delegateToPluginInfos.Any(info => info is not null))
            return false;
        
        Plugin.Logger.LogWarning(
            $"{Name} feature is disabled due to the presence of '{String.Join(", ", delegateToPluginInfos.Select(info => info.Metadata.Name))}'"
        );
        return true;
    }

    public void Initialise(Func<string, Harmony> harmonyFactory)
    {
        if (PatchHarmony is not null)
            throw new Exception("PatchInfo has already been initialised!");

        PatchHarmony = harmonyFactory(typeof(TPatch).Name);
        var onChangeHandler = new EventHandler<SettingChangedEventArgs>(
            (sender, eventArgs) =>
            {
                if (!ListenToConfigEntries.Contains(eventArgs.ChangedSetting)) return;
                OnChange();
            }
        );
        ListenToConfigEntries
            .Do(entry => entry.ConfigFile.SettingChanged += onChangeHandler);

        OnChange();
    }
    
    private void OnChange()
    {
        if (ShouldLoad)
        {
            Patch();
            return;
        }
        
        Unpatch();
    }

    private void Patch()
    {
        lock (PatchingLock)
        {
            if (PatchHarmony is null)
                throw new Exception("PatchInfo has not been initialised. Cannot patch without a Harmony instance.");
            if (PatchInstance is not null) return;
            
            Plugin.Logger.LogInfo($"Attaching {Name} patches...");
            PatchInstance = new TPatch();
            PatchInstance.OnPatch();
            PatchHarmony.CreateClassProcessor(typeof(TPatch), true).Patch();
        }
    }

    private void Unpatch()
    {
        lock (PatchingLock)
        {
            if (PatchHarmony is null)
                throw new Exception("PatchInfo has not been initialised. Cannot unpatch without a Harmony instance.");
            if (PatchInstance is null) return;
            
            Plugin.Logger.LogInfo($"Detaching {Name} patches...");
            PatchHarmony.UnpatchSelf();
            PatchInstance.OnUnpatch();
            PatchInstance = null;
        }
    }
}