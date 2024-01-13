using System;
using BepInEx.Logging;
using TerminalApiHelper = TerminalApi.TerminalApi;
using TerminalApi.Classes;

namespace Enhancer.Patches;

public enum ThreatScannerMode
{
    Disabled,
    Contacts,
    ThreatLevelPercentage,
    ThreatLevelName
}

public class ThreatScanCommand : IPatch
{
    protected static ManualLogSource Logger { get; set; } = null!;

    private TerminalNode? _triggerNode;
    private TerminalKeyword? _nounKeyword;

    public void SetLogger(ManualLogSource logger)
    {
        logger.LogDebug("Logger assigned.");
        Logger = logger;
    }
    private static string GetThreatLevel(float threatCoefficient)
    {
        return threatCoefficient switch {
            > 0.99f => "OMEGA",
            > 0.69f => "RED",
            > 0.49f => "ORANGE",
            > 0.24f => "YELLOW",
            > 0 => "GREEN",
            _ => "CLEAR"
        };
    }

    private static string GetThreatDescription(int enemyPower, int enemyMaxPower, int enemyCount)
    {
        if (Plugin.BoundConfig.ThreatScanner.Value is ThreatScannerMode.Disabled) return null;

        if (Plugin.BoundConfig.ThreatScanner.Value is ThreatScannerMode.Contacts)
            return $"\nHostile Contacts: {enemyCount}\n";

        var threatCoefficient = (float)enemyPower / enemyMaxPower;

        if (Plugin.BoundConfig.ThreatScanner.Value is ThreatScannerMode.ThreatLevelPercentage)
            return $"\nThreat Level: {threatCoefficient:p1}\n";

        if (Plugin.BoundConfig.ThreatScanner.Value is ThreatScannerMode.ThreatLevelName)
            return $"\nThreat Level: {GetThreatLevel(threatCoefficient)}\n";

        throw new ArgumentOutOfRangeException($"Invalid threat scanner type '{enemyPower}'.");
    }

    public void OnPatch()
    {
        _triggerNode = TerminalApiHelper.CreateTerminalNode("Safe zone detected\n", true);
        _nounKeyword = TerminalApiHelper.CreateTerminalKeyword("threats");
        _nounKeyword.specialKeywordResult = _triggerNode;

        TerminalApiHelper.AddTerminalKeyword(_nounKeyword, new CommandInfo
        {
            TriggerNode = _triggerNode,
            DisplayTextSupplier = ThreatScanTextSupplier,
            Category = "Other",
            Description = "Scan for threats on the current moon."
        });
    }

    public void OnUnpatch()
    {
        if (_nounKeyword is not null)
            TerminalApiHelper.DeleteKeyword(_nounKeyword.word);
    }

    public static string ThreatScanTextSupplier()
    {
        //If there are no enemies in the level, do nothing
        if (!RoundManager.Instance.currentLevel.spawnEnemiesAndScrap) return "Safe zone detected\n";

        var power = RoundManager.Instance.currentEnemyPower;
        var maxp = RoundManager.Instance.currentLevel.maxEnemyPowerCount;
        var count = RoundManager.Instance.numberOfEnemiesInScene;

        return GetThreatDescription(power, maxp, count);
    }
}
