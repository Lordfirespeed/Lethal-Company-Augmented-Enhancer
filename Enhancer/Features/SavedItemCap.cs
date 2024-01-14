using BepInEx.Logging;
using HarmonyLib;

namespace Enhancer.Features;

public class SavedItemCap : IFeature
{
    protected static ManualLogSource Logger { get; set; } = null!;

    public void SetLogger(ManualLogSource logger)
    {
        logger.LogDebug("Logger assigned.");
        Logger = logger;
    }

    [HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.SaveItemsInShip))]
    [HarmonyPrefix]
    static void SetSavedItemCap()
    {
        if (StartOfRound.Instance is null) return;
        StartOfRound.Instance.maxShipItemCapacity = Plugin.BoundConfig.SavedItemCap.Value;
        Logger.LogInfo($"Saved item cap set to {StartOfRound.Instance.maxShipItemCapacity}");
    }
}
