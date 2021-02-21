using System.Windows.Media;

namespace Baku.VMagicMirrorConfig
{
    public class WindowSettingViewModel : SettingViewModelBase
    {
        internal WindowSettingViewModel(WindowSettingSync model, IMessageSender sender) : base(sender)
        {
            _model = model;

            void UpdatePickerColor() =>
                PickerColor = Color.FromRgb((byte)_model.R.Value, (byte)_model.G.Value, (byte)_model.B.Value);
            _model.R.PropertyChanged += (_, __) => UpdatePickerColor();                 
            _model.G.PropertyChanged += (_, __) => UpdatePickerColor(); 
            _model.B.PropertyChanged += (_, __) => UpdatePickerColor();


            ResetBackgroundColorSettingCommand = new ActionCommand(
                () => SettingResetUtils.ResetSingleCategoryAsync(_model.ResetBackgroundColor)
                );
            ResetWindowPositionCommand = new ActionCommand(_model.ResetWindowPosition);
            ResetOpacitySettingCommand = new ActionCommand(
                () => SettingResetUtils.ResetSingleCategoryAsync(_model.ResetOpacity)
                );
        }

        private readonly WindowSettingSync _model;

        public RPropertyMin<int> R => _model.R;
        public RPropertyMin<int> G => _model.G;
        public RPropertyMin<int> B => _model.B;

        private Color _pickerColor;
        /// <summary> ColorPickerに表示する、Alphaを考慮しない背景色を取得、設定します。 </summary>
        public Color PickerColor
        {
            get => _pickerColor;
            set
            {
                if (SetValue(ref _pickerColor, value))
                {
                    R.Value = PickerColor.R;
                    G.Value = PickerColor.G;
                    B.Value = PickerColor.B;
                }
            }
        }

        public RPropertyMin<bool> IsTransparent => _model.IsTransparent;
        public RPropertyMin<bool> WindowDraggable => _model.WindowDraggable;
        public RPropertyMin<bool> TopMost => _model.TopMost;

        public RPropertyMin<int> WholeWindowTransparencyLevel => _model.WholeWindowTransparencyLevel;
        public RPropertyMin<int> AlphaValueOnTransparent => _model.AlphaValueOnTransparent;

        public ActionCommand ResetWindowPositionCommand { get; }
        public ActionCommand ResetBackgroundColorSettingCommand { get; }
        public ActionCommand ResetOpacitySettingCommand { get; }
    }
}
