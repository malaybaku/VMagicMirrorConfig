using System.Windows;

namespace Baku.VMagicMirrorConfig
{
    public partial class ColorPickerWindow : Window
    {
        public ColorPickerWindow() => InitializeComponent();

        private void OnClickCancel(object sender, RoutedEventArgs e)
            => DialogResult = false;

        private void OnClickOk(object sender, RoutedEventArgs e) 
            => DialogResult = true;
    }
}
