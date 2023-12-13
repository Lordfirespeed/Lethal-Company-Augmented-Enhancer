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
    public string Name { get; }
    private readonly Func<bool>? _enabledCondition;
    public bool IsEnabled => _enabledCondition == null || _enabledCondition();
    protected ConfigEntryBase[] ListenToConfigEntries { get; }
    protected string[] DelegateToModGuids { get; }
    private object PatchingLock { get; } = new();
    private Harmony? PatchHarmony { get; set; }
    private TPatch? PatchInstance { get; set; }

    private PatchInfo(
        string name,
        Func<bool>? enabledCondition,
        ConfigEntryBase[] listenToConfigEntries,
        string[] delegateToModGuids
    )
    {
        Name = name;
        _enabledCondition = enabledCondition;
        ListenToConfigEntries = listenToConfigEntries;
        DelegateToModGuids = delegateToModGuids;
    }

    public bool ShouldLoad => IsEnabled && !HasLoadedDelegate();

    protected bool HasLoadedDelegate() {
        if (!Plugin.BoundConfig.DelegationEnabled.Value) return false;

        var delegateToPluginInfosEnumerable = from delegateToModGuid in DelegateToModGuids
        select Chainloader.PluginInfos.Get(delegateToModGuid);
        var delegateToPluginInfos = delegateToPluginInfosEnumerable as BepInEx.PluginInfo [] ??
        delegateToPluginInfosEnumerable.ToArray();
        if (!delegateToPluginInfos.Any(info => info is not null)) return false;
        Plugin.Log.LogWarning(
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
            
            Plugin.Log.LogInfo($"Attaching {Name} patches...");
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
            
            Plugin.Log.LogInfo($"Detaching {Name} patches...");
            PatchHarmony.UnpatchSelf();
            PatchInstance.OnUnpatch();
            PatchInstance = null;
        }
    }
    
    public class Builder
    {
        private string? _thisName;
        private Func<bool>? _thisEnabledCondition;
        private readonly List<ConfigEntryBase> _thisListenToConfigEntries = new();
        private readonly List<string> _thisDelegateToModGuids = new();
            
        public Builder SetName(string newName)
        {
            _thisName = newName;
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

        public PatchInfo<TPatch> Build() => new(
            _thisName ?? throw new Exception("PatchInfo Name must be set."),
            _thisEnabledCondition,
            _thisListenToConfigEntries.ToArray(),
            _thisDelegateToModGuids.ToArray()
        );
    }
}