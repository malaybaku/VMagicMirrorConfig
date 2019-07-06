using System;
using System.IO;
using System.Reflection;
using System.Diagnostics;

namespace Baku.VMagicMirrorConfig
{
    /// <summary>ポインターを表示したり隠したりするクラス。</summary>
    /// <remarks>クリックスルー処理の都合でポインターは別プロセスになることに注意！</remarks>
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
        }

        private Process[] GetActiveLargePointerProcesses()
            => Process.GetProcessesByName(LargePointerProcessName);

    }
}
