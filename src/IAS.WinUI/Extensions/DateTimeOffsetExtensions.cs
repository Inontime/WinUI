﻿using System;
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

        /// <summary>
        /// Converts a DateTime to a DateTimeOffset, without risking any onerous exceptions
        /// the framework quite unfortunately throws within the DateTimeOffset constructor, 
        /// such as they do when the source DateTime's Kind is not set to UTC. The best and 
        /// most performant way around this, which we do herein, is to simply construct the 
        /// new DateTimeOffset with the overload that excepts Ticks. Also, we will simply 
        /// return <see cref="DateTimeOffset.MinValue"/> if the source DateTime was 
        /// <see cref="DateTime.MinValue"/>.
        /// </summary>
        /// <param name="dt">Source DateTime.</param>
        /// <param name="offset">Offset</param>
        public static DateTimeOffset ToDateTimeOffset(this DateTime dt, TimeSpan offset)
        {
            // adding negative offset to a min-datetime will throw, this is a 
            // sufficient catch. Note however that a DateTime of just a few hours can still throw
            if (dt == DateTime.MinValue)
                return DateTimeOffset.MinValue;

            return new DateTimeOffset(dt.Ticks, offset);
        }

        public static DateTimeOffset ToDateTimeOffset(this DateTime dt, double offsetInHours = 0)
            => ToDateTimeOffset(dt, offsetInHours == 0 ? TimeSpan.Zero : TimeSpan.FromHours(offsetInHours));
    }
}
