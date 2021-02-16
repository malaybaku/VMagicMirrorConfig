using System.Collections.ObjectModel;

namespace Baku.VMagicMirrorConfig
{
    /// <summary>
    /// ファイルに保存すべき設定のモデル層を直接的に全部保持したクラス。
    /// MainWindowの裏にあり、アプリの生存期間中つねに単一のインスタンスがあるような使い方をします。
    /// </summary>
    class SettingModel
    {
        public SettingModel(IMessageSender sender, IMessageReceiver receiver)
        {
            WindowSetting = new WindowSettingModel(sender);
            MotionSetting = new MotionSettingModel(sender);
            LayoutSetting = new LayoutSettingModel(sender);
            GamepadSetting = new GamepadSettingModel(sender);
            LightSetting = new LightSettingModel(sender);
            WordToMotionSetting = new WordToMotionSettingModel(sender);
            ExternalTrackerSetting = new ExternalTrackerSettingModel(sender);
        }

        //TODO: ここにファイルのセクションと無関係な縦断的なモデル/コードを追加して良い気がする。ファイルのロード/セーブとかやるタイプの。

        private readonly ObservableCollection<string> _availableLanguageNames
            = new ObservableCollection<string>()
        {
            "Japanese",
            "English",
        };
        public ReadOnlyObservableCollection<string> AvailableLanguageNames { get; }


        public WindowSettingModel WindowSetting { get; }

        public MotionSettingModel MotionSetting { get; }

        public LayoutSettingModel LayoutSetting { get; }

        public GamepadSettingModel GamepadSetting { get; }

        public LightSettingModel LightSetting { get; }

        public WordToMotionSettingModel WordToMotionSetting { get; }

        public ExternalTrackerSettingModel ExternalTrackerSetting { get; }


    }
}
