/**********************************************************
    Plugin Config Information Class

    All configuration options go in here
***********************************************************/

using BepInEx.Configuration;
using Enhancer.Patches;

namespace Enhancer;

public class PluginConfig
{
    public readonly bool Enabled;
    public readonly bool DelegationEnabled;
    
    public readonly bool KeepConsoleEnabled;
    public readonly bool SuitUnlocksEnabled;
    
    public readonly bool RandomiseCompanyBuyingFactor;
    public readonly float MinimumCompanyBuyingFactor;

    public readonly bool TimeSpeedEnabled;
    public readonly float TimeSpeed;
    
    public readonly bool DoorPowerDurationEnabled;
    public readonly float DoorPowerDuration;

    public readonly bool StartingCreditsEnabled;
    public readonly int StartingCredits;
    
    public readonly bool DaysPerQuotaEnabled;
    public readonly int DaysPerQuota;
    
    public readonly bool QuotaFormulaEnabled;
    public readonly int StartingQuota;
    public readonly float QuotaIncreaseSteepness;
    public readonly float QuotaBaseIncrease;
    public readonly float QuotaIncreaseRandomFactor;

    public readonly ThreatScannerMode ThreatScanner;

    public readonly bool ScrapProtectionEnabled;
    public readonly float ScrapProtection;
    public readonly float ScrapProtectionRandomness;

    public readonly bool DeathPenaltyFormulaEnabled;
    public readonly float MaximumDeathPenalty;
    public readonly float MaximumDeathPenaltyPerPlayer;
    public readonly float DeadBodyRecoveryDiscount;
    public readonly float DeathPenaltyScalingCurvature;

