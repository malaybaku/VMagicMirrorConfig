using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;

namespace Baku.VMagicMirrorConfig
{
    using static LineParseUtils;

    //モデルが小さいのでVMと完全癒着してる点に注意(VMというよりNotifiableなモデル)
    public class WindowSettingViewModel : SettingViewModelBase
    {
        internal WindowSettingViewModel(UdpSender sender, StartupSettingViewModel startup) : base(sender, startup)
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

        public Color Color { get; private set; }

        private void UpdateBackgroundColor()
        {
            Color = IsTransparent ?
                Color.FromArgb(0, 0, 0, 0) :
                Color.FromArgb(255, (byte)R, (byte)G, (byte)B);

            if (IsTransparent)
            {
                SendMessage(UdpMessageFactory.Instance.Chromakey(0, 0, 0, 0));
            }
            else
            {
                SendMessage(UdpMessageFactory.Instance.Chromakey(255, R, G, B));
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
                            SendMessage(UdpMessageFactory.Instance.IgnoreMouse(IgnoreMouseWhenTransparent));
                        }
                    }
                    else
                    {
                        SendMessage(UdpMessageFactory.Instance.IgnoreMouse(false));
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
                    SendMessage(UdpMessageFactory.Instance.WindowDraggable(WindowDraggable));
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
                    SendMessage(UdpMessageFactory.Instance.TopMost(TopMost));
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
            => SendMessage(UdpMessageFactory.Instance.MoveWindow(
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
                    SendMessage(UdpMessageFactory.Instance.WindowFrameVisibility(!HideWindowFrame));
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
                    SendMessage(UdpMessageFactory.Instance.IgnoreMouse(IgnoreMouseWhenTransparent));
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
        }

        protected override void SaveSetting()
        {
            var dialog = new SaveFileDialog()
            {
                Title = "Save Background Setting File",
                Filter = "VMagicMirror Background File(*.vmm_background)|*.vmm_background",
                DefaultExt = ".vmm_background",
                AddExtension = true,
            };
            if (dialog.ShowDialog() == true)
            {
                SaveSetting(dialog.FileName);
            }
        }

        protected override void LoadSetting()
        {
            var dialog = new OpenFileDialog()
            {
                Title = "Open Background Setting File",
                Filter = "VMagicMirror Background File(*.vmm_background)|*.vmm_background",
                Multiselect = false,
            };
            if (dialog.ShowDialog() == true)
            {
                LoadSetting(dialog.FileName);
            }
        }

        internal override void SaveSetting(string path)
        {
            File.WriteAllLines(path, new string[]
            {
                $"{nameof(R)}:{R}",
                $"{nameof(G)}:{G}",
                $"{nameof(B)}:{B}",
                $"{nameof(IsTransparent)}:{IsTransparent}",
                $"{nameof(WindowDraggable)}:{WindowDraggable}",
                $"{nameof(TopMost)}:{TopMost}",
                $"{nameof(EnableWindowInitialPlacement)}:{EnableWindowInitialPlacement}",
                $"{nameof(WindowInitialPositionX)}:{WindowInitialPositionX}",
                $"{nameof(WindowInitialPositionY)}:{WindowInitialPositionY}"
            });
        }

        internal override void LoadSetting(string path)
        {
            if (!File.Exists(path))
            {
                return;
            }

            try
            {
                var lines = File.ReadAllLines(path);
                foreach (var line in lines)
                {
                    //戻り値を拾うのはシンタックス対策
                    var _ =
                        TryReadIntParam(line, nameof(R), v => R = v) ||
                        TryReadIntParam(line, nameof(G), v => G = v) ||
                        TryReadIntParam(line, nameof(B), v => B = v) ||
                        TryReadBoolParam(line, nameof(IsTransparent), v => IsTransparent = v) ||
                        TryReadBoolParam(line, nameof(TopMost), v => TopMost = v) ||
                        TryReadBoolParam(line, nameof(WindowDraggable), v => WindowDraggable = v) ||
                        TryReadBoolParam(line, nameof(EnableWindowInitialPlacement), v => EnableWindowInitialPlacement = v) ||
                        TryReadIntParam(line, nameof(WindowInitialPositionX), v => WindowInitialPositionX = v) ||
                        TryReadIntParam(line, nameof(WindowInitialPositionY), v => WindowInitialPositionY = v);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("設定の読み込みに失敗しました: " + ex.Message);
            }

        }

    }
}
