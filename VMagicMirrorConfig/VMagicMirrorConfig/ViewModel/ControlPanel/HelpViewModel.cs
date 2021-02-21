namespace Baku.VMagicMirrorConfig
{
    /// <summary> ヘルプ用のリンク類を処理するビューモデル </summary>
    public class HelpViewModel : ViewModelBase
    {
        private ActionCommand? _openManualUrlCommand;
        public ActionCommand OpenManualUrlCommand
            => _openManualUrlCommand ??= new ActionCommand(OpenManualUrl);

        private ActionCommand? _openDownloadUrlCommand;
        public ActionCommand OpenDownloadUrlCommand
            => _openDownloadUrlCommand ??= new ActionCommand(OpenDownloadUrl);

        private ActionCommand? _showLicenseCommand;
        public ActionCommand ShowLicenseCommand
            => _showLicenseCommand ??= new ActionCommand(() => new LicenseWindow().ShowDialog());

        private void OpenManualUrl()
        {
            string url = LocalizedString.GetString("URL_help_top");
            UrlNavigate.Open(url);
        }

        private void OpenDownloadUrl() => UrlNavigate.Open("https://baku-dreameater.booth.pm/items/1272298");
    }
}
