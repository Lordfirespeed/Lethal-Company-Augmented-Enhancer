using System;
using System.Linq;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using Enhancer.Extensions;
using Enhancer.Features;
using HarmonyLib;
using UnityEngine.UIElements.Collections;

namespace Enhancer.FeatureInfo;

internal static class FeatureInfoInitializers
{
    public static Func<string, Harmony> HarmonyFactory { get; set; } =
        s => throw new InvalidOperationException("PatchInfo HarmonyFactory has not been initialized.");

    public static Func<string, ManualLogSource> LogSourceFactory { get; set; } =
        s => throw new InvalidOperationException("PatchInfo LogSourceFactory has not been initialized.");
}

internal class FeatureInfo<TFeature> : IFeatureInfo<TFeature> where TFeature : class, IFeature, new()
{
    // I want to use 'required' here but netstandard2.1 doesn't have support.
    public string Name { get; set; }
    public Func<bool>? EnabledCondition { get; set; }
    public ConfigEntryBase[] ListenToConfigEntries { get; set; } = Array.Empty<ConfigEntryBase>();
    public string[] DelegateToModGuids { get; set; } = Array.Empty<string>();
    private object PatchingLock { get; } = new();
    private Harmony? FeatureHarmony { get; set; }
    private ManualLogSource? FeatureLogger { get; set; }
    private TFeature? FeatureInstance { get; set; }
    private readonly EventHandler<SettingChangedEventArgs> _onChangeEventHandler;
    private bool _disposed = false;

    public bool IsEnabled => EnabledCondition == null || EnabledCondition();
    public bool ShouldLoad => IsEnabled && !HasLoadedDelegate();

    public FeatureInfo()
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
        if (FeatureHarmony is not null)
            throw new InvalidOperationException("PatchInfo has already been initialised!");

        FeatureHarmony = FeatureInfoInitializers.HarmonyFactory(typeof(TFeature).Name);
        FeatureLogger = FeatureInfoInitializers.LogSourceFactory(Name);

        ListenToConfigEntries
            .Do(entry => entry.ConfigFile.SettingChanged += _onChangeEventHandler);

        OnChange();
    }

    private void OnChange()
    {
        if (ShouldLoad) {
            if (FeatureInstance is null) {
                Enable();
                return;
            }

            FeatureInstance!.OnConfigChange();
            return;
        }

        Disable();
    }

    private void InstantiatePatch()
    {
        Plugin.Logger.LogDebug($"Instantiating patch...");
        FeatureInstance = new TFeature();

        if (FeatureLogger is null) {
            Plugin.Logger.LogWarning($"PatchLogger is null, using global logger for {Name}.");
            FeatureInstance.SetLogger(Plugin.Logger);
            return;
        }

        Plugin.Logger.LogDebug($"Assigning logger...");
        FeatureInstance.SetLogger(FeatureLogger);
    }

    private void Enable()
    {
        lock (PatchingLock) {
            if (FeatureHarmony is null)
                throw new Exception("PatchInfo has not been initialised. Cannot patch without a Harmony instance.");
            if (FeatureInstance is not null) return;

            Plugin.Logger.LogInfo($"Attaching {Name} patches...");
            InstantiatePatch();
            FeatureInstance!.OnEnable();
            FeatureHarmony.PatchAllWithNestedTypes(typeof(TFeature));
        }
    }

    private void Disable()
    {
        lock (PatchingLock) {
            if (FeatureHarmony is null)
                throw new Exception("PatchInfo has not been initialised. Cannot unpatch without a Harmony instance.");
            if (FeatureInstance is null) return;

            Plugin.Logger.LogInfo($"Detaching {Name} patches...");
            FeatureHarmony.UnpatchSelf();
            FeatureInstance.OnDisable();
            FeatureInstance = null;
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

        if (disposing && FeatureHarmony is not null) {
            ListenToConfigEntries
                .Do(entry => entry.ConfigFile.SettingChanged -= _onChangeEventHandler);
            Disable();
        }

        _disposed = true;
    }
}
