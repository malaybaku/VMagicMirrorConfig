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
        }

        public void Initialize()
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
                    UpdatePickerColor();
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
                    UpdatePickerColor();
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
                    UpdatePickerColor();
                    UpdateBackgroundColor();
                    RaisePropertyChanged(nameof(Color));
                }
            }
        }

        private Color _pickerColor = Color.FromRgb(0, 255, 0);
        /// <summary>
        /// ColorPickerに表示する、Alphaを考慮しない背景色を取得、設定します。
        /// </summary>
        [XmlIgnore]
        public Color PickerColor
        {
            get => _pickerColor;
            set
            {
                if (SetValue(ref _pickerColor, value))
                {
                    R = PickerColor.R;
                    G = PickerColor.G;
                    B = PickerColor.B;
                }
            }
        }

        [XmlIgnore]
        public Color Color { get; private set; }

        private void UpdatePickerColor() => PickerColor = Color.FromRgb((byte)R, (byte)G, (byte)B);
        
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

        private ActionCommand? _resetWindowPositionCommand;
        public ActionCommand ResetWindowPositionCommand
            => _resetWindowPositionCommand ??= new ActionCommand(ResetWindowPosition);

        private void ResetWindowPosition()
        {
            //NOTE: ウィンドウが被ると困るのを踏まえ、すぐ上ではなく右わきに寄せる点にご注目
            var pos = WindowPositionUtil.GetThisWindowRightTopPosition();
            SendMessage(MessageFactory.Instance.MoveWindow(pos.X, pos.Y));
            SendMessage(MessageFactory.Instance.ResetWindowSize());
        }


        private int _wholeWindowTransparencyLevel = 2;
        public int WholeWindowTransparencyLevel
        {
            get => _wholeWindowTransparencyLevel;
            set
            {
                if (SetValue(ref _wholeWindowTransparencyLevel, value))
                {
                    SendMessage(
                        MessageFactory.Instance.SetWholeWindowTransparencyLevel(_wholeWindowTransparencyLevel)
                        );
                }
            }
        }

        private int _alphaValueOnTransparent = 128;
        public int AlphaValueOnTransparent
        {
            get => _alphaValueOnTransparent;
            set
            {
                if (SetValue(ref _alphaValueOnTransparent, value))
                {
                    SendMessage(
                        MessageFactory.Instance.SetAlphaValueOnTransparent(_alphaValueOnTransparent)
                        );
                }
            }
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

        #region Reset API

        private ActionCommand? _resetBackgroundColorSettingCommand = null;
        public ActionCommand ResetBackgroundColorSettingCommand
            => _resetBackgroundColorSettingCommand ??= new ActionCommand(
                () => SettingResetUtils.ResetSingleCategoryAsync(ResetBackgroundColor)
                );

        private void ResetBackgroundColor()
        {
            R = 0;
            G = 255;
            B = 0;
        }

        private ActionCommand? _resetOpacitySettingCommand = null;
        public ActionCommand ResetOpacitySettingCommand
            => _resetOpacitySettingCommand ??= new ActionCommand(
                () => SettingResetUtils.ResetSingleCategoryAsync(ResetOpacity)
                );

        private void ResetOpacity()
        {
            WholeWindowTransparencyLevel = 2;
            AlphaValueOnTransparent = 128;
        }


        #endregion

        public override void ResetToDefault()
        {
            ResetBackgroundColor();

            IsTransparent = false;
            WindowDraggable = true;
            TopMost = true;

            ResetOpacity();

            //このリセットはあまり定数的ではないことに注意！
            ResetWindowPosition();
        }

        #region Unity側にUIが出っぱなしの状態でexeを切ったときの復帰用API

        /// <summary>
        /// <see cref="TopMost"/>の値を変更しますが、プロセス間通信は行いません。
        /// </summary>
        /// <param name="topMost"></param>
        public void SilentSetTopMost(bool topMost)
            => SetValue(ref _topMost, topMost, nameof(TopMost));

        #endregion
    }
}
