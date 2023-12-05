using HarmonyLib;

namespace Enhancer.Patches;

public static class AlwaysShowTerminal
{
    [HarmonyPatch(typeof(Terminal), "Update")]
    [HarmonyPostfix]
    public static void TerminalUpdatePost(Terminal __instance)
    {
        if (__instance.terminalUIScreen.gameObject.activeSelf) return;
        __instance.terminalUIScreen.gameObject.SetActive(true);
    }
}