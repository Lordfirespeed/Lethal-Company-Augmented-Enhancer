/**********************************************************
    Plugin Config Information Class

    All configuration options go in here
***********************************************************/

using BepInEx.Configuration;

namespace Enhancer;

public class PluginConfig
{

    public readonly bool Enabled;
    public readonly bool KeepConsoleEnabled;
    public readonly bool UseRandomPrices;
    public readonly bool DoSuitPatches;

    public readonly float TimeScale;
    public readonly float MinimumBuyRate;
    public readonly float DoorTimer;

    public readonly int DaysPerQuota;
    public readonly int ThreatScannerType;

    public readonly float MaxDeathPenalty;
    public readonly float MaxDeathPenaltyPerPlayer;
    public readonly float DeadBodyRecoveryDiscount;
    public readonly float DeathPenaltyScalingCurvature;

    public readonly Patches.ItemProtection.ProtectionType ScrapProtection;

    public PluginConfig(Plugin BindingPlugin)
    {
        Enabled = BindingPlugin.Config.Bind(PluginInfo.PLUGIN_GUID, "bEnabled", true, "Globally enable/disable the plugin").Value;
        KeepConsoleEnabled = BindingPlugin.Config.Bind(PluginInfo.PLUGIN_GUID, "bAlwaysShowTerminal", true, "Whether to keep the terminal enabled after a player stops using it\nHost Required: No").Value;
        UseRandomPrices = BindingPlugin.Config.Bind(PluginInfo.PLUGIN_GUID, "bUseRandomPrices", false, "Enables the random prices setting. Great if you're using longer quota deadlines.\nThis uses a variety of things to randomize prices such as the company mood, time passed in the quota, etc.\nRespects the minimum sale value, too.\nHost Required: Yes").Value;
        DoSuitPatches = BindingPlugin.Config.Bind(PluginInfo.PLUGIN_GUID, "bUnlockSuits", false, "Unlocks a few of the cheaper suits from the start so your crew has options.\nHost Required: Yes").Value;

        TimeScale = BindingPlugin.Config.Bind(PluginInfo.PLUGIN_GUID, "fTimeScale", 1.5f, "How fast time passes on moons. Lower values mean time passes more slowly.\nRecommended value for single play: 1.15\nHost Required: Yes").Value;
        MinimumBuyRate = BindingPlugin.Config.Bind(PluginInfo.PLUGIN_GUID, "fMinCompanyBuyPCT", 0.0f, "The default formula for selling items to the company doesn't allow days remaining above 3.\nAlways keep this set to at least 0.0 but you probably want something higher if you have more days set for the quota.\nRecommended values for games above 3 days: 0.3 - 0.5\nHost Required: Yes").Value;
        DoorTimer = BindingPlugin.Config.Bind(PluginInfo.PLUGIN_GUID, "fDoorTimer", 30.0f, "How long the hangar door can be kept shut at a time (in seconds)\nRecommended values: 60.0 - 180.0\nHost Required: All players should use the same setting here").Value;

        DaysPerQuota = BindingPlugin.Config.Bind(PluginInfo.PLUGIN_GUID, "iQuotaDays", 3, "How long you have to meet each quota (in days)\nRecommended values: 3 - 7\nHost Required: Yes").Value;
        ThreatScannerType = BindingPlugin.Config.Bind(PluginInfo.PLUGIN_GUID, "eThreatScannerType", 0, "How the threat scanner functions. Valid types:\n0 - Disabled\n1 - Number of Enemies on level\n2 - Percentage of max enemies on level\n3 - Vague Text description (In order of threat level) [Clear -> Green -> Yellow -> Orange - Red]\nHost Required: No").Value;

        MaxDeathPenalty = BindingPlugin.Config.Bind(
            PluginInfo.PLUGIN_GUID, 
            "fMaxDeathPenalty", 
            0.8f, 
            new ConfigDescription(
                "The maximum fraction of your money that you can lose per round.\nValue should be in [0,1], e.g.\n0 - No money can be lost.\n0.5 - Half your money can be lost in one run.\n1 - All money can be lost in one run.\nUse 0.8 for vanilla behaviour.\nHost Required: Yes",
                new AcceptableValueRange<float>(0f, 1f)
            )
        ).Value;
        MaxDeathPenaltyPerPlayer = BindingPlugin.Config.Bind(
            PluginInfo.PLUGIN_GUID, 
            "fMaxDeathPenaltyPerPlayer", 
            0.2f, 
            new ConfigDescription(
                "The maximum fraction of your money that you can lose per round, per dead player.\nValue should be in [0,1].\nUse 0.2 for vanilla behaviour.\nHost Required: Yes",
                new AcceptableValueRange<float>(0f, 1f)
            )
        ).Value;
        DeadBodyRecoveryDiscount = BindingPlugin.Config.Bind(
            PluginInfo.PLUGIN_GUID, 
            "fDeadBodyRecoveryDiscount", 
            0.6f, 
            new ConfigDescription(
                "How much recovering dead bodies reduces the penalty for that death by.\nValue should be in [0,1], e.g.\n0 - Recovering a body does not reduce the fine.\n1 - Recovering a body completely removes the fine for that death.\nUse 0.6 for vanilla behaviour.\nHost Required: Yes",
                new AcceptableValueRange<float>(0f, 1f)
            )
        ).Value;
        DeathPenaltyScalingCurvature = BindingPlugin.Config.Bind(
            PluginInfo.PLUGIN_GUID, 
            "fDeathPenaltyCurveDegree", 
            0f, 
            "How curved the death penalty scaling is. Positive -> less fine for fewer deaths. Negative -> more fine for fewer deaths.\ne.g. with a 4-player lobby:\n0 - The fine scales linearly: 25%, 50%, 75%, 100%.\n1 - The fine scales quadratically: 6.3%, 25%, 56.3%, 100%\n-1 - The fine scales anti-quadratically: 50%, 70.1%, 86.6%, 100%"
        ).Value;
        
        ScrapProtection = BindingPlugin.Config.Bind(PluginInfo.PLUGIN_GUID, "eScrapProtection", Patches.ItemProtection.ProtectionType.SAVE_NONE, "Sets how scrap will be handled when all players die in a round.\nSAVE_NONE: Default all scrap is deleted\nSAVE_ALL: No scrap is removed\nSAVE_COINFLIP: Each piece of scrap has a 50/50 of being removed\nHost Required: Yes").Value;
    }
}