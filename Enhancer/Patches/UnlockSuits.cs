using System;
using System.Reflection;
using HarmonyLib;

namespace Enhancer.Patches;

public class UnlockSuits : IPatch
{
    private static readonly MethodInfo UnlockItem = typeof(StartOfRound).GetMethod("SpawnUnlockable", BindingFlags.NonPublic | BindingFlags.Instance) ?? throw new InvalidOperationException("Couldn't find method: StartOfRound.SpawnUnlockable");

    public static void SpawnUnlockableDelegate(StartOfRound instance, int ID)
    {
        UnlockItem.Invoke(instance, new object[] { ID });
    }

    [HarmonyPatch(typeof(StartOfRound), "Start")]
    [HarmonyPostfix]
    public static void StartOfRoundSuitPatch(StartOfRound __instance)
    {
        Plugin.Log.LogInfo("Setting unlocked suits this round");

        //Green Suit
        SpawnUnlockableDelegate(__instance, 1);
        //Hazard Suit
        SpawnUnlockableDelegate(__instance, 2);
    }
}