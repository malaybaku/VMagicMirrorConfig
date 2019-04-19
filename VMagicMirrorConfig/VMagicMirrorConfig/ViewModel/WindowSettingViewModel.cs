using System.Windows.Media;
using System.Xml.Serialization;

namespace Baku.VMagicMirrorConfig
{
    //モデルが小さいのでVMと完全癒着してる点に注意(VMというよりNotifiableなモデル)
    public class WindowSettingViewModel : SettingViewModelBase
    {
        public WindowSettingViewModel() : base() { }
        internal WindowSettingViewModel(IMessageSender sender, StartupSettingViewModel startup) : base(sender, startup)
        {
            UpdateBackgroundColor();
        }

        protected override string SaveDialogTitle => "Save Background Setting File";
        protected override string LoadDialogTitle => "Open Background Setting File";
        protected override string FileIoDialogFilter => "VMagicMirror Background File(*.vmm_background)|*.vmm_background";
        protected override string FileExt => ".vmm_background";


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

        private bool _topMost = false;
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

        //ウィンドウ初期配置周り
        //ウィンドウは軽率に動かすと怖いので起動時かボタン押したときしか動かさない！
        private bool _enableWindowInitialPlacement = false;
        public bool EnableWindowInitialPlacement
        {
            get => _enableWindowInitialPlacement;
            set
            {
                if (SetValue(ref _enableWindowInitialPlacement, value))
                {
                    //初期設定でウィンドウを動かすには初期設定が有効じゃないとダメなのでスイッチしておく
                    if (EnableWindowInitialPlacement)
                    {
                        Startup.LoadBackgroundSetting = true;
                    }
                }
            }
        }

        private int _windowInitialPositionX = 0;
        public int WindowInitialPositionX
        {
            get => _windowInitialPositionX;
            set => SetValue(ref _windowInitialPositionX, value);
        }

        private int _windowInitialPositionY = 0;
        public int WindowInitialPositionY
        {
            get => _windowInitialPositionY;
            set => SetValue(ref _windowInitialPositionY, value);
        }

        private ActionCommand _fetchUnityWindowPositionCommand;
        public ActionCommand FetchUnityWindowPositionCommand
            => _fetchUnityWindowPositionCommand ?? (_fetchUnityWindowPositionCommand = new ActionCommand(FetchUnityWindowPosition));
        private void FetchUnityWindowPosition()
        {
            var pos = UnityWindowChecker.GetUnityWindowPosition();
            WindowInitialPositionX = pos.X;
            WindowInitialPositionY = pos.Y;
        }

        private ActionCommand _moveWindowCommand;
        public ActionCommand MoveWindowCommand
            => _moveWindowCommand ?? (_moveWindowCommand = new ActionCommand(MoveWindow));

        internal void MoveWindow()
            => SendMessage(MessageFactory.Instance.MoveWindow(
                WindowInitialPositionX,
                WindowInitialPositionY
                ));


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

        protected override void ResetToDefault()
        {
            R = 0;
            G = 255;
            B = 0;
            IsTransparent = false;
            WindowDraggable = true;
            TopMost = false;

            EnableWindowInitialPlacement = false;
            WindowInitialPositionX = 0;
            WindowInitialPositionY = 0;
        }
    }
}
