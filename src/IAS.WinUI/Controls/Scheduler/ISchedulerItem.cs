using System;

namespace IAS.WinUI.Controls
{
    public interface ISchedulerItem
    {
        DateTimeOffset StartTime { get; set; }
        DateTimeOffset EndTime { get; set; }
    }
}
