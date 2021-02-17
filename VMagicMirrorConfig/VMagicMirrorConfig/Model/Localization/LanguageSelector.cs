using System;
using System.Globalization;
using System.Windows;

namespace Baku.VMagicMirrorConfig
{
    class LanguageSelector
    {
        private static LanguageSelector? _instance;
        public static LanguageSelector Instance => _instance ??= new LanguageSelector();
        private LanguageSelector() { }

        private IMessageSender? _sender = null;

        /// <summary>
        /// <see cref="LanguageName"/>が変化すると発火します。
        /// </summary>
        /// <remarks>
        /// 切り替わったあとの言語名は現状では不要。LocalizedStringで文字列取得する分には関知不要なため。
        /// </remarks>
        public event Action? LanguageChanged;

        private string _languageName = nameof(Languages.Japanese);
        public string LanguageName
        {
            get => _languageName;
            set
            {
                if (_languageName != value && 
                    (value == nameof(Languages.Japanese) || value == nameof(Languages.English))
                    )
                {
                    _languageName = value;
                    SetLanguage(LanguageName);
                    LanguageChanged?.Invoke();
                }
            }
        }

        public void Initialize(IMessageSender sender, string preferredLanguageName)
        {
            _sender = sender;

            if (preferredLanguageName == "Japanese" || 
                preferredLanguageName == "English")
            {
                LanguageName = preferredLanguageName;
            }
            else
            {
                LanguageName =
                    (CultureInfo.CurrentCulture.Name == "ja-JP") ?
                    "Japanese" :
                    "English";
            }
        }

        public static Languages StringToLanguage(string languageName) => languageName switch
        {
            "Japanese" => Languages.Japanese,
            _ => Languages.English,
        };

        private void SetLanguage(string languageName)
        {
            Application.Current.Resources.MergedDictionaries[0] = new ResourceDictionary()
            {
                Source = new Uri(
                  $"/VMagicMirrorConfig;component/Resources/{languageName}.xaml",
                  UriKind.Relative
                  ),
            };
            _sender?.SendMessage(MessageFactory.Instance.Language(languageName));
        }
    }

    enum Languages
    {
        Japanese,
        English
    }
}
