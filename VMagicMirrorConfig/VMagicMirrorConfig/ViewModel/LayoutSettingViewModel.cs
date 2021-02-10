using System;
using System.Xml.Serialization;
using MaterialDesignThemes.Wpf;
//TODO: ViewModelにこの2つのusingがあるのはけしからんですね～
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Baku.VMagicMirrorConfig
{
    public class LayoutSettingViewModel : SettingViewModelBase
    {
        internal LayoutSettingViewModel(LayoutSettingModel model, GamepadSettingModel gamepadModel, IMessageSender sender, IMessageReceiver receiver) : base(sender)
        {
            _model = model;
            Gamepad = new GamepadSettingViewModel(gamepadModel, sender);

            _typingEffectItem = TypingEffectSelections[0];
            receiver.ReceivedCommand += OnReceiveCommand;

            QuickSaveViewPointCommand = new ActionCommand<string>(QuickSaveViewPoint);
            QuickLoadViewPointCommand = new ActionCommand<string>(QuickLoadViewPoint);
            ResetCameraPositionCommand = new ActionCommand(() => SendMessage(MessageFactory.Instance.ResetCameraPosition()));

            ResetCameraSettingCommand = new ActionCommand(
                () => SettingResetUtils.ResetSingleCategoryAsync(_model.ResetCameraSetting)
                );

            ResetDeviceLayoutCommand = new ActionCommand(
                () => SettingResetUtils.ResetSingleCategoryAsync(_model.ResetDeviceLayout)
                );
            
            ResetHidSettingCommand = new ActionCommand(
                () => SettingResetUtils.ResetSingleCategoryAsync(_model.ResetHidSetting)
                );
            ResetCameraSettingCommand = new ActionCommand(
                () => SettingResetUtils.ResetSingleCategoryAsync(_model.ResetCameraSetting)
                );
            ResetMidiSettingCommand = new ActionCommand(
                () => SettingResetUtils.ResetSingleCategoryAsync(_model.ResetMidiSetting)
                );

        }

        private readonly LayoutSettingModel _model;

        //public LayoutSettingViewModel() : base()
        //{
        //    Gamepad = new GamepadSettingViewModel();
        //    _typingEffectItem = TypingEffectSelections[0];
        //}
        //internal LayoutSettingViewModel(IMessageSender sender, IMessageReceiver receiver) : base(sender)
        //{
        //    Gamepad = new GamepadSettingViewModel(sender, receiver);
        //    _typingEffectItem = TypingEffectSelections[0];
        //    receiver.ReceivedCommand += OnReceiveCommand;
        //}

        private void OnReceiveCommand(object? sender, CommandReceivedEventArgs e)
        {
            if (e.Command == ReceiveMessageNames.UpdateDeviceLayout)
            {
                _deviceLayout = e.Args;
            }
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

        public RPropertyMin<int> CameraFov => _model.CameraFov;

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

        public RPropertyMin<bool> EnableMidiRead => _model.EnableMidiRead;
        public RPropertyMin<bool> MidiControllerVisibility => _model.MidiControllerVisibility;

        //NOTE: カメラ位置、デバイスレイアウト、クイックセーブした視点については、ユーザーが直接いじる想定ではない

        #region カメラ位置とデバイスレイアウト

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

        //NOTE: Setterはファイルロード時にのみ呼ばれる
        private string _deviceLayout = "";
        public string DeviceLayout
        {
            get => _deviceLayout;
            set
            {
                if (SetValue(ref _deviceLayout, value))
                {
                    SendMessage(MessageFactory.Instance.SetDeviceLayout(DeviceLayout));
                }
            }
        }

        /// <summary>
        /// カメラ位置の情報を更新しますが、設定時にUnityプロセスにメッセージを送信しません。
        /// </summary>
        /// <param name="cameraPos"></param>
        public void SilentSetCameraPosition(string cameraPos)
            => _cameraPosition = cameraPos ?? "";

        #endregion

        #region 視点のクイックセーブ/ロード

        public RPropertyMin<string> QuickSave1 => _model.QuickSave1;
        public RPropertyMin<string> QuickSave2 => _model.QuickSave2;
        public RPropertyMin<string> QuickSave3 => _model.QuickSave3;

        public ActionCommand<string> QuickSaveViewPointCommand { get; }
        public ActionCommand<string> QuickLoadViewPointCommand { get; }

        //TODO: この辺の処理はモデルに移動してよい
        private async void QuickSaveViewPoint(string? index)
        {
            if (!(int.TryParse(index, out int i) && i > 0 && i <= 3))
            {
                return;
            }

            try
            {
                string res = await SendQueryAsync(MessageFactory.Instance.CurrentCameraPosition());
                string saveData = new JObject()
                {
                    ["fov"] = CameraFov.Value,
                    ["pos"] = res,
                }.ToString(Formatting.None);

                switch (i)
                {
                    case 1:
                        QuickSave1.Value = saveData;
                        break;
                    case 2:
                        QuickSave2.Value = saveData;
                        break;
                    case 3:
                        QuickSave3.Value = saveData;
                        break;
                    default:
                        //NOTE: ここは来ない
                        break;
                }
            }
            catch (Exception ex)
            {
                LogOutput.Instance.Write(ex);
            }
        }
        private void QuickLoadViewPoint(string? index)
        {
            if (!(int.TryParse(index, out int i) && i > 0 && i <= 3))
            {
                return;
            }

            try
            {
                string saveData =
                    (i == 1) ? QuickSave1.Value :
                    (i == 2) ? QuickSave2.Value :
                    QuickSave3.Value;

                var obj = JObject.Parse(saveData);
                string cameraPos = (string?)obj["pos"] ?? "";
                int fov = (int)(obj["fov"] ?? new JValue(40));

                CameraFov.Value = fov;
                //NOTE: CameraPositionには書き込まない。
                //CameraPositionへの書き込みはCameraPositionCheckerのポーリングに任せとけばOK 
                SendMessage(MessageFactory.Instance.QuickLoadViewPoint(cameraPos));
            }
            catch (Exception ex)
            {
                LogOutput.Instance.Write(ex);
            }
        }

        #endregion

        public ActionCommand ResetCameraPositionCommand { get; }


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
                    SilentSetCameraPosition(response);
                }
            }
        }

        public RPropertyMin<bool> HidVisibility => _model.HidVisibility;

        public RPropertyMin<bool> EnableDeviceFreeLayout => _model.EnableDeviceFreeLayout;

        #region タイピングエフェクト

        public RPropertyMin<int> SelectedTypingEffectId => _model.SelectedTypingEffectId;

        private TypingEffectSelectionItem? _typingEffectItem = null;
        public TypingEffectSelectionItem? TypingEffectItem
        {
            get => _typingEffectItem;
            set
            {
                //ここのガード文はComboBoxを意識した書き方なことに注意
                if (value == null || _typingEffectItem == value || (_typingEffectItem != null && _typingEffectItem.Id == value.Id))
                {
                    return;
                }

                _typingEffectItem = value;
                SelectedTypingEffectId.Value = _typingEffectItem.Id;
                RaisePropertyChanged();
            }
        }

        public TypingEffectSelectionItem[] TypingEffectSelections { get; } = new TypingEffectSelectionItem[]
        {
            new TypingEffectSelectionItem(LayoutSetting.TypingEffectIndexNone, "None", PackIconKind.EyeOff),
            new TypingEffectSelectionItem(LayoutSetting.TypingEffectIndexText, "Text", PackIconKind.Abc),
            new TypingEffectSelectionItem(LayoutSetting.TypingEffectIndexLight, "Light", PackIconKind.FlashOn),
            //new TypingEffectSelectionItem(LayoutSetting.TypingEffectIndexLaser, "Laser", PackIconKind.Wand),
            new TypingEffectSelectionItem(LayoutSetting.TypingEffectIndexButtefly, "Butterfly", PackIconKind.DotsHorizontal),
        };

        #endregion

        #region Reset API

        public ActionCommand ResetDeviceLayoutCommand { get; }
        public ActionCommand ResetHidSettingCommand { get; }
        public ActionCommand ResetCameraSettingCommand { get; }
        public ActionCommand ResetMidiSettingCommand { get; }

        public override void ResetToDefault() => _model.ResetToDefault();

        #endregion

        //TODO: Recordで書きたい…
        public class TypingEffectSelectionItem
        {
            public TypingEffectSelectionItem(int id, string name, PackIconKind iconKind)
            {
                Id = id;
                EffectName = name;
                IconKind = iconKind;
            }
            public int Id { get; }
            public string EffectName { get; }
            public PackIconKind IconKind { get; }
        }
    }

}
