using System;
using System.Collections.Generic;
using TheArtOfDev.HtmlRenderer.Core.Dom;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.Core.Parse;

internal static class HtmlParser
{
    public static CssBox ParseDocument(string source)
    {
        var root = CssBoxHelper.CreateBlock();
        var curBox = root;

        int endIdx = 0;
        int startIdx = 0;

        while (startIdx >= 0)
        {
            var tagIdx = source.IndexOf('<', startIdx);
            if (tagIdx >= 0 && tagIdx < source.Length)
            {
                // add the html text as anon css box to the structure
                AddTextBox(source, startIdx, tagIdx, ref curBox);

                if (source[tagIdx + 1] == '!')
                {
                    if (source[tagIdx + 2] == '-')
                    {
                        // skip the html comment elements (<!-- bla -->)
                        startIdx = source.IndexOf("-->", tagIdx + 2);
                        endIdx = startIdx > 0 ? startIdx + 3 : tagIdx + 2;
                    }
                    else
                    {
                        // skip the html crap elements (<!crap bla>)
                        startIdx = source.IndexOf(">", tagIdx + 2);
                        endIdx = startIdx > 0 ? startIdx + 1 : tagIdx + 2;
                    }
                }
                else
                {
                    // parse element tag to css box structure
                    endIdx = ParseHtmlTag(source, tagIdx, ref curBox) + 1;

                    if (curBox.HtmlTag != null && curBox.HtmlTag.Name.Equals(HtmlConstants.Style, StringComparison.OrdinalIgnoreCase))
                    {
                        var endIdxS = endIdx;
                        endIdx = source.IndexOf("</style>", endIdx, StringComparison.OrdinalIgnoreCase);
                        if (endIdx > -1)
                            AddTextBox(source, endIdxS, endIdx, ref curBox);
                    }
                }
            }

            startIdx = tagIdx > -1 && endIdx > 0 ? endIdx : -1;
        }

        // handle pieces of html without proper structure
        if (endIdx > -1 && endIdx < source.Length)
        {
            // there is text after the end of last element
            var endText = source.AsMemory(endIdx, source.Length - endIdx);
            if (!endText.Span.IsWhiteSpace())
            {
                var abox = CssBoxHelper.CreateBox(root);
                abox.Text = endText;
            }
        }

        return root;
    }

    private static void AddTextBox(string source, int startIdx, int tagIdx, ref CssBox curBox)
    {
        if (tagIdx <= startIdx)
            return;

        var abox = CssBoxHelper.CreateBox(curBox);
        abox.Text = source.AsMemory(startIdx, tagIdx - startIdx);
    }

    private static int ParseHtmlTag(string source, int tagIdx, ref CssBox curBox)
    {
        var endIdx = source.IndexOf('>', tagIdx + 1);
        if (endIdx <= 0)
            return endIdx;

        var length = endIdx - tagIdx + 1 - (source[endIdx - 1] == '/' ? 1 : 0);
        if (ParseHtmlTag(source, tagIdx, length, out string tagName, out Dictionary<string, string> tagAttributes))
        {
            if (!HtmlUtils.IsSingleTag(tagName) && curBox.ParentBox != null)
            {
                // need to find the parent tag to go one level up
                curBox = DomUtils.FindParent(curBox.ParentBox, tagName, curBox);
            }
        }
        else if (!string.IsNullOrEmpty(tagName))
        {
            //new SubString(source, lastEnd + 1, tagmatch.Index - lastEnd - 1)
            var isSingle = HtmlUtils.IsSingleTag(tagName) || source[endIdx - 1] == '/';
            var tag = new HtmlTag(tagName, isSingle, tagAttributes);

            if (isSingle)
            {
                // the current box is not changed
                CssBoxHelper.CreateBox(tag, curBox);
            }
            else
            {
                // go one level down, make the new box the current box
                curBox = CssBoxHelper.CreateBox(tag, curBox);
            }
        }
        else
        {
            endIdx = tagIdx + 1;
        }

        return endIdx;
    }

    private static bool ParseHtmlTag(string source, int idx, int length, out string name, out Dictionary<string, string> attributes)
    {
        idx++;
        length = length - (source[idx + length - 3] == '/' ? 3 : 2);

        // Check if is end tag
        var isClosing = false;
        if (source[idx] == '/')
        {
            idx++;
            length--;
            isClosing = true;
        }

        int spaceIdx = idx;
        while (spaceIdx < idx + length && !char.IsWhiteSpace(source, spaceIdx))
            spaceIdx++;

        // Get the name of the tag
        name = source.Substring(idx, spaceIdx - idx).ToLower();

        attributes = null;
        if (!isClosing && idx + length > spaceIdx)
            ExtractAttributes(source, spaceIdx, length - (spaceIdx - idx), out attributes);

        return isClosing;
    }

    private static void ExtractAttributes(string source, int idx, int length, out Dictionary<string, string> attributes)
    {
        attributes = null;

        int startIdx = idx;
        while (startIdx < idx + length)
        {
            while (startIdx < idx + length && char.IsWhiteSpace(source, startIdx))
                startIdx++;

            var endIdx = startIdx + 1;
            while (endIdx < idx + length && !char.IsWhiteSpace(source, endIdx) && source[endIdx] != '=')
                endIdx++;

            if (startIdx < idx + length)
            {
                var key = source.Substring(startIdx, endIdx - startIdx);
                var value = "";

                startIdx = endIdx + 1;
                while (startIdx < idx + length && (char.IsWhiteSpace(source, startIdx) || source[startIdx] == '='))
                    startIdx++;

                bool hasPChar = false;
                if (startIdx < idx + length)
                {
                    char pChar = source[startIdx];
                    if (pChar == '"' || pChar == '\'')
                    {
                        hasPChar = true;
                        startIdx++;
                    }

                    endIdx = startIdx + (hasPChar ? 0 : 1);
                    while (endIdx < idx + length && (hasPChar ? source[endIdx] != pChar : !char.IsWhiteSpace(source, endIdx)))
                        endIdx++;

                    value = source.Substring(startIdx, endIdx - startIdx);
                    value = HtmlUtils.DecodeHtml(value);
                }

                if (key.Length != 0)
                {
                    attributes ??= new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
                    attributes[key.ToLower()] = value;
                }

                startIdx = endIdx + (hasPChar ? 2 : 1);
            }
        }
    }
}