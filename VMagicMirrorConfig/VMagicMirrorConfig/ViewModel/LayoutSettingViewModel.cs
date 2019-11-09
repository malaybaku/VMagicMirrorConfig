using System;
using System.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace Baku.VMagicMirrorConfig
{
    public class LayoutSettingViewModel : SettingViewModelBase
    {
        private const int TypingEffectIndexNone = -1;
        private const int TypingEffectIndexText = 0;
        private const int TypingEffectIndexLight = 1;

        public LayoutSettingViewModel() : base() 
        {
            Gamepad = new GamepadSettingViewModel();
            _typingEffectItem = TypingEffectSelections[0];
        }
        internal LayoutSettingViewModel(IMessageSender sender, IMessageReceiver receiver) : base(sender)
        {
            Gamepad = new GamepadSettingViewModel(sender, receiver);
            _typingEffectItem = TypingEffectSelections[0];
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

        private ActionCommand? _resetCameraPositionCommand;
        public ActionCommand ResetCameraPositionCommand
            => _resetCameraPositionCommand ??= new ActionCommand(ResetCameraPosition);

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
        //TODO: エフェクトがこれ以上増える可能性が高いので、コンボボックスでの表示を前提にデータ構造を見直すべき

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

                _typingEffectIsNone = value;
                if (value)
                {
                    TypingEffectIsText = false;
                    TypingEffectIsLight = false;
                    SendMessage(MessageFactory.Instance.SetKeyboardTypingEffectType(TypingEffectIndexNone));
                    TypingEffectItem = TypingEffectSelections.FirstOrDefault(i => i.Id == TypingEffectIndexNone);
                }
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

                _typingEffectIsText = value;
                if (value)
                {
                    TypingEffectIsNone = false;
                    TypingEffectIsLight = false;
                    SendMessage(MessageFactory.Instance.SetKeyboardTypingEffectType(TypingEffectIndexText));
                    TypingEffectItem = TypingEffectSelections.FirstOrDefault(i => i.Id == TypingEffectIndexText);
                }
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

                _typingEffectIsLight = value;
                if (value)
                {
                    TypingEffectIsNone = false;
                    TypingEffectIsText = false;
                    SendMessage(MessageFactory.Instance.SetKeyboardTypingEffectType(TypingEffectIndexLight));
                    TypingEffectItem = TypingEffectSelections.FirstOrDefault(i => i.Id == TypingEffectIndexLight);
                }
                RaisePropertyChanged();
            }
        }

        private TypingEffectSelectionItem? _typingEffectItem = null;        
        [XmlIgnore]
        public TypingEffectSelectionItem? TypingEffectItem
        {
            get => _typingEffectItem;
            set
            {
                if (value == null || _typingEffectItem == value)
                {
                    return;
                }

                _typingEffectItem = value;
                switch (value.Id)
                {
                    case TypingEffectIndexNone:
                        TypingEffectIsNone = true;
                        break;
                    case TypingEffectIndexText:
                        TypingEffectIsText = true;
                        break;
                    case TypingEffectIndexLight:
                        TypingEffectIsLight = true;
                        break;
                }
                RaisePropertyChanged();
            }
        }

        [XmlIgnore]
        public TypingEffectSelectionItem[] TypingEffectSelections { get; } = new TypingEffectSelectionItem[]
        {
            new TypingEffectSelectionItem(TypingEffectIndexNone, "None", MaterialDesignThemes.Wpf.PackIconKind.EyeOff),
            new TypingEffectSelectionItem(TypingEffectIndexText, "Text", MaterialDesignThemes.Wpf.PackIconKind.Abc),
            new TypingEffectSelectionItem(TypingEffectIndexLight, "Light", MaterialDesignThemes.Wpf.PackIconKind.FlashOn),
        };

        private void OnReceiveCommand(object? sender, CommandReceivedEventArgs e)
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

        private ActionCommand? _resetHidSettingCommand = null;
        public ActionCommand ResetHidSettingCommand
            => _resetHidSettingCommand ??= new ActionCommand(
                () => SettingResetUtils.ResetSingleCategorySetting(ResetHidSetting)
                );
        private void ResetHidSetting()
        {
            HidHeight = 90;
            HidHorizontalScale = 70;
            HidVisibility = true;
            TypingEffectIsNone = true;
        }

        private ActionCommand? _resetCameraSettingCommand = null;
        public ActionCommand ResetCameraSettingCommand
            => _resetCameraSettingCommand ??= new ActionCommand(
                () => SettingResetUtils.ResetSingleCategorySetting(ResetCameraSetting)
                );
        private void ResetCameraSetting()
        {
            //NOTE: フリーカメラモードについては、もともと揮発性の設定にしているのでココでは触らない
            CameraFov = 40;
            //カメラ位置については、Unity側がカメラの基準位置を持っているのに任せる
            ResetCameraPosition();
        }

        public override void ResetToDefault()
        {
            Gamepad.ResetToDefault();

            ResetHidSetting();
            ResetCameraSetting();
        }

        #endregion

        public class TypingEffectSelectionItem
        {
            public TypingEffectSelectionItem(int id, string name, MaterialDesignThemes.Wpf.PackIconKind iconKind)
            {
                Id = id;
                EffectName = name;
                IconKind = iconKind;
            }
            public int Id { get; }
            public string EffectName { get; }
            public MaterialDesignThemes.Wpf.PackIconKind IconKind { get; }
        }
    }

}
