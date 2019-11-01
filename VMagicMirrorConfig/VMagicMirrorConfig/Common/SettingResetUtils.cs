using System;
using System.Windows;

namespace Baku.VMagicMirrorConfig
{
    /// <summary>
    /// 設定リセット処理の共通処理
    /// </summary>
    static class SettingResetUtils
    {
        /// <summary>
        /// 確認ダイアログを出したうえで、個別カテゴリの設定をリセットします。
        /// </summary>
        /// <param name="resetAction"></param>
        public static void ResetSingleCategorySetting(Action resetAction)
        {
            var indication = MessageIndication.ResetSingleCategoryConfirmation(
                LanguageSelector.Instance.LanguageName
                );

            MessageBoxResult res;
            if (SettingWindow.CurrentWindow != null)
            {
                res = MessageBox.Show(
                    SettingWindow.CurrentWindow,    
                    indication.Content,
                    indication.Title,
                    MessageBoxButton.OKCancel
                    );
            }
            else
            {
                res = MessageBox.Show(
                    indication.Content,
                    indication.Title,
                    MessageBoxButton.OKCancel
                    );
            }

            if (res == MessageBoxResult.OK)
            {
                resetAction();
            }
        }
    }
}
