﻿using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.RegularExpressions;

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

        //NOTE: 「1.-2.3みたいなのは良いのか」みたいな話もあるけど、そんな事しないでしょ…という事で良識に期待します。
        public bool IsValid => Major > 0 || Minor > 0 || Build > 0;
        
        public override string ToString() => $"v{Major}.{Minor}.{Build}";
    }

    public record UpdateCheckResult(bool UpdateNeeded, VmmAppVersion Version, ReleaseNote ReleaseNote)
    {
        public static UpdateCheckResult NoUpdateNeeded() 
            => new UpdateCheckResult(false, VmmAppVersion.LoadInvalid(), ReleaseNote.Empty);
    }

    public record ReleaseNote(string JapaneseNote, string EnglishNote, string DateString)
    {
        /// <summary>
        /// 日英のリリースノートが混在しているはずのテキストをパースし、
        /// 日本語・英語に分割したリリースノートを生成します。
        /// </summary>
        /// <param name="rawValue"></param>
        /// <returns></returns>
        /// <remarks>
        /// 分割に失敗した場合は情報の欠落を防ぐため、元のテキスト全体をJapanese, Englishの双方に適用しつつ、リリース日は不明とします。
        /// </remarks>
        public static ReleaseNote FromRawString(string? rawValue)
        {
            if (string.IsNullOrEmpty(rawValue))
            {
                return Empty;
            }

            var lines = rawValue.Replace("\r\n", "\n").Split('\n');
            //冒頭行がリリース年月日のみからなる("1234/5/6"など)ことを期待したパース処理。
            //アメリカの月日年形式には対応したくないので、単に年月日が入ったものを横流しする。
            var dateString = (lines.Length > 0 && Regex.IsMatch(lines[0], "[0-9]+/[0-9]+/[0-9]+"))
                ? lines[0]
                : "";

            var japaneseNote = string.Join(
                "\n",
                lines.SkipWhile(line => !line.StartsWith("Japanese:"))
                    .Skip(1)
                    .TakeWhile(line => !line.StartsWith("English:"))
                    .Where(line => !string.IsNullOrEmpty(line))
                );

            var englishNote = string.Join(
                "\n",
                lines.SkipWhile(line => !line.StartsWith("English:"))
                    .Skip(1)
                    .TakeWhile(line => !line.StartsWith("Note:"))
                    .Where(line => !string.IsNullOrEmpty(line))
                );

            if (string.IsNullOrEmpty(japaneseNote) || string.IsNullOrEmpty(englishNote))
            {
                return new ReleaseNote(rawValue, rawValue, "");
            }
            else
            {
                return new ReleaseNote(japaneseNote, englishNote, dateString);
            }
        }


        public static ReleaseNote Empty => new ReleaseNote("", "", "");
    }


    public class UpdateNotificationModel
    {
        const string LatestReleaseApiEndPoint = "https://api.github.com/repos/malaybaku/VMagicMirror/releases/latest";
        const double ApiTimeout = 3.0;

        //NOTE: 確かコレ系のClientは使い回すと不幸になるので、単一インスタンスにしておく
        private static readonly HttpClient _httpClient = new HttpClient();
      
        static UpdateNotificationModel()
        {
            _httpClient.Timeout = TimeSpan.FromSeconds(ApiTimeout);
            //GitHub君がUser-Agentを欲しているのでEdgeであるということにします
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Microsoft Edge");
        }

        public async Task<UpdateCheckResult> CheckUpdateAvailable()
        {
            try
            {                
                var responseJson = await _httpClient.GetStringAsync(LatestReleaseApiEndPoint);
                //todo: 量が多いので消したい
                //LogOutput.Instance.Write("latest ver api result:" + responseJson);

                var jobj = JObject.Parse(responseJson);
                var releaseName = jobj["name"] is JValue jvName ? ((string?)jvName) : null;
                if (!VmmAppVersion.TryParse(releaseName, out var version))
                {
                    return UpdateCheckResult.NoUpdateNeeded();
                }

                var rawReleaseNote = jobj["body"] is JValue jvBody ? ((string?)jvBody) : null;
                var releaseNote = ReleaseNote.FromRawString(rawReleaseNote);

                return new UpdateCheckResult(
                    version.IsNewerThan(AppConsts.AppVersion),
                    version,
                    releaseNote
                    );
            }
            catch (Exception ex)
            {
                LogOutput.Instance.Write(ex);
                return UpdateCheckResult.NoUpdateNeeded();
            }
        }
    }
}
