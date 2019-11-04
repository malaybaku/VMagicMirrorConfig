using System;
using System.Threading;
using System.Threading.Tasks;

namespace Baku.VMagicMirrorConfig
{
    class WordToMotionItemPreviewDataSender
    {
        public WordToMotionItemPreviewDataSender(IMessageSender sender)
        {
            _sender = sender;
            MotionRequest = MotionRequest.GetDefault();
        }

        /// <summary>送信機能が有効な間、このリクエストの中身を送信します。</summary>
        public MotionRequest MotionRequest { get; }

        //あくまでプレビューが目当てなのでザツに。
        private const int DataSendIntervalMillisec = 500;
        private readonly IMessageSender _sender;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public event EventHandler? PrepareDataSend;

        public void Start()
        {
            Task.Run(async () =>
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    PrepareDataSend?.Invoke(this, EventArgs.Empty);
                    SendData(MotionRequest);
                    await Task.Delay(DataSendIntervalMillisec, _cts.Token);
                }
            });            
        }

        public void End() => _cts.Cancel();

        private void SendData(MotionRequest request)
            => _sender.SendMessage(
            MessageFactory.Instance.SendWordToMotionPreviewInfo(request.ToJson())
            );
    }
}
