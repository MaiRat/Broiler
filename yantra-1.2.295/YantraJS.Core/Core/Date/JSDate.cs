using System;
using Yantra.Core;

namespace YantraJS.Core;

// [JSRuntime(typeof(JSDateStatic), typeof(JSDatePrototype))]
[JSFunctionGenerator("Date")]
public partial class JSDate: JSObject
{

    internal static readonly DateTimeOffset InvalidDate = DateTimeOffset.MinValue;

    internal static readonly JSDate invalidDate = new(DateTimeOffset.MinValue);

    internal static TimeSpan Local => TimeZoneInfo.Local.BaseUtcOffset;
    //internal static TimeSpan Local {
    //    get {
    //        return TimeZoneInfo.Local.BaseUtcOffset;
    //    }
    //}

    internal DateTimeOffset value;

    public DateTimeOffset Value
    {
        get => value;
        set => this.value = value;
    }

    public DateTime DateTime => Value.DateTime;

    internal JSDate(JSObject prototype, DateTimeOffset time) : base(prototype) => value = time;


    public JSDate(DateTimeOffset time) : this() => value = time;

    //public override string ToString()
    //{
    //    return this.value.ToString();
    //}

    public override string ToDetailString() => value.ToString();

    public override bool ConvertTo(Type type, out object value)
    {
        if (type == typeof(DateTime))
        {
            value = this.value.LocalDateTime;
            return true;
        }
        if (type == typeof(DateTimeOffset))
        {
            value = this.value;
            return true;
        }
        if (type.IsAssignableFrom(typeof(JSDate)))
        {
            value = this;
            return true;
        }
        if (type == typeof(object))
        {
            value = this.value;
            return true;
        }
        return base.ConvertTo(type, out value);
    }

}
