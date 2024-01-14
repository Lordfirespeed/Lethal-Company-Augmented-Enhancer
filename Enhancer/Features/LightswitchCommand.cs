using System.Text;
using BepInEx.Logging;
using HarmonyLib;
using TerminalApi;
using TerminalApi.Classes;
using Unity.Netcode;
using TerminalApiHelper = TerminalApi.TerminalApi;

namespace Enhancer.Features;

public class LightswitchCommand : IFeature
{
    protected static ManualLogSource Logger { get; set; } = null!;
    protected static LightswitchCommand? Instance { get; set; } = null;

    private bool _initialized = false;

    private TerminalKeyword? _lightsVerbKeyword;
    private TerminalKeyword? _helpNounKeyword;
    private TerminalKeyword? _onNounKeyword;
    private TerminalKeyword? _offNounKeyword;
    private TerminalKeyword? _toggleNounKeyword;

    private CommandInfo? _onCommandInfo;
    private CommandInfo? _offCommandInfo;
    private CommandInfo? _toggleCommandInfo;

    public void SetLogger(ManualLogSource logger)
    {
        logger.LogDebug("Logger assigned.");
        Logger = logger;
    }

    private static ShipLights? _shipLightsInstance = null;

    private static ShipLights? ShipLightsInstance {
        get {
            if (_shipLightsInstance is not null) return _shipLightsInstance;

            _shipLightsInstance ??= UnityEngine.Object.FindObjectOfType<ShipLights>();

            return _shipLightsInstance;
        }
    }

    private static bool ShipLightsAreOn => ShipLightsInstance is not null && ShipLightsInstance.areLightsOn;

    private static void SetShipLights(bool lightsOn)
    {
        if (_shipLightsInstance is null) return;
        _shipLightsInstance.SetShipLightsBoolean(lightsOn);
    }

    public static string ToggleLightsTextSupplier() => SetLightsTextSupplier(!ShipLightsAreOn);

    public static string SetLightsTextSupplier(bool lightsOn)
    {
        SetShipLights(lightsOn);
        return $"Lights {(lightsOn ? "on" : "out")}!\n\n";
    }

    private static readonly string LightsHelpText = new StringBuilder()
        .AppendLine(">LIGHTS HELP")
        .AppendLine("Display this information.")
        .AppendLine()
        .AppendLine(">LIGHTS ON")
        .AppendLine("Turns the ship interior lights on.")
        .AppendLine()
        .AppendLine(">LIGHTS OFF")
        .AppendLine("Turns the ship interior lights off.")
        .AppendLine()
        .AppendLine(">LIGHTS TOGGLE")
        .AppendLine("Toggles the ship interior lights on/off.")
        .AppendLine()
        .ToString();

    public void Initialize()
    {
        if (_initialized) return;
        _initialized = true;

        var triggerHelpNode = TerminalApiHelper.CreateTerminalNode(LightsHelpText, true);
        var triggerOnNode = TerminalApiHelper.CreateTerminalNode("Lights on\n", true);
        var triggerOffNode = TerminalApiHelper.CreateTerminalNode("Lights out\n", true);
        var triggerToggleNode = TerminalApiHelper.CreateTerminalNode("Lights toggle\n", true);

        _lightsVerbKeyword = TerminalApiHelper.CreateTerminalKeyword("lights", true, triggerToggleNode);

        _helpNounKeyword = TerminalApiHelper.GetKeyword("help");
        _onNounKeyword = TerminalApiHelper.CreateTerminalKeyword("on");
        _offNounKeyword = TerminalApiHelper.CreateTerminalKeyword("off");
        _toggleNounKeyword = TerminalApiHelper.CreateTerminalKeyword("toggle");

        _lightsVerbKeyword.AddCompatibleNoun(_helpNounKeyword, triggerHelpNode);
        _lightsVerbKeyword.AddCompatibleNoun(_onNounKeyword, triggerOnNode);
        _lightsVerbKeyword.AddCompatibleNoun(_offNounKeyword, triggerOffNode);
        _lightsVerbKeyword.AddCompatibleNoun(_toggleNounKeyword, triggerToggleNode);

        TerminalApiHelper.AddTerminalKeyword(_lightsVerbKeyword, new CommandInfo {
            Title = "Lights",
            Category = "Other",
            Description = "Lightswitch-related commands. Run 'lights help' for info."
        });

        TerminalApiHelper.AddTerminalKeyword(_helpNounKeyword);

        _onCommandInfo = new CommandInfo {
            TriggerNode = triggerOnNode,
            DisplayTextSupplier = () => SetLightsTextSupplier(true),
        };
        TerminalApiHelper.AddTerminalKeyword(_onNounKeyword);
        TerminalApiHelper.CommandInfos.Add(_onCommandInfo);

        _offCommandInfo = new CommandInfo {
            TriggerNode = triggerOffNode,
            DisplayTextSupplier = () => SetLightsTextSupplier(false),
        };
        TerminalApiHelper.AddTerminalKeyword(_offNounKeyword);
        TerminalApiHelper.CommandInfos.Add(_offCommandInfo);

        _toggleCommandInfo = new CommandInfo {
            TriggerNode = triggerToggleNode,
            DisplayTextSupplier = ToggleLightsTextSupplier,
        };
        TerminalApiHelper.AddTerminalKeyword(_toggleNounKeyword);
        TerminalApiHelper.CommandInfos.Add(_toggleCommandInfo);
    }

    public void OnEnable()
    {
        Instance = this;
        if (NetworkManager.Singleton is null) return;
        if (!NetworkManager.Singleton.IsConnectedClient) return;
        if (UnityEngine.Object.FindObjectOfType<Terminal>() is null) return;
        Initialize();
    }

    public void OnDisable()
    {
        if (Instance == this) Instance = null;
        if (!_initialized) return;

        if (_lightsVerbKeyword is not null)
            TerminalApiHelper.DeleteKeyword(_lightsVerbKeyword.word);
        if (_onNounKeyword is not null)
            TerminalApiHelper.DeleteKeyword(_onNounKeyword.word);
        if (_offNounKeyword is not null)
            TerminalApiHelper.DeleteKeyword(_offNounKeyword.word);
        if (_toggleNounKeyword is not null)
            TerminalApiHelper.DeleteKeyword(_toggleNounKeyword.word);

        TerminalApiHelper.CommandInfos.Remove(_onCommandInfo);
        TerminalApiHelper.CommandInfos.Remove(_offCommandInfo);
        TerminalApiHelper.CommandInfos.Remove(_toggleCommandInfo);
    }

    [HarmonyPatch(typeof(Terminal), nameof(Terminal.Awake))]
    [HarmonyPostfix]
    public static void OnTerminalAwake()
    {
        Instance?.Initialize();
    }
}
