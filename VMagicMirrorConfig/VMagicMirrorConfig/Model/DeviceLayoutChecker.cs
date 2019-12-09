using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Baku.VMagicMirrorConfig
{
    class DeviceLayoutChecker
    {
        public DeviceLayoutChecker(IMessageSender sender)
        {
            _sender = sender;
            _cts = new CancellationTokenSource();
        }

        private readonly IMessageSender _sender;
        private readonly CancellationTokenSource _cts;

        public void Start(int intervalMillisec, Action<string> onResult)
            => Task.Run(async () =>
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
                        string data = await _sender.QueryMessageAsync(MessageFactory.Instance.CurrentDeviceLayout());
                        onResult(data);
                    }
                    catch (Exception ex)
                    {
                        LogOutput.Instance.Write(ex);
                    }
                }
            });

        public void Stop() => _cts.Cancel();
    }
}
