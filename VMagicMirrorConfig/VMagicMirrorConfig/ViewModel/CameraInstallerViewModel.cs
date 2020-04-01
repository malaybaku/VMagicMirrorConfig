using System.IO;
using System.Windows;

namespace Baku.VMagicMirrorConfig
{
    /// <summary>
    /// 仮想カメラのインストール操作に関するビューモデル。
    /// batファイルを叩く処理まで代行する凄いやつです。
    /// </summary>
    public class CameraInstallerViewModel : ViewModelBase
    {
        private ActionCommand? _installCameraCommand;
        public ActionCommand InstallCameraCommand
            => _installCameraCommand ??= new ActionCommand(InstallCamera);
        private void InstallCamera()
        {
            string batPath = GetInstallBatFilePath();
            if (!File.Exists(batPath))
            {
                MessageBox.Show("`Install.bat` file was not fuond.", "Camera Install Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
            {
                FileName = batPath,
                UseShellExecute = true,
            });
        }

        private ActionCommand? _uninstallCameraCommand;
        public ActionCommand UninstallCameraCommand
            => _uninstallCameraCommand ??= new ActionCommand(UninstallCamera);
        private void UninstallCamera()
        {
            string batPath = GetUninstallBatFilePath();
            if (!File.Exists(batPath))
            {
                MessageBox.Show("`Uninstall.bat` file was not fuond.", "Camera Install Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
            {
                FileName = batPath,
                UseShellExecute = true,
            });
        }

        private static string GetInstallBatFilePath() => Path.Combine(
            Path.GetDirectoryName(SpecialFilePath.UnityAppPath) ?? "",
            "CameraInstall",
            "Install.bat"
            );

        private static string GetUninstallBatFilePath() => Path.Combine(
            Path.GetDirectoryName(SpecialFilePath.UnityAppPath) ?? "",
            "CameraInstall",
            "Install.bat"
            );
    }
}
