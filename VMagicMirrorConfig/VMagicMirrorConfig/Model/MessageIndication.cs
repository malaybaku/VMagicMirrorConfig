namespace Baku.VMagicMirrorConfig
{
    //メッセージボックスで表示するテキストの言語別対応。
    //リソースに書くほどでもないのでベタに書く
    class MessageIndication
    {
        private MessageIndication(string title, string content)
        {
            Title = title;
            Content = content;
        }

        public string Title { get; }
        public string Content { get; }

        public static MessageIndication LoadVrmConfirmation(string languageName)
            => LoadVrmConfirmation(LanguageSelector.StringToLanguage(languageName));

        public static MessageIndication ResetSettingConfirmation(string languageName)
            => ResetSettingConfirmation(LanguageSelector.StringToLanguage(languageName));

        public static MessageIndication LoadVrmConfirmation(Languages lang)
        {
            switch (lang)
            {
                case Languages.Japanese:
                    return new MessageIndication(
                        "VRMの読み込み",
                        "ビューアー画面のライセンスを確認してください。読み込みますか？"
                        );
                case Languages.English:
                default:
                    return new MessageIndication(
                        "Load VRM",
                        "Please confirm the license in viewer window. Do you load the character?"
                        );
            }
        }

        public static MessageIndication ResetSettingConfirmation(Languages lang)
        {
            switch (lang)
            {
                case Languages.Japanese:
                    return new MessageIndication(
                        "設定のリセット",
                        "リセットを実行すると全ての設定が初期状態に戻ります。リセットしますか？"
                        );
                case Languages.English:
                default:
                    return new MessageIndication(
                        "Reset Setting",
                        "Are you sure you want to reset all settings in VMagicMirror?"
                        );
            }
        }
    }
}
