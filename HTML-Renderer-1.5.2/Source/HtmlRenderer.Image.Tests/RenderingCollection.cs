using Xunit;

namespace HtmlRenderer.Image.Tests;

/// <summary>
/// Defines a shared test collection with a rendering fixture.
/// All rendering is done once in the fixture, and tests verify the results.
/// </summary>
[CollectionDefinition("Rendering")]
public class RenderingCollection : ICollectionFixture<RenderingFixture>
{
}
