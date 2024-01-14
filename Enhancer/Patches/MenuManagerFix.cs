using System.Collections.Generic;
using System.Reflection.Emit;
using BepInEx.Logging;
using HarmonyLib;

namespace Enhancer.Patches;

public class MenuManagerFix: IPatch
{
    protected static ManualLogSource Logger { get; set; } = null!;

    public void SetLogger(ManualLogSource logger)
    {
        logger.LogDebug("Logger assigned.");
        Logger = logger;
    }

    [HarmonyPatch(typeof(MenuManager), nameof(MenuManager.Update))]
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> NoGameNetworkManagerNullDereference(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var matcher = new CodeMatcher(instructions, generator);

        matcher
            .Start()
            .MatchForward(
                true,
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(MenuManager), nameof(MenuManager.MenuAudio))),
                new CodeMatch(OpCodes.Call, AccessTools.PropertyGetter(typeof(GameNetworkManager), nameof(GameNetworkManager.Instance))),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(GameNetworkManager), nameof(GameNetworkManager.buttonSelectSFX))),
                new CodeMatch(instr => instr.opcode == OpCodes.Callvirt)
            )
            .Advance(1)
            .CreateLabel(out var skipAudioLabel)
            .Advance(-5)
            .InsertAndAdvance(
                new CodeInstruction(OpCodes.Call, AccessTools.PropertyGetter(typeof(GameNetworkManager), nameof(GameNetworkManager.Instance))),
                new CodeInstruction(OpCodes.Ldnull),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(UnityEngine.Object), "op_Equality", [typeof(UnityEngine.Object), typeof(UnityEngine.Object)])),
                new CodeInstruction(OpCodes.Brtrue_S, skipAudioLabel)
            );

        return matcher.InstructionEnumeration();
    }
}
