using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Serialization;

namespace Baku.VMagicMirrorConfig
{
    public class ExternalTrackerViewModel : SettingViewModelBase
    {
        private const int TrackSourceNone = 0;
        private const int TrackSourceIFacialMocap = 1;

        private ExternalTrackerFaceSwitchSetting _settingModel
            = ExternalTrackerFaceSwitchSetting.LoadDefault();
        private readonly ExternalTrackerBlendShapeNameStore _blendShapeNameStore
            = new ExternalTrackerBlendShapeNameStore();

        public ExternalTrackerViewModel() : base() 
        {
        }

        internal ExternalTrackerViewModel(IMessageSender sender, IMessageReceiver receiver) : base(sender)
        {
            RefreshReceiverSetting();
            receiver.ReceivedCommand += OnMessageReceived;

            //DEBUG: ひとまずガワを見てみたい。
            _settingModel = ExternalTrackerFaceSwitchSetting.LoadDefault();
            FaceSwitchItems.Clear();
            foreach (var item in _settingModel.Items)
            {
                FaceSwitchItems.Add(new ExternalTrackerFaceSwitchItemViewModel(this, item));
            }
        }

        /// <summary>
        /// 顔スイッチングの設定について、シリアライズされた文字列からUIに表示するデータを復元します。
        /// 設定ファイルをロードしたときや、設定をリセットしたときに呼び出します。
        /// </summary>
        internal void LoadFaceSwitchSettingFromString()
        {
            _settingModel = ExternalTrackerFaceSwitchSetting.FromJson(SerializedFaceSwitchSetting);
            //NOTE: 先に名前を更新することで「ComboBoxに無い値をSelectedValueにしちゃう」みたいな不整合を防ぐのが狙い
            _blendShapeNameStore.Refresh(_settingModel);

            foreach (var item in FaceSwitchItems)
            {
                item.UnsubscribeLanguageSelector();
            }
            FaceSwitchItems.Clear();
            foreach(var item in _settingModel.Items)
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
            else if(e.Command == ReceiveMessageNames.ExTrackerCalibrateComplete)
            {
                //キャリブレーション結果を向こうから受け取る: この場合は、ただ覚えてるだけでよい
                _calibrateData = e.Args;
            }
            else if (e.Command == ReceiveMessageNames.ExTrackerSetPerfectSyncMissedClipNames)
            {
                MissingBlendShapeNames = e.Args;
            }
            else if (e.Command == ReceiveMessageNames.ExTrackerSetIFacialMocapTroubleMessage)
            {
                IFacialMocapTroubleMessage = e.Args;
            }
        }

        #region 基本メニュー部分

        private bool _enableExternalTracking = false;
        public bool EnableExternalTracking
        {
            get => _enableExternalTracking;
            set
            {
                if (SetValue(ref _enableExternalTracking, value))
                {
                    SendMessage(MessageFactory.Instance.ExTrackerEnable(EnableExternalTracking));
                    ShouldNotifyMissingBlendShapeClipNames =
                        EnableExternalTracking &&
                        !string.IsNullOrEmpty(MissingBlendShapeNames);
                }
            }
        }

        private bool _enableExternalTrackerLipSync = true;
        public bool EnableExternalTrackerLipSync
        {
            get => _enableExternalTrackerLipSync;
            set
            {
                if (SetValue(ref _enableExternalTrackerLipSync, value))
                {
                    SendMessage(MessageFactory.Instance.ExTrackerEnableLipSync(EnableExternalTrackerLipSync));
                }
            }
        }

        private bool _enableExternalTrackerPerfectSync = false;
        public bool EnableExternalTrackerPerfectSync
        {
            get => _enableExternalTrackerPerfectSync;
            set
            {
                if (SetValue(ref _enableExternalTrackerPerfectSync, value))
                {
                    SendMessage(MessageFactory.Instance.ExTrackerEnablePerfectSync(
                        EnableExternalTrackerPerfectSync
                        ));
                }
            }
        }

        private bool _useVRoidDefaultForPerfectSync = false;
        public bool UseVRoidDefaultForPerfectSync
        {
            get => _useVRoidDefaultForPerfectSync;
            set
            {
                if (SetValue(ref _useVRoidDefaultForPerfectSync, value))
                {
                    SendMessage(MessageFactory.Instance.ExTrackerUseVRoidDefaultForPerfectSync(
                        UseVRoidDefaultForPerfectSync
                        ));
                }
            }
        }

        private ActionCommand? _openPerfectSyncTipsUrlCommand;
        public ActionCommand OpenPerfectSyncTipsUrlCommand
            => _openPerfectSyncTipsUrlCommand ??= new ActionCommand(OpenPerfectSyncTipsUrl);

        private void OpenPerfectSyncTipsUrl()
        {
            string url =
                (LanguageSelector.Instance.LanguageName == "Japanese") ?
                "https://malaybaku.github.io/VMagicMirror/tips/perfect_sync" :
                "https://malaybaku.github.io/VMagicMirror/en/tips/perfect_sync";
            UrlNavigate.Open(url);
        }


