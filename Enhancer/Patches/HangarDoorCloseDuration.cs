using HarmonyLib;

namespace Enhancer.Patches;

public class HangarDoorCloseDuration
{
    [HarmonyPatch(typeof(HangarShipDoor), "Start")]
    [HarmonyPostfix]
    public static void HangarShipDoorPost(HangarShipDoor __instance)
    {
        Plugin.Log.LogInfo($"Setting Hangar door power to last for {Plugin.BoundConfig.DoorPowerDuration} seconds...");
        __instance.doorPowerDuration = Plugin.BoundConfig.DoorPowerDuration;
    }
}