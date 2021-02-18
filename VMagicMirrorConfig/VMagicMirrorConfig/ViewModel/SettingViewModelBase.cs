using System.Threading.Tasks;

namespace Baku.VMagicMirrorConfig
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// 継承したクラスでは、publicプロパティデータのうちファイルにセーブしたくないものに[XmlIgnore]属性を付ける事
    /// </remarks>
    public abstract class SettingViewModelBase : ViewModelBase
    {
        private protected SettingViewModelBase(IMessageSender sender)
        {
            Sender = sender;
        }

        private protected readonly IMessageSender Sender;

        private protected virtual void SendMessage(Message message)
            => Sender.SendMessage(message);

        private protected async Task<string> SendQueryAsync(Message message)
            => await Sender.QueryMessageAsync(message);
    }
}
