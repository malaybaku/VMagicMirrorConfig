using Baku.VMagicMirrorConfig.Model;

namespace Baku.VMagicMirrorConfig
{
    //NOTE: このクラスで、メインウィンドウやライセンスに表示する名称を管理します。
    public static class AppLabels
    {
        public static string AppName => "VMagicMirror v1.8.2";
        public static string EditionName => FeatureLocker.FeatureLocked ? "Standard Edition" : "Full Edition";
        public static string AppFullName => AppName + " " + EditionName;
        public static string AppFullNameWithEnvSuffix => 
            AppFullName + (TargetEnvironmentChecker.CheckIsDebugEnv() ? "(Dev)" : "");
    }
}
