using System.Collections.Generic;
using System.Reflection.Emit;
using BepInEx.Logging;
using Enhancer.Extensions;
using HarmonyLib;

namespace Enhancer.Features;

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
                new CodeMatch(OpCodes.Ldloc_S),
                new CodeMatch(OpCodes.Call, AccessTools.PropertyGetter(typeof(StartOfRound), nameof(StartOfRound.Instance))),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(StartOfRound), nameof(StartOfRound.maxShipItemCapacity))),
                new CodeMatch(OpCodes.Bgt)
            )
            .AddLabelsAt(matcher.Pos + 4, matcher.Labels)
            .RemoveInstructions(4);

        return matcher.InstructionEnumeration();
    }
}
