using Broiler.App.Rendering;

namespace Broiler.App.Tests;

public class CssBoxModelTests
{
    [Fact]
    public void BuildLayoutTree_BlockChildren_StackedVertically()
    {
        var root = new DomElement("div", null, null, string.Empty);
        var child1 = new DomElement("div", null, null, string.Empty);
        var child2 = new DomElement("div", null, null, string.Empty);
        root.Children.Add(child1); child1.Parent = root;
        root.Children.Add(child2); child2.Parent = root;

        var model = new CssBoxModel();
        var box = model.BuildLayoutTree(root, 800f);

        Assert.Equal(2, box.Children.Count);
        Assert.True(box.Children[1].Dimensions.Y > box.Children[0].Dimensions.Y,
            "Second block child should be below the first");
    }

    [Fact]
    public void ResolveDisplay_FromStyleDictionary()
    {
        var el = new DomElement("span", null, null, string.Empty,
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "display", "block" }
            });

        var model = new CssBoxModel();
        var box = model.BuildLayoutTree(el, 800f);

        Assert.Equal(CssDisplay.Block, box.Display);
    }

    [Fact]
    public void ResolvePosition_FromStyleDictionary()
    {
        var el = new DomElement("div", null, null, string.Empty,
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "position", "absolute" }
            });

        var model = new CssBoxModel();
        var box = model.BuildLayoutTree(el, 800f);

        Assert.Equal(CssPosition.Absolute, box.Position);
    }

    [Fact]
    public void ResolveFloat_FromStyleDictionary()
    {
        var el = new DomElement("div", null, null, string.Empty,
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "float", "left" }
            });

        var model = new CssBoxModel();
        var box = model.BuildLayoutTree(el, 800f);

        Assert.Equal(CssFloat.Left, box.Float);
    }

    [Fact]
    public void ResolveClear_FromStyleDictionary()
    {
        var el = new DomElement("div", null, null, string.Empty,
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "clear", "both" }
            });

        var model = new CssBoxModel();
        var box = model.BuildLayoutTree(el, 800f);

        Assert.Equal(CssClear.Both, box.Clear);
    }

    [Fact]
    public void ParseCssValue_PixelValue_ReturnsParsedFloat()
    {
        var result = CssBoxModel.ParseCssValue("10px", 800f, 0f);

        Assert.Equal(10f, result);
    }

    [Fact]
    public void ParseCssValue_PercentageValue_ResolvesAgainstContainer()
    {
        var result = CssBoxModel.ParseCssValue("50%", 800f, 0f);

        Assert.Equal(400f, result);
    }

    [Fact]
    public void PaddingBox_CalculatesCorrectRect()
    {
        var dims = new BoxDimensions
        {
            X = 100f, Y = 50f, Width = 200f, Height = 100f,
            Padding = new BoxEdges { Top = 10f, Right = 20f, Bottom = 10f, Left = 20f }
        };

        var rect = dims.PaddingBox();

        Assert.Equal(80f, rect.X);    // 100 - 20
        Assert.Equal(40f, rect.Y);    // 50 - 10
        Assert.Equal(240f, rect.Width);  // 200 + 20 + 20
        Assert.Equal(120f, rect.Height); // 100 + 10 + 10
    }

    [Fact]
    public void BorderBox_CalculatesCorrectRect()
    {
        var dims = new BoxDimensions
        {
            X = 100f, Y = 50f, Width = 200f, Height = 100f,
            Padding = new BoxEdges { Top = 10f, Right = 10f, Bottom = 10f, Left = 10f },
            Border = new BoxEdges { Top = 2f, Right = 2f, Bottom = 2f, Left = 2f }
        };

        var rect = dims.BorderBox();

        Assert.Equal(88f, rect.X);     // 100 - 10 - 2
        Assert.Equal(38f, rect.Y);     // 50 - 10 - 2
        Assert.Equal(224f, rect.Width);  // 200 + 10+10 + 2+2
        Assert.Equal(124f, rect.Height); // 100 + 10+10 + 2+2
    }

    [Fact]
    public void MarginBox_CalculatesCorrectRect()
    {
        var dims = new BoxDimensions
        {
            X = 100f, Y = 50f, Width = 200f, Height = 100f,
            Padding = new BoxEdges { Top = 5f, Right = 5f, Bottom = 5f, Left = 5f },
            Border = new BoxEdges { Top = 1f, Right = 1f, Bottom = 1f, Left = 1f },
            Margin = new BoxEdges { Top = 10f, Right = 10f, Bottom = 10f, Left = 10f }
        };

        var rect = dims.MarginBox();

        Assert.Equal(84f, rect.X);      // 100 - 5 - 1 - 10
        Assert.Equal(34f, rect.Y);      // 50 - 5 - 1 - 10
        Assert.Equal(232f, rect.Width);  // 200 + (5+5) + (1+1) + (10+10)
        Assert.Equal(132f, rect.Height); // 100 + (5+5) + (1+1) + (10+10)
    }
}
