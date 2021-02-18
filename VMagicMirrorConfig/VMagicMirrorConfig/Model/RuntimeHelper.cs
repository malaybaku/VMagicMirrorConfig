namespace Baku.VMagicMirrorConfig
{
    /// <summary>
    /// モデルっぽいけど置き場所が難しい処理(スクショ撮影とか)を寄せ集めたクラス。
    /// ViewModelがファットになるのを防ぐために処理を集めるのが主目的なため、
    /// このクラスを細分化したクラスに分け直してもよい
    /// </summary>
    class RuntimeHelper
    {
        public RuntimeHelper(IMessageSender sender, IMessageReceiver receiver)
        {
            _sender = sender;
            _receiver = receiver;
        }

        private readonly IMessageSender _sender;
        private readonly IMessageReceiver _receiver;


        /// <summary> スクリーンショットの撮影をUnity側に要求します。 </summary>
        public void TakeScreenshot() => _sender.SendMessage(MessageFactory.Instance.TakeScreenshot());

        /// <summary> スクリーンショットの保存フォルダを開くようUnity側に要求します。 </summary>
        public void OpenScreenshotSavedFolder() => _sender.SendMessage(MessageFactory.Instance.OpenScreenshotFolder());

    }
}
