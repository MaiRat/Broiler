using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace TheArtOfDev.HtmlRenderer.Adapters;

public abstract class RPen
{
    public abstract double Width { get; set; }
    public abstract RDashStyle DashStyle { set; }
}