using System;
using System.Diagnostics;
using System.Windows;

namespace Baku.VMagicMirrorConfig
{
    public partial class MainWindow : Window
    {
        public MainWindow() => InitializeComponent();

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            (DataContext as IWindowViewModel)?.Initialize();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            (DataContext as IDisposable)?.Dispose();
        }

        private void OnClickHomeButton(object sender, RoutedEventArgs e)
        {
            HideAllTabElements();
            homeTab.Visibility = Visibility.Visible;
        }

        private void OnClickBGSettingButton(object sender, RoutedEventArgs e)
        {
            HideAllTabElements();
            backgroundTab.Visibility = Visibility.Visible;
        }

        private void OnClickLayoutButton(object sender, RoutedEventArgs e)
        {
            HideAllTabElements();
            layoutTab.Visibility = Visibility.Visible;
        }

        private void OnClickStartupButton(object sender, RoutedEventArgs e)
        {
            HideAllTabElements();
            startupTab.Visibility = Visibility.Visible;
        }

        private void HideAllTabElements()
        {
            homeTab.Visibility = Visibility.Collapsed;
            backgroundTab.Visibility = Visibility.Collapsed;
            layoutTab.Visibility = Visibility.Collapsed;
            startupTab.Visibility = Visibility.Collapsed;

        }

        private void OnClickHyperLinkToGitHub(object sender, RoutedEventArgs e) 
            => Process.Start("https://github.com/malaybaku/VMagicMirror");
    }
}
