using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Baku.VMagicMirrorConfig
{
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
