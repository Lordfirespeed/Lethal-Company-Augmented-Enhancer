using System;
using HarmonyLib;
using UnityEngine;

namespace Enhancer.Patches;

public static class ImprovedScanCommand
{
    private static string getThreatLevel(float threatCoefficient)
    {
        return threatCoefficient switch
        {
            > 0.99f => "OMEGA",
            > 0.69f => "RED",
            > 0.49f => "ORANGE",
            > 0.24f => "YELLOW",
            > 0 => "GREEN",
            _ => "CLEAR",
        };
    }
    
    private static string getThreatDescription(int enemyPower, int enemyMaxPower, int enemyCount)
    {
        if (Plugin.BoundConfig.ThreatScannerType == 0) return null;
        
        if (Plugin.BoundConfig.ThreatScannerType == 1) {
            return $"\nHostile Contacts: {enemyCount}\n";
        }
        
        float threatCoefficient = (float)enemyPower / enemyMaxPower;

        if (Plugin.BoundConfig.ThreatScannerType == 2) {
            return $"\nThreat Level: {threatCoefficient:p1}\n";
        }

        if (Plugin.BoundConfig.ThreatScannerType == 3)
        {
            return $"\nThreat Level: {getThreatLevel(threatCoefficient)}\n";
        }
        
        Plugin.Log.LogWarning("Invalid threat scanner type is configured.");
        return "";
    }
    
    //Todo: This should probably be changed to a postfix on the text modifier
    //function so I can add custom tags to terminal nodes
    [HarmonyPatch(typeof(Terminal), nameof(Terminal.LoadNewNode))]
    [HarmonyPostfix]
    public static void TerminalLoadHackPost(Terminal __instance, TerminalNode node)
    {
        //If the command is not 'scan', do nothing
        if (node.name != "ScanInfo") return;
        
        //If scan command improvements are disabled, do nothing
        if (Plugin.Cfg.ThreatScannerType == 0) return;

        //If there are no enemies in the level, do nothing
        if (!RoundManager.Instance.currentLevel.spawnEnemiesAndScrap) return;
        
        //Inject data into the command
    
        /*
            We cache these values (and the ones in the switch below) because
            The actual in-game terminal crashes when accessing RoundManager
            sometimes and I don't know why but this configuration works

            Recommendation: Do not modify this function ever, it is a headache
        */
        int power = RoundManager.Instance.currentEnemyPower;
        int maxp = RoundManager.Instance.currentLevel.maxEnemyPowerCount;
        int count = RoundManager.Instance.numberOfEnemiesInScene;

        __instance.screenText.text += getThreatDescription(power, maxp, count);
        __instance.currentText = __instance.screenText.text;
        __instance.textAdded = 0;
    }
}