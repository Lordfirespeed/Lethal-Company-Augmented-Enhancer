using BepInEx.Logging;

namespace Enhancer.Features;

public interface IFeature
{
    public void SetLogger(ManualLogSource logger) { }
    public void OnEnable() { }
    public void OnDisable() { }
    public void OnConfigChange() { }
}
