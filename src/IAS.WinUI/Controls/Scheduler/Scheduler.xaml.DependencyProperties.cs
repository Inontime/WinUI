using Microsoft.UI.Xaml;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace IAS.WinUI.Controls
{
    public sealed partial class Scheduler
    {
        #region ItemsSource
        public object ItemsSource
        {
            get { return (object)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemsSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(object), typeof(Scheduler), new PropertyMetadata(null, OnItemsSourceChanged));



        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Scheduler scheduler)
            {
                scheduler.SubscribeToCollectionChanges(e.OldValue, e.NewValue);
            }
        }
        private List<INotifyPropertyChanged> _trackedItems = new();
        private void SubscribeToCollectionChanges(object? oldValue, object? newValue)
        {
            // Unsubscribe from previous collection
            if (oldValue is INotifyCollectionChanged _oldObservableCollection)
            {
                _oldObservableCollection.CollectionChanged -= OnCollectionChanged;
                UnsubscribeFromItemChanges();
            }

            if (newValue is INotifyCollectionChanged newCollection)
            {
                newCollection.CollectionChanged += OnCollectionChanged;
                SubscribeToItemChanges();
            }

            ResetRowItems();
            _timeItems.ReGenerateTimeItems(_layoutState.TimeScale, StartTime, GetMaxEndDate());
            _layoutState.InvalidateLayout();

        }
        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                //Subscribe to property changes for New Items
                foreach (var item in e.NewItems!.OfType<INotifyPropertyChanged>())
                {
                    item.PropertyChanged += OnItemPropertyChanged;
                    _trackedItems.Add(item);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                //Unsubscribe to property changes from old Items
                foreach (var item in e.OldItems!.OfType<INotifyPropertyChanged>())
                {
                    item.PropertyChanged -= OnItemPropertyChanged;
                    _trackedItems.Remove(item);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                UnsubscribeFromItemChanges();
                SubscribeToItemChanges();
            }
            else if (e.Action == NotifyCollectionChangedAction.Replace)
            {
                foreach (var item in e.OldItems!.OfType<INotifyPropertyChanged>())
                {
                    item.PropertyChanged -= OnItemPropertyChanged;
                    _trackedItems.Remove(item);
                }
                foreach (var item in e.NewItems!.OfType<INotifyPropertyChanged>())
                {
                    item.PropertyChanged += OnItemPropertyChanged;
                    _trackedItems.Add(item);
                }
            }
            ResetRowItems();
            ResetTimeItems();
            _layoutState.InvalidateLayout();
        }

        private void SubscribeToItemChanges()
        {
            if (ItemsSource == null) return;

            if (ItemsSource is IEnumerable e)
            {
                foreach (var item in e.OfType<INotifyPropertyChanged>())
                {
                    item.PropertyChanged += OnItemPropertyChanged;
                    _trackedItems.Add(item);
                }
            }
        }

        private void UnsubscribeFromItemChanges()
        {
            foreach (var item in _trackedItems)
            {
                item.PropertyChanged -= OnItemPropertyChanged;
            }
            _trackedItems.Clear();
        }

        private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == ResourceDisplayMemberPath)
            {
                ResetRowItems();
                _layoutState.InvalidateLayout();
            }
            else if (e.PropertyName == StartTimePath || e.PropertyName == EndTimePath)
            {
                _timeItems.ReGenerateTimeItems(_layoutState.TimeScale, StartTime, GetMaxEndDate());
                _layoutState.InvalidateLayout();
            }
        }
        private void ResetRowItems()
        {
            PropertyInfo? resourceProperty = null;
            if (ItemsSource is IEnumerable e)
            {
                SortedSet<string> _resources = [];
                foreach (var item in e)
                {
                    resourceProperty ??= item.GetType().GetProperty(ResourceDisplayMemberPath);
                    if (resourceProperty != null)
                    {
                        var resourceName = resourceProperty.GetValue(item)?.ToString();
                        if (resourceName is not null)
                        { _resources.Add(resourceName); }
                    }
                }
                _rowItems.Clear();
                _resources.ToList().ForEach(r => _rowItems.Add(new ResourceItem(r)));
            }
        }
        private void ResetTimeItems()
        {
            _timeItems.ReGenerateTimeItems(_layoutState.TimeScale, StartTime, GetMaxEndDate());
        }
        #endregion

        #region ItemTemplate Property
        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ItemTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register(nameof(ItemTemplate), typeof(DataTemplate), typeof(Scheduler), new PropertyMetadata(new DataTemplate(), OnItemTemplatePropertyChanged));

        private static void OnItemTemplatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }

        #endregion

        #region RowHeaderDisplayPath Property
        public string ResourceDisplayMemberPath
        {
            get { return (string)GetValue(RowHeaderDisplayMemberPathProperty); }
            set { SetValue(RowHeaderDisplayMemberPathProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RowHeaderDisplayMemberPath.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RowHeaderDisplayMemberPathProperty =
            DependencyProperty.Register(nameof(ResourceDisplayMemberPath), typeof(string), typeof(Scheduler), new PropertyMetadata("", OnResourceDisplayMemberPathChanged));

        private static void OnResourceDisplayMemberPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Scheduler scheduler)
            {
                scheduler._layoutState.ResourceDisplayMember = (string)e.NewValue;
            }
        }
        #endregion

        #region StartTimePath Property
        /// <summary>
        /// The name of the property in the item source that should be used for calculating the start time.  This
        /// property should either be a DateTime or a DateTimeoffset
        /// </summary>
        public string StartTimePath
        {
            get { return (string)GetValue(StartTimePathProperty); }
            set { SetValue(StartTimePathProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StartTimePath.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StartTimePathProperty =
            DependencyProperty.Register(nameof(StartTimePath), typeof(string), typeof(Scheduler), new PropertyMetadata("StartTime", OnStartTimePathPropertyChanged));

        private static void OnStartTimePathPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Scheduler scheduler)
            {
                scheduler._layoutState.StartTimeDisplayMember = (string)e.NewValue;
            }
        }

        #endregion

        #region EndTimePath Property


        public string EndTimePath
        {
            get { return (string)GetValue(EndTimePathProperty); }
            set { SetValue(EndTimePathProperty, value); }
        }

        // Using a DependencyProperty as the backing store for EndTimePath.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty EndTimePathProperty =
            DependencyProperty.Register(nameof(EndTimePath), typeof(string), typeof(Scheduler), new PropertyMetadata("EndTime", OnEndTimePathPropertyChanged));

        private static void OnEndTimePathPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Scheduler scheduler)
            {
                scheduler._layoutState.EndTimeDisplayMember = (string)e.NewValue;

            }
        }



        #endregion

        #region StartTimeProperty


        public DateTimeOffset StartTime
        {
            get { return (DateTimeOffset)GetValue(StartTimeProperty); }
            set { SetValue(StartTimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for StartTime.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty StartTimeProperty =
            DependencyProperty.Register(nameof(StartTime), typeof(DateTimeOffset), typeof(Scheduler), new PropertyMetadata(GetDefaultStartTime(), StartTimePropertyChanged));
        private static DateTimeOffset GetDefaultStartTime()
        {
            var today = DateTime.Today;
            var offset = TimeZoneInfo.Local.GetUtcOffset(today); // Corrects for DST
            var rVal = new DateTimeOffset(today, offset);
            return rVal;
        }
        private static void StartTimePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Scheduler scheduler)
            {
                if (e.NewValue is DateTimeOffset nv)
                {
                    scheduler._layoutState.StartTime = nv;
                }
            }
        }


        #endregion

        #region TimeWindowStartTime Property

        public DateTimeOffset TimeWindowStartTime
        {
            get { return (DateTimeOffset)GetValue(TimeWindowStartTimeProperty); }
            set { SetValue(TimeWindowStartTimeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TimeWindowStartTimeProperty =
            DependencyProperty.Register(nameof(TimeWindowStartTime), typeof(int), typeof(Scheduler), new PropertyMetadata(GetDefaultStartTime(), OnTimeWindowStartTimePropertyChanged));

        private static void OnTimeWindowStartTimePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Scheduler scheduler)
            {

            }
        }


        #endregion

        #region TimeScale Property

        public SchedulerTimeScale TimeScale
        {
            get { return (SchedulerTimeScale)GetValue(TimeScaleProperty); }
            set { SetValue(TimeScaleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TimeScale.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TimeScaleProperty =
            DependencyProperty.Register(nameof(TimeScale), typeof(SchedulerTimeScale), typeof(Scheduler), new PropertyMetadata(SchedulerTimeScale.FourHour, OnTimeScalePropertyChanged));

        private static void OnTimeScalePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Scheduler scheduler)
            {
                scheduler._timeItems.ReGenerateTimeItems((SchedulerTimeScale)e.NewValue, scheduler.StartTime, scheduler.GetMaxEndDate());
                scheduler._layoutState.TimeScale = (SchedulerTimeScale)e.NewValue;
                //scheduler.DetailsScrollViewer.ScrollTo(0, 0, new ScrollingScrollOptions(ScrollingAnimationMode.Disabled));
            }
        }


        #endregion

        #region TimePresenter DataTemplate


        public DataTemplate? TimePresenterTemplate
        {
            get { return (DataTemplate?)GetValue(TimePresenterTemplateProperty); }
            set { SetValue(TimePresenterTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TimePresenterTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TimePresenterTemplateProperty =
            DependencyProperty.Register(nameof(TimePresenterTemplate), typeof(DataTemplate), typeof(Scheduler), new PropertyMetadata(null, OnTimePresenterTemplateChanged));

        private static void OnTimePresenterTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }


        #endregion

    }


}
