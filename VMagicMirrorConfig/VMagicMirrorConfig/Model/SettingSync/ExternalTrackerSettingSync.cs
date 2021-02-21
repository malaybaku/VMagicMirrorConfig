using System;

namespace Baku.VMagicMirrorConfig
{
    class ExternalTrackerSettingSync : SettingSyncBase<ExternalTrackerSetting>
    {
        public ExternalTrackerSettingSync(IMessageSender sender) : base(sender) 
        {
            var setting = ExternalTrackerSetting.Default;
            var factory = MessageFactory.Instance;

            //NOTE: ひとまず初期値を入れておくと非null保証できて都合がいい、という話
            FaceSwitchSetting = ExternalTrackerFaceSwitchSetting.LoadDefault();

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
            //NOTE: このアドレスはコマンド実行時に使うため、書き換わってもメッセージは送らない
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

        public ExternalTrackerFaceSwitchSetting FaceSwitchSetting { get; private set; }

        //TODO: ここにデフォルト値が入りやすいような仕掛けを作るのもアリかも、という問題があるが、
        //特に値が空だったときのリカバーをモデル層でやる形にするのも選択肢。
        public RPropertyMin<string> SerializedFaceSwitchSetting { get; }

        public void SaveFaceSwitchSetting()
        {
            //文字列で保存 + 送信しつつ、手元の設定もリロードする。イベントハンドリング次第でもっとシンプルになるかも。
            SerializedFaceSwitchSetting.Value = FaceSwitchSetting.ToJson();
        }

        //NOTE: 想定挙動としてはセーブ前の時点で値が更新された時点でシリアライズされているため、
        //この再シリアライズをしたからといって普通は値は変わらない。
        //が、初期値そのままのケースとかが安全になって都合がよい
        protected override void PreSave() => SaveFaceSwitchSetting();

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

        public override void ResetToDefault()
        {
            Load(ExternalTrackerSetting.Default);
            //TODO: このLoadによってFaceSwitchSettingのロードが保証されているかな、というのが気になるので要チェック。
            //Viewの表示が更新されないかもしれないので。
        }

    }
}
