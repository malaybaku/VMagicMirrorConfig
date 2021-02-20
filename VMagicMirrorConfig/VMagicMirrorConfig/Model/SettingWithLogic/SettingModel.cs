using System.Collections.ObjectModel;
using System.Globalization;

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

            //NOTE; LanguageSelectorとの二重管理っぽくて若干アレだがこのままで行く
            //初期値Defaultを入れることで、起動直後にPCのカルチャベースで言語を指定しなきゃダメかどうか判別する
            LanguageName = new RPropertyMin<string>("Default", s =>
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

        //NOTE: VRMのロード処理はUI依存の処理が多すぎるためViewModel実装のままにしている

        public RPropertyMin<string> LanguageName { get; }

        public WindowSettingModel WindowSetting { get; }

        public MotionSettingModel MotionSetting { get; }

        public LayoutSettingModel LayoutSetting { get; }

        public GamepadSettingModel GamepadSetting { get; }

        public LightSettingModel LightSetting { get; }

        public WordToMotionSettingModel WordToMotionSetting { get; }

        public ExternalTrackerSettingModel ExternalTrackerSetting { get; }

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