        private bool _shouldNotifyMissingBlendShapeClipNames = false;
        [XmlIgnore]
        public bool ShouldNotifyMissingBlendShapeClipNames
        {
            get => _shouldNotifyMissingBlendShapeClipNames;
            set => SetValue(ref _shouldNotifyMissingBlendShapeClipNames, value);
        }

        private string _missingBlendShapeNames = "";
        [XmlIgnore]
        public string MissingBlendShapeNames
        {
            get => _missingBlendShapeNames;
            set
            {
                if (SetValue(ref _missingBlendShapeNames, value))
                {
                    ShouldNotifyMissingBlendShapeClipNames =
                        EnableExternalTracking &&
                        !string.IsNullOrEmpty(MissingBlendShapeNames);
                }
            }
        }

        private ActionCommand? _showMissingBlendShapeNotificationCommand = null;
        public ActionCommand ShowMissingBlendShapeNotificationCommand
            => _showMissingBlendShapeNotificationCommand ??= new ActionCommand(ShowMissingBlendShapeNotification);
        private void ShowMissingBlendShapeNotification()
        {
            var indication = MessageIndication.ExTrackerMissingBlendShapeNames(LanguageSelector.Instance.LanguageName);
            var lines = MissingBlendShapeNames.Split('\n').ToList();
            if (lines.Count > 8)
            {
                //未定義ブレンドシェイプがあまりに多いとき、後ろを"…"で切る
                lines = lines.Take(8).ToList();
                lines.Add("…");
            }
            MessageBoxWrapper.Instance.ShowAsync(
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

        private ActionCommand? _resetSettingsCommand = null;
        public ActionCommand ResetSettingsCommand
            => _resetSettingsCommand ??= new ActionCommand(
                () => SettingResetUtils.ResetSingleCategorySettingAsync(ResetToDefault)
                );

        #endregion

        #region アプリ別のやつ(※今んとこIPを一方的に表示するだけなのであんまり難しい事はないです)

        [XmlIgnore]
        public bool IsTrackSourceNone
        {
            get => _trackSourceType == TrackSourceNone;
            set
            {
                if (!IsTrackSourceNone && value)
                {
                    TrackSourceType = TrackSourceNone;
                    RaisePropertyChanged(nameof(IsTrackSourceIFacialMocap));
                    RaisePropertyChanged(nameof(IsTrackSourceNone));
                }
            }
        }

        [XmlIgnore]
        public bool IsTrackSourceIFacialMocap
        {
            get => _trackSourceType == TrackSourceIFacialMocap;
            set
            {
                if (!IsTrackSourceIFacialMocap && value)
                {
                    TrackSourceType = TrackSourceIFacialMocap;
                    RaisePropertyChanged(nameof(IsTrackSourceNone));
                    RaisePropertyChanged(nameof(IsTrackSourceIFacialMocap));
                }
            }
        }

        private int _trackSourceType = 0;
        /// <summary>
        /// NOTE: この値を保存するけど、UIから直接叩くわけではない。
        /// </summary>
        public int TrackSourceType
        {
            get => _trackSourceType;
            set
            {
                if (SetValue(ref _trackSourceType, value))
                {
                    SendMessage(MessageFactory.Instance.ExTrackerSetSource(TrackSourceType));
                    RaisePropertyChanged(nameof(IsTrackSourceNone));
                    RaisePropertyChanged(nameof(IsTrackSourceIFacialMocap));
                }
            }
        }

        private string _iFacialMocapTargetIpAddress = "";
        public string IFacialMocapTargetIpAddress
        {
            get => _iFacialMocapTargetIpAddress;
            set => SetValue(ref _iFacialMocapTargetIpAddress, value);
        }

        private ActionCommand? _refreshIFacialMocapTargetCommand = null;
        public ActionCommand RefreshIFacialMocapTargetCommand
            => _refreshIFacialMocapTargetCommand ??= new ActionCommand(RefreshIFacialMocapTarget);

        private void RefreshIFacialMocapTarget()
        {
            NetworkEnvironmentUtils.SendIFacialMocapDataReceiveRequest(IFacialMocapTargetIpAddress);
        }

        private string _selfIpAddress = "(unknown)";
        [XmlIgnore]
        public string SelfIpAddress
        {
            get => _selfIpAddress;
            set => SetValue(ref _selfIpAddress, value);
        }

        private ActionCommand? _openInstructionUrlCommand;
        public ActionCommand OpenInstructionUrlCommand
            => _openInstructionUrlCommand ??= new ActionCommand(OpenInstructionUrl);

        private void OpenInstructionUrl()
        {
            string url =
                (LanguageSelector.Instance.LanguageName == "Japanese") ?
                "https://malaybaku.github.io/VMagicMirror/docs/external_tracker" :
                "https://malaybaku.github.io/VMagicMirror/en/docs/external_tracker";
            UrlNavigate.Open(url);
        }

        private ActionCommand? _refreshReceiverSettingCommand;
        public ActionCommand RefreshReceiverSettingCommand
            => _refreshReceiverSettingCommand ??= new ActionCommand(RefreshReceiverSetting);
        private void RefreshReceiverSetting() 
            => SelfIpAddress = NetworkEnvironmentUtils.GetLocalIpAddressAsString();

        private string _calibrateData = "";
        public string CalibrateData
        {
            get => _calibrateData;
            set
            {
                if (SetValue(ref _calibrateData, value))
                {
                    SendMessage(MessageFactory.Instance.ExTrackerSetCalibrateData(CalibrateData));
                }
            }
        }

        #endregion

        #region 表情スイッチのやつ

        private string _serializedFaceSwitchSetting = "";
        //NOTE: setterはアプリ起動直後、およびそれ以降で表情スイッチ系の設定を変えたときに(UIではなくコードから)呼ばれます。
        public string SerializedFaceSwitchSetting
        {
            get => _serializedFaceSwitchSetting;
            set
            {
                if (SetValue(ref _serializedFaceSwitchSetting, value))
                {
                    SendMessage(MessageFactory.Instance.ExTrackerSetFaceSwitchSetting(SerializedFaceSwitchSetting));
                }
            }
        }

        /// <summary>
        /// 子要素になってる<see cref="ExternalTrackerFaceSwitchItemViewModel"/>から呼び出すことで、
        /// 現在の設定を保存した状態にします。
        /// </summary>
        public void SaveFaceSwitchSettingAsString() 
            => SerializedFaceSwitchSetting = _settingModel.ToJson();

        /// <summary> UIで個別設定として表示する、表情スイッチの要素です。 </summary>
        [XmlIgnore]
        public ObservableCollection<ExternalTrackerFaceSwitchItemViewModel> FaceSwitchItems { get; }
            = new ObservableCollection<ExternalTrackerFaceSwitchItemViewModel>();

        /// <summary>  </summary>
        [XmlIgnore]
        public ReadOnlyObservableCollection<string> BlendShapeNames => _blendShapeNameStore.BlendShapeNames;

        #endregion

        #region エラーまわり: iFMの設定が怪しそうなときのメッセージ + webカメラが止まる問題の対処

        private string _iFacialMocapTroubleMessage = "";
        [XmlIgnore]
        public string IFacialMocapTroubleMessage
        {
            get => _iFacialMocapTroubleMessage;
            set
            {
                if (SetValue(ref _iFacialMocapTroubleMessage, value))
                {
                    IFacialMocapHasTrouble = !string.IsNullOrEmpty(IFacialMocapTroubleMessage);
                }
            }
        }

        private bool _iFacialMocapHasTrouble = false;
        [XmlIgnore]
        public bool IFacialMocapHasTrouble
        {
            get => _iFacialMocapHasTrouble;
            set => SetValue(ref _iFacialMocapHasTrouble, value);
        }

        private ActionCommand? _openIFMTroubleShootCommand;
        private ActionCommand? OpenIFMTroubleShootCommand => _openIFMTroubleShootCommand ??= new ActionCommand(OpenIFMTroubleShoot);
        private void OpenIFMTroubleShoot()
        {
            var url = LanguageSelector.StringToLanguage(LanguageSelector.Instance.LanguageName) switch
            {
                Languages.Japanese => "https://malaybaku.github.io/VMagicMirror/docs/external_tracker_ifacialmocap#troubleshoot",
                _ => "https://malaybaku.github.io/VMagicMirror/en/docs/external_tracker_ifacialmocap#troubleshoot",
            };
            UrlNavigate.Open(url);
        }

        private ActionCommand? _endExTrackerIfNeededCommand;
        public ActionCommand EndExTrackerIfNeededCommand
            => _endExTrackerIfNeededCommand ??= new ActionCommand(EndExTrackerIfNeeded);
        private async void EndExTrackerIfNeeded()
        {
            var indication = MessageIndication.ExTrackerCheckTurnOff(LanguageSelector.Instance.LanguageName);
            bool result = await MessageBoxWrapper.Instance.ShowAsync(
                indication.Title,
                indication.Content,
                MessageBoxWrapper.MessageBoxStyle.OKCancel
                );

            if (result)
            {
                EnableExternalTracking = false;
            }
        }

        #endregion

        public override void ResetToDefault()
        {
            _settingModel = ExternalTrackerFaceSwitchSetting.LoadDefault();
            LoadFaceSwitchSettingFromString();
            SaveFaceSwitchSettingAsString();
            CalibrateData = "";
            TrackSourceType = TrackSourceNone;
            EnableExternalTracking = false;
            EnableExternalTrackerLipSync = true;
            UseVRoidDefaultForPerfectSync = false;
            EnableExternalTrackerPerfectSync = false;
        }
    }
}
