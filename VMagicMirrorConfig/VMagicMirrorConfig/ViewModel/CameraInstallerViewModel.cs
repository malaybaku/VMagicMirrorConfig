using System;
using System.IO;
using System.Windows;

namespace Baku.VMagicMirrorConfig
{
    /// <summary>
    /// 仮想カメラのインストール操作に関するビューモデル。
    /// batファイルを叩く処理をさせようと思っていたが、そこはユーザーに「これやれ」と指示して終わりにします。
    /// </summary>
    public class CameraInstallerViewModel : ViewModelBase
    {
        private ActionCommand? _openBatFileDirCommand;
        public ActionCommand OpenBatFileDirCommand
            => _openBatFileDirCommand ??= new ActionCommand(OpenBatFileDir);
        private void OpenBatFileDir()
        {
            string batDir = Path.Combine(
                Path.GetDirectoryName(SpecialFilePath.UnityAppPath) ?? "",
                "CameraInstall"
                );
            try
            {
                if (Directory.Exists(batDir))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
                    {
                        FileName = batDir,
                        UseShellExecute = true
                    });
                }
                else
                {
                    MessageBox.Show(".bat file directory was not found", "Camera Install Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message + "\n" + ex.StackTrace, ex.GetType().Name, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private ActionCommand? _openVirtualCamTipsCommand;
        public ActionCommand OpenVirtualCamTipsCommand
            => _openVirtualCamTipsCommand ??= new ActionCommand(OpenVirtualCamTips);
        private void OpenVirtualCamTips()
        {
            var url = LanguageSelector.StringToLanguage(LanguageSelector.Instance.LanguageName) switch
            {
                Languages.Japanese => "https://malaybaku.github.io/VMagicMirror/tips/virtual_camera",
                _ => "https://malaybaku.github.io/VMagicMirror/en/tips/virtual_camera",
            };
            UrlNavigate.Open(url);
        }
    }
}
