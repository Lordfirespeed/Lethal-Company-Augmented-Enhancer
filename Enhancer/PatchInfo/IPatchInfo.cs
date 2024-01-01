using System;
using Enhancer.Patches;

namespace Enhancer.PatchInfo;

public interface IPatchInfo<out TPatch> : IDisposable where TPatch : IPatch
{
    public string Name { get; }
    public bool IsEnabled { get; }
    public void Initialise();
}