using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace Baku.VMagicMirrorConfig
{
    public class MotionSettingViewModel : SettingViewModelBase
    {
        public MotionSettingViewModel() : base() { }
        internal MotionSettingViewModel(IMessageSender sender, IMessageReceiver receiver) : base(sender)
        {
            receiver.ReceivedCommand += OnReceivedCommand;
        }

        private void OnReceivedCommand(object sender, CommandReceivedEventArgs e)
        {
            if (e.Command == ReceiveMessageNames.SetCalibrationFaceData)
            {
                CalibrateFaceData = e.Args;
            }
        }

        public async Task InitializeDeviceNamesAsync()
        {
            string microphones = await SendQueryAsync(MessageFactory.Instance.MicrophoneDeviceNames());
            Application.Current.MainWindow.Dispatcher.Invoke(() =>
            {
                _writableMicrophoneDeviceNames.Clear();
                foreach (var deviceName in microphones.Split('\t'))
                {
                    _writableMicrophoneDeviceNames.Add(deviceName);
                }
            });

            string cameras = await SendQueryAsync(MessageFactory.Instance.CameraDeviceNames());
            Application.Current.MainWindow.Dispatcher.Invoke(() =>
            {
                _writableCameraDeviceNames.Clear();
                foreach (var deviceName in cameras.Split('\t'))
                {
                    _writableCameraDeviceNames.Add(deviceName);
                }
            });
        }

        #region Face

        private bool _enableFaceTracking = true;
        public bool EnableFaceTracking
        {
            get => _enableFaceTracking;
            set
            {
                if (SetValue(ref _enableFaceTracking, value))
                {
                    SendMessage(MessageFactory.Instance.EnableFaceTracking(EnableFaceTracking));
                }
            }
        }

        private string _cameraDeviceName = "";
        public string CameraDeviceName
        {
            get => _cameraDeviceName;
            set
            {
                if (SetValue(ref _cameraDeviceName, value))
                {
                    SendMessage(MessageFactory.Instance.SetCameraDeviceName(CameraDeviceName));
                }
            }
        }

        private readonly ObservableCollection<string> _writableCameraDeviceNames
            = new ObservableCollection<string>();
        private ReadOnlyObservableCollection<string> _cameraDeviceNames = null;
        [XmlIgnore]
        public ReadOnlyObservableCollection<string> CameraDeviceNames
            => _cameraDeviceNames ??
            (_cameraDeviceNames = new ReadOnlyObservableCollection<string>(_writableCameraDeviceNames));

        private ActionCommand _calibrateFaceCommand;
        public ActionCommand CalibrateFaceCommand
            => _calibrateFaceCommand ?? (_calibrateFaceCommand = new ActionCommand(CalibrateFace));
        
        private void CalibrateFace()
        {
            SendMessage(MessageFactory.Instance.CalibrateFace());
        }

        private string _calibrateFaceData = "";
        /// <summary>
        /// NOTE: この値はUIに出す必要はないが、起動時に空でなければ送り、Unityからデータが来たら受け取り、終了時にはセーブする。
        /// </summary>
        public string CalibrateFaceData
        {
            get => _calibrateFaceData;
            set => SetValue(ref _calibrateFaceData, value);
        }

        public void SendCalibrateFaceData()
        {
            if (string.IsNullOrWhiteSpace(CalibrateFaceData))
            {
                return;
            }

            SendMessage(MessageFactory.Instance.SetCalibrateFaceData(CalibrateFaceData));
        }

        #endregion

        #region Eye

        private bool _useLookAtPointNone = false;
        public bool UseLookAtPointNone
        {
            get => _useLookAtPointNone;
            set
            {
                if (SetValue(ref _useLookAtPointNone, value) && value)
                {
                    SendMessage(MessageFactory.Instance.LookAtStyle(nameof(UseLookAtPointNone)));
                    UseLookAtPointMousePointer = false;
                    UseLookAtPointMainCamera = false;
                }
            }
        }

        private bool _useLookAtPointMousePointer = true;
        public bool UseLookAtPointMousePointer
        {
            get => _useLookAtPointMousePointer;
            set
            {
                if (SetValue(ref _useLookAtPointMousePointer, value) && value)
                {
                    SendMessage(MessageFactory.Instance.LookAtStyle(nameof(UseLookAtPointMousePointer)));
                    UseLookAtPointNone = false;
                    UseLookAtPointMainCamera = false;
                }
            }
        }

        private bool _useLookAtPointMainCamera = false;
        public bool UseLookAtPointMainCamera
        {
            get => _useLookAtPointMainCamera;
            set
            {
                if (SetValue(ref _useLookAtPointMainCamera, value) && value)
                {
                    SendMessage(MessageFactory.Instance.LookAtStyle(nameof(UseLookAtPointMainCamera)));
                    UseLookAtPointNone = false;
                    UseLookAtPointMousePointer = false;
                }
            }
        }

        #endregion

        #region Mouth

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

        #endregion

        #region Arm

        private int _waistWidth = 30;
        public int WaistWidth
        {
            get => _waistWidth;
            set
            {
                if (SetValue(ref _waistWidth, value))
                {
                    SendMessage(MessageFactory.Instance.SetWaistWidth(WaistWidth));
                }
            }
        }

        private int _elbowCloseStrength = 30;
        public int ElbowCloseStrength
        {
            get => _elbowCloseStrength;
            set
            {
                if (SetValue(ref _elbowCloseStrength, value))
                {
                    SendMessage(MessageFactory.Instance.SetElbowCloseStrength(ElbowCloseStrength));
                }
            }
        }

        private bool _enablePresenterMotion = false;
        public bool EnablePresenterMotion
        {
            get => _enablePresenterMotion;
            set
            {
                if (SetValue(ref _enablePresenterMotion, value))
                {
                    SendMessage(MessageFactory.Instance.EnablePresenterMotion(EnablePresenterMotion));
                }
            }
        }

        private int _presentationArmMotionScale = 30;
        public int PresentationArmMotionScale
        {
            get => _presentationArmMotionScale;
            set
            {
                if (SetValue(ref _presentationArmMotionScale, value))
                {
                    SendMessage(MessageFactory.Instance.PresentationArmMotionScale(PresentationArmMotionScale));
                }
            }
        }

        private int _presentationArmRadiusMin = 20;
        public int PresentationArmRadiusMin
        {
            get => _presentationArmRadiusMin;
            set
            {
                if (SetValue(ref _presentationArmRadiusMin, value))
                {
                    SendMessage(MessageFactory.Instance.PresentationArmRadiusMin(PresentationArmRadiusMin));
                }
            }
        }

        #endregion

        #region Hand

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

        #endregion

        #region Wait

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

        #endregion

        public override void ResetToDefault()
        {
            EnableFaceTracking = true;
            CameraDeviceName = "";

            EnableLipSync = true;
            LipSyncMicrophoneDeviceName = "";

            UseLookAtPointNone = false;
            UseLookAtPointMousePointer = true;
            UseLookAtPointMainCamera = false;

            WaistWidth = 30;
            ElbowCloseStrength = 30;
            EnablePresenterMotion = false;
            PresentationArmMotionScale = 30;
            PresentationArmRadiusMin = 20;

            LengthFromWristToTip = 18;
            LengthFromWristToPalm = 9;
            HandYOffsetBasic = 4;
            HandYOffsetAfterKeyDown = 2;

            EnableWaitMotion = true;
            WaitMotionScale = 100;
            WaitMotionPeriod = 10;
        }
    }
}
