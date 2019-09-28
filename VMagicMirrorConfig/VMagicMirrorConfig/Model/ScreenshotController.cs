namespace Baku.VMagicMirrorConfig
{
    /// <summary>
    /// スクリーンショットを撮影するクラス。撮影自体は
    /// </summary>
    class ScreenshotController
    {
        public ScreenshotController(IMessageSender sender)
        {
            _sender = sender;
        }

        private readonly IMessageSender _sender;

        public void TakeScreenshot()
        {
            _sender.SendMessage(MessageFactory.Instance.TakeScreenshot());
        }

        public void OpenSavedFolder()
        {
            _sender.SendMessage(MessageFactory.Instance.OpenScreenshotFolder());
        }
    }
}
