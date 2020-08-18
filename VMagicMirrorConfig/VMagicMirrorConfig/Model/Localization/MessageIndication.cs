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
        public static MessageIndication ShowVRoidSdkUi(string languageName)
            => ShowVRoidSdkUi(LanguageSelector.StringToLanguage(languageName));

        public static MessageIndication ShowLoadingPreviousVRoid(string languageName)
            => ShowLoadingPreviousVRoid(LanguageSelector.StringToLanguage(languageName));

        /// <summary>
        /// NOTE: Contentのほうがフォーマット文字列なのでstring.Formatで消すアイテムの名前を指定して完成させること！
        /// string.Format(res.Content, "itemName")
        /// みたいな。
        /// </summary>
        /// <param name="languageName"></param>
        /// <returns></returns>
        public static MessageIndication ErrorLoadSetting(string languageName)
            => ErrorLoadSetting(LanguageSelector.StringToLanguage(languageName));

        /// <summary>
        /// NOTE: Contentのほうがフォーマット文字列なのでstring.Formatで消すアイテムの名前を指定して完成させること！
        /// ex: string.Format(res.Content, "itemName")
        /// </summary>
        /// <param name="languageName"></param>
        /// <returns></returns>
        public static MessageIndication DeleteWordToMotionItem(string languageName)
            => DeleteWordToMotionItem(LanguageSelector.StringToLanguage(languageName));

        /// <summary>
        /// NOTE: Contentがフォーマット文字列なため、削除予定のブレンドシェイプ名を指定して完成させること
        /// ex: string.Format(res.Content, "clipName")
        /// </summary>
        /// <param name="languageName"></param>
        /// <returns></returns>
        public static MessageIndication ForgetBlendShapeClip(string languageName)
            => ForgetBlendShapeClip(LanguageSelector.StringToLanguage(languageName));

        /// <summary>
        /// 無効なIPアドレスを指定したときに怒る文言です。
        /// </summary>
        /// <param name="languageName"></param>
        /// <returns></returns>
        public static MessageIndication InvalidIpAddress(string languageName)
            => InvalidIpAddress(LanguageSelector.StringToLanguage(languageName));

        /// <summary>
        /// モデルでExTrackerのパーフェクトシンクに必要なブレンドシェイプクリップが未定義だったときのエラーのヘッダー部
        /// </summary>
        /// <param name="languageName"></param>
        /// <returns></returns>
        public static MessageIndication ExTrackerMissingBlendShapeNames(string languageName)
            => ExTrackerMissingBlendShapeNames(LanguageSelector.StringToLanguage(languageName));

        public static MessageIndication LoadVrmConfirmation(Languages lang) => lang switch
        {
            Languages.Japanese => new MessageIndication(
                "VRMの読み込み",
                "ビューアー画面のライセンスを確認してください。読み込みますか？"
                ),
            _ => new MessageIndication(
                "Load VRM",
                "Please confirm the license in viewer window. Do you load the character?"
                ),
        };

        public static MessageIndication ResetSettingConfirmation(Languages lang) => lang switch
        {
            Languages.Japanese => new MessageIndication(
                "設定のリセット",
                "リセットを実行すると全ての設定が初期状態に戻ります。リセットしますか？"
                ),
            _ => new MessageIndication(
                "Reset Setting",
                "Are you sure you want to reset all settings in VMagicMirror?"
                ),
        };

        public static MessageIndication ResetSingleCategoryConfirmation(Languages lang) => lang switch
        {
            Languages.Japanese => new MessageIndication(
                "設定のリセット",
                "選択したカテゴリの設定をリセットしますか？"
                ),
            _ => new MessageIndication(
                "Reset Setting",
                "Are you sure you want to reset selected category setting?"
                ),
        };

        public static MessageIndication ErrorLoadSetting(Languages lang) => lang switch
        {
            Languages.Japanese => new MessageIndication(
                "ロード失敗",
                "設定ファイルのロードに失敗しました。エラー: "
                ),
            _ => new MessageIndication(
                "Load failed",
                "Failed to load setting file. Error: "
                ),
        };

        public static MessageIndication DeleteWordToMotionItem(Languages lang) => lang switch
        {
            Languages.Japanese => new MessageIndication(
                "モーションの削除",
                "このモーション'{0}'を削除しますか？"
                ),
            _ => new MessageIndication(
                "Delete Item",
                "Are you sure to delete this item '{0}'?"
                ),
        };

        public static MessageIndication ForgetBlendShapeClip(Languages lang) => lang switch
        {
            Languages.Japanese => new MessageIndication(
                "ブレンドシェイプ情報のクリア",
                "このブレンドシェイプ'{0}'の設定を削除しますか？"
                ),
            _ => new MessageIndication(
                "Clear Blend Shape Setting",
                "Are you sure to clear the blend shape setting of '{0}'?"
                ),
        };

        public static MessageIndication InvalidIpAddress(Languages lang) => lang switch
        {
            Languages.Japanese => new MessageIndication(
                "無効なIPアドレス",
                "無効なIPアドレスが指定されています。入力を確認して下さい。"
                ),
            _ => new MessageIndication(
                "Invalid IP Address",
                "Invalid IP Address is set, please check the input."
                ),
        };

        public static MessageIndication ShowVRoidSdkUi(Languages lang) => lang switch
        {
            Languages.Japanese => new MessageIndication(
                "VRoid Hubに接続中",
                "キャラクターウィンドウ上でモデルを選択するか、またはキャンセルしてください。"),
            _ => new MessageIndication(
                "Connecting to VRoid Hub",
                "Select model to load, or cancel operation."),
        };

        public static MessageIndication ShowLoadingPreviousVRoid(Languages lang) => lang switch
        {
            Languages.Japanese => new MessageIndication(
                "VRoid Hubに接続中",
                "前回使用したモデルのロードを試みています。モデルをロードするか、またはキャンセルしてください。"),
            _ => new MessageIndication(
                "Connecting to VRoid Hub",
                "Trying to load avatar used in previous time. Select model or cancel operation."),
        };

        public static MessageIndication ExTrackerMissingBlendShapeNames(Languages lang) => lang switch
        {
            Languages.Japanese => new MessageIndication(
                "未定義のBlendShapeClipがあります",
                @"パーフェクトシンクに必要なBlendShapeClipの一部が未定義です。

モデルの見た目が正常であれば、この警告を無視して構いません。
モデルの見た目がおかしい場合はこのダイアログを閉じてから
「パーフェクトシンクとは？」をクリックし、
モデルのセットアップ手順を確認して下さい。

---
定義されていないBlendShapeClipの名称: 
"),
            _ => new MessageIndication(
                "Missing BlendShapeClip",
                @"Detect missing BlendShapeClips for perfect sync.

If your avatar looks normal, ignore this message. 
Otherwise, close this dialog and see 'About Perfect Sync'
to check how to setup the model.

---
Undefined BlendShaepClip names:
"),
        };
    }
}
