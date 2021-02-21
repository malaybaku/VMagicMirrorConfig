using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Baku.VMagicMirrorConfig
{
    /// <summary>
    /// Word to Motionのアイテムのインデックスと、MIDIのノート番号との対応を取るためのアイテム
    /// Unity側でJsonUtilityで読めるようなフォーマットにしてます
    /// </summary>
    public class MidiNoteToMotionMap
    {
        internal static readonly int InvalidNoteNumber = -1;

        public List<MidiNoteToMotionItem> Items { get; set; } 
            = new List<MidiNoteToMotionItem>();

        public string ToJson()
        {
            var serializer = new JsonSerializer();
            var sb = new StringBuilder();
            using (var writer = new StringWriter(sb))
            {
                serializer.Serialize(writer, this);
            }
            return sb.ToString();
        }

        public MidiNoteToMotionMap CreateCopy()
        {
            var result = new MidiNoteToMotionMap();
            foreach (var i in Items)
            {
                result.Items.Add(new MidiNoteToMotionItem()
                {
                    ItemIndex = i.ItemIndex,
                    NoteNumber = i.NoteNumber,
                });
            }
            return result;
        }

        public static MidiNoteToMotionMap DeserializeFromJson(TextReader reader)
        {
            var serializer = new JsonSerializer();
            using (var jsonReader = new JsonTextReader(reader))
            {
                return
                    serializer.Deserialize<MidiNoteToMotionMap>(jsonReader) ??
                    LoadDefault();
            }
        }

        public static MidiNoteToMotionMap LoadDefault()
        {
            var result = new MidiNoteToMotionMap();
            for (int i = 0; i < 9; i++)
            {
                result.Items.Add(new MidiNoteToMotionItem()
                {
                    ItemIndex = i,
                    NoteNumber = InvalidNoteNumber,
                });
            }
            return result;
        }
    }

    public class MidiNoteToMotionItem
    {
        public int ItemIndex { get; set; }
        public int NoteNumber { get; set; } = MidiNoteToMotionMap.InvalidNoteNumber;
    }
}
