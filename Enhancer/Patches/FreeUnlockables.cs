using System.Linq;
using BepInEx.Logging;
using Enhancer.Extensions;
using HarmonyLib;

namespace Enhancer.Patches;

public class FreeUnlockables : IPatch
{
    protected static ManualLogSource Logger { get; set; } = null!;

    public void SetLogger(ManualLogSource logger)
    {
        logger.LogDebug("Logger assigned.");
        Logger = logger;
    }

    [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.Start))]
    [HarmonyPostfix]
    public static void StartOfRoundSuitPatch(StartOfRound __instance)
    {
        Logger.LogInfo("Unlocking items...");

        __instance.unlockablesList.unlockables
            .Select((unlockable, index) => new { unlockable, index })
            .Tap(item => Logger.LogDebug($"Unlockable item {item.index}: '{item.unlockable.unlockableName}'"))
            .Where(item => {
                return Plugin.BoundConfig.FreeUnlockablesList.Value.Contains(item.unlockable.unlockableName);
            })
            .Do(item => __instance.SpawnUnlockable(item.index));
    }
}
