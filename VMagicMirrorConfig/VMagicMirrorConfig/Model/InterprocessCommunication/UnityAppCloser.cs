using System.Diagnostics;
using System.Linq;

namespace Baku.VMagicMirrorConfig
{
    internal static class UnityAppCloser
    {
        public static void Close()
        {
            Process.GetProcesses()
                .FirstOrDefault(p => p.ProcessName == "VMagicMirror")
                ?.CloseMainWindow();
        }
    }
}
