using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using Microsoft.Maui.Graphics;


namespace ScanAndSave.Converters
{
    /// <summary>
    /// Konverterer DateTime? til læsbar dato-streng.
    /// Viser "Ingen udløbsdato" hvis null.
    /// </summary>
    public class DateToStringConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is DateTime date)
            {
                var daysUntil = (date - DateTime.Now).Days;
                if (daysUntil < 0)
                    return $"Udløbet ({date:dd/MM/yyyy})";
                if (daysUntil == 0)
                    return "Udløber i dag!";
                if (daysUntil <= 3)
                    return $"Udløber om {daysUntil} dage";
                return date.ToString("dd/MM/yyyy");
            }
            return "Ingen udløbsdato";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    /// <summary>
    /// Konverterer DateTime? til farve baseret på udløbsstatus.
    /// Rød = udløbet, Orange = snart, Grå = OK/ingen dato.
    /// </summary>
    public class ExpiryToColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is DateTime date)
            {
                var daysUntil = (date - DateTime.Now).Days;
                if (daysUntil < 0)
                    return Color.FromArgb("#EF4444");   // Rød - udløbet
                if (daysUntil <= 3)
                    return Color.FromArgb("#F59E0B");   // Orange - snart
                return Color.FromArgb("#64748B");       // Grå - OK
            }
            return Color.FromArgb("#94A3B8");           // Lys grå - ingen dato
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    /// <summary>
    /// Konverterer bool til synlighed (true = Visible, false = Collapsed)
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return !string.IsNullOrWhiteSpace(value as string);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    /// <summary>
    /// Inverterer en bool (true -> false, false -> true)
    /// </summary>
    public class InverseBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return string.IsNullOrWhiteSpace(value as string);
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

    /// <summary>
    /// Konverterer string til bool (tom streng = false, ellers true)
    /// </summary>
    public class StringToBoolConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return !string.IsNullOrWhiteSpace(value as string);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
