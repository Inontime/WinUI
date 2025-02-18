using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using Windows.Foundation;

namespace IAS.WinUI.Controls
{
    internal partial class RowItemLayoutPanel : VirtualizingLayout
    {
        private readonly SchedulerLayoutState _layoutState;

        public RowItemLayoutPanel(SchedulerLayoutState layoutState)
        {
            this._layoutState = layoutState;
            _layoutState.StateChanged += () =>
            {
                InvalidateMeasure();
                InvalidateArrange();
            };
        }
        protected override Size MeasureOverride(VirtualizingLayoutContext context, Size availableSize)
        {
            for (int i = 0; i < context.ItemCount; i++)
            {
                var element = context.GetOrCreateElementAt(i);
                element.Measure(availableSize);
                var layoutItem = _layoutState.GetRowLayoutItem(i) ?? new SchedulerDetailsLayoutItem();
                layoutItem.DesiredSize = element.DesiredSize;
                layoutItem.IsInView = true;
                layoutItem.RowIndex = i;
                layoutItem.LayoutBounds = new Rect(
                    x: 0,
                    y: GetYOffset(i),
                    width: element.DesiredSize.Width,
                    height: Math.Max(element.DesiredSize.Height, _layoutState.GetRowHeight(i)));
                if (context.GetItemAt(i) is ResourceItem resourceItem)
                {
                    resourceItem.MinHeight = layoutItem.LayoutBounds.Height;
                }
                _layoutState.SetRowLayoutItem(i, layoutItem);
            }
            var maxWidth = context.ItemCount > 0
                ? Enumerable.Range(0, context.ItemCount).Max(i => _layoutState.GetRowLayoutItem(i)?.LayoutBounds.Right ?? 0)
                : 0D;
            var height = context.ItemCount > 0
                ? _layoutState.GetRowLayoutItem(context.ItemCount - 1)?.LayoutBounds.Bottom ?? 0
                : 0D;
            return new Size(maxWidth, height);
        }

        protected override Size ArrangeOverride(VirtualizingLayoutContext context, Size finalSize)
        {

            for (int i = 0; i < context.ItemCount; i++)
            {
                var layoutItem = _layoutState.GetRowLayoutItem(i);
                if (layoutItem == null) continue;

                if (layoutItem.IsInView)
                {
                    var element = context.GetOrCreateElementAt(i);
                    element.Arrange(layoutItem.LayoutBounds);
                }
            }

            return finalSize;
        }
        private double GetYOffset(int rowIndex)
        {
            return Enumerable.Range(0, rowIndex).Sum(r => _layoutState.GetRowHeight(r));
        }
    }
}
