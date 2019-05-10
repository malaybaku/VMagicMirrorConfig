using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace Baku.VMagicMirrorConfig
{
    public static class WindowPositionUtil
    {
        public static WindowPosition GetUnityWindowPosition()
        {
            GetWindowRect(GetUnityWindowHandle(), out RECT rect);
            return new WindowPosition(rect.left, rect.top);
        }

        public static WindowPosition GetThisWindowRightTopPosition()
        {
            GetWindowRect(Process.GetCurrentProcess().MainWindowHandle, out RECT rect);
            return new WindowPosition(rect.right, rect.top);
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

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT rect);

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
