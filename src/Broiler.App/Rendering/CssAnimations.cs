using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Broiler.App.Rendering;

/// <summary>CSS transition timing function values.</summary>
public enum CssTimingFunction { Ease, Linear, EaseIn, EaseOut, EaseInOut }

/// <summary>CSS animation direction values.</summary>
public enum CssAnimationDirection { Normal, Reverse, Alternate, AlternateReverse }

/// <summary>CSS animation fill mode values.</summary>
public enum CssAnimationFillMode { None, Forwards, Backwards, Both }

/// <summary>Represents a CSS transition for a single property.</summary>
public class CssTransition
{
    /// <summary>CSS property to transition (e.g. "opacity", "transform", "all").</summary>
    public string Property { get; set; } = "all";

    /// <summary>How long the transition takes.</summary>
    public TimeSpan Duration { get; set; }

    /// <summary>Easing function for the transition.</summary>
    public CssTimingFunction TimingFunction { get; set; } = CssTimingFunction.Ease;

    /// <summary>Delay before the transition starts.</summary>
    public TimeSpan Delay { get; set; }

    /// <summary>Parses a CSS transition shorthand value like "opacity 0.3s ease-in 0.1s".</summary>
    public static CssTransition Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Transition value must not be empty.", nameof(value));

        var result = new CssTransition();
        var parts = value.Trim().Split([' '], StringSplitOptions.RemoveEmptyEntries);
        var durations = new List<TimeSpan>();

        foreach (var part in parts)
        {
            if (TryParseTimingFunction(part, out var fn))
            {
                result.TimingFunction = fn;
            }
            else if (TryParseDuration(part, out var duration))
            {
                durations.Add(duration);
            }
            else
            {
                result.Property = part;
            }
        }

        if (durations.Count >= 1)
            result.Duration = durations[0];
        if (durations.Count >= 2)
            result.Delay = durations[1];

        return result;
    }

    /// <summary>Parses a comma-separated list of CSS transition shorthand values.</summary>
    public static IReadOnlyList<CssTransition> ParseMultiple(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Transition value must not be empty.", nameof(value));

        return value.Split(',')
            .Select(s => s.Trim())
            .Where(s => s.Length > 0)
            .Select(Parse)
            .ToList();
    }

    /// <summary>Attempts to parse a CSS timing function name.</summary>
    public static bool TryParseTimingFunction(string text, out CssTimingFunction fn)
    {
        switch (text.ToLowerInvariant())
        {
            case "ease": fn = CssTimingFunction.Ease; return true;
            case "linear": fn = CssTimingFunction.Linear; return true;
            case "ease-in": fn = CssTimingFunction.EaseIn; return true;
            case "ease-out": fn = CssTimingFunction.EaseOut; return true;
            case "ease-in-out": fn = CssTimingFunction.EaseInOut; return true;
            default: fn = default; return false;
        }
    }

    /// <summary>Attempts to parse a CSS duration value (s or ms units).</summary>
    public static bool TryParseDuration(string text, out TimeSpan duration)
    {
        duration = TimeSpan.Zero;
        var lower = text.ToLowerInvariant();

        if (lower.EndsWith("ms"))
        {
            if (float.TryParse(lower.Substring(0, lower.Length - 2),
                NumberStyles.Float, CultureInfo.InvariantCulture, out var ms))
            {
                duration = TimeSpan.FromMilliseconds(ms);
                return true;
            }
        }
        else if (lower.EndsWith("s"))
        {
            if (float.TryParse(lower.Substring(0, lower.Length - 1),
                NumberStyles.Float, CultureInfo.InvariantCulture, out var s))
            {
                duration = TimeSpan.FromSeconds(s);
                return true;
            }
        }

        return false;
    }
}

/// <summary>Represents a single keyframe in a CSS <c>@keyframes</c> rule.</summary>
public class CssKeyframe
{
    /// <summary>Keyframe position from 0.0 (0%/from) to 1.0 (100%/to).</summary>
    public float Selector { get; set; }

    /// <summary>CSS property-value pairs declared at this keyframe.</summary>
    public Dictionary<string, string> Declarations { get; set; } = [];
}

/// <summary>Represents a named <c>@keyframes</c> animation definition.</summary>
public class CssAnimationDefinition
{
    /// <summary>Animation name from the <c>@keyframes</c> rule.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Ordered list of keyframes.</summary>
    public IReadOnlyList<CssKeyframe> Keyframes { get; set; } = Array.Empty<CssKeyframe>();

