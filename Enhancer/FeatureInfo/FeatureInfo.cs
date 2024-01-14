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
        s => throw new InvalidOperationException("FeatureInfo HarmonyFactory has not been initialized.");

    public static Func<string, ManualLogSource> LogSourceFactory { get; set; } =
        s => throw new InvalidOperationException("FeatureInfo LogSourceFactory has not been initialized.");
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
            throw new InvalidOperationException("FeatureInfo has already been disposed!.");
        if (FeatureHarmony is not null)
            throw new InvalidOperationException("FeatureInfo has already been initialised!");

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

    private void InstantiateFeature()
    {
        if (FeatureHarmony is null)
            throw new Exception("FeatureInfo has not been initialised. Cannot patch without a Harmony instance.");
        if (FeatureLogger is null)
            throw new Exception("FeatureInfo has not been initialised. Cannot patch without a Logger instance.");

        Plugin.Logger.LogDebug("Instantiating feature...");
        FeatureInstance = new TFeature();

        Plugin.Logger.LogDebug("Assigning logger...");
        FeatureInstance.SetLogger(FeatureLogger);

        Plugin.Logger.LogDebug("Assigning harmony...");
        FeatureInstance.SetHarmony(FeatureHarmony);
    }

    private void Enable()
    {
        lock (PatchingLock) {
            if (FeatureHarmony is null)
                throw new Exception("FeatureInfo has not been initialised. Cannot patch without a Harmony instance.");
            if (FeatureInstance is not null) return;

            Plugin.Logger.LogInfo($"Enabling {Name} feature...");
            InstantiateFeature();
            FeatureInstance!.OnEnable();
            FeatureHarmony.PatchAllWithNestedTypes(typeof(TFeature));
        }
    }

    private void Disable()
    {
        lock (PatchingLock) {
            if (FeatureHarmony is null)
                throw new Exception("FeatureInfo has not been initialised. Cannot unpatch without a Harmony instance.");
            if (FeatureInstance is null) return;

            Plugin.Logger.LogInfo($"Disabling {Name} feature...");
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
