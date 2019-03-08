using System;
using System.Threading;
using System.Threading.Tasks;

namespace Baku.VMagicMirrorConfig
{
    class InputChecker : IDisposable
    {
        private readonly static int IntervalMillisec = 30;

        public int X { get; private set; } = 0;
        public int Y { get; private set; } = 0;
        public string KeyCode { get; private set; } = "";

        private CancellationTokenSource _cts = null;

        private readonly MouseHook _mouseHook = new MouseHook();
        private readonly KeyboardHook _keyboardHook = new KeyboardHook();

        //遅いポーリングをする
        //  マウスイベントは物凄い回数呼ばれるはずなので通信頻度下げるのが狙い
        //  キーボードは相対的に遅いハズなので適当に。
        public void Start()
        {
            Stop();
            _cts = new CancellationTokenSource();

            _mouseHook.SetHook();
            _keyboardHook.KeyboardHooked += OnKeyboardHookEvent;

            //不要: マウスの位置はUnityが自力で追えそうなのでいったん忘れる
            //Task.Run(async () => await CheckAndRaiseEventsAsync(_cts.Token));
        }

        private void OnKeyboardHookEvent(object sender, KeyboardHookedEventArgs e)
        {
            if (e.UpDown == KeyboardUpDown.Down)
            {
                KeyCode = e.KeyCode.ToString();
                KeyDown?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Stop() 
        {
            _cts?.Cancel();
            _cts = null;

            _keyboardHook.KeyboardHooked -= OnKeyboardHookEvent;
            _mouseHook.RemoveHook();
        }

        private async Task CheckAndRaiseEventsAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(IntervalMillisec);

                int x = _mouseHook.X;
                int y = _mouseHook.Y;

                if (X != x || Y != y)
                {
                    X = x;
                    Y = y;
                    MouseMoved?.Invoke(this, EventArgs.Empty);
                }

            }
        }

        public void Dispose()
        {
            Stop();
            _keyboardHook.KeyboardHooked -= OnKeyboardHookEvent;
            _mouseHook.Dispose();
        }

        public event EventHandler<MouseButtonEventArgs> MouseButton
        {
            add { _mouseHook.MouseButton += value; }
            remove { _mouseHook.MouseButton -= value; }
        }
        public event EventHandler MouseMoved;
        public event EventHandler KeyDown;

    }
}
