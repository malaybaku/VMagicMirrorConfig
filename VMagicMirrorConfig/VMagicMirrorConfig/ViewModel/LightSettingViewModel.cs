using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;

namespace Baku.VMagicMirrorConfig
{
    using static LineParseUtils;

    public class LightSettingViewModel : SettingViewModelBase
    {
        internal LightSettingViewModel(UdpSender sender, StartupSettingViewModel startup) : base(sender, startup)
        {
            UpdateLightColor();
            UpdateBloomColor();
        }

        #region ライト

        private int _lightIntensity = 100;
        public int LightIntensity
        {
            get => _lightIntensity;
            set
            {
                if (SetValue(ref _lightIntensity, value))
                {
                    SendMessage(
                        UdpMessageFactory.Instance.LightIntensity(LightIntensity)
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

        public Color LightColor { get; private set; }

        private void UpdateLightColor()
        {
            LightColor = Color.FromRgb((byte)LightR, (byte)LightG, (byte)LightB);
            SendMessage(UdpMessageFactory.Instance.LightColor(LightR, LightG, LightB));
        }

        #endregion

        #region Bloom

        private int _bloomIntensity = 100;
        public int BloomIntensity
        {
            get => _bloomIntensity;
            set
            {
                if (SetValue(ref _bloomIntensity, value))
                {
                    SendMessage(
                        UdpMessageFactory.Instance.BloomIntensity(BloomIntensity)
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
                        UdpMessageFactory.Instance.BloomThreshold(BloomThreshold)
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

        public Color BloomColor { get; private set; }

        private void UpdateBloomColor()
        {
            BloomColor = Color.FromRgb((byte)BloomR, (byte)BloomG, (byte)BloomB);
            SendMessage(UdpMessageFactory.Instance.BloomColor(BloomR, BloomG, BloomB));
        }

        #endregion

        protected override void ResetToDefault()
        {
            LightR = 255;
            LightG = 244;
            LightB = 214;
            LightIntensity = 100;

            BloomR = 255;
            BloomG = 255;
            BloomB = 255;
            BloomIntensity = 100;
            BloomThreshold = 100;
        }

        protected override void SaveSetting()
        {
            var dialog = new SaveFileDialog()
            {
                Title = "Save Light Setting File",
                Filter = "VMagicMirror Light File(*.vmm_light)|*.vmm_light",
                DefaultExt = ".vmm_light",
                AddExtension = true,
            };
            if (dialog.ShowDialog() == true)
            {
                SaveSetting(dialog.FileName);
            }
        }

        protected override void LoadSetting()
        {
            var dialog = new OpenFileDialog()
            {
                Title = "Open Light Setting File",
                Filter = "VMagicMirror Light File(*.vmm_light)|*.vmm_light",
                Multiselect = false,
            };
            if (dialog.ShowDialog() == true)
            {
                LoadSetting(dialog.FileName);
            }
        }

        internal override void SaveSetting(string path)
        {
            File.WriteAllLines(path, new string[]
            {
                $"{nameof(LightR)}:{LightR}",
                $"{nameof(LightG)}:{LightG}",
                $"{nameof(LightB)}:{LightB}",
                $"{nameof(LightIntensity)}:{LightIntensity}",

                $"{nameof(BloomR)}:{BloomR}",
                $"{nameof(BloomG)}:{BloomG}",
                $"{nameof(BloomB)}:{BloomB}",
                $"{nameof(BloomIntensity)}:{BloomIntensity}",
                $"{nameof(BloomThreshold)}:{BloomThreshold}",
            });
        }

        internal override void LoadSetting(string path)
        {
            if (!File.Exists(path))
            {
                return;
            }

            try
            {
                var lines = File.ReadAllLines(path);
                foreach (var line in lines)
                {
                    //戻り値を拾うのはシンタックス対策
                    var _ =
                        TryReadIntParam(line, nameof(LightR), v => LightR = v) ||
                        TryReadIntParam(line, nameof(LightG), v => LightG = v) ||
                        TryReadIntParam(line, nameof(LightB), v => LightB = v) ||
                        TryReadIntParam(line, nameof(LightIntensity), v => LightIntensity = v) ||

                        TryReadIntParam(line, nameof(BloomR), v => BloomR = v) ||
                        TryReadIntParam(line, nameof(BloomG), v => BloomG = v) ||
                        TryReadIntParam(line, nameof(BloomB), v => BloomB = v) ||
                        TryReadIntParam(line, nameof(BloomIntensity), v => BloomIntensity = v) ||
                        TryReadIntParam(line, nameof(BloomThreshold), v => BloomThreshold = v);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("設定の読み込みに失敗しました: " + ex.Message);
            }

        }
    }
}
