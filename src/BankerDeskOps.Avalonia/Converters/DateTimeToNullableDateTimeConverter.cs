using System.Globalization;
using Avalonia.Data.Converters;

namespace BankerDeskOps.Avalonia.Converters;

/// <summary>
/// Converts between <see cref="DateTime"/> (ViewModel) and
/// <see cref="DateTime?"/> (Avalonia CalendarDatePicker.SelectedDate).
/// </summary>
public sealed class DateTimeToNullableDateTimeConverter : IValueConverter
{
    public static readonly DateTimeToNullableDateTimeConverter Instance = new();

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTime dt && dt != default)
            return (DateTime?)dt;

        return (DateTime?)null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is DateTime dt)
            return dt;

        // Fall back to a sensible default rather than DateTime.MinValue
        return new DateTime(1990, 1, 1);
    }
}
