using System.Windows;
using System.Windows.Controls;

namespace Baku.VMagicMirrorConfig
{
    public partial class AboutPanel : UserControl
    {
        public AboutPanel() => InitializeComponent();

        private void OnClickHyperLinkToGitHub(object sender, RoutedEventArgs e)
            => UrlNavigate.Open("https://github.com/malaybaku/VMagicMirror");

        private void OnClickHyperLinkToModelData(object sender, RoutedEventArgs e)
            => UrlNavigate.Open("https://sketchfab.com/3d-models/xbox-controller-fb71f28a6eab4a2785cf68ff87c4c1fc");
    }
}
