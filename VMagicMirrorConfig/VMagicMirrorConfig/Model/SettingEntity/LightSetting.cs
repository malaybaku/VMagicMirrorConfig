namespace Baku.VMagicMirrorConfig
{
    /// <summary>XMLシリアライズを想定した、エフェクト系設定のエンティティ。</summary>
    /// <remarks>UI上はEffectと表示される項目なんですが、歴史的経緯でLightという名称を使っています。</remarks>
    public class LightSetting
    {
        #region Light

        public int LightIntensity { get; set; } = 100;
        public int LightYaw { get; set; } = 30;
        public int LightPitch { get; set; } = 50;

        public int LightR { get; set; } = 255;
        public int LightG { get; set; } = 255;
        public int LightB { get; set; } = 255;

        #endregion

        #region Shadow

        public bool EnableShadow { get; set; } = true;
        public int ShadowIntensity { get; set; } = 65;
        public int ShadowYaw { get; set; } = -20;
        public int ShadowPitch { get; set; } = 8;
        public int ShadowDepthOffset { get; set; } = 40;

        #endregion

        #region Bloom

        public int BloomIntensity { get; set; } = 50;
        public int BloomThreshold { get; set; } = 100;

        public int BloomR { get; set; } = 255;
        public int BloomG { get; set; } = 255;
        public int BloomB { get; set; } = 255;

        #endregion

        #region Wind

        public bool EnableWind { get; set; } = true;
        public int WindStrength { get; set; } = 100;
        public int WindInterval { get; set; } = 100;
        public int WindYaw { get; set; } = 90;

        #endregion

        #region Reset API

        public void ResetLightSetting()
        {
            LightR = 255;
            LightG = 255;
            LightB = 255;
            LightIntensity = 100;
            LightYaw = -30;
            LightPitch = 50;
        }

        public void ResetShadowSetting()
        {
            EnableShadow = true;
            ShadowIntensity = 65;
            ShadowYaw = -20;
            ShadowPitch = 8;
            ShadowDepthOffset = 40;
        }

        public void ResetBloomSetting()
        {
            BloomR = 255;
            BloomG = 255;
            BloomB = 255;
            BloomIntensity = 50;
            BloomThreshold = 100;
        }

        public void ResetWindSetting()
        {
            EnableWind = true;
            WindStrength = 100;
            WindInterval = 100;
            WindYaw = 90;
        }

        public void ResetToDefault()
        {
            ResetLightSetting();
            ResetShadowSetting();
            ResetBloomSetting();
            ResetWindSetting();
        }

        #endregion
    }
}
