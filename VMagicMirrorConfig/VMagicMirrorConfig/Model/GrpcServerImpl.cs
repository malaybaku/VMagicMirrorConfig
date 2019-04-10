using System;
using System.Threading.Tasks;
using Baku.VMagicMirror;
using Grpc.Core;

namespace Baku.VMagicMirrorConfig
{
    public class GrpcServerImpl : VmmGrpc.VmmGrpcBase
    {
        public override Task<GenericCommandResponse> CommandGeneric(GenericCommandRequest request, ServerCallContext context)
        {
            ReceivedCommand?.Invoke(this, new CommandReceivedEventArgs(request.Command, request.Args));
            return Task.FromResult(new GenericCommandResponse()
            {
                Result = true,
            });
        }

        public override Task<GenericQueryResponse> QueryGeneric(GenericQueryRequest request, ServerCallContext context)
        {
            var ea = new QueryReceivedEventArgs(request.Command, request.Args);
            ReceivedQuery?.Invoke(this, ea);
            return Task.FromResult(new GenericQueryResponse()
            {
                Result = ea.Result,
            });
        }

        public event EventHandler<CommandReceivedEventArgs> ReceivedCommand;
        public event EventHandler<QueryReceivedEventArgs> ReceivedQuery;
    }


}
