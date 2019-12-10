using System;
using System.Diagnostics;

namespace Baku.VMagicMirrorConfig
{
    public static class UrlNavigate
    {
        public static void Open(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo()
                {
                    FileName = url,
                    UseShellExecute = true,
                });
            }
            catch(Exception ex)
            {
                LogOutput.Instance.Write(ex);
            }
        }
    }
}
