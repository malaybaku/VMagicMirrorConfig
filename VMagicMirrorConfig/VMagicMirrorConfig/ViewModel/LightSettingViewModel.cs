using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Baku.VMagicMirrorConfig
{
    /// <summary>
    /// ライトと言ってるが、実際にはタイピングエフェクトを除いたエフェクトっぽい色々の設定を持ってるクラス。
    /// クラス名やメンバー名はヘタに変えると旧バージョンの設定が読めなくなるので、変更時は要注意。
    /// (たしかクラス名は改変可だが、勢いでSaveDataクラスまでリファクタリングすると後方互換でなくなってしまう)
    /// </summary>
    public class LightSettingViewModel : SettingViewModelBase
    {
        internal LightSettingViewModel(LightSettingSync model, IMessageSender sender) : base(sender)
        {
            _model = model;
            ImageQualityNames = new ReadOnlyObservableCollection<string>(_imageQualityNames);

            _lightColor = Color.FromRgb((byte)model.LightR.Value, (byte)model.LightG.Value, (byte)model.LightB.Value);
            model.LightR.PropertyChanged += (_, __) => UpdateLightColor();
            model.LightG.PropertyChanged += (_, __) => UpdateLightColor();
            model.LightB.PropertyChanged += (_, __) => UpdateLightColor();

            _bloomColor = Color.FromRgb((byte)model.BloomR.Value, (byte)model.BloomG.Value, (byte)model.BloomB.Value);
            model.BloomR.PropertyChanged += (_, __) => UpdateBloomColor();
            model.BloomG.PropertyChanged += (_, __) => UpdateBloomColor();
            model.BloomB.PropertyChanged += (_, __) => UpdateBloomColor();            

            ResetLightSettingCommand = new ActionCommand(
                () => SettingResetUtils.ResetSingleCategoryAsync(model.ResetLightSetting)
                );
            ResetShadowSettingCommand = new ActionCommand(
                () => SettingResetUtils.ResetSingleCategoryAsync(_model.ResetShadowSetting)
                );
            ResetBloomSettingCommand = new ActionCommand(
                () => SettingResetUtils.ResetSingleCategoryAsync(_model.ResetBloomSetting)
                );
            ResetWindSettingCommand = new ActionCommand(
                () => SettingResetUtils.ResetSingleCategoryAsync(_model.ResetWindSetting)
                );

            //最初の時点で不整合しなければ後は何でもOK
            UpdateLightColor();
            UpdateBloomColor();
        }

        private readonly LightSettingSync _model;

        public async Task InitializeQualitySelectionsAsync()
        {
            string res = await SendQueryAsync(MessageFactory.Instance.GetQualitySettingsInfo());
            var info = ImageQualityInfo.ParseFromJson(res);
            if (info.ImageQualityNames != null &&
                info.CurrentQualityIndex >= 0 &&
                info.CurrentQualityIndex < info.ImageQualityNames.Length
                )
            {
                foreach (var name in info.ImageQualityNames)
                {
                    _imageQualityNames.Add(name);
                }
                ImageQuality = info.ImageQualityNames[info.CurrentQualityIndex];
            }
        }

        #region ImageQuality

        //NOTE: 画質設定はもともとUnityが持っており、かつShift+ダブルクリックの起動によって書き換えられる可能性があるので、
        //WPF側からは揮発性データのように扱う

        private string _imageQuality = "";
        public string ImageQuality
        {
            get => _imageQuality;
            set
            {
                if (SetValue(ref _imageQuality, value))
                {
                    SendMessage(MessageFactory.Instance.SetImageQuality(ImageQuality));
                }
            }
        }

        private readonly ObservableCollection<string> _imageQualityNames = new ObservableCollection<string>();
        public ReadOnlyObservableCollection<string> ImageQualityNames { get; }

        #endregion

        #region Light

        public RPropertyMin<int> LightIntensity => _model.LightIntensity;
        public RPropertyMin<int> LightYaw => _model.LightYaw;
        public RPropertyMin<int> LightPitch => _model.LightPitch;

        //TODO: ColorPickerの対応方法
        public RPropertyMin<int> LightR => _model.LightR;
        public RPropertyMin<int> LightG => _model.LightG;
        public RPropertyMin<int> LightB => _model.LightB;

        private Color _lightColor;
        public Color LightColor 
        {
            get => _lightColor;
            set
            {
                if (SetValue(ref _lightColor, value))
                {
                    LightR.Value = value.R;
                    LightG.Value = value.G;
                    LightB.Value = value.B;
                }
            }
        }

        //NOTE: 色が変わったら表示を追従させるだけでいいのがポイント。メッセージ送信自体はモデル側で行う
        private void UpdateLightColor() 
            => LightColor = Color.FromRgb((byte)LightR.Value, (byte)LightG.Value, (byte)LightB.Value);

        #endregion

        #region Shadow

        public RPropertyMin<bool> EnableShadow => _model.EnableShadow;
        public RPropertyMin<int> ShadowIntensity => _model.ShadowIntensity;
        public RPropertyMin<int> ShadowYaw => _model.ShadowYaw;
        public RPropertyMin<int> ShadowPitch => _model.ShadowPitch;
        public RPropertyMin<int> ShadowDepthOffset => _model.ShadowDepthOffset;

        #endregion

        #region Bloom

        public RPropertyMin<int> BloomIntensity => _model.BloomIntensity;
        public RPropertyMin<int> BloomThreshold => _model.BloomThreshold;

        public RPropertyMin<int> BloomR => _model.BloomR;
        public RPropertyMin<int> BloomG => _model.BloomG;
        public RPropertyMin<int> BloomB => _model.BloomB;

        private Color _bloomColor;
        public Color BloomColor 
        {
            get => _bloomColor;
            set
            {
                if (SetValue(ref _bloomColor, value))
                {
                    BloomR.Value = value.R;
                    BloomG.Value = value.G;
                    BloomB.Value = value.B;
                }
            }
        }

        private void UpdateBloomColor() 
            => BloomColor = Color.FromRgb((byte)BloomR.Value, (byte)BloomG.Value, (byte)BloomB.Value);

        #endregion

        #region Wind

        public RPropertyMin<bool> EnableWind => _model.EnableWind;
        public RPropertyMin<int> WindStrength => _model.WindStrength;
        public RPropertyMin<int> WindInterval => _model.WindInterval;
        public RPropertyMin<int> WindYaw => _model.WindYaw;

        #endregion


        public ActionCommand ResetLightSettingCommand { get; }
        public ActionCommand ResetShadowSettingCommand { get; }
        public ActionCommand ResetBloomSettingCommand { get; }
        public ActionCommand ResetWindSettingCommand { get; }

    }
}
