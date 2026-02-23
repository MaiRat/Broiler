using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using Expression = YantraJS.Expressions.YExpression;

namespace YantraJS.Core;

public class JSVariable
{
    public JSValue Value;

    static readonly FieldInfo _ValueField =
        typeof(JSVariable).GetField("Value");
    internal readonly StringSpan Name;
    private KeyString key;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public JSVariable(JSValue v, string name)
    {
        Value = v;
        Name = name;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public JSVariable(JSValue v, in StringSpan name)
    {
        Value = v;
        Name = name;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public JSVariable(in Arguments a, int i, string name)
    {
        Value = a.GetAt(i);
        Name = name;
    }

    public JSValue GlobalValue
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set
        {
            Value = value;
            if (key.Value == null)
            {
                key = KeyStrings.GetOrCreate(Name);
            }
            var old = JSContext.Current[key];
            if (old != value && !value.IsUndefined)
            {
                JSContext.Current[key] = value;
            }
        }
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public JSVariable(Exception e, string name)
        : this(e is JSException je 
              ? je.Error
              : JSException.From(e).Error , name)
    {

    }

    //[MethodImpl(MethodImplOptions.AggressiveInlining)]
    //internal static JSVariable New(in Arguments a, int i, string name)
    //{
    //    return new JSVariable(a.GetAt(i), name);
    //}

    internal static Expression ValueExpression(Expression exp) => Expression.Field(exp, _ValueField);

}
