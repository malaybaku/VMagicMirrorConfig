using System.Windows;

namespace Baku.VMagicMirrorConfig
{
    public partial class BGSettingWindow : Window
    {
        public BGSettingWindow() => InitializeComponent();

        private void OnClickOk(object sender, RoutedEventArgs e) 
            => DialogResult = true;
    }
}
