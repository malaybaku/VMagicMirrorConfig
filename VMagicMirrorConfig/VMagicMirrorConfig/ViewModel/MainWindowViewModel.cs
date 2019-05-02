using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Xml.Serialization;
using Microsoft.Win32;

namespace Baku.VMagicMirrorConfig
{
    public class MainWindowViewModel : ViewModelBase, IWindowViewModel
    {
        internal ModelInitializer Initializer { get; } = new ModelInitializer();
        internal IMessageSender MessageSender => Initializer.MessageSender;
        internal InputChecker InputChecker => Initializer.InputChecker;

        public WindowSettingViewModel WindowSetting { get; private set; }
        public MotionSettingViewModel MotionSetting { get; private set; }
        public LayoutSettingViewModel LayoutSetting { get; private set; }
        public LightSettingViewModel LightSetting { get; private set; }

        private string _lastVrmLoadFilePath = "";
        private bool _isDisposed = false;

        public MainWindowViewModel()
        {
            WindowSetting = new WindowSettingViewModel(MessageSender);
            MotionSetting = new MotionSettingViewModel(MessageSender);
            LayoutSetting = new LayoutSettingViewModel(MessageSender);
            LightSetting = new LightSettingViewModel(MessageSender);

            AvailableLanguageNames = new ReadOnlyObservableCollection<string>(_availableLanguageNames);
        }

        #region Properties for View

        private bool _autoLoadLastLoadedVrm = false;
        public bool AutoLoadLastLoadedVrm
        {
            get => _autoLoadLastLoadedVrm;
            set => SetValue(ref _autoLoadLastLoadedVrm, value);
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

        #endregion

        #region Commands

        private ActionCommand _loadVrmCommand;
        public ActionCommand LoadVrmCommand
            => _loadVrmCommand ?? (_loadVrmCommand = new ActionCommand(LoadVrm));

        private ActionCommand _openSettingWindowCommand;
        public ActionCommand OpenSettingWindowCommand
            => _openSettingWindowCommand ?? (_openSettingWindowCommand = new ActionCommand(OpenSettingWindow));

        private ActionCommand _resetToDefaultCommand;
        public ActionCommand ResetToDefaultCommand
            => _resetToDefaultCommand ?? (_resetToDefaultCommand = new ActionCommand(ResetToDefault));

        private ActionCommand _saveSettingToFileCommand;
        public ActionCommand SaveSettingToFileCommand
            => _saveSettingToFileCommand ?? (_saveSettingToFileCommand = new ActionCommand(SaveSettingToFile));

        private ActionCommand _loadSettingFromFileCommand;
        public ActionCommand LoadSettingFromFileCommand
            => _loadSettingFromFileCommand ?? (_loadSettingFromFileCommand = new ActionCommand(LoadSettingFromFile));

        #endregion

        #region Command Impl

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

        private void OpenSettingWindow()
        {
            new SettingWindow()
            {
                Owner = Application.Current.MainWindow,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                DataContext = this,
            }.Show();
        }

        private void SaveSettingToFile()
        {
            var dialog = new SaveFileDialog()
            {
                Title = "Save VMagicMirror Setting",
                Filter = "VMagicMirror Setting File(*.vmm)|*.vmm",
                DefaultExt = ".vmm",
                AddExtension = true,
            };
            if (dialog.ShowDialog() == true)
            {
                SaveSetting(dialog.FileName, false);
            }
        }

        private void LoadSettingFromFile()
        {
            var dialog = new OpenFileDialog()
            {
                Title = "Load VMagicMirror Setting",
                Filter = "VMagicMirror Setting File (*.vmm)|*.vmm",
                Multiselect = false,
            };
            if (dialog.ShowDialog() == true)
            {
                LoadSetting(dialog.FileName, false);
            }
        }


        private void ResetToDefault()
        {
            var indication = MessageIndication.ResetSettingConfirmation(LanguageName);

            var res = MessageBox.Show(
                indication.Content,
                indication.Title,
                MessageBoxButton.OKCancel
                );

            if (res == MessageBoxResult.OK)
            {
                LightSetting.ResetToDefault();
                MotionSetting.ResetToDefault();
                LayoutSetting.ResetToDefault();
                WindowSetting.ResetToDefault();

                _lastVrmLoadFilePath = "";
            }
        }

        #endregion

        public async void Initialize()
        {
            if (Application.Current.MainWindow == null ||
                DesignerProperties.GetIsInDesignMode(Application.Current.MainWindow))
            {
                return;
            }

            Initializer.Initialize();
            LoadSetting(GetFilePath(SpecialFileNames.AutoSaveSettingFileName), true);
            if (AutoLoadLastLoadedVrm)
            {
                LoadLastLoadedVrm();
            }

            if (WindowSetting.EnableWindowInitialPlacement)
            {
                WindowSetting.MoveWindow();
            }

            //LoadCurrentParametersの時点で(もし前回保存した)言語名があればLanguageNameに入っているので、それを渡す。
            LanguageSelector.Instance.Initialize(MessageSender, LanguageName);
            //無効な言語名を渡した場合、LanguageSelector側が面倒を見てくれるので、それをチェックしている。
            //もともとLanguageNameに有効な名前が入っていた場合は、下の行では何も起きない
            LanguageName = LanguageSelector.Instance.LanguageName;

            await MotionSetting.InitializeDeviceNamesAsync();
            //await LayoutSetting.InitializeDeviceNamesAsync();
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                SaveSetting(GetFilePath(SpecialFileNames.AutoSaveSettingFileName), true);
                Initializer.Dispose();
                UnityAppCloser.Close();
            }
        }

