using System.Collections.Generic;
using TheArtOfDev.HtmlRenderer.Core.IR;

namespace HtmlRenderer.Image.Tests;

/// <summary>
/// Reusable assertion helper that walks a <see cref="Fragment"/> tree and
/// checks structural layout invariants. Phase 2 deliverable.
/// Delegates to <see cref="FragmentInvariantChecker"/> for the actual checks.
/// </summary>
public static class LayoutInvariantChecker
{
    /// <summary>
    /// Checks all layout invariants on the given fragment tree.
    /// Returns a list of violation descriptions. An empty list means the tree is valid.
    /// </summary>
    public static IReadOnlyList<string> Check(Fragment root) =>
        FragmentInvariantChecker.Check(root);

    /// <summary>
    /// Asserts that the fragment tree satisfies all layout invariants.
    /// Throws <see cref="Xunit.Sdk.XunitException"/> if any violations are found.
    /// </summary>
    public static void AssertValid(Fragment root)
    {
        var violations = Check(root);
        if (violations.Count > 0)
        {
            var message = $"Layout invariant violations ({violations.Count}):\n"
                + string.Join("\n", violations);
            throw new Xunit.Sdk.XunitException(message);
        }
    }
}
