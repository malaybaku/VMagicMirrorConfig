using System;
using System.Threading;
using System.Threading.Tasks;

namespace Baku.VMagicMirrorConfig
{
    class CameraPositionChecker
    {

        public CameraPositionChecker(IMessageSender sender)
        {
            _sender = sender;
        }

        private readonly IMessageSender _sender;
        private CancellationTokenSource _cts = null;

        public void Start(int intervalMillisec, Action<string> onResult)
        {
            if (_sender == null) { return; }

            Stop();
            _cts = new CancellationTokenSource();
            Task.Run(async () =>
            {
                while (!_cts.Token.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(intervalMillisec, _cts.Token);
                        if (_cts.Token.IsCancellationRequested)
                        {
                            return;
                        }
                        string data = await _sender.QueryMessageAsync(MessageFactory.Instance.CurrentCameraPosition());
                        onResult(data);
                    }
                    catch(Exception ex)
                    {
                        LogOutput.Instance.Write(ex);
                    }
                }
            });
        }

        public void Stop()
        {
            _cts?.Cancel();
            _cts = null;
        }
    }
}
