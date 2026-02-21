using System;
using System.Collections.Generic;
using System.Diagnostics;
using YantraJS.Core;

namespace Broiler.App.Rendering
{
    /// <summary>
    /// Executes JavaScript using the YantraJS engine.
    /// A fresh <see cref="JSContext"/> is created for each call to
    /// <see cref="Execute(IReadOnlyList{string})"/> so that scripts from different pages are isolated.
    /// </summary>
    public sealed class ScriptEngine : IScriptEngine
    {
        /// <inheritdoc />
        public bool Execute(IReadOnlyList<string> scripts)
        {
            if (scripts.Count == 0)
                return true;

            try
            {
                using var context = new JSContext();
                foreach (var script in scripts)
                {
                    context.Eval(script);
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"JavaScript execution error: {ex.Message}");
                return false;
            }
        }

        /// <inheritdoc />
        public bool Execute(IReadOnlyList<string> scripts, string html)
        {
            if (scripts.Count == 0)
                return true;

            try
            {
                using var context = new JSContext();
                var bridge = new DomBridge();
                bridge.Attach(context, html);

                foreach (var script in scripts)
                {
                    context.Eval(script);
                }
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"JavaScript execution error: {ex.Message}");
                return false;
            }
        }
    }
}
