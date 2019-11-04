using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Serialization;

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
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// Only used by serializer
        /// </remarks>
        protected SettingViewModelBase()
        {
            Sender = new EmptyMessageSender();
        }

        private protected SettingViewModelBase(IMessageSender sender)
        {
            Sender = sender;
        }

        private protected readonly IMessageSender Sender;

        private protected virtual void SendMessage(Message message)
            => Sender.SendMessage(message);

        private protected async Task<string> SendQueryAsync(Message message)
            => await Sender.QueryMessageAsync(message);

        public abstract void ResetToDefault();

        public void CopyFrom<T>(T source)
            where T : SettingViewModelBase?
        {
            CopyProperties(source, this, typeof(T));
        }

        private static void CopyProperties(object? src, object? dest, Type type)
        {
            if (src == null || dest == null)
            {
                return;
            }

            foreach (var prop in type
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p =>
                    !typeof(ICommand).IsAssignableFrom(p.PropertyType) &&
                    p.GetCustomAttribute<XmlIgnoreAttribute>() == null
                    )
                )
            {
                if (typeof(SettingViewModelBase).IsAssignableFrom(prop.PropertyType))
                {
                    //プロパティがSettingViewModelBaseの場合、インスタンスをセットせず、プロパティを再帰コピー
                    CopyProperties(
                        prop.GetValue(src),
                        prop.GetValue(dest),
                        prop.PropertyType
                        );
                }
                else
                {
                    //プロパティが通常の組み込み型とかである場合、たんに値をコピー
                    prop.SetValue(dest, prop.GetValue(src));
                }
            }
        }
    }
}
