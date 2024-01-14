using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;

namespace Enhancer.Features;

public class HideClock : IFeature
{
    protected static ManualLogSource Logger { get; set; } = null!;

    public void SetLogger(ManualLogSource logger)
    {
        logger.LogDebug("Logger assigned.");
        Logger = logger;
    }

    private static PlayerControllerB? Player => HUDManager.Instance != null ? HUDManager.Instance.localPlayer : null;
    private static bool PlayerIsInFactory => Player is { isInsideFactory: true };
    private static bool PlayerIsInShip => Player is { isInHangarShipRoom: true };
    private static bool PlayerIsInTerminal => Player is { inTerminalMenu: true };
    private static HUDElement? Clock => HUDManager.Instance != null ? HUDManager.Instance.Clock : null;
    private static bool CurrentClockVisibility => Clock is { targetAlpha: > 0 };

    private static Environment CurrentPlayerLocationEnvironment {
        get {
            if (PlayerIsInShip) return Environment.Ship;
            if (PlayerIsInFactory) return Environment.Facility;
            return Environment.Outside;
        }
    }

    private static Environment CurrentPlayerEnvironment {
        get {
            var environments = CurrentPlayerLocationEnvironment;

            if (PlayerIsInTerminal)
                environments |= Environment.Terminal;

            return environments;
        }
    }

    private static Environment ClockVisibleEnvironments {
        get {
            var environments = Environment.None;

            if (!Plugin.BoundConfig.HideClockOutside.Value)
                environments |= Environment.Outside;

            if (!Plugin.BoundConfig.HideClockOnShip.Value)
                environments |= Environment.Ship;

            if (!Plugin.BoundConfig.HideClockInFacility.Value)
                environments |= Environment.Facility;

            return environments;
        }
    }

    private static Environment ClockInvisibleEnvironments => Environment.Terminal;

    private static bool DesiredClockVisibility {
        get {
            if (Convert.ToBoolean(CurrentPlayerEnvironment & ClockInvisibleEnvironments)) return false;
            return Convert.ToBoolean(CurrentPlayerEnvironment & ClockVisibleEnvironments);
        }
    }

    public static void SetDesiredClockVisibility()
    {
        if (HUDManager.Instance is null) return;

        var newClockVisibility = DesiredClockVisibility;
        if (CurrentClockVisibility == newClockVisibility) return;

        Logger.LogDebug($"{(newClockVisibility ? "Showing" : "Hiding")} clock");
        HUDManager.Instance.SetClockVisible(newClockVisibility);
    }

    [HarmonyPatch(typeof(TimeOfDay), nameof(TimeOfDay.SetInsideLightingDimness))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> RedirectSetClockVisible(IEnumerable<CodeInstruction> instructions)
    {
        var matcher = new CodeMatcher(instructions);

        matcher
            .Start()
            .MatchForward(
                false,
                new CodeMatch(OpCodes.Call, AccessTools.PropertyGetter(typeof(HUDManager), nameof(HUDManager.Instance))),
                new CodeMatch(OpCodes.Ldarg_0),
                new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(TimeOfDay), nameof(TimeOfDay.insideLighting))),
                new CodeMatch(OpCodes.Ldc_I4_0),
                new CodeMatch(OpCodes.Ceq),
                new CodeMatch(instr => instr.opcode == OpCodes.Callvirt)
            )
            .SetAndAdvance(OpCodes.Nop, null)
            .RemoveInstructions(5)
            .Insert(
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(HideClock), nameof(SetDesiredClockVisibility)))
            );

        return matcher.InstructionEnumeration();
    }

    [Flags]
    enum Environment
    {
        None = 0,
        Outside = 1 << 0,
        Ship = 1 << 1,
        Facility = 1 << 2,
        Terminal = 1 << 3,
    }
}
