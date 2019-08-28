using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace Baku.VMagicMirrorConfig
{
    //NOTE: Unity側のMotionRequestとプロパティ名を統一してます。片方だけいじらないように！

    /// <summary>単一のBVHまたはビルトインモーション、および表情制御のリクエスト情報を表す。</summary>
    public class MotionRequest
    {
        public const int MotionTypeNone = 0;
        public const int MotionTypeBuiltInClip = 1;
        public const int MotionTypeBvhFile = 2;

        public int MotionType { get; set; }

        public string Word { get; set; } = "";

        public string BuiltInAnimationClipName { get; set; } = "";

        public string ExternalBvhFilePath { get; set; } = "";

        public float DurationWhenOnlyBlendShape { get; set; } = 3.0f;

        //TODO: ここにHoldBlendShapeとかHoldPoseとかを追加するかも

        /// <summary>
        /// NOTE: ブレンドシェイプは「1個も適用しない」か「全部適用する」のいずれかになる点に留意
        /// </summary>
        public bool UseBlendShape { get; set; }

        /// <summary>
        /// NOTE: シリアライズ都合でpublic propertyになってます
        /// </summary>
        public Dictionary<string, int> BlendShapeValues { get; set; } = new Dictionary<string, int>();

        public void CopyFrom(MotionRequest source)
        {
            MotionType = source.MotionType;
            Word = source.Word;
            BuiltInAnimationClipName = source.BuiltInAnimationClipName;
            ExternalBvhFilePath = source.ExternalBvhFilePath;
            DurationWhenOnlyBlendShape = source.DurationWhenOnlyBlendShape;
            UseBlendShape = source.UseBlendShape;

            foreach(var p in source.BlendShapeValues)
            {
                BlendShapeValues[p.Key] = p.Value;
            }
        }

        /// <summary>この要素をJSONにシリアライズしたものを取得します。</summary>
        /// <returns></returns>
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

        /// <summary>デフォルトの簡単な設定からなる動作リクエストを生成します。</summary>
        /// <returns></returns>
        public static MotionRequest GetDefault()
        {
            var result = new MotionRequest()
            {
                MotionType = MotionTypeNone,
                Word = "name",
                UseBlendShape = true,
                DurationWhenOnlyBlendShape = 3.0f,
            };
            result.BlendShapeValues["Joy"] = 100;

            return result;
        }
    }

    /// <summary>
    /// NOTE: コレクションクラスを作ってるのはJSONのルートをオブジェクトにするため
    /// </summary>
    public class MotionRequestCollection
    {
        public MotionRequestCollection(MotionRequest[] requests)
        {
            Requests = requests;
        }

        public MotionRequest[] Requests { get; }

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

        public static MotionRequestCollection DeserializeFromJson(string filePath)
        {
            using(var sr = new StreamReader(filePath))
            {
                return DeserializeFromJson(sr);
            }
        }

        public static MotionRequestCollection DeserializeFromJson(TextReader reader)
        {
            var serializer = new JsonSerializer();
            using (var jsonReader = new JsonTextReader(reader))
            {
                return serializer.Deserialize<MotionRequestCollection>(jsonReader);
            }
        }
    }
}
