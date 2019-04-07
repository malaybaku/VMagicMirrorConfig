using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace Baku.VMagicMirrorConfig
{
    using System.Linq;
    using static LineParseUtils;

    public class LayoutSettingViewModel : SettingViewModelBase
    {
        internal LayoutSettingViewModel(IMessageSender sender, StartupSettingViewModel startup) : base(sender, startup)
        {
            Gamepad = new GamepadSettingViewModel(sender, startup);
        }

        public GamepadSettingViewModel Gamepad { get; private set; }

        #region Properties

        private int _lengthFromWristToTip = 18;
        /// <summary> Unit: [cm] </summary>
        public int LengthFromWristToTip
        {
            get => _lengthFromWristToTip;
            set
            {
                if (SetValue(ref _lengthFromWristToTip, value))
                {
                    SendMessage(MessageFactory.Instance.LengthFromWristToTip(LengthFromWristToTip));
                }
            }
        }

        private int _lengthFromWristToPalm = 9;
        /// <summary> Unit: [cm] </summary>
        public int LengthFromWristToPalm
        {
            get => _lengthFromWristToPalm;
            set
            {
                if (SetValue(ref _lengthFromWristToPalm, value))
                {
                    SendMessage(MessageFactory.Instance.LengthFromWristToPalm(LengthFromWristToPalm));
                }
            }
        }

        private int _handYOffsetBasic = 4;
        public int HandYOffsetBasic
        {
            get => _handYOffsetBasic;
            set
            {
                if (SetValue(ref _handYOffsetBasic, value))
                {
                    SendMessage(MessageFactory.Instance.HandYOffsetBasic(HandYOffsetBasic));
                }
            }
        }

        private int _handYOffsetAfterKeyDown = 2;
        public int HandYOffsetAfterKeyDown
        {
            get => _handYOffsetAfterKeyDown;
            set
            {
                if (SetValue(ref _handYOffsetAfterKeyDown, value))
                {
                    SendMessage(MessageFactory.Instance.HandYOffsetAfterKeyDown(HandYOffsetAfterKeyDown));
                }
            }
        }

        private bool _enableWaitMotion = true;
        public bool EnableWaitMotion
        {
            get => _enableWaitMotion;
            set
            {
                if (SetValue(ref _enableWaitMotion, value))
                {
                    SendMessage(MessageFactory.Instance.EnableWaitMotion(EnableWaitMotion));
                }
            }
        }

        private int _waitMotionScale = 100;
        public int WaitMotionScale
        {
            get => _waitMotionScale;
            set
            {
                if (SetValue(ref _waitMotionScale, value))
                {
                    SendMessage(MessageFactory.Instance.WaitMotionScale(WaitMotionScale));
                }
            }
        }

        private int _waitMotionPeriod = 10;
        public int WaitMotionPeriod
        {
            get => _waitMotionPeriod;
            set
            {
                if (SetValue(ref _waitMotionPeriod, value))
                {
                    SendMessage(MessageFactory.Instance.WaitMotionPeriod(WaitMotionPeriod));
                }
            }
        }

        private bool _enableTouchTyping = true;
        public bool EnableTouchTyping
        {
            get => _enableTouchTyping;
            set
            {
                if (SetValue(ref _enableTouchTyping, value))
                {
                    SendMessage(MessageFactory.Instance.EnableTouchTyping(EnableTouchTyping));
                }
            }
        }

        private bool _enableLipSync = true;
        public bool EnableLipSync
        {
            get => _enableLipSync;
            set
            {
                if (SetValue(ref _enableLipSync, value))
                {
                    SendMessage(MessageFactory.Instance.EnableLipSync(EnableLipSync));
                }
            }
        }

        private int _cameraHeight = 120;
        /// <summary> Unit: [cm] </summary>
        public int CameraHeight
        {
            get => _cameraHeight;
            set
            {
                if (SetValue(ref _cameraHeight, value))
                {
                    SendMessage(MessageFactory.Instance.CameraHeight(CameraHeight));
                }
            }
        }

        private int _cameraDistance = 100;
        /// <summary> Unit: [cm] </summary>
        public int CameraDistance
        {
            get => _cameraDistance;
            set
            {
                if (SetValue(ref _cameraDistance, value))
                {
                    SendMessage(MessageFactory.Instance.CameraDistance(CameraDistance));
                }
            }
        }

        private int _cameraVerticalAngle = 0;
        /// <summary> Unit: [cm] </summary>
        public int CameraVerticalAngle
        {
            get => _cameraVerticalAngle;
            set
            {
                if (SetValue(ref _cameraVerticalAngle, value))
                {
                    SendMessage(MessageFactory.Instance.CameraVerticalAngle(CameraVerticalAngle));
                }
            }
        }

        private int _hidHeight = 100;
        /// <summary> Unit: [cm] </summary>
        public int HidHeight
        {
            get => _hidHeight;
            set
            {
                if (SetValue(ref _hidHeight, value))
                {
                    SendMessage(MessageFactory.Instance.HidHeight(HidHeight));
                }
            }
        }

        private int _hidHorizontalScale = 100;
        /// <summary> Unit: [%] </summary>
        public int HidHorizontalScale
        {
            get => _hidHorizontalScale;
            set
            {
                if (SetValue(ref _hidHorizontalScale, value))
                {
                    SendMessage(MessageFactory.Instance.HidHorizontalScale(HidHorizontalScale));
                }
            }
        }

        private bool _hidVisibility = true;
        /// <summary> Centimeter </summary>
        public bool HidVisibility
        {
            get => _hidVisibility;
            set
            {
                if (SetValue(ref _hidVisibility, value))
                {
                    SendMessage(MessageFactory.Instance.HidVisibility(HidVisibility));
                }
            }
        }

        #endregion

        protected override void ResetToDefault()
        {
            Gamepad.ResetToDefaultCommand?.Execute(null);

            LengthFromWristToTip = 18;
            LengthFromWristToPalm = 9;

            HandYOffsetBasic = 4;
            HandYOffsetAfterKeyDown = 2;

            EnableWaitMotion = true;
            WaitMotionScale = 100;
            WaitMotionPeriod = 10;

            EnableTouchTyping = true;
            EnableLipSync = true;


            CameraHeight = 120;
            CameraDistance = 100;
            CameraVerticalAngle = 0;

            HidHeight = 100;
            HidHorizontalScale = 100;
            HidVisibility = true;
        }

        protected override void SaveSetting()
        {
            var dialog = new SaveFileDialog()
            {
                Title = "Save Layout Setting File",
                Filter = "VMagicMirror Layout File(*.vmm_layout)|*.vmm_layout",
                DefaultExt = ".vmm_layout",
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
                Title = "Open Layout Setting File",
                Filter = "VMagicMirror Layout File(*.vmm_layout)|*.vmm_layout",
                Multiselect = false,
            };
            if (dialog.ShowDialog() == true)
            {
                LoadSetting(dialog.FileName);
            }
        }

        internal override void SaveSetting(string path)
        {
            var lines = new string[]
            {
                $"{nameof(LengthFromWristToTip)}:{LengthFromWristToTip}",
                $"{nameof(LengthFromWristToPalm)}:{LengthFromWristToPalm}",
                $"{nameof(HandYOffsetBasic)}:{HandYOffsetBasic}",
                $"{nameof(HandYOffsetAfterKeyDown)}:{HandYOffsetAfterKeyDown}",

                $"{nameof(EnableWaitMotion)}:{EnableWaitMotion}",
                $"{nameof(WaitMotionScale)}:{WaitMotionScale}",
                $"{nameof(WaitMotionPeriod)}:{WaitMotionPeriod}",

                $"{nameof(EnableTouchTyping)}:{EnableTouchTyping}",
                $"{nameof(EnableLipSync)}:{EnableLipSync}",

                $"{nameof(CameraHeight)}:{CameraHeight}",
                $"{nameof(CameraDistance)}:{CameraDistance}",
                $"{nameof(CameraVerticalAngle)}:{CameraVerticalAngle}",
                $"{nameof(HidHeight)}:{HidHeight}",
                $"{nameof(HidHorizontalScale)}:{HidHorizontalScale}",
                $"{nameof(HidVisibility)}:{HidVisibility}",
            }
                .Concat(Gamepad.GetLinesToSave())
                .ToArray();


            File.WriteAllLines(path, lines);
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
                        TryReadIntParam(line, nameof(LengthFromWristToTip), v => LengthFromWristToTip = v) ||
                        TryReadIntParam(line, nameof(LengthFromWristToPalm), v => LengthFromWristToPalm = v) ||
                        TryReadIntParam(line, nameof(HandYOffsetBasic), v => HandYOffsetBasic = v) ||
                        TryReadIntParam(line, nameof(HandYOffsetAfterKeyDown), v => HandYOffsetAfterKeyDown = v) ||

                        TryReadBoolParam(line, nameof(EnableWaitMotion), v => EnableWaitMotion = v) ||
                        TryReadIntParam(line, nameof(WaitMotionScale), v => WaitMotionScale = v) ||
                        TryReadIntParam(line, nameof(WaitMotionPeriod), v => WaitMotionPeriod = v) ||

                        TryReadBoolParam(line, nameof(EnableTouchTyping), v => EnableTouchTyping = v) ||
                        TryReadBoolParam(line, nameof(EnableLipSync), v => EnableLipSync = v) ||

                        TryReadIntParam(line, nameof(CameraHeight), v => CameraHeight = v) ||
                        TryReadIntParam(line, nameof(CameraDistance), v => CameraDistance = v) ||
                        TryReadIntParam(line, nameof(CameraVerticalAngle), v => CameraVerticalAngle = v) ||

                        TryReadIntParam(line, nameof(HidHeight), v => HidHeight = v) ||
                        TryReadIntParam(line, nameof(HidHorizontalScale), v => HidHorizontalScale = v) ||
                        TryReadBoolParam(line, nameof(HidVisibility), v => HidVisibility = v);
                }

                Gamepad.ParseLines(lines);

            }
            catch (Exception ex)
            {
                MessageBox.Show("レイアウト設定の読み込みに失敗しました: " + ex.Message);
            }

        }

    }
}
