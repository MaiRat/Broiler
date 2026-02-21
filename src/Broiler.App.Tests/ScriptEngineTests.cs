using Broiler.App.Rendering;

namespace Broiler.App.Tests;

public class ScriptEngineTests
{
    private readonly ScriptEngine _engine = new();

    [Fact]
    public void Execute_EmptyList_ReturnsTrue()
    {
        var result = _engine.Execute(Array.Empty<string>());
        Assert.True(result);
    }

    [Fact]
    public void Execute_ValidScript_ReturnsTrue()
    {
        var result = _engine.Execute(new[] { "var x = 1 + 2;" });
        Assert.True(result);
    }

    [Fact]
    public void Execute_InvalidScript_ReturnsFalse()
    {
        var result = _engine.Execute(new[] { "this is not valid javascript @@!!" });
        Assert.False(result);
    }

    [Fact]
    public void Execute_MultipleValidScripts_ReturnsTrue()
    {
        var result = _engine.Execute(new[] { "var a = 1;", "var b = a + 1;" });
        Assert.True(result);
    }
}
