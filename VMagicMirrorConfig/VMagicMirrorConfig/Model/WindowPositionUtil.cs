using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace Baku.VMagicMirrorConfig
{
    public static class WindowPositionUtil
    {
        public static WindowRect GetUnityWindowRect()
        {
            GetWindowRect(GetUnityWindowHandle(), out RECT rect);
            return new WindowRect(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
        }

        public static WindowPosition GetThisWindowRightTopPosition()
        {
            GetWindowRect(Process.GetCurrentProcess().MainWindowHandle, out RECT rect);
            return new WindowPosition(rect.right, rect.top);
        }

        public static void SetUnityWindowRect(int x, int y, int width, int height)
        {
            //念のため: WidthもHeightも普通は非ゼロ値が入る
            if (width > 0 && height > 0)
            {
                MoveWindow(GetUnityWindowHandle(), x, y, width, height, false);
            }
        }

        private static IntPtr GetUnityWindowHandle()
            => Process.GetProcesses()
                .FirstOrDefault(p => p.ProcessName == "VMagicMirror")
                ?.MainWindowHandle
                ?? IntPtr.Zero;

        public struct WindowPosition
        {
            public WindowPosition(int x, int y)
            {
                X = x;
                Y = y;
            }
            public int X { get; }
            public int Y { get; }
        }

        public struct WindowRect
        {
            public WindowRect(int x, int y, int width, int height)
            {
                X = x;
                Y = y;
                Width = width;
                Height = height;
            }
            public int X { get; }
            public int Y { get; }
            public int Width { get; }
            public int Height { get; }

        }

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);

        [DllImport("user32.dll")]
        private static extern bool MoveWindow(IntPtr hWnd, int x, int y, int nWidth, int nHeight, bool repaint);

        [StructLayout(LayoutKind.Sequential)]
        struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }
    }
}
