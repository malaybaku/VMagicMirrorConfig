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

            RequestEnableAutomationCommand = new ActionCommand(OnEnableAutomationRequested);
            RequestDisableAutomationCommand = new ActionCommand(OnDisableAutomationRequested);
            ApplyPortNumberCommand = new ActionCommand(ApplyPortNumber);
            SaveCurrentSettingCommand = new ActionCommand<string>(SaveCurrentSetting);
            LoadSettingCommand = new ActionCommand<string>(LoadSetting);

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

        private void SaveCurrentSetting(string? s)
        {
            if (int.TryParse(s, out var index) && index > 0 && index <= 3)
            {
                _saveFileManager.SaveCurrentSetting(index);
                //面倒なのでインデックスは見ずに全部リフレッシュしておく
                Save1Exist.Value = _saveFileManager.CheckFileExist(1);
                Save2Exist.Value = _saveFileManager.CheckFileExist(2);
                Save3Exist.Value = _saveFileManager.CheckFileExist(3);
            }
        }

        private void LoadSetting(string? s)
        {
            if (int.TryParse(s, out var index) && index > 0 && index <= 3)
            {
                _saveFileManager.LoadSetting(
                    index, LoadCharacterWhenSettingLoaded.Value, LoadNonCharacterWhenSettingLoaded.Value
                    );
            }
        }

        #endregion

        #region オートメーションっぽい所

        public RProperty<bool> IsAutomationEnabled => _model.IsAutomationEnabled;

        public RProperty<string> AutomationPortNumberText { get; }
        //NOTE: Converter使うのも違う気がするのでViewModel層でやってしまう
        public RProperty<bool> PortNumberIsInvalid { get; } = new RProperty<bool>(false);

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


        #endregion

    }
}
