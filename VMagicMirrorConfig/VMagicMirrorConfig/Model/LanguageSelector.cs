using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;

namespace Baku.VMagicMirrorConfig
{
    using static LineParseUtils;

    class LanguageSelector
    {
        private static LanguageSelector _instance;
        public static LanguageSelector Instance
            => _instance ?? (_instance = new LanguageSelector());

        private static string _settingFilePath = null;
        private static string SettingFilePath
            => _settingFilePath ?? (_settingFilePath = GetSettingFilePath());

        private LanguageSelector()
        {
        }

        private UdpSender _sender = null;

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

        public void Initialize(UdpSender sender)
        {
            _sender = sender;

            if (TryLoadSetting())
            {
                //ファイルから読めたのでOK
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
            SaveSetting();

            Application.Current.Resources.MergedDictionaries[0] = new ResourceDictionary()
            {
                Source = new Uri(
                  $"/VMagicMirrorConfig;component/Resources/{languageName}.xaml",
                  UriKind.Relative
                  ),
            };
            _sender?.SendMessage(MessageFactory.Instance.Language(languageName));
        }

        private void SaveSetting()
        {
            File.WriteAllLines(
                SettingFilePath,
                new string[]
                {
                    $"{nameof(LanguageName)}:{LanguageName}",
                });
        }

        private bool TryLoadSetting()
        {
            if (!File.Exists(SettingFilePath))
            {
                return false;
            }

            bool couldRead = false;
            foreach (var line in File.ReadAllLines(SettingFilePath))
            {
                couldRead = couldRead || TryReadStringParam(line, nameof(LanguageName), v => LanguageName = v);
            }
            return couldRead;
        }

        private static string GetSettingFilePath()
            => Path.Combine(
                Path.GetDirectoryName(Assembly.GetEntryAssembly().Location),
                SpecialFileNames.Culture
                );
    }

    enum Languages
    {
        Japanese,
        English
    }
}
