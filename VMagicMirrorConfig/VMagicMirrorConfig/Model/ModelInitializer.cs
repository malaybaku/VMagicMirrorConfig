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
            InputChecker.Start();

            InputChecker.MouseMoved += OnMouseMoved;
            InputChecker.MouseButton += OnMouseButton;
            InputChecker.KeyDown += OnKeyDown;

            AssignMessageReceivers();
            MessageReceiver.Start();

            CameraPositionChecker = new CameraPositionChecker(MessageSender);
        }

        public IMessageSender MessageSender { get; } 
        public IMessageReceiver MessageReceiver { get; } 
        public InputChecker InputChecker { get; } = new InputChecker();
        public CameraPositionChecker CameraPositionChecker { get; private set; } = null;

        public void Dispose()
        {
            InputChecker.Dispose();
            MessageReceiver.Stop();
            CameraPositionChecker?.Stop();
        }

        private void OnKeyDown(object sender, EventArgs e)
            => MessageSender.SendMessage(MessageFactory.Instance.KeyDown(InputChecker.KeyCode));

        private void OnMouseMoved(object sender, EventArgs e) 
            => MessageSender.SendMessage(MessageFactory.Instance.MouseMoved(InputChecker.X, InputChecker.Y));

        private void OnMouseButton(object sender, MouseButtonEventArgs e)
            => MessageSender.SendMessage(MessageFactory.Instance.MouseButton(e.Info));

        private void AssignMessageReceivers()
        {
            new AppExitFromUnityMessage().Initialize(MessageReceiver);
        }
    }
}
