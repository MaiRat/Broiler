using System.Windows.Media;
using TheArtOfDev.HtmlRenderer.Adapters;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace TheArtOfDev.HtmlRenderer.WPF.Adapters;

internal sealed class PenAdapter(Brush brush) : RPen
{
    private double _width;
    private DashStyle _dashStyle = DashStyles.Solid;

    public override double Width
    {
        get { return _width; }
        set { _width = value; }
    }

    public override RDashStyle DashStyle
    {
        set
        {
            _dashStyle = value switch
            {
                RDashStyle.Solid => DashStyles.Solid,
                RDashStyle.Dash => DashStyles.Dash,
                RDashStyle.Dot => DashStyles.Dot,
                RDashStyle.DashDot => DashStyles.DashDot,
                RDashStyle.DashDotDot => DashStyles.DashDotDot,
                _ => DashStyles.Solid,
            };
        }
    }

    public Pen CreatePen()
    {
        var pen = new Pen(brush, _width) { DashStyle = _dashStyle };
        return pen;
    }
}