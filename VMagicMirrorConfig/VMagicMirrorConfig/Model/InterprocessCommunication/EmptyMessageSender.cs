using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Baku.VMagicMirrorConfig
{
    class EmptyMessageSender : IMessageSender
    {
        public void EndCommandComposite() 
        {
        }

        public Task<string> QueryMessageAsync(Message message)
        {
            return Task.FromResult("");
        }

        public void SendMessage(Message message)
        {
        }

        public void StartCommandComposite()
        {
        }
    }
}
