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

        public static MessageIndication ResetSingleCategoryConfirmation(string languageName)
            => ResetSingleCategoryConfirmation(LanguageSelector.StringToLanguage(languageName));

        public static MessageIndication ErrorLoadSetting(string languageName)
            => ErrorLoadSetting(LanguageSelector.StringToLanguage(languageName));

        /// <summary>
        /// NOTE: Contentのほうがフォーマット文字列なのでstring.Formatで消すアイテムの名前を指定して完成させること！
        /// string.Format(res.Content, "itemName")
        /// みたいな。
        /// </summary>
        /// <param name="languageName"></param>
        /// <returns></returns>
        public static MessageIndication DeleteWordToMotionItem(string languageName)
            => DeleteWordToMotionItem(LanguageSelector.StringToLanguage(languageName));

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

        public static MessageIndication ResetSingleCategoryConfirmation(Languages lang)
        {
            switch (lang)
            {
                case Languages.Japanese:
                    return new MessageIndication(
                        "設定のリセット",
                        "選択したカテゴリの設定をリセットしますか？"
                        );
                case Languages.English:
                default:
                    return new MessageIndication(
                        "Reset Setting",
                        "Are you sure you want to reset selected category setting?"
                        );
            }
        }

        /// <summary>
        /// NOTE: Contentのほうがフォーマット文字列なのでstring.Formatで消すアイテムの名前を指定して完成させること！
        /// string.Format(res.Content, "itemName")
        /// みたいな。
        /// </summary>
        /// <param name="languageName"></param>
        /// <returns></returns>
        public static MessageIndication ErrorLoadSetting(Languages lang)
        {
            switch (lang)
            {
                case Languages.Japanese:
                    return new MessageIndication(
                        "ロード失敗",
                        "設定ファイルのロードに失敗しました。エラー: "
                        );
                case Languages.English:
                default:
                    return new MessageIndication(
                        "Load failed",
                        "Failed to load setting file. Error: "
                        );
            }
        }

        public static MessageIndication DeleteWordToMotionItem(Languages lang)
        {
            switch (lang)
            {
                case Languages.Japanese:
                    return new MessageIndication(
                        "モーションの削除",
                        "このモーション'{0}'を削除しますか？"
                        );
                case Languages.English:
                default:
                    return new MessageIndication(
                        "Delete Item",
                        "Are you sure to delete this item '{0}'?"
                        );
            }
        }
    }
}
