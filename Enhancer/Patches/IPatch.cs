using BepInEx.Logging;

namespace Enhancer.Patches;

public interface IPatch
{
    public void SetLogger(ManualLogSource logger) { }
    public void OnPatch() { }
    public void OnUnpatch() { }
    public void OnConfigChange() { }
}
