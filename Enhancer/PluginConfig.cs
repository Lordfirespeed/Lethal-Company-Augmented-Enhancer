/**********************************************************
    Plugin Config Information Class

    All configuration options go in here
***********************************************************/

using System;
using BepInEx;
using BepInEx.Configuration;
using Enhancer.Patches;
using Enhancer.Config;
using Enhancer.Config.AcceptableValues;

namespace Enhancer;

public class PluginConfig
{
    public readonly ConfigEntry<bool> Enabled;
    public readonly ConfigEntry<bool> DelegationEnabled;

    public readonly ConfigEntry<bool> KeepConsoleEnabled;
    public readonly ConfigEntry<bool> SuitUnlocksEnabled;

    public readonly ConfigEntry<bool> CompanyBuyingFactorTweaksEnabled;
    public readonly ConfigEntry<bool> RandomiseCompanyBuyingFactor;
    public readonly ConfigEntry<float> MinimumCompanyBuyingFactor;

    public readonly ConfigEntry<bool> TimeSpeedEnabled;
    public readonly ConfigEntry<float> TimeSpeed;

    public readonly ConfigEntry<bool> DoorPowerDurationEnabled;
    public readonly ConfigEntry<float> DoorPowerDuration;

    public readonly ConfigEntry<bool> StartingCreditsEnabled;
    public readonly ConfigEntry<int> StartingCredits;
    public readonly ConfigEntry<bool> PassiveIncomeEnabled;
    public readonly ConfigEntry<int> PassiveIncomeQuantity;

    public readonly ConfigEntry<bool> DaysPerQuotaAssignmentEnabled;
    public readonly ConfigEntry<QuotaDurationBehaviour> DaysPerQuotaAssignmentBehaviour;
    public readonly ConfigEntry<int> DaysPerQuotaAssignment;
    public readonly ConfigEntry<Interval<int>> DaysPerQuotaAssignmentBounds;
    public readonly ConfigEntry<int> BaseTargetIncomePerDay;
    public readonly ConfigEntry<float> MaxTargetIncomePerDayScalar;
    public readonly ConfigEntry<int> AssignmentsToReachMaximumTargetIncomePerDay;
    public readonly ConfigEntry<float> TargetIncomePerDayScalarCurvature;
    public readonly ConfigEntry<float> TargetIncomePerDayRandomnessScalar;

    public readonly ConfigEntry<bool> QuotaFormulaEnabled;
    public readonly ConfigEntry<int> StartingQuota;
    public readonly ConfigEntry<float> QuotaIncreaseSteepnessCoefficient;
    public readonly ConfigEntry<float> QuotaIncreaseSteepnessExponent;
    public readonly ConfigEntry<float> QuotaBaseIncrease;
    public readonly ConfigEntry<float> QuotaIncreaseRandomFactor;

    public readonly ConfigEntry<bool> ScrapTweaksEnabled;
    public readonly ConfigEntry<float> ScrapValueScalar;
    public readonly ConfigEntry<float> ScrapQuantityScalar;
    public readonly ConfigEntry<float> ScrapPlayercountScaling;

    public readonly ConfigEntry<ThreatScannerMode> ThreatScanner;

    public readonly ConfigEntry<bool> ScrapProtectionEnabled;
    public readonly ConfigEntry<float> ScrapProtection;
    public readonly ConfigEntry<float> ScrapProtectionRandomness;

    public readonly ConfigEntry<bool> DeathPenaltyFormulaEnabled;
    public readonly ConfigEntry<float> MaximumDeathPenalty;
    public readonly ConfigEntry<float> MaximumDeathPenaltyPerPlayer;
    public readonly ConfigEntry<float> DeadBodyRecoveryDiscount;
    public readonly ConfigEntry<float> DeathPenaltyScalingCurvature;