        private void LoadLastLoadedVrm()
        {
            try
            {
                if (File.Exists(_lastVrmLoadFilePath))
                {
                    MessageSender.SendMessage(MessageFactory.Instance.OpenVrm(_lastVrmLoadFilePath));
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load last loaded VRM from {_lastVrmLoadFilePath}: {ex.Message}");
            }
        }

        private void SaveSetting(string path, bool isInternalFile)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            using (var sw = new StreamWriter(path))
            {
                new XmlSerializer(typeof(SaveData)).Serialize(sw, new SaveData()
                {
                    IsInternalSaveFile = isInternalFile,
                    LastLoadedVrmFilePath = isInternalFile ? _lastVrmLoadFilePath : "",
                    AutoLoadLastLoadedVrm = isInternalFile ? AutoLoadLastLoadedVrm : false,
                    PreferredLanguageName = isInternalFile ? LanguageName : "",
                    WindowSetting = this.WindowSetting,
                    MotionSetting = this.MotionSetting,
                    LayoutSetting = this.LayoutSetting,
                    LightSetting = this.LightSetting,
                });
            }
        }

        private void LoadSetting(string path, bool isInternalFile)
        {
            if (!File.Exists(path))
            {
                return;
            }

            try
            {
                using (var sr = new StreamReader(path))
                {
                    var serializer = new XmlSerializer(typeof(SaveData));
                    var saveData = (SaveData)serializer.Deserialize(sr);

                    if (isInternalFile && saveData.IsInternalSaveFile)
                    {
                        _lastVrmLoadFilePath = saveData.LastLoadedVrmFilePath;
                        AutoLoadLastLoadedVrm = saveData.AutoLoadLastLoadedVrm;
                        LanguageName = 
                            AvailableLanguageNames.Contains(saveData.PreferredLanguageName) ?
                            saveData.PreferredLanguageName :
                            "";
                    }

                    WindowSetting.CopyFrom(saveData.WindowSetting);
                    MotionSetting.CopyFrom(saveData.MotionSetting);
                    LayoutSetting.CopyFrom(saveData.LayoutSetting);
                    LightSetting.CopyFrom(saveData.LightSetting);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load setting file {path} : {ex.Message}");
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
