using System;
using Enhancer.Patches;
using HarmonyLib;

namespace Enhancer.PatchInfo;

public interface IPatchInfo<out TPatch> : IDisposable where TPatch : IPatch
{
    public string Name { get; }
    public bool IsEnabled { get; }
    public void Initialise(Func<string, Harmony> harmonyFactory);
}