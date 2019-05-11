using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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

        private bool _activateOnStartup = false;
        public bool ActivateOnStartup
        {
            get => _activateOnStartup;
            set
            {
                if (SetValue(ref _activateOnStartup, value))
                {
                    new StartupRegistrySetting().SetThisVersionRegister(value);
                    if (value)
                    {
                        OtherVersionRegisteredOnStartup = false;
                    }
                }
            }
        }

        private bool _otherVersionRegisteredOnStartup = false;
        public bool OtherVersionRegisteredOnStartup
        {
            get => _otherVersionRegisteredOnStartup;
            private set => SetValue(ref _otherVersionRegisteredOnStartup, value);
        }

        private string _lastVrmLoadFilePath = "";
        private bool _isDisposed = false;

        public MainWindowViewModel()
        {
            WindowSetting = new WindowSettingViewModel(MessageSender);
            MotionSetting = new MotionSettingViewModel(MessageSender, Initializer.MessageReceiver);
            LayoutSetting = new LayoutSettingViewModel(MessageSender, Initializer.MessageReceiver);
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

        private bool _autoAdjustEyebrowOnLoaded = true;
        public bool AutoAdjustEyebrowOnLoaded
        {
            get => _autoAdjustEyebrowOnLoaded;
            set => SetValue(ref _autoAdjustEyebrowOnLoaded, value);
        }

        #endregion

        #region Commands

        private ActionCommand _loadVrmCommand;
        public ActionCommand LoadVrmCommand
            => _loadVrmCommand ?? (_loadVrmCommand = new ActionCommand(LoadVrm));

        private ActionCommand _openVRoidHubCommand;
        public ActionCommand OpenVRoidHubCommand
            => _openVRoidHubCommand ?? (_openVRoidHubCommand = new ActionCommand(OpenVRoidHub));

        private ActionCommand _autoAdjustCommand;
        public ActionCommand AutoAdjustCommand
            => _autoAdjustCommand ?? (_autoAdjustCommand = new ActionCommand(AutoAdjust));

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

        private ActionCommand _loadPrevSettingCommand;
        public ActionCommand LoadPrevSettingCommand
            => _loadPrevSettingCommand ?? (_loadPrevSettingCommand = new ActionCommand(LoadPrevSetting));

        #endregion

        #region Command Impl

        private void LoadVrm()
        {
            bool turnOffTopMostTemporary = WindowSetting.TopMost;
            if (turnOffTopMostTemporary)
            {
                WindowSetting.TopMost = false;
            }

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
                if (turnOffTopMostTemporary)
                {
                    WindowSetting.TopMost = true;
                }
                return;
            }

            MessageSender.SendMessage(MessageFactory.Instance.OpenVrmPreview(dialog.FileName));


            var indication = MessageIndication.LoadVrmConfirmation(LanguageName);

            var res = MessageBox.Show(
                Application.Current.MainWindow,
                indication.Content,
                indication.Title,
                MessageBoxButton.OKCancel
                );

            if (res == MessageBoxResult.OK)
            {
                MessageSender.SendMessage(MessageFactory.Instance.OpenVrm(dialog.FileName));
                _lastVrmLoadFilePath = dialog.FileName;
                if (AutoAdjustEyebrowOnLoaded)
                {
                    MessageSender.SendMessage(MessageFactory.Instance.RequestAutoAdjustEyebrow());
                }
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

        private void OpenVRoidHub()
        {
            //=> MessageSender.SendMessage(MessageFactory.Instance.AccessToVRoidHub());
            Process.Start("https://hub.vroid.com/");
        }

        private void AutoAdjust() => MessageSender.SendMessage(MessageFactory.Instance.RequestAutoAdjust());

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
                Application.Current.MainWindow,
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

        private void LoadPrevSetting()
        {
            var dialog = new OpenFileDialog()
            {
                Title = "Select Previous Version VMagicMirror.exe",
                Filter = "VMagicMirror.exe|VMagicMirror.exe",
                Multiselect = false,
            };
            if (dialog.ShowDialog() != true)
            {
                return;
            }

            try
            {
                string savePath = Path.Combine(
                    Path.GetDirectoryName(dialog.FileName),
                    "ConfigApp",
                    "_autosave"
                    );

                LoadSetting(savePath, true);
            }
            catch(Exception ex)
            {
                var indication = MessageIndication.ErrorLoadSetting(LanguageName);
                MessageBox.Show(
                    indication.Title,
                    indication.Content + ex.Message
                    );
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
            WindowSetting.MoveWindow();

            //LoadCurrentParametersの時点で(もし前回保存した)言語名があればLanguageNameに入っているので、それを渡す。
            LanguageSelector.Instance.Initialize(MessageSender, LanguageName);
            //無効な言語名を渡した場合、LanguageSelector側が面倒を見てくれるので、それをチェックしている。
            //もともとLanguageNameに有効な名前が入っていた場合は、下の行では何も起きない
            LanguageName = LanguageSelector.Instance.LanguageName;

            await MotionSetting.InitializeDeviceNamesAsync();

            Initializer.CameraPositionChecker.Start(
                2000,
                data => LayoutSetting.CameraPosition = data
                );

            var regSetting = new StartupRegistrySetting();
            _activateOnStartup = regSetting.CheckThisVersionRegistered();
            if (_activateOnStartup)
            {
                RaisePropertyChanged(nameof(ActivateOnStartup));
            }
            OtherVersionRegisteredOnStartup = regSetting.CheckOtherVersionRegistered();
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                //Unity側閉じたときはこのタイミングだとちょっと怪しいかも(基本起こらないが)
                WindowSetting.FetchUnityWindowPosition();
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
                    if (AutoAdjustEyebrowOnLoaded)
                    {
                        MessageSender.SendMessage(MessageFactory.Instance.RequestAutoAdjustEyebrow());
                    }
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
                    AdjustEyebrowOnLoaded = AutoAdjustEyebrowOnLoaded,
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

                    AutoAdjustEyebrowOnLoaded = saveData.AdjustEyebrowOnLoaded;

                    WindowSetting.CopyFrom(saveData.WindowSetting);
                    MotionSetting.CopyFrom(saveData.MotionSetting);
                    LayoutSetting.CopyFrom(saveData.LayoutSetting);
                    LightSetting.CopyFrom(saveData.LightSetting);

                    //顔キャリブデータはファイル読み込み時だけ送る特殊なデータなのでここに書いてます
                    MotionSetting.SendCalibrateFaceData();
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
