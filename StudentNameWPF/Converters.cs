using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Collections.ObjectModel;
using FUMiniHotelSystem.Models;
using StudentNameWPF.Models;

namespace StudentNameWPF
{
    public class StringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string currentView && parameter is string targetView)
            {
                return currentView == targetView ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TotalRevenueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ObservableCollection<BookingDisplayModel> bookings)
            {
                var total = bookings.Sum(b => b.TotalAmount);
                return $"${total:0}";
            }
            return "$0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BooleanToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && parameter is string param)
            {
                var parts = param.Split('|');
                if (parts.Length == 2)
                {
                    return boolValue ? parts[0] : parts[1];
                }
            }
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                // If parameter is "Invert", invert the boolean
                if (parameter is string param && param == "Invert")
                {
                    boolValue = !boolValue;
                }
                
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class RoomTypeConverter : IValueConverter
    {
        private static readonly Dictionary<int, string> RoomTypeNames = new Dictionary<int, string>
        {
            { 1, "Standard Single" },
            { 2, "Standard Double" },
            { 3, "Deluxe Suite" },
            { 4, "Family Room" },
            { 5, "Executive Suite" },
            { 6, "Presidential Suite" },
            { 7, "Ocean View" },
            { 8, "Garden View" }
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int roomTypeId && RoomTypeNames.ContainsKey(roomTypeId))
            {
                return RoomTypeNames[roomTypeId];
            }
            return value?.ToString() ?? "Unknown";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BookingStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int status)
            {
                // Đơn giản hóa - chỉ có trạng thái Booked
                return status == 1 ? "Booked" : "Unknown";
            }
            return "Unknown";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
