using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using Microsoft.Win32;

namespace Baku.VMagicMirrorConfig
{
    using static LineParseUtils;

    //モデルが小さいのでVMと完全癒着してる点に注意(VMというよりNotifiableなモデル)
    public class BGSettingViewModel : ViewModelBase
    {
        internal BGSettingViewModel(UdpSender sender)
        {
            _sender = sender;
            UpdateColor();
        }
        private readonly UdpSender _sender;

        private int _r = 0;
        public int R
        {
            get => _r;
            set
            {
                if (SetValue(ref _r, value))
                {
                    UpdateColor();
                    RaisePropertyChanged(nameof(Color));
                }
            }
        }

        private int _g = 255;
        public int G
        {
            get => _g;
            set
            {
                if (SetValue(ref _g, value))
                {
                    UpdateColor();
                    RaisePropertyChanged(nameof(Color));
                }
            }
        }

        private int _b = 0;
        public int B
        {
            get => _b;
            set
            {
                if (SetValue(ref _b, value))
                {
                    UpdateColor();
                    RaisePropertyChanged(nameof(Color));
                }
            }
        }

        public Color Color { get; private set; }

        private int _lightIntensity = 100;
        public int LightIntensity
        {
            get => _lightIntensity;
            set
            {
                if (SetValue(ref _lightIntensity, value))
                {
                    _sender?.SendMessage(
                        UdpMessageFactory.Instance.LightIntensity(LightIntensity)
                        );
                }
            }
        }
   
        private void UpdateColor()
        {
            Color = Color.FromArgb(255, (byte)R, (byte)G, (byte)B);
            _sender.SendMessage(
                 UdpMessageFactory.Instance.Chromakey(255, R, G, B)
                 );
        }

        private ActionCommand _resetToDefaultCommand;
        public ActionCommand ResetToDefaultCommand
            => _resetToDefaultCommand ?? (_resetToDefaultCommand = new ActionCommand(ResetToDefault));

        private void ResetToDefault()
        {
            R = 0;
            G = 255;
            B = 0;
            LightIntensity = 100;
        }

        private ActionCommand _saveSettingCommand;
        public ActionCommand SaveSettingCommand
            => _saveSettingCommand ?? (_saveSettingCommand = new ActionCommand(SaveSetting));

        private ActionCommand _loadSettingCommand;
        public ActionCommand LoadSettingCommand
            => _loadSettingCommand ?? (_loadSettingCommand = new ActionCommand(LoadSetting));

        //※ここから下はVMじゃなくてモデルに書くような内容だけど、プログラムが小さいのであえて分けない

        internal void SaveSetting()
        {
            var dialog = new SaveFileDialog()
            {
                Title = "Save Bakground Setting File",
                Filter = "VMagicMirror Background File(*.vmm_background)|*.vmm_background",
                DefaultExt = ".vmm_background",
                AddExtension = true,
            };
            if (dialog.ShowDialog() == true)
            {
                SaveSetting(dialog.FileName);
            }
        }

        internal void SaveSetting(string path)
        {
            File.WriteAllLines(path, new string[]
            {
                $"{nameof(R)}:{R}",
                $"{nameof(G)}:{G}",
                $"{nameof(B)}:{B}",
                $"{nameof(LightIntensity)}:{LightIntensity}",
            });
        }

        internal void LoadSetting()
        {
            var dialog = new OpenFileDialog()
            {
                Title = "Open Background Setting File",
                Filter = "VMagicMirror Background File(*.vmm_background)|*.vmm_background",
                Multiselect = false,
            };
            if (dialog.ShowDialog() == true)
            {
                LoadSetting(dialog.FileName);
            }
        }

        internal void LoadSetting(string path)
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
                        TryReadIntParam(line, nameof(R), v => R = v) ||
                        TryReadIntParam(line, nameof(G), v => G = v) ||
                        TryReadIntParam(line, nameof(B), v => B = v) ||
                        TryReadIntParam(line, nameof(LightIntensity), v => LightIntensity = v);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("設定の読み込みに失敗しました: " + ex.Message);
            }

        }

    }
}
