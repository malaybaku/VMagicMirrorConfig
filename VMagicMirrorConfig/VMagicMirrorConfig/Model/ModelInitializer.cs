using System;

namespace Baku.VMagicMirrorConfig
{
    class ModelInitializer : IDisposable
    {
        public void Initialize()
        {
            InputChecker.Start();

            InputChecker.MouseMoved += OnMouseMoved;
            InputChecker.MouseButton += OnMouseButton;
            InputChecker.KeyDown += OnKeyDown;

            MessageReceiver.Start();
            AssignMessageReceivers();
        }

        public IMessageSender MessageSender { get; } = new GrpcSender();
        public IMessageReceiver MessageReceiver { get; } = new GrpcReceiver();
        public InputChecker InputChecker { get; } = new InputChecker();

        private void OnKeyDown(object sender, EventArgs e)
            => MessageSender.SendMessage(MessageFactory.Instance.KeyDown(InputChecker.KeyCode));

        private void OnMouseMoved(object sender, EventArgs e) 
            => MessageSender.SendMessage(MessageFactory.Instance.MouseMoved(InputChecker.X, InputChecker.Y));

        private void OnMouseButton(object sender, MouseButtonEventArgs e)
            => MessageSender.SendMessage(MessageFactory.Instance.MouseButton(e.Info));

        public void Dispose()
        {
            InputChecker.Dispose();
            MessageReceiver.Stop();
        }

        private void AssignMessageReceivers()
        {
            new AppExitFromUnityMessage().Initialize(MessageReceiver);
        }
    }
}
