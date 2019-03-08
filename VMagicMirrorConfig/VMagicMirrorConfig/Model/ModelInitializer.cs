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
        }

        public UdpSender UdpSender { get; } = new UdpSender();
        public InputChecker InputChecker { get; } = new InputChecker();


        private void OnKeyDown(object sender, EventArgs e)
            => UdpSender.SendMessage(UdpMessageFactory.Instance.KeyDown(InputChecker.KeyCode));

        private void OnMouseMoved(object sender, EventArgs e) 
            => UdpSender.SendMessage(UdpMessageFactory.Instance.MouseMoved(InputChecker.X, InputChecker.Y));

        private void OnMouseButton(object sender, MouseButtonEventArgs e)
            => UdpSender.SendMessage(UdpMessageFactory.Instance.MouseButton(e.Info));

        public void Dispose()
        {
            InputChecker.Dispose();
        }
    }
}
