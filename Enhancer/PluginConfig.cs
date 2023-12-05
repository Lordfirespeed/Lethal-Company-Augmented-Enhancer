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

    public readonly bool ScrapProtectionEnabled;
    public readonly float ScrapProtection;
    public readonly float ScrapProtectionRandomness;

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

        ScrapProtectionEnabled = BindingPlugin.Config.Bind(
            PluginInfo.PLUGIN_GUID,
            "bScrapProtectionEnabled",
            false,
            new ConfigDescription(
                "Sets whether or not the scrap protection feature is enabled. \nHost Required: Yes"
            )
        ).Value;
        ScrapProtection = BindingPlugin.Config.Bind(
            PluginInfo.PLUGIN_GUID, 
            "fScrapProtection", 
            0f, 
            new ConfigDescription(
                "Sets the average probability that each scrap item is kept in the event that that no players survive a mission.\nThat is, this is the approximate average fraction of secured scrap items kept.\nHost Required: Yes",
                new AcceptableValueRange<float>(0f, 1f)
            )
        ).Value;
        ScrapProtectionRandomness = BindingPlugin.Config.Bind(
            PluginInfo.PLUGIN_GUID, 
            "fScrapProtectionRandomnessScalar", 
            0f, 
            new ConfigDescription(
                "Sets the randomness of the probability that each scrap item is kept in the event that that no players survive a mission.\n 0 -> no randomness, 0.5 -> \u00b10.5, 1 -> \u00b11\nHost Required: Yes",
                new AcceptableValueRange<float>(0f, 1f)
            )
        ).Value;
    }
}