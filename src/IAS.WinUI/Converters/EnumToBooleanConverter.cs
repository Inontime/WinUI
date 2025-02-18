using Microsoft.UI.Xaml.Data;
using System;

namespace IAS.WinUI.Converters
{
    public abstract class EnumToBooleanConverter<T> : IValueConverter
        where T : struct, Enum
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (parameter is string p &&
                Enum.TryParse<T>(p, out T parameterResult))
            {
                return Equals(parameterResult, value);
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
