using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;

namespace Baku.VMagicMirrorConfig
{
    public class MainWindowViewModel : ViewModelBase, IWindowViewModel
    {
        internal SettingModel Model { get; }
        internal SettingFileIo SettingFileIo { get; }
        
        internal ModelInitializer Initializer { get; } = new ModelInitializer();
        internal IMessageSender MessageSender => Initializer.MessageSender;

        public WindowSettingViewModel WindowSetting { get; private set; }
        public MotionSettingViewModel MotionSetting { get; private set; }
        public LayoutSettingViewModel LayoutSetting { get; private set; }
        public GamepadSettingViewModel GamepadSetting { get; private set; }
        public LightSettingViewModel LightSetting { get; private set; }
        public WordToMotionSettingViewModel WordToMotionSetting { get; private set; }
        public ExternalTrackerViewModel ExternalTrackerSetting { get; private set; }

        private DeviceFreeLayoutHelper? _deviceFreeLayoutHelper;

        //TODO: 自動スタート周りもモデルですね

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
            Model = new SettingModel(MessageSender, Initializer.MessageReceiver);
            SettingFileIo = new SettingFileIo(Model, MessageSender);

            _screenshotController = new ScreenshotController(MessageSender);
            WindowSetting = new WindowSettingViewModel(Model.WindowSetting, MessageSender);
            MotionSetting = new MotionSettingViewModel(Model.MotionSetting, MessageSender, Initializer.MessageReceiver);
            GamepadSetting = new GamepadSettingViewModel(Model.GamepadSetting, MessageSender);
            LayoutSetting = new LayoutSettingViewModel(Model.LayoutSetting, Model.GamepadSetting, MessageSender, Initializer.MessageReceiver);
            LightSetting = new LightSettingViewModel(Model.LightSetting, MessageSender);
            WordToMotionSetting = new WordToMotionSettingViewModel(Model.WordToMotionSetting,  MessageSender, Initializer.MessageReceiver);
            ExternalTrackerSetting = new ExternalTrackerViewModel(Model.ExternalTrackerSetting, MessageSender, Initializer.MessageReceiver);

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

        public ReadOnlyObservableCollection<string> AvailableLanguageNames => Model.AvailableLanguageNames;

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

            var indication = MessageIndication.LoadVrmConfirmation();
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
            var message = MessageIndication.ShowVRoidSdkUi();
            bool _ = await MessageBoxWrapper.Instance.ShowAsync(
                message.Title, message.Content, MessageBoxWrapper.MessageBoxStyle.None
                );

            //モデルロード完了またはキャンセルによってここに来るので、共通の処理をして終わり
            _isVRoidHubUiActive = false;
            EndShowUiOnUnity();
        }

        private void OpenVRoidHub() => UrlNavigate.Open("https://hub.vroid.com/");

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
                SettingFileIo.SaveSetting(dialog.FileName, false);
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
                SettingFileIo.LoadSetting(dialog.FileName, false);
            }
        }

        private async void ResetToDefault()
        {
            var indication = MessageIndication.ResetSettingConfirmation();
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

                SettingFileIo.LoadSetting(savePath, true);
                //NOTE: VRoidの自動ロード設定はちょっと概念的に重たいので引き継ぎ対象から除外する。
                _lastLoadedVRoidModelId = "";
                if (AutoLoadLastLoadedVrm)
                {
                    LoadLastLoadedVrm();
                }
            }
            catch (Exception ex)
            {
                var indication = MessageIndication.ErrorLoadSetting();
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

            SettingFileIo.LoadSetting(SpecialFilePath.AutoSaveSettingFilePath, true);

            //書いてる通りだが、ファイルから読んだ言語名があれば渡したのちvalidateされた結果でキレイにし、メッセージを送る。
            //メッセージを明示的に送るのは、タイミングの都合で上のLoadSetting中には言語設定メッセージが積まれないため。
            LanguageSelector.Instance.Initialize(MessageSender, LanguageName);
            LanguageName = LanguageSelector.Instance.LanguageName;
            MessageSender.SendMessage(MessageFactory.Instance.Language(LanguageName));


            await MotionSetting.InitializeDeviceNamesAsync();
            await LightSetting.InitializeQualitySelectionsAsync();
            await WordToMotionSetting.InitializeCustomMotionClipNamesAsync();

            Initializer.CameraPositionChecker.Start(
                2000,
                data => Model.LayoutSetting.CameraPosition.SilentSet(data)
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
                SettingFileIo.SaveSetting(SpecialFilePath.AutoSaveSettingFilePath, true);
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
            var message = MessageIndication.ShowLoadingPreviousVRoid();
            bool _ = await MessageBoxWrapper.Instance.ShowAsync(
                message.Title, message.Content, MessageBoxWrapper.MessageBoxStyle.None
                );

            //モデルロード完了またはキャンセルによってここに来るので、共通の処理をして終わり
            _isVRoidHubUiActive = false;
            EndShowUiOnUnity();
        }

        //Unity側でウィンドウを表示するとき、最前面と透過を無効にする必要があるため、その準備にあたる処理を行います。
        private void PrepareShowUiOnUnity()
        {
            _windowTransparentBeforeLoadProcess = WindowSetting.IsTransparent.Value;
            WindowSetting.IsTransparent.Value = false;
        }
        
        //Unity側でのUI表示が終わったとき、最前面と透過の設定をもとの状態に戻します。
        private void EndShowUiOnUnity()
        {
            if (_windowTransparentBeforeLoadProcess != null)
            {
                WindowSetting.IsTransparent.Value = _windowTransparentBeforeLoadProcess.GetValueOrDefault();
                _windowTransparentBeforeLoadProcess = null;
            }
        }
    }

    public interface IWindowViewModel : IDisposable
    {
        void Initialize();
    }
}
