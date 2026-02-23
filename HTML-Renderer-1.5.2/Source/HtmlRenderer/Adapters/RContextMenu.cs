using System;
using TheArtOfDev.HtmlRenderer.Adapters.Entities;

namespace TheArtOfDev.HtmlRenderer.Adapters;

public abstract class RContextMenu : IDisposable
{
    public abstract int ItemsCount { get; }
    public abstract void AddDivider();
    public abstract void AddItem(string text, bool enabled, EventHandler onClick);
    public abstract void RemoveLastDivider();
    public abstract void Show(RControl parent, RPoint location);
    public abstract void Dispose();
}