using System;
using Baku.VMagicMirror;
using Grpc.Core;

namespace Baku.VMagicMirrorConfig
{
    class GrpcReceiver : IMessageReceiver
    {
        private const string IpAddress = "127.0.0.1";
        private const int Port = 53242;

        private Server _server;
        private GrpcServerImpl _serverImpl;

        public void Start()
        {
            _serverImpl = new GrpcServerImpl();
            _serverImpl.ReceivedCommand += OnReceivedCommand;
            _serverImpl.ReceivedQuery += OnReceivedQuery;
            _server = new Server()
            {
                Services =
                {
                    VmmGrpc.BindService(_serverImpl)
                },
                Ports =
                {
                    new ServerPort(IpAddress, Port, ServerCredentials.Insecure),
                },
            };
            _server.Start();
        }

        public void Stop()
        {
            _serverImpl.ReceivedCommand -= OnReceivedCommand;
            _serverImpl.ReceivedQuery -= OnReceivedQuery;
            _serverImpl = null;

            _server.ShutdownAsync().Wait();
            _server = null;
        }

        public event EventHandler<CommandReceivedEventArgs> ReceivedCommand;
        public event EventHandler<QueryReceivedEventArgs> ReceivedQuery;

        private void OnReceivedQuery(object sender, QueryReceivedEventArgs e)
            => ReceivedQuery?.Invoke(this, e);

        private void OnReceivedCommand(object sender, CommandReceivedEventArgs e)
            => ReceivedCommand?.Invoke(this, e);

    }
}
