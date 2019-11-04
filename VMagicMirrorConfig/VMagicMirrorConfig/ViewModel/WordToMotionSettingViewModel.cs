using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Serialization;

namespace Baku.VMagicMirrorConfig
{
    public class WordToMotionSettingViewModel : SettingViewModelBase
    {
        private const int DeviceTypeNone = 0;
        private const int DeviceTypeGamepad = 1;
        private const int DeviceTypeKeyboard = 2;

        public WordToMotionSettingViewModel() : base()
        {
            Items = new ReadOnlyObservableCollection<WordToMotionItemViewModel>(_items);
            _previewDataSender = new WordToMotionItemPreviewDataSender(Sender);
        }
        internal WordToMotionSettingViewModel(IMessageSender sender) : base(sender)
        {
            Items = new ReadOnlyObservableCollection<WordToMotionItemViewModel>(_items);
            _previewDataSender = new WordToMotionItemPreviewDataSender(sender);
            _previewDataSender.PrepareDataSend += 
                (_, __) => _dialogItem?.WriteToModel(_previewDataSender.MotionRequest);

            LoadDefaultItemsIfInitialStart();
        }

        private readonly WordToMotionItemPreviewDataSender _previewDataSender;
        private WordToMotionItemViewModel? _dialogItem;
        
        private bool _enableWordToMotion = true;
        public bool EnableWordToMotion
        {
            get => _enableWordToMotion;
            set
            {
                if (SetValue(ref _enableWordToMotion, value))
                {
                    SendMessage(MessageFactory.Instance.EnableWordToMotion(EnableWordToMotion));
                }
            }
        }

        #region デバイスをWord to Motionに割り当てる設定

        private bool _useNoDeviceToStartWordToMotion = true;
        public bool UseNoDeviceToStartWordToMotion
        {
            get => _useNoDeviceToStartWordToMotion;
            set
            {
                if (value == _useNoDeviceToStartWordToMotion)
                {
                    return;
                }

                if (value)
                {
                    UseGamepadToStartWordToMotion = false;
                    UseKeyboardToStartWordToMotion = false;
                    SendMessage(MessageFactory.Instance.SetDeviceTypeToStartWordToMotion(DeviceTypeNone));
                }
                _useNoDeviceToStartWordToMotion = value;
                RaisePropertyChanged();
            }
        }

        private bool _useGamepadToStartWordToMotion = false;
        public bool UseGamepadToStartWordToMotion
        {
            get => _useGamepadToStartWordToMotion;
            set
            {
                if (value == _useGamepadToStartWordToMotion)
                {
                    return;
                }

                if (value)
                {
                    UseNoDeviceToStartWordToMotion = false;
                    UseKeyboardToStartWordToMotion = false;                    
                    SendMessage(MessageFactory.Instance.SetDeviceTypeToStartWordToMotion(DeviceTypeGamepad));
                }
                _useGamepadToStartWordToMotion = value;
                RaisePropertyChanged();
            }
        }

        private bool _useKeyboardToStartWordToMotion = false;
        public bool UseKeyboardToStartWordToMotion
        {
            get => _useKeyboardToStartWordToMotion;
            set
            {
                if (value == _useKeyboardToStartWordToMotion)
                {
                    return;
                }

                if (value)
                {
                    UseNoDeviceToStartWordToMotion = false;
                    UseGamepadToStartWordToMotion = false;
                    SendMessage(MessageFactory.Instance.SetDeviceTypeToStartWordToMotion(DeviceTypeKeyboard));
                }
                _useKeyboardToStartWordToMotion = value;
                RaisePropertyChanged();
            }
        }

        #endregion

        [XmlIgnore]
        public ReadOnlyObservableCollection<WordToMotionItemViewModel> Items { get; }
        private readonly ObservableCollection<WordToMotionItemViewModel> _items 
            = new ObservableCollection<WordToMotionItemViewModel>();

        /// <summary>
        /// <see cref="Items"/>をシリアライズした文字列。
        /// </summary>
        public string ItemsContentString { get; set; } = "";

        //NOTE: プレビュー関係の想定挙動
        // EnablePreviewWhenStartEdit == trueなら、
        private bool _enablePreview = false;
        [XmlIgnore]
        public bool EnablePreview
        {
            get => _enablePreview;
            set
            {
                if (SetValue(ref _enablePreview, value))
                {
                    SendMessage(MessageFactory.Instance.EnableWordToMotionPreview(value));
                    if (value)
                    {
                        _previewDataSender.Start();
                    }
                    else
                    {
                        _previewDataSender.End();
                    }
                }
            }
        }

        [XmlIgnore]
        public bool EnablePreviewWhenStartEdit { get; set; } = true;