    public PluginConfig(Plugin bindingPlugin)
    {
        Enabled = bindingPlugin.Config.Bind(
            PluginInfo.PLUGIN_GUID, 
            "bEnabled", 
            true, 
            "Globally enable/disable the plugin"
        ).Value;
        DelegationEnabled = bindingPlugin.Config.Bind(
            PluginInfo.PLUGIN_GUID, 
            "bDelegationEnabled", 
            true, 
            "Globally enables/disables delegation for the plugin. When this is true, features will be disabled automatically (delegated to other mods) depending on the mods you have installed."
        ).Value;
        
        KeepConsoleEnabled = bindingPlugin.Config.Bind(
            PluginInfo.PLUGIN_GUID, 
            "bAlwaysShowTerminal", 
            true, 
            "Whether to keep the terminal enabled after a player stops using it\nHost Required: No"
        ).Value;
        SuitUnlocksEnabled = bindingPlugin.Config.Bind(
            PluginInfo.PLUGIN_GUID, 
            "bUnlockSuits", 
            false, 
            "Unlocks a few of the cheaper suits from the start so your crew has options.\nHost Required: Yes"
        ).Value;
        
        RandomiseCompanyBuyingFactor = bindingPlugin.Config.Bind(
            PluginInfo.PLUGIN_GUID, 
            "bCompanyBuyingFactorRandomizerEnabled", 
            false, 
            "Randomises the company buying % when enabled. Great if you're using longer quota deadlines.\nThis uses a variety of things to randomize prices such as the company mood, time passed in the quota, etc.\nRespects the minimum sale value, too.\nHost Required: Yes"
        ).Value;
        MinimumCompanyBuyingFactor = bindingPlugin.Config.Bind(
            PluginInfo.PLUGIN_GUID, 
            "fMinimumCompanyBuyingFactor", 
            0.0f, 
            "The default formula for selling items to the company isn't designed to handle more than 3 days remaining.\nThe Company will be prevented from offering a factor lower than this configured value.\nRecommended values for games above 3 days: 0.3 - 0.5\nHost Required: Yes"
        ).Value;
        
        TimeSpeedEnabled = bindingPlugin.Config.Bind(
            PluginInfo.PLUGIN_GUID,
            "bTimeSpeedEnabled",
            false,
            "Feature flag for the 'time speed' variable.\nHost Required: Yes"
        ).Value;
        TimeSpeed = bindingPlugin.Config.Bind(
            PluginInfo.PLUGIN_GUID, 
            "fTimeSpeed", 
            1.0f, 
            "How fast time passes on moons. Lower values mean time passes more slowly.\nRecommended value for single play: 0.75\nHost Required: Yes"
        ).Value;
        
        DoorPowerDurationEnabled = bindingPlugin.Config.Bind(
            PluginInfo.PLUGIN_GUID,
            "bDoorPowerDurationEnabled",
            false,
            "Feature flag for the 'door power duration' variable.\nHost Required: Yes"
        ).Value;
        DoorPowerDuration = bindingPlugin.Config.Bind(
            PluginInfo.PLUGIN_GUID, 
            "fDoorPowerDuration", 
            30.0f, 
            "How long the hangar door can be kept shut at a time (in seconds)\nRecommended values: 60.0 - 180.0\nHost Required: All players should use the same value."
        ).Value;

        StartingCreditsEnabled = bindingPlugin.Config.Bind(
            PluginInfo.PLUGIN_GUID,
            "bStartingCreditsEnabled",
            false,
            "Feature flag for the 'starting credits' variable.\nHost Required: Yes"
        ).Value;
        StartingCredits = bindingPlugin.Config.Bind(
            PluginInfo.PLUGIN_GUID,
            "iStartingCredits",
            60,
            "How many credits the group starts with on a new run.\nHost Required: Yes"
        ).Value;
        
        DaysPerQuotaEnabled = bindingPlugin.Config.Bind(
            PluginInfo.PLUGIN_GUID,
            "bDaysPerQuotaEnabled",
            false,
            "Feature flag for the 'days per quota' variable.\nHost Required: Yes"
        ).Value;
        DaysPerQuota = bindingPlugin.Config.Bind(
            PluginInfo.PLUGIN_GUID, 
            "iQuotaDays", 
            3, 
            "How long you have to meet each quota (in days)\nRecommended values: 3 - 7\nHost Required: Yes"
        ).Value;
        
        QuotaFormulaEnabled = bindingPlugin.Config.Bind(
            PluginInfo.PLUGIN_GUID,
            "bQuotaFormulaEnabled",
            false,
            "Feature flag for the 'quota formula' variables, which include:\n - 'starting quota'\n - 'quota increase steepness'\n - 'quota base increase'\n - 'quota increase randomness'\nHost Required: Yes"
        ).Value;
        StartingQuota = bindingPlugin.Config.Bind(
            PluginInfo.PLUGIN_GUID,
            "iStartingQuota",
            130,
            "The starting quota on a new run.\nHost Required: Yes"
        ).Value;
        QuotaIncreaseSteepness = bindingPlugin.Config.Bind(
            PluginInfo.PLUGIN_GUID,
            "fQuotaIncreaseSteepness",
            0.0625f,
            "Used in calculating quota increase. Multiplier for the quadratic increase factor.\nHost Required: Yes"
        ).Value;
        QuotaBaseIncrease = bindingPlugin.Config.Bind(
            PluginInfo.PLUGIN_GUID,
            "fQuotaBaseIncrease",
            100f,
            "Used in calculating quota increase. Multiplier for the constant increase factor.\nHost Required: Yes"
        ).Value;
        QuotaIncreaseRandomFactor = bindingPlugin.Config.Bind(
            PluginInfo.PLUGIN_GUID,
            "fQuotaIncreaseRandomFactor",
            1f,
            "Used in calculating quota increase. Multiplier for the random increase factor.\nHost Required: Yes"
        ).Value;
        
        ThreatScanner = bindingPlugin.Config.Bind(
            PluginInfo.PLUGIN_GUID, 
            "eThreatScannerMode", 
            ThreatScannerMode.Disabled, 
            "How the threat scanner functions. Valid types:\n - Disabled\n - Contacts: Number of Enemies on level\n - ThreatLevelPercentage: Percentage of max enemies on level\n - ThreatLevelName: Vague Text description (In order of threat level) [Clear -> Green -> Yellow -> Orange - Red]\nHost Required: No"
        ).Value;
        
        ScrapProtectionEnabled = bindingPlugin.Config.Bind(
            PluginInfo.PLUGIN_GUID,
            "bScrapProtectionEnabled",
            false,
            new ConfigDescription(
                "Sets whether or not the scrap protection feature is enabled. \nHost Required: Yes"
            )
        ).Value;
        ScrapProtection = bindingPlugin.Config.Bind(
            PluginInfo.PLUGIN_GUID, 
            "fScrapProtection", 
            0f, 
            new ConfigDescription(
                "Sets the average probability that each scrap item is kept in the event that that no players survive a mission.\nThat is, this is the approximate average fraction of secured scrap items kept.\nHost Required: Yes",
                new AcceptableValueRange<float>(0f, 1f)
            )
        ).Value;
        ScrapProtectionRandomness = bindingPlugin.Config.Bind(
            PluginInfo.PLUGIN_GUID, 
            "fScrapProtectionRandomnessScalar", 
            0f, 
            new ConfigDescription(
                "Sets the randomness of the probability that each scrap item is kept in the event that that no players survive a mission.\n 0 -> no randomness, 0.5 -> \u00b10.5, 1 -> \u00b11\nHost Required: Yes",
                new AcceptableValueRange<float>(0f, 1f)
            )
        ).Value;

        DeathPenaltyFormulaEnabled = bindingPlugin.Config.Bind(
            PluginInfo.PLUGIN_GUID,
            "bDeathPenaltyFormulaEnabled",
            false,
            "Feature flag for the 'death penalty formula' variables, which includes\n - 'max death penalty'\n - 'max death penalty per player'\n - 'body recovery discount'\n - 'death penalty scaling curvature'\nHost Required: Yes"
        ).Value;
        MaximumDeathPenalty = bindingPlugin.Config.Bind(
            PluginInfo.PLUGIN_GUID, 
            "fMaximumDeathPenalty", 
            0.8f, 
            new ConfigDescription(
                "The maximum fraction of your money that you can lose per round.\nValue should be in [0,1], e.g.\n0 - No money can be lost.\n0.5 - Half your money can be lost in one run.\n1 - All money can be lost in one run.\nUse 0.8 for vanilla behaviour.\nHost Required: Yes",
                new AcceptableValueRange<float>(0f, 1f)
            )
        ).Value;
        MaximumDeathPenaltyPerPlayer = bindingPlugin.Config.Bind(
            PluginInfo.PLUGIN_GUID, 
            "fMaximumDeathPenaltyPerPlayer", 
            0.2f, 
            new ConfigDescription(
                "The maximum fraction of your money that you can lose per round, per dead player.\nValue should be in [0,1].\nUse 0.2 for vanilla behaviour.\nHost Required: Yes",
                new AcceptableValueRange<float>(0f, 1f)
            )
        ).Value;
        DeadBodyRecoveryDiscount = bindingPlugin.Config.Bind(
            PluginInfo.PLUGIN_GUID, 
            "fDeadBodyRecoveryDiscount", 
            0.6f, 
            new ConfigDescription(
                "How much recovering dead bodies reduces the penalty for that death by.\nValue should be in [0,1], e.g.\n0 - Recovering a body does not reduce the fine.\n1 - Recovering a body completely removes the fine for that death.\nUse 0.6 for vanilla behaviour.\nHost Required: Yes",
                new AcceptableValueRange<float>(0f, 1f)
            )
        ).Value;
        DeathPenaltyScalingCurvature = bindingPlugin.Config.Bind(
            PluginInfo.PLUGIN_GUID, 
            "fDeathPenaltyCurveDegree", 
            0f, 
            "How curved the death penalty scaling is. Positive -> less fine for fewer deaths. Negative -> more fine for fewer deaths.\ne.g. with a 4-player lobby:\n0 - The fine scales linearly: 25%, 50%, 75%, 100%.\n1 - The fine scales quadratically: 6.3%, 25%, 56.3%, 100%\n-1 - The fine scales anti-quadratically: 50%, 70.1%, 86.6%, 100%\nHost Required: Yes"
        ).Value;
    }
}