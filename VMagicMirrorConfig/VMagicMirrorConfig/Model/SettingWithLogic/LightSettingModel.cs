using System;

namespace Baku.VMagicMirrorConfig
{
    /// <summary>
    /// 値を書き換えたときに良い感じに通信でき、かつロード/セーブに対応したエフェクト関連のモデル
    /// </summary>
    class LightSettingModel : SettingModelBase<LightSetting>
    {
        public LightSettingModel(IMessageSender sender) : base(sender)
        {
            var s = LightSetting.Default;
            var factory = MessageFactory.Instance;

            //モデルのプロパティ変更=Unityへの変更通知としてバインド。
            //エフェクト関係は設定項目がシンプルなため、例外はほぼ無い(色関係のメッセージ送信がちょっと特殊なくらい)

            LightIntensity = new RPropertyMin<int>(s.LightIntensity, i => SendMessage(factory.LightIntensity(i)));
            LightYaw = new RPropertyMin<int>(s.LightYaw, i => SendMessage(factory.LightYaw(i)));
            LightPitch = new RPropertyMin<int>(s.LightPitch, i => SendMessage(factory.LightPitch(i)));

            Action sendLightColor = () =>
                SendMessage(factory.LightColor(LightR?.Value ?? 255, LightG?.Value ?? 255, LightB?.Value ?? 255));
            LightR = new RPropertyMin<int>(s.LightR, _ => sendLightColor());
            LightG = new RPropertyMin<int>(s.LightG, _ => sendLightColor());
            LightB = new RPropertyMin<int>(s.LightB, _ => sendLightColor());

            EnableShadow = new RPropertyMin<bool>(s.EnableShadow, b => SendMessage(factory.ShadowEnable(b)));
            ShadowIntensity = new RPropertyMin<int>(s.ShadowIntensity, i => SendMessage(factory.ShadowIntensity(i)));
            ShadowYaw = new RPropertyMin<int>(s.ShadowYaw, i => SendMessage(factory.ShadowYaw(i)));
            ShadowPitch = new RPropertyMin<int>(s.ShadowPitch, i => SendMessage(factory.ShadowPitch(i)));
            ShadowDepthOffset = new RPropertyMin<int>(s.ShadowDepthOffset, i => SendMessage(factory.ShadowDepthOffset(i)));

            BloomIntensity = new RPropertyMin<int>(s.BloomIntensity, i => SendMessage(factory.BloomIntensity(i)));
            BloomThreshold = new RPropertyMin<int>(s.BloomThreshold, i => SendMessage(factory.BloomThreshold(i)));
            Action sendBloomColor = () =>
                SendMessage(factory.BloomColor(BloomR?.Value ?? 255, BloomG?.Value ?? 255, BloomB?.Value ?? 255));
            BloomR = new RPropertyMin<int>(s.BloomR, _ => sendBloomColor());
            BloomG = new RPropertyMin<int>(s.BloomG, _ => sendBloomColor());
            BloomB = new RPropertyMin<int>(s.BloomB, _ => sendBloomColor());

            EnableWind = new RPropertyMin<bool>(s.EnableWind, b => SendMessage(factory.WindEnable(b)));
            WindStrength = new RPropertyMin<int>(s.WindStrength, i => SendMessage(factory.WindStrength(i)));
            WindInterval = new RPropertyMin<int>(s.WindInterval, i => SendMessage(factory.WindInterval(i)));
            WindYaw = new RPropertyMin<int>(s.WindYaw, i => SendMessage(factory.WindYaw(i)));
        }

        #region Light

        public RPropertyMin<int> LightIntensity { get; }
        public RPropertyMin<int> LightYaw { get; }
        public RPropertyMin<int> LightPitch { get; }

        public RPropertyMin<int> LightR { get; }
        public RPropertyMin<int> LightG { get; }
        public RPropertyMin<int> LightB { get; }

        #endregion

        #region Shadow

        public RPropertyMin<bool> EnableShadow { get; }
        public RPropertyMin<int> ShadowIntensity { get; }
        public RPropertyMin<int> ShadowYaw { get; }
        public RPropertyMin<int> ShadowPitch { get; }
        public RPropertyMin<int> ShadowDepthOffset { get; }

        #endregion

        #region Bloom

        public RPropertyMin<int> BloomIntensity { get; } 
        public RPropertyMin<int> BloomThreshold { get; } 

        public RPropertyMin<int> BloomR { get; } 
        public RPropertyMin<int> BloomG { get; } 
        public RPropertyMin<int> BloomB { get; } 

        #endregion

        #region Wind

        public RPropertyMin<bool> EnableWind { get; } 
        public RPropertyMin<int> WindStrength { get; }
        public RPropertyMin<int> WindInterval { get; }
        public RPropertyMin<int> WindYaw { get; }

        #endregion

        #region Reset API

        public void ResetLightSetting()
        {
            var setting = LightSetting.Default;
            LightR.Value = setting.LightR;
            LightG.Value = setting.LightG;
            LightB.Value = setting.LightB;
            LightIntensity.Value = setting.LightIntensity;
            LightYaw.Value = setting.LightYaw;
            LightPitch.Value = setting.LightPitch;
        }

        public void ResetShadowSetting()
        {
            var setting = LightSetting.Default;
            EnableShadow.Value = setting.EnableShadow;
            ShadowIntensity.Value = setting.ShadowIntensity;
            ShadowYaw.Value = setting.ShadowYaw;
            ShadowPitch.Value = setting.ShadowPitch;
            ShadowDepthOffset.Value = setting.ShadowDepthOffset;
        }

        public void ResetBloomSetting()
        {
            var setting = LightSetting.Default;
            BloomR.Value = setting.BloomR;
            BloomG.Value = setting.BloomG;
            BloomB.Value = setting.BloomB;
            BloomIntensity.Value = setting.BloomIntensity;
            BloomThreshold.Value = setting.BloomThreshold;
        }

        public void ResetWindSetting()
        {
            var setting = LightSetting.Default;
            EnableWind.Value = setting.EnableWind;
            WindStrength.Value = setting.WindStrength;
            WindInterval.Value = setting.WindInterval;
            WindYaw.Value = setting.WindYaw;
        }

        public override void ResetToDefault()
        {
            ResetLightSetting();
            ResetShadowSetting();
            ResetBloomSetting();
            ResetWindSetting();
        }

        #endregion

    }
}
