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
        
    }
}