    /// <summary>Parses a <c>@keyframes</c> block body into an animation definition.</summary>
    public static CssAnimationDefinition Parse(string name, string body)
    {
        if (name == null) throw new ArgumentNullException(nameof(name));
        if (body == null) throw new ArgumentNullException(nameof(body));

        var keyframes = new List<CssKeyframe>();
        var trimmed = body.Trim();

        // Split on '}' to find individual keyframe blocks
        var blocks = trimmed.Split(['}'], StringSplitOptions.RemoveEmptyEntries);
        foreach (var block in blocks)
        {
            var openBrace = block.IndexOf('{');
            if (openBrace < 0) continue;

            var selectorText = block.Substring(0, openBrace).Trim();
            var declarationText = block.Substring(openBrace + 1).Trim();

            var selectors = ParseKeyframeSelectors(selectorText);
            var declarations = ParseDeclarations(declarationText);

            foreach (var sel in selectors)
            {
                keyframes.Add(new CssKeyframe
                {
                    Selector = sel,
                    Declarations = new Dictionary<string, string>(declarations)
                });
            }
        }

        keyframes.Sort((a, b) => a.Selector.CompareTo(b.Selector));

        return new CssAnimationDefinition { Name = name, Keyframes = keyframes };
    }

    /// <summary>Parses keyframe selector text (e.g. "from", "50%", "to") into float values.</summary>
    private static List<float> ParseKeyframeSelectors(string text)
    {
        var results = new List<float>();
        var parts = text.Split([','], StringSplitOptions.RemoveEmptyEntries);
        foreach (var part in parts)
        {
            var s = part.Trim().ToLowerInvariant();
            if (s == "from")
            {
                results.Add(0f);
            }
            else if (s == "to")
            {
                results.Add(1f);
            }
            else if (s.EndsWith("%") && float.TryParse(s.Substring(0, s.Length - 1),
                NumberStyles.Float, CultureInfo.InvariantCulture, out var pct))
            {
                results.Add(pct / 100f);
            }
        }
        return results;
    }

    /// <summary>Parses semicolon-separated CSS declarations into a dictionary.</summary>
    private static Dictionary<string, string> ParseDeclarations(string text)
    {
        var result = new Dictionary<string, string>();
        var declarations = text.Split([';'], StringSplitOptions.RemoveEmptyEntries);
        foreach (var decl in declarations)
        {
            var colon = decl.IndexOf(':');
            if (colon < 0) continue;

            var property = decl.Substring(0, colon).Trim();
            var val = decl.Substring(colon + 1).Trim();
            if (property.Length > 0)
                result[property] = val;
        }
        return result;
    }
}

/// <summary>Represents a CSS animation shorthand applied to an element.</summary>
public class CssAnimation
{
    /// <summary>Name of the animation (references a <see cref="CssAnimationDefinition"/>).</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Duration of a single animation cycle.</summary>
    public TimeSpan Duration { get; set; }

    /// <summary>Easing function for the animation.</summary>
    public CssTimingFunction TimingFunction { get; set; } = CssTimingFunction.Ease;

    /// <summary>Delay before the animation starts.</summary>
    public TimeSpan Delay { get; set; }

    /// <summary>Number of iterations. Use <see cref="float.PositiveInfinity"/> for infinite.</summary>
    public float IterationCount { get; set; } = 1f;

    /// <summary>Direction of the animation playback.</summary>
    public CssAnimationDirection Direction { get; set; } = CssAnimationDirection.Normal;

    /// <summary>Fill mode controlling styles before/after animation.</summary>
    public CssAnimationFillMode FillMode { get; set; } = CssAnimationFillMode.None;

    /// <summary>Parses a CSS animation shorthand value.</summary>
    public static CssAnimation Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Animation value must not be empty.", nameof(value));

        var result = new CssAnimation();
        var parts = value.Trim().Split([' '], StringSplitOptions.RemoveEmptyEntries);
        var durations = new List<TimeSpan>();

        foreach (var part in parts)
        {
            if (CssTransition.TryParseTimingFunction(part, out var fn))
            {
                result.TimingFunction = fn;
            }
            else if (CssTransition.TryParseDuration(part, out var dur))
            {
                durations.Add(dur);
            }
            else if (TryParseDirection(part, out var dir))
            {
                result.Direction = dir;
            }
            else if (TryParseFillMode(part, out var fill))
            {
                result.FillMode = fill;
            }
            else if (TryParseIterationCount(part, out var count))
            {
                result.IterationCount = count;
            }
            else
            {
                result.Name = part;
            }
        }

