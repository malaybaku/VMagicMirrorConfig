using System.Windows;

namespace Baku.VMagicMirrorConfig
{
    public partial class StartupSettingWindow : Window
    {
        public StartupSettingWindow() => InitializeComponent();

        private void OnClickOk(object sender, RoutedEventArgs e)
            => DialogResult = true;
    }
}
