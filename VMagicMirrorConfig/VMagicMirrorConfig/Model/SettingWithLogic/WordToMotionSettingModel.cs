using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Linq;

namespace Baku.VMagicMirrorConfig
{
    class WordToMotionSettingModel : SettingModelBase<WordToMotionSetting>
    {

        public WordToMotionSettingModel(IMessageSender sender) : base(sender)
        {
            var settings = WordToMotionSetting.Default;
            var factory = MessageFactory.Instance;

            SelectedDeviceType = new RPropertyMin<int>(settings.SelectedDeviceType, i => SendMessage(factory.SetDeviceTypeToStartWordToMotion(i)));
            ItemsContentString = new RPropertyMin<string>(settings.ItemsContentString, s => SendMessage(factory.ReloadMotionRequests(s)));
            MidiNoteMapString = new RPropertyMin<string>(settings.MidiNoteMapString, s => SendMessage(factory.LoadMidiNoteToMotionMap(s)));
            EnablePreview = new RPropertyMin<bool>(false, b => SendMessage(factory.EnableWordToMotionPreview(b)));
        }

        public override void ResetToDefault()
        {
            //何もしない: ここは複雑すぎるので…
        }

        //TODO: ここデフォルトはKeyboardWordじゃなかったっけ？要確認。
        public RPropertyMin<int> SelectedDeviceType { get; }

        //NOTE: 「UIに出さないけど保存はしたい」系のやつで、キャラロード時にUnityから勝手に送られてくる
        public List<string> ExtraBlendShapeClipNames { get; set; } = new List<string>();

        public RPropertyMin<bool> EnablePreview { get; }

        public RPropertyMin<string> ItemsContentString { get; }

        public RPropertyMin<string> MidiNoteMapString { get; }

        //TODO: この辺を非null保証し、シリアライズ文字列が無効だったらデフォルト設定が入るようにしたい

        public MotionRequestCollection? MotionRequests { get; private set; }

        public MidiNoteToMotionMap? MidiNoteToMotionMap { get; private set; }

        public void RequestSerializeItems()
        {
            SaveMotionRequests();
            SaveMidiNoteMap();
        }

        public void SaveMotionRequests() => ItemsContentString.Value = MotionRequests?.ToJson() ?? "";
        public void SaveMidiNoteMap() => MidiNoteMapString.Value = MidiNoteToMotionMap?.ToJson() ?? "";

        /// <summary>
        /// 指定したモーションを実行します。再生ボタンを押したときに呼び出す想定です
        /// </summary>
        /// <param name="item"></param>
        public void Play(MotionRequest item)
            => SendMessage(MessageFactory.Instance.PlayWordToMotionItem(item.ToJson()));

        #region アイテムの並べ替えと削除

        public void MoveUpItem(MotionRequest item)
        {
            if (MotionRequests?.Requests == null) { return; }

            var requests = MotionRequests.Requests.ToList();
            int index = requests.IndexOf(item);
            if (index > 0)
            {
                requests.RemoveAt(index);
                requests.Insert(index - 1, item);
                MotionRequests = new MotionRequestCollection(requests.ToArray());
                SaveMotionRequests();
            }
        }

        public void MoveDownItem(MotionRequest item)
        {
            if (MotionRequests?.Requests == null) { return; }

            var requests = MotionRequests.Requests.ToList();
            int index = requests.IndexOf(item);
            if (index < requests.Count - 1)
            {
                requests.RemoveAt(index);
                requests.Insert(index + 1, item);
                MotionRequests = new MotionRequestCollection(requests.ToArray());
                SaveMotionRequests();
            }
        }

        public async Task DeleteItem(MotionRequest item)
        {
            if (MotionRequests?.Requests?.Contains(item) != true) { return; }

            var indication = MessageIndication.DeleteWordToMotionItem();
            bool res = await MessageBoxWrapper.Instance.ShowAsync(
                indication.Title,
                string.Format(indication.Content, item.Word),
                MessageBoxWrapper.MessageBoxStyle.OKCancel
                );

            if (res)
            {
                var requests = MotionRequests.Requests.ToList();
                //ダイアログ表示を挟んでいるので再チェック
                if (requests.Contains(item))
                {
                    requests.Remove(item);
                    MotionRequests = new MotionRequestCollection(requests.ToArray());
                    SaveMotionRequests();
                }
            }
        }

        public void AddNewItem()
        {
            if (MotionRequests == null) { return; }

            var request = MotionRequests.Requests.ToList();
            request.Add(MotionRequest.GetDefault());
            SaveMotionRequests();
        }

        public void LoadDefaultMotionRequests() => LoadDefaultMotionRequests(new List<string>());

        public void LoadDefaultMotionRequests(List<string> extraBlendShapeClipNames)
        {
            var items = MotionRequest.GetDefaultMotionRequestSet();
            for (int i = 0; i < items.Length; i++)
            {
                foreach (var extraClip in extraBlendShapeClipNames)
                {
                    items[i].ExtraBlendShapeValues.Add(new BlendShapePairItem()
                    {
                        Name = extraClip,
                        Value = 0,
                    });
                }
            }
            MotionRequests = new MotionRequestCollection(items);
            SaveMotionRequests();
        }

        #endregion

        protected override void PreSave()
        {
            RequestSerializeItems();
        }

        protected override void AfterLoad(WordToMotionSetting entity)
        {
            try
            {
                using (var reader = new StringReader(ItemsContentString.Value))
                {
                    MotionRequests = MotionRequestCollection.DeserializeFromJson(reader);
                }
            }
            catch(Exception ex)
            {
                LogOutput.Instance.Write(ex);
                MotionRequests = new MotionRequestCollection(MotionRequest.GetDefaultMotionRequestSet());
            }

            try
            {
                using (var reader = new StringReader(MidiNoteMapString.Value))
                {
                    MidiNoteToMotionMap = MidiNoteToMotionMap.DeserializeFromJson(reader);
                }
            }
            catch (Exception ex)
            {
                LogOutput.Instance.Write(ex);
                MidiNoteToMotionMap = MidiNoteToMotionMap.LoadDefault();
            }
        }

    }
}
