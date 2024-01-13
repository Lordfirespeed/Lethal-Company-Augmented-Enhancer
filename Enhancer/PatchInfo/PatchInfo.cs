using System;
using System.Linq;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using Enhancer.Extensions;
using Enhancer.Patches;
using HarmonyLib;
using UnityEngine.UIElements.Collections;

namespace Enhancer.PatchInfo;

internal static class PatchInfoInitializers
{
    public static Func<string, Harmony> HarmonyFactory { get; set; } =
        s => throw new InvalidOperationException("PatchInfo HarmonyFactory has not been initialized.");

    public static Func<string, ManualLogSource> LogSourceFactory { get; set; } =
        s => throw new InvalidOperationException("PatchInfo LogSourceFactory has not been initialized.");
}

internal class PatchInfo<TPatch> : IPatchInfo<TPatch> where TPatch : class, IPatch, new()
{
    // I want to use 'required' here but netstandard2.1 doesn't have support.
    public string Name { get; set; }
    public Func<bool>? EnabledCondition { get; set; }
    public ConfigEntryBase[] ListenToConfigEntries { get; set; } = Array.Empty<ConfigEntryBase>();
    public string[] DelegateToModGuids { get; set; } = Array.Empty<string>();
    private object PatchingLock { get; } = new();
    private Harmony? PatchHarmony { get; set; }
    private ManualLogSource? PatchLogger { get; set; }
    private TPatch? PatchInstance { get; set; }
    private readonly EventHandler<SettingChangedEventArgs> _onChangeEventHandler;
    private bool _disposed = false;

    public bool IsEnabled => EnabledCondition == null || EnabledCondition();
    public bool ShouldLoad => IsEnabled && !HasLoadedDelegate();

    public PatchInfo()
    {
        _onChangeEventHandler = (_, eventArgs) => {
            if (!ListenToConfigEntries.Contains(eventArgs.ChangedSetting)) return;
            OnChange();
        };
    }

    protected bool HasLoadedDelegate()
    {
        if (!Plugin.BoundConfig.DelegationEnabled.Value) return false;

        var delegateToPluginInfos = DelegateToModGuids
            .Select(guid => Chainloader.PluginInfos.Get(guid))
            .Where(info => info is not null)
            .Cast<PluginInfo>()
            .ToArray();
        if (!delegateToPluginInfos.Any())
            return false;

        Plugin.Logger.LogWarning(
            $"{Name} feature is disabled due to the presence of '{string.Join(", ", delegateToPluginInfos.Select(info => info.Metadata.Name))}'"
        );
        return true;
    }

    public void Initialise()
    {
        if (_disposed)
            throw new InvalidOperationException("PatchInfo has already been disposed!.");
        if (PatchHarmony is not null)
            throw new InvalidOperationException("PatchInfo has already been initialised!");

        PatchHarmony = PatchInfoInitializers.HarmonyFactory(typeof(TPatch).Name);
        PatchLogger = PatchInfoInitializers.LogSourceFactory(Name);

        ListenToConfigEntries
            .Do(entry => entry.ConfigFile.SettingChanged += _onChangeEventHandler);

        OnChange();
    }

    private void OnChange()
    {
        if (ShouldLoad) {
            if (PatchInstance is null) {
                Patch();
                return;
            }

            PatchInstance!.OnConfigChange();
            return;
        }

        Unpatch();
    }

    private void InstantiatePatch()
    {
        Plugin.Logger.LogDebug($"Instantiating patch...");
        PatchInstance = new TPatch();

        if (PatchLogger is null) {
            Plugin.Logger.LogWarning($"PatchLogger is null, using global logger for {Name}.");
            PatchInstance.SetLogger(Plugin.Logger);
            return;
        }

        Plugin.Logger.LogDebug($"Assigning logger...");
        PatchInstance.SetLogger(PatchLogger);
    }

    private void Patch()
    {
        lock (PatchingLock) {
            if (PatchHarmony is null)
                throw new Exception("PatchInfo has not been initialised. Cannot patch without a Harmony instance.");
            if (PatchInstance is not null) return;

            Plugin.Logger.LogInfo($"Attaching {Name} patches...");
            InstantiatePatch();
            PatchInstance!.OnPatch();
            PatchHarmony.PatchAllWithNestedTypes(typeof(TPatch));
        }
    }

    private void Unpatch()
    {
        lock (PatchingLock) {
            if (PatchHarmony is null)
                throw new Exception("PatchInfo has not been initialised. Cannot unpatch without a Harmony instance.");
            if (PatchInstance is null) return;

            Plugin.Logger.LogInfo($"Detaching {Name} patches...");
            PatchHarmony.UnpatchSelf();
            PatchInstance.OnUnpatch();
            PatchInstance = null;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing && PatchHarmony is not null) {
            ListenToConfigEntries
                .Do(entry => entry.ConfigFile.SettingChanged -= _onChangeEventHandler);
            Unpatch();
        }

        _disposed = true;
    }
}
