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
        private const int TrackSourceWaidayo = 2;

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
                }
            }
        }

        private bool _enableExternalTrackerLipSync = false;
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
                    _trackSourceType = TrackSourceNone;
                    RaisePropertyChanged(nameof(IsTrackSourceIFacialMocap));
                    RaisePropertyChanged(nameof(IsTrackSourceWaidayo));
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
                    _trackSourceType = TrackSourceIFacialMocap;
                    RaisePropertyChanged(nameof(IsTrackSourceNone));
                    RaisePropertyChanged(nameof(IsTrackSourceWaidayo));
                    RaisePropertyChanged(nameof(IsTrackSourceIFacialMocap));
                }
            }
        }

        [XmlIgnore]
        public bool IsTrackSourceWaidayo
        {
            get => _trackSourceType == TrackSourceWaidayo;
            set
            {
                if (!IsTrackSourceWaidayo && value)
                {
                    _trackSourceType = TrackSourceWaidayo;
                    RaisePropertyChanged(nameof(IsTrackSourceNone));
                    RaisePropertyChanged(nameof(IsTrackSourceIFacialMocap));
                    RaisePropertyChanged(nameof(IsTrackSourceWaidayo));
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
                    RaisePropertyChanged(nameof(IsTrackSourceWaidayo));
                }
            }
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
                "https://malaybaku.github.io/VMagicMirror/tips/external_tracker" :
                "https://malaybaku.github.io/VMagicMirror/en/tips/external_tracker";
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

        public override void ResetToDefault()
        {
            _settingModel = ExternalTrackerFaceSwitchSetting.LoadDefault();
            LoadFaceSwitchSettingFromString();
            SaveFaceSwitchSettingAsString();
            CalibrateData = "";
            TrackSourceType = TrackSourceNone;
            EnableExternalTrackerLipSync = false;
            EnableExternalTracking = false;
        }
    }

    /// <summary> 表情スイッチの個別アイテムのビューモデル </summary>
    public class ExternalTrackerFaceSwitchItemViewModel : ViewModelBase 
    {
        public ExternalTrackerFaceSwitchItemViewModel(ExternalTrackerViewModel parent, ExternalTrackerFaceSwitchItem model)
        {
            _parent = parent;
            _model = model;
            SetLanguage(LanguageSelector.Instance.LanguageName == "Japanese" ? Languages.Japanese : Languages.English);
        }

        private void SetLanguage(Languages lang) 
            => Instruction = ExTrackerFaceInfo.GetText(lang, _model.SourceName);

        internal void SubscribeLanguageSelector() 
            => LanguageSelector.Instance.LanguageChanged += SetLanguage;

        internal void UnsubscribeLanguageSelector() 
            => LanguageSelector.Instance.LanguageChanged -= SetLanguage;

        private readonly ExternalTrackerViewModel _parent;
        private readonly ExternalTrackerFaceSwitchItem _model;

        #region 保存しないでよい値

        /// <summary> 
        /// "この表情のパラメタがN%以上になったら"みたいなしきい値の取りうる値。
        /// 細かく設定できる意味がないので10%刻みです。
        /// </summary>
        public ThresholdItem[] AvailablePercentages { get; } = Enumerable
            .Range(1, 9)
            .Select(i => new ThresholdItem(i * 10, $"{i * 10}%"))
            .ToArray();

        public ReadOnlyObservableCollection<string> BlendShapeNames => _parent.BlendShapeNames;

        private string _instruction = "";
        public string Instruction
        {
            get => _instruction;
            set => SetValue(ref _instruction, value);
        }

        #endregion

        #region 保存すべき値

        public int Threshold
        {
            get => _model.ThresholdPercent;
            set
            {
                if (_model.ThresholdPercent != value)
                {
                    _model.ThresholdPercent = value;
                    RaisePropertyChanged();
                    _parent.SaveFaceSwitchSettingAsString();
                }
            }
        }

        public string ClipName
        {
            get => _model.ClipName;
            set
            {
                if (_model.ClipName != value)
                {
                    _model.ClipName = value;
                    RaisePropertyChanged();
                    _parent.SaveFaceSwitchSettingAsString();
                }
            }
        }

        public bool KeepLipSync
        {
            get => _model.KeepLipSync;
            set
            {
                if (_model.KeepLipSync != value)
                {
                    _model.KeepLipSync = value;
                    RaisePropertyChanged();
                    _parent.SaveFaceSwitchSettingAsString();
                }
            }
        }

        #endregion

        public class ThresholdItem
        {
            public ThresholdItem(int value, string text)
            {
                Value = value;
                Text = text;
            }

            public int Value { get; }
            public string Text { get; }
        }
    }

    static class ExTrackerFaceInfo
    {
        public static string GetText(Languages lang, string key)
        {
            var src = (lang == Languages.Japanese) ? _japanese : _english;
            //NOTE: keyはWPFコード内で決め打ちしたものしか来ないハズなので"-"にはならないはず(なったらコードのバグ)
            return src.ContainsKey(key) ? src[key] : "-";
        }

        private static Dictionary<string, string> _japanese = new Dictionary<string, string>()
        {
            //TODO: 目の開閉と眉の上下って筋肉的に連動しているので、片方だけ残すほうがよいかも。
            //TODO: 口をすぼめる動きを入れてもいい…かもしれないが、喋りと両立しなさそうなので乗り気ではない。
            [FaceSwitchKeys.MouthSmile] = "口を笑顔にすると",
            [FaceSwitchKeys.EyeSquint] = "目を細めると",
            [FaceSwitchKeys.EyeWide] = "目を大きく見開くと",
            [FaceSwitchKeys.BrowUp] = "眉を上げると",
            [FaceSwitchKeys.BrowDown] = "眉を下げると",
            [FaceSwitchKeys.CheekPuff] = "頬をふくらませると",
            [FaceSwitchKeys.TongueOut] = "舌を出すと",
        };

        private static Dictionary<string, string> _english = new Dictionary<string, string>()
        {
            [FaceSwitchKeys.MouthSmile] = "Mouth smile",
            [FaceSwitchKeys.EyeSquint] = "Eye squint",
            [FaceSwitchKeys.EyeWide] = "Eye wide",
            [FaceSwitchKeys.BrowUp] = "Brow up",
            [FaceSwitchKeys.BrowDown] = "Brow down",
            [FaceSwitchKeys.CheekPuff] = "Cheek puff",
            [FaceSwitchKeys.TongueOut] = "Tongue out",
        };

    }
}
