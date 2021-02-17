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

        private static MessageIndication Load(string keySuffix) => new MessageIndication(
            LocalizedString.GetString("DialogTitle_" + keySuffix),
            LocalizedString.GetString("DialogMessage_" + keySuffix)
            );

        public static MessageIndication LoadVrmConfirmation() => Load("LoadLocalVrm");
        public static MessageIndication ResetSettingConfirmation() => Load("ResetAllSetting");
        public static MessageIndication ResetSingleCategoryConfirmation() => Load("ResetCategorySetting");
        public static MessageIndication ShowVRoidSdkUi() => Load("ShowVRoidSdkUi");
        public static MessageIndication ShowLoadingPreviousVRoid() => Load("LoadPreviousVRoidModel");

        /// <summary>
        /// NOTE: Contentのほうがフォーマット文字列なのでstring.Formatで消すアイテムの名前を指定して完成させること！
        /// string.Format(res.Content, "itemName")
        /// みたいな。
        /// </summary>
        /// <param name="languageName"></param>
        /// <returns></returns>
        public static MessageIndication ErrorLoadSetting() => Load("LoadSettingFileError");

        /// <summary>
        /// NOTE: Contentのほうがフォーマット文字列なのでstring.Formatで消すアイテムの名前を指定して完成させること！
        /// ex: string.Format(res.Content, "itemName")
        /// </summary>
        /// <param name="languageName"></param>
        /// <returns></returns>
        public static MessageIndication DeleteWordToMotionItem() => Load("DeleteWtmItem");

        /// <summary>
        /// NOTE: Contentがフォーマット文字列なため、削除予定のブレンドシェイプ名を指定して完成させること
        /// ex: string.Format(res.Content, "clipName")
        /// </summary>
        /// <param name="languageName"></param>
        /// <returns></returns>
        public static MessageIndication ForgetBlendShapeClip() => Load("ForgetBlendShapeInfo");

        /// <summary>
        /// 無効なIPアドレスを指定したときに怒る文言です。
        /// </summary>
        /// <param name="languageName"></param>
        /// <returns></returns>
        public static MessageIndication InvalidIpAddress() => Load("InvalidIpAddress");

        /// <summary>
        /// モデルでExTrackerのパーフェクトシンクに必要なブレンドシェイプクリップが未定義だったときのエラーのヘッダー部
        /// </summary>
        /// <param name="languageName"></param>
        /// <returns></returns>
        public static MessageIndication ExTrackerMissingBlendShapeNames() => Load("ExTrackerMissingBlendShape");

        /// <summary>
        /// webカメラのトラッキングを使うために外部トラッキングを切ろうとしてる人向けの確認ダイアログ
        /// </summary>
        /// <param name="languageName"></param>
        /// <returns></returns>
        public static MessageIndication ExTrackerCheckTurnOff() => Load("ExTrackerCheckTurnOff"); 
    }
}
