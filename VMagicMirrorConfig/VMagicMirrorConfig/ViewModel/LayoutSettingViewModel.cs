using MaterialDesignThemes.Wpf;

namespace Baku.VMagicMirrorConfig
{
    public class LayoutSettingViewModel : SettingViewModelBase
    {
        internal LayoutSettingViewModel(
            LayoutSettingSync model, GamepadSettingSync gamepadModel, IMessageSender sender, IMessageReceiver receiver
            ) : base(sender)
        {
            _model = model;
            _gamepadModel = gamepadModel;

            _typingEffectItem = TypingEffectSelections[0];
            receiver.ReceivedCommand += OnReceiveCommand;

            QuickSaveViewPointCommand = new ActionCommand<string>(async s => await _model.QuickSaveViewPoint(s));
            QuickLoadViewPointCommand = new ActionCommand<string>(_model.QuickLoadViewPoint);
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

        private readonly LayoutSettingSync _model;
        //NOTE: ゲームパッド設定(表示/非表示)も使うため、ここに記載。ちょっと例外的な措置ではある
        private readonly GamepadSettingSync _gamepadModel;

        private void OnReceiveCommand(object? sender, CommandReceivedEventArgs e)
        {
            if (e.Command == ReceiveMessageNames.UpdateDeviceLayout)
            {
                //NOTE: Unity側から来た値なため、送り返さないでよいことに注意
                _model.DeviceLayout.SilentSet(e.Args);
            }
        }

        public RPropertyMin<int> CameraFov => _model.CameraFov;
        public RPropertyMin<bool> EnableFreeCameraMode => _model.EnableFreeCameraMode;

        public RPropertyMin<bool> EnableMidiRead => _model.EnableMidiRead;

        //NOTE: カメラ位置、デバイスレイアウト、クイックセーブした視点については、ユーザーが直接いじる想定ではない

        #region 視点のクイックセーブ/ロード

        //NOTE: これらの値はUIで「有効なデータを持ってるかどうか」という間接的な情報として使う
        public RPropertyMin<string> QuickSave1 => _model.QuickSave1;
        public RPropertyMin<string> QuickSave2 => _model.QuickSave2;
        public RPropertyMin<string> QuickSave3 => _model.QuickSave3;

        public ActionCommand<string> QuickSaveViewPointCommand { get; }
        public ActionCommand<string> QuickLoadViewPointCommand { get; }

        #endregion

        public ActionCommand ResetCameraPositionCommand { get; }


        // デバイス類の表示/非表示
        public RPropertyMin<bool> HidVisibility => _model.HidVisibility;
        public RPropertyMin<bool> MidiControllerVisibility => _model.MidiControllerVisibility;
        public RPropertyMin<bool> GamepadVisibility => _gamepadModel.GamepadVisibility;

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


        public ActionCommand ResetDeviceLayoutCommand { get; }
        public ActionCommand ResetHidSettingCommand { get; }
        public ActionCommand ResetCameraSettingCommand { get; }
        public ActionCommand ResetMidiSettingCommand { get; }

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
