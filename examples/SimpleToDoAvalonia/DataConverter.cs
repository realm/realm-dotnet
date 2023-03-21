using System;
using Avalonia.Data.Converters;

namespace SimpleToDo;

public static class DataConverter
{
    public static readonly IValueConverter ToShortTimeString =
        new FuncValueConverter<DateTimeOffset, string>(offset => offset.DateTime.ToShortTimeString());
}