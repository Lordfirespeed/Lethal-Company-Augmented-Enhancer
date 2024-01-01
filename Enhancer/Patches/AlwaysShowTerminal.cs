using HarmonyLib;

namespace Enhancer.Patches;

public class AlwaysShowTerminal : IPatch
{
    [HarmonyPatch(typeof(Terminal), nameof(Terminal.waitUntilFrameEndToSetActive))]
    [HarmonyPostfix]
    public static void TerminalUpdatePost(Terminal __instance)
    {
        if (__instance.terminalUIScreen.gameObject.activeSelf) return;
        __instance.terminalUIScreen.gameObject.SetActive(true);
    }
}