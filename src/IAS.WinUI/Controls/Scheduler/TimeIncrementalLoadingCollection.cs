using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace IAS.WinUI.Controls
{
    public class TimeIncrementalLoadingCollection : ObservableCollection<TimeItemGroup>
    {
        private SchedulerTimeScale _timeScale = SchedulerTimeScale.FourHour;
        private DateTimeOffset _startTime;
        private bool _deferNotifications;
        private List<NotifyCollectionChangedEventArgs> _deferredEvents = new List<NotifyCollectionChangedEventArgs>();
        private SynchronizationContext _syncContext;

        public TimeIncrementalLoadingCollection()
        {
            _syncContext = SynchronizationContext.Current ?? throw new ArgumentNullException("SynchronizationContext.Current should be null");
        }

        public bool HasMoreItems => true;

        public void DeferNotifications()
        {
            _deferNotifications = true;
        }

        /// <summary>
        /// Resumes 
        /// </summary>
        /// <param name="replay"></param>
        public void ResumeNotifications(bool replay = false)
        {
            _deferNotifications = false;
            if (replay)
            {
                foreach (var e in _deferredEvents)
                {
                    OnCollectionChanged(e);
                }
            }
            else
            {
                if (_deferredEvents.Count > 0)
                {
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                }
            }

            _deferredEvents.Clear();
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (_deferNotifications)
            {
                _deferredEvents.Add(e);
            }
            else
            {
                base.OnCollectionChanged(e);
            }
        }
        public void ReGenerateTimeItems(SchedulerTimeScale scale, DateTimeOffset startTime, DateTimeOffset endTime)
        {
            this._timeScale = scale;
            this._startTime = startTime;
            DeferNotifications();
            Clear();
            var minor = SchedulerHelper.GetMinorColumnCount(_timeScale);
            var minorTimeInterval = SchedulerHelper.GetMinorTimeInterval(_timeScale);
            var majorTimeInterval = SchedulerHelper.GetMajorTimeInterval(_timeScale);
            var items = Enumerable.Range(0, int.MaxValue)
                .TakeWhile(i => startTime.Add(i * majorTimeInterval) < endTime)
                .Select(majorIndex =>
                {
                    var minorItems = Enumerable.Range(0, minor)
                    .Select(minorIndex => new TimeItem(
                            time: startTime.Add((majorIndex * majorTimeInterval) + (minorIndex * minorTimeInterval)),
                            width: minorTimeInterval.TotalMinutes * SchedulerHelper.PixelsPerMinute(_timeScale)))
                        .ToArray();

                    var rVal = new TimeItemGroup(
                            majorTime: startTime.Add(majorIndex * majorTimeInterval),
                            width: majorTimeInterval.TotalMinutes * SchedulerHelper.PixelsPerMinute(_timeScale),
                            minorItems: [.. minorItems]);
                    return rVal;
                });
            foreach (var i in items)
            {
                Add(i);
            }
            ResumeNotifications(false);
        }

    }
}
