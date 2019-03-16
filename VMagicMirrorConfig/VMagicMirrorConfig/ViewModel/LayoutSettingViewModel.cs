using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;

namespace Baku.VMagicMirrorConfig
{
    using static LineParseUtils;

    public class LayoutSettingViewModel : SettingViewModelBase
    {
        internal LayoutSettingViewModel(UdpSender sender, StartupSettingViewModel startup) : base(sender, startup)
        {
        }

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
                    SendMessage(UdpMessageFactory.Instance.LengthFromWristToTip(LengthFromWristToTip));
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
                    SendMessage(UdpMessageFactory.Instance.LengthFromWristToPalm(LengthFromWristToPalm));
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
                    SendMessage(UdpMessageFactory.Instance.HandYOffsetBasic(HandYOffsetBasic));
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
                    SendMessage(UdpMessageFactory.Instance.HandYOffsetAfterKeyDown(HandYOffsetAfterKeyDown));
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
                    SendMessage(UdpMessageFactory.Instance.EnableWaitMotion(EnableWaitMotion));
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
                    SendMessage(UdpMessageFactory.Instance.WaitMotionScale(WaitMotionScale));
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
                    SendMessage(UdpMessageFactory.Instance.WaitMotionPeriod(WaitMotionPeriod));
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
                    SendMessage(UdpMessageFactory.Instance.EnableTouchTyping(EnableTouchTyping));
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
                    SendMessage(UdpMessageFactory.Instance.CameraHeight(CameraHeight));
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
                    SendMessage(UdpMessageFactory.Instance.CameraDistance(CameraDistance));
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
                    SendMessage(UdpMessageFactory.Instance.CameraVerticalAngle(CameraVerticalAngle));
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
                    SendMessage(UdpMessageFactory.Instance.HidHeight(HidHeight));
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
                    SendMessage(UdpMessageFactory.Instance.HidHorizontalScale(HidHorizontalScale));
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
                    SendMessage(UdpMessageFactory.Instance.HidVisibility(HidVisibility));
                }
            }
        }

        #endregion

        protected override void ResetToDefault()
        {
            LengthFromWristToTip = 18;
            LengthFromWristToPalm = 9;

            HandYOffsetBasic = 4;
            HandYOffsetAfterKeyDown = 2;

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
            File.WriteAllLines(path, new string[]
            {
                $"{nameof(LengthFromWristToTip)}:{LengthFromWristToTip}",
                $"{nameof(LengthFromWristToPalm)}:{LengthFromWristToPalm}",
                $"{nameof(HandYOffsetBasic)}:{HandYOffsetBasic}",
                $"{nameof(HandYOffsetAfterKeyDown)}:{HandYOffsetAfterKeyDown}",

                $"{nameof(EnableWaitMotion)}:{EnableWaitMotion}",
                $"{nameof(WaitMotionScale)}:{WaitMotionScale}",
                $"{nameof(WaitMotionPeriod)}:{WaitMotionPeriod}",

                $"{nameof(EnableTouchTyping)}:{EnableTouchTyping}",

                $"{nameof(CameraHeight)}:{CameraHeight}",
                $"{nameof(CameraDistance)}:{CameraDistance}",
                $"{nameof(CameraVerticalAngle)}:{CameraVerticalAngle}",
                $"{nameof(HidHeight)}:{HidHeight}",
                $"{nameof(HidHorizontalScale)}:{HidHorizontalScale}",
                $"{nameof(HidVisibility)}:{HidVisibility}",
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
                foreach(var line in lines)
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

                        TryReadIntParam(line, nameof(CameraHeight), v => CameraHeight = v) ||
                        TryReadIntParam(line, nameof(CameraDistance), v => CameraDistance = v) ||
                        TryReadIntParam(line, nameof(CameraVerticalAngle), v => CameraVerticalAngle = v) ||

                        TryReadIntParam(line, nameof(HidHeight), v => HidHeight = v) ||
                        TryReadIntParam(line, nameof(HidHorizontalScale), v => HidHorizontalScale = v) ||
                        TryReadBoolParam(line, nameof(HidVisibility), v => HidVisibility = v);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("レイアウト設定の読み込みに失敗しました: " + ex.Message);
            }

        }

    }
}
