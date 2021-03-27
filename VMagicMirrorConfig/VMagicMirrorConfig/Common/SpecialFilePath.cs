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

        private const string SaveSlotFileNamePrefix = "_save";

        public static string LogFileDir { get; }
        public static string LogFilePath { get; }
        public static string UnityAppPath { get; }
        public static string AutoSaveSettingFilePath { get; }

        private static string _exeDir;

        /// <summary>
        /// スロット番号を指定して保存ファイル名を指定します。0を指定した場合は特別にオートセーブファイルのパスを返します。
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static string GetSaveFilePath(int index) => index == 0 
            ? AutoSaveSettingFilePath 
            : Path.Combine(_exeDir, SaveSlotFileNamePrefix + index.ToString());

        static SpecialFilePath()
        {
            //NOTE: 実際はnullになることはない(コーディングエラーでのみ発生する)
            string exePath = Process.GetCurrentProcess().MainModule?.FileName ?? "";
            string exeDir = Path.GetDirectoryName(exePath) ?? "";
            AutoSaveSettingFilePath = Path.Combine(exeDir, AutoSaveSettingFileName);
            _exeDir = exeDir;

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
        public static bool IsAutoSaveFileExist() => File.Exists(AutoSaveSettingFilePath);


    }
}
