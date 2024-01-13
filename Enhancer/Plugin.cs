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

using BepInEx;
using BepInEx.Logging;
using UnityEngine;

namespace Enhancer;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("LC_API", "3.3.2")]
[BepInDependency("atomic.terminalapi", "1.5.0")]
[BepInDependency("mom.llama.enhancer", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("Haha.DynamicDeadline", BepInDependency.DependencyFlags.SoftDependency)]
public class Plugin : BaseUnityPlugin
{
    internal new static ManualLogSource Logger { get; private set; } = null!;
    internal static PluginConfig BoundConfig { get; private set; } = null!;

    private void Awake()
    {
        Logger = base.Logger;
        Logger.LogInfo("Binding config...");
        PluginConfig.RegisterTypeConverters();
        BoundConfig = new PluginConfig(this);

        EnhancerPatcher.Info = Info;
        EnhancerPatcher.Logger = Logger;
        EnhancerPatcher.BoundConfig = BoundConfig;

        var enhancerGameObject = new GameObject("Enhancer") {
            hideFlags = HideFlags.HideAndDontSave,
        };
        enhancerGameObject.AddComponent<EnhancerPatcher>();
        DontDestroyOnLoad(enhancerGameObject);
    }
}
