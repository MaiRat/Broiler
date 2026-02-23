using System.Windows;
using TheArtOfDev.HtmlRenderer.Core.Utils;

namespace TheArtOfDev.HtmlRenderer.WPF;

public delegate void RoutedEventHandler<T>(object sender, RoutedEventArgs<T> args) where T : class;

public sealed class RoutedEventArgs<T> : RoutedEventArgs where T : class
{
    public RoutedEventArgs(RoutedEvent routedEvent, T data) : base(routedEvent)
    {
        ArgChecker.AssertArgNotNull(data, "args");
        Data = data;
    }

    public RoutedEventArgs(RoutedEvent routedEvent, object source, T data) : base(routedEvent, source)
    {
        ArgChecker.AssertArgNotNull(data, "args");
        Data = data;
    }

    public T Data { get; }

    public override string ToString() => $"RoutedEventArgs({Data})";
}