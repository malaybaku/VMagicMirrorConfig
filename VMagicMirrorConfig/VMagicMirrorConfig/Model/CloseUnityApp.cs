using System.Diagnostics;
using System.Linq;

namespace Baku.VMagicMirrorConfig
{
    internal static class CloseUnityApp
    {
        public static void Close()
        {
            Process.GetProcesses()
                .FirstOrDefault(p => p.ProcessName == "VMagicMirror")
                ?.CloseMainWindow();
        }
    }
}
