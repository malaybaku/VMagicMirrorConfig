using MaterialDesignThemes.Wpf;

namespace Baku.VMagicMirrorConfig
{
    public class LayoutSettingViewModel : SettingViewModelBase
    {
        internal LayoutSettingViewModel(LayoutSettingModel model, IMessageSender sender, IMessageReceiver receiver) : base(sender)
        {
            _model = model;

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

            EnableFreeCameraMode = new RPropertyMin<bool>(false, b => OnEnableFreeCameraModeChanged(b));
        }

        private readonly LayoutSettingModel _model;

        private void OnReceiveCommand(object? sender, CommandReceivedEventArgs e)
        {
            if (e.Command == ReceiveMessageNames.UpdateDeviceLayout)
            {
                //NOTE: Unity側から来た値なため、送り返さないでよいことに注意
                _model.DeviceLayout.SilentSet(e.Args);
            }
        }

        public RPropertyMin<int> CameraFov => _model.CameraFov;

        //NOTE: この値は揮発性が高いのでVMでもいいかな、という裁量。逆にモデルに実態があってもOK
        public RPropertyMin<bool> EnableFreeCameraMode { get; }

        private async void OnEnableFreeCameraModeChanged(bool value)
        {
            SendMessage(MessageFactory.Instance.EnableFreeCameraMode(EnableFreeCameraMode.Value));
            //トグルさげた場合: 切った時点のカメラポジションを取得、保存する。
            //TODO:
            //フリーレイアウト中の切り替えと若干相性が悪いので、
            //もう少し方法が洗練しているといい…のかもしれない。
            if (!value)
            {
                string response = await SendQueryAsync(MessageFactory.Instance.CurrentCameraPosition());
                if (!string.IsNullOrWhiteSpace(response))
                {
                    _model.CameraPosition.SilentSet(response);
                }
            }
        }

        public RPropertyMin<bool> EnableMidiRead => _model.EnableMidiRead;
        public RPropertyMin<bool> MidiControllerVisibility => _model.MidiControllerVisibility;

        //NOTE: カメラ位置、デバイスレイアウト、クイックセーブした視点については、ユーザーが直接いじる想定ではない

        #region 視点のクイックセーブ/ロード

        //NOTE: これらの値はUIで「有効なデータを持ってるかどうか」という間接的な情報として使う
        public RPropertyMin<string> QuickSave1 => _model.QuickSave1;
        public RPropertyMin<string> QuickSave2 => _model.QuickSave2;
        public RPropertyMin<string> QuickSave3 => _model.QuickSave3;

        public ActionCommand<string> QuickSaveViewPointCommand { get; }
        public ActionCommand<string> QuickLoadViewPointCommand { get; }

        //TODO: この辺の処理はモデルに移動してよい
        

        #endregion

        public ActionCommand ResetCameraPositionCommand { get; }

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
