using System;
using System.Threading.Tasks;
using Baku.VMagicMirrorConfig.Mmf;

namespace Baku.VMagicMirrorConfig
{
    class MmfClient : IMessageSender, IMessageReceiver
    {
        private const string ChannelName = "Baku.VMagicMirror";

        public MmfClient()
        {
            _client = new MemoryMappedFileConnectClient();
            _client.ReceiveCommand += OnReceivedCommand;
            _client.ReceiveQuery += OnReceivedQuery;
        }

        private readonly MemoryMappedFileConnectClient _client;

        #region IMessageSender

        public void SendMessage(Message message)
        {
            //NOTE: 前バージョンが投げっぱなし通信だったため、ここでも戻り値はとらない
            try
            {
                _client.SendCommand(message.Command + ":" + message.Content);
            }
            catch (Exception ex)
            {
                LogOutput.Instance.Write(ex);
            }
        }

        public async Task<string> QueryMessageAsync(Message message)
        {
            try
            {
                var response = await _client.SendQueryAsync(message.Command + ":" + message.Content);
                return response;
            }
            catch (Exception ex)
            {
                LogOutput.Instance.Write(ex);
                return "";
            }
        }

        #endregion

        #region IMessageReceiver

        public void Start()
        {
            //NOTE: この実装だと受信開始するまで送信もできない(ややヘンテコな感じがする)が、気にしないことにする
            StartAsync();
        }

        private async void StartAsync()
        {
            await _client.StartAsync(ChannelName);
        }

        public void Stop()
        {
            _client.Stop();
        }

        public event EventHandler<CommandReceivedEventArgs> ReceivedCommand;
        public event EventHandler<QueryReceivedEventArgs> ReceivedQuery;

        private void OnReceivedCommand(object sender, ReceiveCommandEventArgs e)
        {
            string content = e.Command;
            int i = FindColonCharIndex(content);
            string command = (i == -1) ? content : content.Substring(0, i);
            string args = (i == -1) ? "" : content.Substring(i + 1);

            ReceivedCommand?.Invoke(this, new CommandReceivedEventArgs(command, args));            
        }

        private void OnReceivedQuery(object sender, ReceiveQueryEventArgs e)
        {
            string content = e.Query.Query;
            int i = FindColonCharIndex(content);
            string command = (i == -1) ? content : content.Substring(0, i);
            string args = (i == -1) ? "" : content.Substring(i + 1);

            var ea = new QueryReceivedEventArgs(command, args);
            ReceivedQuery?.Invoke(this, ea);

            e.Query.Reply(string.IsNullOrWhiteSpace(ea.Result) ? "" : ea.Result);
        }

        //コマンド名と引数名の区切り文字のインデックスを探します。
        private static int FindColonCharIndex(string s)
        {
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == ':')
                {
                    return i;
                }
            }
            return -1;
        }

        #endregion

    }
}
