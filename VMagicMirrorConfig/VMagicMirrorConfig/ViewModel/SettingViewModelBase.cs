namespace Baku.VMagicMirrorConfig
{
    public abstract class SettingViewModelBase : ViewModelBase
    {
        private protected SettingViewModelBase(IMessageSender sender, StartupSettingViewModel startup)
        {
            _sender = sender;
            Startup = startup;
        }

        private readonly IMessageSender _sender;

        //NOTE: 他のタブにもスタートアップ時の有効/無効がいじれるチェックボックス置きたいので追加
        public StartupSettingViewModel Startup { get; }

        private protected void SendMessage(Message message)
            => _sender.SendMessage(message);

        private ActionCommand _resetToDefaultCommand;
        public ActionCommand ResetToDefaultCommand
            => _resetToDefaultCommand ?? (_resetToDefaultCommand = new ActionCommand(ResetToDefault));

        private ActionCommand _saveSettingCommand;
        public ActionCommand SaveSettingCommand
            => _saveSettingCommand ?? (_saveSettingCommand = new ActionCommand(SaveSetting));

        private ActionCommand _loadSettingCommand;
        public ActionCommand LoadSettingCommand
            => _loadSettingCommand ?? (_loadSettingCommand = new ActionCommand(LoadSetting));

        protected abstract void ResetToDefault();
        protected abstract void SaveSetting();
        protected abstract void LoadSetting();

        internal abstract void SaveSetting(string path);
        internal abstract void LoadSetting(string path);
    }
}
