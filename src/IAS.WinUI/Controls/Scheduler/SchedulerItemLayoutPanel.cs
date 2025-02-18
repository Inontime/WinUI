using IAS.WinUI.Extensions;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Windows.Foundation;

namespace IAS.WinUI.Controls
{
    internal partial class SchedulerItemLayoutPanel : VirtualizingLayout
    {
        private readonly SchedulerLayoutState _layoutState;
        private PropertyInfo? _resourceNameProperty;
        private PropertyInfo? _startTimeProperty;
        private PropertyInfo? _endTimeProperty;
        public SchedulerItemLayoutPanel(SchedulerLayoutState layoutState)
        {
            _layoutState = layoutState;
            _layoutState.StateChanged += () =>
            {
                _resourceNameProperty = null;
                _startTimeProperty = null;
                _endTimeProperty = null;
                InvalidateMeasure();
                InvalidateArrange();

            };
        }
        protected override Size MeasureOverride(VirtualizingLayoutContext context, Size availableSize)
        {

            bool rowChanged = false;

            if (context.ItemCount > 0)
            {
                Dictionary<int, List<int>> rowItemIndices = new(); // Store item indices per row
                SortedSet<string> resourceNames = new();

                // Pass 1: Initialize layout item properties and create list of resource names           
                for (int i = 0; i < context.ItemCount; i++)
                {
                    var item = context.GetItemAt(i);
                    var layoutItem = _layoutState.GetLayoutItem(i) ?? new SchedulerDetailsLayoutItem();

                    _resourceNameProperty ??= item.GetType().GetProperty(_layoutState.ResourceDisplayMember ?? "");
                    _startTimeProperty ??= item.GetType().GetProperty(_layoutState.StartTimeDisplayMember ?? "");
                    _endTimeProperty ??= item.GetType().GetProperty(_layoutState.EndTimeDisplayMember ?? "");
                    if (_resourceNameProperty != null && layoutItem.ResourceName is null)
                    {
                        layoutItem.ResourceName = _resourceNameProperty.GetValue(item) as string;
                    }

                    if (_startTimeProperty != null && layoutItem.StartTime == default)
                    {
                        layoutItem.StartTime = (DateTimeOffset)_startTimeProperty.GetValue(item)!;
                    }

                    if (_endTimeProperty != null && layoutItem.EndTime == default)
                    {
                        layoutItem.EndTime = (DateTimeOffset)_endTimeProperty.GetValue(item)!;
                    }

                    resourceNames.Add(layoutItem.ResourceName ?? "No Resource");
                    _layoutState.SetLayoutItem(i, layoutItem);
                }

                // Pass 2: Group items by Resource Name
                for (int i = 0; i < context.ItemCount; i++)
                {
                    var layoutItem = _layoutState.GetLayoutItem(i) ?? throw new Exception("Collection changed");
                    int rowIndex = resourceNames.IndexOf(layoutItem.ResourceName ?? "No Resource");
                    if (!rowItemIndices.ContainsKey(rowIndex))
                        rowItemIndices[rowIndex] = [];
                    rowItemIndices[rowIndex].Add(i);
                }


                // Pass 3: Assign layout positions and resolve stacking within rows
                foreach (var row in rowItemIndices.OrderBy(kvp => kvp.Key))
                {
                    var rowItemIndexes = row.Value.OrderBy(i => i).ToList();
                    var rowHeight = _layoutState.GetRowHeight(row.Key);
                    foreach (var itemIndex in rowItemIndexes)
                    {
                        var layoutItem = _layoutState.GetLayoutItem(itemIndex) ?? new SchedulerDetailsLayoutItem();
                        layoutItem.RowIndex = row.Key;
                        var item = context.GetItemAt(itemIndex);
                        var baseYOffset = GetBaseYOffset(row.Key);

                        if (layoutItem.LayoutBounds == default)
                        {
                            var element = context.GetOrCreateElementAt(itemIndex);
                            element.Measure(new Size(GetWidth(layoutItem.StartTime, layoutItem.EndTime), availableSize.Height));

                            layoutItem.IsInView = true; // will recycle later if not in view
                            layoutItem.LayoutBounds = new Rect(
                                x: GetXOffset(layoutItem.StartTime),
                                y: baseYOffset,
                                width: GetWidth(layoutItem.StartTime, layoutItem.EndTime),
                                height: element.DesiredSize.Height);
                            layoutItem.DesiredSize = element.DesiredSize;
                        }

                        var proposedRect = layoutItem.LayoutBounds;
                        bool nowInView = proposedRect.Intersects(context.RealizationRect);
                        // If it wasn't visible and now is, realize the container
                        if (nowInView && !layoutItem.IsInView)
                        {
                            var element = context.GetOrCreateElementAt(itemIndex);
                            element.Measure(new Size(GetWidth(layoutItem.StartTime, layoutItem.EndTime), availableSize.Height));
                            layoutItem.DesiredSize = element.DesiredSize;
                            proposedRect = new Rect(
                                proposedRect.X,
                                proposedRect.Y,
                                proposedRect.Width,
                                element.DesiredSize.Height);
                        }
                        // If it was visible, but now isn't recycle the container
                        else if (!nowInView && layoutItem.IsInView)
                        {
                            var element = context.GetOrCreateElementAt(itemIndex);
                            context.RecycleElement(element);
                        }


                        while (true)
                        {
                            var overlappingItem = rowItemIndexes
                                .Where(r => r < itemIndex).Select(_layoutState.GetLayoutItem)
                                .FirstOrDefault(r => r is not null && r.LayoutBounds.Intersects(proposedRect));
                            if (overlappingItem != null)
                            {
                                proposedRect = new Rect(
                                    x: proposedRect.X,
                                    y: overlappingItem.LayoutBounds.Bottom,
                                    width: proposedRect.Width,
                                    height: proposedRect.Height);
                            }
                            else { break; }
                        }
                        layoutItem.LayoutBounds = proposedRect;
                        layoutItem.IsInView = layoutItem.LayoutBounds.Intersects(context.RealizationRect);
                        _layoutState.SetLayoutItem(itemIndex, layoutItem);
                    }
                    var yOffset = GetBaseYOffset(row.Key);
                    var newRowHeight = _layoutState.GetRowHeight(row.Key);
                    rowChanged = newRowHeight != rowHeight;
                }

                _layoutState.UpdateRowsComplete(rowChanged);
                var maxHeight = _layoutState.GetMaxHeight();
                var maxWidth = _layoutState.GetMaxWidth();

                Debug.WriteLine($"MeasureOverride - MaxHeight: {maxHeight}, MaxWidth: {maxWidth}");
                return new Size(maxWidth, maxHeight);
            }

            return availableSize;
        }
        protected override Size ArrangeOverride(VirtualizingLayoutContext context, Size finalSize)
        {
            for (int i = 0; i < context.ItemCount; i++)
            {
                if (_layoutState.GetLayoutItem(i) is SchedulerDetailsLayoutItem layoutItem &&
                    layoutItem.IsInView)
                {
                    var element = context.GetOrCreateElementAt(i);
                    element.Arrange(layoutItem.LayoutBounds);
                }
            }
            var maxHeight = _layoutState.GetMaxHeight();
            var maxWidth = _layoutState.GetMaxWidth();
            var newSize = new Size(maxWidth, maxHeight);
            Debug.WriteLine($"ArrangeOverride called with finalSize: {finalSize} newSize {newSize} {DateTime.Now.Ticks}");
            return newSize;
        }
        private double GetBaseYOffset(int rowIndex)
        {
            return rowIndex > 0
                ? Enumerable.Range(0, rowIndex).Sum(_layoutState.GetRowHeight)
                : 0;
        }
        private double GetXOffset(DateTimeOffset forTime)
        {
            var pixelsPerMinute = SchedulerHelper.PixelsPerMinute(_layoutState.TimeScale);
            var gap = forTime - _layoutState.StartTime;
            return gap.TotalMinutes * pixelsPerMinute;
        }
        private double GetWidth(DateTimeOffset startTime, DateTimeOffset endTime)
        {
            var pixelsPerMinute = SchedulerHelper.PixelsPerMinute(_layoutState.TimeScale);
            return (endTime - startTime).TotalMinutes * pixelsPerMinute;
        }


    }
}
