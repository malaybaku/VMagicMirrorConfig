using System;

namespace Baku.VMagicMirrorConfig
{
    class ExternalTrackerSettingModel : SettingModelBase<ExternalTrackerSetting>
    {
        public ExternalTrackerSettingModel(IMessageSender sender) : base(sender) 
        {
            var setting = ExternalTrackerSetting.Default;
            var factory = MessageFactory.Instance;

            EnableExternalTracking = new RPropertyMin<bool>(
                setting.EnableExternalTracking, b => SendMessage(factory.ExTrackerEnable(b))
                );
            EnableExternalTrackerLipSync = new RPropertyMin<bool>(
                setting.EnableExternalTrackerLipSync, b => SendMessage(factory.ExTrackerEnableLipSync(b))
                );
            EnableExternalTrackerPerfectSync = new RPropertyMin<bool>(
                setting.EnableExternalTrackerPerfectSync, b => SendMessage(factory.ExTrackerEnablePerfectSync(b))
                );

            UseVRoidDefaultForPerfectSync = new RPropertyMin<bool>(
                setting.UseVRoidDefaultForPerfectSync, b => SendMessage(factory.ExTrackerUseVRoidDefaultForPerfectSync(b))
                );

            TrackSourceType = new RPropertyMin<int>(setting.TrackSourceType, i => SendMessage(factory.ExTrackerSetSource(i)));
            //NOTE: このアドレスはコマンドベースで使うため、このフェーズではSendMessage不要
            IFacialMocapTargetIpAddress = new RPropertyMin<string>(setting.IFacialMocapTargetIpAddress);
            
            CalibrateData = new RPropertyMin<string>(
                setting.CalibrateData, s => SendMessage(factory.ExTrackerSetCalibrateData(s))
                );

            SerializedFaceSwitchSetting = new RPropertyMin<string>(
                setting.SerializedFaceSwitchSetting, v => SendMessage(factory.ExTrackerSetFaceSwitchSetting(v))
                );
        }

        // 基本メニュー部分
        public RPropertyMin<bool> EnableExternalTracking { get; }
        public RPropertyMin<bool> EnableExternalTrackerLipSync { get; }
        public RPropertyMin<bool> EnableExternalTrackerPerfectSync { get; }
        public RPropertyMin<bool> UseVRoidDefaultForPerfectSync { get; }

        // アプリ別設定
        public RPropertyMin<int> TrackSourceType { get; }
        public RPropertyMin<string> IFacialMocapTargetIpAddress { get; }
        public RPropertyMin<string> CalibrateData { get; }

        // FaceSwitchの設定

        //TODO: ここは非null保証すべきで、SerializedFaceSwitchSettingが無効な場合はデフォルト値を当てるようにしていい、と思う。
        public ExternalTrackerFaceSwitchSetting? FaceSwitchSetting { get; private set; }

        //TODO: ここにデフォルト値が入りやすいような仕掛けを作るのもアリかも、という問題があるが、
        //特に値が空だったときのリカバーをモデル層でやる形にするのも選択肢。
        public RPropertyMin<string> SerializedFaceSwitchSetting { get; }

        protected override void PreSave()
        {
            //NOTE: 想定挙動としてはセーブ前の時点で値が更新された時点でシリアライズされているため、この再シリアライズをしたからといって値は変わらない
            SerializedFaceSwitchSetting.Value = FaceSwitchSetting?.ToJson() ?? "";
        }

        protected override void AfterLoad(ExternalTrackerSetting entity)
        {
            try
            {
                FaceSwitchSetting = 
                    ExternalTrackerFaceSwitchSetting.FromJson(SerializedFaceSwitchSetting.Value, enableThrow: true);
            }
            catch (Exception ex)
            {
                LogOutput.Instance.Write(ex);
                FaceSwitchSetting = ExternalTrackerFaceSwitchSetting.LoadDefault();
            }
        }

        public void ResetToDefault()
        {
            Load(ExternalTrackerSetting.Default);
        }

    }
}
