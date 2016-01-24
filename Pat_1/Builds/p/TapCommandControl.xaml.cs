using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Pat_1
{
    public sealed partial class TapCommandControl : UserControl
    {
        public readonly DependencyProperty TapCommandProperty =
            DependencyProperty.Register(
                "TapCommand",
                typeof(ICommand),
                typeof(TapCommandControl),
                new PropertyMetadata(null));

        public readonly DependencyProperty TapCommandParameterProperty =
            DependencyProperty.Register(
                "TapCommandParameter",
                typeof(object),
                typeof(TapCommandControl),
                new PropertyMetadata(null));

        public TapCommandControl()
        {
            this.InitializeComponent();
        }

        public ICommand TapCommand
        {
            get { return (ICommand)GetValue(TapCommandProperty); }
            set { this.SetValue(TapCommandProperty, value); }
        }

        public object TapCommandParameter
        {
            get { return GetValue(TapCommandParameterProperty); }
            set { this.SetValue(TapCommandParameterProperty, value); }
        }

        public void ControlTapped(object sender, TappedRoutedEventArgs e)
        {
            if (this.TapCommand != null)
            {
                this.TapCommand.Execute(this.TapCommandParameter);
            }
        }
    }
}
