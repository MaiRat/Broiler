using System.Collections.Generic;

namespace Broiler.App.Rendering
{
    /// <summary>
    /// Abstraction over a JavaScript execution engine.
    /// </summary>
    public interface IScriptEngine
    {
        /// <summary>
        /// Execute the supplied <paramref name="scripts"/> in a fresh context.
        /// Returns <c>true</c> when all scripts executed without error.
        /// </summary>
        bool Execute(IReadOnlyList<string> scripts);
    }
}
