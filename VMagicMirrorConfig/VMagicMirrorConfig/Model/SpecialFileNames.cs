namespace Baku.VMagicMirrorConfig
{
    public static class SpecialFileNames
    {
        //拡張子.vmmを付けるのを期待したファイルだが、付けちゃうとユーザーの誤操作で上書きする懸念があるので、つけない。
        public static readonly string AutoSaveSettingFileName = "_autosave";
    }
}
