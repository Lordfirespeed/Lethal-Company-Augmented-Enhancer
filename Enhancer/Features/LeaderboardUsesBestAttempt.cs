using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using BepInEx.Logging;
using HarmonyLib;
using Steamworks.Data;

namespace Enhancer.Features;

public class LeaderboardUsesBestAttempt : IFeature
{
    protected static ManualLogSource Logger { get; set; } = null!;
    protected static Harmony Harmony { get; set; } = null!;

    public void SetLogger(ManualLogSource logger)
    {
        logger.LogDebug("Logger assigned.");
        Logger = logger;
    }

    public void SetHarmony(Harmony harmony)
    {
        Logger.LogDebug("Harmony assigned.");
        Harmony = harmony;
    }

    public void OnEnable()
    {
        // 'Async Method Reflection Snippet' Copyright (c) 2024 Ryan Gregory https://github.com/Xilophor
        var target = AccessTools
            .Method(typeof(MenuManager), nameof(MenuManager.GetLeaderboardForChallenge))
            .GetCustomAttribute<AsyncStateMachineAttribute>()
            .StateMachineType
            .GetMethod("MoveNext", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
        // End Copyright

        var transpilerHarmonyMethod = new HarmonyMethod(AccessTools.Method(typeof(LeaderboardUsesBestAttempt), nameof(DoNotOverwriteBetterChallengeAttempts)));
        Harmony.Patch(target, transpiler: transpilerHarmonyMethod);
    }

    public static IEnumerable<CodeInstruction> DoNotOverwriteBetterChallengeAttempts(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
    {
        var matcher = new CodeMatcher(instructions, generator);

        matcher
            .Start()
            .MatchForward(
                false,
                new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Leaderboard), nameof(Leaderboard.ReplaceScore)))
            )
            .SetAndAdvance(OpCodes.Call, AccessTools.Method(typeof(Leaderboard), nameof(Leaderboard.SubmitScoreAsync)));

        return matcher.InstructionEnumeration();
    }
}
