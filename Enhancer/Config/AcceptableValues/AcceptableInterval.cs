using System;
using BepInEx.Configuration;

namespace Enhancer.Config.AcceptableValues;

public class AcceptableInterval<T> : AcceptableValueBase where T : IComparable
{
    public AcceptableInterval(Interval<T>? interval) : base(typeof(Interval<T>))
    {
        Interval = interval ?? throw new ArgumentNullException(nameof(interval));
    }
    
    public virtual Interval<T> Interval { get; }

    public override object Clamp(object value)
    {
        if (!ValueType.IsInstanceOfType(value))
            throw new ArgumentException($"{nameof(value)} is not of type {ValueType.Name}");
        return Interval.Restrict((Interval<T>)value);
    }

    public override bool IsValid(object value)
    {
        if (!ValueType.IsInstanceOfType(value))
            throw new ArgumentException($"{nameof(value)} is not of type {ValueType.Name}");
        return Interval.Contains((Interval<T>)value);
    }

    public override string ToDescriptionString()
    {
        return $"# Acceptable bounds: Contained by {Interval}";
    }
}