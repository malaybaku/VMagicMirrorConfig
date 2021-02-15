using System;
using System.Collections.Generic;
using System.IO;

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
            ItemsContentString.Value = MotionRequests?.ToJson() ?? "";
            MidiNoteMapString.Value = MidiNoteToMotionMap?.ToJson() ?? "";
        }

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
