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
            _sender = null;
        }

        private protected SettingViewModelBase(IMessageSender sender)
        {
            _sender = sender;
        }

        private readonly IMessageSender _sender;

        private protected void SendMessage(Message message)
            => _sender?.SendMessage(message);

        private protected async Task<string> SendQueryAsync(Message message) 
            => (_sender != null) ? 
            await _sender.QueryMessageAsync(message) : 
            "";

        public abstract void ResetToDefault();

        public void CopyFrom<T>(T source)
            where T : SettingViewModelBase
        {
            CopyProperties(source, this, typeof(T));
        }

        private static void CopyProperties(object src, object dest, Type type)
        {
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
