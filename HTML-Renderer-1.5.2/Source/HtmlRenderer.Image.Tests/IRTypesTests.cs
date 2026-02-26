using System.Drawing;
using SkiaSharp;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.Core.IR;
using TheArtOfDev.HtmlRenderer.Image;

namespace HtmlRenderer.Image.Tests;

/// <summary>
/// Tests for the Phase 1 IR (Intermediate Representation) types:
/// <see cref="BoxEdges"/>, <see cref="ComputedStyle"/>, <see cref="Fragment"/>,
/// <see cref="DisplayList"/>, and <see cref="IRasterBackend"/>.
/// Also tests shadow IR building via <see cref="HtmlContainerInt.LatestFragmentTree"/>.
/// </summary>
[Collection("Rendering")]
public class IRTypesTests
{
    // =================================================================
    // BoxEdges
    // =================================================================

    [Fact]
    public void BoxEdges_Zero_HasAllZeroValues()
    {
        var zero = BoxEdges.Zero;
        Assert.Equal(0, zero.Top);
        Assert.Equal(0, zero.Right);
        Assert.Equal(0, zero.Bottom);
        Assert.Equal(0, zero.Left);
    }

    [Fact]
    public void BoxEdges_Constructor_StoresValues()
    {
        var edges = new BoxEdges(1.5, 2.5, 3.5, 4.5);
        Assert.Equal(1.5, edges.Top);
        Assert.Equal(2.5, edges.Right);
        Assert.Equal(3.5, edges.Bottom);
        Assert.Equal(4.5, edges.Left);
    }

