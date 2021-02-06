namespace Baku.VMagicMirrorConfig
{
    /// <summary>
    /// SaveDataのリファクタ版。旧版のSaveDataとXML的に等価になってないといけない事に注意
    /// </summary>
    public class EntityBasedSaveData
    {
        public bool IsInternalSaveFile { get; set; } = false;

        public string? LastLoadedVrmFilePath { get; set; } = "";

        public string? LastLoadedVRoidModelId { get; set; } = "";

        public bool AutoLoadLastLoadedVrm { get; set; } = false;

        public string? PreferredLanguageName { get; set; } = "";

        public WindowSetting? WindowSetting { get; set; }

        public MotionSetting? MotionSetting { get; set; }

        public LayoutSetting? LayoutSetting { get; set; }

        public LightSetting? LightSetting { get; set; }

        public WordToMotionSetting? WordToMotionSetting { get; set; }

        public ExternalTrackerSetting? ExternalTrackerSetting { get; set; }
    }
}
