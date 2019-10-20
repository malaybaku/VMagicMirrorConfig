using System;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace Baku.VMagicMirrorConfig
{
    public class LayoutSettingViewModel : SettingViewModelBase
    {
        private const int TypingEffectIndexNone = -1;
        private const int TypingEffectIndexText = 0;
        private const int TypingEffectIndexLight = 1;

        public LayoutSettingViewModel() : base() { }
        internal LayoutSettingViewModel(IMessageSender sender, IMessageReceiver receiver) : base(sender)
        {
            Gamepad = new GamepadSettingViewModel(sender, receiver);
            receiver.ReceivedCommand += OnReceiveCommand;
        }

        private bool _silentPropertySetter = false;
        private protected override void SendMessage(Message message)
        {
            if (!_silentPropertySetter)
            {
                base.SendMessage(message);
            }
        }


        /// <summary>
        /// </summary>
        /// <remarks>
        /// Setterがpublicなのはシリアライザのご機嫌取ってるだけなので普通のコードでは触らない事！
        /// </remarks>
        public GamepadSettingViewModel Gamepad { get; set; }

        private int _cameraFov = 40;
        public int CameraFov
        {
            get => _cameraFov;
            set
            {
                if (SetValue(ref _cameraFov, value))
                {
                    {
                        SendMessage(MessageFactory.Instance.CameraFov(CameraFov));
                    }
                }
            }
        }


        private bool _enableFreeCameraMode = false;
        [XmlIgnore]
        public bool EnableFreeCameraMode
        {
            get => _enableFreeCameraMode;
            set
            {
                if (SetValue(ref _enableFreeCameraMode, value))
                {
                    //asyncメソッド呼ぶので外に出しておく。
                    //ややだらしない書き方だが、仮に連打されてもさほど害ないのでコレで行きます。
                    OnEnableFreeCameraModeChanged(value);
                }
            }
        }

        //NOTE: このプロパティは他と違ってユーザーが直接いじるのは想定してない
        private string _cameraPosition = "";
        public string CameraPosition
        {
            get => _cameraPosition;
            set
            {
                if (SetValue(ref _cameraPosition, value))
                {
                    SendMessage(MessageFactory.Instance.SetCustomCameraPosition(CameraPosition));
                }
            }
        }

        private ActionCommand _resetCameraPositionCommand;
        public ActionCommand ResetCameraPositionCommand
            => _resetCameraPositionCommand ?? (_resetCameraPositionCommand = new ActionCommand(ResetCameraPosition));

        private void ResetCameraPosition()
            => SendMessage(MessageFactory.Instance.ResetCameraPosition());

        private async void OnEnableFreeCameraModeChanged(bool value)
        {
            SendMessage(MessageFactory.Instance.EnableFreeCameraMode(EnableFreeCameraMode));
            //トグルさげた場合: 切った時点のカメラポジションを取得、保存する。
            //NOTE:
            //将来版でフリーレイアウト(キーボードやゲームパッドだけ動かす)を入れた場合、
            //トグルが下がっても値を適用しないほうがいいかも
            if (!value)
            {
                string response = await SendQueryAsync(MessageFactory.Instance.CurrentCameraPosition());
                if (!string.IsNullOrWhiteSpace(response))
                {
                    CameraPosition = response;
                }
            }
        }

        private int _hidHeight = 90;
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

        private int _hidHorizontalScale = 70;
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

        //NOTE: ラジオボタン表示をザツにやるためにbool値たくさんで代用している(ほんとはあまり良くない)
        private bool _typingEffectIsNone = true;
        public bool TypingEffectIsNone
        {
            get => _typingEffectIsNone;
            set
            {
                if (value == _typingEffectIsNone)
                {
                    return;
                }

                if (value)
                {
                    TypingEffectIsText = false;
                    TypingEffectIsLight = false;
                    SendMessage(MessageFactory.Instance.SetKeyboardTypingEffectType(TypingEffectIndexNone));
                }
                _typingEffectIsNone = value;
                RaisePropertyChanged();
            }
        }

        private bool _typingEffectIsText = false;
        public bool TypingEffectIsText
        {
            get => _typingEffectIsText;
            set
            {
                if (value == _typingEffectIsText)
                {
                    return;
                }

                if (value)
                {
                    TypingEffectIsNone = false;
                    TypingEffectIsLight = false;
                    SendMessage(MessageFactory.Instance.SetKeyboardTypingEffectType(TypingEffectIndexText));
                }
                _typingEffectIsText = value;
                RaisePropertyChanged();
            }
        }

        private bool _typingEffectIsLight = false;
        public bool TypingEffectIsLight
        {
            get => _typingEffectIsLight;
            set
            {
                if (value == _typingEffectIsLight)
                {
                    return;
                }

                if (value)
                {
                    TypingEffectIsNone = false;
                    TypingEffectIsText = false;
                    SendMessage(MessageFactory.Instance.SetKeyboardTypingEffectType(TypingEffectIndexLight));
                }
                _typingEffectIsLight = value;
                RaisePropertyChanged();
            }
        }
        

        private void OnReceiveCommand(object sender, CommandReceivedEventArgs e)
        {
            switch (e.Command)
            {
                case ReceiveMessageNames.AutoAdjustResults:
                    SetAutoAdjustResults(e.Args);
                    break;
                default:
                    break;
            }
        }

        private void SetAutoAdjustResults(string args)
        {
            try
            {
                var parameters = JsonConvert.DeserializeObject<AutoAdjustParameters>(args);
                _silentPropertySetter = true;
                HidHeight = parameters.HidHeight;
                HidHorizontalScale = parameters.HidHorizontalScale;
            }
            catch (Exception)
            {
                //諦める
            }
            finally
            {
                _silentPropertySetter = false;
            }
        }

        #region Reset API

        private ActionCommand _resetHidSettingCommand = null;
        public ActionCommand ResetHidSettingCommand
            => _resetHidSettingCommand ??
            (_resetHidSettingCommand = new ActionCommand(ResetHidCommandImpl));
        private void ResetHidCommandImpl()
            => SettingResetUtils.ResetSingleCategorySetting(ResetHidSetting);
        
        private void ResetHidSetting()
        {
            HidHeight = 90;
            HidHorizontalScale = 70;
            HidVisibility = true;
            CameraFov = 40;
            TypingEffectIsNone = true;
        }


        public override void ResetToDefault()
        {
            Gamepad.ResetToDefault();
 
            ResetHidSetting();

            //カメラ位置については、Unity側がカメラの基準位置を持っているのに任せる
            ResetCameraPosition();
        }

        #endregion
    }
}
