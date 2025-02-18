using IAS.WinUI.Converters;

namespace IAS.WinUI.Controls
{

    public enum SchedulerTimeScale
    {
        FourHour,
        EightHour,
        TwelveHour,
        TwentyFourHour,
        ThirtySixHours,
        SeventyTwoHours,
        Week,
    }

    public class SchedulerTimeScaleValueConverter : EnumToBooleanConverter<SchedulerTimeScale>
    {

    }
}
