using System;
using System.IO;

namespace TheArtOfDev.HtmlRenderer.Core.Utils;

public static class ArgChecker
{
    public static void AssertIsTrue<TException>(bool condition, string message) where TException : Exception, new()
    {
        // Checks whether the condition is false
        if (!condition)
        {
            // Throwing exception
            throw (TException)Activator.CreateInstance(typeof(TException), message);
        }
    }

    public static void AssertArgNotNull(object arg, string argName)
    {
        if (arg == null)
            throw new ArgumentNullException(argName);
    }

    public static void AssertArgNotNull(IntPtr arg, string argName)
    {
        if (arg == IntPtr.Zero)
            throw new ArgumentException("IntPtr argument cannot be Zero", argName);
    }

    public static void AssertArgNotNullOrEmpty(string arg, string argName)
    {
        if (string.IsNullOrEmpty(arg))
            throw new ArgumentNullException(argName);
    }

    public static T AssertArgOfType<T>(object arg, string argName)
    {
        AssertArgNotNull(arg, argName);

        if (arg is T t)
            return t;

        throw new ArgumentException($"Given argument isn't of type '{typeof(T).Name}'.", argName);
    }

    public static void AssertFileExist(string arg, string argName)
    {
        AssertArgNotNullOrEmpty(arg, argName);

        if (false == File.Exists(arg))
            throw new FileNotFoundException($"Given file in argument '{argName}' not exist.", arg);
    }
}