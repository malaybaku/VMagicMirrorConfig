using System;
using System.Threading.Tasks;

namespace Baku.VMagicMirrorConfig
{
    public enum UpdateDialogResult
    {
        GetLatestVersion,
        AskMeLater,
        SkipThisVersion,
    }

    /// <summary>
    /// アプリ起動時に適当に走らせるとアプリ更新チェックができるすごいやつだよ
    /// </summary>
    class UpdateChecker
    {
        private void OpenStandardDownloadUrl() => UrlNavigate.Open("https://baku-dreameater.booth.pm/items/1272298");
        private void OpenFullDownloadUrl() => UrlNavigate.Open("https://baku-dreameater.booth.pm/items/3064040");

        //アップデート表示は5日おきに行う
        private const double DialogAppearMinimumInterval = 5.0;

        /// <summary>
        /// アプリ起動時か、あるいはそれ以外で明示的に要求された場合に更新を確認します。
        /// </summary>
        /// <param name="startupCheck"></param>
        /// <returns></returns>
        public async Task RunAsync(bool startupCheck)
        {
            var model = new UpdateNotificationModel();
            var checkResult = await model.CheckUpdateAvailable();
            if (!checkResult.UpdateNeeded)
            {
                if (!startupCheck)
                {
                    var indication = MessageIndication.AlreadyLatestVersion();
                    _ = await MessageBoxWrapper.Instance.ShowAsync(
                        indication.Title,
                        string.Format(indication.Content, AppConsts.AppVersion.ToString()),
                        MessageBoxWrapper.MessageBoxStyle.OK
                        );
                }
                return;
            }

            var preference = UpdatePreferenceRepository.Load();
            var lastShownVersion = VmmAppVersion.TryParse(preference.LastShownVersion, out var version)
                ? version
                : VmmAppVersion.LoadInvalid();

            //コードの通りだが、ダイアログを出せるのは以下のケース。
            // - 前回表示したのより新しいバージョンである
            // - アプリ起動時のものではなく、明示的にチェック操作をしている
            // - 前回表示したのとバージョンで、かつユーザーが「そのバージョンはスキップする」を指定しておらず、かつ前回から十分時間があいている
            var shouldShowDialog =
                checkResult.Version.IsNewerThan(lastShownVersion) ||
                !startupCheck ||
                (!preference.SkipLastShownVersion &&
                    (DateTime.Now - preference.LastDialogShownTime).TotalDays > DialogAppearMinimumInterval);

            if (!shouldShowDialog)
            {
                return;
            }


            var vm = new UpdateNotificationViewModel(checkResult);
            var dialog = new UpdateNotificationWindow()
            {
                DataContext = vm,
            };
            dialog.ShowDialog();

            var openStorePage = false;
            var skipLastShownVersion = false;
            switch (vm.Result)
            {
                case UpdateDialogResult.GetLatestVersion:
                    openStorePage = true;
                    break;
                case UpdateDialogResult.AskMeLater:
                    //何もしない
                    break;
                case UpdateDialogResult.SkipThisVersion:
                    skipLastShownVersion = true;
                    break;
            }

            var newPreference = new UpdatePreference()
            {
                LastDialogShownTime = DateTime.Now,
                LastShownVersion = checkResult.Version.ToString(),
                SkipLastShownVersion = skipLastShownVersion,
            };
            UpdatePreferenceRepository.Save(newPreference);

            if (openStorePage)
            {
                OpenStorePage();
            }
        }

        private void OpenStorePage()
        {
            if (FeatureLocker.FeatureLocked)
            {
                OpenStandardDownloadUrl();
            }
            else
            {
                OpenFullDownloadUrl();
            }
        }
    }
}
