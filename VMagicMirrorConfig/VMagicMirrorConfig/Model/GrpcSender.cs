using System.Threading.Tasks;
using Grpc.Core;

namespace Baku.VMagicMirrorConfig
{
    class GrpcSender : IMessageSender
    {
        private static readonly string TargetIpAddress = "127.0.0.1";
        private static readonly int TargetPort = 53241;

        public GrpcSender()
        {
            _channel = new Channel(TargetIpAddress + ":" + TargetPort, ChannelCredentials.Insecure);
            _client = new VMagicMirror.VmmGrpc.VmmGrpcClient(_channel);
        }

        private readonly Channel _channel;
        private readonly VMagicMirror.VmmGrpc.VmmGrpcClient _client;

        public void SendMessage(Message message)
        {
            //NOTE: 前バージョンが投げっぱなし通信だったため、ここでも戻り値はとらない
            _client.CommandGeneric(new VMagicMirror.GenericCommandRequest()
            {
                Command = message.Command,
                Args = message.Content,
            });
        }

        public async Task<string> QueryMessageAsync(Message message)
        {
            var response = await _client.QueryGenericAsync(new VMagicMirror.GenericQueryRequest()
            {
                Command = message.Command,
                Args = message.Content,
            });

            return response.Result;
        }
    }
}
