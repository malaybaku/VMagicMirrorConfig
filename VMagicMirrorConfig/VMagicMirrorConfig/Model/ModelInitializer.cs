using System;

namespace Baku.VMagicMirrorConfig
{
    class ModelInitializer : IDisposable
    {
        public ModelInitializer()
        {
            var mmfClient = new MmfClient();
            MessageSender = mmfClient;
            MessageReceiver = mmfClient;
        }

        public void Initialize()
        {
            new AppExitFromUnityMessage().Initialize(MessageReceiver);
            MessageReceiver.Start();
            CameraPositionChecker = new CameraPositionChecker(MessageSender);
        }

        public IMessageSender MessageSender { get; } 
        public IMessageReceiver MessageReceiver { get; } 
        public CameraPositionChecker CameraPositionChecker { get; private set; }

        public void Dispose()
        {
            MessageReceiver.Stop();
            CameraPositionChecker?.Stop();
        }
    }
}
