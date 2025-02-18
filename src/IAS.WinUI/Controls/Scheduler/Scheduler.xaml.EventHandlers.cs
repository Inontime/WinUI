using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace IAS.WinUI.Controls
{
    public sealed partial class Scheduler
    {
        private void TimeScaleButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string timeScale &&
                Enum.TryParse<SchedulerTimeScale>(timeScale, out var scale))
            {
                this.TimeScale = scale;
            }
        }

        private void MoveTimeWindowForwardButton_Click(object sender, RoutedEventArgs e)
        {
            var advance = TimeScale switch
            {
                SchedulerTimeScale.FourHour => TimeSpan.FromHours(4),
                SchedulerTimeScale.EightHour => TimeSpan.FromHours(8),
                SchedulerTimeScale.TwelveHour => TimeSpan.FromHours(12),
                SchedulerTimeScale.TwentyFourHour => TimeSpan.FromHours(24),
                SchedulerTimeScale.Week => TimeSpan.FromDays(7),
                _ => TimeSpan.FromHours(4),
            };
            TimeWindowStartTime = TimeWindowStartTime.Add(advance);
        }
        private void MoveTimeWindowBackwardButton_Click(object sender, RoutedEventArgs e)
        {
            var back = TimeScale switch
            {
                SchedulerTimeScale.FourHour => TimeSpan.FromHours(-4),
                SchedulerTimeScale.EightHour => TimeSpan.FromHours(-8),
                SchedulerTimeScale.TwelveHour => TimeSpan.FromHours(-12),
                SchedulerTimeScale.TwentyFourHour => TimeSpan.FromHours(-24),
                SchedulerTimeScale.Week => TimeSpan.FromDays(-7),
                _ => TimeSpan.FromHours(-4),
            };
            TimeWindowStartTime = TimeWindowStartTime.Add(back);
        }
    }
}
