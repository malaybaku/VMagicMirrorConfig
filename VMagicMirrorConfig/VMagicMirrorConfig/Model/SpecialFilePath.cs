using System.IO;
using System.Reflection;

namespace Baku.VMagicMirrorConfig
{
    public static class SpecialFilePath
    {
        //拡張子.vmmを付けるのを期待したファイルだが、付けちゃうとユーザーの誤操作で上書きする懸念があるので、つけない。
        public static readonly string AutoSaveSettingFileName = "_autosave";

        //実行中のVMagicMirrorの設定が保存された設定ファイルのパスを取得します。
        public static string GetSettingFilePath()
            => Path.Combine(
            Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
            AutoSaveSettingFileName
            );

        /// <summary>
        /// <see cref="GetSettingFilePath"/>のパスに設定ファイルがあるかどうかを確認します。
        /// </summary>
        /// <returns></returns>
        public static bool SettingFileExists() => File.Exists(GetSettingFilePath());
    }
}
