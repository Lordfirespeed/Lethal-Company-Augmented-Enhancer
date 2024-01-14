using BepInEx.Logging;

namespace Enhancer.Features;

public interface IPatch
{
    public void SetLogger(ManualLogSource logger) { }
    public void OnPatch() { }
    public void OnUnpatch() { }
    public void OnConfigChange() { }
}
