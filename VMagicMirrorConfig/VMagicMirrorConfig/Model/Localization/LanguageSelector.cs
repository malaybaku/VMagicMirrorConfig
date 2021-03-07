using System;
using System.Globalization;
using System.Linq;
using System.Windows;

namespace Baku.VMagicMirrorConfig
{
    class LanguageSelector : NotifiableBase
    {
        //NOTE: 外部ファイルで他の名前も指定できることに注意
        public const string LangNameJapanese = "Japanese";
        public const string LangNameEnglish = "English";

        private static LanguageSelector? _instance;
        public static LanguageSelector Instance => _instance ??= new LanguageSelector();
        private LanguageSelector() { }

        private IMessageSender? _sender = null;
        private readonly LocalizationDictionaryLoader _dictionaryLoader = new LocalizationDictionaryLoader();

        /// <summary>
        /// <see cref="LanguageName"/>が変化すると発火します。
        /// </summary>
        /// <remarks>
        /// 切り替わったあとの言語名は現状では不要。LocalizedStringで文字列取得する分には関知不要なため。
        /// </remarks>
        public event Action? LanguageChanged;

        private string _languageName = nameof(LangNameJapanese);
        public string LanguageName
        {
            get => _languageName;
            set
            {
                if (_languageName != value && IsValidLanguageName(value))
                {
                    _languageName = value;
                    SetLanguage(LanguageName);
                    LanguageChanged?.Invoke();
                    //NOTE: Bindingしたい人向け
                    RaisePropertyChanged();
                }
            }
        }

        public string[] GetAdditionalSupportedLanguageNames() => _dictionaryLoader
            .GetLoadedDictionaries()
            .Keys
            //NOTE: 順番が不変じゃなくなってると嫌なので、名前順で不変にしておく
            .OrderBy(v => v)
            .ToArray();

        public void Initialize(IMessageSender sender)
        {
            _sender = sender;
            _dictionaryLoader.LoadFromDefaultLocation();
        }

        public void InitializePreferredLanguage(string preferredLanguageName)
        {
            if (preferredLanguageName == "Japanese" || 
                preferredLanguageName == "English")
            {
                LanguageName = preferredLanguageName;
            }
            else
            {
                //NOTE: 普通はここには来ない
                LanguageName =
                    (CultureInfo.CurrentCulture.Name == "ja-JP") ?
                    "Japanese" :
                    "English";
            }
        }

        private bool IsValidLanguageName(string languageName)
        {
            return
                languageName == LangNameJapanese ||
                languageName == LangNameEnglish ||
                _dictionaryLoader.GetLoadedDictionaries().ContainsKey(languageName);
        }

        private void SetLanguage(string languageName)
        {
            //NOTE: 日本語と英語についてはexe内部から読み込む。わざわざ外に配置して壊すのも嫌なので
            var dict =
                (languageName == LangNameJapanese || languageName == LangNameEnglish)
                ? new ResourceDictionary()
                {
                    Source = new Uri(
                  $"/VMagicMirrorConfig;component/Resources/{languageName}.xaml",
                  UriKind.Relative
                  ),
                }
                : _dictionaryLoader.GetLoadedDictionaries()[languageName];

            Application.Current.Resources.MergedDictionaries[0] = dict;
            _sender?.SendMessage(MessageFactory.Instance.Language(languageName));
        }
    }
}
