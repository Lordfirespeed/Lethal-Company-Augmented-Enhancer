using BepInEx.Logging;

namespace Enhancer.Patches;

public abstract class BasePatch : IPatch
{
    protected static ManualLogSource Logger { get; set; } = null!;

    public void SetLogger(ManualLogSource logger)
    {
        logger.LogDebug("Logger assigned.");
        Logger = logger;
    }
}