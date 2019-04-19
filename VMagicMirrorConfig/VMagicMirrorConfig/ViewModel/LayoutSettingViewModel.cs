using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace Baku.VMagicMirrorConfig
{
    public class LayoutSettingViewModel : SettingViewModelBase
    {
        public LayoutSettingViewModel() : base() { }
        internal LayoutSettingViewModel(IMessageSender sender, StartupSettingViewModel startup) : base(sender, startup)
        {
            Gamepad = new GamepadSettingViewModel(sender, startup);
        }

        /// <summary>
        /// </summary>
        /// <remarks>
        /// Setterがpublicなのはシリアライザのご機嫌取ってるだけなので普通のコードでは触らない事！
        /// </remarks>
        public GamepadSettingViewModel Gamepad { get; set; }

        public async Task InitializeAvailableMicrophoneNamesAsync()
        {
            string result = await SendQueryAsync(MessageFactory.Instance.MicrophoneDeviceNames());
            Application.Current.MainWindow.Dispatcher.Invoke(() => 
            {
                _writableMicrophoneDeviceNames.Clear();
                foreach (var deviceName in result.Split('\t'))
                {
                    _writableMicrophoneDeviceNames.Add(deviceName);
                }
            });
        }

        protected override string SaveDialogTitle => "Save Layout Setting File";
        protected override string LoadDialogTitle => "Open Layout Setting File";
        protected override string FileIoDialogFilter => "VMagicMirror Layout File(*.vmm_layout)|*.vmm_layout";
        protected override string FileExt => ".vmm_layout";

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

        private bool _enableLipSync = false;
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

        private string _lipSyncMicrophoneDeviceName = "";
        public string LipSyncMicrophoneDeviceName
        {
            get => _lipSyncMicrophoneDeviceName;
            set
            {
                if (SetValue(ref _lipSyncMicrophoneDeviceName, value))
                {
                    SendMessage(MessageFactory.Instance.SetMicrophoneDeviceName(LipSyncMicrophoneDeviceName));
                }
            }
        }

        private readonly ObservableCollection<string> _writableMicrophoneDeviceNames 
            = new ObservableCollection<string>();
        private ReadOnlyObservableCollection<string> _microphoneDeviceNames = null;
        [XmlIgnore]
        public ReadOnlyObservableCollection<string> MicrophoneDeviceNames
            => _microphoneDeviceNames ?? 
            (_microphoneDeviceNames = new ReadOnlyObservableCollection<string>(_writableMicrophoneDeviceNames));

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
            LipSyncMicrophoneDeviceName = "";

            CameraHeight = 120;
            CameraDistance = 100;
            CameraVerticalAngle = 0;

            HidHeight = 100;
            HidHorizontalScale = 100;
            HidVisibility = true;
        }
    }
}
