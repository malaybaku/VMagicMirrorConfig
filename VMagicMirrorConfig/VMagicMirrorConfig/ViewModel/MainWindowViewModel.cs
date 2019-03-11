using System;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;
using Microsoft.Win32;

namespace Baku.VMagicMirrorConfig
{
    public class MainWindowViewModel : ViewModelBase, IWindowViewModel
    {
        internal ModelInitializer Initializer { get; } = new ModelInitializer();
        internal UdpSender UdpSender => Initializer.UdpSender;
        internal InputChecker InputChecker => Initializer.InputChecker;

        //とりあえず設定ファイルとか無視して常にGBでスタートしましょう
        private readonly BGSettingViewModel _bgSetting;
        private readonly LayoutSettingViewModel _layoutSetting;
        private readonly StartupSettingViewModel _startupSetting;

        private string _lastVrmLoadFilePath = "";

        public MainWindowViewModel()
        {
            _bgSetting = new BGSettingViewModel(UdpSender);
            _layoutSetting = new LayoutSettingViewModel(UdpSender);
            _startupSetting = new StartupSettingViewModel();
        }

        #region Command

        private ActionCommand _loadVrmCommand;
        public ActionCommand LoadVrmCommand
            => _loadVrmCommand ?? (_loadVrmCommand = new ActionCommand(LoadVrm));

        private ActionCommand _openBGSettingCommand;
        public ActionCommand OpenBGSettingCommand
            => _openBGSettingCommand ?? (_openBGSettingCommand = new ActionCommand(OpenBGSetting));

        private ActionCommand _openLayoutSettingCommand;
        public ActionCommand OpenLayoutSettingCommand
            => _openLayoutSettingCommand ?? (_openLayoutSettingCommand = new ActionCommand(OpenLayoutSetting));

        private ActionCommand _openStartupSettingCommand;
        public ActionCommand OpenStartupSettingCommand
            => _openStartupSettingCommand ?? (_openStartupSettingCommand = new ActionCommand(OpenStartupSetting));

        private ActionCommand _openAboutWindowCommand;
        public ActionCommand OpenAboutWindowCommand
            => _openAboutWindowCommand ?? (_openAboutWindowCommand = new ActionCommand(OpenAboutWindow));

        #endregion

        private void LoadVrm()
        {
            var dialog = new OpenFileDialog()
            {
                Title = "Open VRM file",
                Filter = "VRM files (*.vrm)|*.vrm|All files (*.*)|*.*",
                Multiselect = false,
            };

            if (!(
                dialog.ShowDialog() == true && 
                File.Exists(dialog.FileName)
                ))
            {
                return;
            }

            UdpSender.SendMessage(UdpMessageFactory.Instance.OpenVrmPreview(dialog.FileName));

            var res = MessageBox.Show(
                "ビューアー画面のライセンスを確認してください。読み込みますか？", 
                "VRMの読み込み", 
                MessageBoxButton.OKCancel
                );

            if (res == MessageBoxResult.OK)
            {
                UdpSender.SendMessage(UdpMessageFactory.Instance.OpenVrm(dialog.FileName));
            }
            _lastVrmLoadFilePath = dialog.FileName;
        }

        private void OpenBGSetting()
        {
            new BGSettingWindow()
            {
                DataContext = _bgSetting
            }.ShowDialog();
        }

        private void OpenLayoutSetting()
        {
            new LayoutSettingWindow()
            {
                DataContext = _layoutSetting
            }.ShowDialog();
        }

        private void OpenStartupSetting()
        {
            new StartupSettingWindow()
            {
                DataContext = _startupSetting
            }.ShowDialog();
        }

        private void OpenAboutWindow()
            => new AboutWindow().ShowDialog();

        public void Initialize()
        {
            if (Application.Current.MainWindow != null &&
                !DesignerProperties.GetIsInDesignMode(Application.Current.MainWindow))
            {
                Initializer.Initialize();
                LoadCurrentParameters();
            }
        }

        public void Dispose()
        {
            SaveCurrentParameters();
            Initializer.Dispose();
            CloseUnityApp.Close();
        }

        private void SaveCurrentParameters()
        {
            File.WriteAllText(
                GetFilePath(SpecialFileNames.LastVrmLoadedFile),
                _lastVrmLoadFilePath
                );

            _bgSetting.SaveSetting(GetFilePath(SpecialFileNames.Background));
            _layoutSetting.SaveSetting(GetFilePath(SpecialFileNames.Layout));
            _startupSetting.SaveSetting(GetFilePath(SpecialFileNames.Startup));
        }

        private void LoadCurrentParameters()
        {
            _startupSetting.LoadSetting(GetFilePath(SpecialFileNames.Startup));

            if (_startupSetting.LoadVrm)
            {
                LoadLastLoadedVrm();
            }

            if (_startupSetting.LoadBackgroundSetting)
            {
                _bgSetting.LoadSetting(GetFilePath(SpecialFileNames.Background));
            }

            if (_startupSetting.LoadLayoutSetting)
            {
                _layoutSetting.LoadSetting(GetFilePath(SpecialFileNames.Layout));
            }
        }

        private void LoadLastLoadedVrm()
        {
            try
            {
                string settingFilePath = GetFilePath(SpecialFileNames.LastVrmLoadedFile);
                if (!File.Exists(settingFilePath))
                {
                    return;
                }

                string vrmPath = File.ReadAllText(settingFilePath);
                if (File.Exists(vrmPath))
                {
                    UdpSender.SendMessage(UdpMessageFactory.Instance.OpenVrm(vrmPath));
                    _lastVrmLoadFilePath = vrmPath;
                }
            }
            catch (Exception)
            {
            }
        }

        private string GetFilePath(string fileName)
            => Path.Combine(
                Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                fileName
                );
    }

    public static class SpecialFileNames
    {
        public static readonly string LastVrmLoadedFile = "_currentVrm";
        public static readonly string Background = "_currentBackground";
        public static readonly string Layout = "_currentLayout";
        public static readonly string Startup = "_startup";
    }


    public interface IWindowViewModel : IDisposable
    {
        void Initialize();
    }
}
