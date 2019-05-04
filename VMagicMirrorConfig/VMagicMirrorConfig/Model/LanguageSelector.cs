using System;
using System.Globalization;
using System.Windows;

namespace Baku.VMagicMirrorConfig
{
    class LanguageSelector
    {
        private static LanguageSelector _instance;
        public static LanguageSelector Instance
            => _instance ?? (_instance = new LanguageSelector());

        private LanguageSelector()
        {
        }

        private IMessageSender _sender = null;

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

        public static Languages StringToLanguage(string languageName)
        {
            switch (languageName)
            {
                case "Japanese":
                    return Languages.Japanese;
                case "English":
                default:
                    return Languages.English;
            }
        }

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
