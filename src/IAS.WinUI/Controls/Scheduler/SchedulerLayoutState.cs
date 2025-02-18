using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Windows.Foundation;

namespace IAS.WinUI.Controls
{
    internal class SchedulerLayoutState
    {
        private ConcurrentDictionary<int, SchedulerDetailsLayoutItem> _itemPositions = new();
        private ConcurrentDictionary<int, SchedulerDetailsLayoutItem> _rowPositions = new();
        private SchedulerTimeScale _timeScale;
        private DateTimeOffset _startTime;
        private string? _resourceDisplayMember;
        private string? _startTimeDisplayMember;
        private string? _endTimeDisplayMember;

        public event Action? StateChanged;

        public SchedulerLayoutState(DateTimeOffset startTime, string resourceDisplayMemberPath, string startTimePath, string endTimePath)
        {
            StartTime = startTime;
            _resourceDisplayMember = resourceDisplayMemberPath;
            _startTimeDisplayMember = startTimePath;
            _endTimeDisplayMember = endTimePath;
        }

        public SchedulerDetailsLayoutItem? GetLayoutItem(int index)
        {
            if (_itemPositions.TryGetValue(index, out var item))
            {
                return item;
            }
            return null;
        }
        public void SetLayoutItem(int index, SchedulerDetailsLayoutItem item)
        {
            _itemPositions[index] = item;
        }
        public SchedulerDetailsLayoutItem? GetRowLayoutItem(int index)
        {
            if (_rowPositions.TryGetValue(index, out var item))
            {
                return item;
            }
            return null;
        }
        public void SetRowLayoutItem(int index, SchedulerDetailsLayoutItem item)
        {
            _rowPositions[index] = item;
        }
        public double GetMaxWidth() => _itemPositions.Any() ? _itemPositions.Max(kv => kv.Value.LayoutBounds.Right) : 0D;
        public double GetMaxHeight() => _itemPositions.Any() ? _itemPositions.Max(kv => kv.Value.LayoutBounds.Bottom) : 0D;
        public void UpdateRowsComplete(bool hasChanges)
        {
            if (hasChanges)
            {
                NotifyStateChanged();
            }
        }
        public DateTimeOffset StartTime
        {
            get => _startTime;
            set
            {
                _startTime = value;
                InvalidateLayout();
            }
        }
        public string? ResourceDisplayMember
        {
            get => _resourceDisplayMember;
            set
            {
                if (_resourceDisplayMember != value)
                {
                    _resourceDisplayMember = value;
                    InvalidateLayout();
                }
            }
        }
        public string? StartTimeDisplayMember
        {
            get => _startTimeDisplayMember;
            set
            {
                if (_startTimeDisplayMember != value)
                {
                    _startTimeDisplayMember = value;
                    InvalidateLayout();
                }
            }
        }
        public string? EndTimeDisplayMember
        {
            get => _endTimeDisplayMember;
            set
            {
                if (_endTimeDisplayMember != value)
                {
                    _endTimeDisplayMember = value;
                    InvalidateLayout();
                }
            }
        }
        public double GetItemWidth(DateTimeOffset startTime, DateTimeOffset endTime)
        {
            var pixelsPerMinute = SchedulerHelper.GetPixelsPerMinorColumn(_timeScale) / SchedulerHelper.GetMinorTimeInterval(_timeScale).TotalMinutes;
            return (endTime - startTime).TotalMinutes * pixelsPerMinute;
        }
        public void InvalidateLayout()
        {
            _itemPositions.Clear();
            _rowPositions.Clear();
            NotifyStateChanged();
        }
        public void NotifyStateChanged()
        {
            Debug.WriteLine("SchedulerLayoutState - StateChanged triggered");
            Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread().TryEnqueue(() => StateChanged?.Invoke());
        }

        public double GetRowHeight(int index)
        {
            var itemMaxBottom = _itemPositions.Any(i => i.Value.RowIndex == index)
                ? _itemPositions.Where(i => i.Value.RowIndex == index)
                    .Select(i => i.Value.LayoutBounds.Bottom)
                    .Max()
                : 0;
            var rowRect = _rowPositions.TryGetValue(index, out var rowItem)
                ? rowItem.LayoutBounds
                : new Rect(0, 0, 0, 0);

            var itemMaxTop = _itemPositions.Any(i => i.Value.RowIndex == index)
                ? _itemPositions.Where(i => i.Value.RowIndex == index)
                    .Select(i => i.Value.LayoutBounds.Top)
                    .Min()
                : 0;
            var itemsRequestHeight = Math.Clamp(itemMaxBottom - itemMaxTop, 0, double.MaxValue);
            return Math.Max(rowRect.Height, itemsRequestHeight);
        }

        internal void ClearLayoutItems()
        {
            _itemPositions.Clear();
        }

        public void TrimLayoutItems(int endIndex)
        {
            var keysToRemove = _itemPositions.Keys.Where(k => k >= endIndex).ToList();
            foreach (var key in keysToRemove)
            {
                _itemPositions.Remove(key, out SchedulerDetailsLayoutItem? _);
            }
        }

        public SchedulerTimeScale TimeScale
        {
            get => _timeScale;
            set
            {
                if (Equals(_timeScale, value) == false)
                {
                    _timeScale = value;
                    InvalidateLayout();
                }
            }
        }
    }
    internal class SchedulerDetailsLayoutItem
    {
        public string? ResourceName { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public Size DesiredSize { get; set; }
        public Rect LayoutBounds { get; set; }
        public bool IsInView { get; set; }
        public int RowIndex { get; set; }
    }

}
