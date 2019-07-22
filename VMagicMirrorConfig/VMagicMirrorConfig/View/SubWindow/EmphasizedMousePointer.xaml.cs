using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace Baku.VMagicMirrorConfig
{
    using static NativeMethods;

    public partial class EmphasizedMousePointer : Window
    {
        public EmphasizedMousePointer()
        {
            InitializeComponent();
        }

        private CancellationTokenSource _cts;
        private IntPtr _hWnd = IntPtr.Zero;
        private int _width = 1;
        private int _height = 1;

        //NOTE: Win32APIとかウィンドウ自体の操作があまりに多いのでデザパタ捨ててコードビハインドで頑張ります。

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            _hWnd = new WindowInteropHelper(this).Handle;
            SetClickThrough(_hWnd);

            var rect = GetWindowRect(_hWnd);
            _width = rect.right - rect.left;
            _height = rect.bottom - rect.top;

            StartSyncWindowPositionToMouse();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            _cts?.Cancel();
        }

        private void SetClickThrough(IntPtr hWnd)
        {
            //uint style = GetWindowLong(hWnd, GWL_STYLE);
            //style = style | WS_POPUP;
            //SetWindowLong(hWnd, GWL_STYLE, style);

            //uint exStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
            //exStyle = exStyle | WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW;
            //SetWindowLong(hWnd, GWL_EXSTYLE, exStyle);

            uint exStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
            SetWindowLong(hWnd, GWL_EXSTYLE, exStyle | WS_EX_TRANSPARENT);
        }

        private void StartSyncWindowPositionToMouse()
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            Task.Run(async () =>
            {
                while (!_cts.IsCancellationRequested)
                {
                    await Task.Delay(16, _cts.Token);
                    //ここでマウス位置をとって移動
                    await Dispatcher.BeginInvoke(
                        new Action(() => UpdateWindowPosition())
                        );
                }
            });
        }

        private void UpdateWindowPosition()
        {
            var cursorPos = System.Windows.Forms.Cursor.Position;
            SetWindowPosition(_hWnd, cursorPos.X - _width / 2, cursorPos.Y - _height / 2);
        }

    }
}
