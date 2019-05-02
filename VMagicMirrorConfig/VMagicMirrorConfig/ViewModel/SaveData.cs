using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Baku.VMagicMirrorConfig
{
    /// <summary>シリアライザで保存する設定ファイル</summary>
    /// <remarks>
    /// NOTE: 本来はモデルをセーブするが、本アプリは(小さいので)VMとMが癒着しちゃっている。
    /// そこで、SaveDataも基本的にVMをそのまんまコピーする方針を取る。
    /// </remarks>
    public class SaveData
    {
        public bool IsInternalSaveFile { get; set; } = false;

        public string LastLoadedVrmFilePath { get; set; } = "";

        public bool AutoLoadLastLoadedVrm { get; set; } = false;

        public string PreferredLanguageName { get; set; } = "";

        public WindowSettingViewModel WindowSetting { get; set; }

        public MotionSettingViewModel MotionSetting { get; set; }

        public LayoutSettingViewModel LayoutSetting { get; set; }

        public LightSettingViewModel LightSetting { get; set; }
    }
}
