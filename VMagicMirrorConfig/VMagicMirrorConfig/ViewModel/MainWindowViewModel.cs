using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

namespace Baku.VMagicMirrorConfig
{
    public class MainWindowViewModel : ViewModelBase, IWindowViewModel
    {
        internal ModelInitializer Initializer { get; } = new ModelInitializer();
        internal IMessageSender MessageSender => Initializer.MessageSender;
        internal InputChecker InputChecker => Initializer.InputChecker;

        public WindowSettingViewModel WindowSetting { get; private set; }
        public LayoutSettingViewModel LayoutSetting { get; private set; }
        public LightSettingViewModel LightSetting { get; private set; }
        public StartupSettingViewModel StartupSetting { get; private set; }

        private string _lastVrmLoadFilePath = "";
        private bool _isDisposed = false;

        public MainWindowViewModel()
        {
            StartupSetting = new StartupSettingViewModel();
            WindowSetting = new WindowSettingViewModel(MessageSender, StartupSetting);
            LayoutSetting = new LayoutSettingViewModel(MessageSender, StartupSetting);
            LightSetting = new LightSettingViewModel(MessageSender, StartupSetting);

            AvailableLanguageNames = new ReadOnlyObservableCollection<string>(_availableLanguageNames);
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

            MessageSender.SendMessage(MessageFactory.Instance.OpenVrmPreview(dialog.FileName));

            bool turnOffTopMostTemporary = WindowSetting.TopMost;

            if (turnOffTopMostTemporary)
            {
                WindowSetting.TopMost = false;
            }

            var indication = MessageIndication.LoadVrmConfirmation(LanguageName);

            var res = MessageBox.Show(
                indication.Content,
                indication.Title,
                MessageBoxButton.OKCancel
                );

            if (res == MessageBoxResult.OK)
            {
                MessageSender.SendMessage(MessageFactory.Instance.OpenVrm(dialog.FileName));
                _lastVrmLoadFilePath = dialog.FileName;
            }
            else
            {
                MessageSender.SendMessage(MessageFactory.Instance.CancelLoadVrm());
            }

            if (turnOffTopMostTemporary)
            {
                WindowSetting.TopMost = true;
            }
        }

        public async void Initialize()
        {
            if (Application.Current.MainWindow == null ||
                DesignerProperties.GetIsInDesignMode(Application.Current.MainWindow))
            {
                return;
            }

            Initializer.Initialize();
            LoadCurrentParameters();

            if (WindowSetting.EnableWindowInitialPlacement)
            {
                WindowSetting.MoveWindow();
            }

            LanguageSelector.Instance.Initialize(MessageSender);
            LanguageName = LanguageSelector.Instance.LanguageName;

            await LayoutSetting.InitializeAvailableMicrophoneNamesAsync();
        }

        private readonly ObservableCollection<string> _availableLanguageNames
            = new ObservableCollection<string>()
        {
            "Japanese",
            "English",
        };
        public ReadOnlyObservableCollection<string> AvailableLanguageNames { get; }

        private string _languageName = nameof(Languages.Japanese);
        public string LanguageName
        {
            get => _languageName;
            set
            {
                if (SetValue(ref _languageName, value))
                {
                    LanguageSelector.Instance.LanguageName = LanguageName;
                }
            }
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                SaveCurrentParameters();
                Initializer.Dispose();
                UnityAppCloser.Close();
            }
        }

        private void SaveCurrentParameters()
        {
            File.WriteAllText(
                GetFilePath(SpecialFileNames.LastVrmLoadedFile),
                _lastVrmLoadFilePath
                );

            WindowSetting.SaveSetting(GetFilePath(SpecialFileNames.Background));
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
                WindowSetting.LoadSetting(GetFilePath(SpecialFileNames.Background));
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
                    MessageSender.SendMessage(MessageFactory.Instance.OpenVrm(vrmPath));
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

    public interface IWindowViewModel : IDisposable
    {
        void Initialize();
    }
}
