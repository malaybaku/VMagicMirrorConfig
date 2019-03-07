using System.Net.Sockets;
using System.Text;

namespace Baku.VMagicMirrorConfig
{
    class UdpSender
    {
        private static readonly string TargetIpAddress = "127.0.0.1";
        private static readonly int TargetPort = 53241;

        public void SendMessage(UdpMessage message)
        {
            var bytes = Encoding.UTF8.GetBytes(message.Command + ":" + message.Content);
            var client = new UdpClient();
            client.Send(bytes, bytes.Length, TargetIpAddress, TargetPort);
        }
    }
}
