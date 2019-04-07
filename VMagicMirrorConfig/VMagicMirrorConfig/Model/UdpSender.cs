using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Baku.VMagicMirrorConfig
{
    //TODO: NamedPipe等への変更(ループバックとはいえUDP使ってるのが落ち着かない為)
    class UdpSender : IMessageSender
    {
        private static readonly string TargetIpAddress = "127.0.0.1";
        private static readonly int TargetPort = 53241;

        public void SendMessage(Message message)
        {
            var bytes = Encoding.UTF8.GetBytes(message.Command + ":" + message.Content);
            var client = new UdpClient();
            client.Send(bytes, bytes.Length, TargetIpAddress, TargetPort);
        }

        public Task<string> QueryMessageAsync(Message message)
        {
            throw new NotImplementedException();
        }
    }
}
