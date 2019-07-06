using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace Baku.VMagicMirrorConfig.LargePointer
{
    using static NativeMethods;

    //NOTE: ウィンドウハンドルをゴリゴリ操作する + コレ以外のコードほぼないのでコードビハインド使う。
    public partial class MainWindow : Window
    {
        private const int MouseTrackIntervalMillisec = 16;
        private const int MouseStopDisappearTimeMillisec = 5000;
        private const double OpacityChangeLerpFactor = 0.1;

        public MainWindow()
        {
            InitializeComponent();
        }

        private CancellationTokenSource _cts;
        private IntPtr _hWnd = IntPtr.Zero;
        private int _width = 1;
        private int _height = 1;

        private int _prevMouseX = -1;
        private int _prevMouseY = -1;
        private double _prevOpacity = 1.0;
        private int _mouseStopTimeMillisec = 0;

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

        protected override void OnActivated(EventArgs e)
        {
            //メタファとしてこのプロセスはコンフィグプロセスの子ウィンドウっぽく動くべき = コンフィグプロセスがアクティブになったものと扱う
            ActivateConfigWindowProcess();
        }

        private void SetClickThrough(IntPtr hWnd)
        {
            uint exStyle = GetWindowLong(hWnd, GWL_EXSTYLE);
            SetWindowLong(hWnd, GWL_EXSTYLE, exStyle | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW);
        }

        private void StartSyncWindowPositionToMouse()
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            Task.Run(async () => await LoopUpdateWindowPositionAsync(_cts.Token));
        }

        private async Task LoopUpdateWindowPositionAsync(CancellationToken token)
        {
            while (!_cts.IsCancellationRequested)
            {
                Thread.Sleep(MouseTrackIntervalMillisec);
                //ここでマウス位置をとって移動
                await Dispatcher.BeginInvoke(
                    new Action(UpdatePointerPositionAndOpacity)
                    );
            }
        }

        private void UpdatePointerPositionAndOpacity()
        {
            bool isMouseMoved = CheckAndTrackMousePosition();

            //マウスが止まりっぱなしかどうかチェック
            if (isMouseMoved)
            {
                _mouseStopTimeMillisec = 0;
            }
            else if (_mouseStopTimeMillisec < MouseStopDisappearTimeMillisec)
            {
                _mouseStopTimeMillisec += MouseTrackIntervalMillisec;
            }

            //ずっと止まってるならポインターを消す。そうじゃなければつける。
            //いずれもパッと変えると違和感あるのでLerpする。
            double goalOpacity =
                (_mouseStopTimeMillisec < MouseStopDisappearTimeMillisec) ?
                1.0 :
                0.0;

            double nextOpacity = Lerp(_prevOpacity, goalOpacity, OpacityChangeLerpFactor);
            MainGrid.Opacity = nextOpacity;
            _prevOpacity = nextOpacity;
        }

        private bool CheckAndTrackMousePosition()
        {
            var cursorPos = GetWindowsMousePosition();
            if (cursorPos.X != _prevMouseX || cursorPos.Y != _prevMouseY)
            {
                SetWindowPosition(_hWnd, cursorPos.X - _width / 2, cursorPos.Y - _height / 2);
                _prevMouseX = cursorPos.X;
                _prevMouseY = cursorPos.Y;
                return true;
            }
            else
            {
                return false;
            }
        }

        private void ActivateConfigWindowProcess()
        {
            var processes = Process.GetProcessesByName("VMagicMirrorConfig");
            if(processes.Length == 0)
            {
                return;
            }
            ActivateWindow(processes[0].MainWindowHandle);
        }

        private static double Lerp(double a, double b, double rate)
            => a * (1.0 - rate) + b * rate;

    }
}