    [Fact]
    public void BoxEdges_Equality_SameValues_AreEqual()
    {
        var a = new BoxEdges(1, 2, 3, 4);
        var b = new BoxEdges(1, 2, 3, 4);
        Assert.Equal(a, b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void BoxEdges_Equality_DifferentValues_AreNotEqual()
    {
        var a = new BoxEdges(1, 2, 3, 4);
        var b = new BoxEdges(4, 3, 2, 1);
        Assert.NotEqual(a, b);
    }

    // =================================================================
    // ComputedStyle
    // =================================================================

    [Fact]
    public void ComputedStyle_Defaults_HaveExpectedValues()
    {
        var style = new ComputedStyle();
        Assert.Equal("inline", style.Display);
        Assert.Equal("static", style.Position);
        Assert.Equal("none", style.Float);
        Assert.Equal("none", style.Clear);
        Assert.Equal("visible", style.Overflow);
        Assert.Equal("visible", style.Visibility);
        Assert.Equal("auto", style.Width);
        Assert.Equal("auto", style.Height);
        Assert.Equal("normal", style.FontStyle);
        Assert.Equal("normal", style.FontWeight);
        Assert.Equal("none", style.BackgroundImage);
        Assert.Equal("disc", style.ListStyleType);
        Assert.Equal("1", style.Opacity);
    }

    [Fact]
    public void ComputedStyle_InitProperties_CanBeSet()
    {
        var style = new ComputedStyle
        {
            Display = "block",
            Position = "absolute",
            Float = "left",
            ActualWidth = 100.5,
            ActualHeight = 50.25,
            ActualColor = Color.FromArgb(255, 0, 0),
            FontFamily = "Arial",
        };

        Assert.Equal("block", style.Display);
        Assert.Equal("absolute", style.Position);
        Assert.Equal("left", style.Float);
        Assert.Equal(100.5, style.ActualWidth);
        Assert.Equal(50.25, style.ActualHeight);
        Assert.Equal(Color.FromArgb(255, 0, 0), style.ActualColor);
        Assert.Equal("Arial", style.FontFamily);
    }

    [Fact]
    public void ComputedStyle_BoxEdges_CanBeAssigned()
    {
        var style = new ComputedStyle
        {
            Margin = new BoxEdges(10, 20, 10, 20),
            Border = new BoxEdges(1, 1, 1, 1),
            Padding = new BoxEdges(5, 5, 5, 5),
        };

        Assert.Equal(10, style.Margin.Top);
        Assert.Equal(20, style.Margin.Right);
        Assert.Equal(1, style.Border.Top);
        Assert.Equal(5, style.Padding.Left);
    }

    // =================================================================
    // Fragment
    // =================================================================

    [Fact]
    public void Fragment_Defaults_HaveExpectedValues()
    {
        var fragment = new Fragment();
        Assert.Equal(PointF.Empty, fragment.Location);
        Assert.Equal(SizeF.Empty, fragment.Size);
        Assert.Empty(fragment.Children);
        Assert.Null(fragment.Lines);
        Assert.NotNull(fragment.Style);
        Assert.False(fragment.CreatesStackingContext);
        Assert.Equal(0, fragment.StackLevel);
    }

    [Fact]
    public void Fragment_Bounds_ComputedFromLocationAndSize()
    {
        var fragment = new Fragment
        {
            Location = new PointF(10, 20),
            Size = new SizeF(100, 50),
        };

        Assert.Equal(10, fragment.Bounds.X);
        Assert.Equal(20, fragment.Bounds.Y);
        Assert.Equal(100, fragment.Bounds.Width);
        Assert.Equal(50, fragment.Bounds.Height);
    }

    [Fact]
    public void Fragment_Children_CanBePopulated()
    {
        var child1 = new Fragment { Location = new PointF(0, 0), Size = new SizeF(50, 50) };
        var child2 = new Fragment { Location = new PointF(50, 0), Size = new SizeF(50, 50) };

        var parent = new Fragment
        {
            Location = new PointF(0, 0),
            Size = new SizeF(100, 50),
            Children = [child1, child2],
        };

        Assert.Equal(2, parent.Children.Count);
    }

    // =================================================================
    // LineFragment / InlineFragment
    // =================================================================

    [Fact]
    public void LineFragment_CanHoldInlines()
    {
        var inline1 = new InlineFragment { X = 0, Y = 0, Width = 30, Height = 12, Text = "Hello" };
        var inline2 = new InlineFragment { X = 30, Y = 0, Width = 40, Height = 12, Text = "World" };

        var line = new LineFragment
        {
            X = 0,
            Y = 0,
            Width = 70,
            Height = 12,
            Inlines = [inline1, inline2],
        };

        Assert.Equal(2, line.Inlines.Count);
        Assert.Equal("Hello", line.Inlines[0].Text);
        Assert.Equal("World", line.Inlines[1].Text);
    }

    // =================================================================
    // DisplayList
    // =================================================================

    [Fact]
    public void DisplayList_Empty_HasNoItems()
    {
        var list = new DisplayList();
        Assert.Empty(list.Items);
    }

    [Fact]
    public void DisplayList_CanHoldVariousItems()
    {
        var items = new DisplayItem[]
        {
            new FillRectItem { Bounds = new RectangleF(0, 0, 100, 100), Color = Color.Red },
            new DrawBorderItem
            {
                Bounds = new RectangleF(0, 0, 100, 100),
                Widths = new BoxEdges(1, 1, 1, 1),
                TopColor = Color.Black,
                RightColor = Color.Black,
                BottomColor = Color.Black,
                LeftColor = Color.Black,
            },
            new DrawTextItem
            {
                Text = "Hello",
                FontFamily = "Arial",
                FontSize = 12,
                Color = Color.Black,
                Origin = new PointF(10, 10),
            },
            new ClipItem { ClipRect = new RectangleF(0, 0, 200, 200) },
            new RestoreItem(),
            new OpacityItem { Opacity = 0.5f },
        };

        var list = new DisplayList { Items = items };
        Assert.Equal(6, list.Items.Count);
        Assert.IsType<FillRectItem>(list.Items[0]);
        Assert.IsType<DrawBorderItem>(list.Items[1]);
        Assert.IsType<DrawTextItem>(list.Items[2]);
        Assert.IsType<ClipItem>(list.Items[3]);
        Assert.IsType<RestoreItem>(list.Items[4]);
        Assert.IsType<OpacityItem>(list.Items[5]);
    }

    [Fact]
    public void DrawImageItem_CanHoldImageReference()
    {
        var item = new DrawImageItem
        {
            Bounds = new RectangleF(0, 0, 100, 100),
            DestRect = new RectangleF(0, 0, 100, 100),
            SourceRect = new RectangleF(0, 0, 50, 50),
            ImageHandle = null,
        };

        Assert.Null(item.ImageHandle);
        Assert.Equal(100, item.DestRect.Width);
    }

    // =================================================================
    // Shadow IR building (integration test via HtmlContainer)
    // =================================================================

    [Fact]
    public void PerformLayout_BuildsShadowFragmentTree()
    {
        using var container = new HtmlContainer();
        container.AvoidAsyncImagesLoading = true;
        container.AvoidImagesLateLoading = true;
        container.SetHtml("<div style='width:200px;height:100px;'>Hello</div>");

        using var bitmap = new SKBitmap(500, 500);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.White);

        var clip = new RectangleF(0, 0, 500, 500);
        container.PerformLayout(canvas, clip);

        var fragmentTree = container.HtmlContainerInt.LatestFragmentTree;
        Assert.NotNull(fragmentTree);
    }

    [Fact]
    public void PerformLayout_FragmentTree_HasChildren()
    {
        using var container = new HtmlContainer();
        container.AvoidAsyncImagesLoading = true;
        container.AvoidImagesLateLoading = true;
        container.SetHtml(
            @"<div style='width:200px;'>
                <p>First</p>
                <p>Second</p>
            </div>");

        using var bitmap = new SKBitmap(500, 500);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.White);

        var clip = new RectangleF(0, 0, 500, 500);
        container.PerformLayout(canvas, clip);

        var fragmentTree = container.HtmlContainerInt.LatestFragmentTree;
        Assert.NotNull(fragmentTree);
        Assert.True(fragmentTree.Children.Count > 0, "Root fragment should have children");
    }

    [Fact]
    public void PerformLayout_FragmentTree_CapturesComputedStyle()
    {
        using var container = new HtmlContainer();
        container.AvoidAsyncImagesLoading = true;
        container.AvoidImagesLateLoading = true;
        container.SetHtml("<div style='display:block;width:200px;'>Test</div>");

        using var bitmap = new SKBitmap(500, 500);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.White);

        var clip = new RectangleF(0, 0, 500, 500);
        container.PerformLayout(canvas, clip);

        var fragmentTree = container.HtmlContainerInt.LatestFragmentTree;
        Assert.NotNull(fragmentTree);
        Assert.NotNull(fragmentTree.Style);
        Assert.Equal("block", fragmentTree.Style.Display);
    }

