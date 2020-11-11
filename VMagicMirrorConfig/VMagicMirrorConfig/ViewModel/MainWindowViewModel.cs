using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;
using Microsoft.Win32;

namespace Baku.VMagicMirrorConfig
{
    public class MainWindowViewModel : ViewModelBase, IWindowViewModel
    {
        internal ModelInitializer Initializer { get; } = new ModelInitializer();
        internal IMessageSender MessageSender => Initializer.MessageSender;

        public WindowSettingViewModel WindowSetting { get; private set; }
        public MotionSettingViewModel MotionSetting { get; private set; }
        public LayoutSettingViewModel LayoutSetting { get; private set; }
        public LightSettingViewModel LightSetting { get; private set; }
        public WordToMotionSettingViewModel WordToMotionSetting { get; private set; }
        public ExternalTrackerViewModel ExternalTrackerSetting { get; private set; }

        public DialogHelperViewModel DialogHelper => DialogHelperViewModel.Instance;

        private DeviceFreeLayoutHelper? _deviceFreeLayoutHelper;

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
        private string _lastLoadedVRoidModelId = "";

        private bool _isDisposed = false;
        //VRoid Hubに接続した時点でウィンドウが透過だったかどうか。
        private bool _isVRoidHubUiActive = false;

        //NOTE: モデルのロード確認UI(ファイル/VRoidHubいずれか)を出す直前時点での値を保持するフラグで、UIが出てないときはnullになる
        private bool? _windowTransparentBeforeLoadProcess = null;

        private readonly ScreenshotController _screenshotController;

        public MainWindowViewModel()
        {
            _screenshotController = new ScreenshotController(MessageSender);
            WindowSetting = new WindowSettingViewModel(MessageSender);
            MotionSetting = new MotionSettingViewModel(MessageSender, Initializer.MessageReceiver);
            LayoutSetting = new LayoutSettingViewModel(MessageSender, Initializer.MessageReceiver);
            LightSetting = new LightSettingViewModel(MessageSender);
            WordToMotionSetting = new WordToMotionSettingViewModel(MessageSender, Initializer.MessageReceiver);
            ExternalTrackerSetting = new ExternalTrackerViewModel(MessageSender, Initializer.MessageReceiver);

            AvailableLanguageNames = new ReadOnlyObservableCollection<string>(_availableLanguageNames);

            Initializer.MessageReceiver.ReceivedCommand += OnReceiveCommand;
        }

