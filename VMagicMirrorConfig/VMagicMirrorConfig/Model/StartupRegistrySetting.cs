using System;
using System.IO;
using System.Reflection;
using Microsoft.Win32;

namespace Baku.VMagicMirrorConfig
{
    class StartupRegistrySetting
    {
        const string StartupRegistryKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
        const string ProductName = @"Baku.VMagicMirror";

        private static string GetUnityAppPath()
        {
            string dir = Path.GetDirectoryName(
                Path.GetDirectoryName(
                    Assembly.GetEntryAssembly().Location
                    )
                );
            return Path.Combine(dir, "VMagicMirror.exe");
        }

        public StartupRegistrySetting()
        {
            _unityAppPath = GetUnityAppPath();
        }

        private readonly string _unityAppPath;

        public bool CheckThisVersionRegistered()
        {
            try
            {
                using (var regKey = Registry.CurrentUser.OpenSubKey(StartupRegistryKey, false))
                {
                    return (
                        regKey.GetValue(ProductName, "") is string s &&
                        s == _unityAppPath
                        );
                }
            }
            catch(Exception)
            {
                return false;
            }
        }

        public bool CheckOtherVersionRegistered()
        {
            try
            {
                using (var regKey = Registry.CurrentUser.OpenSubKey(StartupRegistryKey, false))
                {
                    return (
                        regKey.GetValue(ProductName, _unityAppPath) is string s &&
                        s != _unityAppPath
                        );
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void SetThisVersionRegister(bool enable)
        {
            try
            {
                using (var regKey = Registry.CurrentUser.OpenSubKey(StartupRegistryKey, true))
                {
                    if (enable)
                    {
                        regKey.SetValue(ProductName, _unityAppPath);
                    }
                    else
                    {
                        regKey.DeleteValue(ProductName);
                    }
                }
            }
            catch (Exception)
            {
                //諦める
            }
        }
    }
}
