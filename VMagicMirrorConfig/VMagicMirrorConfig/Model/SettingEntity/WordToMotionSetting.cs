using System.Collections.Generic;

namespace Baku.VMagicMirrorConfig
{
    public class WordToMotionSetting
    {
        internal const int DeviceTypeNone = -1;
        internal const int DeviceTypeKeyboardWord = 0;
        internal const int DeviceTypeGamepad = 1;
        internal const int DeviceTypeKeyboardTenKey = 2;
        internal const int DeviceTypeMidiController = 3;
    
        //TODO: ここデフォルトはKeyboardWordじゃなかったっけ？要確認。
        public int SelectedDeviceType { get; set; } = DeviceTypeNone;

        //NOTE: 「UIに出さないけど保存はしたい」系のやつで、キャラロード時にUnityから勝手に送られてくる、という想定
        public List<string> ExtraBlendShapeClipNames { get; set; } = new List<string>();

        /// <summary>
        /// 一覧要素をシリアライズした文字列
        /// </summary>
        public string ItemsContentString { get; set; } = "";

        /// <summary>
        /// MIDIのノートマッピングをシリアライズした文字列
        /// </summary>
        public string MidiNoteMapString { get; set; } = "";
    }
}
