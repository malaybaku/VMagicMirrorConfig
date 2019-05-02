using System.Xml.Serialization;

namespace Baku.VMagicMirrorConfig
{
    public class LayoutSettingViewModel : SettingViewModelBase
    {
        public LayoutSettingViewModel() : base() { }
        internal LayoutSettingViewModel(IMessageSender sender) : base(sender)
        {
            Gamepad = new GamepadSettingViewModel(sender);
        }

        /// <summary>
        /// </summary>
        /// <remarks>
        /// Setterがpublicなのはシリアライザのご機嫌取ってるだけなので普通のコードでは触らない事！
        /// </remarks>
        public GamepadSettingViewModel Gamepad { get; set; }

        #region Properties

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

        public override void ResetToDefault()
        {
            Gamepad.ResetToDefault();

            HidHeight = 100;
            HidHorizontalScale = 100;
            HidVisibility = true;
        }
    }
}
