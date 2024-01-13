using System;
using System.Text.RegularExpressions;
using BepInEx.Configuration;

namespace Enhancer.Config;

public class Interval<T> where T : IComparable
{
    public Interval(T? lowerBound, T? upperBound)
    {
        if (upperBound == null)
            throw new ArgumentNullException(nameof(upperBound));
        if (lowerBound == null)
            throw new ArgumentNullException(nameof(lowerBound));
        LowerEndpoint = lowerBound.CompareTo(upperBound) <= 0 ? lowerBound : throw new ArgumentException("lowerBound has to be lesser than or equal to than upperBound");
        UpperEndpoint = upperBound;
    }

    public virtual T LowerEndpoint { get; }

    public virtual T UpperEndpoint { get; }

    public virtual bool LowerEndpointClosed { get; set; } = true;

    public virtual bool UpperEndpointClosed { get; set; } = true;

    public virtual Interval<T> Restrict(Interval<T> interval)
    {
        var lowerEndpointCompared = LowerEndpoint.CompareTo(interval.LowerEndpoint);
        var upperEndpointCompared = UpperEndpoint.CompareTo(interval.UpperEndpoint);

        bool lowerEndpointClosed = true;
        bool upperEndpointClosed = true;

        if (lowerEndpointCompared < 0) lowerEndpointClosed = interval.LowerEndpointClosed;
        if (lowerEndpointCompared == 0) lowerEndpointClosed = LowerEndpointClosed && interval.LowerEndpointClosed;
        if (lowerEndpointCompared > 0) lowerEndpointClosed = LowerEndpointClosed;

        if (upperEndpointCompared < 0) upperEndpointClosed = UpperEndpointClosed;
        if (upperEndpointCompared == 0) upperEndpointClosed = UpperEndpointClosed && interval.UpperEndpointClosed;
        if (upperEndpointCompared > 0) upperEndpointClosed = interval.UpperEndpointClosed;

        return new Interval<T>(
            lowerEndpointCompared <= 0 ? interval.LowerEndpoint : LowerEndpoint,
            upperEndpointCompared >= 0 ? interval.UpperEndpoint : UpperEndpoint
        ) {
            LowerEndpointClosed = lowerEndpointClosed,
            UpperEndpointClosed = upperEndpointClosed,
        };
    }

    public virtual bool Contains(Interval<T> interval)
    {
        var lowerEndpointCompared = LowerEndpoint.CompareTo(interval.LowerEndpoint);
        var upperEndpointCompared = UpperEndpoint.CompareTo(interval.UpperEndpoint);

        if (lowerEndpointCompared > 0) return false;
        if (lowerEndpointCompared == 0 && !LowerEndpointClosed && interval.LowerEndpointClosed) return false;
        if (upperEndpointCompared < 0) return false;
        if (upperEndpointCompared == 0 && !UpperEndpointClosed && interval.UpperEndpointClosed) return false;
        return true;
    }

    public virtual T Clamp(T value)
    {
        if (value.CompareTo(LowerEndpoint) <= 0) {
            if (!LowerEndpointClosed) throw new InvalidOperationException("Can't clamp to a closed interval endpoint.");
            return LowerEndpoint;
        }

        if (value.CompareTo(UpperEndpoint) >= 0) {
            if (!UpperEndpointClosed) throw new InvalidOperationException("Can't clamp to a closed interval endpoint.");
            return UpperEndpoint;
        }

        return value;
    }

    public override string ToString()
    {
        return $"{(LowerEndpointClosed ? '[' : '(')}{LowerEndpoint}, {UpperEndpoint}{(UpperEndpointClosed ? ']' : ')')}";
    }

    public static Regex IntervalRegex = new(@"(?<openingParen>[([])(?<lowerBound>.*), (?<upperBound>.*)(?<closingParen>[)\]])", RegexOptions.Compiled);

    public static Interval<T> Parse(string value)
    {
        var match = IntervalRegex.Match(value);
        if (!match.Success) throw new ArgumentException($"{nameof(value)} could not be parsed as an Interval.");

        var lowerBound = TomlTypeConverter.ConvertToValue<T>(match.Groups["lowerBound"].Value);
        var upperBound = TomlTypeConverter.ConvertToValue<T>(match.Groups["upperBound"].Value);

        return new Interval<T>(lowerBound, upperBound) {
            LowerEndpointClosed = match.Groups["openingParen"].Value == "[",
            UpperEndpointClosed = match.Groups["closingParen"].Value == "]",
        };
    }
}
