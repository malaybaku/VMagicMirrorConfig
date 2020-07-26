using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml.Serialization;

namespace Baku.VMagicMirrorConfig
{
    /// <summary>
    /// ライトと言ってるが、実際にはタイピングエフェクトを除いたエフェクトっぽい色々の設定を持ってるクラス。
    /// クラス名やメンバー名はヘタに変えると旧バージョンの設定が読めなくなるので、変更時は要注意。
    /// (たしかクラス名は改変可だが、勢いでSaveDataクラスまでリファクタリングすると後方互換でなくなってしまう)
    /// </summary>
    public class LightSettingViewModel : SettingViewModelBase
    {
        public LightSettingViewModel() : base()
        {
            UpdateLightColor();
            UpdateBloomColor();
            ImageQualityNames = new ReadOnlyObservableCollection<string>(_imageQualityNames);
        }

        internal LightSettingViewModel(IMessageSender sender) : base(sender)
        {
            ImageQualityNames = new ReadOnlyObservableCollection<string>(_imageQualityNames);
        }

        public void Initialize()
        {
            UpdateLightColor();
            UpdateBloomColor();
        }

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
        [XmlIgnore]
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

        private readonly ObservableCollection<string> _imageQualityNames 
            = new ObservableCollection<string>();
        [XmlIgnore]
        public ReadOnlyObservableCollection<string> ImageQualityNames { get; }

        #endregion

        #region Light

        private int _lightIntensity = 100;
        public int LightIntensity
        {
            get => _lightIntensity;
            set
            {
                if (SetValue(ref _lightIntensity, value))
                {
                    SendMessage(
                        MessageFactory.Instance.LightIntensity(LightIntensity)
                        );
                }
            }
        }

        private int _lightYaw = -30;
        public int LightYaw
        {
            get => _lightYaw;
            set
            {
                if (SetValue(ref _lightYaw, value))
                {
                    SendMessage(
                        MessageFactory.Instance.LightYaw(LightYaw)
                        );
                }
            }
        }

        private int _lightPitch = 50;
        public int LightPitch
        {
            get => _lightPitch;
            set
            {
                if (SetValue(ref _lightPitch, value))
                {
                    SendMessage(
                        MessageFactory.Instance.LightPitch(LightPitch)
                        );
                }
            }
        }

        private int _lightR = 255;
        public int LightR
        {
            get => _lightR;
            set
            {
                if (SetValue(ref _lightR, value))
                {
                    UpdateLightColor();
                    RaisePropertyChanged(nameof(LightColor));
                }
            }
        }

        private int _lightG = 255;
        public int LightG
        {
            get => _lightG;
            set
            {
                if (SetValue(ref _lightG, value))
                {
                    UpdateLightColor();
                    RaisePropertyChanged(nameof(LightColor));
                }
            }
        }

        private int _lightB = 255;
        public int LightB
        {
            get => _lightB;
            set
            {
                if (SetValue(ref _lightB, value))
                {
                    UpdateLightColor();
                    RaisePropertyChanged(nameof(LightColor));
                }
            }
        }

        [XmlIgnore]
        public Color LightColor { get; private set; }

        private void UpdateLightColor()
        {
            LightColor = Color.FromRgb((byte)LightR, (byte)LightG, (byte)LightB);
            SendMessage(MessageFactory.Instance.LightColor(LightR, LightG, LightB));
        }

        #endregion

        #region Shadow

        private bool _enableShadow = true;
        public bool EnableShadow
        {
            get => _enableShadow;
            set
            {
                if (SetValue(ref _enableShadow, value))
                {
                    SendMessage(
                        MessageFactory.Instance.ShadowEnable(EnableShadow)
                        );
                }
            }
        }

        private int _shadowIntensity = 65;
        public int ShadowIntensity
        {
            get => _shadowIntensity;
            set
            {
                if (SetValue(ref _shadowIntensity, value))
                {
                    SendMessage(
                        MessageFactory.Instance.ShadowIntensity(ShadowIntensity)
                        );
                }
            }
        }

        private int _shadowYaw = -20;
        public int ShadowYaw
        {
            get => _shadowYaw;
            set
            {
                if (SetValue(ref _shadowYaw, value))
                {
                    SendMessage(
                        MessageFactory.Instance.ShadowYaw(ShadowYaw)
                        );
                }
            }
        }

        private int _shadowPitch = 8;
        public int ShadowPitch
        {
            get => _shadowPitch;
            set
            {
                if (SetValue(ref _shadowPitch, value))
                {
                    SendMessage(
                        MessageFactory.Instance.ShadowPitch(ShadowPitch)
                        );
                }
            }
        }

        private int _shadowDepthOffset = 40;
        public int ShadowDepthOffset
        {
            get => _shadowDepthOffset;
            set
            {
                if (SetValue(ref _shadowDepthOffset, value))
                {
                    SendMessage(
                        MessageFactory.Instance.ShadowDepthOffset(ShadowDepthOffset)
                        );
                }
            }
        }

