using System.Diagnostics;
using System.IO;

namespace Baku.VMagicMirrorConfig
{
    public static class SpecialFilePath
    {
        //拡張子.vmmを付けるのを期待したファイルだが、付けちゃうとユーザーの誤操作で上書きする懸念があるので、つけない。
        public const string AutoSaveSettingFileName = "_autosave";
        private const string LogTextName = "log_config.txt";
        private const string UnityAppFileName = "VMagicMirror.exe";

        public static string LogFileDir { get; }
        public static string LogFilePath { get; }
        public static string UnityAppPath { get; }
        public static string AutoSaveSettingFilePath { get; }

        static SpecialFilePath()
        {
            //NOTE: 実際はnullになることはない(コーディングエラーでのみ発生する)
            string exePath = Process.GetCurrentProcess().MainModule?.FileName ?? "";
            string exeDir = Path.GetDirectoryName(exePath) ?? "";
            AutoSaveSettingFilePath = Path.Combine(exeDir, AutoSaveSettingFileName);

            string unityAppDir = Path.GetDirectoryName(exeDir) ?? "";
            UnityAppPath = Path.Combine(unityAppDir, UnityAppFileName);
            LogFileDir = unityAppDir;
            LogFilePath = File.Exists(UnityAppPath) ?
                Path.Combine(unityAppDir, LogTextName) :
                "";
        }

        /// <summary>
        /// <see cref="GetSettingFilePath"/>のパスに設定ファイルがあるかどうかを確認します。
        /// </summary>
        /// <returns></returns>
        public static bool SettingFileExists() => File.Exists(AutoSaveSettingFilePath);


    }
}
