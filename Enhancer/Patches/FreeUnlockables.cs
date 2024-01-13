using System.Collections.Generic;
using System.Linq;
using BepInEx.Logging;
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

    private static readonly HashSet<string> UnlockableNamesToSpawn = ["Green suit", "Hazard suit"];

    [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.Start))]
    [HarmonyPostfix]
    public static void StartOfRoundSuitPatch(StartOfRound __instance)
    {
        Logger.LogInfo("Setting unlocked suits this round");

        __instance.unlockablesList.unlockables
            .Select((unlockable, index) => new { unlockable, index })
            .Where(item => {
                Logger.LogDebug(item.unlockable.unlockableName);
                return UnlockableNamesToSpawn.Contains(item.unlockable.unlockableName);
            })
            .Do(item => __instance.SpawnUnlockable(item.index));
    }
}