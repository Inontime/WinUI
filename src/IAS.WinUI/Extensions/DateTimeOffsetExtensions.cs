using System;
using System.Collections.Generic;
using System.Linq;

namespace IAS.WinUI
{
    public static class DateTimeOffsetExtensions
    {
        public static DateTimeOffset ClosestItem(this IEnumerable<DateTimeOffset> array, DateTimeOffset value)
        {
            if (array == null || !array.Any())
                return value;

            DateTimeOffset? bestMatch = null;

            foreach (var item in array)
            {
                if (item <= value && (bestMatch == null || item > bestMatch))
                {
                    bestMatch = item;
                }
            }

            return bestMatch ?? DateTimeOffset.MinValue;
        }
    }
}
