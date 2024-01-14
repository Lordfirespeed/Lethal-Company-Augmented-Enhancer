using System;
using System.Collections.Generic;
using BepInEx.Logging;
using HarmonyLib;
using TMPro;
using UnityEngine;
using OpCodes = System.Reflection.Emit.OpCodes;

namespace Enhancer.Features;

public class MilitaryTime : IFeature
{
    protected static ManualLogSource Logger { get; set; } = null!;

    public void SetLogger(ManualLogSource logger)
    {
        logger.LogDebug("Logger assigned.");
        Logger = logger;
    }

    [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.SetClock))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> Transpile24HourClock(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions);

        matcher
            .Start()
            .MatchForward(
                false,
                new CodeMatch(OpCodes.Ldarg_3),
                new CodeMatch(OpCodes.Brtrue)
            );

        var removeFromIndex = matcher.Pos;

        matcher
            .MatchForward(
                true,
                new CodeMatch(OpCodes.Ldloc_1),
                new CodeMatch(instr => instr.opcode == OpCodes.Ldc_I4_S && instr.OperandIs(12)),
                new CodeMatch(OpCodes.Rem),
                new CodeMatch(OpCodes.Stloc_1)
            )
            .RemoveInstructionsInRange(removeFromIndex, matcher.Pos);

        matcher
            .Start()
            .MatchForward(
                false,
                new CodeMatch(instr => instr.opcode == OpCodes.Ldc_I4_S && instr.OperandIs(48)),
                new CodeMatch(instr => instr.opcode == OpCodes.Callvirt),
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(HUDManager), nameof(HUDManager.amPM))),
                new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(string), nameof(string.Concat), [typeof(string), typeof(string)]))
            )
            .RemoveInstructions(5);

        return matcher.InstructionEnumeration();
    }
}
