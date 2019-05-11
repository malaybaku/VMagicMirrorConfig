using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace Baku.VMagicMirrorConfig
{
    public class MotionSettingViewModel : SettingViewModelBase
    {
        public MotionSettingViewModel() : base() { }
        internal MotionSettingViewModel(IMessageSender sender, IMessageReceiver receiver) : base(sender)
        {
            receiver.ReceivedCommand += OnReceivedCommand;
        }

        //フラグが立っている間はプロパティが変わってもメッセージを投げない。これはUnityから指定されたパラメタの適用中に
        private bool _silentPropertySetter = false;
        private protected override void SendMessage(Message message)
        {
            if (!_silentPropertySetter)
            {
                base.SendMessage(message);
            }
        }

        private void OnReceivedCommand(object sender, CommandReceivedEventArgs e)
        {
            switch(e.Command)
            {
                case ReceiveMessageNames.SetCalibrationFaceData:
                    CalibrateFaceData = e.Args;
                    break;
                case ReceiveMessageNames.AutoAdjustResults:
                    SetAutoAdjustResults(e.Args);
                    break;
                case ReceiveMessageNames.AutoAdjustEyebrowResults:
                    SetAutoAdjustResults(e.Args, true);
                    break;
                case ReceiveMessageNames.SetBlendShapeNames:
                    SetBlendShapeNames(e.Args);
                    break;
                default:
                    break;
            }
        }

        private void SetBlendShapeNames(string args)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                _writableBlendShapeNames.Clear();
                _writableBlendShapeNames.Add("");
                foreach (var name in args.Split('\t'))
                {
                    _writableBlendShapeNames.Add(name);
                }
            });
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

        private void SetAutoAdjustResults(string data) => SetAutoAdjustResults(data, false);

        private void SetAutoAdjustResults(string data, bool onlyEyebrow)
        {
            try
            {
                var parameters = JsonConvert.DeserializeObject<AutoAdjustParameters>(data);
                _silentPropertySetter = true;

                if (parameters.EyebrowIsValidPreset)
                {
                    EyebrowLeftUpKey = parameters.EyebrowLeftUpKey;
                    EyebrowLeftDownKey = parameters.EyebrowLeftDownKey;
                    UseSeparatedKeyForEyebrow = parameters.UseSeparatedKeyForEyebrow;
                    EyebrowRightUpKey = parameters.EyebrowRightUpKey;
                    EyebrowRightDownKey = parameters.EyebrowRightDownKey;
                    EyebrowUpScale = parameters.EyebrowUpScale;
                    EyebrowDownScale = parameters.EyebrowDownScale;
                }

                if (!onlyEyebrow)
                {
                    LengthFromWristToPalm = parameters.LengthFromWristToPalm;
                    LengthFromWristToTip = parameters.LengthFromWristToTip;
                }
            }
            catch (Exception)
            {
                //何もしない: データ形式が悪いので諦める
            }
            finally
            {
                _silentPropertySetter = false;
            }
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

        private int _faceDefaultFun = 20;
        public int FaceDefaultFun
        {
            get => _faceDefaultFun;
            set
            {
                if (SetValue(ref _faceDefaultFun, value))
                {
                    SendMessage(MessageFactory.Instance.FaceDefaultFun(FaceDefaultFun));
                }
            }
        }

        #region Eyebrow

        private readonly ObservableCollection<string> _writableBlendShapeNames
          = new ObservableCollection<string>();
        private ReadOnlyObservableCollection<string> _availableBlendShapeNames = null;
        [XmlIgnore]
        public ReadOnlyObservableCollection<string> AvailableBlendShapeNames
            => _availableBlendShapeNames ??
            (_availableBlendShapeNames = new ReadOnlyObservableCollection<string>(_writableBlendShapeNames));

        private string _eyebrowLeftUpKey = "";
        public string EyebrowLeftUpKey
        {
            get => _eyebrowLeftUpKey;
            set
            {
                if (SetValue(ref _eyebrowLeftUpKey, value))
                {
                    SendMessage(MessageFactory.Instance.EyebrowLeftUpKey(EyebrowLeftUpKey));
                }
            }
        }

        private string _eyebrowLeftDownKey = "";
        public string EyebrowLeftDownKey
        {
            get => _eyebrowLeftDownKey;
            set
            {
                if (SetValue(ref _eyebrowLeftDownKey, value))
                {
                    SendMessage(MessageFactory.Instance.EyebrowLeftDownKey(EyebrowLeftDownKey));
                }
            }
        }

        private bool _useSeparatedKeyForEyebrow = false;
        public bool UseSeparatedKeyForEyebrow
        {
            get => _useSeparatedKeyForEyebrow;
            set
            {
                if (SetValue(ref _useSeparatedKeyForEyebrow, value))
                {
                    SendMessage(MessageFactory.Instance.UseSeparatedKeyForEyebrow(UseSeparatedKeyForEyebrow));
                }
            }
        }

        private string _eyebrowRightUpKey = "";
        public string EyebrowRightUpKey
        {
            get => _eyebrowRightUpKey;
            set
            {
                if (SetValue(ref _eyebrowRightUpKey, value))
                {
                    SendMessage(MessageFactory.Instance.EyebrowRightUpKey(EyebrowRightUpKey));
                }
            }
        }

        private string _eyebrowRightDownKey = "";
        public string EyebrowRightDownKey
        {
            get => _eyebrowRightDownKey;
            set
            {
                if (SetValue(ref _eyebrowRightDownKey, value))
                {
                    SendMessage(MessageFactory.Instance.EyebrowRightDownKey(EyebrowRightDownKey));
                }
            }
        }

        private int _eyebrowUpScale = 100;
        public int EyebrowUpScale
        {
            get => _eyebrowUpScale;
            set
            {
                if (SetValue(ref _eyebrowUpScale, value))
                {
                    SendMessage(MessageFactory.Instance.EyebrowUpScale(EyebrowUpScale));
                }
            }
        }

        private int _eyebrowDownScale = 100;
        public int EyebrowDownScale
        {
            get => _eyebrowDownScale;
            set
            {
                if (SetValue(ref _eyebrowDownScale, value))
                {
                    SendMessage(MessageFactory.Instance.EyebrowDownScale(EyebrowDownScale));
                }
            }
        }

        #endregion

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

        private int _lengthFromWristToTip = 12;
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

        private int _lengthFromWristToPalm = 6;
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

        private int _handYOffsetBasic = 3;
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

            FaceDefaultFun = 20;

            EyebrowLeftUpKey = ""; 
            EyebrowLeftDownKey = "";
            UseSeparatedKeyForEyebrow = false;
            EyebrowRightUpKey = "";
            EyebrowRightDownKey = "";
            EyebrowUpScale = 100;
            EyebrowDownScale = 100;

            WaistWidth = 30;
            ElbowCloseStrength = 30;
            EnablePresenterMotion = false;
            PresentationArmMotionScale = 30;
            PresentationArmRadiusMin = 20;

            LengthFromWristToTip = 12;
            LengthFromWristToPalm = 6;
            HandYOffsetBasic = 3;
            HandYOffsetAfterKeyDown = 2;

            EnableWaitMotion = true;
            WaitMotionScale = 100;
            WaitMotionPeriod = 10;
        }
    }
}
