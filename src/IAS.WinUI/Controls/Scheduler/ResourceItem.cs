using Microsoft.UI.Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace IAS.WinUI.Controls
{
    public class ResourceItem : DependencyObject
    {
        public string Resource { get; }
        public ResourceItem(string resource)
        {
            Resource = resource;
        }

        public double MinHeight
        {
            get { return (double)GetValue(MinHeightProperty); }
            set { SetValue(MinHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MinHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinHeightProperty =
            DependencyProperty.Register(nameof(MinHeight), typeof(double), typeof(ResourceItem), new PropertyMetadata(0.0D));


    }
}
