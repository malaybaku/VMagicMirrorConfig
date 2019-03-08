using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Baku.VMagicMirrorConfig
{
    //グローバルフックのほうが筋がよいらしいのでお蔵入り
    [Obsolete]
    class MousePositionAndEvent : IDisposable
    {
        public MousePositionAndEvent(Window window)
        {
            _window = window;
        }

        private readonly Window _window;
        private CancellationTokenSource _cts = null;

        public void Start()
        {
            Stop();
            _cts = new CancellationTokenSource();
            Task.Run(() => ObserveMouse(_cts.Token));
        }
        
        private void ObserveMouse(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                Thread.Sleep(50);
                var p = GetMousePosition();
            }
        }

        public void Stop()
        {
            _cts?.Cancel();
            _cts = null;
        }

        public void Dispose()
        {
            Stop();
        }

        #region interop

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetCursorPos(ref Win32Point pt);

        [StructLayout(LayoutKind.Sequential)]
        internal struct Win32Point
        {
            public int X;
            public int Y;
        }

        public static Point GetMousePosition()
        {
            Win32Point w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }

        #endregion

    }
}
