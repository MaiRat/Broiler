using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace TheArtOfDev.HtmlRenderer.Core.Parse;

internal static class RegexParserUtils
{
    public const string CssMediaTypes = @"@media[^\{\}]*\{";
    /// WARNING: Blocks will include blocks inside at-rules.
    public const string CssBlocks = @"[^\{\}]*\{[^\{\}]*\}";
    public const string CssNumber = @"([0-9]+|[0-9]*\.[0-9]+)";
    public const string CssPercentage = @"([0-9]+|[0-9]*\.[0-9]+)\%";
    public const string CssLength = @"([0-9]+|[0-9]*\.[0-9]+)(em|ex|px|in|cm|mm|pt|pc)";
    public const string CssLineHeight = "(normal|" + CssNumber + "|" + CssLength + "|" + CssPercentage + ")";
    public const string CssFontFamily = "(\"[^\"]*\"|'[^']*'|\\S+\\s*)(\\s*\\,\\s*(\"[^\"]*\"|'[^']*'|\\S+))*";
    public const string CssFontStyle = "(normal|italic|oblique)";
    public const string CssFontVariant = "(normal|small-caps)";
    public const string CssFontWeight = "(normal|bold|bolder|lighter|100|200|300|400|500|600|700|800|900)";
    public const string CssFontSize = "(" + CssLength + "|" + CssPercentage + "|xx-small|x-small|small|medium|large|x-large|xx-large|larger|smaller)";
    public const string CssFontSizeAndLineHeight = CssFontSize + @"(\/" + CssLineHeight + @")?(\s|$)";
    private static readonly ConcurrentDictionary<string, Regex> _regexes = new();

    public static string GetCssAtRules(string stylesheet, ref int startIdx)
    {
        startIdx = stylesheet.IndexOf('@', startIdx);

        if (startIdx <= -1)
            return null;

        int count = 1;
        int endIdx = stylesheet.IndexOf('{', startIdx);

        if (endIdx <= -1)
            return null;

        endIdx++; // to prevent IndexOutOfRangeException at line 113. When '}' is last character in 'stylesheet' variable

        while (count > 0 && endIdx < stylesheet.Length)
        {
            if (stylesheet[endIdx] == '{')
            {
                count++;
            }
            else if (stylesheet[endIdx] == '}')
            {
                count--;
            }
            endIdx++;
        }

        if (endIdx >= stylesheet.Length)
            return null;

        var atrule = stylesheet.Substring(startIdx, endIdx - startIdx + 1);
        startIdx = endIdx;
        return atrule;
    }

    public static MatchCollection Match(string regex, string source)
    {
        var r = GetRegex(regex);
        return r.Matches(source);
    }

    public static string Search(string regex, string source) => Search(regex, source, out int position);

    public static string Search(string regex, string source, out int position)
    {
        MatchCollection matches = Match(regex, source);

        if (matches.Count > 0)
        {
            position = matches[0].Index;
            return matches[0].Value;
        }
        else
        {
            position = -1;
        }

        return null;
    }

    private static Regex GetRegex(string regex) => _regexes.GetOrAdd(regex, r => new Regex(r, RegexOptions.IgnoreCase | RegexOptions.Singleline));
}
