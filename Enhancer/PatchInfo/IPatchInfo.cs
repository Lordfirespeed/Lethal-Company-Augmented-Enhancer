using System;
using Enhancer.Patches;
using HarmonyLib;

namespace Enhancer.PatchInfo;

public interface IPatchInfo<out TPatch> where TPatch : IPatch
{
    public string Name { get; }
    public bool IsEnabled { get; }
    public void Initialise(Func<string, Harmony> harmonyFactory);
}