namespace Baku.VMagicMirrorConfig
{
    public class GamepadSettingViewModel : SettingViewModelBase
    {
        internal GamepadSettingViewModel(GamepadSettingModel model, IMessageSender sender) : base(sender)
        {
            _model = model;
            ResetSettingCommand = new ActionCommand(
                () => SettingResetUtils.ResetSingleCategoryAsync(ResetToDefault)
                );
        }

        private readonly GamepadSettingModel _model;

        public RPropertyMin<bool> GamepadEnabled => _model.GamepadEnabled;
        public RPropertyMin<bool> PreferDirectInput => _model.PreferDirectInputGamepad;
        public RPropertyMin<bool> GamepadVisibility => _model.GamepadVisibility;

        //NOTE: 以下、本来ならEnum値1つで管理する方がよいが、TwoWayバインディングが簡便になるのでbool4つで代用。
        //というのをViewModel層でやってたのがModelに波及してしまった悪い例です…
        public RPropertyMin<bool> GamepadLeanNone => _model.GamepadLeanNone;
        public RPropertyMin<bool> GamepadLeanLeftStick => _model.GamepadLeanLeftStick;
        public RPropertyMin<bool> GamepadLeanRightStick => _model.GamepadLeanRightStick;
        public RPropertyMin<bool> GamepadLeanLeftButtons => _model.GamepadLeanLeftButtons;

        public RPropertyMin<bool> GamepadLeanReverseHorizontal => _model.GamepadLeanReverseHorizontal;
        public RPropertyMin<bool> GamepadLeanReverseVertical => _model.GamepadLeanReverseVertical;

        public ActionCommand ResetSettingCommand { get; }

        //NOTE: Visibilityはいじらない。UI上の表示場所がちょっと違うため。
        public override void ResetToDefault() => _model.ResetToDefault();
    }
}