        #endregion

        #region Bloom

        private int _bloomIntensity = 50;
        public int BloomIntensity
        {
            get => _bloomIntensity;
            set
            {
                if (SetValue(ref _bloomIntensity, value))
                {
                    SendMessage(
                        MessageFactory.Instance.BloomIntensity(BloomIntensity)
                        );
                }
            }
        }

        private int _bloomThreshold = 100;
        public int BloomThreshold
        {
            get => _bloomThreshold;
            set
            {
                if (SetValue(ref _bloomThreshold, value))
                {
                    SendMessage(
                        MessageFactory.Instance.BloomThreshold(BloomThreshold)
                        );
                }
            }
        }

        private int _bloomR = 255;
        public int BloomR
        {
            get => _bloomR;
            set
            {
                if (SetValue(ref _bloomR, value))
                {
                    UpdateBloomColor();
                    RaisePropertyChanged(nameof(BloomColor));
                }
            }
        }

        private int _bloomG = 255;
        public int BloomG
        {
            get => _bloomG;
            set
            {
                if (SetValue(ref _bloomG, value))
                {
                    UpdateBloomColor();
                    RaisePropertyChanged(nameof(BloomColor));
                }
            }
        }

        private int _bloomB = 255;
        public int BloomB
        {
            get => _bloomB;
            set
            {
                if (SetValue(ref _bloomB, value))
                {
                    UpdateBloomColor();
                    RaisePropertyChanged(nameof(BloomColor));
                }
            }
        }

        [XmlIgnore]
        public Color BloomColor { get; private set; }

        private void UpdateBloomColor()
        {
            BloomColor = Color.FromRgb((byte)BloomR, (byte)BloomG, (byte)BloomB);
            SendMessage(MessageFactory.Instance.BloomColor(BloomR, BloomG, BloomB));
        }

        #endregion

        #region Wind

        private bool _enableWind = true;
        public bool EnableWind
        {
            get => _enableWind;
            set
            {
                if (SetValue(ref _enableWind, value))
                {
                    SendMessage(MessageFactory.Instance.WindEnable(EnableWind));
                }
            }
        }

        private int _windStrengh = 100;
        public int WindStrength
        {
            get => _windStrengh;
            set
            {
                if (SetValue(ref _windStrengh, value))
                {
                    SendMessage(MessageFactory.Instance.WindStrength(WindStrength));
                }
            }
        }

        private int _windInterval = 100;
        public int WindInterval
        {
            get => _windInterval;
            set
            {
                if (SetValue(ref _windInterval, value))
                {
                    SendMessage(MessageFactory.Instance.WindInterval(WindInterval));
                }
            }
        }

        private int _windYaw = 90;
        public int WindYaw
        {
            get => _windYaw;
            set
            {
                if (SetValue(ref _windYaw, value))
                {
                    SendMessage(
                        MessageFactory.Instance.WindYaw(WindYaw)
                        );
                }
            }
        }

        #endregion

        #region Reset API

        private ActionCommand? _resetLightSettingCommand = null;
        public ActionCommand ResetLightSettingCommand
            => _resetLightSettingCommand ??= new ActionCommand(
                () => SettingResetUtils.ResetSingleCategorySettingAsync(ResetLightSetting)
                );

        private ActionCommand? _resetShadowSettingCommand = null;
        public ActionCommand ResetShadowSettingCommand
            => _resetShadowSettingCommand ??= new ActionCommand(
                () => SettingResetUtils.ResetSingleCategorySettingAsync(ResetShadowSetting)
                );

        private ActionCommand? _resetBloomSettingCommand = null;
        public ActionCommand ResetBloomSettingCommand
            => _resetBloomSettingCommand ??= new ActionCommand(
                () => SettingResetUtils.ResetSingleCategorySettingAsync(ResetBloomSetting)
                );

        private ActionCommand? _resetWindSettingCommand = null;
        public ActionCommand ResetWindSettingCommand
            => _resetWindSettingCommand ??= new ActionCommand(
                () => SettingResetUtils.ResetSingleCategorySettingAsync(ResetWindSetting)
                );

        private void ResetLightSetting()
        {
            LightR = 255;
            LightG = 255;
            LightB = 255;
            LightIntensity = 100;
            LightYaw = -30;
            LightPitch = 50;
        }

        private void ResetShadowSetting()
        {
            EnableShadow = true;
            ShadowIntensity = 65;
            ShadowYaw = -20;
            ShadowPitch = 8;
            ShadowDepthOffset = 40;
        }

        private void ResetBloomSetting()
        {
            BloomR = 255;
            BloomG = 255;
            BloomB = 255;
            BloomIntensity = 50;
            BloomThreshold = 100;
        }

        private void ResetWindSetting()
        {
            EnableWind = true;
            WindStrength = 100;
            WindInterval = 100;
            WindYaw = 90;
        }

        public override void ResetToDefault()
        {
            ResetLightSetting();
            ResetShadowSetting();
            ResetBloomSetting();
        }

        #endregion
    }
}
