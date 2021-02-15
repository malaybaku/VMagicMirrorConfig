using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace Baku.VMagicMirrorConfig
{
    public class WordToMotionSettingViewModel : SettingViewModelBase
    {
        internal WordToMotionSettingViewModel(WordToMotionSettingModel model, IMessageSender sender, IMessageReceiver receiver) : base(sender)
        {
            _model = model;
            Items = new ReadOnlyObservableCollection<WordToMotionItemViewModel>(_items);
            CustomMotionClipNames = new ReadOnlyObservableCollection<string>(_customMotionClipNames);

            Devices = WordToMotionDeviceItem.LoadAvailableItems();
            SelectedDevice = Devices.FirstOrDefault(d => d.Index == WordToMotionSetting.DeviceTypes.KeyboardWord);

            //TODO: この辺のSenderとかReceiverがモデル感あるよね
            _previewDataSender = new WordToMotionItemPreviewDataSender(sender);
            _previewDataSender.PrepareDataSend +=
                (_, __) => _dialogItem?.WriteToModel(_previewDataSender.MotionRequest);
            receiver.ReceivedCommand += OnReceiveCommand;
            MidiNoteReceiver = new MidiNoteReceiver(receiver);
            MidiNoteReceiver.Start();

            LoadDefaultItemsIfInitialStart();
        }

        private readonly WordToMotionSettingModel _model;
        private readonly WordToMotionItemPreviewDataSender _previewDataSender;
        private WordToMotionItemViewModel? _dialogItem;

        internal MidiNoteReceiver MidiNoteReceiver { get; }

        /// <summary>直近で読み込んだモデルに指定されている、VRM標準以外のブレンドシェイプ名の一覧を取得します。</summary>
        public IReadOnlyList<string> LatestAvaterExtraClipNames => _latestAvaterExtraClipNames;

        private string[] _latestAvaterExtraClipNames = new string[0];

        private void OnReceiveCommand(object? sender, CommandReceivedEventArgs e)
        {
            if (e.Command != ReceiveMessageNames.ExtraBlendShapeClipNames)
            {
                return;
            }

            //やることは2つ: 
            // - 知らない名前のブレンドシェイプが飛んできたら記憶する
            // - アバターが持ってるExtraなクリップ名はコレですよ、というのを明示的に与える
            _latestAvaterExtraClipNames = e.Args
                .Split(',')
                .Where(n => !string.IsNullOrEmpty(n))
                .ToArray();

            bool hasNewBlendShape = false;
            foreach (var name in _latestAvaterExtraClipNames
                .Where(n => !ExtraBlendShapeClipNames.Contains(n))
                )
            {
                hasNewBlendShape = true;
                ExtraBlendShapeClipNames.Add(name);
            }

            if (hasNewBlendShape)
            {
                //新しい名称のクリップを子要素側に反映
                foreach (var item in _items)
                {
                    item.CheckBlendShapeClipNames();
                }
            }

            foreach (var item in _items)
            {
                item.CheckAvatarExtraClips();
            }
        }

        public async Task InitializeCustomMotionClipNamesAsync()
        {
            var rawClipNames = await SendQueryAsync(MessageFactory.Instance.GetAvailableCustomMotionClipNames());
            var clipNames = rawClipNames.Split('\t');
            foreach (var name in clipNames)
            {
                _customMotionClipNames.Add(name);
            }
        }

        private readonly ObservableCollection<string> _customMotionClipNames = new ObservableCollection<string>();
        public ReadOnlyObservableCollection<string> CustomMotionClipNames { get; }

        public RPropertyMin<bool> EnableWordToMotion { get; } = new RPropertyMin<bool>(true);

        #region デバイスをWord to Motionに割り当てる設定

        public WordToMotionDeviceItem[] Devices { get; }

        private WordToMotionDeviceItem? _selectedDevice = null;
        public WordToMotionDeviceItem? SelectedDevice
        {
            get => _selectedDevice;
            set
            {
                if (_selectedDevice == value)
                {
                    return;
                }
                _selectedDevice = value;
                RaisePropertyChanged();
                SelectedDeviceType = _selectedDevice?.Index ?? WordToMotionSetting.DeviceTypes.None;
            }
        }


        private int _selectedDeviceType = WordToMotionSetting.DeviceTypes.None;
        public int SelectedDeviceType
        {
            get => _selectedDeviceType;
            set
            {
                if (SetValue(ref _selectedDeviceType, value))
                {
                    SelectedDevice = Devices.FirstOrDefault(d => d.Index == SelectedDeviceType);
                    EnableWordToMotion.Value = (SelectedDeviceType != WordToMotionSetting.DeviceTypes.None);
                    SendMessage(MessageFactory.Instance.SetDeviceTypeToStartWordToMotion(SelectedDeviceType));
                }
            }
        }

        #endregion

        //NOTE: 「UIに出さないけど保存はしたい」系のやつで、キャラロード時にUnityから勝手に送られてくる、という想定
        public List<string> ExtraBlendShapeClipNames { get; set; } = new List<string>();

        public ReadOnlyObservableCollection<WordToMotionItemViewModel> Items { get; }
        private readonly ObservableCollection<WordToMotionItemViewModel> _items
            = new ObservableCollection<WordToMotionItemViewModel>();

        public MidiNoteToMotionMapViewModel MidiNoteMap { get; private set; }
            = new MidiNoteToMotionMapViewModel(MidiNoteToMotionMap.LoadDefault());

        public RPropertyMin<bool> EnablePreview => _model.EnablePreview;

        /// <summary>Word to Motionのアイテム編集を開始した時すぐプレビューを開始するかどうか。普通は即スタートでよい</summary>
        public bool EnablePreviewWhenStartEdit { get; set; } = true;

        /// <summary>
        /// モデルが持ってるWord to MotionなりMidiなりの情報をVMにコピーします。
        /// </summary>
        public void LoadSerializedItems()
        {
            LoadMotionItems();
            LoadMidiSettingItems();
        }

        private void LoadMotionItems()
        {
            _items.Clear();

            var modelItems = _model.MotionRequests?.Requests;
            if (modelItems == null || modelItems.Length == 0)
            {
                return;
            }

            foreach (var item in modelItems)
            {
                if (item == null)
                {
                    //一応チェックしてるけど本来nullはあり得ない
                    LogOutput.Instance.Write("Receive null MotionRequest");
                    continue;
                }

                //NOTE: 前処理として、この時点で読み込んだモデルに不足なExtraClipがある場合は差し込んでおく
                //これは異バージョンとか考慮した処理です
                foreach (var extraClip in ExtraBlendShapeClipNames)
                {
                    if (!item.ExtraBlendShapeValues.Any(i => i.Name == extraClip))
                    {
                        item.ExtraBlendShapeValues.Add(new BlendShapePairItem()
                        {
                            Name = extraClip,
                            Value = 0,
                        });
                    }
                }

                _items.Add(new WordToMotionItemViewModel(this, item));
            }
        }

        private void LoadMidiSettingItems()
        {
            var midiNoteMap = _model.MidiNoteToMotionMap;
            if (midiNoteMap?.Items == null || midiNoteMap.Items.Count == 0)
            {
                MidiNoteMap = new MidiNoteToMotionMapViewModel(MidiNoteToMotionMap.LoadDefault());
                return;
            }

            MidiNoteMap = new MidiNoteToMotionMapViewModel(midiNoteMap);
        }


        /// <summary>
        /// <see cref="ItemsContentString"/>に、現在の<see cref="Items"/>の内容をシリアライズした文字列を設定します。
        /// </summary>
        public void SaveItems()
        {
            _model.RequestSerializeItems();
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

        public async Task DeleteItem(WordToMotionItemViewModel item)
        {
            var indication = MessageIndication.DeleteWordToMotionItem(LanguageSelector.Instance.LanguageName);
            bool res = await MessageBoxWrapper.Instance.ShowAsync(
                indication.Title,
                string.Format(indication.Content, item.Word),
                MessageBoxWrapper.MessageBoxStyle.OKCancel
                );
            if (res)
            {
                _items.Remove(item);
                RequestReload();
            }
        }

        /// <summary>
        /// 指定されたアイテムについて、必要ならアプリの設定から忘却させる処理をします。
        /// </summary>
        /// <param name="blendShapeItem"></param>
        public async void ForgetClip(BlendShapeItemViewModel blendShapeItem)
        {
            string name = blendShapeItem.BlendShapeName;
            var indication = MessageIndication.ForgetBlendShapeClip(LanguageSelector.Instance.LanguageName);
            bool res = await MessageBoxWrapper.Instance.ShowAsyncOnWordToMotionItemEdit(
                indication.Title,
                string.Format(indication.Content, name)
                );
            if (res)
            {
                foreach (var item in _items)
                {
                    item.ForgetClip(name);
                }

                if (ExtraBlendShapeClipNames.Contains(name))
                {
                    ExtraBlendShapeClipNames.Remove(name);
                }
                RequestReload();
            }
        }

        /// <summary>モーション一覧の情報が変わったとき、Unity側に再読み込みをリクエストします。</summary>
        public void RequestReload()
        {
            //NOTE: この結果シリアライズ文字列が変わるとモデル側でメッセージ送信もやってくれる
            SaveItems();
        }

        private ActionCommand? _openKeyAssigmnentEditorCommand = null;
        public ActionCommand OpenKeyAssignmentEditorCommand
            => _openKeyAssigmnentEditorCommand ??= new ActionCommand(OpenKeyAssignmentEditor);

        private void OpenKeyAssignmentEditor()
        {
            //note: 今のところMIDIコン以外は割り当て固定です
            if (SelectedDeviceType != WordToMotionSetting.DeviceTypes.MidiController)
            {
                return;
            }

            var vm = new MidiNoteToMotionEditorViewModel(MidiNoteMap, MidiNoteReceiver);

            SendMessage(MessageFactory.Instance.RequireMidiNoteOnMessage(true));
            var window = new MidiNoteAssignEditorWindow()
            {
                DataContext = vm,
            };
            bool? res = window.ShowDialog();
            SendMessage(MessageFactory.Instance.RequireMidiNoteOnMessage(false));

            if (res != true)
            {
                return;
            }

            MidiNoteMap.Load(vm.Result);
            RequestReload();
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
                () => SettingResetUtils.ResetSingleCategoryAsync(LoadDefaultItems)
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
            ExtraBlendShapeClipNames.Clear();
            _items.Clear();
            //NOTE: 現在ロードされてるキャラがいたら、そのキャラのブレンドシェイプをただちに当て直す
            ExtraBlendShapeClipNames.AddRange(_latestAvaterExtraClipNames);

            var models = MotionRequest.GetDefaultMotionRequestSet();
            for (int i = 0; i < models.Length; i++)
            {
                foreach(var extraClip in ExtraBlendShapeClipNames)
                {
                    models[i].ExtraBlendShapeValues.Add(new BlendShapePairItem()
                    {
                        Name = extraClip,
                        Value = 0,
                    });
                }
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
           
            EnablePreview.Value = EnablePreviewWhenStartEdit;

            if (dialog.ShowDialog() == true)
            {
                item.SaveChanges();
                RequestReload();
            }
            else
            {
                item.ResetChanges();
            }

            EnablePreviewWhenStartEdit = EnablePreview.Value;
            EnablePreview.Value = false;
            _dialogItem = null;
        }

        public void RequestCustomMotionDoctor()
            => SendMessage(MessageFactory.Instance.RequestCustomMotionDoctor());
    }

    /// <summary> Word to Motion機能のコントロールに利用できるデバイスの選択肢1つに相当するViewModelです。 </summary>
    public class WordToMotionDeviceItem : ViewModelBase
    {
        private WordToMotionDeviceItem(int index, string enName, string jpName)
        {
            Index = index;
            _enName = enName;
            _jpName = jpName;
            SetLanguage(Languages.Japanese);
            LanguageSelector.Instance.LanguageChanged += SetLanguage;
        }

        public int Index { get; }

        private readonly string _jpName;
        private readonly string _enName;

        private string _displayName = "";
        public string DisplayName
        {
            get => _displayName;
            private set => SetValue(ref _displayName, value);
        }

        internal void SetLanguage(Languages lang)
        {
            DisplayName = lang switch
            {
                Languages.Japanese => _jpName,
                Languages.English => _enName,
                _ => _enName,
            };
        }

        public static WordToMotionDeviceItem None()
            => new WordToMotionDeviceItem(
                WordToMotionSetting.DeviceTypes.None, "None", "なし"
                );

        public static WordToMotionDeviceItem KeyboardTyping()
            => new WordToMotionDeviceItem(
                WordToMotionSetting.DeviceTypes.KeyboardWord, "Keyboard (word)", "キーボード (単語入力)"
                );

        public static WordToMotionDeviceItem Gamepad() 
            => new WordToMotionDeviceItem(
                WordToMotionSetting.DeviceTypes.Gamepad, "Gamepad", "ゲームパッド"
                );

        public static WordToMotionDeviceItem KeyboardNumKey()
            => new WordToMotionDeviceItem(
                WordToMotionSetting.DeviceTypes.KeyboardTenKey, "Keyboard (num 0-8)", "キーボード (数字の0-8)"
                );

        public static WordToMotionDeviceItem MidiController()
            => new WordToMotionDeviceItem(
                WordToMotionSetting.DeviceTypes.MidiController, "MIDI Controller", "MIDIコントローラ"
                );

        public static WordToMotionDeviceItem[] LoadAvailableItems()
            => new[]
            {
                None(),
                KeyboardTyping(),
                Gamepad(),
                KeyboardNumKey(),
                MidiController(),
            };
    }

}
