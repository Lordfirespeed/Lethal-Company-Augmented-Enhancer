using HarmonyLib;

namespace Enhancer.Patches;

public class HangarDoorCloseDuration : IPatch
{
    [HarmonyPatch(typeof(HangarShipDoor), "Start")]
    [HarmonyPostfix]
    public static void HangarShipDoorPost(HangarShipDoor __instance)
    {
        Plugin.Logger.LogInfo($"Setting Hangar door power to last for {Plugin.BoundConfig.DoorPowerDuration.Value} seconds...");
        __instance.doorPowerDuration = Plugin.BoundConfig.DoorPowerDuration.Value;
    }
}