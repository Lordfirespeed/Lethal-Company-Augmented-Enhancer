using BepInEx.Logging;
using HarmonyLib;

namespace Enhancer.Features;

public interface IFeature
{
    public void SetLogger(ManualLogSource logger) { }
    public void SetHarmony(Harmony harmony) { }
    public void OnEnable() { }
    public void OnDisable() { }
    public void OnConfigChange() { }
}