        private void OnReceiveCommand(object? sender, CommandReceivedEventArgs e)
        {
            switch (e.Command)
            {
                case ReceiveMessageNames.VRoidModelLoadCompleted:
                    //WPF側のダイアログによるUIガードを終了: _isVRoidHubUiActiveフラグは別のとこで折るのでここでは無視でOK
                    if (_isVRoidHubUiActive)
                    {
                        MessageBoxWrapper.Instance.SetDialogResult(false);
                        DialogHelper.IsOpen = false;
                    }

                    //ファイルパスではなくモデルID側を最新情報として覚えておく
                    _lastVrmLoadFilePath = "";
                    _lastLoadedVRoidModelId = e.Args;

                    break;
                case ReceiveMessageNames.VRoidModelLoadCanceled:
                    //WPF側のダイアログによるUIガードを終了
                    if (_isVRoidHubUiActive)
                    {
                        MessageBoxWrapper.Instance.SetDialogResult(false);
                        DialogHelper.IsOpen = false;
                    }
                    break;
            }
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

        private ActionCommand? _loadVrmCommand;
        public ActionCommand LoadVrmCommand
            => _loadVrmCommand ??= new ActionCommand(LoadVrm);

        private ActionCommand<string>? _loadVrmByPathCommand;
        public ActionCommand<string> LoadVrmByFilePathCommand
            => _loadVrmByPathCommand ??= new ActionCommand<string>(LoadVrmByFilePath);

        private ActionCommand? _connectToVRoidHubCommand;
        public ActionCommand ConnectToVRoidHubCommand
            => _connectToVRoidHubCommand ??= new ActionCommand(ConnectToVRoidHubAsync);

        private ActionCommand? _openVRoidHubCommand;
        public ActionCommand OpenVRoidHubCommand
            => _openVRoidHubCommand ??= new ActionCommand(OpenVRoidHub);

        private ActionCommand? _openManualUrlCommand;
        public ActionCommand OpenManualUrlCommand
            => _openManualUrlCommand ??= new ActionCommand(OpenManualUrl);

        private ActionCommand? _autoAdjustCommand;
        public ActionCommand AutoAdjustCommand
            => _autoAdjustCommand ??= new ActionCommand(AutoAdjust);

        private ActionCommand? _openSettingWindowCommand;
        public ActionCommand OpenSettingWindowCommand
            => _openSettingWindowCommand ??= new ActionCommand(OpenSettingWindow);

        private ActionCommand? _resetToDefaultCommand;
        public ActionCommand ResetToDefaultCommand
            => _resetToDefaultCommand ??= new ActionCommand(ResetToDefault);

        private ActionCommand? _saveSettingToFileCommand;
        public ActionCommand SaveSettingToFileCommand
            => _saveSettingToFileCommand ??= new ActionCommand(SaveSettingToFile);

        private ActionCommand? _loadSettingFromFileCommand;
        public ActionCommand LoadSettingFromFileCommand
            => _loadSettingFromFileCommand ??= new ActionCommand(LoadSettingFromFile);

        private ActionCommand? _loadPrevSettingCommand;
        public ActionCommand LoadPrevSettingCommand
            => _loadPrevSettingCommand ??= new ActionCommand(LoadPrevSetting);

        private ActionCommand? _takeScreenshotCommand;
        public ActionCommand TakeScreenshotCommand
            => _takeScreenshotCommand ??= new ActionCommand(TakeScreenshot);

        private ActionCommand? _openScreenshotFolderCommand;
        public ActionCommand OpenScreenshotFolderCommand
            => _openScreenshotFolderCommand ??= new ActionCommand(OpenScreenshotFolder);

        #endregion

        #region Command Impl

        //NOTE: async voidを使ってるが、ここはUIイベントのハンドラ相当なので許して

        private async void LoadVrm()
        {
            await LoadVrmSub(() =>
            {
                var dialog = new OpenFileDialog()
                {
                    Title = "Open VRM file",
                    Filter = "VRM files (*.vrm)|*.vrm|All files (*.*)|*.*",
                    Multiselect = false,
                };

                return
                    (dialog.ShowDialog() == true && File.Exists(dialog.FileName))
                    ? dialog.FileName
                    : "";
            });
        }

        private async void LoadVrmByFilePath(string? filePath)
        {
            if (!string.IsNullOrEmpty(filePath) &&
                Path.GetExtension(filePath) == ".vrm")
            {
                await LoadVrmSub(() => filePath);
            }
        }

        /// <summary>
        /// ファイルパスを取得する処理を指定して、VRMをロードします。
        /// </summary>
        /// <param name="getFilePathProcess"></param>
        private async Task LoadVrmSub(Func<string> getFilePathProcess)
        {
            PrepareShowUiOnUnity();

            string filePath = getFilePathProcess();
            if (!File.Exists(filePath))
            {
                EndShowUiOnUnity();
                return;
            }

            MessageSender.SendMessage(MessageFactory.Instance.OpenVrmPreview(filePath));

            var indication = MessageIndication.LoadVrmConfirmation(LanguageName);
            bool res = await MessageBoxWrapper.Instance.ShowAsync(
                indication.Title,
                indication.Content,
                MessageBoxWrapper.MessageBoxStyle.OKCancel
                );

            if(res)
            {
                MessageSender.SendMessage(MessageFactory.Instance.OpenVrm(filePath));
                _lastVrmLoadFilePath = filePath;
                _lastLoadedVRoidModelId = "";
            }
            else
            {
                MessageSender.SendMessage(MessageFactory.Instance.CancelLoadVrm());
            }

            EndShowUiOnUnity();
        }

        private async void ConnectToVRoidHubAsync()
        {
            PrepareShowUiOnUnity();

            MessageSender.SendMessage(MessageFactory.Instance.OpenVRoidSdkUi());

            //VRoidHub側の操作が終わるまでダイアログでガードをかける: モーダル的な管理状態をファイルロードの場合と揃える為
            _isVRoidHubUiActive = true;
            var message = MessageIndication.ShowVRoidSdkUi(LanguageName);
            bool _ = await MessageBoxWrapper.Instance.ShowAsync(
                message.Title, message.Content, MessageBoxWrapper.MessageBoxStyle.None
                );

            //モデルロード完了またはキャンセルによってここに来るので、共通の処理をして終わり
            _isVRoidHubUiActive = false;
            EndShowUiOnUnity();
        }

        private void OpenVRoidHub() => UrlNavigate.Open("https://hub.vroid.com/");

        private void OpenManualUrl()
        {
            string url =
                (LanguageName == "Japanese") ?
                "https://malaybaku.github.io/VMagicMirror" :
                "https://malaybaku.github.io/VMagicMirror/en";

            UrlNavigate.Open(url);
        }

        private void AutoAdjust() => MessageSender.SendMessage(MessageFactory.Instance.RequestAutoAdjust());

        private void OpenSettingWindow() 
            => SettingWindow.OpenOrActivateExistingWindow(this);

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

        private async void ResetToDefault()
        {
            var indication = MessageIndication.ResetSettingConfirmation(LanguageName);
            bool res = await MessageBoxWrapper.Instance.ShowAsync(
                indication.Title,
                indication.Content,
                MessageBoxWrapper.MessageBoxStyle.OKCancel
                );

            if (res)
            {
                LightSetting.ResetToDefault();
                MotionSetting.ResetToDefault();
                LayoutSetting.ResetToDefault();
                WindowSetting.ResetToDefault();
                WordToMotionSetting.ResetToDefault();
                ExternalTrackerSetting.ResetToDefault();

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
            if (dialog.ShowDialog() != true || string.IsNullOrEmpty(dialog.FileName))
            {
                return;
            }

            try
            {
                string savePath = Path.Combine(
                    Path.GetDirectoryName(dialog.FileName) ?? "",
                    "ConfigApp",
                    SpecialFilePath.AutoSaveSettingFileName
                    );

                LoadSetting(savePath, true);
                //NOTE: VRoidの自動ロード設定はちょっと概念的に重たいので引き継ぎ対象から除外する。
                _lastLoadedVRoidModelId = "";
                if (AutoLoadLastLoadedVrm)
                {
                    LoadLastLoadedVrm();
                }
            }
            catch (Exception ex)
            {
                var indication = MessageIndication.ErrorLoadSetting(LanguageName);
                MessageBox.Show(
                    indication.Title,
                    indication.Content + ex.Message
                    );
            }
        }

        private void TakeScreenshot() 
            => _screenshotController.TakeScreenshot();

        private void OpenScreenshotFolder()
            => _screenshotController.OpenSavedFolder();

        #endregion

        public async void Initialize()
        {
            if (Application.Current.MainWindow == null ||
                DesignerProperties.GetIsInDesignMode(Application.Current.MainWindow))
            {
                return;
            }

            Initializer.StartObserveRoutine();

            //NOTE: ここでコンポジットを開始することで、背景色/ライト/影のメッセージも統一してしまう
            Initializer.MessageSender.StartCommandComposite();
            WindowSetting.Initialize();
            LightSetting.Initialize();
            LoadSetting(SpecialFilePath.AutoSaveSettingFilePath, true);
            //NOTE: ここのEndCommandCompositeはLoadSettingが(ファイル無いとかで)中断したときの対策
            Initializer.MessageSender.EndCommandComposite();

            //書いてる通りだが、ファイルから読んだ言語名があれば渡したのちvalidateされた結果でキレイにし、メッセージを送る。
            //メッセージを明示的に送るのは、タイミングの都合で上のLoadSetting中には言語設定メッセージが積まれないため。
            LanguageSelector.Instance.Initialize(MessageSender, LanguageName);
            LanguageName = LanguageSelector.Instance.LanguageName;
            MessageSender.SendMessage(MessageFactory.Instance.Language(LanguageName));


            await MotionSetting.InitializeDeviceNamesAsync();
            await LightSetting.InitializeQualitySelectionsAsync();

            Initializer.CameraPositionChecker.Start(
                2000,
                data => LayoutSetting.SilentSetCameraPosition(data)
                );

            var regSetting = new StartupRegistrySetting();
            _activateOnStartup = regSetting.CheckThisVersionRegistered();
            if (_activateOnStartup)
            {
                RaisePropertyChanged(nameof(ActivateOnStartup));
            }
            OtherVersionRegisteredOnStartup = regSetting.CheckOtherVersionRegistered();

            if (AutoLoadLastLoadedVrm && !string.IsNullOrEmpty(_lastVrmLoadFilePath))
            {
                LoadLastLoadedVrm();
            }            

            _deviceFreeLayoutHelper = new DeviceFreeLayoutHelper(LayoutSetting, WindowSetting);
            _deviceFreeLayoutHelper.StartObserve();

            if (AutoLoadLastLoadedVrm && !string.IsNullOrEmpty(_lastLoadedVRoidModelId))
            {
                LoadLastLoadedVRoid();
            }
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                SaveSetting(SpecialFilePath.AutoSaveSettingFilePath, true);
                Initializer.Dispose();
                _deviceFreeLayoutHelper?.EndObserve();
                MotionSetting.ClosePointer();
                Initializer.UnityAppCloser.Close();
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

        private async void LoadLastLoadedVRoid()
        {
            if (string.IsNullOrEmpty(_lastLoadedVRoidModelId))
            {
                return;
            }

            PrepareShowUiOnUnity();

            //NOTE: モデルIDを載せる以外は通常のUIオープンと同じフロー
            MessageSender.SendMessage(MessageFactory.Instance.RequestLoadVRoidWithId(_lastLoadedVRoidModelId));

            _isVRoidHubUiActive = true;
            var message = MessageIndication.ShowLoadingPreviousVRoid(LanguageName);
            bool _ = await MessageBoxWrapper.Instance.ShowAsync(
                message.Title, message.Content, MessageBoxWrapper.MessageBoxStyle.None
                );

            //モデルロード完了またはキャンセルによってここに来るので、共通の処理をして終わり
            _isVRoidHubUiActive = false;
            EndShowUiOnUnity();
        }

        private void SaveSetting(string path, bool isInternalFile)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            using (var sw = new StreamWriter(path))
            {
                //note: 動作設定の一覧は(Unityに投げる都合で)JSONになってるのでやや構造がめんどいです。
                WordToMotionSetting.SaveItems();
                new XmlSerializer(typeof(SaveData)).Serialize(sw, new SaveData()
                {
                    IsInternalSaveFile = isInternalFile,
                    LastLoadedVrmFilePath = isInternalFile ? _lastVrmLoadFilePath : "",
                    LastLoadedVRoidModelId = isInternalFile ? _lastLoadedVRoidModelId : "",
                    AutoLoadLastLoadedVrm = isInternalFile ? AutoLoadLastLoadedVrm : false,
                    PreferredLanguageName = isInternalFile ? LanguageName : "",
                    WindowSetting = this.WindowSetting,
                    MotionSetting = this.MotionSetting,
                    LayoutSetting = this.LayoutSetting,
                    LightSetting = this.LightSetting,
                    WordToMotionSetting = this.WordToMotionSetting,
                    ExternalTrackerSetting = this.ExternalTrackerSetting,
                });
            }
        }

        private void LoadSettingSub(string path, bool isInternalFile)
        {
            using (var sr = new StreamReader(path))
            {
                var serializer = new XmlSerializer(typeof(SaveData));
                var saveData = (SaveData?)serializer.Deserialize(sr);
                if (saveData == null)
                {
                    return;
                } 

                if (isInternalFile && saveData.IsInternalSaveFile)
                {
                    _lastVrmLoadFilePath = saveData.LastLoadedVrmFilePath ?? "";
                    _lastLoadedVRoidModelId = saveData.LastLoadedVRoidModelId ?? "";
                    AutoLoadLastLoadedVrm = saveData.AutoLoadLastLoadedVrm;
                    LanguageName =
                        AvailableLanguageNames.Contains(saveData.PreferredLanguageName ?? "") ?
                        (saveData.PreferredLanguageName ?? "") :
                        "";
                }

                WindowSetting.CopyFrom(saveData.WindowSetting);
                MotionSetting.CopyFrom(saveData.MotionSetting);
                LayoutSetting.CopyFrom(saveData.LayoutSetting);
                LightSetting.CopyFrom(saveData.LightSetting);
                //コレはv0.9.0で追加したので、それ以前のバージョンのデータを読み込むとnullになってる
                if (saveData.WordToMotionSetting != null)
                {
                    WordToMotionSetting.CopyFrom(saveData.WordToMotionSetting);
                }
                WordToMotionSetting.LoadSerializedItems();
                WordToMotionSetting.RequestReload();

                //これも同様に、古いデータだとnullになる
                if (saveData.ExternalTrackerSetting != null)
                {
                    ExternalTrackerSetting.CopyFrom(saveData.ExternalTrackerSetting);
                }
                ExternalTrackerSetting.LoadFaceSwitchSettingFromString();

                //顔キャリブデータはファイル読み込み時だけ送る特殊なデータなのでここに書いてます
                MotionSetting.SendCalibrateFaceData();
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
                Initializer.MessageSender.StartCommandComposite();
                LoadSettingSub(path, isInternalFile);
                Initializer.MessageSender.EndCommandComposite();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load setting file {path} : {ex.Message}");
            }
        }

        //Unity側でウィンドウを表示するとき、最前面と透過を無効にする必要があるため、その準備にあたる処理を行います。
        private void PrepareShowUiOnUnity()
        {
            _windowTransparentBeforeLoadProcess = WindowSetting.IsTransparent;
            WindowSetting.IsTransparent = false;
            WindowSetting.TopMost = false;
        }
        
        //Unity側でのUI表示が終わったとき、最前面と透過の設定をもとの状態に戻します。
        private void EndShowUiOnUnity()
        {
            if (_windowTransparentBeforeLoadProcess != null)
            {
                WindowSetting.IsTransparent = _windowTransparentBeforeLoadProcess.GetValueOrDefault();
                _windowTransparentBeforeLoadProcess = null;
            }
        }
    }

    public interface IWindowViewModel : IDisposable
    {
        void Initialize();
    }
}
