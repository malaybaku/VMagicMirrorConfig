namespace Baku.VMagicMirrorConfig
{
    /// <summary>
    /// カメラの解像度設定
    /// 設定ファイルやUnityとのやりとりでは整数ID(0, 1, 2, ...)を使い、表示名は勝手に設定したいのでクラス化している
    /// </summary>
    public class CameraResolutionSetting
    {
        private CameraResolutionSetting(int id, string displayName)
        {
            Id = id;
            DisplayName = displayName;
        }

        /// <summary>
        /// UnityとWPFであらかじめ認知しておく、解像度をあらわすID
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// WPF側で表示するときの解像度表記で、意味としては解像度のリクエスト値
        /// (あくまでリクエストなので、この値になるとは限らない)
        /// </summary>
        public string DisplayName { get; }


        public static CameraResolutionSetting LoadSetting(ResolutionTypes resolution)
        {
            switch(resolution)
            {
                case ResolutionTypes.Excellent:
                    return new CameraResolutionSetting(3, "1280 x 960");
                case ResolutionTypes.High:
                    return new CameraResolutionSetting(2, "960 x 720");
                case ResolutionTypes.Mid:
                    return new CameraResolutionSetting(1, "640 x 480");
                case ResolutionTypes.Low:
                default:
                    //不明な場合は防御的に最低解像度ということにします
                    return new CameraResolutionSetting(0, "320 x 240 (Default)");
            }
        }

        public enum ResolutionTypes
        {
            Low,
            Mid,
            High,
            Excellent,
        }
    }
}
