using System;

namespace Baku.VMagicMirrorConfig
{
    class MessageIo : IDisposable
    {
        public MessageIo()
        {
            var mmfClient = new MmfClient();
            Sender = mmfClient;
            Receiver = mmfClient;
        }

        public void Start()
        {
            new AppExitFromUnityMessage().Initialize(Receiver);
            Receiver.Start();
        }

        public IMessageSender Sender { get; } 
        public IMessageReceiver Receiver { get; } 

        public void Dispose()
        {
            Receiver.Stop();
        }
    }
}
