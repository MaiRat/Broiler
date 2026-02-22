using YantraJS.Core;

namespace Broiler.Cli.Tests;

/// <summary>
/// Tests for the window/document stubs registered by
/// <see cref="CaptureService.RegisterWindowStub"/> to prevent
/// JSException when scripts access browser globals.
/// </summary>
public class WindowStubTests
{
    [Fact]
    public void RegisterWindowStub_WindowIsDefined()
    {
        using var context = new JSContext();
        CaptureService.RegisterWindowStub(context);

        var result = context.Eval("typeof window !== 'undefined'");
        Assert.True(result.BooleanValue);
    }

    [Fact]
    public void RegisterWindowStub_LocalStorageIsDefined()
    {
        using var context = new JSContext();
        CaptureService.RegisterWindowStub(context);

        var result = context.Eval("typeof window.localStorage !== 'undefined'");
        Assert.True(result.BooleanValue);
    }

    [Fact]
    public void RegisterWindowStub_LocalStorageBracketAccessReturnsUndefined()
    {
        using var context = new JSContext();
        CaptureService.RegisterWindowStub(context);

        var result = context.Eval("window.localStorage['nonexistent'] === undefined");
        Assert.True(result.BooleanValue);
    }

    [Fact]
    public void RegisterWindowStub_LocalStorageGetItemReturnsNull()
    {
        using var context = new JSContext();
        CaptureService.RegisterWindowStub(context);

        var result = context.Eval("window.localStorage.getItem('missing') === null");
        Assert.True(result.BooleanValue);
    }

    [Fact]
    public void RegisterWindowStub_MatchMediaReturnsFalseMatches()
    {
        using var context = new JSContext();
        CaptureService.RegisterWindowStub(context);

        var result = context.Eval("window.matchMedia('(prefers-color-scheme: dark)').matches === false");
        Assert.True(result.BooleanValue);
    }

    [Fact]
    public void RegisterWindowStub_DocumentElementClassListAdd()
    {
        using var context = new JSContext();
        CaptureService.RegisterWindowStub(context);

        var result = context.Eval(@"
            document.documentElement.classList.add('dark');
            document.documentElement.classList.contains('dark')");
        Assert.True(result.BooleanValue);
    }

    /// <summary>
    /// Regression test: the exact heise.de script that caused
    /// JSException: 'Cannot get property localStorage of undefined'.
    /// </summary>
    [Fact]
    public void RegisterWindowStub_HeiseColorSchemeScript_DoesNotThrow()
    {
        using var context = new JSContext();
        CaptureService.RegisterWindowStub(context);

        var ex = Record.Exception(() => context.Eval(@"
            var config = JSON.parse(window.localStorage['akwaConfig-v2'] || '{}')
            var scheme = config.colorScheme ? config.colorScheme.scheme : 'auto'
            if (scheme === 'dark' || (scheme === 'auto' && window.matchMedia('(prefers-color-scheme: dark)').matches)) {
              document.documentElement.classList.add('dark')
            }
        "));

        Assert.Null(ex);
    }
}
