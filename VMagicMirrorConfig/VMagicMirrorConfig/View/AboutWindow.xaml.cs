using System.Diagnostics;
using System.Windows;

namespace Baku.VMagicMirrorConfig
{
    public partial class AboutWindow : Window
    {
        public AboutWindow() => InitializeComponent();

        private void OnClickHyperLinkToGitHub(object sender, RoutedEventArgs e) 
            => Process.Start("https://github.com/malaybaku/VMagicMirror");

    }
}
