namespace Baku.VMagicMirrorConfig
{
    class LayoutSettingModel : SettingModelBase<LayoutSetting>
    {
        public LayoutSettingModel(IMessageSender sender) : base(sender)
        {
            var s = LayoutSetting.Default;
            var factory = MessageFactory.Instance;

            CameraFov = new RPropertyMin<int>(s.CameraFov, i => SendMessage(factory.CameraFov(i)));
            EnableMidiRead = new RPropertyMin<bool>(
                s.EnableMidiRead, b => SendMessage(factory.EnableMidiRead(b))
                );
            MidiControllerVisibility = new RPropertyMin<bool>(
                s.MidiControllerVisibility, b => SendMessage(factory.MidiControllerVisibility(b))
                );

            CameraPosition = new RPropertyMin<string>(s.CameraPosition, v => SendMessage(factory.SetCustomCameraPosition(v)));
            DeviceLayout = new RPropertyMin<string>(s.DeviceLayout, v => SendMessage(factory.SetDeviceLayout(v)));

            //NOTE: ここは初期値が空なのであんまり深い意味はない。
            QuickSave1 = new RPropertyMin<string>(s.QuickSave1);
            QuickSave2 = new RPropertyMin<string>(s.QuickSave2);
            QuickSave3 = new RPropertyMin<string>(s.QuickSave3);

            HidVisibility = new RPropertyMin<bool>(s.HidVisibility, b => SendMessage(factory.HidVisibility(b)));
            SelectedTypingEffectId = new RPropertyMin<int>(s.SelectedTypingEffectId, i => SendMessage(factory.SetKeyboardTypingEffectType(i)));

            EnableDeviceFreeLayout = new RPropertyMin<bool>(false, v => SendMessage(factory.EnableDeviceFreeLayout(v)));
        }

        //NOTE: Gamepadはモデルクラス的には関連づけしないでおく: 代わりにSave/Loadの関数内で調整してもらう感じにする

        public RPropertyMin<int> CameraFov { get; }
        public RPropertyMin<bool> EnableMidiRead { get; }
        public RPropertyMin<bool> MidiControllerVisibility { get; }

        public RPropertyMin<string> CameraPosition { get; }
        public RPropertyMin<string> DeviceLayout { get; }

        //NOTE: この辺にカメラ、フリーレイアウトのフラグも用意した方がいいかも。揮発フラグだがViewModelで保持するのも違和感あるため。
        public RPropertyMin<string> QuickSave1 { get; }
        public RPropertyMin<string> QuickSave2 { get; }
        public RPropertyMin<string> QuickSave3 { get; }

        public RPropertyMin<bool> HidVisibility { get; }
        public RPropertyMin<int> SelectedTypingEffectId { get; }

        //NOTE: ファイルには保存しない
        public RPropertyMin<bool> EnableDeviceFreeLayout { get; }

        #region Reset API

        public void ResetCameraSetting()
        {
            var setting = LayoutSetting.Default;
            //NOTE: フリーカメラモードについては、もともと揮発性の設定にしているのでココでは触らない
            CameraFov.Value = setting.CameraFov;
            QuickSave1.Value = setting.QuickSave1;
            QuickSave2.Value = setting.QuickSave2;
            QuickSave3.Value = setting.QuickSave3;

            //NOTE: カメラ位置を戻すようUnityにメッセージ投げる必要もある: この後にリセットされた値を拾うのはポーリングでいい
            SendMessage(MessageFactory.Instance.ResetCameraPosition());
        }

        /// <summary>
        /// NOTE: 設定ファイルは直ちには書き換わらない。この関数を呼び出すとUnity側がよろしくレイアウトを直し、
        /// そのあと直したレイアウト情報を別途投げ返してくる
        /// </summary>
        public void ResetDeviceLayout() => SendMessage(MessageFactory.Instance.ResetDeviceLayout());

        public void ResetHidSetting()
        {
            var setting = LayoutSetting.Default;
            HidVisibility.Value = setting.HidVisibility;
            MidiControllerVisibility.Value = setting.MidiControllerVisibility;
            //NOTE: ここにGamepadのvisibilityも載ってたけど消した。必要なら書かないといけない
            SelectedTypingEffectId.Value = setting.SelectedTypingEffectId;
        }

        public void ResetMidiSetting()
        {
            var setting = LayoutSetting.Default;
            EnableMidiRead.Value = setting.EnableMidiRead;
            MidiControllerVisibility.Value = setting.MidiControllerVisibility;
        }

        public void ResetToDefault()
        {
            //TODO: UI上必要だったら復活させる必要あり。
            //Gamepad.ResetToDefault();
            ResetHidSetting();
            ResetMidiSetting();
            ResetCameraSetting();
        }

        #endregion    
    }
}
