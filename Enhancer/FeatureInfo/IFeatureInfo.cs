using System;
using Enhancer.Features;

namespace Enhancer.FeatureInfo;

public interface IFeatureInfo<out TFeature> : IDisposable where TFeature : IFeature
{
    public string Name { get; }
    public bool IsEnabled { get; }
    public void Initialise();
}
