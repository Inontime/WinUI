using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Reflection;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace IAS.WinUI.Controls
{
    public sealed partial class Scheduler : UserControl
    {
        private readonly SchedulerLayoutState _layoutState;
        private ObservableCollection<ResourceItem> _rowItems = [];
        private TimeIncrementalLoadingCollection _timeItems = [];

        public Scheduler()
        {
            InitializeComponent();
            _layoutState = new(StartTime, ResourceDisplayMemberPath, StartTimePath, EndTimePath);
            _layoutState.StartTime = StartTime;
            ResourceItemsRepeater.Layout = new RowItemLayoutPanel(_layoutState);
            DetailsItemsRepeater.Layout = new SchedulerItemLayoutPanel(_layoutState);
            DetailsItemsRepeater.SizeChanged += (s, e) =>
            {
                RepeaterGrid.Width = DetailsItemsRepeater.ActualWidth + ResourceNameColumn.ActualWidth;
                RepeaterGrid.Height = DetailsItemsRepeater.ActualHeight + TimeSeriesRow.ActualHeight;

            };
            DetailsScrollViewer.ViewChanged += DetailsScrollViewer_ViewChanged;
            _timeItems.ReGenerateTimeItems(_layoutState.TimeScale, StartTime, GetMaxEndDate());

        }

        private void DetailsScrollViewer_ViewChanged(ScrollView sender, object args)
        {
            double maxX = Math.Max(0, sender.ExtentWidth - sender.ViewportWidth);
            double maxY = Math.Max(0, sender.ExtentHeight - sender.ViewportHeight);

            TimesHeadersTransform.Y = Math.Clamp(sender.VerticalOffset, 0, maxY);
            ResourceItemsTransform.X = Math.Clamp(sender.HorizontalOffset, 0, maxX);

            DetailsItemsRepeater.Clip = new RectangleGeometry()
            {
                Rect = new Windows.Foundation.Rect(sender.HorizontalOffset, sender.VerticalOffset, sender.ViewportWidth, sender.ViewportHeight)
            };
            TimeHeadersItemsRepeater.Clip = new RectangleGeometry()
            {
                Rect = new Windows.Foundation.Rect(sender.HorizontalOffset, 0, sender.ViewportWidth, TimeHeadersItemsRepeater.ActualHeight)
            };
            ResourceItemsRepeater.Clip = new RectangleGeometry()
            {
                Rect = new Windows.Foundation.Rect(0, sender.VerticalOffset, ResourceItemsRepeater.ActualWidth, sender.ViewportHeight)
            };
        }

        private DateTimeOffset GetMaxEndDate()
        {
            PropertyInfo? propInfo = null;
            DateTimeOffset rVal = DateTimeOffset.MinValue;
            if (ItemsSource is IEnumerable e)
            {
                foreach (var item in e)
                {
                    propInfo ??= item.GetType().GetProperty(EndTimePath);
                    if (propInfo != null)
                    {
                        var value = propInfo.GetValue(item);
                        if (value is DateTimeOffset dateTimeOffset)
                        {
                            rVal = dateTimeOffset > rVal ? dateTimeOffset : rVal;
                        }
                    }
                }
            }
            return rVal;
        }


    }
}
