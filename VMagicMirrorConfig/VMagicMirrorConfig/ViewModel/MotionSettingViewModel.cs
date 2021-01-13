using System;
using System.Collections.ObjectModel;
using System.Linq;
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
            ShowInstallPathWarning = InstallPathChecker.HasMultiByteCharInInstallPath();
        }

        private LargePointerController _largePointerController => LargePointerController.Instance;

        //フラグが立っている間はプロパティが変わってもメッセージを投げない。これはUnityから指定されたパラメタの適用中に立てる
        private bool _silentPropertySetter = false;
        private protected override void SendMessage(Message message)
        {
            if (!_silentPropertySetter)
            {
                base.SendMessage(message);
            }
        }

        private void OnReceivedCommand(object? sender, CommandReceivedEventArgs e)
        {
            switch (e.Command)
            {
                case ReceiveMessageNames.SetCalibrationFaceData:
                    CalibrateFaceData = e.Args;
                    break;
                case ReceiveMessageNames.AutoAdjustResults:
                    SetAutoAdjustResults(e.Args);
                    break;
                case ReceiveMessageNames.MicrophoneVolumeLevel:
                    if (ShowMicrophoneVolume && int.TryParse(e.Args, out int i))
                    {
                        MicrophoneVolumeValue = i;
                    }
                    break;
                case ReceiveMessageNames.ExtraBlendShapeClipNames:
                    try
                    {
                        //いちおう信頼はするけどIPCだし…みたいな書き方。FaceSwitchと同じ
                        var names = e.Args
                            .Split(',')
                            .Where(w => !string.IsNullOrEmpty(w))
                            .ToArray();
                        _blendShapeNameStore.Refresh(names);
                    }
                    catch(Exception ex)
                    {
                        LogOutput.Instance.Write(ex);
                    }
                    break;
                default:
                    break;
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

        public void ClosePointer() => _largePointerController.Close();

        private void SetAutoAdjustResults(string data) 
        {
            try
            {
                var parameters = JsonConvert.DeserializeObject<AutoAdjustParameters>(data);
                _silentPropertySetter = true;
                LengthFromWristToTip = parameters.LengthFromWristToTip;
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

        #region Full Body 

        private bool _enableNoHandTrackMode = false;
        public bool EnableNoHandTrackMode
        {
            get => _enableNoHandTrackMode;
            set
            {
                if (SetValue(ref _enableNoHandTrackMode, value))
                {
                    SendMessage(MessageFactory.Instance.EnableNoHandTrackMode(EnableNoHandTrackMode));
                }
            }
        }

        #endregion

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

        private bool _showInstallPathWarning = false;
        [XmlIgnore]
        public bool ShowInstallPathWarning
        {
            get => _showInstallPathWarning;
            set => SetValue(ref _showInstallPathWarning, value);
        }

        private bool _autoblinkDuringFaceTracking = true;
        public bool AutoBlinkDuringFaceTracking
        {
            get => _autoblinkDuringFaceTracking;
            set
            {
                if (SetValue(ref _autoblinkDuringFaceTracking, value))
                {
                    SendMessage(MessageFactory.Instance.AutoBlinkDuringFaceTracking(AutoBlinkDuringFaceTracking));
                }
            }
        }

        private bool _enableBodyLeanZ = false;
        public bool EnableBodyLeanZ
        {
            get => _enableBodyLeanZ;
            set
            {
                if (SetValue(ref _enableBodyLeanZ, value))
                {
                    SendMessage(MessageFactory.Instance.EnableBodyLeanZ(EnableBodyLeanZ));
                }
            }
        }

        private bool _enableBlinkAdjust = true;
        public bool EnableBlinkAdjust
        {
            get => _enableBlinkAdjust;
            set
            {
                if (SetValue(ref _enableBlinkAdjust, value))
                {
                    SendMessage(MessageFactory.Instance.EnableHeadRotationBasedBlinkAdjust(EnableBlinkAdjust));
                    SendMessage(MessageFactory.Instance.EnableLipSyncBasedBlinkAdjust(EnableBlinkAdjust));
                }
            }
        }
        private bool _enableVoiceBasedMotion = true;
        public bool EnableVoiceBasedMotion
        {
            get => _enableVoiceBasedMotion;
            set
            {
                if (SetValue(ref _enableVoiceBasedMotion, value))
                {
                    SendMessage(MessageFactory.Instance.EnableVoiceBasedMotion(EnableVoiceBasedMotion));
                }
            }
        }

        private bool _disableFaceTrackingHorizontalFlip = false;
        public bool DisableFaceTrackingHorizontalFlip
        {
            get => _disableFaceTrackingHorizontalFlip;
            set
            {
                if (SetValue(ref _disableFaceTrackingHorizontalFlip, value))
                {
                    SendMessage(MessageFactory
                        .Instance
                        .DisableFaceTrackingHorizontalFlip(_disableFaceTrackingHorizontalFlip)
                        );
                }
            }
        }

        private bool _enableWebCamHighPowerMode = false;
        public bool EnableWebCamHighPowerMode
        {
            get => _enableWebCamHighPowerMode;
            set
            {
                if (SetValue(ref _enableWebCamHighPowerMode, value))
                {
                    SendMessage(MessageFactory.Instance.EnableWebCamHighPowerMode(EnableWebCamHighPowerMode));
                }
            }
        }

        private bool _enableImageBasedHandTracking = false;
        public bool EnableImageBasedHandTracking
        {
            get => _enableImageBasedHandTracking;
            set
            {
                if (SetValue(ref _enableImageBasedHandTracking, value))
                {
                    SendMessage(
                        MessageFactory.Instance.EnableImageBasedHandTracking(EnableImageBasedHandTracking)
                        );
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
        private ReadOnlyObservableCollection<string>? _cameraDeviceNames = null;
        [XmlIgnore]
        public ReadOnlyObservableCollection<string> CameraDeviceNames
            => _cameraDeviceNames ??= new ReadOnlyObservableCollection<string>(_writableCameraDeviceNames);

        private ActionCommand? _calibrateFaceCommand;
        public ActionCommand CalibrateFaceCommand
            => _calibrateFaceCommand ??= new ActionCommand(
                () => SendMessage(MessageFactory.Instance.CalibrateFace())
                );

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

        private int _faceDefaultFun = 0;
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

        private readonly FaceMotionBlendShapeNameStore _blendShapeNameStore = new FaceMotionBlendShapeNameStore();
        [XmlIgnore]
        public ReadOnlyObservableCollection<string> BlendShapeNames => _blendShapeNameStore.BlendShapeNames;

        private string? _faceNeutralClip = "";
        public string? FaceNeutralClip
        {
            get => _faceNeutralClip;
            set
            {
                if (_faceNeutralClip != value)
                {
                    _faceNeutralClip = value;
                    RaisePropertyChanged();
                    _blendShapeNameStore.Refresh(FaceNeutralClip, FaceOffsetClip);
                    SendMessage(MessageFactory.Instance.FaceNeutralClip(FaceNeutralClip ?? ""));
                }
            }
        }

        private string? _faceOffsetClip = "";
        public string? FaceOffsetClip
        {
            get => _faceOffsetClip;
            set
            {
                if (_faceOffsetClip != value)
                {
                    _faceOffsetClip = value;
                    RaisePropertyChanged();
                    _blendShapeNameStore.Refresh(FaceNeutralClip, FaceOffsetClip);
                    SendMessage(MessageFactory.Instance.FaceOffsetClip(FaceOffsetClip ?? ""));
                }
            }
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

        private int _eyeBoneRotationScale = 100;
        public int EyeBoneRotationScale
        {
            get => _eyeBoneRotationScale;
            set
            {
                if (SetValue(ref _eyeBoneRotationScale, value))
                {
                    SendMessage(MessageFactory.Instance.SetEyeBoneRotationScale(EyeBoneRotationScale));
                    UpdateEyeRotRangeText();
                }
            }
        }

        //NOTE: ちょっと作法が悪いけど、「-7.0 ~ +7.0」のようなテキストでViewにわたす
        private const double EyeRotDefaultRange = 7.0;
        private string _eyeRotRangeText = $"-{EyeRotDefaultRange:0.00} ~ +{EyeRotDefaultRange:0.00}";
        [XmlIgnore]
        public string EyeRotRangeText
        {
            get => _eyeRotRangeText;
            private set => SetValue(ref _eyeRotRangeText, value);
        }
        private void UpdateEyeRotRangeText()
        {
            double range = EyeRotDefaultRange * EyeBoneRotationScale * 0.01;
            EyeRotRangeText =  $"-{range:0.00} ~ +{range:0.00}";
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
                    if (!value)
                    {
                        //マイク切ったのにゲイン計算が持続してると嬉しくない、という事。
                        ShowMicrophoneVolume = false;
                    }
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

        //NOTE: dB単位なので0がデフォルト。対数ベースのほうがレンジ取りやすい
        private int _microphoneSensitivity = 0;
        public int MicrophoneSensitivity
        {
            get => _microphoneSensitivity;
            set
            {
                if (SetValue(ref _microphoneSensitivity, value))
                {
                    SendMessage(MessageFactory.Instance.SetMicrophoneSensitivity(MicrophoneSensitivity));
                }
            }
        }

        private bool _showMicrophoneVolume = false;
        [XmlIgnore]
        public bool ShowMicrophoneVolume
        {
            get => _showMicrophoneVolume;
            set
            {
                if (SetValue(ref _showMicrophoneVolume, value))
                {
                    SendMessage(MessageFactory.Instance.SetMicrophoneVolumeVisibility(ShowMicrophoneVolume));
                    if (!value)
                    {
                        MicrophoneVolumeValue = 0;
                    }
                }
            }
        }
        
        private int _microphoneVolumeValue = 0;
        //NOTE: 0 ~ 20が無音、21~40が適正、41~50がデカすぎになる。これはUnity側がそういう整形をしてくれる
        [XmlIgnore]
        public int MicrophoneVolumeValue
        {
            get => _microphoneVolumeValue;
            set => SetValue(ref _microphoneVolumeValue, value);
        }


        private readonly ObservableCollection<string> _writableMicrophoneDeviceNames
            = new ObservableCollection<string>();
        private ReadOnlyObservableCollection<string>? _microphoneDeviceNames = null;
        [XmlIgnore]
        public ReadOnlyObservableCollection<string> MicrophoneDeviceNames
            => _microphoneDeviceNames ??= new ReadOnlyObservableCollection<string>(_writableMicrophoneDeviceNames);

        #endregion

        #region Arm

        private bool _enableHidArmMotion = true;
        public bool EnableHidArmMotion
        {
            get => _enableHidArmMotion;
            set
            {
                if (SetValue(ref _enableHidArmMotion, value))
                {
                    SendMessage(MessageFactory.Instance.EnableHidArmMotion(EnableHidArmMotion));
                }
            }
        }

        private bool _enableHidRandomTyping = false;
        public bool EnableHidRandomTyping
        {
            get => _enableHidRandomTyping;
            set
            {
                if (SetValue(ref _enableHidRandomTyping, value))
                {
                    SendMessage(MessageFactory.Instance.EnableHidRandomTyping(EnableHidRandomTyping));
                }
            }
        }

        private bool _enableShoulderMotionModify = true;
        public bool EnableShoulderMotionModify
        {
            get => _enableShoulderMotionModify;
            set
            {
                if (SetValue(ref _enableShoulderMotionModify, value))
                {
                    SendMessage(MessageFactory.Instance.EnableShoulderMotionModify(EnableShoulderMotionModify));
                }
            }
        }

        private bool _enableHandDownTimeout = true;
        public bool EnableHandDownTimeout
        {
            get => _enableHandDownTimeout;
            set
            {
                if (SetValue(ref _enableHandDownTimeout, value))
                {
                    SendMessage(MessageFactory.Instance.EnableTypingHandDownTimeout(EnableHandDownTimeout));
                }
            }
        }

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

        private bool _enableFpsAssumedRightHand = false;
        public bool EnableFpsAssumedRightHand
        {
            get => _enableFpsAssumedRightHand;
            set
            {
                if (SetValue(ref _enableFpsAssumedRightHand, value))
                {
                    SendMessage(MessageFactory.Instance.EnableFpsAssumedRightHand(EnableFpsAssumedRightHand));
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
                    UpdatePointerVisibility();
                }
            }
        }

        private bool _showPresentationPointer = false;
        public bool ShowPresentationPointer
        {
            get => _showPresentationPointer;
            set
            {
                if (SetValue(ref _showPresentationPointer, value))
                {
                    UpdatePointerVisibility();
                }
            }
        }

        private void UpdatePointerVisibility()
        { 
            _largePointerController.UpdateVisibility(
                EnablePresenterMotion && ShowPresentationPointer
                );
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

        private int _waitMotionScale = 125;
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

        #region Reset API

        private ActionCommand? _resetFaceMotionSettingCommand = null;
        public ActionCommand ResetFaceMotionSettingCommand
            => _resetFaceMotionSettingCommand ??= new ActionCommand(
                () => SettingResetUtils.ResetSingleCategorySettingAsync(ResetFaceSetting)
                );

        private ActionCommand? _resetArmMotionSettingCommand = null;
        public ActionCommand ResetArmMotionSettingCommand
            => _resetArmMotionSettingCommand ??= new ActionCommand(
                () => SettingResetUtils.ResetSingleCategorySettingAsync(ResetArmSetting)
                );

        private ActionCommand? _resetHandMotionSettingCommand = null;
        public ActionCommand ResetHandMotionSettingCommand
            => _resetHandMotionSettingCommand ??= new ActionCommand(
                () => SettingResetUtils.ResetSingleCategorySettingAsync(ResetHandSetting)
                );

        private ActionCommand? _resetWaitMotionSettingCommand = null;
        public ActionCommand ResetWaitMotionSettingCommand
            => _resetWaitMotionSettingCommand ??= new ActionCommand(
                () => SettingResetUtils.ResetSingleCategorySettingAsync(ResetWaitMotionSetting)
                );

        private void ResetFaceSetting()
        {
            EnableFaceTracking = true;
            CameraDeviceName = "";
            AutoBlinkDuringFaceTracking = true;
            EnableBodyLeanZ = false;

            EnableBlinkAdjust = true;
            EnableVoiceBasedMotion = true;
            DisableFaceTrackingHorizontalFlip = false;
            EnableImageBasedHandTracking = false;

            EnableLipSync = true;
            LipSyncMicrophoneDeviceName = "";
            MicrophoneSensitivity = 0;

            UseLookAtPointNone = false;
            UseLookAtPointMousePointer = true;
            UseLookAtPointMainCamera = false;
            EyeBoneRotationScale = 100;

            FaceDefaultFun = 0;
            FaceNeutralClip = null;
            FaceOffsetClip = null;
        }

        private void ResetArmSetting()
        {
            EnableHidArmMotion = true;
            EnableHidRandomTyping = false;
            EnableShoulderMotionModify = true;
            EnableHandDownTimeout = true;
            WaistWidth = 30;
            ElbowCloseStrength = 30;
            EnableFpsAssumedRightHand = false;
            EnablePresenterMotion = false;
            ShowPresentationPointer = false;
            PresentationArmRadiusMin = 20;
        }

        private void ResetHandSetting()
        {
            LengthFromWristToTip = 12;
            HandYOffsetBasic = 3;
            HandYOffsetAfterKeyDown = 2;
        }

        private void ResetWaitMotionSetting()
        {
            EnableWaitMotion = true;
            WaitMotionScale = 125;
            WaitMotionPeriod = 10;
        }

        public override void ResetToDefault()
        {
            EnableNoHandTrackMode = false;
            ResetFaceSetting();
            ResetArmSetting();
            ResetHandSetting();
            ResetWaitMotionSetting();
        }

        #endregion
    }


    /// <summary>
    /// ブレンドシェイプクリップ名を一覧保持するクラスです。
    /// ExTrackerとクラスが分かれてるのは、クリップ名の持ち方がちょっと違うためです。
    /// </summary>
    public class FaceMotionBlendShapeNameStore
    {
        public FaceMotionBlendShapeNameStore()
        {
            BlendShapeNames = new ReadOnlyObservableCollection<string>(_blendShapeNames);
            var defaultNames = LoadDefaultNames();
            for (int i = 0; i < defaultNames.Length; i++)
            {
                _blendShapeNames.Add(defaultNames[i]);
            }
        }

        private readonly ObservableCollection<string> _blendShapeNames = new ObservableCollection<string>();
        /// <summary> UIに表示するのが妥当と考えられるブレンドシェイプクリップ名の一覧です。 </summary>
        public ReadOnlyObservableCollection<string> BlendShapeNames { get; }

        //Unityで読み込まれたキャラクターのブレンドシェイプ名の一覧です。
        //NOTE: この値は標準ブレンドシェイプ名を含んでいてもいなくてもOK。ただし現行動作では標準ブレンドシェイプ名は含まない。
        private string[] _avatarClipNames = new string[0];

        //設定ファイルから読み込んだ設定で使われていたブレンドシェイプ名の一覧。
        //NOTE: この値に標準ブレンドシェイプ名とそうでないのが混在することがあるが、それはOK
        private string[] _settingUsedNames = new string[0];

        /// <summary>
        /// ロードされたVRMの標準以外のブレンドシェイプ名を指定して、名前一覧を更新します。
        /// </summary>
        /// <param name="avatarBlendShapeNames"></param>
        public void Refresh(string[] avatarBlendShapeNames)
        {
            //なんとなく正格評価しておく(値コピーの方が安心なので…
            _avatarClipNames = avatarBlendShapeNames.ToArray();
            RefreshInternal();
        }

        /// <summary>
        /// ファイルからロードされたはずの設定を参照し、その中で使われているブレンドシェイプ名を参考にして名前一覧を更新します。
        /// </summary>
        /// <param name="neutralClipName"></param>
        /// <param name="offsetClipName"></param>
        public void Refresh(string? neutralClipName, string? offsetClipName)
        {
            //NOTE: Refreshの挙動上、ここで""だけを2つ入れたりしても大丈夫。
            _settingUsedNames = new string[] { neutralClipName ?? "", offsetClipName ?? "" };
            RefreshInternal();
        }

        private void RefreshInternal()
        {
            //理想の並び: デフォルトのやつ一覧、今ロードしたVRMにある名前一覧、(今ロードしたVRMにはないけど)設定で使ってる名前一覧
            var newNames = LoadDefaultNames().ToList();
            int defaultSetLength = newNames.Count;
            foreach (var nameInModel in _avatarClipNames)
            {
                if (!newNames.Contains(nameInModel))
                {
                    newNames.Add(nameInModel);
                }
            }

            foreach (var nameInSetting in _settingUsedNames)
            {
                if (!newNames.Contains(nameInSetting))
                {
                    newNames.Add(nameInSetting);
                }
            }

            var newNameArray = newNames.ToArray();

            //NOTE: ここポイントで、既存要素は消さないよう慎重に並べ替えます(消すとOC<T>の怒りを買ってUI側の要素選択に悪影響が出たりするので…)
            for (int i = defaultSetLength; i < newNameArray.Length; i++)
            {
                if (_blendShapeNames.Contains(newNameArray[i]))
                {
                    int currentIndex = _blendShapeNames.IndexOf(newNameArray[i]);
                    if (currentIndex != i)
                    {
                        //もう入ってる値だが、場所を入れ替えたいケース
                        _blendShapeNames.Move(currentIndex, i);
                    }
                }
                else
                {
                    //そもそも入ってないケース
                    _blendShapeNames.Insert(i, newNameArray[i]);
                }
            }

            //OC<T>側のほうが配列が長い場合、ハミ出た分は余計なやつなので消しちゃってOK
            while (_blendShapeNames.Count > newNameArray.Length)
            {
                _blendShapeNames.RemoveAt(newNameArray.Length);
            }
        }

        private static string[] LoadDefaultNames()
        {
            return new string[]
            {
                //「なし」があるのが大事。これによって、条件に合致しても何のブレンドシェイプを起動しない！という事ができる。
                "",
                "Joy",
                "Angry",
                "Sorrow",
                "Fun",

                "A",
                "I",
                "U",
                "E",
                "O",

                "Neutral",
                "Blink",
                "Blink_L",
                "Blink_R",

                "LookUp",
                "LookDown",
                "LookLeft",
                "LookRight",
            };
        }
    }

}
