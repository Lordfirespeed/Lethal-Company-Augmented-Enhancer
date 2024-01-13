using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using CodeMatch = HarmonyLib.CodeMatch;

namespace Enhancer.Patches;

public class AlwaysShowTerminal : IPatch
{
    private static readonly MethodInfo WaitUntilFrameEndToSetActiveMethod =
        AccessTools.Method(typeof(Terminal), nameof(Terminal.waitUntilFrameEndToSetActive));

    private static readonly MethodInfo StartCoroutineMethod = AccessTools.Method(typeof(UnityEngine.MonoBehaviour),
        nameof(UnityEngine.MonoBehaviour.StartCoroutine), [typeof(IEnumerator)]);

    private static readonly FieldInfo TerminalScrollBarVertical =
        AccessTools.Field(typeof(Terminal), nameof(Terminal.scrollBarVertical));

    private static readonly PropertyInfo ScrollbarValue =
        AccessTools.Property(typeof(Scrollbar), nameof(Scrollbar.value));

    [HarmonyPatch(typeof(Terminal), nameof(Terminal.QuitTerminal))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> TerminalQuitTranspile(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions);

        matcher
            .Start()
            .MatchForward(false,
                new CodeMatch(OpCodes.Call, WaitUntilFrameEndToSetActiveMethod),
                new CodeMatch(OpCodes.Call, StartCoroutineMethod)
            )
            .MatchBack(false,
                new CodeMatch(OpCodes.Ldc_I4_0)
            )
            .SetOpcodeAndAdvance(OpCodes.Ldc_I4_1)
            .MatchForward(false,
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, TerminalScrollBarVertical),
                new CodeMatch(OpCodes.Ldc_R4, 0f),
                new CodeMatch(OpCodes.Callvirt, ScrollbarValue.GetSetMethod())
            )
            .SetOpcodeAndAdvance(OpCodes.Nop)
            .RemoveInstructions(3);

        return matcher.InstructionEnumeration();
    }

    [HarmonyPatch(typeof(Terminal), nameof(Terminal.BeginUsingTerminal))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> TerminalOpenTranspile(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions);

        matcher
            .Start()
            .MatchForward(
                false,
                new CodeMatch(instr => instr.opcode == OpCodes.Ldc_I4_S && instr.OperandIs(13))
            )
            .Advance(-4)
            .SetAndAdvance(OpCodes.Nop, null)
            .RemoveInstructions(6);

        return matcher.InstructionEnumeration();
    }
}
