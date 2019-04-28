using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;
using Microsoft.Win32;

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
            Startup = null;
        }

        private protected SettingViewModelBase(IMessageSender sender, StartupSettingViewModel startup)
        {
            _sender = sender;
            Startup = startup;
        }

        private readonly IMessageSender _sender;

        //NOTE: 他のタブにもスタートアップ時の有効/無効がいじれるチェックボックス置きたいので追加
        [XmlIgnore]
        public StartupSettingViewModel Startup { get; }

        private protected void SendMessage(Message message)
            => _sender?.SendMessage(message);

        private protected async Task<string> SendQueryAsync(Message message) 
            => (_sender != null) ? 
            await _sender.QueryMessageAsync(message) : 
            "";

        private ActionCommand _resetToDefaultCommand;
        public ActionCommand ResetToDefaultCommand
            => _resetToDefaultCommand ?? (_resetToDefaultCommand = new ActionCommand(ResetToDefault));

        private ActionCommand _saveSettingCommand;
        public ActionCommand SaveSettingCommand
            => _saveSettingCommand ?? (_saveSettingCommand = new ActionCommand(SaveSetting));

        private ActionCommand _loadSettingCommand;
        public ActionCommand LoadSettingCommand
            => _loadSettingCommand ?? (_loadSettingCommand = new ActionCommand(LoadSetting));

        protected abstract void ResetToDefault();

        private void SaveSetting()
        {
            var dialog = new SaveFileDialog()
            {
                Title = SaveDialogTitle,
                Filter = FileIoDialogFilter,
                DefaultExt = FileExt,
                AddExtension = true,
            };
            if (dialog.ShowDialog() == true)
            {
                SaveSettingTo(dialog.FileName);
            }
        }

        private void LoadSetting()
        {
            var dialog = new OpenFileDialog()
            {
                Title = LoadDialogTitle,
                Filter = FileIoDialogFilter,
                Multiselect = false,
            };
            if (dialog.ShowDialog() == true)
            {
                LoadSettingFrom(dialog.FileName);
            }
        }

        protected virtual string SaveDialogTitle => "";
        protected virtual string LoadDialogTitle => "";
        protected virtual string FileIoDialogFilter => "";
        protected virtual string FileExt => "";

        #region シリアライズ周り

        internal void SaveSettingTo(string path)
        {
            using (var sw = new StreamWriter(path))
            {
                new XmlSerializer(GetType()).Serialize(sw, this);
            }
        }

        internal void LoadSettingFrom(string path)
        {
            try
            {
                var type = GetType();
                using (var sr = new StreamReader(path))
                {
                    object src = new XmlSerializer(type).Deserialize(sr);
                    CopyProperties(src, this, type);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Failed to load setting file {path} : {ex.Message}");
            }
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

        #endregion
    }
}
