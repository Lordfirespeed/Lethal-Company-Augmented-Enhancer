using BepInEx.Logging;
using HarmonyLib;

namespace Enhancer.Patches;

public class HangarDoorCloseDuration : IPatch
{
    protected static ManualLogSource Logger { get; set; } = null!;

    public void SetLogger(ManualLogSource logger)
    {
        logger.LogDebug("Logger assigned.");
        Logger = logger;
    }

    [HarmonyPatch(typeof(HangarShipDoor), "Start")]
    [HarmonyPostfix]
    public static void HangarShipDoorPost(HangarShipDoor __instance)
    {
        Logger.LogInfo($"Setting Hangar door power to last for {Plugin.BoundConfig.DoorPowerDuration.Value} seconds...");
        __instance.doorPowerDuration = Plugin.BoundConfig.DoorPowerDuration.Value;
    }
}
