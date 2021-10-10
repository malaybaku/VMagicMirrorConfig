namespace Baku.VMagicMirrorConfig.Model
{
    public static class TargetEnvironmentChecker
    {
        public static bool CheckIsDebugEnv()
        {
#if DEV_ENV
            //DEV_ENV フラグは、dev系のpublish profileでビルドすると定義される
            return true;
#endif
            //Unityからパイプ情報が渡されてない = Unity側がエディタ実行であると考えられる時、デバッグ実行と判断できる
            return !CommandLineArgParser.TryLoadMmfFileName(out _);
        }
    }
}
