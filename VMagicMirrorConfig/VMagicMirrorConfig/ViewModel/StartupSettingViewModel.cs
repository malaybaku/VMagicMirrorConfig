using System;
using System.IO;
using System.Windows;

namespace Baku.VMagicMirrorConfig
{
    using static LineParseUtils;

    public class StartupSettingViewModel : ViewModelBase
    {
        private bool _loadVrm = false;
        public bool LoadVrm
        {
            get => _loadVrm;
            set => SetValue(ref _loadVrm, value);
        }

        private bool _loadBackgroundSetting = false;
        public bool LoadBackgroundSetting
        {
            get => _loadBackgroundSetting;
            set => SetValue(ref _loadBackgroundSetting, value);
        }

        private bool _loadLayoutSetting = false;
        public bool LoadLayoutSetting
        {
            get => _loadLayoutSetting;
            set => SetValue(ref _loadLayoutSetting, value);
        }

        private bool _loadLightSetting = false;
        public bool LoadLightSetting
        {
            get => _loadLightSetting;
            set => SetValue(ref _loadLightSetting, value);
        }

        internal void SaveSetting(string path)
        {
            File.WriteAllLines(path, new string[]
            {
                $"{nameof(LoadVrm)}:{LoadVrm}",
                $"{nameof(LoadBackgroundSetting)}:{LoadBackgroundSetting}",
                $"{nameof(LoadLayoutSetting)}:{LoadLayoutSetting}",
                $"{nameof(LoadLightSetting)}:{LoadLightSetting}",
            });
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
                        TryReadBoolParam(line, nameof(LoadVrm), b => LoadVrm = b) ||
                        TryReadBoolParam(line, nameof(LoadBackgroundSetting), b => LoadBackgroundSetting = b) ||
                        TryReadBoolParam(line, nameof(LoadLayoutSetting), b => LoadLayoutSetting = b) ||
                        TryReadBoolParam(line, nameof(LoadLightSetting), b => LoadLightSetting = b);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("スタートアップ設定の読み込みに失敗しました: " + ex.Message);
            }

        }
    }
}
