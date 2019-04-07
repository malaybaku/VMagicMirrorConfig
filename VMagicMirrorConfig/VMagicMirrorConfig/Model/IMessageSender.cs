using System.Threading.Tasks;

namespace Baku.VMagicMirrorConfig
{
    /// <summary>
    /// IPC用のメッセージを送信する方式を定義します。
    /// </summary>
    /// <remarks>
    /// GRPCにべったり依存するのも嫌なので、簡単なものは文字列送る原始的な方法で済ませる。データ構造が面倒なものは特別に作る。
    /// </remarks>
    internal interface IMessageSender
    {
        void SendMessage(Message message);

        Task<string> QueryMessageAsync(Message message);
    }
}
