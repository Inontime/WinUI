using System;
using System.Collections.ObjectModel;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace IAS.WinUI.Controls
{
    public class TimeItem
    {
        public DateTimeOffset Time { get; }
        public double Width { get; }

        public string TimeValue => Time.ToString("HH:mm");
        public string TimeDate => Time.ToString("MM/dd/yyyy");
        public TimeItem(DateTimeOffset time, double width)
        {
            Time = time;
            Width = width;
        }
    }
    public class TimeItemGroup
    {
        public string TimeString => Time.ToLocalTime().ToString("HH:mm");
        public string DateString => Time.ToLocalTime().ToString("MM/dd/yyyy");
        public DateTimeOffset Time { get; }
        public double Width { get; }
        public ObservableCollection<TimeItem> MinorItems { get; }

        public TimeItemGroup(
            DateTimeOffset majorTime,
            double width,
            ObservableCollection<TimeItem> minorItems)
        {
            Time = majorTime;
            Width = width;
            MinorItems = minorItems;
        }
    }
}
