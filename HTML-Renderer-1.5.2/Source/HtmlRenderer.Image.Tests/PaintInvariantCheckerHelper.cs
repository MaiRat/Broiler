using System.Collections.Generic;
using TheArtOfDev.HtmlRenderer.Core.IR;

namespace HtmlRenderer.Image.Tests;

/// <summary>
/// Reusable assertion helper that checks a <see cref="DisplayList"/>
/// against paint-level invariants. Phase 4 deliverable.
/// Delegates to <see cref="PaintInvariantChecker"/> for the actual checks.
/// </summary>
public static class PaintInvariantCheckerHelper
{
    /// <summary>
    /// Checks all paint invariants on the given display list.
    /// Returns a list of violation descriptions. An empty list means the list is valid.
    /// </summary>
    public static IReadOnlyList<string> Check(DisplayList displayList) =>
        PaintInvariantChecker.Check(displayList);

    /// <summary>
    /// Asserts that the display list satisfies all paint invariants.
    /// Throws <see cref="Xunit.Sdk.XunitException"/> if any violations are found.
    /// </summary>
    public static void AssertValid(DisplayList displayList)
    {
        var violations = Check(displayList);
        if (violations.Count > 0)
        {
            var message = $"Paint invariant violations ({violations.Count}):\n"
                + string.Join("\n", violations);
            throw new Xunit.Sdk.XunitException(message);
        }
    }
}
