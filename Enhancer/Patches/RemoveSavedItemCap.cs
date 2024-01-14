using System.Collections.Generic;
using System.Reflection.Emit;
using BepInEx.Logging;
using Enhancer.Extensions;
using HarmonyLib;

namespace Enhancer.Patches;

public class RemoveSavedItemCap : IPatch
{
    protected static ManualLogSource Logger { get; set; } = null!;

    public void SetLogger(ManualLogSource logger)
    {
        logger.LogDebug("Logger assigned.");
        Logger = logger;
    }

    [HarmonyPatch(typeof(GameNetworkManager), nameof(GameNetworkManager.SaveItemsInShip))]
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> RemoveSavedItemCapTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions);

        matcher
            .Start()
            .MatchForward(
                false,
                new CodeMatch(OpCodes.Ldloc_S, 6),
                new CodeMatch(OpCodes.Call, AccessTools.PropertyGetter(typeof(StartOfRound), nameof(StartOfRound.Instance))),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(StartOfRound), nameof(StartOfRound.maxShipItemCapacity))),
                new CodeMatch(instr => instr.opcode == OpCodes.Bgt)
            )
            .SetAndAdvance(OpCodes.Nop, null)
            .RemoveInstructions(3);

        Logger.LogDebugInstructionsFrom(matcher);

        return matcher.InstructionEnumeration();
    }
}
