using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Baku.VMagicMirrorConfig
{
    public class ExternalTrackerViewModel : SettingViewModelBase
    {
        private readonly ExternalTrackerBlendShapeNameStore _blendShapeNameStore
            = new ExternalTrackerBlendShapeNameStore();

        private readonly ExternalTrackerSettingModel _model;

        internal ExternalTrackerViewModel(ExternalTrackerSettingModel model, IMessageSender sender, IMessageReceiver receiver) : base(sender)
        {
            _model = model;

            //この辺はModel/VMの接続とかコマンド周りの設定
            UpdateTrackSourceType();
            model.TrackSourceType.PropertyChanged += (_, __) => UpdateTrackSourceType();
            model.EnableExternalTracking.PropertyChanged += (_, __) => UpdateShouldNotifyMissingBlendShapeClipNames();
            MissingBlendShapeNames = new RPropertyMin<string>("", _ =>
            {
                UpdateShouldNotifyMissingBlendShapeClipNames();
            });

            RefreshIFacialMocapTargetCommand = new ActionCommand(
                () => NetworkEnvironmentUtils.SendIFacialMocapDataReceiveRequest(IFacialMocapTargetIpAddress.Value)
                );
            OpenInstructionUrlCommand = new ActionCommand(OpenInstructionUrl);
            OpenPerfectSyncTipsUrlCommand = new ActionCommand(OpenPerfectSyncTipsUrl);
            OpenIFMTroubleShootCommand = new ActionCommand(OpenIFMTroubleShoot);
            EndExTrackerIfNeededCommand = new ActionCommand(EndExTrackerIfNeeded);
            ShowMissingBlendShapeNotificationCommand = new ActionCommand(ShowMissingBlendShapeNotification);
            ResetSettingsCommand = new ActionCommand(
                () => SettingResetUtils.ResetSingleCategoryAsync(ResetToDefault)
                );

            //TODO: メッセージ受信の処理もモデル側案件のはず…うーん…
            receiver.ReceivedCommand += OnMessageReceived;

            FaceSwitchItems.Clear();
            foreach (var item in _model.FaceSwitchSetting.Items)
            {
                var vm = new ExternalTrackerFaceSwitchItemViewModel(this, item);
                vm.SubscribeLanguageSelector();
                FaceSwitchItems.Add(vm);
            }
        }

        /// <summary>
        /// Face Switchの設定が更新されたときにViewModelに情報を反映します。
        /// 設定ファイルをロードしたときや、設定をリセットしたときに呼び出される想定です。
        /// </summary>
        internal void LoadFaceSwitchSetting()
        {
            //NOTE: 先に名前を更新することで「ComboBoxに無い値をSelectedValueにしちゃう」みたいな不整合を防ぐのが狙い
            _blendShapeNameStore.Refresh(_model.FaceSwitchSetting);

            foreach (var item in FaceSwitchItems)
            {
                item.UnsubscribeLanguageSelector();
            }
            FaceSwitchItems.Clear();

            foreach (var item in _model.FaceSwitchSetting.Items)
            {
                var vm = new ExternalTrackerFaceSwitchItemViewModel(this, item);
                vm.SubscribeLanguageSelector();
                FaceSwitchItems.Add(vm);
            }
        }

        private void OnMessageReceived(object? sender, CommandReceivedEventArgs e)
        {
            if (e.Command == ReceiveMessageNames.ExtraBlendShapeClipNames)
            {
                try
                {
                    //いちおう信頼はするけどIPCだし…みたいな書き方です
                    var names = e.Args
                        .Split(',')
                        .Where(w => !string.IsNullOrEmpty(w))
                        .ToArray();
                    _blendShapeNameStore.Refresh(names);
                }
                catch (Exception ex)
                {
                    LogOutput.Instance.Write(ex);
                }
            }
            else if (e.Command == ReceiveMessageNames.ExTrackerCalibrateComplete)
            {
                //キャリブレーション結果を向こうから受け取る: この場合は、ただ覚えてるだけでよい
                _model.CalibrateData.SilentSet(e.Args);
            }
            else if (e.Command == ReceiveMessageNames.ExTrackerSetPerfectSyncMissedClipNames)
            {
                MissingBlendShapeNames.Value = e.Args;
            }
            else if (e.Command == ReceiveMessageNames.ExTrackerSetIFacialMocapTroubleMessage)
            {
                IFacialMocapTroubleMessage.Value = e.Args;
            }
        }

        #region 基本メニュー部分

        public RPropertyMin<bool> EnableExternalTracking => _model.EnableExternalTracking;
        public RPropertyMin<bool> EnableExternalTrackerLipSync => _model.EnableExternalTrackerLipSync;
        public RPropertyMin<bool> EnableExternalTrackerPerfectSync => _model.EnableExternalTrackerPerfectSync;
        public RPropertyMin<bool> UseVRoidDefaultForPerfectSync => _model.UseVRoidDefaultForPerfectSync;

        public ActionCommand OpenPerfectSyncTipsUrlCommand { get; }

        private void OpenPerfectSyncTipsUrl()
        {
            string url =
                (LanguageSelector.Instance.LanguageName == "Japanese") ?
                "https://malaybaku.github.io/VMagicMirror/tips/perfect_sync" :
                "https://malaybaku.github.io/VMagicMirror/en/tips/perfect_sync";
            UrlNavigate.Open(url);
        }

        public RPropertyMin<bool> ShouldNotifyMissingBlendShapeClipNames { get; } = new RPropertyMin<bool>(false);

        public RPropertyMin<string> MissingBlendShapeNames { get; }

        private void UpdateShouldNotifyMissingBlendShapeClipNames()
        {
            ShouldNotifyMissingBlendShapeClipNames.Value =
                EnableExternalTracking.Value &&
                !string.IsNullOrEmpty(MissingBlendShapeNames.Value);
        }

        public ActionCommand ShowMissingBlendShapeNotificationCommand { get; }
        private async void ShowMissingBlendShapeNotification()
        {
            var indication = MessageIndication.ExTrackerMissingBlendShapeNames(LanguageSelector.Instance.LanguageName);
            var lines = MissingBlendShapeNames.Value.Split('\n').ToList();
            if (lines.Count > 8)
            {
                //未定義ブレンドシェイプがあまりに多いとき、後ろを"…"で切る
                lines = lines.Take(8).ToList();
                lines.Add("…");
            }

            await MessageBoxWrapper.Instance.ShowAsync(
                indication.Title,
                indication.Content + string.Join("\n", lines),
                MessageBoxWrapper.MessageBoxStyle.OK
                );
        }

        private ActionCommand? _calibrateCommand = null;
        public ActionCommand CalibrateCommand
            => _calibrateCommand ??= new ActionCommand(Calibrate);

        private void Calibrate()
            => SendMessage(MessageFactory.Instance.ExTrackerCalibrate());

        public ActionCommand ResetSettingsCommand { get; }

        #endregion

        #region アプリ別のやつ(※今んとこIPを一方的に表示するだけなのであんまり難しい事はないです)

        private bool _isTrackSourceNone;
        public bool IsTrackSourceNone
        {
            get => _isTrackSourceNone;
            set
            {
                if (SetValue(ref _isTrackSourceNone, value) && value)
                {
                    _model.TrackSourceType.Value = ExternalTrackerSetting.TrackSourceNone;
                }
            }
        }

        private bool _isTrackSourceIFacialMocap;
        public bool IsTrackSourceIFacialMocap
        {
            get => _isTrackSourceIFacialMocap;
            set
            {
                if (SetValue(ref _isTrackSourceIFacialMocap, value) && value)
                {
                    _model.TrackSourceType.Value = ExternalTrackerSetting.TrackSourceIFacialMocap;
                }
            }
        }

        private void UpdateTrackSourceType()
        {
            IsTrackSourceNone = _model.TrackSourceType.Value == ExternalTrackerSetting.TrackSourceNone;
            IsTrackSourceIFacialMocap = _model.TrackSourceType.Value == ExternalTrackerSetting.TrackSourceIFacialMocap;
        }

        //NOTE: 上記のbool2つ+UpdateTrackSourceTypeを廃止し、この整数値を読み込んだViewがConverterで頑張るのでもよい。はず
        public RPropertyMin<int> TrackSourceType => _model.TrackSourceType;

        public RPropertyMin<string> IFacialMocapTargetIpAddress => _model.IFacialMocapTargetIpAddress;

        public ActionCommand RefreshIFacialMocapTargetCommand { get; }

        public ActionCommand OpenInstructionUrlCommand { get; }

        private void OpenInstructionUrl()
        {
            string url =
                (LanguageSelector.Instance.LanguageName == "Japanese") ?
                "https://malaybaku.github.io/VMagicMirror/docs/external_tracker" :
                "https://malaybaku.github.io/VMagicMirror/en/docs/external_tracker";
            UrlNavigate.Open(url);
        }

        public RPropertyMin<string> CalibrateData => _model.CalibrateData;

        #endregion

        #region 表情スイッチのやつ

        //NOTE: setterはアプリ起動直後、およびそれ以降で表情スイッチ系の設定を変えたときに(UIではなくコードから)呼ばれます。
        public RPropertyMin<string> SerializedFaceSwitchSetting => _model.SerializedFaceSwitchSetting;

        /// <summary>
        /// 子要素になってる<see cref="ExternalTrackerFaceSwitchItemViewModel"/>から呼び出すことで、
        /// 現在の設定を保存した状態にします。
        /// </summary>
        public void SaveFaceSwitchSetting()
        {
            //文字列で保存 + 送信しつつ、手元の設定もリロードする。イベントハンドリング次第でもっとシンプルになるかも。
            _model.SerializedFaceSwitchSetting.Value = _model.FaceSwitchSetting?.ToJson() ?? "";
            //TODO?: モデルの挙動次第でここ不要かも
            LoadFaceSwitchSetting();
        }

        /// <summary> UIで個別設定として表示する、表情スイッチの要素です。 </summary>
        public ObservableCollection<ExternalTrackerFaceSwitchItemViewModel> FaceSwitchItems { get; }
            = new ObservableCollection<ExternalTrackerFaceSwitchItemViewModel>();

        /// <summary> Face Switch機能で表示可能なブレンドシェイプ名の一覧です。 </summary>
        public ReadOnlyObservableCollection<string> BlendShapeNames => _blendShapeNameStore.BlendShapeNames;

        #endregion

        #region エラーまわり: iFMの設定が怪しそうなときのメッセージ + webカメラが止まる問題の対処

        public RPropertyMin<string> IFacialMocapTroubleMessage { get; } = new RPropertyMin<string>("");

        public ActionCommand OpenIFMTroubleShootCommand  { get; }
        
        private void OpenIFMTroubleShoot()
        {
            var url = LanguageSelector.StringToLanguage(LanguageSelector.Instance.LanguageName) switch
            {
                Languages.Japanese => "https://malaybaku.github.io/VMagicMirror/docs/external_tracker_ifacialmocap#troubleshoot",
                _ => "https://malaybaku.github.io/VMagicMirror/en/docs/external_tracker_ifacialmocap#troubleshoot",
            };
            UrlNavigate.Open(url);
        }

        public ActionCommand EndExTrackerIfNeededCommand { get; }

        private async void EndExTrackerIfNeeded()
        {
            //NOTE: これもモデル層…いやメッセージボックス相当だからVMでいいのかな…？
            var indication = MessageIndication.ExTrackerCheckTurnOff(LanguageSelector.Instance.LanguageName);
            bool result = await MessageBoxWrapper.Instance.ShowAsync(
                indication.Title,
                indication.Content,
                MessageBoxWrapper.MessageBoxStyle.OKCancel
                );

            if (result)
            {
                EnableExternalTracking.Value = false;
            }
        }

        #endregion

        public override void ResetToDefault() 
        {
            _model.ResetToDefault();
            //TODO?: モデルの挙動次第でここ不要かも
            LoadFaceSwitchSetting();
        }
    }
}