    public static void RegisterTypeConverters()
    {
        TomlTypeConverter.AddConverter(typeof(Interval<int>), new TypeConverter {
            ConvertToString = (Func<object, Type, string>)((obj, type) => obj.ToString()),
            ConvertToObject = (Func<string, Type, object>)((str, type) => Interval<int>.Parse(str))
        });
        TomlTypeConverter.AddConverter(typeof(Interval<float>), new TypeConverter {
            ConvertToString = (Func<object, Type, string>)((obj, type) => obj.ToString()),
            ConvertToObject = (Func<string, Type, object>)((str, type) => Interval<float>.Parse(str))
        });
    }

    public PluginConfig(BaseUnityPlugin bindingPlugin)
    {
        #region Global Config

        Enabled = bindingPlugin.Config.Bind(
            "Global",
            "bEnabled",
            true,
            "Globally enable/disable the plugin"
        );
        DelegationEnabled = bindingPlugin.Config.Bind(
            "Global",
            "bDelegationEnabled",
            true,
            "Globally enables/disables delegation for the plugin. When this is true, features will be disabled automatically (delegated to other mods) depending on the mods you have installed."
        );

        #endregion

        #region Misc Tweaks

        KeepConsoleEnabled = bindingPlugin.Config.Bind(
            "Misc Tweaks",
            "bAlwaysShowTerminal",
            true,
            "Whether to keep the terminal enabled after a player stops using it\nHost Required: No"
        );
        SuitUnlocksEnabled = bindingPlugin.Config.Bind(
            "Misc Tweaks",
            "bUnlockSuits",
            false,
            "Unlocks a few of the cheaper suits from the start so your crew has options.\nHost Required: Yes"
        );

        #endregion

        #region Company Buying Prices

        CompanyBuyingFactorTweaksEnabled = bindingPlugin.Config.Bind(
            "Company Buying Prices",
            "bCompanyBuyingFactorTweaksEnabled",
            false,
            "Whether or not the company buying price tweaks are enabled.\nHost required: Yes"
        );

        RandomiseCompanyBuyingFactor = bindingPlugin.Config.Bind(
            "Company Buying Prices",
            "bCompanyBuyingFactorRandomizerEnabled",
            false,
            "Randomises the company buying % when enabled. Great if you're using longer quota deadlines.\nThis uses a variety of things to randomize prices such as the company mood, time passed in the quota, etc.\nRespects the minimum sale value, too.\nHost Required: Yes"
        );
        MinimumCompanyBuyingFactor = bindingPlugin.Config.Bind(
            "Company Buying Prices",
            "fMinimumCompanyBuyingFactor",
            0.0f,
            "The default formula for selling items to the company isn't designed to handle more than 3 days remaining.\nThe Company will be prevented from offering a factor lower than this configured value.\nRecommended values for games above 3 days: 0.3 - 0.5\nHost Required: Yes"
        );

        #endregion

        #region Time Speed

        TimeSpeedEnabled = bindingPlugin.Config.Bind(
            "Time Speed",
            "bTimeSpeedEnabled",
            false,
            "Feature flag for the 'time speed' variable.\nHost Required: Yes"
        );
        TimeSpeed = bindingPlugin.Config.Bind(
            "Time Speed",
            "fTimeSpeed",
            1.0f,
            "How fast time passes on moons. Lower values mean time passes more slowly.\nRecommended value for single play: 0.75\nHost Required: Yes"
        );

        #endregion

        #region Door Power

        DoorPowerDurationEnabled = bindingPlugin.Config.Bind(
            "Door Power",
            "bDoorPowerDurationEnabled",
            false,
            "Feature flag for the 'door power duration' variable.\nHost Required: Yes"
        );
        DoorPowerDuration = bindingPlugin.Config.Bind(
            "Door Power",
            "fDoorPowerDuration",
            30.0f,
            "How long the hangar door can be kept shut at a time (in seconds)\nRecommended values: 60.0 - 180.0\nHost Required: All players should use the same value."
        );

        #endregion

        #region Starting Credits & Passive Income

        StartingCreditsEnabled = bindingPlugin.Config.Bind(
            "Starting Credits & Passive Income",
            "bStartingCreditsEnabled",
            false,
            "Feature flag for the 'starting credits' variable.\nHost Required: Yes"
        );
        StartingCredits = bindingPlugin.Config.Bind(
            "Starting Credits & Passive Income",
            "iStartingCredits",
            60,
            "How many credits the group starts with on a new run.\nHost Required: Yes"
        );
        PassiveIncomeEnabled = bindingPlugin.Config.Bind(
            "Starting Credits & Passive Income",
            "bPassiveIncomeEnabled",
            false,
            "Feature flag for the 'passive income' variable.\nHost Required: Yes"
        );
        PassiveIncomeQuantity = bindingPlugin.Config.Bind(
            "Starting Credits & Passive Income",
            "iPassiveIncomeAmount",
            0,
            "The number of credits you will be given at the end of each level.\nHost Required: Yes"
        );

        #endregion

        #region Quota Assignment Duration

        DaysPerQuotaAssignmentEnabled = bindingPlugin.Config.Bind(
            "Quota Assignment Duration",
            "bDaysPerQuotaAssignmentEnabled",
            false,
            "Feature flag for the 'days per quota' variable.\nHost Required: Yes"
        );
        DaysPerQuotaAssignmentBehaviour = bindingPlugin.Config.Bind(
            "Quota Assignment Duration",
            "eQuotaAssignmentBehaviour",
            QuotaDurationBehaviour.Constant,
            "The behaviour of the quota duration.\n" +
            "- Constant: Quota duration remains constant throughout play.\n" +
            "- Variable: Quota duration varies based upon 'target income per day' (configured below)\n" +
            "- DynamicVariable: Quota duration varies upon your lifetime average income per day .\n" +
            "Host Required: Yes"
        );
        DaysPerQuotaAssignment = bindingPlugin.Config.Bind(
            "Quota Assignment Duration",
            "iQuotaAssignmentDays",
            3,
            "How long you have to meet each quota (in days)\nRecommended values: 3 - 7\nHost Required: Yes"
        );
        DaysPerQuotaAssignmentBounds = bindingPlugin.Config.Bind(
            "Quota Assignment Duration",
            "bounds<i>QuotaAssignmentDays",
            new Interval<int>(3, 10),
            new ConfigDescription(
                "Bounds for the quota assignment duration when using variable quota duration behaviour.\n" + 
                "Host Required: Yes",
                new AcceptableInterval<int>(new Interval<int>(1, int.MaxValue))
            )
        );
        BaseTargetIncomePerDay = bindingPlugin.Config.Bind(
            "Quota Assignment Duration",
            "iBaseTargetIncomePerDay",
            200,
            new ConfigDescription(
                ""
            )
        );
        MaxTargetIncomePerDayScalar = bindingPlugin.Config.Bind( 
            "Quota Assignment Duration",
            "interval<f>TargetIncomePerDay",
            1.5f,
            new ConfigDescription(
                "Upper bound for target income per day multiplier when using variable quota duration behaviour.\n" +
                "Host Required: Yes"
            )
        );
        AssignmentsToReachMaximumTargetIncomePerDay = bindingPlugin.Config.Bind(
            "Quota Assignment Duration",
            "iAssignmentCountToReachMaximumTargetIncomePerDay",
            10,
            new ConfigDescription(
                "Number of assignments you must complete for target income per day to hit the upper-bound.\n" +
                "Host Required: Yes"
            )
        );
        TargetIncomePerDayScalarCurvature = bindingPlugin.Config.Bind(
            "Quota Assignment Duration",
            "fTargetIncomeScalarCurvature",
            0f,
            new ConfigDescription(
                "How curved the graph of target income per day against quota assignments completed is.\n" +
                "- 0: Target income per day increases linearly.\n" +
                "- 1: "
            )
        );
        TargetIncomePerDayRandomnessScalar = bindingPlugin.Config.Bind(
            "Quota Assignment Duration",
            "fTargetIncomeScalarRandomnessScalar",
            0.4f,
            new ConfigDescription(
                "Randomness of target income per day value used to calculate the quota duration.\n" +
                "Host Required: Yes",
                new AcceptableValueRange<float>(0, float.MaxValue)
            )
        );

        #endregion

        #region Quota Calculation

        QuotaFormulaEnabled = bindingPlugin.Config.Bind(
            "Quota Calculation",
            "bQuotaFormulaEnabled",
            false,
            "Feature flag for the 'quota formula' variables, which include:\n - 'starting quota'\n - 'quota increase steepness'\n - 'quota base increase'\n - 'quota increase randomness'\nHost Required: Yes"
        );
        StartingQuota = bindingPlugin.Config.Bind(
            "Quota Calculation",
            "iStartingQuota",
            130,
            "The starting quota on a new run.\nHost Required: Yes"
        );
        QuotaIncreaseSteepnessCoefficient = bindingPlugin.Config.Bind(
            "Quota Calculation",
            "fQuotaIncreaseSteepness",
            0.0625f,
            "Used in calculating quota increase. Coefficient for the increase factor dependent on the number of completed quota assignments.\nHost Required: Yes"
        );
        QuotaIncreaseSteepnessExponent = bindingPlugin.Config.Bind(
            "Quota Calculation",
            "fQuotaIncreaseSteepnessExponent",
            2f,
            "Used in calculating quota increase. Exponent for the increase factor dependent on the number of completed quota assignments.\nHost Required: Yes"
        );
        QuotaBaseIncrease = bindingPlugin.Config.Bind(
            "Quota Calculation",
            "fQuotaBaseIncrease",
            100f,
            "Used in calculating quota increase. Multiplier for the constant increase factor.\nHost Required: Yes"
        );
        QuotaIncreaseRandomFactor = bindingPlugin.Config.Bind(
            "Quota Calculation",
            "fQuotaIncreaseRandomFactor",
            0.5f,
            "Used in calculating quota increase. Multiplier for the random increase factor.\nHost Required: Yes"
        );

        #endregion

        #region Scrap Value, Quantity & Playercount Scaling

        ScrapTweaksEnabled = bindingPlugin.Config.Bind(
            "Scrap Value, Quantity & Playercount Scaling",
            "bScrapTweaksEnabled",
            false,
            new ConfigDescription(
                "Whether or not to enable scrap tweaks.\nHost required: Yes"
            )
        );

        ScrapValueScalar = bindingPlugin.Config.Bind(
            "Scrap Value, Quantity & Playercount Scaling",
            "fScrapValueScalar",
            1f,
            new ConfigDescription(
                "Multiplier for value of spawned scrap items. Should be a positive float." +
                "\nHost Required: Yes",
                new AcceptableValueRange<float>(0, float.MaxValue)
            )
        );
        ScrapQuantityScalar = bindingPlugin.Config.Bind(
            "Scrap Value, Quantity & Playercount Scaling",
            "fScrapQuantityScalar",
            1f,
            new ConfigDescription(
                "Multiplier for quantity of spawned scrap items. Should be a positive float.\n" +
                "Host Required: Yes",
                new AcceptableValueRange<float>(0, float.MaxValue)
            )
        );

        ScrapPlayercountScaling = bindingPlugin.Config.Bind(
            "Scrap Value, Quantity & Playercount Scaling",
            "fScrapPlayercountScaling",
            0f,
            new ConfigDescription(
                "Multiplier for 'playercount scaling' - Higher values increase scrap spawn quantity but decrease scrap value.\n" +
                "Has no effect when 4 or fewer players are present.\n" +
                "A value of 0 disables playercount scaling.\n" +
                "A value of 1.0 means that each additional player will cause 25% more scrap items to spawn (linear, not compounding) and reduce scrap value by an equal factor.\n" +
                "Host Required: Yes",
                new AcceptableValueRange<float>(0, float.MaxValue)
            )
        );

        #endregion

        #region Threat Scanner

        ThreatScanner = bindingPlugin.Config.Bind(
            "Threat Scanner",
            "eThreatScannerMode",
            ThreatScannerMode.Disabled,
            "How the threat scanner functions. Valid types:\n - Disabled\n - Contacts: Number of Enemies on level\n - ThreatLevelPercentage: Percentage of max enemies on level\n - ThreatLevelName: Vague Text description (In order of threat level) [Clear -> Green -> Yellow -> Orange - Red]\nHost Required: No"
        );

        #endregion

        #region Scrap Protection

        ScrapProtectionEnabled = bindingPlugin.Config.Bind(
            "Threat Scanner",
            "bScrapProtectionEnabled",
            false,
            new ConfigDescription(
                "Sets whether or not the scrap protection feature is enabled. \nHost Required: Yes"
            )
        );
        ScrapProtection = bindingPlugin.Config.Bind(
            "Threat Scanner",
            "fScrapProtection",
            0f,
            new ConfigDescription(
                "Sets the average probability that each scrap item is kept in the event that that no players survive a mission.\nThat is, this is the approximate average fraction of secured scrap items kept.\nHost Required: Yes",
                new AcceptableValueRange<float>(0f, 1f)
            )
        );
        ScrapProtectionRandomness = bindingPlugin.Config.Bind(
            "Threat Scanner",
            "fScrapProtectionRandomnessScalar",
            0f,
            new ConfigDescription(
                "Sets the randomness of the probability that each scrap item is kept in the event that that no players survive a mission.\n 0 -> no randomness, 0.5 -> \u00b10.5, 1 -> \u00b11\nHost Required: Yes",
                new AcceptableValueRange<float>(0f, 1f)
            )
        );

        #endregion

        #region Death Penalty

        DeathPenaltyFormulaEnabled = bindingPlugin.Config.Bind(
            "Death Penalty",
            "bDeathPenaltyFormulaEnabled",
            false,
            "Feature flag for the 'death penalty formula' variables, which includes\n - 'max death penalty'\n - 'max death penalty per player'\n - 'body recovery discount'\n - 'death penalty scaling curvature'\nHost Required: Yes"
        );
        MaximumDeathPenalty = bindingPlugin.Config.Bind(
            "Death Penalty",
            "fMaximumDeathPenalty",
            0.8f,
            new ConfigDescription(
                "The maximum fraction of your money that you can lose per round.\nValue should be in [0,1], e.g.\n0 - No money can be lost.\n0.5 - Half your money can be lost in one run.\n1 - All money can be lost in one run.\nUse 0.8 for vanilla behaviour.\nHost Required: Yes",
                new AcceptableValueRange<float>(0f, 1f)
            )
        );
        MaximumDeathPenaltyPerPlayer = bindingPlugin.Config.Bind(
            "Death Penalty",
            "fMaximumDeathPenaltyPerPlayer",
            0.2f,
            new ConfigDescription(
                "The maximum fraction of your money that you can lose per round, per dead player.\nValue should be in [0,1].\nUse 0.2 for vanilla behaviour.\nHost Required: Yes",
                new AcceptableValueRange<float>(0f, 1f)
            )
        );
        DeadBodyRecoveryDiscount = bindingPlugin.Config.Bind(
            "Death Penalty",
            "fDeadBodyRecoveryDiscount",
            0.6f,
            new ConfigDescription(
                "How much recovering dead bodies reduces the penalty for that death by.\nValue should be in [0,1], e.g.\n0 - Recovering a body does not reduce the fine.\n1 - Recovering a body completely removes the fine for that death.\nUse 0.6 for vanilla behaviour.\nHost Required: Yes",
                new AcceptableValueRange<float>(0f, 1f)
            )
        );
        DeathPenaltyScalingCurvature = bindingPlugin.Config.Bind(
            "Death Penalty",
            "fDeathPenaltyCurveDegree",
            0f,
            "How curved the death penalty scaling is. Positive -> less fine for fewer deaths. Negative -> more fine for fewer deaths.\ne.g. with a 4-player lobby:\n0 - The fine scales linearly: 25%, 50%, 75%, 100%.\n1 - The fine scales quadratically: 6.3%, 25%, 56.3%, 100%\n-1 - The fine scales anti-quadratically: 50%, 70.1%, 86.6%, 100%\nHost Required: Yes"
        );

        #endregion
    }
}