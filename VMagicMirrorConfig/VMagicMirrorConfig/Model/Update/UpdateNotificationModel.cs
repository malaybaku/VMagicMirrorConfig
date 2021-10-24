using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Baku.VMagicMirrorConfig
{
    public record VmmAppVersion(int Major, int Minor, int Build)
    {
        //無効なアプリバージョンを取得します。
        public static VmmAppVersion LoadInvalid() => new VmmAppVersion(0, 0, 0);

        //NOTE: VMagicMirrorの既存バージョンではhotfixを行ったとき"v1.2.3a"のようにsuffixを使っていたが、
        //suffixは扱いが面倒なのでこの際廃止する

        /// <summary>
        /// "v1.2.3"のような文字列を受け取ってバージョン値に変換します。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        public static bool TryParse(string? value, out VmmAppVersion result)
        {
            if (string.IsNullOrEmpty(value))
            {
                result = LoadInvalid();
                return false;
            }

            //先頭の"v"はあってもなくてもOK
            value = value.TrimStart('v');
            if (string.IsNullOrEmpty(value))
            {
                result = LoadInvalid();
                return false;
            }

            var numbers = value.Split('.');

            int major = 0;
            int minor = 0;
            int build = 0;
            
            if (numbers.Length > 0)
            {
                int.TryParse(numbers[0], out major);
            }

            if (numbers.Length > 1)
            {
                int.TryParse(numbers[1], out minor);
            }

            if (numbers.Length > 2)
            {
                int.TryParse(numbers[2], out build);
            }

            result = new VmmAppVersion(major, minor, build);
            return true;
        }

        public bool IsNewerThan(VmmAppVersion other)
        {
            if (Major > other.Major)
            {
                return true;
            }
            else if (Major < other.Major)
            {
                return false;
            }

            if (Minor > other.Minor)
            {
                return true;
            }
            else if (Minor < other.Minor)
            {
                return false;
            }

            if (Build > other.Build)
            {
                return true;
            }

            return false;
        }
    }

    public record UpdateCheckResult(bool UpdateNeeded, VmmAppVersion Version, ReleaseNote ReleaseNote)
    {
        public static UpdateCheckResult NoUpdateNeeded() 
            => new UpdateCheckResult(false, VmmAppVersion.LoadInvalid(), ReleaseNote.Empty);
    }

    public record ReleaseNote(string JapaneseNote, string EnglishNote)
    {
        /// <summary>
        /// 日英のリリースノートが混在しているはずのテキストをパースし、
        /// 日本語・英語に分割したリリースノートを生成します。
        /// </summary>
        /// <param name="rawValue"></param>
        /// <returns></returns>
        /// <remarks>
        /// 分割に失敗した場合は情報の欠落を防ぐため、元のテキスト全体をJapanese, Englishの双方に適用します。
        /// </remarks>
        public static ReleaseNote FromRawString(string? rawValue)
        {
            if (string.IsNullOrEmpty(rawValue))
            {
                return Empty;
            }



        }


        public static ReleaseNote Empty => new ReleaseNote("", "");
    }


    public class UpdateNotificationModel
    {
        private readonly WebClient _webClient = new WebClient();

        public async Task<UpdateCheckResult> CheckUpdateAvailable()
        {
            try
            {
                var latestReleaseJson = await _webClient.DownloadStringTaskAsync("https://api.github.com/repos/malaybaku/VMagicMirror/releases/latest");
                var jobj = JObject.Parse(latestReleaseJson);

                var releaseName = jobj["name"] is JValue jvName ? ((string?)jvName) : null;
                if (!VmmAppVersion.TryParse(releaseName, out var version))
                {
                    return UpdateCheckResult.NoUpdateNeeded();
                }

                var rawReleaseNote = jobj["body"] is JValue jvBody ? ((string?)jvBody) : null;
                var releaseNote = ReleaseNote.FromRawString(rawReleaseNote);


                return new UpdateCheckResult(
                    version.IsNewerThan(                    AppConsts.AppVersion),
                    version,


                    
                    )
            }
            catch (Exception ex)
            {
                LogOutput.Instance.Write(ex);
            }

        }
    }
}
