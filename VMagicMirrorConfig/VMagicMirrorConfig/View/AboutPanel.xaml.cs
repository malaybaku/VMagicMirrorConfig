using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace Baku.VMagicMirrorConfig
{
    public partial class AboutPanel : UserControl
    {
        public AboutPanel() => InitializeComponent();

        private void OnClickHyperLinkToGitHub(object sender, RoutedEventArgs e)
            => Process.Start("https://github.com/malaybaku/VMagicMirror");

        private void OnShowLicenseClick(object sender, RoutedEventArgs e)
            => new LicenseWindow().ShowDialog();
    }
}
