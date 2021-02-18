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
            AvailableLanguageNames = new ReadOnlyObservableCollection<string>(_availableLanguageNames);

            _sender = sender;

            WindowSetting = new WindowSettingModel(sender);
            MotionSetting = new MotionSettingModel(sender);
            LayoutSetting = new LayoutSettingModel(sender);
            GamepadSetting = new GamepadSettingModel(sender);
            LightSetting = new LightSettingModel(sender);
            WordToMotionSetting = new WordToMotionSettingModel(sender);
            ExternalTrackerSetting = new ExternalTrackerSettingModel(sender);

            //TODO?: ここの書き方からしてプロパティが二重に存在する感じがちょっと抵抗あるよね
            LanguageName = new RPropertyMin<string>("Japanese", s =>
            {
                LanguageSelector.Instance.LanguageName = s;
            });

        }

        private readonly IMessageSender _sender;

        private readonly ObservableCollection<string> _availableLanguageNames
            = new ObservableCollection<string>()
        {
            "Japanese",
            "English",
        };
        public ReadOnlyObservableCollection<string> AvailableLanguageNames { get; }

        //NOTE: 自動ロードがオフなのにロードしたVRMのファイルパスが残ったりするのはメモリ上ではOK。
        //SettingFileIoがセーブする時点において、自動ロードが無効だとファイルパスが転写されないようにガードがかかる。
        public string LastVrmLoadFilePath { get; set; } = "";
        public string LastLoadedVRoidModelId { get; set; } = "";
        public RPropertyMin<bool> AutoLoadLastLoadedVrm { get; } = new RPropertyMin<bool>(false);

        //TODO: モデルの自動ロードってここに書くのがいいのかな？？VMのままでもいい？

        public RPropertyMin<string> LanguageName { get; }

        public WindowSettingModel WindowSetting { get; }

        public MotionSettingModel MotionSetting { get; }

        public LayoutSettingModel LayoutSetting { get; }

        public GamepadSettingModel GamepadSetting { get; }

        public LightSettingModel LightSetting { get; }

        public WordToMotionSettingModel WordToMotionSetting { get; }

        public ExternalTrackerSettingModel ExternalTrackerSetting { get; }

        public void OnVRoidModelLoaded(string modelId)
        {
            LastVrmLoadFilePath = "";
            LastLoadedVRoidModelId = modelId;
        }

        public void OnLocalModelLoaded(string filePath)
        {
            LastVrmLoadFilePath = filePath;
            LastLoadedVRoidModelId = "";
        }

        public void ResetToDefault()
        {
            _sender.StartCommandComposite();



            _sender.EndCommandComposite();
        }
    }
}
