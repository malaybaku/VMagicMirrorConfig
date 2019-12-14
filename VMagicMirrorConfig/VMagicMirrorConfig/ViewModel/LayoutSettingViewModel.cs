using System;
using System.Linq;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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

        /// <summary>
        /// デバイスのレイアウト情報を更新しますが、設定時にUnityプロセスにメッセージを送信しません。
        /// </summary>
        /// <param name="deviceLayout"></param>
        public void SilentSetDeviceLayout(string deviceLayout)
            => _deviceLayout = deviceLayout ?? "";

        #endregion

        #region 視点のクイックセーブ/ロード

        //NOTE: 数が少ないので、ラクな方法ということでハードコーディングにしてます
        //以下3つの文字列は"CameraPosition+視野角"というデータで構成されます
        private string _quickSave1 = "";
        public string QuickSave1
        {
            get => _quickSave1;
            set
            {
                if (SetValue(ref _quickSave1, value))
                {
                    RaisePropertyChanged(nameof(HasQuickSaveItem1));
                }
            }
        }

        private string _quickSave2 = "";
        public string QuickSave2
        {
            get => _quickSave2;
            set
            {
                if (SetValue(ref _quickSave2, value))
                {
                    RaisePropertyChanged(nameof(HasQuickSaveItem2));
                }
            }
        }

        private string _quickSave3 = "";
        public string QuickSave3
        {
            get => _quickSave3;
            set
            {
                if (SetValue(ref _quickSave3, value))
                {
                    RaisePropertyChanged(nameof(HasQuickSaveItem3));
                }
            }
        }

        [XmlIgnore]
        public bool HasQuickSaveItem1 => !string.IsNullOrWhiteSpace(QuickSave1);
        [XmlIgnore]
        public bool HasQuickSaveItem2 => !string.IsNullOrWhiteSpace(QuickSave2);
        [XmlIgnore]
        public bool HasQuickSaveItem3 => !string.IsNullOrWhiteSpace(QuickSave3);

        private ActionCommand<string>? _quickSaveViewPointCommand;
        public ActionCommand<string> QuickSaveViewPointCommand
            => _quickSaveViewPointCommand ??= new ActionCommand<string>(QuickSaveViewPoint);
        private async void QuickSaveViewPoint(string? index)
        {
            if (!(
                int.TryParse(index, out int i) && 
                i > 0 && i <= 3
                ))
            {
                return;
            }

            try
            {
                string res = await SendQueryAsync(MessageFactory.Instance.CurrentCameraPosition());
                string saveData = new JObject()
                {
                    ["fov"] = CameraFov,
                    ["pos"] = res,
                }.ToString(Formatting.None);

                if (i == 1)
                {
                    QuickSave1 = saveData;
                }
                else if (i == 2)
                {
                    QuickSave2 = saveData;
                }
                else
                {
                    QuickSave3 = saveData;
                }
            }
            catch(Exception ex)
            {
                LogOutput.Instance.Write(ex);
            }
        }

        private ActionCommand<string>? _quickLoadViewPointCommand;
        public ActionCommand<string> QuickLoadViewPointCommand
            => _quickLoadViewPointCommand ??= new ActionCommand<string>(QuickLoadViewPoint);
        private void QuickLoadViewPoint(string? index)
        {
            if (!(int.TryParse(index, out int i) && i > 0 && i <= 3))
            {
                return;
            }

            try
            {
                string saveData =
                (i == 1) ? QuickSave1 :
                (i == 2) ? QuickSave2 :
                QuickSave3;

                var obj = JObject.Parse(saveData);
                string cameraPos = (string?)obj["pos"] ?? "";
                int fov = (int)(obj["fov"] ?? new JValue(40));

                CameraFov = fov;
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
                    SilentSetCameraPosition(response);
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

        private bool _enableDeviceFreeLayout = false;
        [XmlIgnore]
        public bool EnableDeviceFreeLayout
        {
            get => _enableDeviceFreeLayout;
            set
            {
                if (SetValue(ref _enableDeviceFreeLayout, value))
                {
                    SendMessage(
                        MessageFactory.Instance.EnableDeviceFreeLayout(EnableDeviceFreeLayout)
                        );
                }
            }
        }

        #region タイピングエフェクト

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

        #endregion

        #region Reset API

        private ActionCommand? _resetDeviceLayoutCommand = null;
        public ActionCommand ResetDeviceLayoutCommand
            => _resetDeviceLayoutCommand ??= new ActionCommand(
                () => SettingResetUtils.ResetSingleCategorySettingAsync(ResetDeviceLayout)
                );
        private void ResetDeviceLayout()
        {
            SendMessage(MessageFactory.Instance.ResetDeviceLayout());
        }

        private ActionCommand? _resetHidSettingCommand = null;
        public ActionCommand ResetHidSettingCommand
            => _resetHidSettingCommand ??= new ActionCommand(
                () => SettingResetUtils.ResetSingleCategorySettingAsync(ResetHidSetting)
                );
        private void ResetHidSetting()
        {
            HidVisibility = true;
            TypingEffectIsNone = true;
        }

        private ActionCommand? _resetCameraSettingCommand = null;
        public ActionCommand ResetCameraSettingCommand
            => _resetCameraSettingCommand ??= new ActionCommand(
                () => SettingResetUtils.ResetSingleCategorySettingAsync(ResetCameraSetting)
                );
        private void ResetCameraSetting()
        {
            //NOTE: フリーカメラモードについては、もともと揮発性の設定にしているのでココでは触らない
            CameraFov = 40;
            QuickSave1 = "";
            QuickSave2 = "";
            QuickSave3 = "";
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
