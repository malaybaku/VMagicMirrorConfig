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
        public WordToMotionSettingViewModel() : base()
        {
            Items = new ReadOnlyObservableCollection<WordToMotionItemViewModel>(_items);
        }
        internal WordToMotionSettingViewModel(IMessageSender sender) : base(sender)
        {
            Items = new ReadOnlyObservableCollection<WordToMotionItemViewModel>(_items);
            _previewDataSender = new WordToMotionItemPreviewDataSender(sender);
            _previewDataSender.PrepareDataSend += 
                (_, __) => _dialogItem?.WriteToModel(_previewDataSender.MotionRequest);
        }

        private readonly WordToMotionItemPreviewDataSender _previewDataSender;
        private WordToMotionItemViewModel _dialogItem;
        
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

        private ActionCommand _addNewItemCommand;
        public ActionCommand AddNewItemCommand
            => _addNewItemCommand ?? (_addNewItemCommand = new ActionCommand(AddNewItem));
        private void AddNewItem()
        {
            _items.Add(new WordToMotionItemViewModel(this, MotionRequest.GetDefault()));
            RequestReload();
        }

        public void Play(WordToMotionItemViewModel item)
        {
            SendMessage(
                MessageFactory.Instance.PlayWordToMotionItem(
                    item.MotionRequest.ToJson()
                    )
                );
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

        public override void ResetToDefault()
        {
            //とりあえず無視！
            //本来はアイテム一覧を喜怒哀楽セットにするくらいがちょうどいいか
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
