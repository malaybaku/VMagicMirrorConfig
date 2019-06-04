using System.Windows.Media;
using System.Xml.Serialization;

namespace Baku.VMagicMirrorConfig
{
    public class LightSettingViewModel : SettingViewModelBase
    {
        public LightSettingViewModel() : base()
        {
            UpdateLightColor();
            UpdateBloomColor();
        }

        internal LightSettingViewModel(IMessageSender sender) : base(sender)
        {
            UpdateLightColor();
            UpdateBloomColor();
        }

        #region Light

        private int _lightIntensity = 55;
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

        private int _lightG = 244;
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

        private int _lightB = 214;
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

        public override void ResetToDefault()
        {
            LightR = 255;
            LightG = 244;
            LightB = 214;
            LightIntensity = 55;
            LightYaw = -30;
            LightPitch = 50;

            EnableShadow = true;
            ShadowIntensity = 65;
            ShadowYaw = -20;
            ShadowPitch = 8;

            BloomR = 255;
            BloomG = 255;
            BloomB = 255;
            BloomIntensity = 50;
            BloomThreshold = 100;
        }
    }
}
