using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Baku.VMagicMirrorConfig
{
    /// <summary> ネットワーク環境について何か情報提供してくれるユーティリティ </summary>
    static class NetworkEnvironmentUtils
    {
        /// <summary>
        /// IPv4のローカルアドレスっぽいものを取得します。
        /// </summary>
        /// <returns></returns>
        public static string GetLocalIpAddressAsString()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            //空ではないIPv4アドレスをてきとーに拾うだけ。これで十分じゃない場合は…多分それは人類には難しいやつ…
            return host.AddressList
                .Where(ip => ip.AddressFamily == AddressFamily.InterNetwork)
                .Select(ip => ip.ToString())
                .Where(s => !string.IsNullOrEmpty(s))
                .FirstOrDefault() 
                ?? "(unknown)";
        }
    }
}
