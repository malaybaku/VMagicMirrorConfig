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

        public WindowSettingViewModel BackgroundSetting { get; private set; }
        public LayoutSettingViewModel LayoutSetting { get; private set; }
        public LightSettingViewModel LightSetting { get; private set; }
        public StartupSettingViewModel StartupSetting { get; private set; }

        private string _lastVrmLoadFilePath = "";
        private bool _isDisposed = false;

        public MainWindowViewModel()
        {
            BackgroundSetting = new WindowSettingViewModel(UdpSender);
            LayoutSetting = new LayoutSettingViewModel(UdpSender);
            LightSetting = new LightSettingViewModel(UdpSender);
            StartupSetting = new StartupSettingViewModel();
        }

        private ActionCommand _loadVrmCommand;
        public ActionCommand LoadVrmCommand
            => _loadVrmCommand ?? (_loadVrmCommand = new ActionCommand(LoadVrm));

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

            bool turnOffTopMostTemporary = BackgroundSetting.TopMost;

            if (turnOffTopMostTemporary)
            {
                BackgroundSetting.TopMost = false;
            }
            
            var res = MessageBox.Show(
                "ビューアー画面のライセンスを確認してください。読み込みますか？", 
                "VRMの読み込み", 
                MessageBoxButton.OKCancel
                );

            if (res == MessageBoxResult.OK)
            {
                UdpSender.SendMessage(UdpMessageFactory.Instance.OpenVrm(dialog.FileName));
                _lastVrmLoadFilePath = dialog.FileName;
            }
            else
            {
                UdpSender.SendMessage(UdpMessageFactory.Instance.CancelLoadVrm());
            }

            if (turnOffTopMostTemporary)
            {
                BackgroundSetting.TopMost = true;
            }
        }

        public void Initialize()
        {
            if (Application.Current.MainWindow != null &&
                !DesignerProperties.GetIsInDesignMode(Application.Current.MainWindow))
            {
                Initializer.Initialize();
                LoadCurrentParameters();

                Application.Current.Exit += OnAppExit;
            }
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                SaveCurrentParameters();
                Initializer.Dispose();
                CloseUnityApp.Close();
            }
        }

        private void OnAppExit(object sender, ExitEventArgs e)
        {
            Dispose();
        }


        private void SaveCurrentParameters()
        {
            File.WriteAllText(
                GetFilePath(SpecialFileNames.LastVrmLoadedFile),
                _lastVrmLoadFilePath
                );

            BackgroundSetting.SaveSetting(GetFilePath(SpecialFileNames.Background));
            LayoutSetting.SaveSetting(GetFilePath(SpecialFileNames.Layout));
            LightSetting.SaveSetting(GetFilePath(SpecialFileNames.Light));
            StartupSetting.SaveSetting(GetFilePath(SpecialFileNames.Startup));
        }

        private void LoadCurrentParameters()
        {
            StartupSetting.LoadSetting(GetFilePath(SpecialFileNames.Startup));

            if (StartupSetting.LoadVrm)
            {
                LoadLastLoadedVrm();
            }

            if (StartupSetting.LoadBackgroundSetting)
            {
                BackgroundSetting.LoadSetting(GetFilePath(SpecialFileNames.Background));
            }

            if (StartupSetting.LoadLayoutSetting)
            {
                LayoutSetting.LoadSetting(GetFilePath(SpecialFileNames.Layout));
            }

            if (StartupSetting.LoadLightSetting)
            {
                LightSetting.LoadSetting(GetFilePath(SpecialFileNames.Light));
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
        public static readonly string Light = "_light";
        public static readonly string Startup = "_startup";
    }

    public interface IWindowViewModel : IDisposable
    {
        void Initialize();
    }
}