    [Fact]
    public void PerformLayout_FragmentTree_HasPositiveSize()
    {
        using var container = new HtmlContainer();
        container.AvoidAsyncImagesLoading = true;
        container.AvoidImagesLateLoading = true;
        container.SetHtml("<div style='width:200px;height:100px;'>Content</div>");

        using var bitmap = new SKBitmap(500, 500);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.White);

        var clip = new RectangleF(0, 0, 500, 500);
        container.PerformLayout(canvas, clip);

        var fragmentTree = container.HtmlContainerInt.LatestFragmentTree;
        Assert.NotNull(fragmentTree);
        Assert.True(fragmentTree.Size.Width > 0, "Fragment width should be positive");
        Assert.True(fragmentTree.Size.Height > 0, "Fragment height should be positive");
    }

    [Fact]
    public void PerformLayout_FragmentTree_StyleHasBorderEdges()
    {
        using var container = new HtmlContainer();
        container.AvoidAsyncImagesLoading = true;
        container.AvoidImagesLateLoading = true;
        container.SetHtml("<div style='width:200px;padding:10px;border:2px solid black;'>Styled</div>");

        using var bitmap = new SKBitmap(500, 500);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.White);

        var clip = new RectangleF(0, 0, 500, 500);
        container.PerformLayout(canvas, clip);

        var fragmentTree = container.HtmlContainerInt.LatestFragmentTree;
        Assert.NotNull(fragmentTree);

        // The styled div is a child of the root; find the fragment with padding/border
        var styledFragment = FindFragmentWithPadding(fragmentTree);
        Assert.NotNull(styledFragment);
        Assert.True(styledFragment.Style.Padding.Top >= 10, "Padding top should be >= 10");
        Assert.True(styledFragment.Style.Border.Top >= 2, "Border top should be >= 2");
    }

    private static Fragment? FindFragmentWithPadding(Fragment fragment)
    {
        if (fragment.Style.Padding.Top >= 10)
            return fragment;

        foreach (var child in fragment.Children)
        {
            var found = FindFragmentWithPadding(child);
            if (found != null)
                return found;
        }

        return null;
    }

    [Fact]
    public void PerformLayout_ExistingBehavior_Unchanged()
    {
        // Verify that adding shadow IR building doesn't change actual rendered output
        using var container = new HtmlContainer();
        container.AvoidAsyncImagesLoading = true;
        container.AvoidImagesLateLoading = true;
        container.SetHtml("<div style='width:200px;'>Hello World</div>");

        using var bitmap = new SKBitmap(500, 500);
        using var canvas = new SKCanvas(bitmap);
        canvas.Clear(SKColors.White);

        var clip = new RectangleF(0, 0, 500, 500);
        container.PerformLayout(canvas, clip);

        // Layout should still produce correct sizes
        Assert.True(container.ActualSize.Width > 0);
        Assert.True(container.ActualSize.Height > 0);

        // Paint should still work
        container.PerformPaint(canvas, clip);

        // Verify the bitmap has non-white pixels (text was rendered)
        bool hasNonWhitePixels = false;
        for (int y = 0; y < bitmap.Height && !hasNonWhitePixels; y++)
        {
            for (int x = 0; x < bitmap.Width; x++)
            {
                var pixel = bitmap.GetPixel(x, y);
                if (pixel != SKColors.White)
                {
                    hasNonWhitePixels = true;
                    break;
                }
            }
        }

        Assert.True(hasNonWhitePixels, "Rendering should produce visible output");
    }
}
