using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Baku.VMagicMirrorConfig
{
    class MouseHook : IDisposable
    {
        private IntPtr hHook;
        //明示的に参照保持しないとデリゲートがGCされてしまうのでわざわざ参照を持つ(アンマネージ感がすごい)
        private WindowsAPI.HOOKPROC _hookProc;

        private readonly object _posXLock = new object();
        private int _x = 0;
        public int X
        {
            get { lock (_posXLock) return _x; }
            set { lock (_posXLock) _x = value; }
        }

        private readonly object _posYLock = new object();
        private int _y = 0;
        public int Y
        {
            get { lock (_posYLock) return _y; }
            set { lock (_posYLock) _y = value; }
        }

        private readonly object _mouseMessagesLock = new object();
        private readonly Queue<int> _mouseMessages = new Queue<int>();
        private void EnqueueMouseMessage(int msg)
        {
            lock (_mouseMessagesLock)
            {
                _mouseMessages.Enqueue(msg);
            }
        }

        public int DequeueMouseMessage()
        {
            lock (_mouseMessagesLock)
            {
                if (_mouseMessages.Count > 0)
                {
                    return _mouseMessages.Dequeue();
                }
                else
                {
                    return 0;
                }
            }
        }

        public bool HasMouseMessage()
        {
            lock (_mouseMessagesLock)
            {
                return (_mouseMessages.Count > 0);
            }
        }

        public MouseHook()
        {
            _hookProc = HookProc;
        }

        public bool SetHook()
        {
            var hModule = WindowsAPI.GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName);
            hHook = WindowsAPI.SetWindowsHookEx(
                (int)WindowsAPI.HookType.WH_MOUSE_LL,
                _hookProc, 
                hModule,
                IntPtr.Zero
                );

            return (hHook != IntPtr.Zero);
        }

        private IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode == WindowsAPI.HC_ACTION)
            {
                try
                {
                    var mouseHook = Marshal.PtrToStructure<WindowsAPI.MSLLHOOKSTRUCT>(lParam);
                    X = mouseHook.pt.x;
                    Y = mouseHook.pt.y;


                    int wParamVal = wParam.ToInt32();

                    string info =
                        (wParamVal == WindowsAPI.MouseMessages.WM_LBUTTONDOWN) ? "LDown" :
                        (wParamVal == WindowsAPI.MouseMessages.WM_LBUTTONUP) ? "LUp" :
                        (wParamVal == WindowsAPI.MouseMessages.WM_RBUTTONDOWN) ? "RDown" :
                        (wParamVal == WindowsAPI.MouseMessages.WM_RBUTTONUP) ? "RUp" :
                        (wParamVal == WindowsAPI.MouseMessages.WM_MBUTTONDOWN) ? "MDown" :
                        (wParamVal == WindowsAPI.MouseMessages.WM_MBUTTONUP) ? "MUp" :
                        "";

                    if (!string.IsNullOrEmpty(info))
                    {
                        MouseButton?.Invoke(this, new MouseButtonEventArgs(info));
                    }
                }
                catch (Exception ex)
                {
                    Debug.Write(ex.Message);
                }
            }

            return WindowsAPI.CallNextHookEx(hHook, nCode, wParam, lParam);
        }

        public void RemoveHook() => WindowsAPI.UnhookWindowsHookEx(hHook);

        public void Dispose() => RemoveHook();

        public event EventHandler<MouseButtonEventArgs> MouseButton;
    }

    class MouseButtonEventArgs : EventArgs
    {
        public MouseButtonEventArgs(string info)
        {
            Info = info;
        }
        public string Info { get; }
    }

}