        if (durations.Count >= 1)
            result.Duration = durations[0];
        if (durations.Count >= 2)
            result.Delay = durations[1];

        return result;
    }

    /// <summary>Attempts to parse a CSS animation direction value.</summary>
    private static bool TryParseDirection(string text, out CssAnimationDirection dir)
    {
        switch (text.ToLowerInvariant())
        {
            case "normal": dir = CssAnimationDirection.Normal; return true;
            case "reverse": dir = CssAnimationDirection.Reverse; return true;
            case "alternate": dir = CssAnimationDirection.Alternate; return true;
            case "alternate-reverse": dir = CssAnimationDirection.AlternateReverse; return true;
            default: dir = default; return false;
        }
    }

    /// <summary>Attempts to parse a CSS animation fill mode value.</summary>
    private static bool TryParseFillMode(string text, out CssAnimationFillMode fill)
    {
        switch (text.ToLowerInvariant())
        {
            case "none": fill = CssAnimationFillMode.None; return true;
            case "forwards": fill = CssAnimationFillMode.Forwards; return true;
            case "backwards": fill = CssAnimationFillMode.Backwards; return true;
            case "both": fill = CssAnimationFillMode.Both; return true;
            default: fill = default; return false;
        }
    }

    /// <summary>Attempts to parse an animation iteration count value.</summary>
    private static bool TryParseIterationCount(string text, out float count)
    {
        if (text.ToLowerInvariant() == "infinite")
        {
            count = float.PositiveInfinity;
            return true;
        }

        if (float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out count)
            && count >= 0)
        {
            return true;
        }

        count = 0;
        return false;
    }
}

/// <summary>Utility class for computing transition and animation interpolation.</summary>
public static class CssTransitionEngine
{
    /// <summary>Linearly interpolates between two values.</summary>
    public static float Interpolate(float from, float to, float progress) => from + (to - from) * progress;

    /// <summary>Applies an easing curve to a linear progress value (0–1).</summary>
    public static float ApplyTimingFunction(CssTimingFunction fn, float t)
    {
        switch (fn)
        {
            case CssTimingFunction.Linear:
                return t;
            case CssTimingFunction.Ease:
                return CubicBezier(0.25f, 0.1f, 0.25f, 1.0f, t);
            case CssTimingFunction.EaseIn:
                return CubicBezier(0.42f, 0f, 1.0f, 1.0f, t);
            case CssTimingFunction.EaseOut:
                return CubicBezier(0f, 0f, 0.58f, 1.0f, t);
            case CssTimingFunction.EaseInOut:
                return CubicBezier(0.42f, 0f, 0.58f, 1.0f, t);
            default:
                return t;
        }
    }

    /// <summary>
    /// Computes a cubic bezier value for timing curves.
    /// Control points are (0,0), (p1x,p1y), (p2x,p2y), (1,1).
    /// </summary>
    public static float CubicBezier(float p1x, float p1y, float p2x, float p2y, float t)
    {
        // Find the parametric t value for the given x using Newton's method
        float paramT = FindParametricT(p1x, p2x, t);
        // Evaluate the y value at that parametric t
        return EvaluateCubic(p1y, p2y, paramT);
    }

    /// <summary>Evaluates a cubic bezier axis at parametric value t.</summary>
    private static float EvaluateCubic(float p1, float p2, float t)
    {
        // B(t) = 3(1-t)^2*t*p1 + 3(1-t)*t^2*p2 + t^3
        float oneMinusT = 1f - t;
        return 3f * oneMinusT * oneMinusT * t * p1
             + 3f * oneMinusT * t * t * p2
             + t * t * t;
    }

    /// <summary>Evaluates the derivative of a cubic bezier axis at parametric value t.</summary>
    private static float EvaluateCubicDerivative(float p1, float p2, float t)
    {
        float oneMinusT = 1f - t;
        return 3f * oneMinusT * oneMinusT * p1
             + 6f * oneMinusT * t * (p2 - p1)
             + 3f * t * t * (1f - p2);
    }

    /// <summary>Finds the parametric t for a given x value using Newton-Raphson iteration.</summary>
    private static float FindParametricT(float p1x, float p2x, float x)
    {
        float guess = x;
        for (int i = 0; i < 8; i++)
        {
            float currentX = EvaluateCubic(p1x, p2x, guess) - x;
            float derivative = EvaluateCubicDerivative(p1x, p2x, guess);
            if (Math.Abs(derivative) < 1e-6f)
                break;
            guess -= currentX / derivative;
        }
        return Math.Max(0f, Math.Min(1f, guess));
    }
}
