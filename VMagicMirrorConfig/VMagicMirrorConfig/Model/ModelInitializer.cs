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
            CameraPositionChecker = new CameraPositionChecker(MessageSender);
            DeviceLayoutChecker = new DeviceLayoutChecker(MessageSender);
        }

        public void StartObserveRoutine()
        {
            new AppExitFromUnityMessage().Initialize(MessageReceiver);
            MessageReceiver.Start();
        }

        public IMessageSender MessageSender { get; } 
        public IMessageReceiver MessageReceiver { get; } 
        public CameraPositionChecker CameraPositionChecker { get; private set; }
        public DeviceLayoutChecker DeviceLayoutChecker { get; private set; }

        public void Dispose()
        {
            MessageReceiver.Stop();
            CameraPositionChecker?.Stop();
            DeviceLayoutChecker?.Stop();
        }
    }
}
