/**********************************************************
    Plugin Config Information Class

    All configuration options go in here
***********************************************************/

using BepInEx;
using BepInEx.Configuration;
using Enhancer.Patches;

namespace Enhancer;

public class PluginConfig
{
    public readonly ConfigEntry<bool> Enabled;
    public readonly ConfigEntry<bool>  DelegationEnabled;
    
    public readonly ConfigEntry<bool>  KeepConsoleEnabled;
    public readonly ConfigEntry<bool>  SuitUnlocksEnabled;
    
    public readonly ConfigEntry<bool>  RandomiseCompanyBuyingFactor;
    public readonly ConfigEntry<float> MinimumCompanyBuyingFactor;

    public readonly ConfigEntry<bool>  TimeSpeedEnabled;
    public readonly ConfigEntry<float> TimeSpeed;
    
    public readonly ConfigEntry<bool>  DoorPowerDurationEnabled;
    public readonly ConfigEntry<float> DoorPowerDuration;

    public readonly ConfigEntry<bool>  StartingCreditsEnabled;
    public readonly ConfigEntry<int> StartingCredits;
    public readonly ConfigEntry<bool>  PassiveIncomeEnabled;
    public readonly ConfigEntry<int> PassiveIncomeQuantity;
    
    public readonly ConfigEntry<bool>  DaysPerQuotaEnabled;
    public readonly ConfigEntry<int> DaysPerQuota;
    
    public readonly ConfigEntry<bool>  QuotaFormulaEnabled;
    public readonly ConfigEntry<int> StartingQuota;
    public readonly ConfigEntry<float> QuotaIncreaseSteepness;
    public readonly ConfigEntry<float> QuotaBaseIncrease;
    public readonly ConfigEntry<float> QuotaIncreaseRandomFactor;

    public readonly ConfigEntry<ThreatScannerMode> ThreatScanner;

    public readonly ConfigEntry<bool>  ScrapProtectionEnabled;
    public readonly ConfigEntry<float> ScrapProtection;
    public readonly ConfigEntry<float> ScrapProtectionRandomness;

    public readonly ConfigEntry<bool>  DeathPenaltyFormulaEnabled;
    public readonly ConfigEntry<float> MaximumDeathPenalty;
    public readonly ConfigEntry<float> MaximumDeathPenaltyPerPlayer;
    public readonly ConfigEntry<float> DeadBodyRecoveryDiscount;
    public readonly ConfigEntry<float> DeathPenaltyScalingCurvature;

