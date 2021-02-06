namespace Baku.VMagicMirrorConfig
{
    public class ExternalTrackerSetting
    { 
        public const int TrackSourceNone = 0;
        public const int TrackSourceIFacialMocap = 1;
     
        // 基本メニュー部分
        public bool EnableExternalTracking { get; set; } = false;
        public bool EnableExternalTrackerLipSync { get; set; } = true;
        public bool EnableExternalTrackerPerfectSync { get; set; } = false;
        public bool UseVRoidDefaultForPerfectSync { get; set; } = false;

        // アプリ別の設定 (※今んとこIPを一方的に表示するだけなのであんまり難しい事はないです)
        public int TrackSourceType { get; set; } = 0;
        public string IFacialMocapTargetIpAddress { get; set; } = "";
        public string CalibrateData { get; set; } = "";
        
        // FaceSwitchの設定

        //TODO: ここにデフォルト値が入りやすいような仕掛けを作るのもアリかも
        //NOTE1: この値は単体でJSONシリアライズされます(Unityにもそのまんま渡すため)
        //NOTE2: setterはアプリ起動直後、およびそれ以降で表情スイッチ系の設定を変えたときに(UIではなくコードから)呼ばれます。
        public string SerializedFaceSwitchSetting { get; set; } = "";

        public void ResetToDefault()
        {
            //TODO: SerializedFaceSwitchSettingの取り扱い
            CalibrateData = "";
            TrackSourceType = TrackSourceNone;
            EnableExternalTracking = false;
            EnableExternalTrackerLipSync = true;
            UseVRoidDefaultForPerfectSync = false;
            EnableExternalTrackerPerfectSync = false;
        }
    }
}
