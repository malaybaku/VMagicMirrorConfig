using System.Collections.Generic;

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
        }

        //TODO: ここデフォルトはKeyboardWordじゃなかったっけ？要確認。
        public RPropertyMin<int> SelectedDeviceType { get; }

        //NOTE: 「UIに出さないけど保存はしたい」系のやつで、キャラロード時にUnityから勝手に送られてくる
        public List<string> ExtraBlendShapeClipNames { get; set; } = new List<string>();

        public RPropertyMin<string> ItemsContentString { get; }

        public RPropertyMin<string> MidiNoteMapString { get; }

        public MotionRequestCollection? MotionRequests { get; private set; }


        protected override void PreSave()
        {
            base.PreSave();

            ItemsContentString.SilentSet()
            
        }

    }
}
