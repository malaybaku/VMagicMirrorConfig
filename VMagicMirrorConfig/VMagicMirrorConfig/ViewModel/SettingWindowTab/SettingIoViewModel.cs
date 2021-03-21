using Microsoft.Win32;

namespace Baku.VMagicMirrorConfig
{
    public class SettingIoViewModel : SettingViewModelBase
    {
        internal SettingIoViewModel(
            AutomationSettingSync model, SaveFileManager saveFileManager, IMessageSender sender
            ) : base(sender)
        {
            _model = model;
            _saveFileManager = saveFileManager;

            OpenInstructionUrlCommand = new ActionCommand(OpenInstructionUrl);
            RequestEnableAutomationCommand = new ActionCommand(OnEnableAutomationRequested);
            RequestDisableAutomationCommand = new ActionCommand(OnDisableAutomationRequested);
            ApplyPortNumberCommand = new ActionCommand(ApplyPortNumber);
            SaveCurrentSettingCommand = new ActionCommand<string>(SaveCurrentSetting);
            LoadSettingCommand = new ActionCommand<string>(LoadSetting);



            ExportSettingToFileCommand = new ActionCommand(SaveSettingToFile);
            ImportSettingFromFileCommand = new ActionCommand(LoadSettingFromFile);

            AutomationPortNumberText = new RProperty<string>(
                _model.AutomationPortNumber.Value.ToString(), v =>
                {
                    //フォーマット違反になってないかチェック
                    PortNumberIsInvalid.Value = !(int.TryParse(v, out int i) && i >= 0 && i < 65536);
                });

            Save1Exist.Value = _saveFileManager.CheckFileExist(1);
            Save2Exist.Value = _saveFileManager.CheckFileExist(2);
            Save3Exist.Value = _saveFileManager.CheckFileExist(3);

            _model.AutomationPortNumber.PropertyChanged += (_, __) =>
            {
                AutomationPortNumberText.Value = _model.AutomationPortNumber.Value.ToString();
            };
        }

        private readonly AutomationSettingSync _model;
        private readonly SaveFileManager _saveFileManager;


        #region ファイル1,2,3のセーブ/ロードするとこ

        public ActionCommand<string> SaveCurrentSettingCommand { get; }
        public ActionCommand<string> LoadSettingCommand { get; }

        //デフォルトではキャラロードだけ有効にして、「同じモデルで服が違うのをパッと切り替えます」みたいなUXを重視しておく。
        public RProperty<bool> LoadCharacterWhenSettingLoaded { get; } = new RProperty<bool>(true);
        public RProperty<bool> LoadNonCharacterWhenSettingLoaded { get; } = new RProperty<bool>(false);

        public RProperty<bool> Save1Exist { get; } = new RProperty<bool>(false);
        public RProperty<bool> Save2Exist { get; } = new RProperty<bool>(false);
        public RProperty<bool> Save3Exist { get; } = new RProperty<bool>(false);

        private async void SaveCurrentSetting(string? s)
        {
            if (!(int.TryParse(s, out var index) && index > 0 && index <= 3))
            {
                return;
            }
            
            //上書き保存があり得るので確認を挟む。
            //初セーブの場合は上書きにならないが、「次から上書きになるで」の意味で出しておく
            var indication = MessageIndication.ConfirmSettingFileSave();
            var result = await MessageBoxWrapper.Instance.ShowAsync(
                indication.Title, 
                string.Format(indication.Content, index), 
                MessageBoxWrapper.MessageBoxStyle.OKCancel
                );
            if (!result)
            {
                return;
            }

            _saveFileManager.SaveCurrentSetting(index);
            //面倒なのでインデックスは見ずに全部リフレッシュしておく
            Save1Exist.Value = _saveFileManager.CheckFileExist(1);
            Save2Exist.Value = _saveFileManager.CheckFileExist(2);
            Save3Exist.Value = _saveFileManager.CheckFileExist(3);

            //ファイルレベルの処理なので流石にスナックバーくらい出しておく(ロードとかインポート/エクスポートも同様)
            SnackbarWrapper.Enqueue(string.Format(
                LocalizedString.GetString("SettingFile_SaveCompleted"), index
                ));
        }

        private async void LoadSetting(string? s)
        {
            if (!(int.TryParse(s, out var index) && index > 0 && index <= 3))
            {
                return;
            }

            var indication = MessageIndication.ConfirmSettingFileLoad();
            var result = await MessageBoxWrapper.Instance.ShowAsync(
                indication.Title,
                string.Format(indication.Content, index),
                MessageBoxWrapper.MessageBoxStyle.OKCancel
                );
            if (!result)
            {
                return;
            }

            _saveFileManager.LoadSetting(
                index, LoadCharacterWhenSettingLoaded.Value, LoadNonCharacterWhenSettingLoaded.Value, false
                );
            //NOTE: コケる事も考えられるんだけど判別がムズいんですよね…
            SnackbarWrapper.Enqueue(string.Format(
                LocalizedString.GetString("SettingFile_LoadCompleted"), index
                ));
        }

        #endregion

        #region エクスポート/インポート

        public ActionCommand ExportSettingToFileCommand { get; }
        public ActionCommand ImportSettingFromFileCommand { get; }

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
                _saveFileManager.SettingFileIo.SaveSetting(dialog.FileName, SettingFileReadWriteModes.Exported);
                SnackbarWrapper.Enqueue(LocalizedString.GetString("SettingFile_SaveCompleted_ExportedFile"));
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
                _saveFileManager.SettingFileIo.LoadSetting(dialog.FileName, SettingFileReadWriteModes.Exported);
                SnackbarWrapper.Enqueue(LocalizedString.GetString("SettingFile_LoadCompleted_ExportedFile"));
            }
        }

        #endregion

        #region オートメーションっぽい所

        public RProperty<bool> IsAutomationEnabled => _model.IsAutomationEnabled;

        public RProperty<string> AutomationPortNumberText { get; }
        //NOTE: Converter使うのも違う気がするのでViewModel層でやってしまう
        public RProperty<bool> PortNumberIsInvalid { get; } = new RProperty<bool>(false);

        public ActionCommand OpenInstructionUrlCommand { get; }
        public ActionCommand RequestEnableAutomationCommand { get; }
        public ActionCommand RequestDisableAutomationCommand { get; }
        public ActionCommand ApplyPortNumberCommand { get; }

        private async void OnEnableAutomationRequested()
        {
            var indication = MessageIndication.EnableAutomation();
            var result = await MessageBoxWrapper.Instance.ShowAsync(
                indication.Title, indication.Content, MessageBoxWrapper.MessageBoxStyle.OKCancel
                );

            if (result)
            {
                _model.IsAutomationEnabled.Value = true;
            }
        }

        private async void OnDisableAutomationRequested()
        {
            var indication = MessageIndication.DisableAutomation();
            var result = await MessageBoxWrapper.Instance.ShowAsync(
                indication.Title, indication.Content, MessageBoxWrapper.MessageBoxStyle.OKCancel
                );

            if (result)
            {
                _model.IsAutomationEnabled.Value = false;
            }
        }

        private void ApplyPortNumber()
        {
            if (int.TryParse(AutomationPortNumberText.Value, out int i) && i >= 0 && i < 65536)
            {
                _model.AutomationPortNumber.Value = i;
            }
        }

        //NOTE: オートメーションの説明ではあるけど設定ファイルタブ全体の設定に飛ばす。
        //どのみちファイルI/Oがどうなってるか説明する必要あるので
        private void OpenInstructionUrl()
            => UrlNavigate.Open(LocalizedString.GetString("URL_docs_setting_files"));

        #endregion

    }
}
