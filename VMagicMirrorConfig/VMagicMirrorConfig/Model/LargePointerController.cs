using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace Baku.VMagicMirrorConfig
{
    /// <summary>ポインターを表示したり隠したりするクラス。</summary>
    /// <remarks>
    /// クリックスルー処理の都合でポインターは別プロセスになることに注意！
    /// </remarks>
    class LargePointerController
    {
        private const string LargePointerProcessName = "VMagicMirrorConfig.LargePointer";

        private static string GetLargePoiterExeFilePath()
            => Path.Combine(
                Path.GetDirectoryName(Path.GetDirectoryName(
                    Assembly.GetEntryAssembly().Location
                    )),
                "LargePointer",
                "VMagicMirrorConfig.LargePointer.exe"
                );

        //NOTE: シングルトンにしているのはポインター表示プロセスをインスタンス別に管理できるような実装になってないから。
        private LargePointerController() { }
        private static LargePointerController _instance = null;
        internal static LargePointerController Instance
            => _instance ?? (_instance = new LargePointerController());

        public bool IsVisible { get; private set; } = false;

        public void UpdateVisibility(bool visible)
        {
            if (visible == IsVisible)
            {
                return;
            }

            if (visible)
            {
                Show();
            }
            else
            {
                Close();
            }
        }

        public void Show()
        {
            if (GetActiveLargePointerProcesses().Length > 0)
            {
                return;
            }

            string filePath = GetLargePoiterExeFilePath();
            if (File.Exists(filePath))
            {
                Process.Start(GetLargePoiterExeFilePath());
            }
            IsVisible = true;
        }

        public void Close()
        {
            var largePointers = GetActiveLargePointerProcesses();
            for (int i = 0; i < largePointers.Length; i++)
            {
                try
                {
                    //ただのインジケータなので強引にプロセスキルしても大丈夫
                    largePointers[i].Kill();
                }
                catch(Exception ex)
                {
                    LogOutput.Instance.Write(ex);
                }
            }
            IsVisible = false;
        }

        private Process[] GetActiveLargePointerProcesses()
            => Process.GetProcessesByName(LargePointerProcessName);

    }
}
