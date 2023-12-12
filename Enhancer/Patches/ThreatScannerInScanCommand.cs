using BepInEx.Logging;
using Dissonance;
using HarmonyLib;

namespace Enhancer.Patches;

public enum ThreatScannerMode
{
    Disabled,
    Contacts,
    ThreatLevelPercentage,
    ThreatLevelName,
}

public class ThreatScannerInScanCommand : IPatch
{
    private static string GetThreatLevel(float threatCoefficient)
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
    
    private static string? GetThreatDescription(int enemyPower, int enemyMaxPower, int enemyCount)
    {
        if (Plugin.BoundConfig.ThreatScanner.Value is ThreatScannerMode.Disabled) return null;
        
        if (Plugin.BoundConfig.ThreatScanner.Value is ThreatScannerMode.Contacts) {
            return $"\nHostile Contacts: {enemyCount}\n";
        }
        
        float threatCoefficient = (float)enemyPower / enemyMaxPower;

        if (Plugin.BoundConfig.ThreatScanner.Value is ThreatScannerMode.ThreatLevelPercentage) {
            return $"\nThreat Level: {threatCoefficient:p1}\n";
        }

        if (Plugin.BoundConfig.ThreatScanner.Value is ThreatScannerMode.ThreatLevelName)
        {
            return $"\nThreat Level: {GetThreatLevel(threatCoefficient)}\n";
        }
        
        Plugin.Log.LogWarning("Invalid threat scanner type is configured.");
        return null;
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
        if (Plugin.BoundConfig.ThreatScanner.Value is ThreatScannerMode.Disabled) return;

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

        __instance.screenText.text += GetThreatDescription(power, maxp, count);
        __instance.currentText = __instance.screenText.text;
        __instance.textAdded = 0;
    }
}