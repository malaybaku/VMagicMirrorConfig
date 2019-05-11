using System.Windows.Media;
using System.Xml.Serialization;

namespace Baku.VMagicMirrorConfig
{
    //モデルが小さいのでVMと完全癒着してる点に注意(VMというよりNotifiableなモデル)
    public class WindowSettingViewModel : SettingViewModelBase
    {
        public WindowSettingViewModel() : base() { }
        internal WindowSettingViewModel(IMessageSender sender) : base(sender)
        {
            UpdateBackgroundColor();
        }

        private int _r = 0;
        public int R
        {
            get => _r;
            set
            {
                if (SetValue(ref _r, value))
                {
                    UpdateBackgroundColor();
                    RaisePropertyChanged(nameof(Color));
                }
            }
        }

        private int _g = 255;
        public int G
        {
            get => _g;
            set
            {
                if (SetValue(ref _g, value))
                {
                    UpdateBackgroundColor();
                    RaisePropertyChanged(nameof(Color));
                }
            }
        }

        private int _b = 0;
        public int B
        {
            get => _b;
            set
            {
                if (SetValue(ref _b, value))
                {
                    UpdateBackgroundColor();
                    RaisePropertyChanged(nameof(Color));
                }
            }
        }

        [XmlIgnore]
        public Color Color { get; private set; }

        private void UpdateBackgroundColor()
        {
            Color = IsTransparent ?
                Color.FromArgb(0, 0, 0, 0) :
                Color.FromArgb(255, (byte)R, (byte)G, (byte)B);

            if (IsTransparent)
            {
                SendMessage(MessageFactory.Instance.Chromakey(0, 0, 0, 0));
            }
            else
            {
                SendMessage(MessageFactory.Instance.Chromakey(255, R, G, B));
            }
        }

        private bool _isTransparent = false;
        public bool IsTransparent
        {
            get => _isTransparent;
            set
            {
                if (SetValue(ref _isTransparent, value))
                {
                    HideWindowFrame = IsTransparent;
                    UpdateBackgroundColor();
                    if (IsTransparent)
                    {
                        HideWindowFrame = true;
                        if (IgnoreMouseWhenTransparent)
                        {
                            SendMessage(MessageFactory.Instance.IgnoreMouse(IgnoreMouseWhenTransparent));
                        }
                    }
                    else
                    {
                        SendMessage(MessageFactory.Instance.IgnoreMouse(false));
                    }
                }
            }
        }

        private bool _windowDraggable = true;
        public bool WindowDraggable
        {
            get => _windowDraggable;
            set
            {
                if (SetValue(ref _windowDraggable, value))
                {
                    IgnoreMouseWhenTransparent = !WindowDraggable;
                    SendMessage(MessageFactory.Instance.WindowDraggable(WindowDraggable));
                }
            }
        }

        private bool _topMost = true;
        public bool TopMost
        {
            get => _topMost;
            set
            {
                if (SetValue(ref _topMost, value))
                {
                    SendMessage(MessageFactory.Instance.TopMost(TopMost));
                }
            }
        }

        private bool _hasValidWindowInitialPosition = false;
        public bool HasValidWindowInitialPosition
        {
            get => _hasValidWindowInitialPosition;
            set => SetValue(ref _hasValidWindowInitialPosition, value);
        }

        private int _windowInitialPositionX = 0;
        public int WindowInitialPositionX
        {
            get => _windowInitialPositionX;
            set
            {
                if (SetValue(ref _windowInitialPositionX, value))
                {
                    HasValidWindowInitialPosition = true;
                }
            }
        }

        private int _windowInitialPositionY = 0;
        public int WindowInitialPositionY
        {
            get => _windowInitialPositionY;
            set
            {
                if (SetValue(ref _windowInitialPositionY, value))
                {
                    HasValidWindowInitialPosition = true;
                }
            }
        }
        
        private ActionCommand _resetWindowPositionCommand;
        public ActionCommand ResetWindowPositionCommand
            => _resetWindowPositionCommand ?? (_resetWindowPositionCommand = new ActionCommand(ResetWindowPosition));

        private void ResetWindowPosition()
        {
            //NOTE: ウィンドウが被ると困るのを踏まえ、すぐ上ではなく右わきに寄せる点にご注目
            var pos = WindowPositionUtil.GetThisWindowRightTopPosition();
            WindowInitialPositionX = pos.X;
            WindowInitialPositionY = pos.Y;
            //Unity側が初期解像度として800 x 600にしているので合わせる。ウィンドウを小さくし過ぎた場合に復帰してほしいので、それもかねて。
            MoveWindow(800, 600);
        }

        internal void FetchUnityWindowPosition()
        {
            var rect = WindowPositionUtil.GetUnityWindowRect();
            WindowInitialPositionX = rect.X;
            WindowInitialPositionY = rect.Y;
        }

        internal void MoveWindow()
        {
            if (HasValidWindowInitialPosition)
            {
                var rect = WindowPositionUtil.GetUnityWindowRect();
                MoveWindow(rect.Width, rect.Height);
            }
        }

        private void MoveWindow(int width, int height)
        {
            WindowPositionUtil.SetUnityWindowRect(
                WindowInitialPositionX,
                WindowInitialPositionY,
                width,
                height
                );
        }

        #region privateになったプロパティ

        private bool _hideWindowFrame = false;
        private bool HideWindowFrame
        {
            get => _hideWindowFrame;
            set
            {
                if (SetValue(ref _hideWindowFrame, value))
                {
                    SendMessage(MessageFactory.Instance.WindowFrameVisibility(!HideWindowFrame));
                }
            }
        }

        private bool _ignoreMouseWhenTransparent = false;
        private bool IgnoreMouseWhenTransparent
        {
            get => _ignoreMouseWhenTransparent;
            set
            {
                //すでに透明だったときだけ処理する(不透明時はIsTransparentが変わったタイミングでメッセージが飛ぶ)
                if (SetValue(ref _ignoreMouseWhenTransparent, value) &&
                    IsTransparent)
                {
                    SendMessage(MessageFactory.Instance.IgnoreMouse(IgnoreMouseWhenTransparent));
                }
            }
        }

        #endregion

        public override void ResetToDefault()
        {
            R = 0;
            G = 255;
            B = 0;
            IsTransparent = false;
            WindowDraggable = true;
            TopMost = true;

            //このリセットはあまり定数的ではないことに注意！
            ResetWindowPosition();
        }
    }
}
