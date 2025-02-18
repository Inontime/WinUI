using System;
using System.Collections.Generic;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace IAS.WinUI.Controls
{
    public static class SchedulerHelper
    {
        public static TimeSpan GetMinorTimeInterval(SchedulerTimeScale timeScale)
        {
            return GetMajorTimeInterval(timeScale) / GetMinorColumnCount(timeScale);
        }
        public static TimeSpan GetMajorTimeInterval(SchedulerTimeScale timeScale)
        {
            return timeScale switch
            {
                SchedulerTimeScale.FourHour => TimeSpan.FromHours(1),
                SchedulerTimeScale.EightHour => TimeSpan.FromHours(1),
                SchedulerTimeScale.TwelveHour => TimeSpan.FromHours(2),
                SchedulerTimeScale.TwentyFourHour => TimeSpan.FromHours(4),
                SchedulerTimeScale.ThirtySixHours => TimeSpan.FromHours(8),
                SchedulerTimeScale.SeventyTwoHours => TimeSpan.FromHours(12),
                SchedulerTimeScale.Week => TimeSpan.FromDays(1),
                _ => TimeSpan.FromHours(24)
            };
        }

        public static int GetMinorColumnCount(SchedulerTimeScale timeScale)
            => timeScale switch
            {
                SchedulerTimeScale.FourHour => 4,
                SchedulerTimeScale.EightHour => 2,
                SchedulerTimeScale.TwelveHour => 4,
                SchedulerTimeScale.TwentyFourHour => 4,
                SchedulerTimeScale.ThirtySixHours => 4,
                SchedulerTimeScale.SeventyTwoHours => 6,
                SchedulerTimeScale.Week => 1,
                _ => 1
            };
        public static double PixelsPerMinute(SchedulerTimeScale timeScale)
        {
            var pixels = 2000;
            var timeSpanInMins = timeScale switch
            {
                SchedulerTimeScale.FourHour => TimeSpan.FromHours(4).TotalMinutes,
                SchedulerTimeScale.EightHour => TimeSpan.FromHours(8).TotalMinutes,
                SchedulerTimeScale.TwelveHour => TimeSpan.FromHours(12).TotalMinutes,
                SchedulerTimeScale.ThirtySixHours => TimeSpan.FromHours(36).TotalMinutes,
                SchedulerTimeScale.TwentyFourHour => TimeSpan.FromHours(24).TotalMinutes,
                SchedulerTimeScale.SeventyTwoHours => TimeSpan.FromHours(72).TotalMinutes,
                SchedulerTimeScale.Week => TimeSpan.FromDays(7).TotalMinutes,
                _ => 1
            };
            return pixels / timeSpanInMins;
        }
        public static double GetPixelsPerMinorColumn(SchedulerTimeScale timeScale)
        {
            var timeInterval = GetMinorTimeInterval(timeScale);
            var pixelsPerMinute = PixelsPerMinute(timeScale);
            return timeInterval.TotalMinutes * pixelsPerMinute;

        }

        public static int IndexOf(this SortedSet<string> set, string value)
        {
            int index = 0;
            foreach (var item in set)
            {
                if (item == value) return index;
                index++;
            }
            return -1; // Not found
        }
    }
}