    public PluginConfig(BaseUnityPlugin bindingPlugin)
    {
        Enabled = bindingPlugin.Config.Bind(
            bindingPlugin.Info.Metadata.GUID, 
            "bEnabled", 
            true, 
            "Globally enable/disable the plugin"
        );
        DelegationEnabled = bindingPlugin.Config.Bind(
            bindingPlugin.Info.Metadata.GUID, 
            "bDelegationEnabled", 
            true, 
            "Globally enables/disables delegation for the plugin. When this is true, features will be disabled automatically (delegated to other mods) depending on the mods you have installed."
        );
        
        KeepConsoleEnabled = bindingPlugin.Config.Bind(
            bindingPlugin.Info.Metadata.GUID, 
            "bAlwaysShowTerminal", 
            true, 
            "Whether to keep the terminal enabled after a player stops using it\nHost Required: No"
        );
        SuitUnlocksEnabled = bindingPlugin.Config.Bind(
            bindingPlugin.Info.Metadata.GUID, 
            "bUnlockSuits", 
            false, 
            "Unlocks a few of the cheaper suits from the start so your crew has options.\nHost Required: Yes"
        );
        
        RandomiseCompanyBuyingFactor = bindingPlugin.Config.Bind(
            bindingPlugin.Info.Metadata.GUID, 
            "bCompanyBuyingFactorRandomizerEnabled", 
            false, 
            "Randomises the company buying % when enabled. Great if you're using longer quota deadlines.\nThis uses a variety of things to randomize prices such as the company mood, time passed in the quota, etc.\nRespects the minimum sale value, too.\nHost Required: Yes"
        );
        MinimumCompanyBuyingFactor = bindingPlugin.Config.Bind(
            bindingPlugin.Info.Metadata.GUID, 
            "fMinimumCompanyBuyingFactor", 
            0.0f, 
            "The default formula for selling items to the company isn't designed to handle more than 3 days remaining.\nThe Company will be prevented from offering a factor lower than this configured value.\nRecommended values for games above 3 days: 0.3 - 0.5\nHost Required: Yes"
        );
        
        TimeSpeedEnabled = bindingPlugin.Config.Bind(
            bindingPlugin.Info.Metadata.GUID,
            "bTimeSpeedEnabled",
            false,
            "Feature flag for the 'time speed' variable.\nHost Required: Yes"
        );
        TimeSpeed = bindingPlugin.Config.Bind(
            bindingPlugin.Info.Metadata.GUID, 
            "fTimeSpeed", 
            1.0f, 
            "How fast time passes on moons. Lower values mean time passes more slowly.\nRecommended value for single play: 0.75\nHost Required: Yes"
        );
        
        DoorPowerDurationEnabled = bindingPlugin.Config.Bind(
            bindingPlugin.Info.Metadata.GUID,
            "bDoorPowerDurationEnabled",
            false,
            "Feature flag for the 'door power duration' variable.\nHost Required: Yes"
        );
        DoorPowerDuration = bindingPlugin.Config.Bind(
            bindingPlugin.Info.Metadata.GUID, 
            "fDoorPowerDuration", 
            30.0f, 
            "How long the hangar door can be kept shut at a time (in seconds)\nRecommended values: 60.0 - 180.0\nHost Required: All players should use the same value."
        );

        StartingCreditsEnabled = bindingPlugin.Config.Bind(
            bindingPlugin.Info.Metadata.GUID,
            "bStartingCreditsEnabled",
            false,
            "Feature flag for the 'starting credits' variable.\nHost Required: Yes"
        );
        StartingCredits = bindingPlugin.Config.Bind(
            bindingPlugin.Info.Metadata.GUID,
            "iStartingCredits",
            60,
            "How many credits the group starts with on a new run.\nHost Required: Yes"
        );
        PassiveIncomeEnabled = bindingPlugin.Config.Bind(
            bindingPlugin.Info.Metadata.GUID,
            "bPassiveIncomeEnabled",
            false,
            "Feature flag for the 'passive income' variable.\nHost Required: Yes"
        );
        PassiveIncomeQuantity = bindingPlugin.Config.Bind(
            bindingPlugin.Info.Metadata.GUID,
            "iPassiveIncomeAmount",
            0,
            "The number of credits you will be given at the end of each level.\nHost Required: Yes"
        );
        
        DaysPerQuotaEnabled = bindingPlugin.Config.Bind(
            bindingPlugin.Info.Metadata.GUID,
            "bDaysPerQuotaEnabled",
            false,
            "Feature flag for the 'days per quota' variable.\nHost Required: Yes"
        );
        DaysPerQuota = bindingPlugin.Config.Bind(
            bindingPlugin.Info.Metadata.GUID, 
            "iQuotaDays", 
            3, 
            "How long you have to meet each quota (in days)\nRecommended values: 3 - 7\nHost Required: Yes"
        );
        
        QuotaFormulaEnabled = bindingPlugin.Config.Bind(
            bindingPlugin.Info.Metadata.GUID,
            "bQuotaFormulaEnabled",
            false,
            "Feature flag for the 'quota formula' variables, which include:\n - 'starting quota'\n - 'quota increase steepness'\n - 'quota base increase'\n - 'quota increase randomness'\nHost Required: Yes"
        );
        StartingQuota = bindingPlugin.Config.Bind(
            bindingPlugin.Info.Metadata.GUID,
            "iStartingQuota",
            130,
            "The starting quota on a new run.\nHost Required: Yes"
        );
        QuotaIncreaseSteepness = bindingPlugin.Config.Bind(
            bindingPlugin.Info.Metadata.GUID,
            "fQuotaIncreaseSteepness",
            0.0625f,
            "Used in calculating quota increase. Multiplier for the quadratic increase factor.\nHost Required: Yes"
        );
        QuotaBaseIncrease = bindingPlugin.Config.Bind(
            bindingPlugin.Info.Metadata.GUID,
            "fQuotaBaseIncrease",
            100f,
            "Used in calculating quota increase. Multiplier for the constant increase factor.\nHost Required: Yes"
        );
        QuotaIncreaseRandomFactor = bindingPlugin.Config.Bind(
            bindingPlugin.Info.Metadata.GUID,
            "fQuotaIncreaseRandomFactor",
            1f,
            "Used in calculating quota increase. Multiplier for the random increase factor.\nHost Required: Yes"
        );
        
        ThreatScanner = bindingPlugin.Config.Bind(
            bindingPlugin.Info.Metadata.GUID, 
            "eThreatScannerMode", 
            ThreatScannerMode.Disabled, 
            "How the threat scanner functions. Valid types:\n - Disabled\n - Contacts: Number of Enemies on level\n - ThreatLevelPercentage: Percentage of max enemies on level\n - ThreatLevelName: Vague Text description (In order of threat level) [Clear -> Green -> Yellow -> Orange - Red]\nHost Required: No"
        );
        
        ScrapProtectionEnabled = bindingPlugin.Config.Bind(
            bindingPlugin.Info.Metadata.GUID,
            "bScrapProtectionEnabled",
            false,
            new ConfigDescription(
                "Sets whether or not the scrap protection feature is enabled. \nHost Required: Yes"
            )
        );
        ScrapProtection = bindingPlugin.Config.Bind(
            bindingPlugin.Info.Metadata.GUID, 
            "fScrapProtection", 
            0f, 
            new ConfigDescription(
                "Sets the average probability that each scrap item is kept in the event that that no players survive a mission.\nThat is, this is the approximate average fraction of secured scrap items kept.\nHost Required: Yes",
                new AcceptableValueRange<float>(0f, 1f)
            )
        );
        ScrapProtectionRandomness = bindingPlugin.Config.Bind(
            bindingPlugin.Info.Metadata.GUID, 
            "fScrapProtectionRandomnessScalar", 
            0f, 
            new ConfigDescription(
                "Sets the randomness of the probability that each scrap item is kept in the event that that no players survive a mission.\n 0 -> no randomness, 0.5 -> \u00b10.5, 1 -> \u00b11\nHost Required: Yes",
                new AcceptableValueRange<float>(0f, 1f)
            )
        );

        DeathPenaltyFormulaEnabled = bindingPlugin.Config.Bind(
            bindingPlugin.Info.Metadata.GUID,
            "bDeathPenaltyFormulaEnabled",
            false,
            "Feature flag for the 'death penalty formula' variables, which includes\n - 'max death penalty'\n - 'max death penalty per player'\n - 'body recovery discount'\n - 'death penalty scaling curvature'\nHost Required: Yes"
        );
        MaximumDeathPenalty = bindingPlugin.Config.Bind(
            bindingPlugin.Info.Metadata.GUID, 
            "fMaximumDeathPenalty", 
            0.8f, 
            new ConfigDescription(
                "The maximum fraction of your money that you can lose per round.\nValue should be in [0,1], e.g.\n0 - No money can be lost.\n0.5 - Half your money can be lost in one run.\n1 - All money can be lost in one run.\nUse 0.8 for vanilla behaviour.\nHost Required: Yes",
                new AcceptableValueRange<float>(0f, 1f)
            )
        );
        MaximumDeathPenaltyPerPlayer = bindingPlugin.Config.Bind(
            bindingPlugin.Info.Metadata.GUID, 
            "fMaximumDeathPenaltyPerPlayer", 
            0.2f, 
            new ConfigDescription(
                "The maximum fraction of your money that you can lose per round, per dead player.\nValue should be in [0,1].\nUse 0.2 for vanilla behaviour.\nHost Required: Yes",
                new AcceptableValueRange<float>(0f, 1f)
            )
        );
        DeadBodyRecoveryDiscount = bindingPlugin.Config.Bind(
            bindingPlugin.Info.Metadata.GUID, 
            "fDeadBodyRecoveryDiscount", 
            0.6f, 
            new ConfigDescription(
                "How much recovering dead bodies reduces the penalty for that death by.\nValue should be in [0,1], e.g.\n0 - Recovering a body does not reduce the fine.\n1 - Recovering a body completely removes the fine for that death.\nUse 0.6 for vanilla behaviour.\nHost Required: Yes",
                new AcceptableValueRange<float>(0f, 1f)
            )
        );
        DeathPenaltyScalingCurvature = bindingPlugin.Config.Bind(
            bindingPlugin.Info.Metadata.GUID, 
            "fDeathPenaltyCurveDegree", 
            0f, 
            "How curved the death penalty scaling is. Positive -> less fine for fewer deaths. Negative -> more fine for fewer deaths.\ne.g. with a 4-player lobby:\n0 - The fine scales linearly: 25%, 50%, 75%, 100%.\n1 - The fine scales quadratically: 6.3%, 25%, 56.3%, 100%\n-1 - The fine scales anti-quadratically: 50%, 70.1%, 86.6%, 100%\nHost Required: Yes"
        );
    }
}