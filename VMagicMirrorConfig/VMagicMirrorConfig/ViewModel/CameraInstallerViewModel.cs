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
        private ActionCommand OpenBatFileDirCommand
            => _openBatFileDirCommand ??= new ActionCommand(OpenBatFileDir);
        private void OpenBatFileDir()
        {
            string batDir = GetBatFileDir();
            if (Directory.Exists(batDir))
            {
                System.Diagnostics.Process.Start(batDir);
            }
            else
            {
                MessageBox.Show(".bat file directory was not found", "Camera Install Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private static string GetBatFileDir() => Path.Combine(
            Path.GetDirectoryName(SpecialFilePath.UnityAppPath) ?? "",
            "CameraInstall"
            );

    }
}
