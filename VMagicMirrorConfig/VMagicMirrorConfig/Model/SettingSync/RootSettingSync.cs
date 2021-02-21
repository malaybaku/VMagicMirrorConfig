using System.Collections.ObjectModel;
using System.Globalization;

namespace Baku.VMagicMirrorConfig
{
    /// <summary>
    /// ファイルに保存すべき設定のモデル層を直接的に全部保持したクラス。
    /// MainWindowの裏にあり、アプリの生存期間中つねに単一のインスタンスがあるような使い方をします。
    /// </summary>
    class RootSettingSync
    {
        public RootSettingSync(IMessageSender sender, IMessageReceiver receiver)
        {
            AvailableLanguageNames = new ReadOnlyObservableCollection<string>(_availableLanguageNames);

            _sender = sender;

            WindowSetting = new WindowSettingSync(sender);
            MotionSetting = new MotionSettingSync(sender);
            LayoutSetting = new LayoutSettingSync(sender);
            GamepadSetting = new GamepadSettingSync(sender);
            LightSetting = new LightSettingSync(sender);
            WordToMotionSetting = new WordToMotionSettingSync(sender);
            ExternalTrackerSetting = new ExternalTrackerSettingSync(sender);

            //NOTE; LanguageSelectorとの二重管理っぽくて若干アレだがこのままで行く
            //初期値Defaultを入れることで、起動直後にPCのカルチャベースで言語を指定しなきゃダメかどうか判別する
            LanguageName = new RProperty<string>("Default", s =>
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
        public RProperty<bool> AutoLoadLastLoadedVrm { get; } = new RProperty<bool>(false);

        //NOTE: VRMのロード処理はUI依存の処理が多すぎるためViewModel実装のままにしている

        public RProperty<string> LanguageName { get; }

        public WindowSettingSync WindowSetting { get; }

        public MotionSettingSync MotionSetting { get; }

        public LayoutSettingSync LayoutSetting { get; }

        public GamepadSettingSync GamepadSetting { get; }

        public LightSettingSync LightSetting { get; }

        public WordToMotionSettingSync WordToMotionSetting { get; }

        public ExternalTrackerSettingSync ExternalTrackerSetting { get; }

        /// <summary>
        /// 自動保存される設定ファイルに言語設定が保存されていなかった場合、
        /// 現在のカルチャに応じた初期言語を設定します。
        /// </summary>
        public void InitializeLanguageIfNeeded()
        {
            if (LanguageName.Value == "Default")
            {
                LanguageName.Value =
                    (CultureInfo.CurrentCulture.Name == "ja-JP") ?
                    "Japanese" :
                    "English";
            }
        }

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