        /// <summary>
        /// <see cref="ItemsContentString"/>の内容を<see cref="Items"/>にコピーします。
        /// </summary>
        public void LoadItems()
        {
            _items.Clear();
            if (string.IsNullOrWhiteSpace(ItemsContentString))
            {
                return;
            }

            try
            {
                using (var reader = new StringReader(ItemsContentString))
                {
                    var requests = MotionRequestCollection.DeserializeFromJson(reader);
                    foreach (var item in requests.Requests)
                    {
                        _items.Add(new WordToMotionItemViewModel(this, item));
                    }
                }
            }
            catch(Exception ex)
            {
                LogOutput.Instance.Write(ex);
                //諦める: ちょっとザツだが…
            }
        }

        /// <summary>
        /// <see cref="ItemsContentString"/>に、現在の<see cref="Items"/>の内容をシリアライズした文字列を設定します。
        /// </summary>
        public void SaveItems()
        {
            ItemsContentString = new MotionRequestCollection(
                _items.Select(i => i.MotionRequest).ToArray()
                )
                .ToJson();
        }

        public void Play(WordToMotionItemViewModel item)
        {
            if (item.MotionRequest != null)
            {
                SendMessage(
                    MessageFactory.Instance.PlayWordToMotionItem(
                        item.MotionRequest.ToJson()
                        )
                    );
            }
        }

        public void MoveUpItem(WordToMotionItemViewModel item)
        {
            int index = _items.IndexOf(item);
            if (index > 0)
            {
                _items.Move(index, index - 1);
                RequestReload();
            }
        }

        public void MoveDownItem(WordToMotionItemViewModel item)
        {
            int index = _items.IndexOf(item);
            if (index < _items.Count - 1)
            {
                _items.Move(index, index + 1);
                RequestReload();
            }
        }

        public void DeleteItem(WordToMotionItemViewModel item)
        {
            var indication = MessageIndication.DeleteWordToMotionItem(LanguageSelector.Instance.LanguageName);
            var res = MessageBox.Show(
                Application.Current.MainWindow,
                string.Format(indication.Content, item.Word),
                indication.Title,
                MessageBoxButton.OKCancel
                );
            if (res == MessageBoxResult.OK)
            {
                _items.Remove(item);
                RequestReload();
            }
        }

        /// <summary>モーション一覧の情報が変わったとき、Unity側に再読み込みをリクエストします。</summary>
        public void RequestReload()
        {
            SaveItems();
            SendMessage(MessageFactory.Instance.ReloadMotionRequests(ItemsContentString));
        }


        private ActionCommand? _addNewItemCommand;
        public ActionCommand AddNewItemCommand
            => _addNewItemCommand ??= new ActionCommand(() =>
            {
                _items.Add(new WordToMotionItemViewModel(this, MotionRequest.GetDefault()));
                RequestReload();
            });

        private ActionCommand? _resetByDefaultItemsCommand = null;
        public ActionCommand ResetByDefaultItemsCommand
            => _resetByDefaultItemsCommand ??= new ActionCommand(
                () => SettingResetUtils.ResetSingleCategorySetting(LoadDefaultItems)
                );

        public override void ResetToDefault()
        {
            //何もしない: ここは設定がフクザツなのでとりあえずいじらない方針で。
            //(このパネル単体のリセットUIがちゃんとできたら何か考える)
        }

        //このマシン上でこのバージョンのVMagicMirrorが初めて実行されたと推定できるとき、
        //デフォルトのWord To Motion一覧を生成して初期化します。
        public void LoadDefaultItemsIfInitialStart()
        {
            if (SpecialFilePath.SettingFileExists()) 
            {
                return;
            }
            LoadDefaultItems();
        }

        private void LoadDefaultItems()
        {
            _items.Clear();
            var models = MotionRequest.GetDefaultMotionRequestSet();
            for (int i = 0; i < models.Length; i++)
            {
                _items.Add(new WordToMotionItemViewModel(this, models[i]));
            }
            RequestReload();
        }

        public void EditItemByDialog(WordToMotionItemViewModel item)
        {
            var dialog = new WordToMotionItemEditWindow()
            {
                DataContext = item,
                Owner = SettingWindow.CurrentWindow,
            };

            _dialogItem = item;
            EnablePreview = EnablePreviewWhenStartEdit;

            if (dialog.ShowDialog() == true)
            {
                item.SaveChanges();
                RequestReload();
            }
            else
            {
                item.ResetChanges();
            }

            EnablePreviewWhenStartEdit = EnablePreview;
            EnablePreview = false;
            _dialogItem = null;
        }
    }

}
